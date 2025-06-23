using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.CeilingUltra.ModInterop;
using Celeste.Mod.CeilingUltra.Module;
using Celeste.Mod.CeilingUltra.Utils;
using CelesteInput = Celeste.Input;

namespace Celeste.Mod.CeilingUltra.Gameplay;
internal static class WaterInteraction {
    public static bool CanWaterWaveDash => LevelSettings.WaterWaveDashEnabled;

    public static bool CanWaterCeilingHyper => LevelSettings.WaterCeilingHyperEnabled;

    public static bool CanWaterBottomSurfaceJump => LevelSettings.WaterBottomSurfaceJumpEnabled;

    public static Vector2 DetectLeniency_TopSurface = new Vector2(0f, 3f);

    public static Vector2 DetectLeniency_BottomSurface = new Vector2(0f, -5f); // ensure when demo at the bottom surface, can still hyper
    
    public const int AirSearchDistance_StSwim = 12;
    
    public const int AirSearchDistance_StNormal = 10;
    
    public const int AirSearchDistance_StDash = 20; // how much deep can player be inside water

    internal static bool TryCeilingJumpOutOfWater(Player player) {
        if (CanWaterBottomSurfaceJump && CelesteInput.MoveY > 0f && FindSafeWaterSurface(player, top: false, AirSearchDistance_StSwim) is not null) {
            player.CeilingJump(particles: true, playSfx: true, checkDownPress: false);
            return true;
        }
        return false;
    }
    internal static void TryCeilingJumpOnWaterSurface(Player player) {
        if (CanWaterBottomSurfaceJump && FindSafeWaterSurface(player, top: false, AirSearchDistance_StNormal) is { } water) {
            DoSurfaceRipple(water, player.Position, top: false);
            player.CeilingJump(particles: true, playSfx: true, checkDownPress: false);
        }
    }
    internal static bool CheckAndApplyWaterWaveDash(Player player) {
        if (!(CelesteInput.Jump.Pressed && Math.Abs(player.DashDir.X) > 0.1f)) {
            return false;
        }

        if (CanWaterWaveDash && player.CanUnDuck && FindSafeWaterSurface(player, top: true, AirSearchDistance_StDash) is { } water) {
            DoSurfaceRipple(water, player.Position, top: true);
            WaterWaveDash(player);
            return true;
            // make it return to StNormal
        }
        else if (CanWaterCeilingHyper
            && FindSafeWaterSurface(player, top: false, AirSearchDistance_StDash) is { } water2
            && player.TryCeilingUnduck(out bool wasDuck, (int)player.Facing)
            ) {
            DoSurfaceRipple(water2, player.Position, top: false);
            WaterCeilingHyper(player, wasDuck || player.DashDir.Y < 0f);
            return true;
            // make it return to StNormal
        }

        return false;
    }

    private static void WaterWaveDash(Player player) {
        if (player.DashDir.Y > 0f) {
            player.Ducking = true;
        }
        player.SuperJump();
        player.DashDir.Y = 0f;
        if (!player.Inventory.NoRefills && player.dashRefillCooldownTimer <= 0f) {
            player.RefillDash(); // in case buffer jump on water surface and can't get refill
        }

        /* this includes:
         * 1. down-diag dash into hyper
         * 2. horizontal dash into super / hyper
         * 3. up-diag normal dash into super
         * 4. up-diag demo dash into hyper
         * and their reverse versions
         */
    }

    private static void WaterCeilingHyper(Player player, bool wasDuck) {
        player.CeilingHyper(wasDuck);
        player.DashDir.Y = 0f;
        if (!player.Inventory.NoRefills && player.dashRefillCooldownTimer <= 0f) { // we don't check CeilingRefillDash ... here the concept should be WaterRefillDash
            player.RefillDash(); // in case buffer jump on water surface and can't get refill
        }
    }

    private static Entity FindSafeWaterSurface(Player player, bool top, int airSearchDistance) {
        // the AirCheck assumes there is only one type of water entity nearby

        Vector2 waterAt = player.Position + (top ? DetectLeniency_TopSurface : DetectLeniency_BottomSurface) * GravityImports.InvertY;

        if (player.CollideFirst<Water>(waterAt) is Water water) {
            if (IsSafeWater(water) && AirCheck(player, water, player.Scene.Tracker.GetEntities<Water>(), top, airSearchDistance)){
                return water;
            }
            return null;
        }

        if (XaphanLiquid is not null
            && player.Scene.Tracker.Entities.TryGetValue(XaphanLiquid, out List<Entity> liquids)
            && liquids?.Where(
                    liquid => liquid.GetFieldValue<string>("liquidType") == "water"
                           && liquid.GetFieldValue<bool>("canSwim")
                )?.ToList() is List<Entity> safeWaters
            && safeWaters.IsNotNullOrEmpty()) {
            if (Collide.First(player, safeWaters, waterAt) is { } liquid) {
                if (AirCheck(player, liquid, liquids, top, airSearchDistance)) {
                    return liquid;
                }
                return null;
            }
        }

        if (OmniZipWater is not null && player.Scene.Tracker.Entities.TryGetValue(OmniZipWater, out List<Entity> zipwaters) && zipwaters.IsNotNullOrEmpty()) {
            if (Collide.First(player, zipwaters, waterAt) is { } zipwater) {
                if (AirCheck(player, zipwater, zipwaters, top, airSearchDistance)) {
                    return zipwater;
                }
                return null;
            }
        }

        return null;

        static bool AirCheck(Player player, Entity water, List<Entity> otherWater, bool top, int airSearchDistance) {
            float y = player.Position.Y;
            float height = player.Collider.Height;
            bool success = false;
            if (top ^ GravityImports.IsPlayerInverted) { // check upwards
                int dist = -(int)(water.Top - player.Bottom);
                if (dist <= airSearchDistance) {
                    if (dist > 0) {
                        // check if we move player up by dist, then whether there is solid on this way
                        player.Position.Y -= dist;
                        player.Collider.Height += dist;
                    }
                    success = true;
                }
            }
            else {
                int dist = (int)(water.Bottom - player.Top);
                if (dist <= airSearchDistance) {
                    if (dist > 0) {
                        player.Collider.Height += dist;
                    }
                    success = true;
                }
            }
            if (success) {
                water.Collidable = false;
                success = !Collide.Check(player, otherWater) && !player.CollideCheck<Solid>(); // can't hyper when there's no enough space above/below water surface
                water.Collidable = true;
            }
            player.Position.Y = y;
            player.Collider.Height = height;
            return success;
        }
    }

    private static void DoSurfaceRipple(Entity entity, Vector2 position, bool top) {
        if (entity is Water water){
            if (top ^ GravityImports.IsPlayerInverted) {
                water.TopSurface?.DoRipple(position, 1f);
            }
            else {
                water.BottomSurface?.DoRipple(position, 1f);
            }
        }
    }

    [Initialize]
    private static void Initialize() {
        XaphanLiquid = ModUtils.GetType("XaphanHelper", "Celeste.Mod.XaphanHelper.Entities.Liquid");
        OmniZipWater = ModUtils.GetType("ChroniaHelper", "ChroniaHelper.Entities.OmniZipWater");

        AddCheck("auspicioushelper", "Celeste.Mod.auspicioushelper.DieWater", _ => false);
        AddCheck("JackalHelper", "Celeste.Mod.JackalHelper.Entities.DeadlyWater", _ => false);
        AddCheck("PandorasBox", "Celeste.Mod.PandorasBox.ColoredWater", water => water.GetFieldValue<bool>("CanJumpOnSurface"));
        AddCheck("LuckyHelper", "LuckyHelper.Entities.CustomWater", water => !water.GetFieldValue<bool>("KillPlayer") || water.GetFieldValue<float>("KillPlayerDelay") > 0f);
        // Celeste.Mod.ScuffedHelper.FourSurfaceWater is safe

        OnlyVanillaWater = SafeJumpableWaterChecks.IsNullOrEmpty();
    }

    private static Type XaphanLiquid;

    private static Type OmniZipWater;

    private static bool OnlyVanillaWater;

    private static readonly Dictionary<Type, Func<Water, bool>> SafeJumpableWaterChecks = new();

    private static void AddCheck(string mod, string typeName, Func<Water, bool> check) {
        if (ModUtils.GetType(mod, typeName) is { } type) {
            SafeJumpableWaterChecks[type] = check;
        }
    }

    private static bool IsSafeWater(Water water) {
        if (OnlyVanillaWater) {
            return true;
        }
        if (SafeJumpableWaterChecks.TryGetValue(water.GetType(), out Func<Water, bool> check)) {
            return check(water);
        }
        return true;
    }
}
