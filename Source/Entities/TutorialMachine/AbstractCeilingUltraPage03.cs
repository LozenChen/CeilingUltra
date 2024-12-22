using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public abstract class AbstractCeilingUltraPage03 : CeilingUltraPage {
    private readonly string title;

    private string titleDisplayed;

    private MTexture clipArt;

    private float clipArtEase;

    private FancyText.Text infoText;

    private AreaCompleteTitle easyText;

    public abstract string Title { get; }

    public abstract string ClipArt { get; }

    public abstract string Info { get; }

    public abstract Vector2 ClipArtOffset { get; }

    public AbstractCeilingUltraPage03() {
        Transition = Transitions.Blocky;
        ClearColor = Calc.HexToColor("d9ead3");
        title = Dialog.Clean(Title);
        titleDisplayed = "";
    }

    public override void Added(CeilingUltraPresentation presentation) {
        base.Added(presentation);
        clipArt = presentation.Gfx[ClipArt];
    }

    public override IEnumerator Routine() {
        while (titleDisplayed.Length < title.Length) {
            titleDisplayed += title[titleDisplayed.Length];
            yield return 0.05f;
        }
        yield return PressButton();
        Audio.Play("event:/new_content/game/10_farewell/ppt_wavedash_whoosh");
        while (clipArtEase < 1f) {
            clipArtEase = Calc.Approach(clipArtEase, 1f, Engine.DeltaTime);
            yield return null;
        }
        yield return 0.25f;
        infoText = FancyText.Parse(Dialog.Get(Info), Width - 240, 32, 1f, Color.Black * 0.7f);
        yield return PressButton();
        Audio.Play("event:/new_content/game/10_farewell/ppt_its_easy");
        easyText = new AreaCompleteTitle(new Vector2((float)Width / 2f, Height - 150), Dialog.Clean("CEILING_ULTRA_PAGE3_EASY"), 2f, rainbow: true);
        yield return 1f;
    }

    public override void Update() {
        if (easyText != null) {
            easyText.Update();
        }
    }

    public override void Render() {
        ActiveFont.DrawOutline(titleDisplayed, new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
        if (clipArtEase > 0f) {
            Vector2 scale = Vector2.One * (1f + (1f - clipArtEase) * 3f) * 0.8f;
            float rotation = (1f - clipArtEase) * 8f;
            Color color = Color.White * clipArtEase;
            clipArt.DrawCentered(new Vector2((float)base.Width / 2f + ClipArtOffset.X, (float)base.Height / 2f + ClipArtOffset.Y - 90f), color, scale, rotation);
        }
        if (infoText != null) {
            infoText.Draw(new Vector2((float)base.Width / 2f, base.Height - 350), new Vector2(0.5f, 0f), Vector2.One, 1f);
        }
        if (easyText != null) {
            easyText.Render();
        }
    }
}