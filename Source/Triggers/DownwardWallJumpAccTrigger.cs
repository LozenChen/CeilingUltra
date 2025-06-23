using Celeste.Mod.CeilingUltra.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/DownwardWallJumpAccelerationTrigger")]
public class DownwardWallJumpAccelerationTrigger : AbstractTrigger {

    public DownwardWallJumpAccelerationTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideDownwardWallJumpAcceleration = Enable;
    }
}