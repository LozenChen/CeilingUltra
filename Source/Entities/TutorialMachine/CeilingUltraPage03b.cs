using Microsoft.Xna.Framework;
namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;
public class CeilingUltraPage03b : AbstractCeilingUltraPage03 {

    public override string Title => "CEILING_ULTRA_PAGE3B_TITLE";

    public override string ClipArt => "wall_hyper";

    public override string Info => "CEILING_ULTRA_PAGE3B_INFO";

    public override Vector2 ClipArtOffset => new Vector2(500f, 120f);
    public CeilingUltraPage03b() { }
}