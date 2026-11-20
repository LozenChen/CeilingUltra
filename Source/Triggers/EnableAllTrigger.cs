using Celeste.Mod.CeilingUltra.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/EnableAllTrigger")]
public class EnableAllTrigger : AbstractTrigger {

    public EnableAllTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        Enable = data.Bool("ModEnable", true);
        WallRefill = data.Bool("WallRefill", true);
        CeilingRefill = data.Bool("CeilingRefill", true);
        CeilingTech = data.Bool("CeilingTech", true);
        BigInertiaUpdiagDash = data.Bool("BigInertiaUpdiagDash", true);
        WallTech = data.Bool("WallTech", true);
        UpwardWallJumpAcceleration = data.Bool("UpwardWallJumpAcceleration", true);
        DownwardWallJumpAcceleration = data.Bool("DownwardWallJumpAcceleration", true);
        GroundTech = data.Bool("GroundTech", true);
        QoL = data.Bool("QoL", true);
        // for backward compatibility, newly added field should have default value null, unless it's QoL (which defaults to be true)
        WaterSurfaceTech = data.Has("WaterSurfaceTech") ? data.Bool("WaterSurfaceTech", true) : null;
    }

    public bool WallRefill;

    public bool CeilingRefill;

    public bool CeilingTech;

    public bool BigInertiaUpdiagDash;

    public bool WallTech;

    public bool UpwardWallJumpAcceleration;

    public bool DownwardWallJumpAcceleration;

    public bool GroundTech;

    public bool? WaterSurfaceTech;

    public bool QoL;

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideMainEnabled = Enable;
        LevelSettings.OverrideWallRefillStamina = WallRefill;
        LevelSettings.OverrideWallRefillDash = WallRefill;

        LevelSettings.OverrideCeilingRefillStamina = CeilingRefill;
        LevelSettings.OverrideCeilingRefillDash = CeilingRefill;
        LevelSettings.OverrideCeilingJumpEnabled = CeilingTech;
        LevelSettings.OverrideCeilingHyperEnabled = CeilingTech;
        LevelSettings.OverrideCeilingUltraEnabled = CeilingTech;

        LevelSettings.OverrideUpdiagDashEndNoHorizontalSpeedLoss = BigInertiaUpdiagDash;

        LevelSettings.OverrideVerticalUltraEnabled = WallTech;
        LevelSettings.OverrideVerticalHyperEnabled = WallTech;
        LevelSettings.OverrideDashBeginNoVerticalSpeedLoss = WallTech;
        LevelSettings.OverrideUpdiagDashEndNoVerticalSpeedLoss = WallTech;

        LevelSettings.OverrideUpwardWallJumpAcceleration = UpwardWallJumpAcceleration;

        LevelSettings.OverrideDownwardWallJumpAcceleration = DownwardWallJumpAcceleration;

        LevelSettings.OverrideGroundJumpEnabled = GroundTech;
        LevelSettings.OverrideGroundHyperEnabled = GroundTech;
        LevelSettings.OverrideGroundUltraEnabled = GroundTech;

        LevelSettings.OverrideWaterWaveDashEnabled = WaterSurfaceTech;
        LevelSettings.OverrideWaterCeilingHyperEnabled = WaterSurfaceTech;
        LevelSettings.OverrideWaterBottomSurfaceJumpEnabled = WaterSurfaceTech;

        LevelSettings.OverrideQoLBufferVerticalHyper = QoL;
        LevelSettings.OverrideQoLBufferVerticalUltra = QoL;
        LevelSettings.OverrideQoLRefillDashOnWallJump = QoL;
        LevelSettings.OverrideQoLBufferCeilingUltra = QoL;

        Logger.Info("Ceiling Ultra", $"MainEnabled: {Enable}\n" +
            $"WallRefill: {WallRefill}\n" +
            $"CeilingRefill: {CeilingRefill}\n" +
            $"CeilingTech: {CeilingTech}\n" +
            $"UpdiagDash: {BigInertiaUpdiagDash}\n" +
            $"WallTech: {WallTech}\n" +
            $"UpJumpAcc: {UpwardWallJumpAcceleration}\n" +
            $"DownJumpAcc: {DownwardWallJumpAcceleration}\n" +
            $"GroundTech: {GroundTech}\n" +
            $"WaterTech: {WaterSurfaceTech}\n" +
            $"QoL: {QoL}");
    }
}