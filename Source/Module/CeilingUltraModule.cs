using FMOD.Studio;
using Microsoft.Xna.Framework;
using static Celeste.TextMenu;

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
        
        menu.Add(new OnOffExt("Enabled".ToDialogText(), LevelSettings.MainEnabled, overrideMain).Change(value => ceilingUltraSetting.Enabled = value));


        menu.Add(new TextMenuExt.SubHeaderExt("Ceiling Mechanisms".ToDialogText()) { HeightExtra = 10f});


        menu.Add(new OnOffExt("Ceiling Refill Stamina".ToDialogText(), LevelSettings.CeilingRefillStamina, overrideCeilRefill).Change(value => ceilingUltraSetting.CeilingRefillStamina = value));
        menu.Add(new OnOffExt("Ceiling Refill Dash".ToDialogText(), LevelSettings.CeilingRefillDash, overrideCeilRefill).Change(value => ceilingUltraSetting.CeilingRefillDash = value));


        menu.Add(new OnOffExt("Ceiling Jump".ToDialogText(), LevelSettings.CeilingJumpEnabled, overrideCeilTech).Change(value => ceilingUltraSetting.CeilingJumpEnabled = value));
        menu.Add(new OnOffExt("Ceiling Super Hyper".ToDialogText(), LevelSettings.CeilingHyperEnabled, overrideCeilTech).Change(value => ceilingUltraSetting.CeilingHyperEnabled = value));
        menu.Add(new OnOffExt("Ceiling Ultra".ToDialogText(), LevelSettings.CeilingUltraEnabled, overrideCeilTech).Change(value => ceilingUltraSetting.CeilingUltraEnabled = value));
        menu.Add(new OnOffExt("Updiag Dash End No Horizontal Speed Loss".ToDialogText(), LevelSettings.UpdiagDashEndNoHorizontalSpeedLoss, overrideCeilTech).Change(value => ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss = value));


        menu.Add(new TextMenuExt.SubHeaderExt("Vertical Mechanisms".ToDialogText()) { HeightExtra = 10f});


        menu.Add(new OnOffExt("Wall Refill Stamina".ToDialogText(), LevelSettings.WallRefillStamina, overrideWallRefill).Change(value => ceilingUltraSetting.WallRefillStamina = value));
        menu.Add(new OnOffExt("Wall Refill Dash".ToDialogText(), LevelSettings.WallRefillDash, overrideWallRefill).Change(value => ceilingUltraSetting.WallRefillDash = value));


        menu.Add(new OnOffExt("Vertical Hyper".ToDialogText(), LevelSettings.VerticalHyperEnabled, overrideWallRefill).Change(value => ceilingUltraSetting.VerticalHyperEnabled = value));
        menu.Add(new OnOffExt("Vertical Ultra".ToDialogText(), LevelSettings.VerticalUltraEnabled, overrideWallRefill).Change(value => ceilingUltraSetting.VerticalUltraEnabled = value));
        menu.Add(new OnOffExt("Dash Begin No Vertical Speed Loss".ToDialogText(), LevelSettings.DashBeginNoVerticalSpeedLoss, overrideWallRefill).Change(value => ceilingUltraSetting.DashBeginNoVerticalSpeedLoss = value));
        menu.Add(new OnOffExt("Updiag Dash End No Vertical Speed Loss".ToDialogText(), LevelSettings.UpdiagDashEndNoVerticalSpeedLoss, overrideWallRefill).Change(value => ceilingUltraSetting.UpdiagDashEndNoVerticalSpeedLoss = value));

        if (overrideMain || overrideCeilRefill || overrideCeilTech || overrideVertTech || overrideWallRefill) {
            menu.Add(new TextMenuExt.SubHeaderExt("Lock By Map".ToDialogText()) { TextColor = Color.Goldenrod, HeightExtra = 10f });
        }
    }

}

public static class DialogExtension {

    internal static string ToDialogText(this string input) => Dialog.Clean("CEILING_ULTRA_" + input.ToUpper().Replace(" ", "_"));
}

public class OnOffExt : Item {

    

    //
    // 摘要:
    //     The displayed name for this setting.
    public string Label;

    //
    // 摘要:
    //     The index of the current selection in Celeste.TextMenu.Option`1.Values.
    public int Index;

    //
    // 摘要:
    //     Invoked when the value changes.
    public Action<bool> OnValueChange;

    //
    // 摘要:
    //     The previously selected index.
    public int PreviousIndex;

    //
    // 摘要:
    //     The list of label/value pairs.
    public List<Tuple<string, bool>> Values;

    public float sine;

    public int lastDir;

    public float cachedRightWidth = 0f;

    public List<string> cachedRightWidthContent = new List<string>();

    //
    // 摘要:
    //     The color the text takes when the option is active, but unselected (defaults
    //     to white).
    public Color UnselectedColor = Color.White;

    //
    // 摘要:
    //     Create a new Celeste.TextMenu.Option`1.
    //
    // 参数:
    //   label:
    //     The display name for this setting.
    public OnOffExt(string label): base() {
        Values = new List<Tuple<string, bool>>();
        Label = label;
        Selectable = true;
    }

    public OnOffExt(string label, bool on, bool disabled) : base() {
        Values = new List<Tuple<string, bool>>();
        Label = label;
        Selectable = true;
        this.disabled = disabled;
        Add(Dialog.Clean("options_off"), value: false, !on);
        Add(Dialog.Clean("options_on"), value: true, on);
    }

    public bool disabled = false;

    //
    // 摘要:
    //     Add an option.
    //
    // 参数:
    //   label:
    //     The display text for this option.
    //
    //   value:
    //     The T value of this option.
    //
    //   selected:
    //     Whether this option should start selected.
    public OnOffExt Add(string label, bool value, bool selected = false) {
        Values.Add(new Tuple<string, bool>(label, value));
        if (selected) {
            PreviousIndex = (Index = Values.Count - 1);
        }

        return this;
    }

    //
    // 摘要:
    //     Set the action that will be invoked when the value changes.
    //
    // 参数:
    //   action:
    public OnOffExt Change(Action<bool> action) {
        OnValueChange = action;
        return this;
    }

    public override void Added() {
        Container.InnerContent = InnerContentMode.TwoColumn;
    }

    public override void LeftPressed() {
        if (disabled) {
            Audio.Play("event:/ui/game/chatoptions_select");
            return;
        }
        if (Index > 0) {
            Audio.Play("event:/ui/main/button_toggle_off");
            PreviousIndex = Index;
            Index--;
            lastDir = -1;
            ValueWiggler.Start();
            if (OnValueChange != null) {
                OnValueChange(Values[Index].Item2);
            }
        }
    }

    public override void RightPressed() {
        if (disabled) {
            Audio.Play("event:/ui/game/chatoptions_select");
            return;
        }
        if (Index < Values.Count - 1) {
            Audio.Play("event:/ui/main/button_toggle_on");
            PreviousIndex = Index;
            Index++;
            lastDir = 1;
            ValueWiggler.Start();
            if (OnValueChange != null) {
                OnValueChange(Values[Index].Item2);
            }
        }
    }

    public override void ConfirmPressed() {
        if (disabled) {
            Audio.Play("event:/ui/game/chatoptions_select");
            return;
        }
        if (Values.Count == 2) {
            if (Index == 0) {
                Audio.Play("event:/ui/main/button_toggle_on");
            }
            else {
                Audio.Play("event:/ui/main/button_toggle_off");
            }

            PreviousIndex = Index;
            Index = 1 - Index;
            lastDir = ((Index == 1) ? 1 : (-1));
            ValueWiggler.Start();
            if (OnValueChange != null) {
                OnValueChange(Values[Index].Item2);
            }
        }
    }

    public override void Update() {
        sine += Monocle.Engine.RawDeltaTime;
    }

    public override float LeftWidth() {
        return ActiveFont.Measure(Label).X + 32f;
    }

    public override float RightWidth() {
        List<string> second = Values.Select((Tuple<string, bool> val) => val.Item1).ToList();
        if (!cachedRightWidthContent.SequenceEqual(second)) {
            cachedRightWidth = orig_RightWidth() * 0.8f + 44f;
            cachedRightWidthContent = second;
        }

        return cachedRightWidth;
    }

    public override float Height() {
        return ActiveFont.LineHeight;
    }

    public override void Render(Vector2 position, bool highlighted) {
        float alpha = Container.Alpha;
        Color strokeColor = Color.Black * (alpha * alpha * alpha);
        Color color = (Disabled || disabled ? (highlighted ? Color.SlateGray : Color.DarkSlateGray) : ((highlighted ? Container.HighlightColor : UnselectedColor) * alpha));
        ActiveFont.DrawOutline(Label, position, new Vector2(0f, 0.5f), Vector2.One, color, 2f, strokeColor);
        if (Values.Count > 0) {
            float num = RightWidth();
            ActiveFont.DrawOutline(Values[Index].Item1, position + new Vector2(Container.Width - num * 0.5f + (float)lastDir * ValueWiggler.Value * 8f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.8f, color, 2f, strokeColor);
            Vector2 vector = Vector2.UnitX * (highlighted ? ((float)Math.Sin(sine * 4f) * 4f) : 0f);
            bool flag = Index > 0;
            Color color2 = (flag ? color : (Color.DarkSlateGray * alpha));
            Vector2 position2 = position + new Vector2(Container.Width - num + 40f + ((lastDir < 0) ? ((0f - ValueWiggler.Value) * 8f) : 0f), 0f) - (flag ? vector : Vector2.Zero);
            ActiveFont.DrawOutline("<", position2, new Vector2(0.5f, 0.5f), Vector2.One, color2, 2f, strokeColor);
            bool flag2 = Index < Values.Count - 1;
            Color color3 = (flag2 ? color : (Color.DarkSlateGray * alpha));
            Vector2 position3 = position + new Vector2(Container.Width - 40f + ((lastDir > 0) ? (ValueWiggler.Value * 8f) : 0f), 0f) + (flag2 ? vector : Vector2.Zero);
            ActiveFont.DrawOutline(">", position3, new Vector2(0.5f, 0.5f), Vector2.One, color3, 2f, strokeColor);
        }
    }


    public float orig_RightWidth() {
        float num = 0f;
        foreach (Tuple<string, bool> value in Values) {
            num = Math.Max(num, ActiveFont.Measure(value.Item1).X);
        }

        return num + 120f;
    }
}