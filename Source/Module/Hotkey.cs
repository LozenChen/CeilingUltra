namespace Celeste.Mod.CeilingUltra.Module;

/*
public static class CeilingUltraHotkey {

    public static VirtualButton hotkey;

    public static bool Enabled = false;

    [Load]
    public static void Load() {
        On.Celeste.Level.Render += HotkeysPressed;
        On.Monocle.Scene.AfterUpdate += Scene_AfterUpdate;
    }



    [Unload]
    public static void Unload() {
        On.Celeste.Level.Render -= HotkeysPressed;
        On.Monocle.Scene.AfterUpdate -= Scene_AfterUpdate;
    }


    [Initialize]
    public static void HotkeyInitialize() {
        hotkey = new VirtualButton();
        hotkey.Binding.Add(Keys.E);
        Enabled = ModUtils.ExtendedVariantInstalled && ModUtils.GravityHelperInstalled;
    }

    private static void Scene_AfterUpdate(On.Monocle.Scene.orig_AfterUpdate orig, Scene self) {
        orig(self);
        hotkey.Update();
    }

    private static void HotkeysPressed(On.Celeste.Level.orig_Render orig, Level self) {
        orig(self);
        if (Enabled) {
            OnHotkeyPressed();
        }
    }

    private static void OnHotkeyPressed() {
        if (hotkey.Pressed) {
            hotkey.ConsumePress();
            ExtendedVariantsModule.Instance.TriggerManager.setVariantValueInSession(ExtendedVariantsModule.Variant.UpsideDown, !(bool)ExtendedVariantsModule.Instance.TriggerManager.GetCurrentVariantValue(ExtendedVariantsModule.Variant.UpsideDown));
            GravityHelperModule.PlayerComponent?.SetGravity(GravityType.Toggle);
        }
    }
}
*/