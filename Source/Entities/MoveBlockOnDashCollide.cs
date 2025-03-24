using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity("CeilingUltra/MoveBlockOnDashCollide")]
[Tracked]
public class MoveBlockOnDashCollide : MoveBlock {

    // we don't need this field, but leave it here so others can find it easily
    public bool ActivateOnDashCollide = true;

    public bool AllowInstantUltra = true; // if ActivateOnDashCollide && AllowInstantUltra, then can activate even it's instant ultra
    public MoveBlockOnDashCollide(EntityData data, Vector2 offset)
        : base(data, offset) {
        ActivateOnDashCollide = data.Bool("ActivateOnDashCollide", true);
        if (ActivateOnDashCollide) {
            OnDashCollide = ActivateOnDash;
        }
    }

    public DashCollisionResults ActivateOnDash(Player player, Vector2 direction) {
        triggered = true;
        return DashCollisionResults.NormalCollision;
    }
}
