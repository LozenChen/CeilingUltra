using Microsoft.Xna.Framework;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public class CeilingUltraPage04a : AbstractCeilingUltraPage04 {

    public CeilingUltraPage04a() { }

    public override void ImportData() {
        AddRecord(new CeilingUltraPlaybackData("ceiling_hyper_1", new Vector2(-75f, 26f), new Vector2(1f, -1f)), "platform/a01", Vector2.Zero, 4f);
        AddRecord(new CeilingUltraPlaybackData("ceiling_hyper_2", new Vector2(-120f, 12f), new Vector2(1f, -1f), new Vector2(1f, -1f)), "platform/a02", Vector2.Zero, 4f);
        AddRecord(new CeilingUltraPlaybackData("ceiling_hyper_3", new Vector2(-153f, 12f), new Vector2(1f, -1f), new Vector2(1f, -1f), new Vector2(1f, -1f)), "platform/a03", Vector2.Zero, 4f);
    }

    public override string Page4_List => "CEILING_ULTRA_PAGE4A_LIST";

}