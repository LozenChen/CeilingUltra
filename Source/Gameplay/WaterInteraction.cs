using Microsoft.Xna.Framework;
using Celeste.Mod.CeilingUltra.ModInterop;
using Celeste.Mod.CeilingUltra.Module;

namespace Celeste.Mod.CeilingUltra.Gameplay;
internal static class WaterInteraction {

    // todo : support mod water
    public static bool CanWaterWaveDash => LevelSettings.WaterWaveDashEnabled;

    public const int AboveWaterSurface = -2;

    public const int BelowWaterSurface = 18;

    // player.Y - waterSurface.Y should be in interval [ AboveWaterSurface, BelowWaterSurface ]
    internal static bool CheckAndApplyWaterWaveDash(Player player) {
        if (!(CanWaterWaveDash && Input.Jump.Pressed && Math.Abs(player.DashDir.X) > 0.1f)) {
            return false;
        }

        if (player.CollideFirst<Water>(player.Position - Vector2.UnitY * AboveWaterSurface * GravityImports.InvertY) is Water water
            && !player.CollideCheck<Water>(player.Position - Vector2.UnitY * BelowWaterSurface * GravityImports.InvertY)
            && player.CanUnDuck
            ) {
            water.TopSurface?.DoRipple(player.Position, 1f);
            WaveDashOnWaterSurface(player);
            return true;
            // make it return to StNormal
        }
        else if (CeilingTechMechanism.CeilingHyperEnabled
            && player.CollideFirst<Water>(player.Position + Vector2.UnitY * AboveWaterSurface * GravityImports.InvertY) is Water water2
            && !player.CollideCheck<Water>(player.Position + Vector2.UnitY * BelowWaterSurface * GravityImports.InvertY)
            && player.TryCeilingUnduck(out bool wasDuck, (int)player.Facing)
            ) {
            water2.BottomSurface?.DoRipple(player.Position, 1f); // it may have no BottomSurface
            CeilingWaveDashOnWaterSurface(player, wasDuck || player.DashDir.Y < 0f);
            return true;
            // make it return to StNormal
        }

        return false;
    }

    internal static void WaveDashOnWaterSurface(Player player) {
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

    internal static void CeilingWaveDashOnWaterSurface(Player player, bool wasDuck) {
        player.CeilingHyper(wasDuck);
        player.DashDir.Y = 0f;
        if (!player.Inventory.NoRefills && player.dashRefillCooldownTimer <= 0f) { // we don't check CeilingRefillDash ... here the concept should be WaterRefillDash
            player.RefillDash(); // in case buffer jump on water surface and can't get refill
        }
    }
}
