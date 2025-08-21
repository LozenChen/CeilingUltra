using Monocle;
using Celeste.Mod.CeilingUltra.Utils;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Celeste.Mod.CeilingUltra.ModInterop;

namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class LiftBoostHelper {

    // just like wall jump, which gives player liftboost when player does not have liftspeed and the nearby block has
    // we set player's liftspeed when Ceiling/Vertical actions occur
    // actions include: jump / hyper / ultra

    // support positive liftboost (from extended variants)

    // it's hard to decide when we should apply the liftboost in opposite direction
    // i think liftboost should not point towards solids, so ceiling jump and ceiling hyper should not accept opposite liftboost
    // upward / downward jump is some kind of ... to leave the wall strongly, and btw it's intended to be a boost, so it should not
    // vertical hyper, this touches the wall for a long time, and it seams reasonable to have a opposite liftboost

    public static bool LiftBoostFromFourSides = false;

    public static void OnCeilingJump(Player player) {
        player.GetLiftSpeedFromCeiling();
        player.Speed.X += player.LiftBoost.X;
        if (player.LiftBoost.Y > 0f) {
            player.Speed.Y += player.LiftBoost.Y;
        }
    }

    public static void OnCeilingHyper(Player player) {
        player.GetLiftSpeedFromCeiling();
        player.Speed.X += player.LiftBoost.X;
        if (player.LiftBoost.Y > 0f) {
            player.Speed.Y += player.LiftBoost.Y;
        }
    }

    public static void OnUpwardJump(Player player) {
        if (player.LiftBoost.Y < 0f) {
            player.Speed.Y += player.LiftBoost.Y;
        }
    }

    public static void OnDownwardJump(Player player) {
        if (player.LiftBoost.Y > 0f) {
            player.Speed.Y += player.LiftBoost.Y;
        }
    }

    public static void OnVerticalHyper(Player player, int xDir) {
        player.GetLiftSpeedFromHorizontal(xDir);
        player.Speed += player.LiftBoost; // even if that's not vertically in same dir with your speed
    }

    public static void GetLiftSpeedFromHorizontal(this Player player, int xDir) {
        player.GetLiftSpeed(-xDir * Vector2.UnitX, xDir, 0);
    }

    public static void GetLiftSpeedFromHorizontal(this Player player, Platform platform, int xDir) {
        player.GetLiftSpeed(platform, xDir, yDir: 0);
    }
    public static void GetLiftSpeedFromCeiling(this Player player) {
        player.GetLiftSpeed(- Vector2.UnitY, xDir: 0, yDir: 1);
        // CanStand already calculates Gravity
    }

    public static void GetLiftSpeedFromCeiling(this Player player, Platform platform) {
        player.GetLiftSpeed(platform, xDir: 0, yDir: 1);
    }


    private static void GetLiftSpeed(this Player player, Platform platform, int xDir = 0, int yDir = 0) {
        if (!LiftBoostFromFourSides) {
            return;
        }
        // dir = 0 means no restriction
        if (platform != null && platform.GetLiftSpeed() is Vector2 liftSpeed) {
            float liftSpeedX = (xDir == 0 || (xDir > 0 && liftSpeed.X > 0) || (xDir < 0 && liftSpeed.X < 0)) ? liftSpeed.X : 0f;
            float liftSpeedY = (yDir == 0 || (yDir > 0 && liftSpeed.Y > 0) || (yDir < 0 && liftSpeed.Y < 0)) ? liftSpeed.Y : 0f;
            player.LiftSpeed = new Vector2(liftSpeedX, liftSpeedY);
        }
    }

    private static void GetLiftSpeed(this Player player, Vector2 from, int xDir = 0, int yDir = 0) {
        if (!LiftBoostFromFourSides) {
            return;
        }
        if (player.CanStand(from, out Entity entity) && entity is Platform platform && platform.GetLiftSpeed() is Vector2 liftSpeed) {
            float liftSpeedX = (xDir == 0 || (xDir > 0 && liftSpeed.X > 0) || (xDir < 0 && liftSpeed.X < 0)) ? liftSpeed.X : 0f;
            float liftSpeedY = (yDir == 0 || (yDir > 0 && liftSpeed.Y > 0) || (yDir < 0 && liftSpeed.Y < 0)) ? liftSpeed.Y : 0f;
            player.LiftSpeed = new Vector2(liftSpeedX, liftSpeedY);
        }
    }

    [Load]
    private static void Load() {
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
        IL.Celeste.Platform.Update += Platform_Update;
    }


    [Unload]
    private static void Unload() {
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
        IL.Celeste.Platform.Update -= Platform_Update;
    }
    private static void Platform_Update(ILContext il) {
        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(ins => ins.MatchLdarg(0), ins => ins.OpCode == OpCodes.Call, ins => ins.MatchStfld<Platform>(nameof(Platform.LiftSpeed)))) {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(RecordLiftSpeed);
        }
    }

    private static Vector2? GetLiftSpeed(this Platform platform) {
        if (LiftSpeedDictionary.TryGetValue(platform, out Vector2 value)) {
            return value;
        }
        return null;
    }

    private static void RecordLiftSpeed(Platform platform) {
        LiftSpeedDictionary[platform] = platform.LiftSpeed;
        // Platform class's baseUpdate will clear liftspeed, we have to record it in order to make it accessible to other entities
    }

    internal static void OnLoadLevel(Level level, Player.IntroTypes introTypes, bool isFromLoader) {
        LiftSpeedDictionary = new();
    }

    internal static void OnPlayerUpdateEnd() {
        LiftSpeedDictionary.Clear();
    }

    [SaveLoad]
    private static Dictionary<Entity, Vector2> LiftSpeedDictionary = new();

}