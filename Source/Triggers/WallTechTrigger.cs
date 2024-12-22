using Celeste.Mod.CeilingUltra.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Entities.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/WallTechTrigger", "CeilingUltra/VerticalTechTrigger")]
public class WallTechTrigger : AbstractTrigger {

    public WallTechTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideVerticalTech = Enable;
    }
}