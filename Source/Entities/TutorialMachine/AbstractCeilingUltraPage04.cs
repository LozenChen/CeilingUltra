using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public abstract class AbstractCeilingUltraPage04 : CeilingUltraPage {
    private CeilingUltraPlaybackTutorial tutorial;

    private FancyText.Text list;

    private int listIndex;

    private int playbackIndex;

    private readonly List<CeilingUltraPlaybackData> datas;

    private readonly List<MTexture> textures;


    private readonly List<Vector2> offsets;

    private readonly List<float> scales;
    public AbstractCeilingUltraPage04() {
        Transition = Transitions.FadeIn;
        ClearColor = Calc.HexToColor("f4cccc");
        datas = new();
        textures = new();
        offsets = new();
        scales = new();
    }

    public abstract void ImportData();

    public abstract string Page4_List { get; }

    public void AddRecord(CeilingUltraPlaybackData data, string texturePath, Vector2 textureOffset, float scale) {
        datas.Add(data);
        textures.Add(Presentation.Gfx[texturePath]);
        offsets.Add(textureOffset);
        scales.Add(scale);
    }

    public override void Added(CeilingUltraPresentation presentation) {
        base.Added(presentation);
        ImportData();
        tutorial = new CeilingUltraPlaybackTutorial(datas);
        tutorial.OnChange = () => {
            playbackIndex = tutorial.PlayBacks.IndexOf(tutorial.CurrPlayback);
        };
        tutorial.Initialize();
        tutorial.OnRender = TutorialOnRender;
    }

    public void TutorialOnRender() {
        textures[playbackIndex].DrawCentered(offsets[playbackIndex]);
    }

    public override IEnumerator Routine() {
        yield return 0.5f;
        list = FancyText.Parse(Dialog.Get(Page4_List), Width, 32, 1f, Color.Black * 0.7f);
        float delay = 0f;
        while (listIndex < list.Nodes.Count) {
            if (list.Nodes[listIndex] is FancyText.NewLine) {
                yield return PressButton();
            }
            else {
                delay += 0.008f;
                if (delay >= 0.016f) {
                    delay -= 0.016f;
                    yield return 0.016f;
                }
            }
            listIndex++;
        }
    }

    public override void Update() {
        tutorial.Update();
    }

    public override void Render() {
        ActiveFont.DrawOutline(Dialog.Clean("CEILING_ULTRA_PAGE4_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
        tutorial.Render(new Vector2((float)base.Width / 2f, (float)base.Height / 2f - 100f), scales[playbackIndex]);
        if (list != null) {
            list.Draw(new Vector2(160f, base.Height - 450), new Vector2(0f, 0f), Vector2.One, 1f, 0, listIndex);
        }
    }
}