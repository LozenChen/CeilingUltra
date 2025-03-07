﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public class CeilingUltraPage01 : CeilingUltraPage {
    private AreaCompleteTitle title;

    private float subtitleEase;

    private string titlePath;
    public CeilingUltraPage01(TitleType title) {
        Transition = Transitions.ScaleIn;
        ClearColor = Calc.HexToColor("9fc5e8");
        titlePath = title switch {
            TitleType.CeilingUltra => "CEILING_ULTRA_PAGE1_TITLE_CEILING_ULTRA",
            TitleType.CeilingHyper => "CEILING_ULTRA_PAGE1_TITLE_CEILING_HYPER",
            TitleType.WallHyper => "CEILING_ULTRA_PAGE1_TITLE_WALL_HYPER",
        };
    }

    public enum TitleType {
        CeilingUltra,
        CeilingHyper,
        WallHyper
    }

    public override void Added(CeilingUltraPresentation presentation) {
        base.Added(presentation);
    }

    public override IEnumerator Routine() {
        Audio.SetAltMusic("event:/new_content/music/lvl10/intermission_powerpoint");
        yield return 1f;
        title = new AreaCompleteTitle(new Vector2((float)Width / 2f, (float)Height / 2f - 100f), Dialog.Clean(titlePath), 2f, rainbow: true);
        yield return 1f;
        while (subtitleEase < 1f) {
            subtitleEase = Calc.Approach(subtitleEase, 1f, Engine.DeltaTime);
            yield return null;
        }
        yield return 0.1f;
    }

    public override void Update() {
        if (title != null) {
            title.Update();
        }
    }

    public override void Render() {
        if (title != null) {
            title.Render();
        }
        if (subtitleEase > 0f) {
            Vector2 position = new Vector2((float)base.Width / 2f, (float)base.Height / 2f + 80f);
            float x = 1f + Ease.BigBackIn(1f - subtitleEase) * 2f;
            float y = 0.25f + Ease.BigBackIn(subtitleEase) * 0.75f;
            ActiveFont.Draw(Dialog.Clean("CEILING_ULTRA_PAGE1_SUBTITLE"), position, new Vector2(0.5f, 0.5f), new Vector2(x, y), Color.Black * 0.8f);
        }
    }
}