global using Celeste.Mod.CeilingUltra.Utils.Attributes;
global using static Celeste.Mod.CeilingUltra.GlobalVariables;
using Celeste.Mod.CeilingUltra.Module;
using Monocle;

namespace Celeste.Mod.CeilingUltra;

internal static class GlobalVariables {

    public static CeilingUltraSettings ceilingUltraSetting => CeilingUltraSettings.Instance;
    public static Player? player => Engine.Scene.Tracker.GetEntity<Player>();

    public static readonly object[] parameterless = { };
}


internal static class GlobalMethod {
    public static T Apply<T>(this T obj, Action<T> action) {
        action(obj);
        return obj;
    }

}