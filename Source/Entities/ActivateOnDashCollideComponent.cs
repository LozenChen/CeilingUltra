using Microsoft.Xna.Framework;
using Monocle;


namespace Celeste.Mod.CeilingUltra.Entities;

[Tracked(inherited: true)]
internal class ActivateOnDashCollideComponent : Component {

    public Platform holder;
    public ActivateOnDashCollideComponent() : base(false, false) { }

    public override void Added(Entity entity) {
        base.Added(entity);
        holder = entity as Platform;
    }

    public virtual void ActivateOnInstantUltra(Player player, Vector2 dir) {
        holder.OnDashCollide?.Invoke(player, dir);
    }

}
