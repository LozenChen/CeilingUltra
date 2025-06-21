using Celeste.Mod.CeilingUltra.Module;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace Celeste.Mod.CeilingUltra.Triggers;

[Tracked]
[CustomEntity("CeilingUltra/BigInertiaUpdiagDashTrigger")]
public class BigInertiaUpdiagDashTrigger : AbstractTrigger {

    public BigInertiaUpdiagDashTrigger(EntityData data, Vector2 offset) : base(data, offset) {
    }

    public override void OnEnter(Player player) {
        base.OnEnter(player);
        LevelSettings.OverrideBigInertiaUpdiagDash = Enable;
    }
}