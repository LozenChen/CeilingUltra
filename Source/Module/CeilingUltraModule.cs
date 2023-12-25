using FMOD.Studio;
using Celeste.Mod;
using Celeste;
using Microsoft.Xna.Framework;

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
        if (!inGame) {
            LevelSettings.ClearAllOverride();
        }
        bool overrideMain = LevelSettings.OverrideMainEnabled.HasValue;
        bool overrideCeilTech = LevelSettings.OverrideCeilingTech.HasValue;
        bool overrideCeilRefill = LevelSettings.OverrideCeilingRefill.HasValue;
        bool overrideWallRefill = LevelSettings.OverrideWallRefill.HasValue;
        bool overrideVertTech = LevelSettings.OverrideVerticalTech.HasValue;
        
        menu.Add(new TextMenu.OnOff("Enabled".ToDialogText(), LevelSettings.MainEnabled).Change(value => ceilingUltraSetting.Enabled = value).Apply(x => x.Disabled = overrideMain));


        menu.Add(new TextMenuExt.SubHeaderExt("Ceiling Mechanisms".ToDialogText()) { HeightExtra = 10f});


        menu.Add(new TextMenu.OnOff("Ceiling Refill Stamina".ToDialogText(), LevelSettings.CeilingRefillStamina).Change(value => ceilingUltraSetting.CeilingRefillStamina = value).Apply(x => x.Disabled = overrideCeilRefill));
        menu.Add(new TextMenu.OnOff("Ceiling Refill Dash".ToDialogText(), LevelSettings.CeilingRefillDash).Change(value => ceilingUltraSetting.CeilingRefillDash = value).Apply(x => x.Disabled = overrideCeilRefill));


        menu.Add(new TextMenu.OnOff("Ceiling Jump".ToDialogText(), LevelSettings.CeilingJumpEnabled).Change(value => ceilingUltraSetting.CeilingJumpEnabled = value).Apply(x => x.Disabled = overrideCeilTech));
        menu.Add(new TextMenu.OnOff("Ceiling Super Hyper".ToDialogText(), LevelSettings.CeilingHyperEnabled).Change(value => ceilingUltraSetting.CeilingHyperEnabled = value).Apply(x => x.Disabled = overrideCeilTech));
        menu.Add(new TextMenu.OnOff("Ceiling Ultra".ToDialogText(), LevelSettings.CeilingUltraEnabled).Change(value => ceilingUltraSetting.CeilingUltraEnabled = value).Apply(x => x.Disabled = overrideCeilTech));
        menu.Add(new TextMenu.OnOff("Updiag Dash End No Horizontal Speed Loss".ToDialogText(), LevelSettings.UpdiagDashEndNoHorizontalSpeedLoss).Change(value => ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss = value).Apply(x => x.Disabled = overrideCeilTech));


        menu.Add(new TextMenuExt.SubHeaderExt("Vertical Mechanisms".ToDialogText()) { HeightExtra = 10f});


        menu.Add(new TextMenu.OnOff("Wall Refill Stamina".ToDialogText(), LevelSettings.WallRefillStamina).Change(value => ceilingUltraSetting.WallRefillStamina = value).Apply(x => x.Disabled = overrideWallRefill));
        menu.Add(new TextMenu.OnOff("Wall Refill Dash".ToDialogText(), LevelSettings.WallRefillDash).Change(value => ceilingUltraSetting.WallRefillDash = value).Apply(x => x.Disabled = overrideWallRefill));


        menu.Add(new TextMenu.OnOff("Vertical Hyper".ToDialogText(), LevelSettings.VerticalHyperEnabled).Change(value => ceilingUltraSetting.VerticalHyperEnabled = value).Apply(x => x.Disabled = overrideVertTech));
        menu.Add(new TextMenu.OnOff("Vertical Ultra".ToDialogText(), LevelSettings.VerticalUltraEnabled).Change(value => ceilingUltraSetting.VerticalUltraEnabled = value).Apply(x => x.Disabled = overrideVertTech));
        menu.Add(new TextMenu.OnOff("Dash Begin No Vertical Speed Loss".ToDialogText(), LevelSettings.DashBeginNoVerticalSpeedLoss).Change(value => ceilingUltraSetting.DashBeginNoVerticalSpeedLoss = value).Apply(x => x.Disabled = overrideVertTech));
        menu.Add(new TextMenu.OnOff("Updiag Dash End No Vertical Speed Loss".ToDialogText(), LevelSettings.UpdiagDashEndNoVerticalSpeedLoss).Change(value => ceilingUltraSetting.UpdiagDashEndNoVerticalSpeedLoss = value).Apply(x => x.Disabled = overrideVertTech));

        if (overrideMain || overrideCeilRefill || overrideCeilTech || overrideVertTech || overrideWallRefill) {
            menu.Add(new TextMenuExt.SubHeaderExt("Lock By Map".ToDialogText()) { TextColor = Color.Goldenrod, HeightExtra = 10f }.Apply(x => x.Selectable = true));
        }
    }

}

public static class DialogExtension {

    internal static string ToDialogText(this string input) => Dialog.Clean("CEILING_ULTRA_" + input.ToUpper().Replace(" ", "_"));
}