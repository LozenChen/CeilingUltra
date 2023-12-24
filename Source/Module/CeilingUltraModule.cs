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
        menu.Add(new TextMenu.OnOff("Enabled".ToDialogText(), ceilingUltraSetting.Enabled).Change(value => ceilingUltraSetting.Enabled = value));
        menu.Add(new TextMenuExt.SubHeaderExt("Ceiling Mechanisms".ToDialogText()) { HeightExtra = 10f});
        menu.Add(new TextMenu.OnOff("Ceiling Refill Stamina".ToDialogText(), ceilingUltraSetting.CeilingRefillStamina).Change(value => ceilingUltraSetting.CeilingRefillStamina = value));
        menu.Add(new TextMenu.OnOff("Ceiling Refill Dash".ToDialogText(), ceilingUltraSetting.CeilingRefillDash).Change(value => ceilingUltraSetting.CeilingRefillDash = value));
        menu.Add(new TextMenu.OnOff("Ceiling Jump".ToDialogText(), ceilingUltraSetting.CeilingJumpEnabled).Change(value => ceilingUltraSetting.CeilingJumpEnabled = value));
        menu.Add(new TextMenu.OnOff("Ceiling Super Hyper".ToDialogText(), ceilingUltraSetting.CeilingHyperEnabled).Change(value => ceilingUltraSetting.CeilingHyperEnabled = value));
        menu.Add(new TextMenu.OnOff("Ceiling Ultra".ToDialogText(), ceilingUltraSetting.CeilingUltraEnabled).Change(value => ceilingUltraSetting.CeilingUltraEnabled = value));
        menu.Add(new TextMenu.OnOff("Updiag Dash End No Horizontal Speed Loss".ToDialogText(), ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss).Change(value => ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss = value));
        menu.Add(new TextMenuExt.SubHeaderExt("Vertical Mechanisms".ToDialogText()) { HeightExtra = 10f});
        menu.Add(new TextMenu.OnOff("Wall Refill Stamina".ToDialogText(), ceilingUltraSetting.WallRefillStamina).Change(value => ceilingUltraSetting.WallRefillStamina = value));
        menu.Add(new TextMenu.OnOff("Wall Refill Dash".ToDialogText(), ceilingUltraSetting.WallRefillDash).Change(value => ceilingUltraSetting.WallRefillDash = value));
        menu.Add(new TextMenu.OnOff("Vertical Hyper".ToDialogText(), ceilingUltraSetting.VerticalHyperEnabled).Change(value => ceilingUltraSetting.VerticalHyperEnabled = value));
        menu.Add(new TextMenu.OnOff("Vertical Ultra".ToDialogText(), ceilingUltraSetting.VerticalUltraEnabled).Change(value => ceilingUltraSetting.VerticalUltraEnabled = value));
        menu.Add(new TextMenu.OnOff("Dash Begin No Vertical Speed Loss".ToDialogText(), ceilingUltraSetting.DashBeginNoVerticalSpeedLoss).Change(value => ceilingUltraSetting.DashBeginNoVerticalSpeedLoss = value));
        menu.Add(new TextMenu.OnOff("Updiag Dash End No Vertical Speed Loss".ToDialogText(), ceilingUltraSetting.UpdiagDashEndNoVerticalSpeedLoss).Change(value => ceilingUltraSetting.UpdiagDashEndNoVerticalSpeedLoss = value));

    }

}

public static class DialogExtension {

    internal static string ToDialogText(this string input) => Dialog.Clean("CEILING_ULTRA_" + input.ToUpper().Replace(" ", "_"));
}