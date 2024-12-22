using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public class CeilingUltraPage05 : CeilingUltraPage {
    private class Display {
        public Vector2 Position;

        public FancyText.Text Info;

        public CeilingUltraPlaybackTutorial Tutorial;

        private Coroutine routine;

        private float xEase;

        private float time;

        public MTexture texture;

        public Display(Vector2 position, string text, string name, Vector2 offset, Vector2 dashDir) {
            Position = position;
            Info = FancyText.Parse(text, 896, 8, 1f, Color.Black * 0.6f);
            Tutorial = new CeilingUltraPlaybackTutorial(new List<CeilingUltraPlaybackTutorial.Data> { new CeilingUltraPlaybackTutorial.Data(name, offset, dashDir) }) {
                OnRender = () => {
                    Draw.Line(-64f, 20f, 64f, 20f, Color.Black);
                }
            };
            Tutorial.Initialize();
            routine = new Coroutine(Routine());
        }

        private IEnumerator Routine() {
            PlayerPlayback playback = Tutorial.CurrPlayback;
            int step = 0;
            while (true) {
                int frameIndex = playback.FrameIndex;
                if (step % 2 == 0) {
                    Tutorial.Update();
                }
                if (frameIndex != playback.FrameIndex && playback.FrameIndex == playback.FrameCount - 1) {
                    while (time < 3f) {
                        yield return null;
                    }
                    yield return 0.1f;
                    while (xEase < 1f) {
                        xEase = Calc.Approach(xEase, 1f, Engine.DeltaTime * 4f);
                        yield return null;
                    }
                    xEase = 1f;
                    yield return 0.5f;
                    xEase = 0f;
                    time = 0f;
                }
                step++;
                yield return null;
            }
        }

        public void Update() {
            time += Engine.DeltaTime;
            routine.Update();
        }

        public void Render() {
            Tutorial.Render(Position, 4f);
            Info.DrawJustifyPerLine(Position + Vector2.UnitY * 200f, new Vector2(0.5f, 0f), Vector2.One * 0.8f, 1f);
            if (xEase > 0f) {
                Vector2 vector = Calc.AngleToVector((1f - xEase) * 0.1f + MathF.PI / 4f, 1f);
                Vector2 vector2 = vector.Perpendicular();
                float num = 0.5f + (1f - xEase) * 0.5f;
                float thickness = 64f * num;
                float num2 = 300f * num;
                Vector2 position = Position;
                Draw.Line(position - vector * num2, position + vector * num2, Color.Red, thickness);
                Draw.Line(position - vector2 * num2, position + vector2 * num2, Color.Red, thickness);
            }
            texture.DrawCentered(Position - Vector2.UnitY * 170f, Color.White, 3f);
        }
    }

    private List<Display> displays = new List<Display>();

    public CeilingUltraPage05() {
        Transition = Transitions.Spiral;
        ClearColor = Calc.HexToColor("fff2cc");
    }


    public override void Added(CeilingUltraPresentation presentation) {
        base.Added(presentation);
        MTexture texture = presentation.Gfx["platform/04"];
        displays.Add(new Display(new Vector2((float)base.Width * 0.28f, base.Height - 600), Dialog.Get("CEILING_ULTRA_PAGE5_INFO1"), "ceiling_too_far", new Vector2(-50f, 20f), new Vector2(1f, -1f)).Apply(x => x.texture = texture));
        displays.Add(new Display(new Vector2((float)base.Width * 0.72f, base.Height - 600), Dialog.Get("CEILING_ULTRA_PAGE5_INFO2"), "ceiling_too_late", new Vector2(-50f, 20f), new Vector2(1f, -1f)).Apply(x => x.texture = texture));
    }

    public override IEnumerator Routine() {
        yield return 0.5f;
    }
    public override void Update() {
        foreach (Display display in displays) {
            display.Update();
        }
    }
    public override void Render() {
        ActiveFont.DrawOutline(Dialog.Clean("CEILING_ULTRA_PAGE5_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
        foreach (Display display in displays) {
            display.Render();
        }
    }
}