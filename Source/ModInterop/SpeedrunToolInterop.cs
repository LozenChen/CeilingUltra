using Celeste.Mod.CeilingUltra.Gameplay;
using Celeste.Mod.CeilingUltra.Utils;
using MonoMod.ModInterop;
using System.Reflection;

namespace Celeste.Mod.CeilingUltra.ModInterop;

[AttributeUsage(AttributeTargets.Field)]
internal class SaveLoadAttribute : Attribute {
    // used to tell SRT to save this field
}

internal static class SpeedrunToolInterop {

    public static bool SpeedrunToolInstalled;

    private static List<object> actions;

    [InitializeAtFirst]
    public static void Initialize() {
        typeof(SpeedrunToolImport).ModInterop();
        SpeedrunToolInstalled = SpeedrunToolImport.RegisterSaveLoadAction is not null;
        AddSaveLoadAction();
    }

    [Unload]
    public static void Unload() {
        RemoveSaveLoadAction();
    }

    private static void AddSaveLoadAction() {
        if (!SpeedrunToolInstalled) {
            return;
        }

        actions = new();

        Type[] types = new Type[] { typeof(CeilingTechMechanism), typeof(VerticalTechMechanism) };
        foreach (Type type in types) {
            List<FieldInfo> type_staticFields = new();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (FieldInfo fieldInfo in fieldInfos.Where(info => info.IsStatic && info.GetCustomAttribute<SaveLoadAttribute>() != null)) {
                type_staticFields.Add(fieldInfo);
            }
            if (type_staticFields.IsNotNullOrEmpty()) {
                object action = SpeedrunToolImport.RegisterStaticTypes(type, type_staticFields.Select(x => x.Name).ToArray());
                actions.Add(action);
            }
        }

        object handleSqueezed = SpeedrunToolImport.RegisterSaveLoadAction(
            (savedValues, level) => {
                if (savedValues.TryGetValue(typeof(VerticalTechMechanism), out Dictionary<string, object> dict)) {
                    dict["isSqueezed"] = level.GetPlayer()?.IsSqueezed() ?? false;
                }
                else {
                    Dictionary<string, object> dict2 = new Dictionary<string, object>();
                    dict2["isSqueezed"] = level.GetPlayer()?.IsSqueezed() ?? false;
                    savedValues[typeof(VerticalTechMechanism)] = dict2;
                }
            },
            (savedValues, level) => {
                if (savedValues.TryGetValue(typeof(VerticalTechMechanism), out Dictionary<string, object> dict) && dict.TryGetValue("isSqueezed", out object isSqueezed)) {
                    if ((bool)isSqueezed && level.GetPlayer() is { } player) {
                        player.SetSqueezedHitbox();
                    }
                }
            },
            null, null, null, null
        );
        actions.Add(handleSqueezed);
    }

    private static void RemoveSaveLoadAction() {
        if (SpeedrunToolInstalled) {
            foreach (object action in actions) {
                SpeedrunToolImport.Unregister(action);
            }
        }
    }
}

[ModImportName("SpeedrunTool.SaveLoad")]
internal static class SpeedrunToolImport {

    public static Func<Action<Dictionary<Type, Dictionary<string, object>>, Level>, Action<Dictionary<Type, Dictionary<string, object>>, Level>, Action, Action<Level>, Action<Level>, Action, object> RegisterSaveLoadAction;

    public static Func<Type, string[], object> RegisterStaticTypes;

    public static Action<object> Unregister;
}