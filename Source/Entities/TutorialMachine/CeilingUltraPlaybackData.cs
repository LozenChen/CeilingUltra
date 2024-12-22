using Microsoft.Xna.Framework;


namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;
public class CeilingUltraPlaybackData {
    public string name;

    public Vector2 offset;

    public List<Vector2> dashDirs;

    public CeilingUltraPlaybackData(string name, Vector2 offset, Vector2 dir1) {
        this.name = name;
        this.offset = offset;
        dashDirs = new List<Vector2>() { dir1 };
    }

    public CeilingUltraPlaybackData(string name, Vector2 offset, Vector2 dir1, Vector2 dir2) {
        this.name = name;
        this.offset = offset;
        dashDirs = new List<Vector2>() { dir1, dir2 };
    }
    public CeilingUltraPlaybackData(string name, Vector2 offset, Vector2 dir1, Vector2 dir2, Vector2 dir3) {
        this.name = name;
        this.offset = offset;
        dashDirs = new List<Vector2>() { dir1, dir2, dir3 };
    }
}