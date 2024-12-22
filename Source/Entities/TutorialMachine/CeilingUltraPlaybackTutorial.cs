using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;
public class CeilingUltraPlaybackTutorial {

    public Action OnRender = null;

    public Action OnChange = null;

    private bool hasUpdated;

    private float dashTrailTimer;

    private int dashTrailCounter;

    private bool dashing;

    private bool launched;

    private float launchedTimer;

    private int tag;

    internal List<CustomPlayerPlayBack> PlayBacks;

    private List<Vector2> DashDirections;

    private int currDashDirIndex;

    private Vector2 CurrDashDir {
        get {
            Vector2 vec = DashDirections[currDashDirIndex];
            currDashDirIndex++;
            if (currDashDirIndex >= DashDirections.Count) {
                currDashDirIndex = 0;
            }
            return vec;
        }
    }

    internal CustomPlayerPlayBack CurrPlayback { get; private set; }

    public CeilingUltraPlaybackTutorial(List<CeilingUltraPlaybackData> infos) {
        PlayBacks = infos.Select(info => new CustomPlayerPlayBack(info.offset, PlayerSpriteMode.MadelineNoBackpack, PlaybackData.Tutorials[$"CeilingUltra/{info.name}"], PlayNextPlayBack)).ToList();
        DashDirections = new();
        infos.ForEach(info => DashDirections.AddRange(info.dashDirs));
        tag = nextTag; // we use the cursed tag system to f**k with trails of two playbacks in same screen with different transition matrix (ppt.page05)
        nextTag = 4 - nextTag;
    }

    private static int nextTag = 4;

    public void Initialize() {
        currDashDirIndex = 0;
        CurrPlayback = PlayBacks.Last();
        PlayNextPlayBack();
    }

    public void PlayNextPlayBack() {
        int n = PlayBacks.IndexOf(CurrPlayback);
        if (n <= PlayBacks.Count - 2) {
            CurrPlayback = PlayBacks[n + 1];
            CurrPlayback.SilentRestart();
        }
        else {
            CurrPlayback = PlayBacks[0];
            CurrPlayback.SilentRestart();
        }
        if (OnChange is not null) {
            OnChange();
        }
    }

    public void Update() {
        CurrPlayback.Update();
        CurrPlayback.Hair.AfterUpdate();
        if (CurrPlayback.Sprite.CurrentAnimationID == "dash" && CurrPlayback.Sprite.CurrentAnimationFrame == 0) {
            if (!dashing) {
                dashing = true;
                global::Celeste.Celeste.Freeze(0.05f);
                SlashFx.Burst(CurrPlayback.Center, CurrDashDir.Angle()).Tag = tag;
                dashTrailTimer = 0.1f;
                dashTrailCounter = 2;
                CreateTrail();
            }
        }
        else {
            dashing = false;
        }
        if (dashTrailTimer > 0f) {
            dashTrailTimer -= Engine.DeltaTime;
            if (dashTrailTimer <= 0f) {
                CreateTrail();
                dashTrailCounter--;
                if (dashTrailCounter > 0) {
                    dashTrailTimer = 0.1f;
                }
            }
        }
        if (launched) {
            float prevVal = launchedTimer;
            launchedTimer += Engine.DeltaTime;
            if (launchedTimer >= 0.5f) {
                launched = false;
                launchedTimer = 0f;
            }
            else if (Calc.OnInterval(launchedTimer, prevVal, 0.15f)) {
                SpeedRing speedRing = Engine.Pooler.Create<SpeedRing>().Init(CurrPlayback.Center, (CurrPlayback.Position - CurrPlayback.LastPosition).Angle(), Color.White);
                speedRing.Tag = tag;
                Engine.Scene.Add(speedRing);
            }
        }
        hasUpdated = true;
    }

    public void Render(Vector2 position, float scale) {
        Matrix transformationMatrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position.X, position.Y, 0f);
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, transformationMatrix);
        // here we need RasterizerState.CullNone (instead of null) to make sprite.X = -1 work

        foreach (Entity entity in Engine.Scene.Tracker.GetEntities<TrailManager.Snapshot>()) {
            if (entity.Tag == tag) {
                entity.Render();
            }
        }
        foreach (Entity entity2 in Engine.Scene.Tracker.GetEntities<SlashFx>()) {
            if (entity2.Tag == tag && entity2.Visible) {
                entity2.Render();
            }
        }
        foreach (Entity entity3 in Engine.Scene.Tracker.GetEntities<SpeedRing>()) {
            if (entity3.Tag == tag) {
                entity3.Render();
            }
        }

        if (CurrPlayback.Visible && hasUpdated) {
            CurrPlayback.Render();
        }
        if (OnRender is not null) {
            OnRender();
        }
        Draw.SpriteBatch.End();
        Draw.SpriteBatch.Begin();
    }


    private void CreateTrail() {
        TrailManager.Add(CurrPlayback.Position, CurrPlayback.Sprite, CurrPlayback.Hair, CurrPlayback.Sprite.Scale, Player.UsedHairColor, 0).Tag = tag;
    }
}