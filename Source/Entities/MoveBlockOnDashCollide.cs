using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity("CeilingUltra/MoveBlockOnDashCollide")]
[Tracked]
public class MoveBlockOnDashCollide : MoveBlock {

    // we don't need this field, but leave it here so others can find it easily
    public bool ActivateOnDashCollide = true;
    public MoveBlockOnDashCollide(EntityData data, Vector2 offset)
        : base(data, offset) {
        ActivateOnDashCollide = data.Bool("ActivateOnDashCollide", true);
        if (ActivateOnDashCollide) {
            OnDashCollide = ActivateOnDash;
            Add(new ActivateOnDashCollideComponent());
        }
    }

    public DashCollisionResults ActivateOnDash(Player player, Vector2 direction) {
        triggered = true;
        return DashCollisionResults.NormalCollision;
    }
}
