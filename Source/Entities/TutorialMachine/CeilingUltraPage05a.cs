using Microsoft.Xna.Framework;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;

public class CeilingUltraPage05a : AbstractCeilingUltraPage05 {

    public CeilingUltraPage05a() { }

    public override void ImportData() {
        AddRecord(new CeilingUltraPlaybackData("ceiling_too_far", new Vector2(-50f, 20f), new Vector2(1f, -1f)), "CEILING_ULTRA_PAGE5A_INFO1", "platform/c01", -Vector2.UnitY * 170f);
        AddRecord(new CeilingUltraPlaybackData("ceiling_too_late", new Vector2(-50f, 20f), new Vector2(1f, -1f)), "CEILING_ULTRA_PAGE5A_INFO2", "platform/c01", -Vector2.UnitY * 170f);
    }
}