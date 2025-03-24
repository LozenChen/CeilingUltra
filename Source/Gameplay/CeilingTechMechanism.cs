using Celeste.Mod.CeilingUltra.Entities;
using Celeste.Mod.CeilingUltra.ModInterop;
using Celeste.Mod.CeilingUltra.Module;
using Celeste.Mod.CeilingUltra.Utils;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System.Runtime.CompilerServices;
using CelesteInput = Celeste.Input;

namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class CeilingTechMechanism {

    public static bool CeilingUltraEnabled => LevelSettings.CeilingUltraEnabled;

    public static bool GroundUltraEnabled => LevelSettings.GroundUltraEnabled;

    public static bool CeilingRefillStamina => LevelSettings.CeilingRefillStamina;

    public static bool WallRefillStamina => LevelSettings.WallRefillStamina;
    public static bool CeilingRefillDash => LevelSettings.CeilingRefillDash;

    public static bool WallRefillDash => LevelSettings.WallRefillDash;
    public static bool CeilingJumpEnabled => LevelSettings.CeilingJumpEnabled;

    public static bool GroundJumpEnabled => LevelSettings.GroundJumpEnabled;
    public static bool CeilingHyperEnabled => LevelSettings.CeilingHyperEnabled;

    public static bool GroundHyperEnabled => LevelSettings.GroundHyperEnabled;

    public static bool UpdiagDashDontLoseHorizontalSpeed => LevelSettings.UpdiagDashEndNoHorizontalSpeedLoss;

    public static bool UpdiagDashDontLoseVerticalSpeed => LevelSettings.UpdiagDashEndNoVerticalSpeedLoss;

    public static bool HorizontalUltraIntoVerticalUltra => LevelSettings.HorizontalUltraIntoVerticalUltra;

    [Load]
    public static void Load() {
        On.Celeste.Level.LoadNewPlayer += OnLoadNewPlayer;
        On.Celeste.FloatySpaceBlock.OnDash += FloatySpaceBlockOnDash;
    }


    [Unload]
    public static void Unload() {
        On.Celeste.Level.LoadNewPlayer -= OnLoadNewPlayer;
        On.Celeste.FloatySpaceBlock.OnDash -= FloatySpaceBlockOnDash;
    }

    [Initialize]
    public static void Initialize() {
        using (new DetourContext { Before = new List<string> { "*" }, ID = "Ceiling Tech Mechanism" }) {

            typeof(Player).GetMethodInfo("orig_Update").IlHook(HookPlayerUpdate);
            typeof(Player).GetMethodInfo("orig_Update").IlHook(UpdateOnEnd);
            typeof(Player).GetMethodInfo("OnCollideV").IlHook(CeilingUltraHookOnCollideV);
            typeof(Player).GetMethodInfo("DashCoroutine").GetStateMachineTarget().IlHook(CeilingVerticalUltraHookDashCoroutine);
            typeof(Player).GetMethodInfo("NormalUpdate").IlHook(CeilingJumpHookNormalUpdate);
            typeof(Player).GetMethodInfo("SwimUpdate").IlHook(CeilingJumpHookSwimUpdate);
            typeof(Player).GetMethodInfo("StarFlyUpdate").IlHook(CeilingJumpHookFeatherUpdate);
            typeof(Player).GetMethodInfo("HitSquashUpdate").IlHook(CeilingJumpHookHitSquashUpdate);
            typeof(Player).GetMethodInfo("DashUpdate").IlHook(CeilingHyperHookDashUpdate);
            typeof(Player).GetMethodInfo("RedDashUpdate").IlHook(CeilingHyperHookRedDashUpdate);

            new List<string> { "OnTransition", "Jump", "SuperJump", "SuperWallJump", "Bounce", "SuperBounce", "StarFlyBegin", "orig_WallJump", "SideBounce", "DreamDashEnd" }.Select(str => typeof(Player).GetMethodInfo(str)).ToList().ForEach(x => x.IlHook(SetExtendedJumpGraceTimerIL));

            new List<string> { "DashBegin", "RedDashBegin" }.Select(str => typeof(Player).GetMethodInfo(str)).ToList().ForEach(x => x.IlHook(ClearOverrideUltraDirHookDashBegin));

            typeof(Player).GetMethodInfo("DashUpdate").IlHook(ClearOverrideUltraDirHookDashUpdate);
        }
    }

    public static void UpdateOnCeilingAndWall(Player player) {
        if (!LastFrameWriteOverrideUltraDir && LastFrameDashDir != player.DashDir) {
            ClearOverrideUltraDir();
        }
        LastFrameDashDir = player.DashDir;
        LastFrameWriteOverrideUltraDir = false;

        if (LastGroundJumpGraceTimer > 0f && player.jumpGraceTimer <= 0f && !LastFrameSetJumpTimerCalled) {
            // so it's killed by something that maybe we have not hooked (e.g. a jump from other mods)
            // but if that's during a protect jump grace time, i will not care you mods (e.g. MaxHelpingHand.UpsideDownJumpThru hook OnCollideV)
            ClearExtendedJumpGraceTimer();
        }
        LastFrameSetJumpTimerCalled = false;

        if (player.StateMachine.State == 9) {
            PlayerOnCeiling = false;
            PlayerOnLeftWall = false;
            PlayerOnRightWall = false;
        }
        else {
            // there's no speed check coz:
            // it's easier to be on ground than on ceiling due to gravity
            // due to nature of a corner, it's hard to corner slip and wall refill at same time if there's a speed check
            PlayerOnCeiling = player.OnCeiling();
            PlayerOnLeftWall = player.CanStand(-Vector2.UnitX);
            PlayerOnRightWall = player.CanStand(Vector2.UnitX);
        }
        switch (FloatySpaceBlockDirection) {
            case 1: {
                PlayerOnRightWall = true;
                break;
            }
            case 2: {
                PlayerOnCeiling = true;
                break;
            }
            case 3: {
                PlayerOnLeftWall = true;
                break;
            }
            default: {
                break;
            }
        }
        FloatySpaceBlockDirection = 0;

        if (PlayerOnCeiling) {
            if (CeilingRefillStamina && !player.CollideCheck<IceCeiling>()) {
                player.Stamina = 110f;
                player.wallSlideTimer = 1.2f;
            }
            CeilingJumpGraceTimer = 0.1f;
        }
        else if (CeilingJumpGraceTimer > 0f) {
            CeilingJumpGraceTimer -= Engine.DeltaTime;
        }

        if (WallRefillStamina && (PlayerOnLeftWall && !ClimbBlocker.Check(player.Scene, player, player.Position - Vector2.UnitX) || PlayerOnRightWall && !ClimbBlocker.Check(player.Scene, player, player.Position + Vector2.UnitX))) {
            player.Stamina = 110f;
            player.wallSlideTimer = 1.2f;
        }
        if (PlayerOnLeftWall) {
            LeftWallGraceTimer = 0.1f;
        }
        else if (LeftWallGraceTimer > 0f) {
            LeftWallGraceTimer -= Engine.DeltaTime;
        }
        if (PlayerOnRightWall) {
            RightWallGraceTimer = 0.1f;
        }
        else if (RightWallGraceTimer > 0f) {
            RightWallGraceTimer -= Engine.DeltaTime;
        }
        if (!CelesteInput.Jump.Check && !player.AutoJump) {
            ProtectVarJumpTimer = 0f;
        }
        if (ProtectVarJumpTimer > 0f) {
            ProtectVarJumpTimer -= Engine.DeltaTime;
        }

        InstantUltraLeaveGround = false;
    }

    private static void HookPlayerUpdate(ILContext il) {
        ILCursor cursor = new(il);
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


    private static void SetExtendedJumpGraceTimerIL(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = false;
        while (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.MatchLdcR4(0f), ins => ins.MatchStfld<Player>(nameof(Player.jumpGraceTimer)))) {
            cursor.Index += 3;
            cursor.EmitDelegate(ClearExtendedJumpGraceTimer);
            success = true;
        }
        "Player.SomeSetJumpGraceTimerMethod".LogHookData("Set Jump Grace Timer", success);

    }

    private static void ClearOverrideUltraDirHookDashBegin(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        cursor.EmitDelegate(ClearOverrideUltraDir);
        "Player.(Red)DashBegin".LogHookData("Clear Override Ultra Dir", success);
    }

    private static void ClearOverrideUltraDirHookDashUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = false;
        while (cursor.TryGotoNext(MoveType.After, ins => ins.MatchStfld<Player>(nameof(Player.DashDir)))) {
            success = true;
            cursor.EmitDelegate(ClearOverrideUltraDir);
        }
        "Player.DashUpdate".LogHookData("Clear Override Ultra Dir", success);
    }

    public static void ClearExtendedJumpGraceTimer() {
        CeilingJumpGraceTimer = 0f;
        LeftWallGraceTimer = 0f;
        RightWallGraceTimer = 0f;
        ProtectVarJumpTimer = 0f;
        ProtectGroundSqueezeTimer = 0f;
        LastFrameSetJumpTimerCalled = true;
        jumpFlag = true;
    }


    private static Player OnLoadNewPlayer(On.Celeste.Level.orig_LoadNewPlayer orig, Vector2 Position, PlayerSpriteMode spriteMode) {
        Player player = orig(Position, spriteMode);
        ClearExtendedJumpGraceTimer();
        PlayerOnCeiling = PlayerOnLeftWall = PlayerOnRightWall = false;
        LastFrameSetJumpTimerCalled = false;
        LastGroundJumpGraceTimer = 1f;
        NextMaxFall = float.MinValue;
        ClearOverrideUltraDir();
        LastFrameWriteOverrideUltraDir = false;
        LastFrameDashDir = Vector2.Zero;
        FloatySpaceBlockDirection = 0;
        InstantUltraLeaveGround = false;
        return player;
    }

    public static void CeilingDuck(this Player player) {
        if (ModImports.IsPlayerInverted) {
            float origBottom = player.Collider.Bottom;
            player.Ducking = true;
            float offset = origBottom - player.Collider.Bottom;
            player.Y += offset;
        }
        else {
            float origTop = player.Collider.Top;
            player.Ducking = true;
            float offset = origTop - player.Collider.Top;
            player.Y += offset;
        }
    }

    public static bool TryCeilingDuck(this Player player, int priorDirection = 1) {
        if (player.IsSqueezed()) {
            int direction = priorDirection >= 0 ? 1 : -1;
            List<Vector2> wiggle = new List<Vector2>() { Vector2.Zero, direction * Vector2.UnitX, -direction * Vector2.UnitX };
            bool result = player.TryTransform(player.duckHitbox, Alignment.Top, wiggle);
            if (result) {
                player.hurtbox = player.duckHurtbox;
            }
            return result;
        }
        else {
            CeilingDuck(player);
            return true;
        }
    }

    public static bool TryGroundDuck(this Player player, int priorDirection = 1) {
        if (player.IsSqueezed()) {
            int direction = priorDirection >= 0 ? 1 : -1;
            List<Vector2> wiggle = new List<Vector2>() { Vector2.Zero, direction * Vector2.UnitX, -direction * Vector2.UnitX };
            bool result = player.TryTransform(player.duckHitbox, Alignment.Bottom, wiggle);
            if (result) {
                player.hurtbox = player.duckHurtbox;
            }
            return result;
        }
        else {
            player.Ducking = true;
            return true;
        }
    }

    public static bool TryCeilingUnduck(this Player player, out bool wasDuck, int priorDirection = 1) {
        wasDuck = player.Ducking;
        if (player.IsSqueezed()) {
            List<Vector2> wiggle = new List<Vector2>() { Vector2.Zero, priorDirection * Vector2.UnitX, -priorDirection * Vector2.UnitX };
            bool result2 = player.TryTransform(player.normalHitbox, Alignment.Top, wiggle);
            if (result2) {
                player.hurtbox = player.normalHurtbox;
            }
            return result2;
        }
        if (!player.Ducking) {
            return true;
        }
        bool result = player.TryTransform(player.normalHitbox, Alignment.Top, new List<Vector2>() { Vector2.Zero });
        if (result) {
            player.hurtbox = player.normalHurtbox;
        }

        return result;
    }

    public static bool TryCeilingUltra(this Player player, bool getOverrideVerticalUltra = false) {
        // why do we check Speed.Y <= 0f instead of < 0f here: coz MaxHelpingHand.UpsideDownJumpThru kills Speed.Y on the start of collision

        if (player.DashDir.X != 0f && player.DashDir.Y < 0f && player.Speed.Y <= 0f && player.TryCeilingDuck(Math.Sign(player.Speed.X))) {
            if (HorizontalUltraIntoVerticalUltra && VerticalTechMechanism.VerticalUltraEnabled && getOverrideVerticalUltra) {
                SetOverrideUltraDir(false, player.DashDir);
            }
            player.DashDir.X = Math.Sign(player.DashDir.X);
            player.DashDir.Y = 0f;
            player.Speed.Y = 0f;
            player.Speed.X *= 1.2f;
            return true;
        }
        return false;
    }

    public static bool TryGroundUltra(this Player player, bool getOverrideVerticalUltra = false) {

        if (player.DashDir.X != 0f && player.DashDir.Y > 0f && player.Speed.Y >= 0f && player.TryGroundDuck(Math.Sign(player.Speed.X))) {
            if (HorizontalUltraIntoVerticalUltra && VerticalTechMechanism.VerticalUltraEnabled && getOverrideVerticalUltra) {
                SetOverrideUltraDir(false, player.DashDir);
            }
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
        bool success1 = false;
        bool success2 = false;
        if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.DreamDashCheck)), ins => ins.OpCode == OpCodes.Brfalse_S)) {

            ILLabel label1 = (ILLabel)cursor.Next.Next.Operand;
            cursor.GotoLabel(label1);
            if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Beq)) {
                success1 = true;
                ILLabel target = (ILLabel)cursor.Next.Operand;
                cursor.GotoLabel(label1);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(CheckAndApplyGroundUltra);
                cursor.Emit(OpCodes.Brtrue, target);
            }

            if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.DreamDashCheck)), ins => ins.OpCode == OpCodes.Brfalse_S)) {
                success2 = true;
                ILLabel label2 = (ILLabel)cursor.Next.Next.Operand;
                cursor.GotoLabel(label2);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(CheckAndApplyCeilingUltra);
            }
        }
        "OnCollideV".LogHookData("Ground Ultra", success1);
        "OnCollideV".LogHookData("Ceiling Ultra", success2);
    }

    private static bool CheckAndApplyGroundUltra(Player player) {
        // return a value indicating whether we should go to the original ultra sentences
        if (!LevelSettings.MainEnabled) {
            return false;
        }

        if (GroundUltraEnabled) {
            if (OverrideGroundUltraDir.HasValue) {
                if (LeftWallGraceTimer <= 0f && RightWallGraceTimer <= 0f && player.TryGroundDuck(Math.Sign(player.Speed.X))) {
                    player.DashDir = OverrideGroundUltraDir.Value;
                    player.TryGroundUltra();
                }
                ClearOverrideUltraDir();
            }
            else {
                player.TryGroundUltra(true);
            }
        }

        // WARNING: we use our own ground ultra here, so other hooks on ultra will do nothing
        // so we don't do this unless our mod is enabled
        return true;
    }

    private static void CheckAndApplyCeilingUltra(Player player) {
        // why do we check Speed.Y <= 0f instead of < 0f here: coz MaxHelpingHand.UpsideDownJumpThru kills Speed.Y on the start of collision
        // fuck
        if (CeilingUltraEnabled && player.Speed.Y <= 0f) { // this does not lie in the Speed.Y < 0f branch, so we need to check here
            if (OverrideCeilingUltraDir.HasValue) {
                if (LeftWallGraceTimer <= 0f && RightWallGraceTimer <= 0f && player.TryCeilingDuck(Math.Sign(player.Speed.X))) {
                    player.DashDir = OverrideCeilingUltraDir.Value;
                    player.TryCeilingUltra();
                }
                ClearOverrideUltraDir();
            }
            else {
                player.TryCeilingUltra(true);
            }
        }
    }

    private static void CeilingVerticalUltraHookDashCoroutine(ILContext iLContext) {
        ILCursor cursor = new ILCursor(iLContext);
        bool success = false;

        if (cursor.TryGotoNext(
            ins => ins.OpCode == OpCodes.Ldloc_1,
            ins => ins.MatchLdfld<Player>(nameof(Player.onGround)),
            ins => ins.OpCode == OpCodes.Brfalse
            )) { // communal helper also hooks nearby (and will hard crash if hook fails), so we don't hook here, but move to next suitable place
            cursor.Index += 2;
            ILLabel target = (ILLabel)cursor.Next.Operand;
            if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Beq)) {
                cursor.Index++;
                cursor.EmitDelegate(CanGroundUltra);
                cursor.Emit(OpCodes.Brfalse, target);
                cursor.GotoLabel(target);
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.EmitDelegate(CheckCeilingVerticalUltraInDashCoroutine);
                success = true;
            }
        }
        "Player.DashCoroutine".LogHookData("Ceiling/Vertical Ultra", success);

        success = true;
        if (cursor.TryGotoNext(ins => ins.MatchLdcR4(160f))) {
            Instruction target = cursor.Next.Next.Next.Next; // goes to Speed.X *= swapCancel.X;
            cursor.Index -= 3;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(SkipDashEndLoseSpeed);
            cursor.Emit(OpCodes.Br, target);
        }
        else {
            success = false;
        }
        "Player.DashCoroutine".LogHookData("Updiag Dash End No Horizontal Speed Loss", success);
        "Player.DashCoroutine".LogHookData("Updiag Dash End No Vertical Speed Loss", success);
        "Player.DashCoroutine".LogHookData("Clear Override Ultra Dir", success);
    }

    private static void CheckCeilingVerticalUltraInDashCoroutine(Player player) {
        if (VerticalTechMechanism.VerticalUltraEnabled
            && HasSpeedX(player, out int sign) 
            && (PlayerOnRightWall && sign > 0 || PlayerOnLeftWall && sign < 0)
            && (!player.Inventory.DreamDash || !player.CollideCheck<DreamBlock>(player.Position + Vector2.UnitX * sign))
            && player.TryVerticalUltra()) {
            // already applied as a side effect of TryVerticalUltra
            // we put TryVerticalUltra inside conditions coz if all other conditions are satisfied but can't vertical ultra (e.g. Can't Squeeze Hitbox), then still need to try Ceiling Ultra
            if (player.DashDir.Y < 0f) {
                InstantUltraLeaveGround = true; // in this case, we dont auto unsqueeze in this frame (although we may be on ground)
            }

            ApplyEffectsOnMoveBlock(player, Vector2.UnitX * sign);
        }
        else if (CeilingUltraEnabled
            && PlayerOnCeiling
            && (!player.Inventory.DreamDash || !player.CollideCheck<DreamBlock>(player.Position - Vector2.UnitY * ModImports.InvertY))
            && player.TryCeilingUltra()) {
            // already applied

            ApplyEffectsOnMoveBlock(player, -Vector2.UnitY * ModImports.InvertY);

            // if ground ultra, then MoveBlock can be triggered by being standing on, so we don't need to apply effects
        }
        // try vertical ultra first, so it matchs the intuition that, first horizontal movement, then vertical
        // although that actually ground ultra > vertical ultra > ceiling ultra
        // ground ultra must be first so a grounded reverse hyper can be performed normally at a corner

        ClearOverrideUltraDir();
        // player.DashDir is created this frame, so we need to clear override ultra dir
        // and such instant ultra should't produce override ultra dir, so we just clear override anyway
    }

    private static void ApplyEffectsOnMoveBlock(Player player, Vector2 dir) {
        Vector2 origPosition = player.Position;
        player.Position += dir;
        foreach (MoveBlockOnDashCollide moveBlock in player.Scene.Tracker.GetEntities<MoveBlockOnDashCollide>()
            .Cast<MoveBlockOnDashCollide>().Where(x => x.ActivateOnDashCollide && x.AllowInstantUltra && x.Collidable)) {
            if (Collide.Check(player, moveBlock)) {
                moveBlock.OnDashCollide.Invoke(player, dir);
            }
        }
        player.Position = origPosition;
    }

    private static bool HasSpeedX(Player player, out int sign) {
        sign = Math.Sign(player.Speed.X);
        return sign != 0;
    }

    public static bool OnCeiling(this Player player, int upCheck = 1) {
        return player.CanStand(-upCheck * Vector2.UnitY * ModImports.InvertY);
    }

    [SaveLoad]
    public static bool PlayerOnCeiling = false;

    [SaveLoad]
    public static bool PlayerOnLeftWall = false;

    [SaveLoad]
    public static bool PlayerOnRightWall = false;

    [SaveLoad]

    public static int FloatySpaceBlockDirection = 0;

    public static bool PlayerOnWall => PlayerOnLeftWall || PlayerOnRightWall;

    [SaveLoad]
    public static float CeilingJumpGraceTimer = 0f;

    [SaveLoad]
    public static float LeftWallGraceTimer = 0f;

    [SaveLoad]
    public static float RightWallGraceTimer = 0f;

    [SaveLoad]
    public static float ProtectVarJumpTimer = 0f;

    [SaveLoad]
    public static float ProtectGroundSqueezeTimer = 0f;

    [SaveLoad]
    public static float LastGroundJumpGraceTimer = 1f;

    [SaveLoad]
    public static bool LastFrameSetJumpTimerCalled = false;

    private static bool jumpFlag = false;
    // used to detect if any jump is performed (so we shouldn't perform a ceiling jump in this case)
    // no need to save load as its "lifetime" is inside Player.NormalUpdate

    [SaveLoad]
    public static Vector2? OverrideGroundUltraDir = null;

    [SaveLoad]
    public static Vector2? OverrideCeilingUltraDir = null;

    [SaveLoad]
    public static Vector2? OverrideLeftWallUltraDir = null;

    [SaveLoad]
    public static Vector2? OverrideRightWallUltraDir = null;

    [SaveLoad]
    public static bool LastFrameWriteOverrideUltraDir = false;

    [SaveLoad]
    public static Vector2 LastFrameDashDir = Vector2.Zero;

    [SaveLoad]
    public static float NextMaxFall = float.MinValue;

    [SaveLoad]
    public static bool InstantUltraLeaveGround = false;

    public static void SetOverrideUltraDir(bool isVerticalUltra, Vector2 dashDir) {
        ClearOverrideUltraDir();
        if (isVerticalUltra) {
            if (dashDir.Y > 0) {
                OverrideGroundUltraDir = dashDir;
            }
            else {
                OverrideCeilingUltraDir = dashDir;
            }
        }
        else {
            if (dashDir.X > 0) {
                OverrideRightWallUltraDir = dashDir;
            }
            else {
                OverrideLeftWallUltraDir = dashDir;
            }
        }
        LastFrameWriteOverrideUltraDir = true;

    }
    public static void ClearOverrideUltraDir() {
        OverrideGroundUltraDir = OverrideCeilingUltraDir = OverrideLeftWallUltraDir = OverrideRightWallUltraDir = null;
    }
    private static void UpdateOnEnd(ILContext il) {
        ILCursor cursor = new(il);
        bool success = false;
        while (cursor.TryGotoNext(MoveType.AfterLabel, i => i.OpCode == OpCodes.Ret)) {
            success = true;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(UpdateMaxFallAndJumpGrace);
            cursor.Index++;
        }
        "Player.orig_Update".LogHookData("Set MaxFall Helper", success);
    }

    private static void UpdateMaxFallAndJumpGrace(Player player) {
        if (NextMaxFall > player.maxFall && player.StateMachine.State == 0) { // NormalBegin resets maxFall to be 160f, so we need this for vertical hyper
            player.maxFall = NextMaxFall;
        }
        NextMaxFall = float.MinValue;
        if (!LastFrameSetJumpTimerCalled && ProtectVarJumpTimer > player.varJumpTimer) {
            player.varJumpTimer = ProtectVarJumpTimer;
        }
    }


    private static void RecordGroundJumpGraceTimer(Player player) {
        LastGroundJumpGraceTimer = player.jumpGraceTimer;
    }

    public static void ExtendedRefillDash(Player player) {
        if (!player.Inventory.NoRefills && player.Dashes < player.MaxDashes
            && (
                (CeilingRefillDash && PlayerOnCeiling && !player.CollideCheck<IceCeiling>()) ||
                (WallRefillDash && (PlayerOnLeftWall && !ClimbBlocker.Check(player.Scene, player, player.Position - Vector2.UnitX) || PlayerOnRightWall && !ClimbBlocker.Check(player.Scene, player, player.Position + Vector2.UnitX)))
            )
            && (!FixedSpikeCollisionCheck(player) || SaveData.Instance.Assists.Invincible)) {
            player.RefillDash();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool FixedSpikeCollisionCheck(Player player) {
        /*
        Collider orig = player.Collider;
        player.Collider = player.hurtbox;
        // we are not using PlayerCollider, so there's no side effect
        bool result = Collide.Check(player, Engine.Scene.Tracker.GetEntities<Spikes>());
        player.Collider = orig;
        return result;
        */
        return Collide.Check(player, Engine.Scene.Tracker.GetEntities<Spikes>());
        // some examples:
        // 1) a spiked corner boost, i think we shouldn't get dashes back in this case
        // 2) grab a wall, but our feet touches an upward spike, i think we should refill dash?
        // 3) our right feet, but not other pixels of our right-side, touches the right wall, i think we should refill dash
        // this looks quite contradictory...
    }

    public static void CeilingJump(this Player player, bool particles = true, bool playSfx = true) {
        int priorDirection = (int)player.moveX;
        if (priorDirection == 0) {
            priorDirection = (int)player.Facing;
        }
        player.TryCeilingUnduck(out _, priorDirection); // it's ok if you can't unduck

        Input.Jump.ConsumeBuffer();
        player.jumpGraceTimer = 0f;
        ClearExtendedJumpGraceTimer();
        player.varJumpTimer = 0f; // does not produce varJumpTimer
        player.AutoJump = false;
        player.dashAttackTimer = 0f;
        player.gliderBoostTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;
        player.Speed.X += 40f * (float)player.moveX + player.LiftBoost.X;
        bool downPressed = CelesteInput.MoveY > 0;
        if (downPressed) {
            NextMaxFall = 320f;
            player.Speed.Y = +280f;
            player.Sprite.Scale = new Vector2(0.5f, 1.5f);
        }
        else {
            NextMaxFall = 240f;
            player.Speed.Y = +105f;
            player.Sprite.Scale = new Vector2(0.6f, 1.4f);
        }
        LiftBoostY.OnCeilingJump(player);
        player.varJumpSpeed = player.Speed.Y;
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
        if (particles) {
            int index = -1;
            Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(player.CollideAll<Platform>(player.Position - Vector2.UnitY * ModImports.InvertY, player.temp));
            if (platformByPriority != null) {
                index = platformByPriority.GetLandSoundIndex(player);
            }
            Dust.BurstFG(ModImports.IsPlayerInverted ? player.BottomCenter : player.TopCenter, (float)Math.PI / 2f * ModImports.InvertY, downPressed ? 8 : 4, downPressed ? 8f : 4f, player.DustParticleFromSurfaceIndex(index));
        }
        SaveData.Instance.TotalJumps++;

        ModUtils.ExtendedVariantsUtils.TryCeilingUltraJump(player, priorDirection);
    }

    private static void CeilingJumpHookNormalUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = false;
        if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.MatchLdfld<Player>(nameof(Player.jumpGraceTimer)), ins => ins.MatchLdcR4(0f))) {
            cursor.MoveAfterLabels();
            cursor.EmitDelegate(ResetJumpFlag);
            if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ble_Un_S)) {
                ILLabel target = (ILLabel)cursor.Next.Operand;
                cursor.Index++;
                cursor.EmitDelegate(CanGroundJump_Parameter0);
                cursor.Emit(OpCodes.Brfalse, target);
                if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldc_I4_0, ins => ins.OpCode == OpCodes.Ret)) {
                    cursor.MoveAfterLabels();
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.EmitDelegate(CheckAndApplyCeilingJumpInNormal);
                    success = true;
                }
            }
        }
        "Player.NormalUpdate".LogHookData("Ceiling Jump", success);
        // let ceiling jump has lowest priority
        // since in most cases we dont expect a ceiling jump
    }


    private static void ResetJumpFlag() {
        jumpFlag = false;
    }
    private static void CheckAndApplyCeilingJumpInNormal(Player player) {
        if (CeilingJumpEnabled && CeilingJumpGraceTimer > 0f && CelesteInput.Jump.Pressed && !jumpFlag && (TalkComponent.PlayerOver == null || !CelesteInput.Talk.Pressed)) {
            player.CeilingJump();
        }
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
        if (cursor.TryGotoNext(ins => ins.MatchCall<Actor>(nameof(Actor.OnGround)))) {
            cursor.Index++;
            cursor.EmitDelegate(CanGroundJump_Parameter1);
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
        if (cursor.TryGotoNext(ins => ins.MatchLdfld<Player>(nameof(Player.onGround)))) {
            cursor.Index++;
            cursor.EmitDelegate(CanGroundJump_Parameter1);
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
        ClearExtendedJumpGraceTimer();
        player.varJumpTimer = 0f; // as what we do in ceiling jump
        player.AutoJump = false;
        player.dashAttackTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;
        player.Speed.X = 260f * xDirection + player.LiftBoost.X;
        player.Speed.Y = +105f;
        LiftBoostY.OnCeilingHyper(player);
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
        Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(player.CollideAll<Platform>(player.Position - Vector2.UnitY * ModImports.InvertY, player.temp));
        if (platformByPriority != null) {
            index = platformByPriority.GetLandSoundIndex(player);
        }
        Dust.Burst(ModImports.IsPlayerInverted ? player.BottomCenter : player.TopCenter, (float)Math.PI / 2f * ModImports.InvertY, 4, player.DustParticleFromSurfaceIndex(index));
        SaveData.Instance.TotalJumps++;
    }

    private static void CeilingHyperHookDashUpdate(ILContext il) {
        ILCursor cursor = new(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.MatchLdsfld(typeof(CelesteInput).FullName, nameof(CelesteInput.Jump)), ins => ins.MatchCallvirt<VirtualButton>("get_Pressed"))) {
            cursor.Index += 2;
            cursor.EmitDelegate(CanGroundHyper);
        }
        else {
            success = false;
        }
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
    private static void SkipDashEndLoseSpeed(Player player) {
        // we are inside "if (DashDir.Y <= 0f)"
        if (player.DashDir.Y == 0f) {
            player.Speed = player.DashDir * 160f;
            return;
        }

        if (player.DashDir.X == 0f) {
            if (OverrideCeilingUltraDir.HasValue) {
                player.Speed = OverrideCeilingUltraDir.Value * 160f;
            }
            // if you updiag dash into a wall and vertical ultra makes your dashdir = (0, -1), we make dash end as if your dash dir is not changed
            else {
                player.Speed = player.DashDir * 160f;
            }
            return;
        }

        // now DashDir.Y < 0f && DashDir.X != 0f

        if (UpdiagDashDontLoseHorizontalSpeed) {
            // player.Speed.X remain invariant
        }
        else {
            player.Speed.X = player.DashDir.X * 160f;
        }

        if (UpdiagDashDontLoseVerticalSpeed) {
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

    private static void CeilingHyperHookRedDashUpdate(ILContext il) {
        ILCursor cursor = new(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.MatchLdsfld(typeof(CelesteInput).FullName, nameof(CelesteInput.Jump)), ins => ins.MatchCallvirt<VirtualButton>("get_Pressed"))) {
            cursor.Index += 2;
            cursor.EmitDelegate(CanGroundHyper);
        }
        else {
            success = false;
        }
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

    // Ceiling Hyper 暂且译为: 穹顶漫步. 不过表意上比较精准的可能是, 踏天疾跃.
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

    private static DashCollisionResults FloatySpaceBlockOnDash(On.Celeste.FloatySpaceBlock.orig_OnDash orig, FloatySpaceBlock self, Player player, Vector2 direction) {
        // floaty space block will only carry player riding it to move, but not under/besides
        // so it will be a bit inconvient for us

        if (self.MasterOfGroup && self.dashEase <= 0.2f && player != null) {
            if (direction.X > 0) {
                FloatySpaceBlockDirection = 1;
            }
            else if (direction.X < 0) {
                FloatySpaceBlockDirection = 3;
            }
            else {
                FloatySpaceBlockDirection = direction.Y * ModImports.InvertY < 0 ? 2 : 0;
            }
        }
        return orig(self, player, direction);
    }

    private static bool CanGroundJump_Parameter0() {
        return GroundJumpEnabled;

    }
    private static bool CanGroundJump_Parameter1(bool origValue) {
        return origValue && GroundJumpEnabled;
    }

    private static bool CanGroundHyper(bool origValue) {
        return origValue && GroundHyperEnabled;
    }

    private static bool CanGroundUltra() {
        return GroundUltraEnabled;
    }
}