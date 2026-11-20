namespace Celeste.Mod.CeilingUltra.Module;

public class CeilingUltraSession : EverestModuleSession {
    public static CeilingUltraSession Instance => (CeilingUltraSession)CeilingUltraModule.Instance._Session;

    public bool? OverrideMainEnabled;
    public bool? OverrideCeilingUltraEnabled;
    public bool? OverrideCeilingRefillStamina;
    public bool? OverrideCeilingRefillDash;
    public bool? OverrideCeilingJumpEnabled;
    public bool? OverrideCeilingHyperEnabled;
    public bool? OverrideUpdiagDashEndNoHorizontalSpeedLoss;
    public bool? OverrideWallRefillStamina;
    public bool? OverrideWallRefillDash;
    public bool? OverrideVerticalUltraEnabled;
    public bool? OverrideVerticalHyperEnabled;
    public bool? OverrideDashBeginNoVerticalSpeedLoss;
    public bool? OverrideUpdiagDashEndNoVerticalSpeedLoss;
    public bool? OverrideUpwardWallJumpAcceleration;
    public bool? OverrideDownwardWallJumpAcceleration;
    public bool? OverrideWaterWaveDashEnabled;
    public bool? OverrideWaterCeilingHyperEnabled;
    public bool? OverrideWaterBottomSurfaceJumpEnabled;
    public bool? OverrideGroundJumpEnabled;
    public bool? OverrideGroundHyperEnabled;
    public bool? OverrideGroundUltraEnabled;
    public bool? OverrideQoLBufferVerticalHyper;
    public bool? OverrideQoLBufferVerticalUltra;
    public bool? OverrideQoLRefillDashOnWallJump;
    public bool? OverrideQoLBufferCeilingUltra;
    public bool? OverrideHorizontalUltraIntoVerticalUltra;
    public bool? OverrideVerticalUltraIntoHorizontalUltra;

    public bool MapConfigKnown;
}
