using Celeste.Mod.CeilingUltra.Utils;
using Microsoft.Xna.Framework;
using Monocle;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;
using CelesteInput = Celeste.Input;

namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class VerticalTechMechanism {

    public static bool VerticalUltraEnabled => ceilingUltraSetting.Enabled && ceilingUltraSetting.VerticalUltraEnabled;

    public static bool VerticalHyperEnabled => ceilingUltraSetting.Enabled && ceilingUltraSetting.VerticalHyperEnabled;

    public static bool DashBeginDontLoseVerticalSpeed => ceilingUltraSetting.Enabled && ceilingUltraSetting.DashBeginNoVerticalSpeedLoss;

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
        using (new DetourContext { Before = new List<string> { "*" }, ID = "Vertical Tech Mechanism" }) {
            typeof(Player).GetMethodInfo("OnCollideH").IlHook(VerticalUltraHookOnCollideH);
            typeof(Player).GetMethodInfo("get_CanUnDuck").IlHook(ModifyCanUnDuck);
            typeof(Player).GetMethodInfo("set_Ducking").IlHook(ModifySetDucking);
            typeof(Player).GetMethodInfo("orig_Update").IlHook(AutoUnsqueeze);
            typeof(Player).GetMethodInfo("DashUpdate").IlHook(VerticalHyperHookDashUpdate);
            typeof(Player).GetMethodInfo("RedDashUpdate").IlHook(VerticalHyperHookDashUpdate);
            typeof(Player).GetMethodInfo("DashCoroutine").GetStateMachineTarget().IlHook(DashBeginDontLoseVertSpeed);
            typeof(Player).GetMethodInfo("OnCollideV").IlHook(ProtectJumpGraceTimer);
        }

        int[] xOffsets = { 0, 1, -1 };
        int count = 0;
        wiggleList = new Vector2[18];
        for (int yOffset = 0; yOffset <= 5; yOffset++) {
            foreach (int xOffset in xOffsets) {
                wiggleList[count] = new Vector2(xOffset, yOffset);
                count++;
            }
        }
    }

    private static Hitbox squeezedHitbox = new Hitbox(6f, 11f, -3f, -11f);

    private static Hitbox squeezedHurtbox = new Hitbox(6f, 9f, -3f, -11f);
    private static Player OnLoadNewPlayer(On.Celeste.Level.orig_LoadNewPlayer orig, Vector2 Position, PlayerSpriteMode spriteMode) {
        Player player = orig(Position, spriteMode);
        squeezedHitbox = new Hitbox(6f, 11f, -3f, -11f);
        squeezedHurtbox = new Hitbox(6f, 9f, -3f, -11f);
        return player;
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
        if (xDirection < 0 && yDirection < 0) {
            // upper-left dash: keep bottom left invariant
            Collider collider = player.Collider;
            Vector2 position = player.Position;
            Vector2 orig = player.Collider.BottomLeft;
            player.Collider = squeezedHitbox;
            offset = orig - player.Collider.BottomLeft; // we didn't use player.BottomLeft, so we can avoid possible float position and related issues. offset will always be integer
            player.Position += offset;
            bool result = !player.CollideCheck<Solid>();
            player.Position = position;
            player.Collider = collider;
            return result;
        }
        if (xDirection < 0 && yDirection > 0) {
            // lower-left dash: keep top left invariant
            Collider collider = player.Collider;
            Vector2 position = player.Position;
            Vector2 orig = player.Collider.TopLeft;
            player.Collider = squeezedHitbox;
            offset = orig - player.Collider.TopLeft;
            player.Position += offset;
            bool result = !player.CollideCheck<Solid>();
            player.Position = position;
            player.Collider = collider;
            return result;
        }
        if (xDirection > 0 && yDirection < 0) {
            // upper-right dash: keep bottom right invariant
            Collider collider = player.Collider;
            Vector2 position = player.Position;
            Vector2 orig = player.Collider.BottomRight;
            player.Collider = squeezedHitbox;
            offset = orig - player.Collider.BottomRight;
            player.Position += offset;
            bool result = !player.CollideCheck<Solid>();
            player.Position = position;
            player.Collider = collider;
            return result;
        }
        if (xDirection > 0 && yDirection > 0) {
            // lower-right dash: keep top right variant
            Collider collider = player.Collider;
            Vector2 position = player.Position;
            Vector2 orig = player.Collider.TopRight;
            player.Collider = squeezedHitbox;
            offset = orig - player.Collider.TopRight;
            player.Position += offset;
            bool result = !player.CollideCheck<Solid>();
            player.Position = position;
            player.Collider = collider;
            return result;
        }
        offset = Vector2.Zero;
        return false;
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
        if (Math.Sign(player.DashDir.X) is { } xSign && xSign != 0 && xSign == Math.Sign(player.Speed.X) && player.DashDir.Y != 0f && player.TrySqueezeHitbox(xSign, player.Speed.Y)) {
            player.wallSpeedRetained = player.Speed.X;
            player.wallSpeedRetentionTimer = 0.06f;
            player.DashDir.Y = Math.Sign(player.DashDir.Y); // wow, this should allow you to super wall jump after an upward vertical ultra (if you manage to unsqueeze during your dash)
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
        cursor.EmitDelegate(CanUnSqueezeInUnDuck);
        cursor.Emit(OpCodes.Ret);
        "Player.get_CanUnDuck".LogHookData("Compatiblity with SqueezedHitbox", true);
    }

    private static bool CanUnSqueezeInUnDuck(this Player player) {
        Collider collider = player.Collider;
        player.Collider = player.normalHitbox;
        bool result = !player.CollideCheck<Solid>() || !player.CollideCheck<Solid>(player.Position + Vector2.UnitX) || !player.CollideCheck<Solid>(player.Position - Vector2.UnitX);
        player.Collider = collider;
        return result;
    }

    private static void UnSqueeze(this Player player, bool duck = false) {
        if (!duck) {
            // in most cases, Ducking = false goes with a CanUnDuck check
            Collider collider = player.Collider;
            Vector2 position = player.Position;
            bool result = false;
            player.Collider = player.normalHitbox;
            if (!player.CollideCheck<Solid>()) {
                result = true;
            }
            else {
                player.Position = position + Vector2.UnitX;
                if (!player.CollideCheck<Solid>()) {
                    result = true;
                }
                else {
                    player.Position = position - Vector2.UnitX;
                    if (!player.CollideCheck<Solid>()) {
                        result = true;
                    }
                }
            }
            if (result) {
                player.hurtbox = player.normalHurtbox;
            }
            else {
                player.Collider = collider;
                player.Position = position;
            }
        }
        else {
            // however, Ducking = true almost has no check
            Collider collider = player.Collider;
            Vector2 position = player.Position;
            bool result = false;
            player.Collider = player.duckHitbox;
            foreach (Vector2 offset in wiggleList) {
                player.Position = position + offset;
                if (!player.CollideCheck<Solid>()) {
                    result = true;
                    break;
                }
            }
            if (result) {
                player.hurtbox = player.duckHurtbox;
            }
            else {
                player.Collider = collider;
                player.Position = position;
            }
        }
    }

    private static Vector2[] wiggleList;

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
            ILLabel label = (ILLabel) cursor.Prev.Operand;
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
        if (player.IsSqueezed() && player.onGround && player.CanUnDuck) {
            player.Ducking = false;
        }
    }

    private static void VerticalHyper(this Player player, int xDirection, int yDirection) {
        // sadly, if you buffer a jump then you will get a wall jump instead of a vertical hyper
        // only hyper, no vertical super jump (which is replaced a super wall jump)
        Input.Jump.ConsumeBuffer();
        if (yDirection > 0) {
             CeilingTechMechanism.NextMaxFall = 320f;
        }
        player.jumpGraceTimer = 0f;
        CeilingTechMechanism.SetExtendedJumpGraceTimer();
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
            CeilingTechMechanism.ProtectJumpGraceTimer = 0.1f; // so we invent this to protect varJumpTimer, this will be enough to go around a 1px corner
        }
        else {
            player.varJumpTimer = 0f;
            player.varJumpSpeed = 0f;
        }

        player.launched = true;
        player.Sprite.Scale = new Vector2(0.6f, 1.4f);
        int index = -1;
        Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(player.CollideAll<Platform>(player.Position - xDirection * Vector2.UnitX, player.temp));
        if (platformByPriority != null) {
            index = platformByPriority.GetLandSoundIndex(player);
        }
        Dust.Burst(xDirection > 0 ? player.CenterLeft : player.CenterRight, xDirection > 0f ? (float) Math.PI : 0f, 4, player.DustParticleFromSurfaceIndex(index));
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
        if (VerticalHyperEnabled && player.IsSqueezed() && CelesteInput.Jump.Pressed && Math.Abs(player.DashDir.X) < 0.1f && player.CanUnSqueezeInUnDuck()) {
            int yDirection = Math.Sign(CelesteInput.MoveY);
            if (yDirection == 0) {
                yDirection = Math.Sign(player.Speed.Y);
            }
            if (yDirection == 0) {
                yDirection = -1;
            }
            int wantedDirection = CelesteInput.MoveX != 0f ? CelesteInput.MoveX : (int)player.Facing; // we dont use player.moveX so forceMoveX is ignored lol
            if (player.CollideCheck<Solid>(player.Position - wantedDirection * Vector2.UnitX)) {
                player.VerticalHyper(wantedDirection, yDirection);
                return true;
            }
            if (player.CollideCheck<Solid>(player.Position + wantedDirection * Vector2.UnitX)) {
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

    private static void ProtectJumpGraceTimer(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        bool success = true;
        if (cursor.TryGotoNext(ins => ins.OpCode == OpCodes.Ldarg_0, ins => ins.MatchLdfld<Player>(nameof(Player.varJumpTimer)), ins => ins.MatchLdcR4(0.15f), ins => ins.OpCode == OpCodes.Bge_Un_S)) {
            ILLabel label = (ILLabel)cursor.Next.Next.Next.Next.Operand;
            cursor.MoveAfterLabels();
            cursor.EmitDelegate(CheckProtectJumpGraceTimer);
            cursor.Emit(OpCodes.Brtrue, label);
        }
        else {
            success = false;
        }
        "Player.OnCollideV".LogHookData("Protect Jump Grace Timer", success);
    }

    private static bool CheckProtectJumpGraceTimer() {
        return CeilingTechMechanism.ProtectJumpGraceTimer > 0f;
    }
}