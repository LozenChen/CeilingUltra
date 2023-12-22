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

    public bool UpdiagDashDontLoseHorizontalSpeed { get; set; } = true;
}