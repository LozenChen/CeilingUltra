namespace Celeste.Mod.CeilingUltra.Module;

[SettingName("CeilingUltra")]
public class CeilingUltraSettings : EverestModuleSettings {

    public static CeilingUltraSettings Instance { get; private set; }

    public CeilingUltraSettings() {
        Instance = this;
    }

    internal void OnLoadSettings() {
    }

    public bool Enabled { get; set; } = true;

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

    [Load]

    private static void Load() {
        ClearAllOverride();
        On.Celeste.Level.LoadLevel += Level_LoadLevel;
    }


    [Unload]
    private static void Unload() {
        On.Celeste.Level.LoadLevel -= Level_LoadLevel;
    }


    private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
        if (self.Session.FirstLevel) {
            ClearAllOverride();
        }
        orig(self, playerIntro, isFromLoader);
    }

    public static void ClearAllOverride() {
        OverrideMainEnabled = OverrideCeilingTech = OverrideCeilingRefill = OverrideVerticalTech = OverrideWallRefill = null;
    }

    public static bool? OverrideMainEnabled;

    public static bool MainEnabled => OverrideMainEnabled.GetValueOrDefault(ceilingUltraSetting.Enabled);

    // CeilingTech

    public static bool? OverrideCeilingTech;
    public static bool CeilingUltraEnabled => MainEnabled && OverrideCeilingTech.GetValueOrDefault(ceilingUltraSetting.CeilingUltraEnabled);

    public static bool CeilingJumpEnabled => MainEnabled && OverrideCeilingTech.GetValueOrDefault(ceilingUltraSetting.CeilingJumpEnabled);

    public static bool CeilingHyperEnabled => MainEnabled && OverrideCeilingTech.GetValueOrDefault(ceilingUltraSetting.CeilingHyperEnabled);

    public static bool UpdiagDashEndNoHorizontalSpeedLoss => MainEnabled && OverrideCeilingTech.GetValueOrDefault(ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss);

    // CeilingRefill

    public static bool? OverrideCeilingRefill;
    public static bool CeilingRefillStamina => MainEnabled && OverrideCeilingRefill.GetValueOrDefault(ceilingUltraSetting.CeilingRefillStamina);

    public static bool CeilingRefillDash => MainEnabled && OverrideCeilingRefill.GetValueOrDefault(ceilingUltraSetting.CeilingRefillDash);

    // WallRefill

    public static bool? OverrideWallRefill;
    public static bool WallRefillStamina => MainEnabled && OverrideWallRefill.GetValueOrDefault(ceilingUltraSetting.WallRefillStamina);

    public static bool WallRefillDash => MainEnabled && OverrideWallRefill.GetValueOrDefault(ceilingUltraSetting.WallRefillDash);

    // VerticallTech

    public static bool? OverrideVerticalTech;
    public static bool UpdiagDashEndNoVerticalSpeedLoss => MainEnabled && OverrideVerticalTech.GetValueOrDefault(ceilingUltraSetting.UpdiagDashEndNoVerticalSpeedLoss);

    public static bool VerticalUltraEnabled => MainEnabled && OverrideVerticalTech.GetValueOrDefault(ceilingUltraSetting.VerticalUltraEnabled);

    public static bool VerticalHyperEnabled => MainEnabled && OverrideVerticalTech.GetValueOrDefault(ceilingUltraSetting.VerticalHyperEnabled);

    public static bool DashBeginNoVerticalSpeedLoss => MainEnabled && OverrideVerticalTech.GetValueOrDefault(ceilingUltraSetting.DashBeginNoVerticalSpeedLoss);
}