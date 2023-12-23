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

    public static void LogHookData(this string methodBase, string hook, bool success) {
        Loader.HookData data = new Loader.HookData(methodBase, hook);
        if (Loader.HookLogs.ContainsKey(data)) {
            // multiple hooks on same methodBase would create too many logs, so we store this and log them after all hooks have been done
            Loader.HookLogs[data] &= success;
        }
        else {
            Loader.HookLogs[data] = success;
        }
    }
}