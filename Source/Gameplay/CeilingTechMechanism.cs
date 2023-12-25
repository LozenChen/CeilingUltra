using Celeste.Mod.CeilingUltra.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using CelesteInput = Celeste.Input;
using Celeste.Mod.CeilingUltra.Module;

namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class CeilingTechMechanism {

    public static bool CeilingUltraEnabled => LevelSettings.CeilingUltraEnabled;

    public static bool CeilingRefillStamina => LevelSettings.CeilingRefillStamina;


    public static bool WallRefillStamina => LevelSettings.WallRefillStamina;
    public static bool CeilingRefillDash => LevelSettings.CeilingRefillDash;

    public static bool WallRefillDash => LevelSettings.WallRefillDash;
    public static bool CeilingJumpEnabled => LevelSettings.CeilingJumpEnabled;

    public static bool CeilingHyperEnabled => LevelSettings.CeilingHyperEnabled;

    public static bool UpdiagDashDontLoseHorizontalSpeed => LevelSettings.UpdiagDashEndNoHorizontalSpeedLoss;

    public static bool UpdiagDashDontLoseVerticalSpeed => LevelSettings.UpdiagDashEndNoVerticalSpeedLoss;

    [Load]
    public static void Load() {
        On.Celeste.Level.LoadNewPlayer += OnLoadNewPlayer;
    }

    [Unload]
    public static void Unload() {
        On.Celeste.Level.LoadNewPlayer -= OnLoadNewPlayer;
    }

    [Initialize]
    public static void Initialize() {
        using (new DetourContext { Before = new List<string> { "*"}, ID = "Ceiling Tech Mechanism" }) {
            typeof(Player).GetMethodInfo("orig_Update").IlHook(HookPlayerUpdate);
            typeof(Player).GetMethodInfo("orig_Update").IlHook(UpdateMaxFallHook);
            typeof(Player).GetMethodInfo("OnCollideV").IlHook(CeilingUltraHookOnCollideV);
            typeof(Player).GetMethodInfo("DashCoroutine").GetStateMachineTarget().IlHook(CeilingVerticalUltraHookDashCoroutine);
            typeof(Player).GetMethodInfo("NormalUpdate").IlHook(CeilingJumpHookNormalUpdate);
            typeof(Player).GetMethodInfo("SwimUpdate").IlHook(CeilingJumpHookSwimUpdate);
            typeof(Player).GetMethodInfo("StarFlyUpdate").IlHook(CeilingJumpHookFeatherUpdate);
            typeof(Player).GetMethodInfo("HitSquashUpdate").IlHook(CeilingJumpHookHitSquashUpdate);
            typeof(Player).GetMethodInfo("DashUpdate").IlHook(CeilingHyperHookDashUpdate);
            typeof(Player).GetMethodInfo("RedDashUpdate").IlHook(CeilingHyperHookRedDashUpdate);

            new List<string> { "OnTransition", "Jump", "SuperJump", "SuperWallJump", "Bounce", "SuperBounce", "StarFlyBegin", "orig_WallJump", "SideBounce", "DreamDashEnd" }.Select(str => typeof(Player).GetMethodInfo(str)).ToList().ForEach(x => x.IlHook(SetExtendedJumpGraceTimerIL));
        }
    }

    private static void SetExtendedJumpGraceTimerIL(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = false;
        while (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.MatchLdcR4(0f), ins => ins.MatchStfld<Player>(nameof(Player.jumpGraceTimer)))) {
            cursor.Index += 3;
            cursor.EmitDelegate(SetExtendedJumpGraceTimer);
            success = true;
        }
        "SomeSetJumpGraceTimerMethod".LogHookData("Set Jump Grace Timer", success);
        
    }
    public static void SetExtendedJumpGraceTimer() {
        CeilingJumpGraceTimer = 0f;
        LeftWallGraceTimer = 0f;
        RightWallGraceTimer = 0f;
        ProtectJumpGraceTimer = 0f;
    }

    private static Player OnLoadNewPlayer(On.Celeste.Level.orig_LoadNewPlayer orig, Vector2 Position, PlayerSpriteMode spriteMode) {
        Player player = orig(Position, spriteMode);
        PlayerOnCeiling = false;
        SetExtendedJumpGraceTimer();
        LastGroundJumpGraceTimer = 1f;
        NextMaxFall = 0f;
        return player;
    }

    public static void CeilingDuck(this Player player) {
        float origTop = player.Collider.Top;
        player.Ducking = true;
        float offset = origTop - player.Collider.Top;
        player.Y += offset;
    }

    public static bool TryCeilingDuck(this Player player, int priorDirection = 1) {
        if (player.IsSqueezed()) {
            Collider collider = player.Collider;
            Vector2 position = player.Position;
            bool result = false;
            float origTop = player.Collider.Top;
            player.Collider = player.duckHitbox;
            float offset = origTop - player.Collider.Top;
            player.Y += offset;
            if (!player.CollideCheck<Solid>()) {
                result = true;
            }
            else {
                int direction = priorDirection >= 0 ? 1 : -1;
                player.X += direction;
                if (!player.CollideCheck<Solid>()) {
                    result = true;
                }
                else {
                    player.X -= 2 * direction;
                    if (!player.CollideCheck<Solid>()) {
                        result = true;
                    }
                }
            }
            if (result) {
                player.hurtbox = player.duckHurtbox;
            }
            else {
                player.Collider = collider;
                player.Position = position;
            }
            return result;
        }
        else {
            CeilingDuck(player);
            return true;
        }
    }

    public static void CeilingUnduck_Normal(this Player player) {
        float origTop = player.Collider.Top;
        player.Ducking = false;
        float offset = origTop - player.Collider.Top;
        player.Y += offset;
    }

    public static bool TryCeilingUnduck(this Player player, out bool wasDuck, int priorDirection = 1) {
        wasDuck = player.Ducking;
        if (player.IsSqueezed()) {
            Collider collider2 = player.Collider;
            Vector2 position2 = player.Position;
            bool result2 = false;
            float origTop = player.Collider.Top;
            player.Collider = player.normalHitbox;
            float offset = origTop - player.Collider.Top;
            player.Y += offset;
            if (!player.CollideCheck<Solid>()) {
                result2 = true;
            }
            else {
                player.X += priorDirection;
                if (!player.CollideCheck<Solid>()) {
                    result2 = true;
                }
                else {
                    player.X -= 2 * priorDirection;
                    if (!player.CollideCheck<Solid>()) {
                        result2 = true;
                    }
                }
            }
            if (result2) {
                player.hurtbox = player.normalHurtbox;
            }
            else {
                player.Collider = collider2;
                player.Position = position2;
            }
            return result2;
        }
        if (!player.Ducking) {
            return true;
        }
        Collider collider = player.Collider;
        Hitbox hurtbox = player.hurtbox;
        Vector2 position = player.Position;
        CeilingUnduck_Normal(player);
        bool result = !player.CollideCheck<Solid>();
        if (!result) {
            player.Position = position;
            player.Collider = collider;
            player.hurtbox = hurtbox;
        }
        return result;
    }

    public static bool TryCeilingUltra(this Player player) {
        if (player.DashDir.X != 0f && player.DashDir.Y < 0f && player.Speed.Y < 0f && player.TryCeilingDuck(Math.Sign(player.Speed.X))) {
            player.DashDir.X = Math.Sign(player.DashDir.X);
            player.DashDir.Y = 0f;
            player.Speed.Y = 0f;
            player.Speed.X *= 1.2f;
            return true;
        }
        return false;
    }

    private static void CeilingUltraHookOnCollideV(ILContext iLContext) {
        ILCursor cursor = new ILCursor(iLContext);
        bool success = false;
        if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.DreamDashCheck)))) {
            cursor.Index++;
            if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.DreamDashCheck)), ins => ins.OpCode == OpCodes.Brfalse_S)) {
                success = true;
                ILLabel label = (ILLabel)cursor.Next.Next.Operand;
                cursor.GotoLabel(label);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(CheckAndApplyCeilingUltra);
            }
        }
        "OnCollideV".LogHookData("Ceiling Ultra", success);
    }

    private static void CheckAndApplyCeilingUltra(Player player) {
        if (CeilingUltraEnabled) {
            player.TryCeilingUltra();
        }
    }

    private static void CeilingVerticalUltraHookDashCoroutine(ILContext iLContext) {
        ILCursor cursor = new ILCursor(iLContext);
        bool success = true;
        if (cursor.TryGotoNext(
            ins => ins.OpCode == OpCodes.Ldloc_1,
            ins => ins.OpCode == OpCodes.Ldfld,
            ins => ins.OpCode == OpCodes.Brfalse
            )) {
            cursor.GotoLabel((ILLabel)cursor.Next.Next.Next.Operand);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(CheckCeilingVerticalUltraInDashCoroutine);
        }
        else {
            success = false;
        }
        "Player.DashCoroutine".LogHookData("Ceiling/Vertical Ultra", success);

        success = true;
        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(160f))) {
            Instruction target = cursor.Next.Next.Next.Next; // goes to Speed.X *= swapCancel.X;
            cursor.Index -= 3;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(SkipDashEndLoseSpeed);
            cursor.Emit(OpCodes.Brtrue, target);
        }
        else {
            success = false;
        }
        "Player.DashCoroutine".LogHookData("Updiag Dash End No Horizontal Speed Loss", success);
        "Player.DashCoroutine".LogHookData("Updiag Dash End No Vertical Speed Loss", success);
    }

    private static void CheckCeilingVerticalUltraInDashCoroutine(Player player) {
        if (VerticalTechMechanism.VerticalUltraEnabled && player.Speed.X != 0f && player.CollideCheck<Solid>(player.Position + Vector2.UnitX * Math.Sign(player.Speed.X)) && (!player.Inventory.DreamDash || !player.CollideCheck<DreamBlock>(player.Position + Vector2.UnitX * Math.Sign(player.Speed.X))) && player.TryVerticalUltra()) {
            // already applied as a side effect of TryVerticalUltra
            // we put TryVerticalUltra inside conditions coz if all other conditions are satisfied but can't vertical ultra (e.g. Can't Squeeze Hitbox), then still need to try Ceiling Ultra
        }
        else if (CeilingUltraEnabled && PlayerOnCeiling && (!player.Inventory.DreamDash || !player.CollideCheck<DreamBlock>(player.Position - Vector2.UnitY)) && player.TryCeilingUltra()) {
            // already applied
        }
        // try vertical ultra first, so it matchs the intuition that, first horizontal movement, then vertical
        // although that actually ground ultra > vertical ultra > ceiling ultra
        // ground ultra must be first so a grounded reverse hyper can be performed normally at a corner
    }

    public static bool OnCeiling(this Player player, int upCheck = 1) {
        return player.CollideCheck<Solid>(player.Position - upCheck * Vector2.UnitY);
    }

    public static bool PlayerOnCeiling = false;

    public static float CeilingJumpGraceTimer = 0f;

    public static float LeftWallGraceTimer = 0f;

    public static float RightWallGraceTimer = 0f;

    public static float ProtectJumpGraceTimer = 0f;

    public static float LastGroundJumpGraceTimer = 1f;

    public static float NextMaxFall = 0f;

    private static void UpdateMaxFallHook(ILContext il) {
        ILCursor cursor = new (il);
        bool success = false;
        while (cursor.TryGotoNext(MoveType.AfterLabel, i => i.OpCode == OpCodes.Ret)) {
            success = true;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(UpdateMaxFall);
            cursor.Index++;
        }
        "Player.orig_Update".LogHookData("Set MaxFall Helper", success);
    }

    private static void UpdateMaxFall(Player player) {
        if (NextMaxFall > 160f && player.StateMachine.State == 0) { // NormalBegin resets maxFall to be 160f, so we need this for vertical hyper
            player.maxFall = NextMaxFall;
        }
        NextMaxFall = 0f;
    }

    public static void UpdateOnCeilingAndWall(Player player) {
        if (LastGroundJumpGraceTimer > 0f && player.jumpGraceTimer <= 0f) { // so it's killed by something that maybe we have not hooked
            SetExtendedJumpGraceTimer();
        }
        if (player.StateMachine.State == 9) {
            PlayerOnCeiling = false;
        }
        else if (player.Speed.Y <= 0f) {
            PlayerOnCeiling = OnCeiling(player);
        }
        else {
            PlayerOnCeiling = false;
        }

        if (PlayerOnCeiling) {
            if (CeilingRefillStamina) {
                player.Stamina = 110f;
                player.wallSlideTimer = 1.2f;
            }
            CeilingJumpGraceTimer = 0.1f;
        } else if (CeilingJumpGraceTimer > 0f) {
            CeilingJumpGraceTimer -= Engine.DeltaTime;
        }

        if (WallRefillStamina && player.PlayerOnWall()) {
            player.Stamina = 110f;
            player.wallSlideTimer = 1.2f;
        }
        if (player.CollideCheck<Solid>(player.Position - Vector2.UnitX)) {
            LeftWallGraceTimer = 0.1f;
        }
        else if (LeftWallGraceTimer > 0f) {
            LeftWallGraceTimer -= Engine.DeltaTime;
        }
        if (player.CollideCheck<Solid>(player.Position + Vector2.UnitX)) {
            RightWallGraceTimer = 0.1f;
        }
        else if (RightWallGraceTimer > 0f) {
            RightWallGraceTimer -= Engine.DeltaTime;
        }
        if (ProtectJumpGraceTimer > 0f) {
            ProtectJumpGraceTimer -= Engine.DeltaTime;
        }
    }

    private static void RecordGroundJumpGraceTimer(Player player) {
        LastGroundJumpGraceTimer = player.jumpGraceTimer;
    }

    public static bool PlayerOnWall(this Player player) {
        return player.CollideCheck<Solid>(player.Position + Vector2.UnitX) || player.CollideCheck<Solid>(player.Position - Vector2.UnitX);
    }

    private static void HookPlayerUpdate(ILContext il) {
        ILCursor cursor = new (il);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(UpdateOnCeilingAndWall); // only hiccup jump will affect this, so i dont insert this after onground evaluation
        "Player.orig_Update".LogHookData("Ceiling Jump", true);
        "Player.orig_Update".LogHookData("Ceiling/Wall RefillStamina", true);

        bool success = true;
        if (cursor.TryGotoNext(MoveType.AfterLabel, ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.MatchLdfld<Player>(nameof(Player.dashRefillCooldownTimer)), ins => ins.MatchLdcR4(0f), ins => ins.OpCode == OpCodes.Ble_Un_S)) {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(RecordGroundJumpGraceTimer);
            ILLabel target = (ILLabel)cursor.Next.Next.Next.Next.Operand;
            cursor.GotoLabel(target);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(ExtendedRefillDash);
        }
        else {
            success = false;
        }
        "Player.orig_Update".LogHookData("Ceiling/Wall RefillDash", success);
    }

    public static void ExtendedRefillDash(Player player) {
        if (!player.Inventory.NoRefills 
            && (
                (CeilingRefillDash && PlayerOnCeiling) || 
                (WallRefillDash && player.PlayerOnWall())
            ) 
            && (!player.CollideCheck<Spikes>() || SaveData.Instance.Assists.Invincible)) {
            player.RefillDash();
        }
    }

    public static void CeilingJump(this Player player, bool particles = true, bool playSfx = true) {
        int priorDirection = (int)player.moveX;
        if (priorDirection == 0) {
            priorDirection = (int)player.Facing;
        }
        player.TryCeilingUnduck(out _, priorDirection); // it's ok if you can't unduck
        NextMaxFall = 240f;
        Input.Jump.ConsumeBuffer();
        player.jumpGraceTimer = 0f;
        SetExtendedJumpGraceTimer();
        player.varJumpTimer = 0f; // does not produce varJumpTimer
        player.AutoJump = false;
        player.dashAttackTimer = 0f;
        player.gliderBoostTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;
        player.Speed.X += 40f * (float)player.moveX + player.LiftBoost.X;
        player.Speed.Y = +105f; // no liftboost
        player.varJumpSpeed = +105f;
        player.LaunchedBoostCheck();
        if (playSfx) {
            if (player.launched) {
                player.Play("event:/char/madeline/jump_assisted");
            }
            if (player.dreamJump) {
                player.Play("event:/char/madeline/jump_dreamblock");
            }
            else {
                player.Play("event:/char/madeline/jump");
            }
        }
        player.Sprite.Scale = new Vector2(0.6f, 1.4f);
        if (particles) {
            int index = -1;
            Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(player.CollideAll<Platform>(player.Position - Vector2.UnitY, player.temp));
            if (platformByPriority != null) {
                index = platformByPriority.GetLandSoundIndex(player);
            }
            Dust.Burst(player.TopCenter, (float)Math.PI / 2f, 4, player.DustParticleFromSurfaceIndex(index));
        }
        SaveData.Instance.TotalJumps++;
    }

    private static void CeilingJumpHookNormalUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.Jump)), ins => ins.OpCode == OpCodes.Br)) {
            cursor.Index += 2;
            ILLabel endTarget = (ILLabel) cursor.Prev.Operand;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CheckAndApplyCeilingJump);
            cursor.Emit(OpCodes.Brtrue, endTarget);
        }
        else {
            success = false;
        }
        "Player.NormalUpdate".LogHookData("Ceiling Jump", success);
    }

    private static bool CheckAndApplyCeilingJump(Player player) {
        if (CeilingJumpEnabled && CeilingJumpGraceTimer > 0f) {
            player.CeilingJump();
            return true;
        }
        return false;
    }

    private static void CeilingJumpHookSwimUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.MatchLdsfld(typeof(CelesteInput).FullName, nameof(CelesteInput.Jump)), ins => true, ins => ins.OpCode == OpCodes.Brfalse_S)) {
            cursor.Next.Next.Next.MatchBrfalse(out ILLabel label);
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CheckAndApplyCeilingJumpInWater);
            cursor.Emit(OpCodes.Brtrue, label);
        }
        else {
            success = false;
        }
        "Player.SwimUpdate".LogHookData("Ceiling Jump", success);
    }

    private static bool CheckAndApplyCeilingJumpInWater(Player player) {
        if (CeilingJumpEnabled && PlayerOnCeiling && Input.Jump.Pressed) {
            player.CeilingJump();
            return true;
        }
        return false;
    }

    private static void CeilingJumpHookFeatherUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.MatchLdsfld(typeof(CelesteInput).FullName, nameof(CelesteInput.Jump)), ins => true, ins => ins.OpCode == OpCodes.Brfalse_S)) {
            cursor.Index += 3;
            Instruction nextIns = cursor.Next;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CheckAndApplyCeilingJumpInFeather);
            cursor.Emit(OpCodes.Brfalse, nextIns);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Ret);
        }
        else {
            success = false;
        }
        "Player.StarFlyUpdate".LogHookData("Ceiling Jump", success);
    }

    private static bool CheckAndApplyCeilingJumpInFeather(Player player) {
        if (CeilingJumpEnabled && player.OnCeiling(3)) {
            player.CeilingJump();
            return true;
        }
        return false;
    }

    private static void CeilingJumpHookHitSquashUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.MatchLdsfld(typeof(CelesteInput).FullName, nameof(CelesteInput.Jump)), ins => true, ins => ins.OpCode == OpCodes.Brfalse)) {
            cursor.Index += 3;
            Instruction nextIns = cursor.Next;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CheckAndApplyCeilingJumpInHitSquash);
            cursor.Emit(OpCodes.Brfalse, nextIns);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Ret);
        }
        else {
            success = false;
        }
        "Player.HitSquashUpdate".LogHookData("Ceiling Jump", success);
    }
    private static bool CheckAndApplyCeilingJumpInHitSquash(Player player) {
        if (CeilingJumpEnabled && PlayerOnCeiling) {
            player.CeilingJump();
            return true;
        }
        return false;
    }

    private static void CeilingHyper(this Player player, bool wasDuck) {
        // this actually contains ceiling super
        float xDirection = Math.Sign(CelesteInput.MoveX);
        if (xDirection == 0) {
            xDirection = (float)player.Facing;
        }
        Input.Jump.ConsumeBuffer();
        player.jumpGraceTimer = 0f;
        SetExtendedJumpGraceTimer();
        player.varJumpTimer = 0f; // as what we do in ceiling jump
        player.AutoJump = false;
        player.dashAttackTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;
        player.Speed.X = 260f * xDirection + player.LiftBoost.X;
        player.Speed.Y = +105f;
        player.gliderBoostTimer = 0.55f; // would be cursed i guess
        player.Play("event:/char/madeline/jump");
        if (wasDuck) {
            player.Speed.X *= 1.25f;
            player.Speed.Y *= 0.5f;
            player.Play("event:/char/madeline/jump_superslide");
            player.gliderBoostDir = Monocle.Calc.AngleToVector((float)Math.PI * (1f / 2f - xDirection * 5f / 16f), 1f);
        }
        else {
            player.gliderBoostDir = Monocle.Calc.AngleToVector((float)Math.PI * (1f / 2f - xDirection / 4f), 1f);
            player.Play("event:/char/madeline/jump_super");
        }
        player.varJumpSpeed = player.Speed.Y;
        player.launched = true;
        player.Sprite.Scale = new Vector2(0.6f, 1.4f);
        int index = -1;
        Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(player.CollideAll<Platform>(player.Position - Vector2.UnitY, player.temp));
        if (platformByPriority != null) {
            index = platformByPriority.GetLandSoundIndex(player);
        }
        Dust.Burst(player.TopCenter, (float)Math.PI / 2f, 4, player.DustParticleFromSurfaceIndex(index));
        SaveData.Instance.TotalJumps++;
    }

    private static void CeilingHyperHookDashUpdate(ILContext il) {
        ILCursor cursor = new (il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.SuperJump)))) {
            cursor.Index += 3;
            Instruction next = cursor.Next;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CheckAndApplyCeilingHyper);
            cursor.Emit(OpCodes.Brfalse, next);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Ret);
        }
        else {
            success = false;
        }
        "Player.DashUpdate".LogHookData("Ceiling Hyper", success);
    }
    private const float startRemainFullVerticalSpeed = -325f;
    private static bool SkipDashEndLoseSpeed(Player player) {
        bool result = UpdiagDashDontLoseHorizontalSpeed && player.DashDir.Y < 0f;
        if (result) {
            if (UpdiagDashDontLoseVerticalSpeed && player.DashDir.X != 0f) {
                // speed < 169.7f: half
                // speed > 325f : remain
                // between: linear interpolate

                float startLerpVerticalSpeed = 240f * player.DashDir.Y;
                if (player.Speed.Y >= startLerpVerticalSpeed) {
                    player.Speed.Y = startLerpVerticalSpeed / 2f;
                }
                else if (player.Speed.Y <= startRemainFullVerticalSpeed) {
                    // remain invariant
                }
                else {
                    player.Speed.Y = MathHelper.Lerp(startLerpVerticalSpeed / 2f, startRemainFullVerticalSpeed, (player.Speed.Y - startLerpVerticalSpeed) / (startRemainFullVerticalSpeed - startLerpVerticalSpeed));
                }
            }
            else {
                player.Speed.Y = player.DashDir.Y * 120f;
            }

            player.Speed.Y *= 4f / 3f; // as it will encounter a 0.75f factor later
        }
        return result;
    }

    private static void CeilingHyperHookRedDashUpdate(ILContext il) {
        ILCursor cursor = new(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.SuperJump)))) {
            cursor.Index += 3;
            Instruction next = cursor.Next;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CheckAndApplyCeilingHyperForRedDash);
            cursor.Emit(OpCodes.Brfalse, next);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Ret);
        }
        else {
            success = false;
        }
        "Player.RedDashUpdate".LogHookData("Ceiling Hyper", success);
    }

    private static bool CheckAndApplyCeilingHyper(Player player) {
        if (CeilingHyperEnabled && Math.Abs(player.DashDir.Y) < 0.1f && Input.Jump.Pressed && CeilingJumpGraceTimer > 0f && player.TryCeilingUnduck(out bool wasDuck, (int)player.Facing)) { // we take the prior direction to be the direction of hyper
            player.CeilingHyper(wasDuck);
            return true;
        }
        return false;
    }

    private static bool CheckAndApplyCeilingHyperForRedDash(Player player) {
        bool flag = player.LastBooster != null && player.LastBooster.Ch9HubTransition;
        if (CeilingHyperEnabled && !flag && Math.Abs(player.DashDir.Y) < 0.1f && Input.Jump.Pressed && CeilingJumpGraceTimer > 0f && player.TryCeilingUnduck(out bool wasDuck, (int)player.Facing)) {
            player.CeilingHyper(wasDuck);
            return true;
        }
        return false;
    }
}