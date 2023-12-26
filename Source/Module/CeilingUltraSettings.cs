using YamlDotNet.Serialization;

namespace Celeste.Mod.CeilingUltra.Module;

[SettingName("Ceiling Ultra")]
public class CeilingUltraSettings : EverestModuleSettings {

    public static CeilingUltraSettings Instance { get; private set; }

    public CeilingUltraSettings() {
        Instance = this;
    }

    internal void OnLoadSettings() {
    }

    [YamlMember(Alias = "Enabled")]

    public bool enabled = true;

    [YamlIgnore]
    public bool Enabled {
        get => enabled;
        set {
            SetAllSettings(value);
        }
    }

    public void SetAllSettings(bool value) {
        enabled = CeilingUltraEnabled = CeilingRefillStamina = CeilingRefillDash = CeilingJumpEnabled = CeilingHyperEnabled = UpdiagDashEndNoHorizontalSpeedLoss = VerticalHyperEnabled = VerticalUltraEnabled = DashBeginNoVerticalSpeedLoss = UpdiagDashEndNoVerticalSpeedLoss = WallRefillStamina = WallRefillDash = value;
        LevelSettings.ClearAllOverride();
    }

    public bool CeilingUltraEnabled { get; set; } = true;

    public bool CeilingRefillStamina { get; set; } = true;

    public bool CeilingRefillDash { get; set; } = true;

    public bool CeilingJumpEnabled { get; set; } = true;

    public bool CeilingHyperEnabled { get; set; } = true;

    public bool UpdiagDashEndNoHorizontalSpeedLoss { get; set; } = true;

    public bool VerticalUltraEnabled { get; set; } = true;

    public bool VerticalHyperEnabled { get; set; } = true;

    public bool DashBeginNoVerticalSpeedLoss { get; set; } = true;

    public bool UpdiagDashEndNoVerticalSpeedLoss { get; set; } = true;

    public bool WallRefillStamina { get; set; } = true;

    public bool WallRefillDash { get; set; } = true;
}

public static class LevelSettings {

    public static void ClearAllOverride() {
        OverrideMainEnabled = OverrideCeilingTech = OverrideCeilingRefill = OverrideVerticalTech = OverrideWallRefill = OverrideBigInertiaUpdiagDash =  null;
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
}

public class CeilingUltraSession: EverestModuleSession {
    public static CeilingUltraSession Instance => (CeilingUltraSession)CeilingUltraModule.Instance._Session;

    public bool? OverrideMainEnabled;

    public bool? OverrideCeilingTech;

    public bool? OverrideBigInertiaUpdiagDash;


    public bool? OverrideCeilingRefill;

    public bool? OverrideWallRefill;

    public bool? OverrideVerticalTech;
}