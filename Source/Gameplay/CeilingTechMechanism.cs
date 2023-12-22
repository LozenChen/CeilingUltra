using Celeste.Mod.CeilingUltra.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using CelesteInput = Celeste.Input;

namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class CeilingTechMechanism {

    public static bool CeilingUltraEnabled => ceilingUltraSetting.Enabled && ceilingUltraSetting.CeilingUltraEnabled;

    public static bool CeilingRefillStamina => ceilingUltraSetting.Enabled && ceilingUltraSetting.CeilingRefillStamina;

    public static bool CeilingRefillDash => ceilingUltraSetting.Enabled && ceilingUltraSetting.CeilingRefillDash;

    public static bool CeilingJumpEnabled => ceilingUltraSetting.Enabled && ceilingUltraSetting.CeilingJumpEnabled;

    public static bool CeilingHyperEnabled => ceilingUltraSetting.Enabled && ceilingUltraSetting.CeilingHyperEnabled;

    public static bool UpdiagDashDontLoseHorizontalSpeed => ceilingUltraSetting.Enabled && ceilingUltraSetting.UpdiagDashDontLoseHorizontalSpeed;

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
        typeof(Player).GetMethodInfo("OnCollideV").IlHook(CeilingUltraHookOnCollideV);
        typeof(Player).GetMethodInfo("DashCoroutine").GetStateMachineTarget().IlHook(CeilingUltraHookDashCoroutine);
        typeof(Player).GetMethodInfo("orig_Update").IlHook(OnCeilingHookPlayerUpdate);
        typeof(Player).GetMethodInfo("NormalUpdate").IlHook(CeilingJumpHookNormalUpdate);
        typeof(Player).GetMethodInfo("SwimUpdate").IlHook(CeilingJumpHookSwimUpdate);
        typeof(Player).GetMethodInfo("StarFlyUpdate").IlHook(CeilingJumpHookFeatherUpdate);
        typeof(Player).GetMethodInfo("HitSquashUpdate").IlHook(CeilingJumpHookHitSquashUpdate);
        typeof(Player).GetMethodInfo("DashUpdate").IlHook(CeilingHyperHookDashUpdate);
        typeof(Player).GetMethodInfo("RedDashUpdate").IlHook(CeilingHyperHookRedDashUpdate);
    }

    private static Player OnLoadNewPlayer(On.Celeste.Level.orig_LoadNewPlayer orig, Vector2 Position, PlayerSpriteMode spriteMode) {
        Player player = orig(Position, spriteMode);
        PlayerOnCeiling = false;
        CeilingJumpGraceTimer = 0f;
        LastGroundJumpGraceTimer = 1f;
        return player;
    }

    public static void CeilingDuck(this Player player) {
        float origTop = player.Collider.Top;
        player.Ducking = true;
        float offset = origTop - player.Collider.Top;
        player.Y += offset;
    }

    public static void CeilingUnduck(this Player player) {
        float origTop = player.Collider.Top;
        player.Ducking = false;
        float offset = origTop - player.Collider.Top;
        player.Y += offset;
    }

    public static bool CanCeilingUnduck(this Player player) {
        if (!player.Ducking) {
            return true;
        }
        Collider collider = player.Collider;
        float Y = player.Y;
        CeilingUnduck(player);
        bool result = !player.CollideCheck<Solid>();
        player.Y = Y;
        player.Collider = collider;
        return result;
    }

    public static void CeilingUltra(this Player player) {
        if (player.DashDir.X != 0f && player.DashDir.Y < 0f && player.Speed.Y < 0f) {
            player.DashDir.X = Math.Sign(player.DashDir.X);
            player.DashDir.Y = 0f;
            player.Speed.Y = 0f;
            player.Speed.X *= 1.2f;
            player.CeilingDuck();
        }
    }

    private static void CeilingUltraHookOnCollideV(ILContext iLContext) {
        ILCursor cursor = new ILCursor(iLContext);
        bool success = false;
        if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.DreamDashCheck)))) {
            cursor.Index++;
            if (cursor.TryGotoNext(MoveType.After, ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.DreamDashCheck)))) {
                success = true;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(TryApplyCeilingUltraAndPassBoolean);
            }
        }
        LogHookData("Ceiling Ultra", "OnCollideV", success);
    }

    private static bool TryApplyCeilingUltraAndPassBoolean(bool b, Player player) {
        if (CeilingUltraEnabled) {
            player.CeilingUltra();
        }
        return b;
    }

    private static void CeilingUltraHookDashCoroutine(ILContext iLContext) {
        ILCursor cursor = new ILCursor(iLContext);
        bool success = true;
        if (cursor.TryGotoNext(
            ins => ins.OpCode == OpCodes.Ldloc_1,
            ins => ins.OpCode == OpCodes.Ldfld,
            ins => ins.OpCode == OpCodes.Brfalse
            )) {
            cursor.GotoLabel((ILLabel)cursor.Next.Next.Next.Operand);
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(CheckCeilingUltraInDashCoroutine);
        }
        else {
            success = false;
        }
        LogHookData("Ceiling Ultra", "Player.DashCoroutine", success);

        success = true;
        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(160f))) {
            Instruction target = cursor.Next.Next.Next.Next;
            cursor.Index -= 3;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(SkipDashEndLoseSpeed);
            cursor.Emit(OpCodes.Brtrue, target);
        }
        else {
            success = false;
        }
        LogHookData("Updiag Dash Don't Lose Horizontal Speed On End", "Player.DashCoroutine", success);
    }

    private static void CheckCeilingUltraInDashCoroutine(Player player) {
        if (CeilingUltraEnabled && PlayerOnCeiling && (!player.Inventory.DreamDash || !player.CollideCheck<DreamBlock>(player.Position - Vector2.UnitY))) {
            player.CeilingUltra();
        }
    }

    public static bool OnCeiling(this Player player, int upCheck = 1) {
        return player.CollideCheck<Solid>(player.Position - upCheck * Vector2.UnitY);
    }

    public static bool PlayerOnCeiling = false;

    public static float CeilingJumpGraceTimer = 0f;

    public static float LastGroundJumpGraceTimer = 1f;

    public static void UpdateOnCeiling(Player player) {
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
            if (CeilingJumpEnabled || CeilingHyperEnabled) {
                CeilingJumpGraceTimer = 0.1f;
            }
        } else if (CeilingJumpGraceTimer > 0f) {
            CeilingJumpGraceTimer -= Engine.DeltaTime;
        }

        if (LastGroundJumpGraceTimer > 0f && player.jumpGraceTimer <= 0f) { // so it's killed by something like a jump
            CeilingJumpGraceTimer = 0f;
        }
    }

    private static void RecordGroundJumpGraceTimer(Player player) {
        LastGroundJumpGraceTimer = player.jumpGraceTimer;
    }

    private static void OnCeilingHookPlayerUpdate(ILContext il) {
        ILCursor cursor = new (il);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(UpdateOnCeiling); // only hiccup jump will affect this, so i dont insert this after onground evaluation
        LogHookData("Ceiling Jump & RefillStamina", "Player.orig_Update", true);

        bool success = true;
        if (cursor.TryGotoNext(MoveType.AfterLabel, ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.MatchLdfld<Player>(nameof(Player.dashRefillCooldownTimer)), ins => ins.MatchLdcR4(0f), ins => ins.OpCode == OpCodes.Ble_Un_S)) {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(RecordGroundJumpGraceTimer);
            ILLabel target = (ILLabel)cursor.Next.Next.Next.Next.Operand;
            cursor.GotoLabel(target);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(CheckCeilingRefillDash);
        }
        else {
            success = false;
        }
        LogHookData("Ceiling RefillDash", "Player.orig_Update", success);
    }

    public static void CheckCeilingRefillDash(Player player) {
        if (CeilingRefillDash && !player.Inventory.NoRefills && PlayerOnCeiling && (!player.CollideCheck<Spikes>() || SaveData.Instance.Assists.Invincible)) {
            player.RefillDash();
        }
    }


    public static void CeilingJump(this Player player, bool particles = true, bool playSfx = true) {
        player.maxFall = 240f;
        Input.Jump.ConsumeBuffer();
        player.jumpGraceTimer = 0f;
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
        LogHookData("Ceiling Jump", "Player.NormalUpdate", success);
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
        LogHookData("Ceiling Jump", "Player.SwimUpdate", success);
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
        LogHookData("Ceiling Jump", "Player.StarFlyUpdate", success);
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
        LogHookData("Ceiling Jump", "Player.HitSquashUpdate", success);
    }
    private static bool CheckAndApplyCeilingJumpInHitSquash(Player player) {
        if (CeilingJumpEnabled && PlayerOnCeiling) {
            player.CeilingJump();
            return true;
        }
        return false;
    }

    private static void CeilingHyper(this Player player) {
        Input.Jump.ConsumeBuffer();
        player.jumpGraceTimer = 0f;
        player.varJumpTimer = 0f; // as what we do in ceiling jump
        player.AutoJump = false;
        player.dashAttackTimer = 0f;
        player.gliderBoostTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;
        player.Speed.X = 260f * (float)player.Facing + player.LiftBoost.X;
        player.Speed.Y = +105f;
        player.gliderBoostTimer = 0.55f; // would be cursed i guess
        player.Play("event:/char/madeline/jump");
        if (player.Ducking) {
            player.CeilingUnduck();
            player.Speed.X *= 1.25f;
            player.Speed.Y *= 0.5f;
            player.Play("event:/char/madeline/jump_superslide");
            player.gliderBoostDir = Monocle.Calc.AngleToVector((float)Math.PI * 3f / 16f, 1f);
        }
        else {
            player.gliderBoostDir = Monocle.Calc.AngleToVector((float)Math.PI / 4f, 1f);
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
        LogHookData("Ceiling Hyper", "Player.DashUpdate", success);
    }

    private static bool SkipDashEndLoseSpeed(Player player) {
        bool result = UpdiagDashDontLoseHorizontalSpeed && player.DashDir.Y != 0f;
        if (result) {
            player.Speed.Y = player.DashDir.Y * 160f; // if vertical speed also doesn't get lost, then it would be a bit crazy
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
        LogHookData("Ceiling Hyper", "Player.RedDashUpdate", success);
    }

    private static bool CheckAndApplyCeilingHyper(Player player) {
        if (CeilingHyperEnabled && Math.Abs(player.DashDir.Y) < 0.1f && Input.Jump.Pressed && CeilingJumpGraceTimer > 0f && player.CanCeilingUnduck()) {
            player.CeilingHyper();
            return true;
        }
        return false;
    }

    private static bool CheckAndApplyCeilingHyperForRedDash(Player player) {
        bool flag = player.LastBooster != null && player.LastBooster.Ch9HubTransition;
        if (CeilingHyperEnabled && !flag && Math.Abs(player.DashDir.Y) < 0.1f && Input.Jump.Pressed && CeilingJumpGraceTimer > 0f && player.CanCeilingUnduck()) {
            player.CeilingHyper();
            return true;
        }
        return false;
    }

    private static void LogHookData(string hook, string methodBase, bool success) {
        if (success) {
            Logger.Log("CeilingUltra", $"{hook} hook {methodBase}");
        }
        else {
            Logger.Log(LogLevel.Warn, "CeilingUltra", $"{hook} fail to hook {methodBase}");
        }
    }
}