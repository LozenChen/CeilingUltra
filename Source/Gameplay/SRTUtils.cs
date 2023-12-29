using Celeste.Mod.CeilingUltra.Utils;
using System.Reflection;

namespace Celeste.Mod.CeilingUltra.Gameplay;

[AttributeUsage(AttributeTargets.Field)]
internal class SaveLoadAttribute : Attribute {
    // tell SRT to save this field
}
internal static class SRTUtils {

    [Initialize]

    private static void Initialize() {
        if (ModUtils.SpeedrunToolInstalled || ModUtils.IsInstalled("TASHelper")) {
            // session values are already handled well
            Type[] types = new Type[] { typeof(CeilingTechMechanism), typeof(VerticalTechMechanism) };
            foreach (Type type in types) {
                List<FieldInfo> type_staticFields = new();
                FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (FieldInfo fieldInfo in fieldInfos.Where(info => info.IsStatic && info.GetCustomAttribute<SaveLoadAttribute>() != null)) {
                    type_staticFields.Add(fieldInfo);
                }
                if (type_staticFields.IsNotNullOrEmpty()) {
                    staticFields[type] = type_staticFields;
                }
            }
        }

        if (ModUtils.GetType("SpeedrunTool", "Celeste.Mod.SpeedrunTool.SaveLoad.SaveLoadAction")?.GetMethodInfo("SafeAdd", new Type[] { typeof(Action<Dictionary<Type, Dictionary<string, object>>, Level>), typeof(Action<Dictionary<Type, Dictionary<string, object>>, Level>), typeof(Action), typeof(Action<Level>), typeof(Action<Level>), typeof(Action) }) is { } safeAddMethod) {
            safeAddMethod.Invoke(null, new object[] { saveState, loadState, clearState, beforeSaveState, beforeLoadState, preCloneEntities });
        }

        if (ModUtils.GetType("TASHelper", "Celeste.Mod.TASHelper.TinySRT.TH_SaveLoadAction")?.GetMethodInfo("SafeAdd", new Type[] { typeof(Action<Dictionary<Type, Dictionary<string, object>>, Level>), typeof(Action<Dictionary<Type, Dictionary<string, object>>, Level>), typeof(Action), typeof(Action<Level>), typeof(Action<Level>), typeof(Action) }) is { } TH_safeAddMethod) {
            TH_safeAddMethod.Invoke(null, new object[] { TH_saveState, TH_loadState, TH_clearState, beforeSaveState, beforeLoadState, preCloneEntities });
        }
    }

    private static Dictionary<Type, List<FieldInfo>> staticFields = new();

    private static Dictionary<Type, Dictionary<FieldInfo, object>> savedValues = new();

    private static Dictionary<Type, Dictionary<FieldInfo, object>> TH_savedValues = new();

    private static bool isSqueezed = false;

    private static bool TH_isSqueezed = false;

    private static readonly Action<Dictionary<Type, Dictionary<string, object>>, Level> saveState = (_, level) => {
        isSqueezed = level.GetPlayer()?.IsSqueezed() ?? false;
        foreach (Type type in staticFields.Keys) {
            savedValues[type] = new Dictionary<FieldInfo, object>();
            foreach (FieldInfo field in staticFields[type]) {
                savedValues[type][field] = field.GetValue(null);
            }
        }
    };

    private static readonly Action<Dictionary<Type, Dictionary<string, object>>, Level> loadState = (_, level) => {
        if (isSqueezed && level.GetPlayer() is { } player) {
            player.SetSqueezedHitbox();
        }
        foreach (Type type in staticFields.Keys) {
            Dictionary<FieldInfo, object> dict = savedValues[type];
            foreach (FieldInfo field in dict.Keys) {
                field.SetValue(null, dict[field]);
            }
        }
    };

    private static readonly Action<Dictionary<Type, Dictionary<string, object>>, Level> TH_saveState = (_, level) => {
        TH_isSqueezed = level.GetPlayer()?.IsSqueezed() ?? false;
        foreach (Type type in staticFields.Keys) {
            TH_savedValues[type] = new Dictionary<FieldInfo, object>();
            foreach (FieldInfo field in staticFields[type]) {
                TH_savedValues[type][field] = field.GetValue(null);
            }
        }
    };

    private static readonly Action<Dictionary<Type, Dictionary<string, object>>, Level> TH_loadState = (_, level) => {
        if (TH_isSqueezed && level.GetPlayer() is { } player) {
            player.SetSqueezedHitbox();
        }
        foreach (Type type in staticFields.Keys) {
            Dictionary<FieldInfo, object> dict = TH_savedValues[type];
            foreach (FieldInfo field in dict.Keys) {
                field.SetValue(null, dict[field]);
            }
        }
    };

    private static readonly Action clearState = savedValues.Clear;

    private static readonly Action TH_clearState = TH_savedValues.Clear;

    private static readonly Action<Level> beforeSaveState = null;

    private static readonly Action<Level> beforeLoadState = null;

    private static readonly Action preCloneEntities = null;

}