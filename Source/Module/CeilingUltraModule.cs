using FMOD.Studio;
using Celeste.Mod;
using Celeste;

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

    public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance snapshot) {
        CreateModMenuSectionHeader(menu, inGame, snapshot);
        CreateMenu(this, menu, inGame);
    }

    public static void CreateMenu(EverestModule everestModule, TextMenu menu, bool inGame) {
        menu.Add(new TextMenu.OnOff("Enabled", ceilingUltraSetting.Enabled).Change(value => ceilingUltraSetting.Enabled = value));
        menu.Add(new TextMenuExt.SubHeaderExt("Ceiling Mechanisms") { HeightExtra = 10f});
        menu.Add(new TextMenu.OnOff("Ceiling Ultra", ceilingUltraSetting.CeilingUltraEnabled).Change(value => ceilingUltraSetting.CeilingUltraEnabled = value));
        menu.Add(new TextMenu.OnOff("Ceiling Refill Stamina", ceilingUltraSetting.CeilingRefillStamina).Change(value => ceilingUltraSetting.CeilingRefillStamina = value));
        menu.Add(new TextMenu.OnOff("Ceiling Refill Dash", ceilingUltraSetting.CeilingRefillDash).Change(value => ceilingUltraSetting.CeilingRefillDash = value));
        menu.Add(new TextMenu.OnOff("Ceiling Jump", ceilingUltraSetting.CeilingJumpEnabled).Change(value => ceilingUltraSetting.CeilingJumpEnabled = value));
        menu.Add(new TextMenu.OnOff("Ceiling Super/Hyper", ceilingUltraSetting.CeilingHyperEnabled).Change(value => ceilingUltraSetting.CeilingHyperEnabled = value));
        menu.Add(new TextMenu.OnOff("Updiag Dash End No Horizontal Speed Loss", ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss).Change(value => ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss = value));
        menu.Add(new TextMenuExt.SubHeaderExt("Vertical Mechanisms") { HeightExtra = 10f});
        menu.Add(new TextMenu.OnOff("Vertical Ultra", ceilingUltraSetting.VerticalUltraEnabled).Change(value => ceilingUltraSetting.VerticalUltraEnabled = value));
        menu.Add(new TextMenu.OnOff("Vertical Hyper", ceilingUltraSetting.VerticalHyperEnabled).Change(value => ceilingUltraSetting.VerticalHyperEnabled = value));
        menu.Add(new TextMenu.OnOff("Wall Refill Stamina", ceilingUltraSetting.WallRefillStamina).Change(value => ceilingUltraSetting.WallRefillStamina = value));
        menu.Add(new TextMenu.OnOff("Wall Refill Dash", ceilingUltraSetting.WallRefillDash).Change(value => ceilingUltraSetting.WallRefillDash = value));
        menu.Add(new TextMenu.OnOff("Dash Begin No Vertical Speed Loss", ceilingUltraSetting.DashBeginNoVerticalSpeedLoss).Change(value => ceilingUltraSetting.DashBeginNoVerticalSpeedLoss = value));

    }
}