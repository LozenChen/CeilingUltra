using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;
internal class CustomPlayerPlayBack : PlayerPlayback {
    // only for ppt

    public Action OnRestart;

    public bool Silent = false;

    public CustomPlayerPlayBack(Vector2 start, PlayerSpriteMode sprite, List<Player.ChaserState> timeline, Action onRestart) : base(start, sprite, timeline) {
        OnRestart = onRestart ?? SilentRestart;
    }

    public void SilentRestart() {
        if (!Silent) {
            Audio.Play("event:/new_content/char/tutorial_ghost/appear", Position);
        }
        Visible = true;
        time = TrimStart;
        index = 0;
        loopDelay = 0.25f;
        while (time > Timeline[index].TimeStamp) {
            index++;
        }

        SetFrame(index);
    }

    public override void Update() {
        if (startDelay > 0f) {
            startDelay -= Engine.DeltaTime;
        }
        LastPosition = Position;
        Components.Update();
        if (index >= Timeline.Count - 1 || Time >= TrimEnd) {
            if (!Silent && Visible) {
                Audio.Play("event:/new_content/char/tutorial_ghost/disappear", Position);
            }
            Visible = false;
            Position = start;
            loopDelay -= Engine.DeltaTime;
            if (loopDelay <= 0f) {
                OnRestart();
            }
        }
        else if (startDelay <= 0f) {
            SetFrame(index);
            time += Engine.DeltaTime;
            while (index < Timeline.Count - 1 && time >= Timeline[index + 1].TimeStamp) {
                index++;
            }
        }
        if (Visible && ShowTrail && base.Scene != null && base.Scene.OnInterval(0.1f)) {
            TrailManager.Add(Position, Sprite, Hair, Sprite.Scale, Hair.Color, base.Depth + 1);
        }
    }
}
