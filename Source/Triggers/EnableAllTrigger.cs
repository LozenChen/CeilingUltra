using Celeste.Mod.CeilingUltra.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Entities.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/EnableAllTrigger")]
public class EnableAllTrigger : AbstractTrigger {

    public EnableAllTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        Enable = data.Bool("ModEnable", true);
        WallRefill = data.Bool("WallRefill", true);
        CeilingRefill = data.Bool("CeilingRefill", true);
        CeilingTech = data.Bool("CeilingTech", true);
        BigInertiaUpdiagDash = data.Bool("BigInertiaUpdiagDash", true);
        VerticalTech = data.Bool("VerticalTech", true);
        UpwardWallJumpAcceleration = data.Bool("UpwardWallJumpAcceleration", true);
        DownwardWallJumpAcceleration = data.Bool("DownwardWallJumpAcceleration", true);
        GroundTech = data.Bool("GroundTech", true);
    }

    public bool WallRefill;

    public bool CeilingRefill;

    public bool CeilingTech;

    public bool BigInertiaUpdiagDash;

    public bool VerticalTech;

    public bool UpwardWallJumpAcceleration;

    public bool DownwardWallJumpAcceleration;

    public bool GroundTech;

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideMainEnabled = Enable;
        LevelSettings.OverrideWallRefill = WallRefill;
        LevelSettings.OverrideCeilingRefill = CeilingRefill;
        LevelSettings.OverrideCeilingTech = CeilingTech;
        LevelSettings.OverrideBigInertiaUpdiagDash = BigInertiaUpdiagDash;
        LevelSettings.OverrideVerticalTech = VerticalTech;
        LevelSettings.OverrideUpwardWallJumpAcceleration = UpwardWallJumpAcceleration;
        LevelSettings.OverrideDownwardWallJumpAcceleration = DownwardWallJumpAcceleration;
        LevelSettings.OverrideGroundTech = GroundTech;
    }
}