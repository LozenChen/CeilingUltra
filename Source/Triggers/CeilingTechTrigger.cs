using Celeste.Mod.CeilingUltra.Module;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/CeilingTechTrigger")]
public class CeilingTechTrigger : AbstractTrigger {

    public CeilingTechTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideCeilingTech = Enable;
    }
}