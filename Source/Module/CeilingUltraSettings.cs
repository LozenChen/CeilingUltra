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

    private static CeilingUltraSession? Session => CeilingUltraSession.Instance;

    [Monocle.Command("ceiling_ultra_cheat", "clear all map settings so you can edit them yourself.")]
    private static void Command_ClearAllOverride() {
        ClearAllOverride();
    }

    public static void ClearAllOverride() {
        OverrideMainEnabled = OverrideCeilingUltraEnabled = OverrideCeilingRefillStamina = OverrideCeilingRefillDash = null;
        OverrideCeilingJumpEnabled = OverrideCeilingHyperEnabled = OverrideUpdiagDashEndNoHorizontalSpeedLoss = null;
        OverrideWallRefillStamina = OverrideWallRefillDash = OverrideVerticalUltraEnabled = OverrideVerticalHyperEnabled = null;
        OverrideDashBeginNoVerticalSpeedLoss = OverrideUpdiagDashEndNoVerticalSpeedLoss = null;
        OverrideUpwardWallJumpAcceleration = OverrideDownwardWallJumpAcceleration = null;
        OverrideWaterWaveDashEnabled = OverrideWaterCeilingHyperEnabled = OverrideWaterBottomSurfaceJumpEnabled = null;
        OverrideGroundJumpEnabled = OverrideGroundHyperEnabled = OverrideGroundUltraEnabled = null;
        OverrideQoLBufferVerticalHyper = OverrideQoLBufferVerticalUltra = null;
        OverrideQoLRefillDashOnWallJump = OverrideQoLBufferCeilingUltra = null;
        OverrideHorizontalUltraIntoVerticalUltra = OverrideVerticalUltraIntoHorizontalUltra = null;
    }

    public static bool? OverrideMainEnabled {
        get => Session?.OverrideMainEnabled;
        set {
            if (Session is { } session) {
                session.OverrideMainEnabled = value;
            }
        }
    }

    public static bool MainEnabled => OverrideMainEnabled.GetValueOrDefault(ceilingUltraSetting.Enabled);

    public static bool? OverrideCeilingUltraEnabled {
        get => Session?.OverrideCeilingUltraEnabled;
        set {
            if (Session is { } session) {
                session.OverrideCeilingUltraEnabled = value;
            }
        }
    }

    public static bool CeilingUltraEnabled => MainEnabled && OverrideCeilingUltraEnabled.GetValueOrDefault(ceilingUltraSetting.CeilingUltraEnabled);

    public static bool? OverrideCeilingRefillStamina {
        get => Session?.OverrideCeilingRefillStamina;
        set {
            if (Session is { } session) {
                session.OverrideCeilingRefillStamina = value;
            }
        }
    }

    public static bool CeilingRefillStamina => MainEnabled && OverrideCeilingRefillStamina.GetValueOrDefault(ceilingUltraSetting.CeilingRefillStamina);

    public static bool? OverrideCeilingRefillDash {
        get => Session?.OverrideCeilingRefillDash;
        set {
            if (Session is { } session) {
                session.OverrideCeilingRefillDash = value;
            }
        }
    }

    public static bool CeilingRefillDash => MainEnabled && OverrideCeilingRefillDash.GetValueOrDefault(ceilingUltraSetting.CeilingRefillDash);

    public static bool? OverrideCeilingJumpEnabled {
        get => Session?.OverrideCeilingJumpEnabled;
        set {
            if (Session is { } session) {
                session.OverrideCeilingJumpEnabled = value;
            }
        }
    }

    public static bool CeilingJumpEnabled => MainEnabled && OverrideCeilingJumpEnabled.GetValueOrDefault(ceilingUltraSetting.CeilingJumpEnabled);

    public static bool? OverrideCeilingHyperEnabled {
        get => Session?.OverrideCeilingHyperEnabled;
        set {
            if (Session is { } session) {
                session.OverrideCeilingHyperEnabled = value;
            }
        }
    }

    public static bool CeilingHyperEnabled => MainEnabled && OverrideCeilingHyperEnabled.GetValueOrDefault(ceilingUltraSetting.CeilingHyperEnabled);

    public static bool? OverrideUpdiagDashEndNoHorizontalSpeedLoss {
        get => Session?.OverrideUpdiagDashEndNoHorizontalSpeedLoss;
        set {
            if (Session is { } session) {
                session.OverrideUpdiagDashEndNoHorizontalSpeedLoss = value;
            }
        }
    }

    public static bool UpdiagDashEndNoHorizontalSpeedLoss => MainEnabled && OverrideUpdiagDashEndNoHorizontalSpeedLoss.GetValueOrDefault(ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss);

    public static bool? OverrideWallRefillStamina {
        get => Session?.OverrideWallRefillStamina;
        set {
            if (Session is { } session) {
                session.OverrideWallRefillStamina = value;
            }
        }
    }

    public static bool WallRefillStamina => MainEnabled && OverrideWallRefillStamina.GetValueOrDefault(ceilingUltraSetting.WallRefillStamina);

    public static bool? OverrideWallRefillDash {
        get => Session?.OverrideWallRefillDash;
        set {
            if (Session is { } session) {
                session.OverrideWallRefillDash = value;
            }
        }
    }

    public static bool WallRefillDash => MainEnabled && OverrideWallRefillDash.GetValueOrDefault(ceilingUltraSetting.WallRefillDash);

    public static bool? OverrideVerticalUltraEnabled {
        get => Session?.OverrideVerticalUltraEnabled;
        set {
            if (Session is { } session) {
                session.OverrideVerticalUltraEnabled = value;
            }
        }
    }

    public static bool VerticalUltraEnabled => MainEnabled && OverrideVerticalUltraEnabled.GetValueOrDefault(ceilingUltraSetting.VerticalUltraEnabled);

    public static bool? OverrideVerticalHyperEnabled {
        get => Session?.OverrideVerticalHyperEnabled;
        set {
            if (Session is { } session) {
                session.OverrideVerticalHyperEnabled = value;
            }
        }
    }

    public static bool VerticalHyperEnabled => MainEnabled && OverrideVerticalHyperEnabled.GetValueOrDefault(ceilingUltraSetting.VerticalHyperEnabled);

    public static bool? OverrideDashBeginNoVerticalSpeedLoss {
        get => Session?.OverrideDashBeginNoVerticalSpeedLoss;
        set {
            if (Session is { } session) {
                session.OverrideDashBeginNoVerticalSpeedLoss = value;
            }
        }
    }

    public static bool DashBeginNoVerticalSpeedLoss => MainEnabled && OverrideDashBeginNoVerticalSpeedLoss.GetValueOrDefault(ceilingUltraSetting.DashBeginNoVerticalSpeedLoss);

    public static bool? OverrideUpdiagDashEndNoVerticalSpeedLoss {
        get => Session?.OverrideUpdiagDashEndNoVerticalSpeedLoss;
        set {
            if (Session is { } session) {
                session.OverrideUpdiagDashEndNoVerticalSpeedLoss = value;
            }
        }
    }

    public static bool UpdiagDashEndNoVerticalSpeedLoss => MainEnabled && OverrideUpdiagDashEndNoVerticalSpeedLoss.GetValueOrDefault(ceilingUltraSetting.UpdiagDashEndNoVerticalSpeedLoss);

    public static bool? OverrideHorizontalUltraIntoVerticalUltra {
        get => Session?.OverrideHorizontalUltraIntoVerticalUltra;
        set {
            if (Session is { } session) {
                session.OverrideHorizontalUltraIntoVerticalUltra = value;
            }
        }
    }

    public static bool HorizontalUltraIntoVerticalUltra => MainEnabled && OverrideHorizontalUltraIntoVerticalUltra.GetValueOrDefault(ceilingUltraSetting.HorizontalUltraIntoVerticalUltra);

    public static bool? OverrideVerticalUltraIntoHorizontalUltra {
        get => Session?.OverrideVerticalUltraIntoHorizontalUltra;
        set {
            if (Session is { } session) {
                session.OverrideVerticalUltraIntoHorizontalUltra = value;
            }
        }
    }

    public static bool VerticalUltraIntoHorizontalUltra => MainEnabled && OverrideVerticalUltraIntoHorizontalUltra.GetValueOrDefault(ceilingUltraSetting.VerticalUltraIntoHorizontalUltra);

    public static bool? OverrideUpwardWallJumpAcceleration {
        get => Session?.OverrideUpwardWallJumpAcceleration;
        set {
            if (Session is { } session) {
                session.OverrideUpwardWallJumpAcceleration = value;
            }
        }
    }

    public static bool UpwardWallJumpAcceleration => MainEnabled && OverrideUpwardWallJumpAcceleration.GetValueOrDefault(ceilingUltraSetting.UpwardWallJumpAcceleration);

    public static bool? OverrideDownwardWallJumpAcceleration {
        get => Session?.OverrideDownwardWallJumpAcceleration;
        set {
            if (Session is { } session) {
                session.OverrideDownwardWallJumpAcceleration = value;
            }
        }
    }

    public static bool DownwardWallJumpAcceleration => MainEnabled && OverrideDownwardWallJumpAcceleration.GetValueOrDefault(ceilingUltraSetting.DownwardWallJumpAcceleration);

    public static bool? OverrideWaterWaveDashEnabled {
        get => Session?.OverrideWaterWaveDashEnabled;
        set {
            if (Session is { } session) {
                session.OverrideWaterWaveDashEnabled = value;
            }
        }
    }

    public static bool WaterWaveDashEnabled => MainEnabled && OverrideWaterWaveDashEnabled.GetValueOrDefault(ceilingUltraSetting.WaterWaveDashEnabled);

    public static bool? OverrideWaterCeilingHyperEnabled {
        get => Session?.OverrideWaterCeilingHyperEnabled;
        set {
            if (Session is { } session) {
                session.OverrideWaterCeilingHyperEnabled = value;
            }
        }
    }

    public static bool WaterCeilingHyperEnabled => MainEnabled && OverrideWaterCeilingHyperEnabled.GetValueOrDefault(ceilingUltraSetting.WaterCeilingHyperEnabled);

    public static bool? OverrideWaterBottomSurfaceJumpEnabled {
        get => Session?.OverrideWaterBottomSurfaceJumpEnabled;
        set {
            if (Session is { } session) {
                session.OverrideWaterBottomSurfaceJumpEnabled = value;
            }
        }
    }

    public static bool WaterBottomSurfaceJumpEnabled => MainEnabled && OverrideWaterBottomSurfaceJumpEnabled.GetValueOrDefault(ceilingUltraSetting.WaterBottomSurfaceJumpEnabled);

    public static bool? OverrideGroundJumpEnabled {
        get => Session?.OverrideGroundJumpEnabled;
        set {
            if (Session is { } session) {
                session.OverrideGroundJumpEnabled = value;
            }
        }
    }

    public static bool GroundJumpEnabled => !MainEnabled || OverrideGroundJumpEnabled.GetValueOrDefault(ceilingUltraSetting.GroundJumpEnabled);

    public static bool? OverrideGroundHyperEnabled {
        get => Session?.OverrideGroundHyperEnabled;
        set {
            if (Session is { } session) {
                session.OverrideGroundHyperEnabled = value;
            }
        }
    }

    public static bool GroundHyperEnabled => !MainEnabled || OverrideGroundHyperEnabled.GetValueOrDefault(ceilingUltraSetting.GroundHyperEnabled);

    public static bool? OverrideGroundUltraEnabled {
        get => Session?.OverrideGroundUltraEnabled;
        set {
            if (Session is { } session) {
                session.OverrideGroundUltraEnabled = value;
            }
        }
    }

    public static bool GroundUltraEnabled => !MainEnabled || OverrideGroundUltraEnabled.GetValueOrDefault(ceilingUltraSetting.GroundUltraEnabled);

    public static bool? OverrideQoLBufferVerticalHyper {
        get => Session?.OverrideQoLBufferVerticalHyper;
        set {
            if (Session is { } session) {
                session.OverrideQoLBufferVerticalHyper = value;
            }
        }
    }

    public static bool QoLBufferVerticalHyper => MainEnabled && OverrideQoLBufferVerticalHyper.GetValueOrDefault(ceilingUltraSetting.QoLBufferVerticalHyper);

    public static bool? OverrideQoLBufferVerticalUltra {
        get => Session?.OverrideQoLBufferVerticalUltra;
        set {
            if (Session is { } session) {
                session.OverrideQoLBufferVerticalUltra = value;
            }
        }
    }

    public static bool QoLBufferVerticalUltra => MainEnabled && OverrideQoLBufferVerticalUltra.GetValueOrDefault(ceilingUltraSetting.QoLBufferVerticalUltra);

    public static bool? OverrideQoLRefillDashOnWallJump {
        get => Session?.OverrideQoLRefillDashOnWallJump;
        set {
            if (Session is { } session) {
                session.OverrideQoLRefillDashOnWallJump = value;
            }
        }
    }

    public static bool QoLRefillDashOnWallJump => MainEnabled && OverrideQoLRefillDashOnWallJump.GetValueOrDefault(ceilingUltraSetting.QoLRefillDashOnWallJump);

    public static bool? OverrideQoLBufferCeilingUltra {
        get => Session?.OverrideQoLBufferCeilingUltra;
        set {
            if (Session is { } session) {
                session.OverrideQoLBufferCeilingUltra = value;
            }
        }
    }

    public static bool QoLBufferCeilingUltra => MainEnabled && OverrideQoLBufferCeilingUltra.GetValueOrDefault(ceilingUltraSetting.QoLBufferCeilingUltra);

    public static bool AnyOverride =>
        OverrideMainEnabled.HasValue
        || OverrideCeilingUltraEnabled.HasValue
        || OverrideCeilingRefillStamina.HasValue
        || OverrideCeilingRefillDash.HasValue
        || OverrideCeilingJumpEnabled.HasValue
        || OverrideCeilingHyperEnabled.HasValue
        || OverrideUpdiagDashEndNoHorizontalSpeedLoss.HasValue
        || OverrideWallRefillStamina.HasValue
        || OverrideWallRefillDash.HasValue
        || OverrideVerticalUltraEnabled.HasValue
        || OverrideVerticalHyperEnabled.HasValue
        || OverrideDashBeginNoVerticalSpeedLoss.HasValue
        || OverrideUpdiagDashEndNoVerticalSpeedLoss.HasValue
        || OverrideHorizontalUltraIntoVerticalUltra.HasValue
        || OverrideVerticalUltraIntoHorizontalUltra.HasValue
        || OverrideUpwardWallJumpAcceleration.HasValue
        || OverrideDownwardWallJumpAcceleration.HasValue
        || OverrideWaterWaveDashEnabled.HasValue
        || OverrideWaterCeilingHyperEnabled.HasValue
        || OverrideWaterBottomSurfaceJumpEnabled.HasValue
        || OverrideGroundJumpEnabled.HasValue
        || OverrideGroundHyperEnabled.HasValue
        || OverrideGroundUltraEnabled.HasValue
        || OverrideQoLBufferVerticalHyper.HasValue
        || OverrideQoLBufferVerticalUltra.HasValue
        || OverrideQoLRefillDashOnWallJump.HasValue
        || OverrideQoLBufferCeilingUltra.HasValue;
}
