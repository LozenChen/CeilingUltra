using Celeste.Mod.CeilingUltra.Module;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/GroundTechTrigger")]
public class GroundTechTrigger : AbstractTrigger {

    public GroundTechTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideGroundTech = Enable;
    }
}