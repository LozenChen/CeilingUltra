namespace Celeste.Mod.CeilingUltra.Module;

public class CeilingUltraModule : EverestModule {

    public static CeilingUltraModule Instance;

    public static CeilingUltraSettings Settings => CeilingUltraSettings.Instance;
    public CeilingUltraModule() {
        Instance = this;
        AttributeUtils.CollectMethods<LoadAttribute>();
        AttributeUtils.CollectMethods<UnloadAttribute>();
        AttributeUtils.CollectMethods<LoadContentAttribute>();
        AttributeUtils.CollectMethods<InitializeAttribute>();
    }

    public override Type SettingsType => typeof(CeilingUltraSettings);
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
        if (firstLoad) {
            Loader.LoadContent();
        }
    }

    public override void LoadSettings() {
        base.LoadSettings();
        GlobalVariables.ceilingUltraSetting.OnLoadSettings();
    }

    public override void OnInputInitialize() {
        base.OnInputInitialize();
    }
}