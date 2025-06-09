using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.CeilingUltra.Entities;

/*
MIT License

Copyright (c) 2021 marshall h (original project)
Copyright (c) 2022 Communal Helper Team (all other changes)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */
// the codes are based on ShroomHelper's codes
// https://github.com/CommunalHelper/ShroomHelper/blob/dev/Code/Entities/CrumbleBlockOnTouch.cs

[CustomEntity("CeilingUltra/CrumbleBlockOnTouch")]
public class CrumbleBlockOnTouch : Solid {
    public bool permanent;
    public float delay;
    public bool triggered;
    public bool blendIn;
    public bool destroyStaticMovers;

    public bool CheckTop = true;
    public bool CheckBottom = true;
    public bool CheckLeft = true;
    public bool CheckRight = true;
    public bool BreakOnDashCollide = true;

    private readonly char tileType;
    private EntityID id;

    public CrumbleBlockOnTouch(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset, data.Width, data.Height, safe: true){
        Depth = -12999;
        this.id = id;
        tileType = data.Char("tiletype", 'm');
        blendIn = data.Bool("blendin", true);
        delay = data.Float("delay");
        permanent = data.Bool("persistent");
        destroyStaticMovers = data.Bool("destroyStaticMovers");
        SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
        CheckLeft = data.Bool("CheckLeft", true);
        CheckRight = data.Bool("CheckRight", true);
        CheckTop = data.Bool("CheckTop", true);
        CheckBottom = data.Bool("CheckBottom", true);
        BreakOnDashCollide = data.Bool("BreakOnDashCollide", true);
        if (BreakOnDashCollide) {
            OnDashCollide = ActivateOnDash;
            Add(new ActivateOnDashCollideComponent());
        }
    }

    public DashCollisionResults ActivateOnDash(Player player, Vector2 direction) {
        triggered = true;
        return DashCollisionResults.NormalCollision;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        TileGrid tileGrid;
        if (!blendIn) {
            tileGrid = GFX.FGAutotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;
        }
        else {
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)(X / 8f) - tileBounds.Left;
            int y = (int)(Y / 8f) - tileBounds.Top;
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            tileGrid = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
            Depth = -10501;
        }

        Add(tileGrid);
        Add(new Coroutine(Sequence()));
        Add(new TileInterceptor(tileGrid, highPriority: true));
        Add(new LightOcclude());
        if (CollideCheck<Player>()) {
            RemoveSelf();
        }
    }

    public override void OnStaticMoverTrigger(StaticMover sm) {
        triggered = true;
    }

    public void Break() {
        if (!Collidable || Scene == null) {
            return;
        }

        Audio.Play("event:/new_content/game/10_farewell/quake_rockbreak", Position);
        Collidable = false;
        for (int i = 0; i < Width / 8f; i++) {
            for (int j = 0; j < Height / 8f; j++) {
                if (!Scene.CollideCheck<Solid>(new Rectangle((int)X + (i * 8), (int)Y + (j * 8), 8, 8))) {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + (i * 8), 4 + (j * 8)), tileType, playSound: true).BlastFrom(TopCenter));
                }
            }
        }

        if (permanent) {
            Level level = SceneAs<Level>();
            level.Session.DoNotLoad.Add(id);
        }

        if (destroyStaticMovers) {
            DestroyStaticMovers();
        }

        RemoveSelf();
    }

    private bool PlayerBreakCheck() {
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player != null) {
            if (CheckTop && CollideCheck(player, Position - Vector2.UnitY)) {
                return true;
            }

            if (CheckLeft && CollideCheck(player, Position - Vector2.UnitX)) {
                return true;
            }
            if (CheckRight && CollideCheck(player, Position + Vector2.UnitX)) { // we don't check facing
                return true;
            }

            if (CheckBottom && CollideCheck(player, Position + Vector2.UnitY)) {
                return true;
            }
        }

        return false;
    }

    private IEnumerator Sequence() {
        while (!triggered && !PlayerBreakCheck()) {
            yield return null;
        }

        while (delay > 0f) {
            delay -= Engine.DeltaTime;
            yield return null;
        }

        while (true) {
            Break();
            yield break;
        }
    }
}