using Celeste.Mod.CeilingUltra.ModInterop;
using Celeste.Mod.CeilingUltra.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity("CeilingUltra/IceCeiling")]
[Tracked(false)]
public class IceCeiling : Entity {

    // use VortexHelper.FloorBooster if you are looking for a ground version
    public IceCeiling(EntityData data, Vector2 offset) : this(data.Position + offset, data.Width, data.Bool("Attached", true)) { }

    public IceCeiling(Vector2 position, int width, bool attached) : base(position) {
        Depth = 1999;
        Collider = new Hitbox(width, 3f, 0f, 0f);

        imageOffset = Vector2.Zero;
        if (attached) {
            Add(new StaticMover {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding,
                OnEnable = null,
                OnDisable = null
            });
        }
        tiles = BuildSprite();
    }

    private List<Sprite> tiles;

    private Vector2 imageOffset;

    private List<Sprite> BuildSprite() {
        List<Sprite> list = new List<Sprite>();
        for (int i = 0; (float)i < base.Width; i += 8) {
            string id = ((i == 0) ? "Left" : ((!((float)(i + 16) > base.Width)) ? "Mid" : "Right"));
            Sprite sprite = IceCeilingSpriteBank.Create("IceCeiling" + id);
            sprite.Position = new Vector2(i, 0f);
            list.Add(sprite);
            Add(sprite);
        }
        return list;
    }

    public void OnShake(Vector2 amount) {
        imageOffset += amount;
    }

    public bool IsRiding(Solid solid) {
        return CollideCheck(solid, Position - Vector2.UnitY);
    }
    public bool IsRiding(JumpThru jumpthru) {
        // both UpsideDownJumpThru doesn't expect to have static movers, so we just return false;
        return false;
    }

    public override void Render() {
        Vector2 orig = Position;
        Position += imageOffset;
        base.Render();
        Position = orig;
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        foreach (Sprite sprite in tiles) {
            sprite.Play("ice");
        }
    }

    [LoadContent]
    private static void LoadContent() {
        IceCeilingSpriteBank = new SpriteBank(GFX.Game, "Graphics/IceCeilingSprites.xml");
    }

    public static SpriteBank IceCeilingSpriteBank;
}

internal static class IceCeilingHelper {


    private static Type ChroniaIceFloor = null;

    private static Type VortexFloorBooster = null;

    [Initialize]
    public static void Initialize() {
        ChroniaIceFloor = ModUtils.GetType("ChroniaHelper", "ChroniaHelper.Entities.IceFloor");

        VortexFloorBooster = ModUtils.GetType("VortexHelper", "Celeste.Mod.VortexHelper.Entities.FloorBooster");

        // no CavernHelper IcyFloor coz it doesn't affect player
    }

    public static bool CheckCeiling(Player player) {
        if (GravityImports.IsPlayerInverted) {
            if (ChroniaIceFloor != null && Engine.Scene.Tracker.GetEntitiesTrackIfNeeded(ChroniaIceFloor) is { } chroniaIceFloor && chroniaIceFloor.IsNotEmpty()) {
                foreach (Entity e in chroniaIceFloor) {
                    if (player.CollideCheck(e)) {
                        return true;
                    }
                }
            }

            if (VortexFloorBooster != null && Engine.Scene.Tracker.GetEntitiesTrackIfNeeded(VortexFloorBooster) is { } floorBoosters && floorBoosters.IsNotEmpty()) {
                foreach (Entity e in floorBoosters) {
                    if (player.Bottom <= e.Bottom && player.CollideCheck(e) && IsVortexIceFloor(e)) {
                        return true;
                    }
                }
            }

            return false;
        }
        else {
            if (player.CollideCheck<IceCeiling>()) {
                return true;
            }

            if (VortexFloorBooster != null && Engine.Scene.Tracker.GetEntitiesTrackIfNeeded(VortexFloorBooster) is { } floorBoosters && floorBoosters.IsNotEmpty()) {
                foreach (Entity e in floorBoosters) {
                    if (player.Top >= e.Top && player.CollideCheck(e) && IsVortexIceCeiling(e)) {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool IsVortexIceFloor(Entity e) {
            return !e.GetFieldValue<bool>("Ceiling") && e.GetFieldValue<bool>("IceMode") && e.GetFieldValue<bool>("NoRefillsOnIce");
        }

        static bool IsVortexIceCeiling(Entity e) {
            // assume player is inverted
            return e.GetFieldValue<bool>("Ceiling") && e.GetFieldValue<bool>("IceMode") && e.GetFieldValue<bool>("NoRefillsOnIce");
        }
    }

    public static bool NoCeilingUltraIceFloor(Player player) {
        if (!GravityImports.IsPlayerInverted) {
            return true;
        }
        return !player.CollideCheck<IceCeiling>();
    }
}