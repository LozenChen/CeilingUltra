using Microsoft.Xna.Framework;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public class CeilingUltraPage04b : AbstractCeilingUltraPage04 {

    public CeilingUltraPage04b() { }

    public override void ImportData() {
        AddRecord(new CeilingUltraPlaybackData("wall_hyper_1", new Vector2(137f, 93f), new Vector2(1f, -1f)), "platform/b01", new Vector2(138f, 48f), 4f);
        AddRecord(new CeilingUltraPlaybackData("wall_hyper_2", new Vector2(137f, 93f), new Vector2(1f, -1f), new Vector2(-1f, -1f)), "platform/b02", new Vector2(120f, 20f), 4f);
        AddRecord(new CeilingUltraPlaybackData("wall_hyper_3", new Vector2(220f, 170f), new Vector2(1f, -1f), new Vector2(-1f, -1f), new Vector2(1f, -1f)), "platform/b03", new Vector2(223f, 30f), 2.5f);
    }

    public override string Page4_List => "CEILING_ULTRA_PAGE4B_LIST";

}