using Celeste.Mod.CeilingUltra.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Entities.CeilingUltra.Triggers;

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