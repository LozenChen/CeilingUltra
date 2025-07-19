using Microsoft.Xna.Framework;
using MonoMod.ModInterop;

namespace Celeste.Mod.CeilingUltra.ModInterop;

public static class GravityImports {

    // gravity helper does not invert the speed, but inverts its effect
    // e.g. MoveV(vec) replaced by MoveV(- vec)
    public static bool IsPlayerInverted => GravityHelperImport.IsPlayerInverted?.Invoke() ?? false;

    public static int InvertY => IsPlayerInverted ? -1 : 1;

    public static Vector2 CeilingDir => IsPlayerInverted ? Vector2.UnitY : -Vector2.UnitY;

    public static Vector2 GravityDir => IsPlayerInverted ? -Vector2.UnitY : Vector2.UnitY;
    public static Vector2 GetGravityAffectedVector2(this Vector2 vec) {
        if (IsPlayerInverted) {
            return new Vector2(vec.X, -vec.Y);
        }
        return vec;
    }

    // gravity helper hooks MoveV to invert its effect, so this is enough
    // note that it's not MoveVExact! though GravityHelper has a hook on MoveVExact, but that's to handle UpsideDownJumpThru
    public static void MoveV_GravityCompatible(this Actor actor, float moveV, Collision onCollide) {
        actor.MoveV(moveV, onCollide);
    }


    [InitializeAtFirst]
    private static void Initialize() {
        typeof(GravityHelperImport).ModInterop();
    }
}


[ModImportName("GravityHelper")]
internal static class GravityHelperImport {
    public static Func<bool> IsPlayerInverted;
}