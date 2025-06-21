using Microsoft.Xna.Framework;
using Celeste.Mod.CeilingUltra.ModInterop;
using Celeste.Mod.CeilingUltra.Module;

namespace Celeste.Mod.CeilingUltra.Gameplay;
internal static class WaterInteraction {

    public static bool CanWaterWaveDash => LevelSettings.WaterWaveDashEnabled;

    public const int AboveWaterSurface = -2;

    public const int BelowWaterSurface = 18;

    // player.Y - waterSurface.Y should be in interval [ AboveWaterSurface, BelowWaterSurface ]
    internal static bool CheckAndApplyWaterWaveDash(Player player) {
        if (CanWaterWaveDash
            && player.CanUnDuck && Input.Jump.Pressed && Math.Abs(player.DashDir.X) > 0.1f
            && player.CollideFirst<Water>(player.Position - Vector2.UnitY * AboveWaterSurface * GravityImports.InvertY) is Water water
            && !player.CollideCheck<Water>(player.Position - Vector2.UnitY * BelowWaterSurface * GravityImports.InvertY)) {
            water.TopSurface.DoRipple(player.Position, 1f);
            WaveDashOnWaterSurface(player);
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
}
