using Celeste.Mod.CeilingUltra.Module;
using Celeste.Mod.CeilingUltra.Utils;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using CelesteInput = Celeste.Input;

namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class VerticalTechMechanism {

    public static bool VerticalUltraEnabled => LevelSettings.VerticalUltraEnabled;

    public static bool VerticalHyperEnabled => LevelSettings.VerticalHyperEnabled;

    public static bool DashBeginDontLoseVerticalSpeed => LevelSettings.DashBeginNoVerticalSpeedLoss;

    public static bool VerticalUltraIntoHorizontalUltra => LevelSettings.VerticalUltraIntoHorizontalUltra;

    public static bool UpwardWallJumpAcceleration => LevelSettings.UpwardWallJumpAcceleration;

    public static bool DownwardWallJumpAcceleration => LevelSettings.DownwardWallJumpAcceleration;

    [Load]
    public static void Load() {
        On.Celeste.Level.LoadNewPlayer += OnLoadNewPlayer;
        On.Celeste.Player.WallJump += OnPlayerWallJump;
    }

    [Unload]
    public static void Unload() {
        On.Celeste.Level.LoadNewPlayer -= OnLoadNewPlayer;
        On.Celeste.Player.WallJump -= OnPlayerWallJump;
    }

    [Initialize]
    public static void Initialize() {
        using (new DetourContext { Before = new List<string> { "*" }, ID = "Vertical Tech Mechanism" }) {
            typeof(Player).GetMethodInfo("OnCollideH").IlHook(VerticalUltraHookOnCollideH);
            typeof(Player).GetMethodInfo("get_CanUnDuck").IlHook(ModifyCanUnDuck);
            typeof(Player).GetMethodInfo("set_Ducking").IlHook(ModifySetDucking);
            typeof(Player).GetMethodInfo("orig_Update").IlHook(AutoUnsqueeze);
            typeof(Player).GetMethodInfo("DashUpdate").IlHook(VerticalHyperHookDashUpdate);
            typeof(Player).GetMethodInfo("RedDashUpdate").IlHook(VerticalHyperHookDashUpdate);
            typeof(Player).GetMethodInfo("DashCoroutine").GetStateMachineTarget().IlHook(DashBeginDontLoseVertSpeed);
            typeof(Player).GetMethodInfo("OnCollideV").IlHook(ProtectVarJumpTimer);
        }

        int[] xOffsets = { 0, 1, -1 };
        UnSqueezeToDuckWiggle = new List<Vector2>();
        for (int yOffset = 0; yOffset <= 5; yOffset++) {
            foreach (int xOffset in xOffsets) {
                UnSqueezeToDuckWiggle.Add(new Vector2(xOffset, yOffset));
            }
        }
    }

    private static Hitbox squeezedHitbox = new Hitbox(6f, 11f, -3f, -11f);

    private static Hitbox squeezedHurtbox = new Hitbox(6f, 9f, -3f, -11f);

    private const float upperWallJumpIncrement = -20f;

    private const float downwardWallJumpIncrement = 40f;
    private static Player OnLoadNewPlayer(On.Celeste.Level.orig_LoadNewPlayer orig, Vector2 Position, PlayerSpriteMode spriteMode) {
        Player player = orig(Position, spriteMode);
        squeezedHitbox = new Hitbox(6f, 11f, -3f, -11f);
        squeezedHurtbox = new Hitbox(6f, 9f, -3f, -11f);
        return player;
    }

    private static void OnPlayerWallJump(On.Celeste.Player.orig_WallJump orig, Player player, int dir) {
        float origSpeedY = player.Speed.Y;
        orig(player, dir); // ExtendedJumpGraceTimer is cleared here
        if (player.StateMachine.State != 0 && player.StateMachine.State != 1) {
            return;
        }
        else if (CelesteInput.MoveY < 0 && UpwardWallJumpAcceleration) {
            player.Speed.Y = Math.Min(-105f, origSpeedY + upperWallJumpIncrement);
            player.Speed.Y += player.LiftBoost.Y;
            player.varJumpSpeed = player.Speed.Y;
            player.varJumpTimer = Monocle.Calc.LerpClamp(0.2f, 0.1f, (player.Speed.Y - (-105f)) / (-325f - (-105f)));
        }
        else if (CelesteInput.MoveY > 0 && DownwardWallJumpAcceleration) {
            player.Speed.Y = Math.Max(40f, origSpeedY + downwardWallJumpIncrement);
            CeilingTechMechanism.NextMaxFall = player.Speed.Y + 20f;
            player.varJumpTimer = 0f;
        }
        else {
            return;
        }

        player.LaunchedBoostCheck();
    }


    public static bool IsSqueezed(this Player player) {
        return player.Collider == squeezedHitbox || player.Collider == squeezedHurtbox;
    }
    public static void SetSqueezedHitbox(this Player player) {
        player.Collider = squeezedHitbox;
        player.hurtbox = squeezedHurtbox;
    }

    public static bool CanSqueezeHitbox(this Player player, float xDirection, float yDirection, out Vector2 offset) {
        // we do this check so if maddy crushes into the wall with a duck hitbox, it's still properly handled
        if (player.Collider == squeezedHitbox) {
            offset = Vector2.Zero;
            return true;
        }
        Alignment alignment = Alignment.TopLeft;

        if (xDirection < 0 && yDirection <= 0) {
            alignment = Alignment.BottomLeft;
        }
        else if (xDirection < 0 && yDirection > 0) {
            alignment = Alignment.TopLeft;
        }
        else if (xDirection > 0 && yDirection <= 0) {
            alignment = Alignment.BottomRight;
        }
        else if (xDirection > 0 && yDirection > 0) {
            alignment = Alignment.TopRight;
        }
        else {
            throw new Exception($"[CeilingUltra] Unexcepted Parameters In {nameof(VerticalTechMechanism)}.{nameof(CanSqueezeHitbox)}");
        }

        return player.CanTransform(squeezedHitbox, alignment, out offset);
    }

    public static bool TrySqueezeHitbox(this Player player, float xDirection, float yDirection) {
        if (player.CanSqueezeHitbox(xDirection, yDirection, out Vector2 offset)) {
            player.NaiveMove(offset);
            player.SetSqueezedHitbox();
            return true;
        }
        else {
            return false;
        }
    }

    private static void VerticalUltraHookOnCollideH(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = false;
        if (cursor.TryGotoNext(ins => ins.MatchCallOrCallvirt<Player>(nameof(Player.DreamDashCheck)), ins => ins.OpCode == OpCodes.Brfalse_S)) {
            ILLabel label = (ILLabel)cursor.Next.Next.Operand; // goto wall speed retention
            cursor.GotoLabel(label);
            if (cursor.Next.Next.Next.Next.MatchBgtUn(out ILLabel label2)) {
                success = true;
                cursor.MoveAfterLabels();
                cursor.Emit(OpCodes.Ldarg_0);
                // cursor.EmitDelegate(TryVerticalUltra);
                cursor.EmitDelegate(CheckAndApplyVerticalUltraWithRetention);
                cursor.Emit(OpCodes.Brtrue, label2); // skip wallSpeedRetention, as in some sense, an ultra converts vertical speed into horizontal speed, so a vertical ultra should converts horizontal speed into vertical, and you will lose your horizontal speed anyway
                // no i've changed my idea now, I LOVE WALL SPEED RETAINED
            }

        }
        "Player.OnCollideH".LogHookData("Vertical Ultra", success);
    }

    public static bool TryVerticalUltra(this Player player) {
        if (Math.Sign(player.DashDir.X) is { } xSign && xSign != 0 && xSign == Math.Sign(player.Speed.X) && player.DashDir.Y != 0f && player.TrySqueezeHitbox(xSign, player.Speed.Y)) {
            player.DashDir.Y = Math.Sign(player.DashDir.Y); // wow, this allows you to super wall jump after an upward vertical ultra
            player.DashDir.X = 0f;
            player.Speed.X = 0f;
            player.Speed.Y *= 1.2f;
            player.Sprite.Scale = new Vector2(0.5f, 1.5f);
            return true;
        }
        return false;
    }

    private static bool CheckAndApplyVerticalUltraWithRetention(this Player player) {
        if (!VerticalUltraEnabled) {
            return false;
        }
        return TryVerticalUltraWithRetention(player);
    }

    public static bool TryVerticalUltraWithRetention(this Player player) {
        // return true if wallSpeedRetained is set
        if (CeilingTechMechanism.OverrideLeftWallUltraDir.HasValue) {
            if (-1 != Math.Sign(player.Speed.X)) {
                return false;
            }
            else if (player.jumpGraceTimer > 0f || CeilingTechMechanism.CeilingJumpGraceTimer > 0f) {
                CeilingTechMechanism.ClearOverrideUltraDir();
                return false;
            }
            else if (player.TrySqueezeHitbox(-1, player.Speed.Y)) {
                player.DashDir = CeilingTechMechanism.OverrideLeftWallUltraDir.Value;
                CeilingTechMechanism.ClearOverrideUltraDir();
                player.wallSpeedRetained = player.Speed.X;
                player.wallSpeedRetentionTimer = 0.06f;
                player.DashDir.Y = Math.Sign(player.DashDir.Y);
                player.DashDir.X = 0f;
                player.Speed.X = 0f;
                player.Speed.Y *= 1.2f;
                player.Sprite.Scale = new Vector2(0.5f, 1.5f);
                return true;
            }
            else {
                CeilingTechMechanism.ClearOverrideUltraDir();
                return false;
            }
        }
        else if (CeilingTechMechanism.OverrideRightWallUltraDir.HasValue) {
            if (1 != Math.Sign(player.Speed.X)) {
                return false;
            }
            else if (player.jumpGraceTimer > 0f || CeilingTechMechanism.CeilingJumpGraceTimer > 0f) {
                CeilingTechMechanism.ClearOverrideUltraDir();
                return false;
            }
            else if (player.TrySqueezeHitbox(1, player.Speed.Y)) {
                player.DashDir = CeilingTechMechanism.OverrideRightWallUltraDir.Value;
                CeilingTechMechanism.ClearOverrideUltraDir();
                player.wallSpeedRetained = player.Speed.X;
                player.wallSpeedRetentionTimer = 0.06f;
                player.DashDir.Y = Math.Sign(player.DashDir.Y);
                player.DashDir.X = 0f;
                player.Speed.X = 0f;
                player.Speed.Y *= 1.2f;
                player.Sprite.Scale = new Vector2(0.5f, 1.5f);
                return true;
            }
            else {
                CeilingTechMechanism.ClearOverrideUltraDir();
                return false;
            }
        }

        if (Math.Sign(player.DashDir.X) is { } xSign && xSign != 0 && xSign == Math.Sign(player.Speed.X) && player.DashDir.Y != 0f && player.TrySqueezeHitbox(xSign, player.Speed.Y)) {
            if (VerticalUltraIntoHorizontalUltra) {
                if (player.DashDir.Y < 0f) {
                    if (CeilingTechMechanism.CeilingUltraEnabled) {
                        CeilingTechMechanism.SetOverrideUltraDir(true, player.DashDir);
                    }
                    else {
                        // don't set a meaningless override
                    }
                }
                else {
                    // it's grounded so it always make sense
                    CeilingTechMechanism.SetOverrideUltraDir(true, player.DashDir);
                }
            }
            player.wallSpeedRetained = player.Speed.X;
            player.wallSpeedRetentionTimer = 0.06f;
            player.DashDir.Y = Math.Sign(player.DashDir.Y); // wow, this should allow you to super wall jump after an upward vertical ultra (if you manage to unsqueeze during your dash)(or if you dont enable vertical hyper)
            player.DashDir.X = 0f;
            player.Speed.X = 0f;
            player.Speed.Y *= 1.2f;
            player.Sprite.Scale = new Vector2(0.5f, 1.5f);
            return true;
        }
        return false;
    }

    private static void ModifyCanUnDuck(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        Instruction target = cursor.Next;
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(IsSqueezed);
        cursor.Emit(OpCodes.Brfalse, target);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(CanUnSqueezeToUnDuck);
        cursor.Emit(OpCodes.Ret);
        "Player.get_CanUnDuck".LogHookData("Compatiblity with SqueezedHitbox", true);
    }

    private static bool CanUnSqueezeToUnDuck(this Player player) {
        return player.CanTransform(player.normalHitbox, Alignment.No, UnsqueezeToUnDuckWiggle, out _);
    }

    private static readonly List<Vector2> UnsqueezeToUnDuckWiggle = new List<Vector2>() { Vector2.Zero, Vector2.UnitX, -Vector2.UnitX };

    private static List<Vector2> UnSqueezeToDuckWiggle;

    private static void UnSqueeze(this Player player, bool duck = false) {
        if (!duck) {
            // in most cases, Ducking = false goes with a CanUnDuck check
            if (player.TryTransform(player.normalHitbox, UnsqueezeToUnDuckWiggle)) {
                player.hurtbox = player.normalHurtbox;
            }
        }
        else {
            // however, Ducking = true almost has no check
            if (player.TryTransform(player.duckHitbox, UnSqueezeToDuckWiggle)) {
                player.hurtbox = player.duckHurtbox;
            }
        }
    }


    private static void ModifySetDucking(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        Instruction next = cursor.Next;
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(IsSqueezed);
        cursor.Emit(OpCodes.Brfalse, next);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.Emit(OpCodes.Ldarg_1);
        cursor.EmitDelegate(UnSqueeze);
        cursor.Emit(OpCodes.Ret);
        "Player.set_Ducking".LogHookData("Compatiblity with SqueezedHitbox", true);
    }

    private static void AutoUnsqueeze(ILContext il) {
        // to conclude, once you are Squeezed, you can keep this state all along a left/right wall, until you touch ground (or untii you wallslide / grab a wall in normal update etc)
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.OpCode == OpCodes.Ldc_I4_0, ins => ins.MatchCallOrCallvirt<Player>("set_Ducking"))) {
            ILLabel label = (ILLabel)cursor.Prev.Operand;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(SkipUnSqueezeInOrigUpdate);
            cursor.Emit(OpCodes.Brtrue, label);
            cursor.GotoLabel(label);
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(UnsqueezeOnGround);
        }
        else {
            success = false;
        }
        "Player.orig_Update".LogHookData("Auto Unsqueeze", success);
    }

    private static bool SkipUnSqueezeInOrigUpdate(Player player) {
        return player.IsSqueezed() && (CeilingTechMechanism.LeftWallGraceTimer > 0f || CeilingTechMechanism.RightWallGraceTimer > 0f);
    }

    private static void UnsqueezeOnGround(Player player) {
        if (player.IsSqueezed()) {
            if (!player.onGround) {
                CeilingTechMechanism.ProtectGroundSqueezeTimer = 0.06f; // gives an extra 4f window if you down diag dash to a corner and want to vertical hyper
            }
            else {
                if (CeilingTechMechanism.ProtectGroundSqueezeTimer > 0f) {
                    CeilingTechMechanism.ProtectGroundSqueezeTimer -= Engine.DeltaTime;
                }
                else if (player.CanUnDuck) {
                    player.Ducking = false;
                }
            }
        }
    }

    private static void VerticalHyper(this Player player, int xDirection, int yDirection) {
        // sadly, if you buffer a jump then you will get a wall jump instead of a vertical hyper
        // only hyper, no vertical super jump (which is replaced a super wall jump)
        Input.Jump.ConsumeBuffer();
        player.jumpGraceTimer = 0f;
        CeilingTechMechanism.ClearExtendedJumpGraceTimer();
        player.AutoJump = false;
        player.dashAttackTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;

        player.Speed.X = 105f * xDirection + player.LiftBoost.X;
        player.Speed.Y = 260f * yDirection + (yDirection < 0 ? player.LiftBoost.Y : 0f);
        player.gliderBoostTimer = 0.55f;
        player.Play("event:/char/madeline/jump");

        if (true) { // always Squeezed
            player.UnSqueeze(false);
            player.Speed.X *= 0.5f;
            player.Speed.Y *= 1.25f;
            player.Play("event:/char/madeline/jump_superslide");
            player.gliderBoostDir = Monocle.Calc.AngleToVector((float)Math.PI * (1f / 2f - 3f / 16f * xDirection) * yDirection, 1f);
        }
        else {
            // how
        }

        if (player.Speed.Y < 0f) {
            player.varJumpTimer = 0.1f; // 12 frames is a bit too long so i cut it half (although it's already a crazy mod)
            // bad news: in this case, OnCollideV will immediately kill varJumpTimer since varJumpTimer < 0.15f
            player.varJumpSpeed = player.Speed.Y;
            CeilingTechMechanism.ProtectVarJumpTimer = player.varJumpTimer; // so we invent this to protect varJumpTimer, this will be enough to go around a 1px corner
        }
        else {
            player.varJumpTimer = 0f;
            player.varJumpSpeed = 0f;
            CeilingTechMechanism.NextMaxFall = 350f;
        }

        player.launched = true;
        player.Sprite.Scale = new Vector2(0.6f, 1.4f);
        int index = -1;
        Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(player.CollideAll<Platform>(player.Position - xDirection * Vector2.UnitX, player.temp));
        if (platformByPriority != null) {
            index = platformByPriority.GetLandSoundIndex(player);
        }
        Dust.BurstFG(xDirection > 0 ? player.CenterLeft : player.CenterRight, xDirection > 0f ? 0f : (float)Math.PI, yDirection > 0 ? 6 : 4, yDirection > 0 ? 6f : 4f, player.DustParticleFromSurfaceIndex(index));
        SaveData.Instance.TotalJumps++;
    }

    private static void VerticalHyperHookDashUpdate(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.MatchCallOrCallvirt<Player>("get_SuperWallJumpAngleCheck"))) {
            Instruction next = cursor.Next;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(TryVerticalHyper);
            cursor.Emit(OpCodes.Brfalse, next);
            cursor.Emit(OpCodes.Ldc_I4_0);
            cursor.Emit(OpCodes.Ret);
        }
        else {
            success = false;
        }
        "Player.(Red)DashUpdate".LogHookData("Vertical Hyper", success);
    }

    private static bool TryVerticalHyper(Player player) {
        if (VerticalHyperEnabled && player.IsSqueezed() && CelesteInput.Jump.Pressed && Math.Abs(player.DashDir.X) < 0.1f && player.CanUnSqueezeToUnDuck()) {
            if (CelesteInput.GrabCheck && player.Stamina > 0f && player.Holding == null) {
                if (player.WallJumpCheck(1) && player.Facing == Facings.Right && !ClimbBlocker.Check(player.Scene, player, player.Position + Vector2.UnitX * 3f)) {
                    player.ClimbJump();
                    return true;
                }
                if (player.WallJumpCheck(-1) && player.Facing == Facings.Left && !ClimbBlocker.Check(player.Scene, player, player.Position - Vector2.UnitX * 3f)) {
                    player.ClimbJump();
                    return true;
                }
            }

            int yDirection = Math.Sign(CelesteInput.MoveY);
            if (yDirection == 0) {
                yDirection = -1;
            }
            int wantedDirection = CelesteInput.MoveX != 0f ? CelesteInput.MoveX : (int)player.Facing; // we dont use player.moveX so forceMoveX is ignored lol
            if (player.CanStand(-wantedDirection * Vector2.UnitX)) {
                player.VerticalHyper(wantedDirection, yDirection);
                return true;
            }
            if (player.CanStand(wantedDirection * Vector2.UnitX)) {
                player.VerticalHyper(-wantedDirection, yDirection);
                return true;
            }
            // in previous two cases, we force the direction according to the wall. if there are two walls, based on your wanted direction
            // now there's no walls
            if (CeilingTechMechanism.LeftWallGraceTimer > 0f) {
                if (CeilingTechMechanism.RightWallGraceTimer > 0f) {
                    player.VerticalHyper(wantedDirection, yDirection);
                }
                else {
                    player.VerticalHyper(1, yDirection);
                }
                return true;
            }
            if (CeilingTechMechanism.RightWallGraceTimer > 0f) {
                player.VerticalHyper(-1, yDirection);
                return true;
            }
        }
        return false;
    }

    private static void DashBeginDontLoseVertSpeed(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldloc_1, ins => ins.OpCode == OpCodes.Ldloc_3, ins => ins.MatchStfld<Player>(nameof(Player.Speed)))) {
            cursor.Index += 3;
            cursor.MoveAfterLabels();
            cursor.Emit(OpCodes.Ldloc_1);
            cursor.EmitDelegate(TryDontLoseVertSpeed);
        }
        else {
            success = false;
        }
        "Player.DashCoroutine".LogHookData("Dash Begin No Vertical Speed Loss", success);
    }

    private static void TryDontLoseVertSpeed(Player player) {
        if (DashBeginDontLoseVerticalSpeed && Math.Sign(player.beforeDashSpeed.Y) == Math.Sign(player.Speed.Y) && Math.Abs(player.beforeDashSpeed.Y) > Math.Abs(player.Speed.Y)) {
            player.Speed.Y = player.beforeDashSpeed.Y;
        }
    }

    private static void ProtectVarJumpTimer(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.MatchLdfld<Player>(nameof(Player.varJumpTimer)), ins => ins.MatchLdcR4(0.15f), ins => ins.OpCode == OpCodes.Bge_Un_S)) {
            ILLabel label = (ILLabel)cursor.Next.Next.Next.Next.Operand;
            cursor.MoveAfterLabels();
            cursor.EmitDelegate(CheckProtectVarJumpTimer);
            cursor.Emit(OpCodes.Brtrue, label);
        }
        else {
            success = false;
        }
        "Player.OnCollideV".LogHookData("Protect Var Jump Timer", success);
    }

    private static bool CheckProtectVarJumpTimer() {
        return CeilingTechMechanism.ProtectVarJumpTimer > 0f;
    }
}