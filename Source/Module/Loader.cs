using Celeste.Mod.CeilingUltra.Utils;
using MonoMod.RuntimeDetour;
using System.Reflection;
using static Celeste.Mod.CeilingUltra.Module.Loader;

namespace Celeste.Mod.CeilingUltra.Module;

internal static class Loader {
    public static void Load() {
        Reloading = GFX.Loaded;
        AttributeUtils.Invoke<LoadAttribute>();
    }

    public static void Unload() {
        AttributeUtils.Invoke<UnloadAttribute>();
        HookHelper.Unload();
    }

    public static void Initialize() {
        HookLogs.Clear();
        CeilingUltraModule.Warnings = "";
        HookHelper.InitializeAtFirst();
        ModUtils.InitializeAtFirst();
        AttributeUtils.Invoke<InitializeAttribute>();
        CeilingUltraModule.Instance.SaveSettings();
        if (Reloading) {
            OnReload();
            Reloading = false;
        }
        foreach (HookData hookData in HookLogs.Keys) {
            if (HookLogs[hookData]) {
                Logger.Log("CeilingUltra", $"{hookData.hook} hook {hookData.methodBase}");
            }
            else {
                CeilingUltraModule.Warnings += $"\n{hookData.hook} fail to hook {hookData.methodBase}";
            }
        }
        if (CeilingUltraModule.Warnings.IsNotNullOrEmpty()) {
            Logger.Log(LogLevel.Warn, "CeilingUltra", CeilingUltraModule.Warnings);
        }
        else {
            Logger.Log(LogLevel.Info, "CeilingUltra", "All Hooks Succeed!");
        }
    }

    public static void LoadContent() {
        AttributeUtils.Invoke<LoadContentAttribute>();
    }

    public static void OnReload() {
        Logger.Log("CeilingUltra", "Reloading!");
        if (ModUtils.GetType("CelesteTAS", "TAS.EverestInterop.InfoHUD.InfoCustom") is { } type) {
            type.InvokeMethod("CollectAllTypeInfo");
        }
    }

    public static bool Reloading;

    public static Dictionary<HookData, bool> HookLogs = new();

    public struct HookData {
        public string methodBase;
        public string hook;
        public HookData(string methodBase, string hook) {
            this.methodBase = methodBase;
            this.hook = hook;
        }
    }
}