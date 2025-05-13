using FMOD.Studio;

namespace Celeste.Mod.CeilingUltra.Module;

public class CeilingUltraModule : EverestModule {

    public static CeilingUltraModule Instance;

    public static string Warnings = "";

    public static CeilingUltraSettings Settings => CeilingUltraSettings.Instance;
    public CeilingUltraModule() {
        Instance = this;
        AttributeUtils.CollectMethods<LoadAttribute>();
        AttributeUtils.CollectMethods<UnloadAttribute>();
        AttributeUtils.CollectMethods<LoadContentAttribute>();
        AttributeUtils.CollectMethods<InitializeAttribute>();
        AttributeUtils.CollectMethods<InitializeAtFirstAttribute>();
    }

    public override Type SettingsType => typeof(CeilingUltraSettings);

    public override Type SessionType => typeof(CeilingUltraSession);
    public override void Load() {
        Loader.Load();
    }

    public override void Unload() {
        Loader.Unload();
    }

    public override void Initialize() {
        Loader.Initialize();
    }

    public override void LoadContent(bool firstLoad) {
        Loader.LoadContent();
    }

    public override void LoadSettings() {
        base.LoadSettings();
        GlobalVariables.ceilingUltraSetting.OnLoadSettings();
    }

    public override void OnInputInitialize() {
        base.OnInputInitialize();
    }

    public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
        CreateModMenuSectionHeader(menu, inGame, snapshot);
        menu.Add(new TextMenuExt.SubHeaderExt("") { HeightExtra = 2f });
        ModOptionsMenu.CreateMenu(this, menu, inGame, false);
    }
}




