using Microsoft.Xna.Framework;
using MonoMod.ModInterop;

namespace Celeste.Mod.CeilingUltra.ModInterop;

public static class ModImports {
    public static bool IsPlayerInverted => GravityHelperImport.IsPlayerInverted?.Invoke() ?? false;

    public static int InvertY => IsPlayerInverted ? -1 : 1;
    public static Vector2 GetGravityAffectedVector2(this Vector2 vec) {
        if (IsPlayerInverted) {
            return new Vector2(vec.X, -vec.Y);
        }
        return vec;
    }


    [Initialize]
    private static void Initialize() {
        typeof(GravityHelperImport).ModInterop();
    }
}


[ModImportName("GravityHelper")]
internal static class GravityHelperImport {
    public static Func<bool> IsPlayerInverted;
}