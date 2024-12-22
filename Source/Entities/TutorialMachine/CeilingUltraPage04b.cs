using System.Collections;
using Monocle;
using Microsoft.Xna.Framework;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public class CeilingUltraPage04b : CeilingUltraPage {
    private CeilingUltraPlaybackTutorial tutorial;

    private FancyText.Text list;

    private int listIndex;

    private float time;

    private int textureIndex;

    private MTexture[] textures;

    private Vector2[] offsets;
    public CeilingUltraPage04b() {
        Transition = Transitions.FadeIn;
        ClearColor = Calc.HexToColor("f4cccc");
    }

    public override void Added(CeilingUltraPresentation presentation) {
        base.Added(presentation);
        CeilingUltraPlaybackTutorial.Data record1 = new("wall_hyper_1", new Vector2(17f, 93f), new Vector2(1f, -1f));
        CeilingUltraPlaybackTutorial.Data record2 = new("wall_hyper_2", new Vector2(17f, 93f), new Vector2(1f, -1f), new Vector2(-1f, -1f));
        CeilingUltraPlaybackTutorial.Data record3 = new("wall_hyper_1", new Vector2(17f, 93f), new Vector2(1f, -1f));
        tutorial = new CeilingUltraPlaybackTutorial(new List<CeilingUltraPlaybackTutorial.Data>() { record1, record2, record3 });
        tutorial.OnChange = () => {
            textureIndex = tutorial.PlayBacks.IndexOf(tutorial.CurrPlayback);
        };
        tutorial.Initialize();
        textures = new MTexture[3] {
            presentation.Gfx["platform/05"],
            presentation.Gfx["platform/06"],
            presentation.Gfx["platform/05"]
        };
        offsets = new Vector2[] {
            new Vector2(18f, 48f),
            Vector2.UnitY * 20f,
            new Vector2(18f, 38f)
        };
        tutorial.OnRender = TutorialOnRender;
    }

    public void TutorialOnRender() {
        textures[textureIndex].DrawCentered(offsets[textureIndex]);
    }

    public override IEnumerator Routine() {
        yield return 0.5f;
        list = FancyText.Parse(Dialog.Get("CEILING_ULTRA_PAGE4B_LIST"), Width, 32, 1f, Color.Black * 0.7f);
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
        time += Engine.DeltaTime * 4f;
        tutorial.Update();
    }

    public override void Render() {
        ActiveFont.DrawOutline(Dialog.Clean("CEILING_ULTRA_PAGE4B_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
        tutorial.Render(new Vector2((float)base.Width / 2f, (float)base.Height / 2f - 100f), 4f);
        if (list != null) {
            list.Draw(new Vector2(160f, base.Height - 400), new Vector2(0f, 0f), Vector2.One, 1f, 0, listIndex);
        }
    }
}