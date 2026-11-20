using System.Diagnostics.CodeAnalysis;
using Celeste.Mod.CeilingUltra.Entities;

namespace Celeste.Mod.CeilingUltra.Module;


internal sealed class MapConfigValues {

    public bool Enabled = true;
    public bool CeilingUltraEnabled = true;
    public bool CeilingRefillStamina = true;
    public bool CeilingRefillDash = true;
    public bool CeilingJumpEnabled = true;
    public bool CeilingHyperEnabled = true;
    public bool GroundJumpEnabled = true;
    public bool GroundUltraEnabled = true;
    public bool GroundHyperEnabled = true;
    public bool UpdiagDashEndNoHorizontalSpeedLoss = true;
    public bool VerticalUltraEnabled = true;
    public bool VerticalHyperEnabled = true;
    public bool DashBeginNoVerticalSpeedLoss = true;
    public bool UpdiagDashEndNoVerticalSpeedLoss = true;
    public bool WallRefillStamina = true;
    public bool WallRefillDash = true;
    public bool HorizontalUltraIntoVerticalUltra = true;
    public bool VerticalUltraIntoHorizontalUltra = true;
    public bool UpwardWallJumpAcceleration = true;
    public bool DownwardWallJumpAcceleration = true;
    public bool WaterWaveDashEnabled = true;
    public bool WaterCeilingHyperEnabled = true;
    public bool WaterBottomSurfaceJumpEnabled = true;
    public bool QoLBufferVerticalHyper = true;
    public bool QoLBufferVerticalUltra = true;
    public bool QoLRefillDashOnWallJump = true;
    public bool QoLBufferCeilingUltra = true;

    public static bool TryRead(Session session, [NotNullWhen(true)] out MapConfigValues? values) {
        values = null;
        MapData? mapData = session?.MapData;
        if (mapData?.Levels == null) {
            return false;
        }

        foreach (LevelData level in mapData.Levels) {
            if (level?.Entities == null) {
                continue;
            }
            foreach (EntityData data in level.Entities) {
                if (data?.Name != MapConfig.Name) {
                    continue;
                }
                if (values == null) {
                    values = From(data);
                    return true;
                }
            }
        }
        return false;
    }

    private static MapConfigValues From(EntityData data) {
        return new MapConfigValues {
            Enabled = data.Bool("Enabled", true),
            CeilingUltraEnabled = data.Bool("CeilingUltraEnabled", true),
            CeilingRefillStamina = data.Bool("CeilingRefillStamina", true),
            CeilingRefillDash = data.Bool("CeilingRefillDash", true),
            CeilingJumpEnabled = data.Bool("CeilingJumpEnabled", true),
            CeilingHyperEnabled = data.Bool("CeilingHyperEnabled", true),
            GroundJumpEnabled = data.Bool("GroundJumpEnabled", true),
            GroundUltraEnabled = data.Bool("GroundUltraEnabled", true),
            GroundHyperEnabled = data.Bool("GroundHyperEnabled", true),
            UpdiagDashEndNoHorizontalSpeedLoss = data.Bool("UpdiagDashEndNoHorizontalSpeedLoss", true),
            VerticalUltraEnabled = data.Bool("VerticalUltraEnabled", true),
            VerticalHyperEnabled = data.Bool("VerticalHyperEnabled", true),
            DashBeginNoVerticalSpeedLoss = data.Bool("DashBeginNoVerticalSpeedLoss", true),
            UpdiagDashEndNoVerticalSpeedLoss = data.Bool("UpdiagDashEndNoVerticalSpeedLoss", true),
            WallRefillStamina = data.Bool("WallRefillStamina", true),
            WallRefillDash = data.Bool("WallRefillDash", true),
            HorizontalUltraIntoVerticalUltra = data.Bool("HorizontalUltraIntoVerticalUltra", true),
            VerticalUltraIntoHorizontalUltra = data.Bool("VerticalUltraIntoHorizontalUltra", true),
            UpwardWallJumpAcceleration = data.Bool("UpwardWallJumpAcceleration", true),
            DownwardWallJumpAcceleration = data.Bool("DownwardWallJumpAcceleration", true),
            WaterWaveDashEnabled = data.Bool("WaterWaveDashEnabled", true),
            WaterCeilingHyperEnabled = data.Bool("WaterCeilingHyperEnabled", true),
            WaterBottomSurfaceJumpEnabled = data.Bool("WaterBottomSurfaceJumpEnabled", true),
            QoLBufferVerticalHyper = data.Bool("QoLBufferVerticalHyper", true),
            QoLBufferVerticalUltra = data.Bool("QoLBufferVerticalUltra", true),
            QoLRefillDashOnWallJump = data.Bool("QoLRefillDashOnWallJump", true),
            QoLBufferCeilingUltra = data.Bool("QoLBufferCeilingUltra", true),
        };
    }

    public void ApplyTo(CeilingUltraSession session) {
        session.OverrideMainEnabled = Enabled;
        session.OverrideCeilingUltraEnabled = CeilingUltraEnabled;
        session.OverrideCeilingRefillStamina = CeilingRefillStamina;
        session.OverrideCeilingRefillDash = CeilingRefillDash;
        session.OverrideCeilingJumpEnabled = CeilingJumpEnabled;
        session.OverrideCeilingHyperEnabled = CeilingHyperEnabled;
        session.OverrideUpdiagDashEndNoHorizontalSpeedLoss = UpdiagDashEndNoHorizontalSpeedLoss;
        session.OverrideWallRefillStamina = WallRefillStamina;
        session.OverrideWallRefillDash = WallRefillDash;
        session.OverrideVerticalUltraEnabled = VerticalUltraEnabled;
        session.OverrideVerticalHyperEnabled = VerticalHyperEnabled;
        session.OverrideDashBeginNoVerticalSpeedLoss = DashBeginNoVerticalSpeedLoss;
        session.OverrideUpdiagDashEndNoVerticalSpeedLoss = UpdiagDashEndNoVerticalSpeedLoss;
        session.OverrideHorizontalUltraIntoVerticalUltra = HorizontalUltraIntoVerticalUltra;
        session.OverrideVerticalUltraIntoHorizontalUltra = VerticalUltraIntoHorizontalUltra;
        session.OverrideUpwardWallJumpAcceleration = UpwardWallJumpAcceleration;
        session.OverrideDownwardWallJumpAcceleration = DownwardWallJumpAcceleration;
        session.OverrideWaterWaveDashEnabled = WaterWaveDashEnabled;
        session.OverrideWaterCeilingHyperEnabled = WaterCeilingHyperEnabled;
        session.OverrideWaterBottomSurfaceJumpEnabled = WaterBottomSurfaceJumpEnabled;
        session.OverrideGroundJumpEnabled = GroundJumpEnabled;
        session.OverrideGroundHyperEnabled = GroundHyperEnabled;
        session.OverrideGroundUltraEnabled = GroundUltraEnabled;
        session.OverrideQoLBufferVerticalHyper = QoLBufferVerticalHyper;
        session.OverrideQoLBufferVerticalUltra = QoLBufferVerticalUltra;
        session.OverrideQoLRefillDashOnWallJump = QoLRefillDashOnWallJump;
        session.OverrideQoLBufferCeilingUltra = QoLBufferCeilingUltra;
    }
}
