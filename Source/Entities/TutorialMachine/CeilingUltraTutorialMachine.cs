﻿using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

[CustomEntity("CeilingUltra/CeilingUltraTutorialMachine")]
public class CeilingUltraTutorialMachine : JumpThru {
    private Entity frontEntity;

    private Image backSprite;

    private Image frontRightSprite;

    private Image frontLeftSprite;

    private Sprite noise;

    private Sprite neon;

    private Solid frontWall;

    private float insideEase;

    private float cameraEase;

    private bool playerInside;

    private bool inCutscene;

    private Coroutine routine;

    private CeilingUltraPresentation presentation;

    private float interactStartZoom;

    private EventInstance snapshot;

    private EventInstance usingSfx;

    private SoundSource signSfx;

    private TalkComponent talk;

    public bool usingNoiseSfx;

    public string usingPptPages;

    public CeilingUltraTutorialMachine(Vector2 position, string pptType)
        : base(position, 88, safe: true) {
        base.Tag = Tags.TransitionUpdate;
        base.Depth = 1000;
        base.Hitbox.Position = new Vector2(-41f, -59f);
        Add(backSprite = new Image(GFX.Game["objects/wavedashtutorial/building_back"]));
        backSprite.JustifyOrigin(0.5f, 1f);
        Add(noise = new Sprite(GFX.Game, "objects/wavedashtutorial/noise"));
        noise.AddLoop("static", "", 0.05f);
        noise.Play("static");
        noise.CenterOrigin();
        noise.Position = new Vector2(0f, -30f);
        noise.Color = Color.White * 0.5f;
        Add(frontLeftSprite = new Image(GFX.Game["objects/wavedashtutorial/building_front_left"]));
        frontLeftSprite.JustifyOrigin(0.5f, 1f);
        Add(talk = new TalkComponent(new Rectangle(-12, -8, 24, 8), new Vector2(0f, -50f), OnInteract));
        talk.Enabled = false;
        SurfaceSoundIndex = 42;
        usingPptPages = pptType switch {
            "Ceiling Hyper Tutorial" => "0a,1a,2,3a,4a,5a,6a",
            "Wall Hyper Tutorial" => "0b,1b,2,3b,4b,5b,6b",
            _ => "0,1,2,3a,4a,5a,3b,4b,5b,6"
        };
    }


    public CeilingUltraTutorialMachine(EntityData data, Vector2 position)
        : this(data.Position + position, data.Attr("PPT_Type", "Ceiling Ultra Tutorial")) {
        usingNoiseSfx = data.Bool("usingNoiseSfx");
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        scene.Add(frontEntity = new Entity(Position));
        frontEntity.Tag = Tags.TransitionUpdate;
        frontEntity.Depth = -10500;
        frontEntity.Add(frontRightSprite = new Image(GFX.Game["objects/wavedashtutorial/building_front_right"]));
        frontRightSprite.JustifyOrigin(0.5f, 1f);
        frontEntity.Add(neon = new Sprite(GFX.Game, "objects/wavedashtutorial/neon_"));
        neon.AddLoop("loop", "", 0.07f);
        neon.Play("loop");
        neon.JustifyOrigin(0.5f, 1f);
        scene.Add(frontWall = new Solid(Position + new Vector2(-41f, -59f), 88f, 38f, safe: true));
        frontWall.SurfaceSoundIndex = 42;
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);
        if (usingNoiseSfx) {
            Add(signSfx = new SoundSource(new Vector2(8f, -16f), "event:/new_content/env/local/cafe_sign"));
        }
        else {
            signSfx = null;
        }
    }

    public override void Update() {
        base.Update();
        if (!inCutscene) {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null) {
                frontWall.Collidable = true;
                bool flag = (entity.X > base.X - 37f && entity.X < base.X + 46f && entity.Y > base.Y - 58f) || frontWall.CollideCheck(entity);
                if (flag != playerInside) {
                    playerInside = flag;
                    if (playerInside) {
                        signSfx?.Stop();
                        snapshot = Audio.CreateSnapshot("snapshot:/game_10_inside_cafe");
                    }
                    else {
                        signSfx?.Play("event:/new_content/env/local/cafe_sign");
                        Audio.ReleaseSnapshot(snapshot);
                        snapshot = null;
                        Audio.SetMusicParam("fade", 1f);
                    }
                }
                if (playerInside) {
                    float percent = 1f - Calc.YoYo(Calc.ClampedMap(entity.X, base.X - 37f, base.X + 46f, 0f, 1f)) / 2f;
                    Audio.SetMusicParam("fade", percent);
                }
            }
            SceneAs<Level>().ZoomSnap(new Vector2(160f, 90f), 1f + Ease.QuadInOut(cameraEase) * 0.75f);
        }
        talk.Enabled = playerInside;
        frontWall.Collidable = !playerInside;
        insideEase = Calc.Approach(insideEase, playerInside ? 1f : 0f, Engine.DeltaTime * 4f);
        cameraEase = Calc.Approach(cameraEase, playerInside ? 1f : 0f, Engine.DeltaTime * 2f);
        frontRightSprite.Color = Color.White * (1f - insideEase);
        frontLeftSprite.Color = frontRightSprite.Color;
        neon.Color = frontRightSprite.Color;
        frontRightSprite.Visible = insideEase < 1f;
        frontLeftSprite.Visible = insideEase < 1f;
        neon.Visible = insideEase < 1f;
        if (base.Scene.OnInterval(0.05f)) {
            noise.Scale = Calc.Random.Choose(new Vector2(1f, 1f), new Vector2(-1f, 1f), new Vector2(1f, -1f), new Vector2(-1f, -1f));
        }
    }

    private void OnInteract(Player player) {
        if (!inCutscene) {
            Level level = base.Scene as Level;
            if (usingSfx != null) {
                Audio.SetParameter(usingSfx, "end", 1f);
                Audio.Stop(usingSfx);
            }
            Audio.SetMusicParam("fade", 0f);
            inCutscene = true;
            interactStartZoom = level.ZoomTarget;
            level.StartCutscene(SkipInteraction, fadeInOnSkip: true, endingChapterAfterCutscene: false, resetZoomOnSkip: false);
            Add(routine = new Coroutine(InteractRoutine(player)));
        }
    }

    private IEnumerator InteractRoutine(Player player) {
        Level level = Scene as Level;
        player.StateMachine.State = 11;
        player.StateMachine.Locked = true;
        yield return CutsceneEntity.CameraTo(new Vector2(X, Y - 30f) - new Vector2(160f, 90f), 0.25f, Ease.CubeOut);
        yield return level.ZoomTo(new Vector2(160f, 90f), 10f, 1f);
        usingSfx = Audio.Play("event:/state/cafe_computer_active", player.Position);
        Audio.Play("event:/new_content/game/10_farewell/cafe_computer_on", player.Position);
        Audio.Play("event:/new_content/game/10_farewell/cafe_computer_startupsfx", player.Position);
        presentation = new CeilingUltraPresentation(usingSfx, usingPptPages);
        Scene.Add(presentation);
        while (presentation.Viewing) {
            yield return null;
        }
        yield return level.ZoomTo(new Vector2(160f, 90f), interactStartZoom, 1f);
        player.StateMachine.Locked = false;
        player.StateMachine.State = 0;
        inCutscene = false;
        level.EndCutscene();
        Audio.SetAltMusic(null);
    }

    private void SkipInteraction(Level level) {
        Audio.SetAltMusic(null);
        inCutscene = false;
        level.ZoomSnap(new Vector2(160f, 90f), interactStartZoom);
        if (usingSfx != null) {
            Audio.SetParameter(usingSfx, "end", 1f);
            usingSfx.release();
        }
        if (presentation != null) {
            presentation.RemoveSelf();
        }
        presentation = null;
        if (routine != null) {
            routine.RemoveSelf();
        }
        routine = null;
        Player entity = level.Tracker.GetEntity<Player>();
        if (entity != null) {
            entity.StateMachine.Locked = false;
            entity.StateMachine.State = 0;
        }
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        Dispose();
    }

    public override void SceneEnd(Scene scene) {
        base.SceneEnd(scene);
        Dispose();
    }

    private void Dispose() {
        if (usingSfx != null) {
            Audio.SetParameter(usingSfx, "quit", 1f);
            usingSfx.release();
            usingSfx = null;
        }
        Audio.ReleaseSnapshot(snapshot);
        snapshot = null;
    }
}
