using Celeste.Mod.CeilingUltra.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Entities.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/EnableAllTrigger")]
public class EnableAllTrigger : AbstractTrigger {

    public EnableAllTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideMainEnabled = Enable;
        LevelSettings.OverrideWallRefill = Enable;
        LevelSettings.OverrideCeilingRefill = Enable;
        LevelSettings.OverrideCeilingTech = Enable;
        LevelSettings.OverrideBigInertiaUpdiagDash = Enable;
        LevelSettings.OverrideVerticalTech = Enable;
        LevelSettings.OverrideUpwardWallJumpAcceleration = Enable;
        LevelSettings.OverrideDownwardWallJumpAcceleration = Enable;
    }
}