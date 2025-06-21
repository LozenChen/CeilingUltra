using Celeste.Mod.CeilingUltra.Module;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.CeilingUltra.Triggers;

public class AbstractTrigger : Trigger {
    public bool OneUse;

    public bool Enable;

    public AbstractTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        OneUse = data.Bool("OneUse", true);
        Enable = data.Bool("Enable", true);
    }

    public override void OnEnter(Player player) {
        LevelSettings.OverrideMainEnabled = true;
        base.OnEnter(player);
        if (OneUse) {
            RemoveSelf();
        }
        Logger.Log("CeilingUltra", $"{this.GetType().Name} triggered");
    }
}