using Celeste.Mod.CeilingUltra.Utils;

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
        HookHelper.InitializeAtFirst();
        ModUtils.InitializeAtFirst();
        AttributeUtils.Invoke<InitializeAttribute>();
        CeilingUltraModule.Instance.SaveSettings();
        if (Reloading) {
            OnReload();
            Reloading = false;
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
}