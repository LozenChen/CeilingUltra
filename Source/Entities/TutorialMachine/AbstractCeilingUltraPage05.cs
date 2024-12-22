using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public abstract class AbstractCeilingUltraPage05 : CeilingUltraPage {
    private class Display {
        public Vector2 Position;

        public FancyText.Text Info;

        public CeilingUltraPlaybackTutorial Tutorial;

        private Coroutine routine;

        private float xEase;

        private float time;

        public MTexture texture;

        public Vector2 textureOffset;

        private int frameIndex;

        private CustomPlayerPlayBack playback;

        public Display(Vector2 position, string text, List<CeilingUltraPlaybackData> datas) {
            Position = position;
            Info = FancyText.Parse(text, 896, 8, 1f, Color.Black * 0.6f);
            Tutorial = new CeilingUltraPlaybackTutorial(datas) {
                OnRender = () => {
                    Draw.Line(-64f, 20f, 64f, 20f, Color.Black);
                },
                OnChange = () => {
                    playback = Tutorial.CurrPlayback;
                }
            };
            Tutorial.Initialize();
            routine = new Coroutine(Routine());
        }

        private IEnumerator Routine() {
            playback = Tutorial.CurrPlayback;
            int step = 0;
            while (true) {
                frameIndex = playback.FrameIndex;
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
            texture.DrawCentered(Position + textureOffset, Color.White, 3f);
        }
    }

    private readonly List<Display> displays = new();

    public AbstractCeilingUltraPage05() {
        Transition = Transitions.Spiral;
        ClearColor = Calc.HexToColor("fff2cc");
    }

    public void AddRecord(List<CeilingUltraPlaybackData> datas, string text, string texturePath, Vector2 textureOffset) {
        Display dis = new Display(new Vector2((float)base.Width * (0.28f + 0.44f * displays.Count), base.Height - 600), Dialog.Get(text), datas);
        dis.texture = Presentation.Gfx[texturePath];
        dis.textureOffset = textureOffset;
        displays.Add(dis);
    }

    public void AddRecord(CeilingUltraPlaybackData data, string text, string texturePath, Vector2 textureOffset) {
        AddRecord(new List<CeilingUltraPlaybackData>() { data }, text, texturePath, textureOffset);
    }

    public abstract void ImportData();

    public override void Added(CeilingUltraPresentation presentation) {
        base.Added(presentation);
        displays.Clear();
        ImportData();
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