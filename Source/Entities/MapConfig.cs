using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity(Name)]
public class MapConfig : Entity {
    public const string Name = "CeilingUltra/CeilingUltraMapConfig";

    public MapConfig(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Visible = false;
        Collidable = false;
        Active = false;
    }

    // all necessary stuff is in data, and is read by MapConfigValues.TryRead
}
