using System.Diagnostics.CodeAnalysis;
using Celeste.Mod.CeilingUltra.Entities;

namespace Celeste.Mod.CeilingUltra.Module;

internal sealed class MapConfigValues {

    public bool? Enabled;
    public bool? CeilingRefillStamina;
    public bool? CeilingRefillDash;
    public bool? CeilingJumpEnabled;
    public bool? CeilingHyperEnabled;
    public bool? CeilingUltraEnabled;
    public bool? UpdiagDashEndNoHorizontalSpeedLoss;
    public bool? WallRefillStamina;
    public bool? WallRefillDash;
    public bool? VerticalHyperEnabled;
    public bool? VerticalUltraEnabled;
    public bool? DashBeginNoVerticalSpeedLoss;
    public bool? UpdiagDashEndNoVerticalSpeedLoss;
    public bool? UpwardWallJumpAcceleration;
    public bool? DownwardWallJumpAcceleration;
    public bool? WaterWaveDashEnabled;
    public bool? WaterCeilingHyperEnabled;
    public bool? WaterBottomSurfaceJumpEnabled;
    public bool? GroundJumpEnabled;
    public bool? GroundHyperEnabled;
    public bool? GroundUltraEnabled;
    public bool? QoLBufferVerticalHyper;
    public bool? QoLBufferVerticalUltra;
    public bool? QoLRefillDashOnWallJump;
    public bool? QoLBufferCeilingUltra;

    // 这两个实际上 Loenn 里我没提供对应的选项. 但你要是直接手写, ... 也可以.
    public bool? HorizontalUltraIntoVerticalUltra;
    public bool? VerticalUltraIntoHorizontalUltra;

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
                values = From(data);
                return true;
            }
        }
        return false;
    }

    // Both placements have ModEnable, so use WallRefill (Normal only) to tell them apart.
    private static MapConfigValues From(EntityData data) {
        return data.Has("WallRefill") ? FromNormal(data) : FromAdvanced(data);
    }

    private static MapConfigValues FromAdvanced(EntityData data) {
        return new MapConfigValues {
            Enabled = data.Bool("ModEnable", true),
            CeilingRefillStamina = data.Bool("CeilingRefillStamina", true),
            CeilingRefillDash = data.Bool("CeilingRefillDash", true),
            CeilingJumpEnabled = data.Bool("CeilingJumpEnabled", true),
            CeilingHyperEnabled = data.Bool("CeilingHyperEnabled", true),
            CeilingUltraEnabled = data.Bool("CeilingUltraEnabled", true),
            UpdiagDashEndNoHorizontalSpeedLoss = data.Bool("UpdiagDashEndNoHorizontalSpeedLoss", true),
            WallRefillStamina = data.Bool("WallRefillStamina", true),
            WallRefillDash = data.Bool("WallRefillDash", true),
            VerticalHyperEnabled = data.Bool("VerticalHyperEnabled", true),
            VerticalUltraEnabled = data.Bool("VerticalUltraEnabled", true),
            DashBeginNoVerticalSpeedLoss = data.Bool("DashBeginNoVerticalSpeedLoss", true),
            UpdiagDashEndNoVerticalSpeedLoss = data.Bool("UpdiagDashEndNoVerticalSpeedLoss", true),
            UpwardWallJumpAcceleration = data.Bool("UpwardWallJumpAcceleration", true),
            DownwardWallJumpAcceleration = data.Bool("DownwardWallJumpAcceleration", true),
            WaterWaveDashEnabled = data.Bool("WaterWaveDashEnabled", true),
            WaterCeilingHyperEnabled = data.Bool("WaterCeilingHyperEnabled", true),
            WaterBottomSurfaceJumpEnabled = data.Bool("WaterBottomSurfaceJumpEnabled", true),
            GroundJumpEnabled = data.Bool("GroundJumpEnabled", true),
            GroundHyperEnabled = data.Bool("GroundHyperEnabled", true),
            GroundUltraEnabled = data.Bool("GroundUltraEnabled", true),
            QoLBufferVerticalHyper = data.Bool("QoLBufferVerticalHyper", true),
            QoLBufferVerticalUltra = data.Bool("QoLBufferVerticalUltra", true),
            QoLRefillDashOnWallJump = data.Bool("QoLRefillDashOnWallJump", true),
            QoLBufferCeilingUltra = data.Bool("QoLBufferCeilingUltra", true),
            HorizontalUltraIntoVerticalUltra = data.Bool("HorizontalUltraIntoVerticalUltra", true),
            VerticalUltraIntoHorizontalUltra = data.Bool("VerticalUltraIntoHorizontalUltra", true),
        };
    }

    private static MapConfigValues FromNormal(EntityData data) {
        bool wallRefill = data.Bool("WallRefill", true);
        bool ceilingRefill = data.Bool("CeilingRefill", true);
        bool ceilingTech = data.Bool("CeilingTech", true);
        bool wallTech = data.Bool("WallTech", true);
        bool groundTech = data.Bool("GroundTech", true);
        bool qol = data.Bool("QoL", true);
        bool waterSurfaceTech = data.Bool("WaterSurfaceTech", true);

        return new MapConfigValues {
            Enabled = data.Bool("ModEnable", true),
            CeilingRefillStamina = ceilingRefill,
            CeilingRefillDash = ceilingRefill,
            CeilingJumpEnabled = ceilingTech,
            CeilingHyperEnabled = ceilingTech,
            CeilingUltraEnabled = ceilingTech,
            UpdiagDashEndNoHorizontalSpeedLoss = data.Bool("BigInertiaUpdiagDash", true),
            WallRefillStamina = wallRefill,
            WallRefillDash = wallRefill,
            VerticalHyperEnabled = wallTech,
            VerticalUltraEnabled = wallTech,
            DashBeginNoVerticalSpeedLoss = wallTech,
            UpdiagDashEndNoVerticalSpeedLoss = wallTech,
            UpwardWallJumpAcceleration = data.Bool("UpwardWallJumpAcceleration", true),
            DownwardWallJumpAcceleration = data.Bool("DownwardWallJumpAcceleration", true),
            WaterWaveDashEnabled = waterSurfaceTech,
            WaterCeilingHyperEnabled = waterSurfaceTech,
            WaterBottomSurfaceJumpEnabled = waterSurfaceTech,
            GroundJumpEnabled = groundTech,
            GroundHyperEnabled = groundTech,
            GroundUltraEnabled = groundTech,
            QoLBufferVerticalHyper = qol,
            QoLBufferVerticalUltra = qol,
            QoLRefillDashOnWallJump = qol,
            QoLBufferCeilingUltra = qol,
        };
    }

    public void ApplyTo(CeilingUltraSession session) {
        session.OverrideMainEnabled = Enabled;
        session.OverrideCeilingRefillStamina = CeilingRefillStamina;
        session.OverrideCeilingRefillDash = CeilingRefillDash;
        session.OverrideCeilingJumpEnabled = CeilingJumpEnabled;
        session.OverrideCeilingHyperEnabled = CeilingHyperEnabled;
        session.OverrideCeilingUltraEnabled = CeilingUltraEnabled;
        session.OverrideUpdiagDashEndNoHorizontalSpeedLoss = UpdiagDashEndNoHorizontalSpeedLoss;
        session.OverrideWallRefillStamina = WallRefillStamina;
        session.OverrideWallRefillDash = WallRefillDash;
        session.OverrideVerticalHyperEnabled = VerticalHyperEnabled;
        session.OverrideVerticalUltraEnabled = VerticalUltraEnabled;
        session.OverrideDashBeginNoVerticalSpeedLoss = DashBeginNoVerticalSpeedLoss;
        session.OverrideUpdiagDashEndNoVerticalSpeedLoss = UpdiagDashEndNoVerticalSpeedLoss;
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
        session.OverrideHorizontalUltraIntoVerticalUltra = HorizontalUltraIntoVerticalUltra;
        session.OverrideVerticalUltraIntoHorizontalUltra = VerticalUltraIntoHorizontalUltra;
    }
}
