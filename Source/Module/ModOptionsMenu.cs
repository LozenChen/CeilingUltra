using Celeste.Mod.CeilingUltra.Utils;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using static Celeste.TextMenu;

namespace Celeste.Mod.CeilingUltra.Module;
internal static class ModOptionsMenu {

    private static TextMenu.Item indexer;
    public static void CreateMenu(EverestModule everestModule, TextMenu menu, bool inGame, bool inPauseMenu = false) {
        if (!inGame) {
            // if you dont exit map normally (e.g. use a titlescreen command, then session still exists)
            LevelSettings.ClearAllOverride();
        }
        bool overrideMain = LevelSettings.OverrideMainEnabled.HasValue;
        TextMenu.Item mainItem = new OnOffExt("Enabled".ToDialogText(), LevelSettings.MainEnabled, overrideMain).Change(value => { ceilingUltraSetting.Enabled = value; UpdateEnableItems(value, true, everestModule, menu, inGame); });
        TextMenu.Item showInPauseMenu = new OnOffExt("Show In Pause Menu".ToDialogText(), ceilingUltraSetting.ShowInPauseMenu, false).Change(value => {
            ceilingUltraSetting.ShowInPauseMenu = value;
            HookPauseMenu.OnShowInPauseMenuChange(value);
        });
        if (inPauseMenu) {
            menu.Add(new TextMenuExt.SubHeaderExt("") { HeightExtra = 20f });
            menu.Add(indexer = mainItem);
            UpdateEnableItems(LevelSettings.MainEnabled, false, everestModule, menu, inGame);
            menu.Add(new TextMenuExt.SubHeaderExt("") { HeightExtra = 40f });
            menu.Add(showInPauseMenu);
        }
        else {
            menu.Add(mainItem);
            menu.Add(indexer = showInPauseMenu);
            UpdateEnableItems(LevelSettings.MainEnabled, false, everestModule, menu, inGame);
        }
        UpdateEnableItems(LevelSettings.MainEnabled, false, everestModule, menu, inGame);
        menu.OnClose += () => disabledItems.Clear();
    }

    private static void UpdateEnableItems(bool enable, bool fromChange, EverestModule everestModule, TextMenu menu, bool inGame) {
        if (enable) {
            foreach (TextMenu.Item item in disabledItems) {
                menu.Remove(item);
            }
            disabledItems = new List<TextMenu.Item>();

            bool overrideMain = LevelSettings.OverrideMainEnabled.HasValue;
            bool overrideCeilTech = LevelSettings.OverrideCeilingTech.HasValue;
            bool overrideCeilRefill = LevelSettings.OverrideCeilingRefill.HasValue;
            bool overrideUpdiagEnd = LevelSettings.OverrideBigInertiaUpdiagDash.HasValue;
            bool overrideWallRefill = LevelSettings.OverrideWallRefill.HasValue;
            bool overrideVertTech = LevelSettings.OverrideVerticalTech.HasValue;
            bool overrideUpWallJumpAcc = LevelSettings.OverrideUpwardWallJumpAcceleration.HasValue;
            bool overrideDownWallJumpAcc = LevelSettings.OverrideDownwardWallJumpAcceleration.HasValue;
            bool overrideGroundTech = LevelSettings.OverrideGroundTech.HasValue;
            bool overrideQoL = LevelSettings.OverrideQoL.HasValue;


            Add(new EaseInSubHeader("Ceiling Mechanisms".ToDialogText()) { HeightExtra = 30f });


            Add(new EaseInOnOffExt("Ceiling Refill Stamina".ToDialogText(), LevelSettings.CeilingRefillStamina, overrideCeilRefill).Change(value => ceilingUltraSetting.CeilingRefillStamina = value));
            Add(new EaseInOnOffExt("Ceiling Refill Dash".ToDialogText(), LevelSettings.CeilingRefillDash, overrideCeilRefill).Change(value => ceilingUltraSetting.CeilingRefillDash = value));


            Add(new EaseInOnOffExt("Ceiling Jump".ToDialogText(), LevelSettings.CeilingJumpEnabled, overrideCeilTech).Change(value => ceilingUltraSetting.CeilingJumpEnabled = value));
            Add(new EaseInOnOffExt("Ceiling Super Hyper".ToDialogText(), LevelSettings.CeilingHyperEnabled, overrideCeilTech).Change(value => ceilingUltraSetting.CeilingHyperEnabled = value));
            Add(new EaseInOnOffExt("Ceiling Ultra".ToDialogText(), LevelSettings.CeilingUltraEnabled, overrideCeilTech).Change(value => ceilingUltraSetting.CeilingUltraEnabled = value));


            Add(new EaseInOnOffExt("Updiag Dash End No Horizontal Speed Loss".ToDialogText(), LevelSettings.UpdiagDashEndNoHorizontalSpeedLoss, overrideUpdiagEnd).Change(value => ceilingUltraSetting.UpdiagDashEndNoHorizontalSpeedLoss = value));


            Add(new EaseInSubHeader("Vertical Mechanisms".ToDialogText()) { HeightExtra = 30f });


            Add(new EaseInOnOffExt("Wall Refill Stamina".ToDialogText(), LevelSettings.WallRefillStamina, overrideWallRefill).Change(value => ceilingUltraSetting.WallRefillStamina = value));
            Add(new EaseInOnOffExt("Wall Refill Dash".ToDialogText(), LevelSettings.WallRefillDash, overrideWallRefill).Change(value => ceilingUltraSetting.WallRefillDash = value));


            Add(new EaseInOnOffExt("Vertical Hyper".ToDialogText(), LevelSettings.VerticalHyperEnabled, overrideVertTech).Change(value => ceilingUltraSetting.VerticalHyperEnabled = value));
            Add(new EaseInOnOffExt("Vertical Ultra".ToDialogText(), LevelSettings.VerticalUltraEnabled, overrideVertTech).Change(value => ceilingUltraSetting.VerticalUltraEnabled = value));
            Add(new EaseInOnOffExt("Dash Begin No Vertical Speed Loss".ToDialogText(), LevelSettings.DashBeginNoVerticalSpeedLoss, overrideVertTech).Change(value => ceilingUltraSetting.DashBeginNoVerticalSpeedLoss = value));
            Add(new EaseInOnOffExt("Updiag Dash End No Vertical Speed Loss".ToDialogText(), LevelSettings.UpdiagDashEndNoVerticalSpeedLoss, overrideVertTech).Change(value => ceilingUltraSetting.UpdiagDashEndNoVerticalSpeedLoss = value));


            Add(new EaseInOnOffExt("Upward Wall Jump Acceleration".ToDialogText(), LevelSettings.UpwardWallJumpAcceleration, overrideUpWallJumpAcc).Change(value => ceilingUltraSetting.UpwardWallJumpAcceleration = value));
            Add(new EaseInOnOffExt("Downward Wall Jump Acceleration".ToDialogText(), LevelSettings.DownwardWallJumpAcceleration, overrideDownWallJumpAcc).Change(value => ceilingUltraSetting.DownwardWallJumpAcceleration = value));


            Add(new EaseInSubHeader("Ground Mechanisms".ToDialogText()) { HeightExtra = 30f });

            Add(new EaseInOnOffExt("Ground Jump".ToDialogText(), LevelSettings.GroundJumpEnabled, overrideGroundTech).Change(value => ceilingUltraSetting.GroundJumpEnabled = value));
            Add(new EaseInOnOffExt("Ground Super Hyper".ToDialogText(), LevelSettings.GroundHyperEnabled, overrideGroundTech).Change(value => ceilingUltraSetting.GroundHyperEnabled = value));
            Add(new EaseInOnOffExt("Ground Ultra".ToDialogText(), LevelSettings.GroundUltraEnabled, overrideGroundTech).Change(value => ceilingUltraSetting.GroundUltraEnabled = value));


            Add(new EaseInSubHeader("QoL".ToDialogText()) { HeightExtra = 30f });

            Add(new EaseInOnOffExt("Buffered Vertical Hyper".ToDialogText(), LevelSettings.QoLBufferVerticalHyper, overrideQoL).Change(value => ceilingUltraSetting.QoLBufferVerticalHyper = value)); // actually not buffered but stops wall jump when dash
            Add(new EaseInOnOffExt("Buffered Vertical Ultra".ToDialogText(), LevelSettings.QoLBufferVerticalUltra, overrideQoL).Change(value => ceilingUltraSetting.QoLBufferVerticalUltra = value)); // stops some wall jump when in normal update
            TextMenu.Item QoL_RefillDashOnWallJump;
            Add(QoL_RefillDashOnWallJump = new EaseInOnOffExt("Refill Dash on Wall Jump".ToDialogText(), LevelSettings.QoLRefillDashOnWallJump, overrideQoL).Change(value => ceilingUltraSetting.QoLRefillDashOnWallJump = value)); // refill dash on wall jump even if you are not adjacent to wall
            Add(CreateDescription(menu, QoL_RefillDashOnWallJump, "Refill Dash On Wall Jump Description".ToDialogText()));

            if (overrideMain || overrideCeilRefill || overrideCeilTech || overrideVertTech || overrideWallRefill || overrideUpdiagEnd || overrideUpWallJumpAcc || overrideDownWallJumpAcc || overrideGroundTech || overrideQoL) {
                Add(new EaseInSubHeader("Lock By Map".ToDialogText()) { TextColor = Color.Goldenrod, HeightExtra = 10f });
            }

            int index = menu.IndexOf(indexer);

            foreach (TextMenu.Item item in disabledItems) {
                index++;
                menu.Insert(index, item);
            }

            foreach (IEaseInItem item in disabledItems) {
                item.Initialize();
            }
        }
        else {
            foreach (IEaseInItem item in disabledItems) {
                item.FadeVisible = false;
            }
            if (!fromChange && LevelSettings.OverrideMainEnabled.HasValue) {
                TextMenu.Item item = new EaseInSubHeader("Lock By Map".ToDialogText()) { TextColor = Color.Goldenrod, HeightExtra = 10f };
                disabledItems.Add(item);
                menu.Insert(menu.IndexOf(indexer) + 1, item);
            }
        }

        void Add(TextMenu.Item item) {
            disabledItems.Add(item);
        }
    }

    private static List<TextMenu.Item> disabledItems = new();

    private static EaseInSubHeader CreateDescription(TextMenu containingMenu, TextMenu.Item subMenuItem, string description) {
        EaseInSubHeader descriptionText = new(description, false) {
            TextColor = Color.Gray,
            HeightExtra = 0f
        };
        subMenuItem.OnEnter += () => descriptionText.FadeVisible = true;
        subMenuItem.OnLeave += () => descriptionText.FadeVisible = false;
        return descriptionText;
    }
}

internal static class HookPauseMenu {
    // basically same as what vanilla variants menu do

    private static TextMenu.Item itemInPauseMenu;

    [Initialize]
    private static void Initialize() {
        using (new DetourContext() { Before = new List<string>() { "*" } }) {
            typeof(Level).GetMethodInfo("Pause").IlHook(il => {
                ILCursor cursor = new ILCursor(il);
                if (cursor.TryGotoNext(ins => ins.MatchCall(typeof(Everest.Events.Level), "CreatePauseMenuButtons"))) {
                    cursor.Index++;
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldloc_0);
                    cursor.Emit(OpCodes.Ldarg_2);
                    cursor.EmitDelegate(TryAddButton);
                }
            });
        }
        // idk, if using Everest.Events.Level.OnCreatePauseMenuButtons, first i will encounter CS0229, after resolving this i find that my function adds to this event but nothing happens
        // so i go back to ilhook
        // previously i just hook into Celeste.Mod.Everest/Events/Level::CreatePauseMenuButtons
        // but it conflicts with XaphanHelper's on hook on Level.Pause (i.e. my hook just disappear), i've checked that mod's code and found no issue
        // even if i tell the publicizer not to publicize Everest.Events.Level.OnCreatePauseMenuButtons, it does not work, unless i don't publicize Celeste
    }


    private static void TryAddButton(Level level, TextMenu menu, bool minimal) {
        if (minimal || !ceilingUltraSetting.ShowInPauseMenu) {
            return;
        }
        // yeah we are after extended variant

        int optionsIndex = menu.Items.FindIndex(item =>
            item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("menu_pause_options"));

        menu.Insert(optionsIndex, itemInPauseMenu = new TextMenu.Button("PauseMenu CU Variant".ToDialogText()).Pressed(() => {
            menu.RemoveSelf();
            level.PauseMainMenuOpen = false;
            level.CeilingUltraVariant(menu.IndexOf(itemInPauseMenu));
        }));
    }

    private static void CeilingUltraVariant(this Level level, int returnIndex) {
        level.Paused = true;
        TextMenu menu = new TextMenu();
        menu.Add(new HeaderExt("CU Variant Title".ToDialogText(), Color.Silver, Color.Black));
        ModOptionsMenu.CreateMenu(CeilingUltraModule.Instance, menu, true, true);

        menu.OnESC = menu.OnCancel = () => {
            Audio.Play("event:/ui/main/button_back");
            CeilingUltraModule.Instance.SaveSettings();
            level.Pause(returnIndex, false);
            menu.Close();
        };
        menu.OnPause = () => {
            Audio.Play("event:/ui/main/button_back");
            CeilingUltraModule.Instance.SaveSettings();
            level.Paused = false;
            level.unpauseTimer = 0.15f;
            menu.Close();
        };
        level.Add(menu);
    }

    internal static void OnShowInPauseMenuChange(bool visible) {
        if (itemInPauseMenu?.Container is { }) {
            itemInPauseMenu.Visible = visible;
        }
    }
}


public static class DialogExtension {
    internal static string ToDialogText(this string input) => Dialog.Clean("CEILING_ULTRA_" + input.ToUpper().Replace(" ", "_"));
}

public interface IEaseInItem {
    public void Initialize();
    public bool FadeVisible { get; set; }
}

public class HeaderExt : Item {
    public const float Scale = 2f;

    public string Title;

    public Color TextColor;

    public Color StrokeColor;
    public HeaderExt(string title, Color text, Color stroke) {
        Title = title;
        Selectable = false;
        IncludeWidthInMeasurement = false;
        TextColor = text;
        StrokeColor = stroke;
    }

    public override float LeftWidth() {
        return ActiveFont.Measure(Title).X * 2f;
    }

    public override float Height() {
        return ActiveFont.LineHeight * 2f;
    }

    public override void Render(Vector2 position, bool highlighted) {
        float alpha = Container.Alpha;
        ActiveFont.DrawEdgeOutline(Title, position + new Vector2(Container.Width * 0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, TextColor * alpha, 4f, Color.DarkSlateBlue * alpha, 2f, StrokeColor * (alpha * alpha * alpha));
    }
}

public class EaseInSubHeader : TextMenuExt.SubHeaderExt, IEaseInItem {
    private float alpha;
    private float unEasedAlpha;
    public bool initVisible = true;

    public void Initialize() {
        alpha = unEasedAlpha = 0f;
        Visible = FadeVisible = initVisible;
    }
    public bool FadeVisible { get; set; }
    public EaseInSubHeader(string label, bool initVisible = true) : base(label) {
        alpha = unEasedAlpha = 1f;
        this.initVisible = initVisible;
        FadeVisible = Visible = initVisible;
    }

    public override float Height() => MathHelper.Lerp(-Container.ItemSpacing, base.Height(), alpha);

    public override void Update() {
        base.Update();

        float targetAlpha = FadeVisible ? 1 : 0;
        if (Math.Abs(unEasedAlpha - targetAlpha) > 0.001f) {
            unEasedAlpha = Calc.Approach(unEasedAlpha, targetAlpha, Engine.RawDeltaTime * 3f);
            alpha = FadeVisible ? Ease.SineOut(unEasedAlpha) : Ease.SineIn(unEasedAlpha);
        }

        Visible = alpha != 0;
    }

    public override void Render(Vector2 position, bool highlighted) {
        float c = Container.Alpha;
        Container.Alpha = alpha;
        base.Render(position, highlighted);
        Container.Alpha = c;
    }
}
public class EaseInOnOffExt : OnOffExt, IEaseInItem {
    private float alpha;
    private float unEasedAlpha;

    public void Initialize() {
        alpha = unEasedAlpha = 0f;
        Visible = FadeVisible = true;
    }
    public bool FadeVisible { get; set; }
    public EaseInOnOffExt(string label, bool on, bool disabled) : base(label, on, disabled) {
        alpha = unEasedAlpha = 1f;
        FadeVisible = Visible = true;
    }

    public override float Height() => MathHelper.Lerp(-Container.ItemSpacing, base.Height(), alpha);

    public override void Update() {
        base.Update();

        float targetAlpha = FadeVisible ? 1 : 0;
        if (Math.Abs(unEasedAlpha - targetAlpha) > 0.001f) {
            unEasedAlpha = Calc.Approach(unEasedAlpha, targetAlpha, Engine.RawDeltaTime * 3f);
            alpha = FadeVisible ? Ease.SineOut(unEasedAlpha) : Ease.SineIn(unEasedAlpha);
        }

        Visible = alpha != 0;
    }

    public override void Render(Vector2 position, bool highlighted) {
        float c = Container.Alpha;
        Container.Alpha = alpha;
        base.Render(position, highlighted);
        Container.Alpha = c;
    }
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
    public OnOffExt(string label) : base() {
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