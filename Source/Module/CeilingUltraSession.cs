namespace Celeste.Mod.CeilingUltra.Module;

public class CeilingUltraSession : EverestModuleSession {
    public static CeilingUltraSession Instance => (CeilingUltraSession)CeilingUltraModule.Instance._Session;

    public bool? OverrideMainEnabled;

    public bool? OverrideCeilingTech;

    public bool? OverrideBigInertiaUpdiagDash;

    public bool? OverrideCeilingRefill;

    public bool? OverrideWallRefill;

    public bool? OverrideVerticalTech;

    public bool? OverrideUpwardWallJumpAcceleration;

    public bool? OverrideDownwardWallJumpAcceleration;

    public bool? OverrideGroundTech;
}