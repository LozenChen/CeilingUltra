namespace Celeste.Mod.CeilingUltra.Module;

[SettingName("Ceiling Ultra")]
public class CeilingUltraSettings : EverestModuleSettings {

    public static CeilingUltraSettings Instance { get; private set; }

    public CeilingUltraSettings() {
        Instance = this;
    }

    internal void OnLoadSettings() {
        GroundJumpEnabled = GroundHyperEnabled = GroundUltraEnabled = true;
    }


    public bool Enabled = false;

    // we provide this property for tas set command.
    // if it's not a mod map which use ceiling ultra on its own, please set this to true
    public bool TasEnableAll {
        get => Enabled;
        set {
            Enabled =
                CeilingUltraEnabled = CeilingRefillStamina = CeilingRefillDash = CeilingJumpEnabled = CeilingHyperEnabled =
                UpdiagDashEndNoHorizontalSpeedLoss = VerticalHyperEnabled = VerticalUltraEnabled = DashBeginNoVerticalSpeedLoss =
                UpdiagDashEndNoVerticalSpeedLoss = WallRefillStamina = WallRefillDash = HorizontalUltraIntoVerticalUltra =
                VerticalUltraIntoHorizontalUltra = UpwardWallJumpAcceleration = DownwardWallJumpAcceleration =
                WaterWaveDashEnabled = WaterCeilingHyperEnabled = WaterBottomSurfaceJumpEnabled = value;
            GroundJumpEnabled = GroundHyperEnabled = GroundUltraEnabled = true;
            QoLBufferVerticalHyper = false;
            QoLBufferVerticalUltra = false;
            QoLRefillDashOnWallJump = false;
            QoLBufferCeilingUltra = false;
            LevelSettings.ClearAllOverride();
        }
    }

    public bool CeilingUltraEnabled { get; set; } = true;

    public bool CeilingRefillStamina { get; set; } = true;

    public bool CeilingRefillDash { get; set; } = true;

    public bool CeilingJumpEnabled { get; set; } = true;

    public bool CeilingHyperEnabled { get; set; } = true;

    public bool GroundJumpEnabled { get; set; } = true;

    public bool GroundUltraEnabled { get; set; } = true;

    public bool GroundHyperEnabled { get; set; } = true;

    public bool UpdiagDashEndNoHorizontalSpeedLoss { get; set; } = true;

    public bool VerticalUltraEnabled { get; set; } = true;

    public bool VerticalHyperEnabled { get; set; } = true;

    public bool DashBeginNoVerticalSpeedLoss { get; set; } = true;

    public bool UpdiagDashEndNoVerticalSpeedLoss { get; set; } = true;

    public bool WallRefillStamina { get; set; } = true;

    public bool WallRefillDash { get; set; } = true;

    public bool HorizontalUltraIntoVerticalUltra { get; set; } = true;

    public bool VerticalUltraIntoHorizontalUltra { get; set; } = true;

    public bool UpwardWallJumpAcceleration { get; set; } = true;

    public bool DownwardWallJumpAcceleration { get; set; } = true;

    public bool WaterWaveDashEnabled { get; set; } = true;

    public bool WaterCeilingHyperEnabled { get; set; } = true;

    public bool WaterBottomSurfaceJumpEnabled { get; set; } = true;
    public bool QoLBufferVerticalHyper { get; set; } = true;

    public bool QoLBufferVerticalUltra { get; set; } = true;

    public bool QoLRefillDashOnWallJump { get; set; } = true;

    public bool QoLBufferCeilingUltra { get; set; } = true;
    public bool ShowInPauseMenu { get; set; } = true;
}

public static class LevelSettings {

    [Monocle.Command("ceiling_ultra_cheat", "clear all map settings so you can edit them yourself.")]
    private static void Command_ClearAllOverride() {
        ClearAllOverride();
    }

    public static void ClearAllOverride() {
        OverrideMainEnabled = OverrideCeilingTech = OverrideCeilingRefill = OverrideVerticalTech = OverrideWallRefill = OverrideBigInertiaUpdiagDash = OverrideUpwardWallJumpAcceleration = OverrideDownwardWallJumpAcceleration = OverrideWaterSurfaceTech = OverrideGroundTech = OverrideQoL = null;
    }

    public static bool? OverrideMainEnabled {
        get => CeilingUltraSession.Instance?.OverrideMainEnabled;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideMainEnabled = value;
            }
        }
    }

    public static bool MainEnabled => OverrideMainEnabled.GetValueOrDefault(ceilingUltraSetting.Enabled);

    // CeilingTech

    public static bool? OverrideCeilingTech {
        get => CeilingUltraSession.Instance?.OverrideCeilingTech;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideCeilingTech = value;
            }
        }
    }
    public static bool CeilingUltraEnabled => MainEnabled && OverrideCeilingTech.GetValueOrDefault(ceilingUltraSetting.CeilingUltraEnabled);

    public static bool CeilingJumpEnabled => MainEnabled && OverrideCeilingTech.GetValueOrDefault(ceilingUltraSetting.CeilingJumpEnabled);

    public static bool CeilingHyperEnabled => MainEnabled && OverrideCeilingTech.GetValueOrDefault(ceilingUltraSetting.CeilingHyperEnabled);

    // Big Inertia Updiag Dash End
    // it will affect normal gameplay even if you dont use those new techs, so it should be controlled by a standalone trigger

    public static bool? OverrideBigInertiaUpdiagDash {
        get => CeilingUltraSession.Instance?.OverrideBigInertiaUpdiagDash;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideBigInertiaUpdiagDash = value;
            }
        }
    }

    public static bool UpdiagDashEndNoHorizontalSpeedLoss => MainEnabled && OverrideBigInertiaUpdiagDash.GetValueOrDefault(ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss);

    // CeilingRefill

    public static bool? OverrideCeilingRefill {
        get => CeilingUltraSession.Instance?.OverrideCeilingRefill;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideCeilingRefill = value;
            }
        }
    }
    public static bool CeilingRefillStamina => MainEnabled && OverrideCeilingRefill.GetValueOrDefault(ceilingUltraSetting.CeilingRefillStamina);

    public static bool CeilingRefillDash => MainEnabled && OverrideCeilingRefill.GetValueOrDefault(ceilingUltraSetting.CeilingRefillDash);

    // WallRefill

    public static bool? OverrideWallRefill {
        get => CeilingUltraSession.Instance?.OverrideWallRefill;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideWallRefill = value;
            }
        }
    }
    public static bool WallRefillStamina => MainEnabled && OverrideWallRefill.GetValueOrDefault(ceilingUltraSetting.WallRefillStamina);

    public static bool WallRefillDash => MainEnabled && OverrideWallRefill.GetValueOrDefault(ceilingUltraSetting.WallRefillDash);

    // VerticallTech

    public static bool? OverrideVerticalTech {
        get => CeilingUltraSession.Instance?.OverrideVerticalTech;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideVerticalTech = value;
            }
        }
    }
    public static bool UpdiagDashEndNoVerticalSpeedLoss => MainEnabled && OverrideVerticalTech.GetValueOrDefault(ceilingUltraSetting.UpdiagDashEndNoVerticalSpeedLoss);

    public static bool VerticalUltraEnabled => MainEnabled && OverrideVerticalTech.GetValueOrDefault(ceilingUltraSetting.VerticalUltraEnabled);

    public static bool VerticalHyperEnabled => MainEnabled && OverrideVerticalTech.GetValueOrDefault(ceilingUltraSetting.VerticalHyperEnabled);

    public static bool DashBeginNoVerticalSpeedLoss => MainEnabled && OverrideVerticalTech.GetValueOrDefault(ceilingUltraSetting.DashBeginNoVerticalSpeedLoss);

    // DelayedUltra

    public static bool HorizontalUltraIntoVerticalUltra => MainEnabled && ceilingUltraSetting.HorizontalUltraIntoVerticalUltra;

    public static bool VerticalUltraIntoHorizontalUltra => MainEnabled && ceilingUltraSetting.VerticalUltraIntoHorizontalUltra;

    // WallJumpAcceleration

    public static bool? OverrideUpwardWallJumpAcceleration {
        get => CeilingUltraSession.Instance?.OverrideUpwardWallJumpAcceleration;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideUpwardWallJumpAcceleration = value;
            }
        }
    }
    public static bool UpwardWallJumpAcceleration => MainEnabled && OverrideUpwardWallJumpAcceleration.GetValueOrDefault(ceilingUltraSetting.UpwardWallJumpAcceleration);


    public static bool? OverrideDownwardWallJumpAcceleration {
        get => CeilingUltraSession.Instance?.OverrideDownwardWallJumpAcceleration;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideDownwardWallJumpAcceleration = value;
            }
        }
    }
    public static bool DownwardWallJumpAcceleration => MainEnabled && OverrideDownwardWallJumpAcceleration.GetValueOrDefault(ceilingUltraSetting.DownwardWallJumpAcceleration);


    // WaterSurfaceTech
    public static bool? OverrideWaterSurfaceTech {
        get => CeilingUltraSession.Instance?.OverrideWaterSurfaceTech;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideWaterSurfaceTech = value;
            }
        }
    }

    public static bool WaterWaveDashEnabled => MainEnabled && OverrideWaterSurfaceTech.GetValueOrDefault(ceilingUltraSetting.WaterWaveDashEnabled);

    public static bool WaterCeilingHyperEnabled => MainEnabled && OverrideWaterSurfaceTech.GetValueOrDefault(ceilingUltraSetting.WaterCeilingHyperEnabled);

    public static bool WaterBottomSurfaceJumpEnabled => MainEnabled && OverrideWaterSurfaceTech.GetValueOrDefault(ceilingUltraSetting.WaterBottomSurfaceJumpEnabled);

    // GroundTech

    public static bool? OverrideGroundTech {
        get => CeilingUltraSession.Instance?.OverrideGroundTech;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideGroundTech = value;
            }
        }
    }
    public static bool GroundJumpEnabled => !MainEnabled || OverrideGroundTech.GetValueOrDefault(ceilingUltraSetting.GroundJumpEnabled);

    public static bool GroundHyperEnabled => !MainEnabled || OverrideGroundTech.GetValueOrDefault(ceilingUltraSetting.GroundHyperEnabled);

    public static bool GroundUltraEnabled => !MainEnabled || OverrideGroundTech.GetValueOrDefault(ceilingUltraSetting.GroundUltraEnabled);

    // QoL

    public static bool? OverrideQoL {
        get => CeilingUltraSession.Instance?.OverrideQoL;
        set {
            if (CeilingUltraSession.Instance is { } instance) {
                instance.OverrideQoL = value;
            }
        }
    }
    public static bool QoLBufferVerticalHyper => MainEnabled && OverrideQoL.GetValueOrDefault(ceilingUltraSetting.QoLBufferVerticalHyper);

    public static bool QoLBufferVerticalUltra => MainEnabled && OverrideQoL.GetValueOrDefault(ceilingUltraSetting.QoLBufferVerticalUltra);

    public static bool QoLRefillDashOnWallJump => MainEnabled && OverrideQoL.GetValueOrDefault(ceilingUltraSetting.QoLRefillDashOnWallJump);

    public static bool QoLBufferCeilingUltra => MainEnabled && OverrideQoL.GetValueOrDefault(ceilingUltraSetting.QoLBufferCeilingUltra);
}
