using Celeste.Mod.CeilingUltra.Module;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/WaterSurfaceTechTrigger")]
public class WaterSurfaceTechTrigger : AbstractTrigger {

    public WaterSurfaceTechTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideWaterSurfaceTech = Enable;
    }
}