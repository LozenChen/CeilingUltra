using Microsoft.Xna.Framework;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public class CeilingUltraPage05b : AbstractCeilingUltraPage05 {

    public CeilingUltraPage05b() { }

    public override void ImportData() {
        AddRecord(new CeilingUltraPlaybackData("wall_too_early", new Vector2(0f, 20f), new Vector2(1f, -1f)), "CEILING_ULTRA_PAGE5B_INFO1", "platform/c02", new Vector2(90f, -25f));
        AddRecord(new List<CeilingUltraPlaybackData>() {
            new CeilingUltraPlaybackData("wall_too_late_1", new Vector2(-19f, 20f), new Vector2(1f, -1f)),
            new CeilingUltraPlaybackData("wall_too_late_2", new Vector2(0f, 20f), new Vector2(1f, -1f))
        }, "CEILING_ULTRA_PAGE5B_INFO2", "platform/c02", new Vector2(103f, -25f));
    }
}