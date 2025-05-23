using Celeste.Mod.CeilingUltra.Gameplay;
using Celeste.Mod.Helpers;
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.CeilingUltra.Utils;

// completely taken from Celeste TAS
internal static class ModUtils {
    public static readonly Assembly VanillaAssembly = typeof(Player).Assembly;

#pragma warning disable CS8603
    public static Type GetType(string modName, string name, bool throwOnError = false, bool ignoreCase = false) {
        return GetAssembly(modName)?.GetType(name, throwOnError, ignoreCase);
    }
    // check here if you dont know what's the correct name for a nested type / generic type
    // https://learn.microsoft.com/zh-cn/dotnet/framework/reflection-and-codedom/specifying-fully-qualified-type-names

    public static Type GetType(string name, bool throwOnError = false, bool ignoreCase = false) {
        return FakeAssembly.GetFakeEntryAssembly().GetType(name, throwOnError, ignoreCase);
    }

    public static Type[] GetTypes() {
        return FakeAssembly.GetFakeEntryAssembly().GetTypes();
    }

    public static EverestModule GetModule(string modName) {
        return Everest.Modules.FirstOrDefault(module => module.Metadata?.Name == modName);
    }

    public static bool IsInstalled(string modName) {
        return GetModule(modName) != null;
    }

    public static Assembly GetAssembly(string modName) {
        return GetModule(modName)?.GetType().Assembly;
    }
#pragma warning restore CS8603

    public static bool ExtendedVariantInstalled = false;

    public static bool SpeedrunToolInstalled = false;

    public static bool GravityHelperInstalled = false;
    public static bool UpsideDown => ExtendedVariantsUtils.UpsideDown;

    [InitializeAtFirst]
    public static void InitializeAtFirst() {
        ExtendedVariantInstalled = IsInstalled("ExtendedVariantMode");
        SpeedrunToolInstalled = IsInstalled("SpeedrunTool");
        GravityHelperInstalled = IsInstalled("GravityHelper");
    }


    internal static class ExtendedVariantsUtils {
        private static readonly Lazy<EverestModule> module = new(() => ModUtils.GetModule("ExtendedVariantMode"));
        private static readonly Lazy<object> triggerManager = new(() => module.Value?.GetFieldValue<object>("TriggerManager"));

        private static readonly Lazy<FastReflectionDelegate> getCurrentVariantValue = new(() =>
            triggerManager.Value?.GetType().GetMethodInfo("GetCurrentVariantValue")?.GetFastDelegate());

        private static readonly Lazy<Type> variantType =
            new(() => module.Value?.GetType().Assembly.GetType("ExtendedVariants.Module.ExtendedVariantsModule+Variant"));

        // enum value might be different between different ExtendedVariantMode version, so we have to parse from string
        private static readonly Lazy<object> upsideDownVariant = new(ParseVariant("UpsideDown"));

        private static readonly Lazy<object> ultraJumpVariant = new(ParseVariant("EveryJumpIsUltra"));

        private static readonly Lazy<object> ultraSpeedMultiplier = new(ParseVariant("UltraSpeedMultiplier"));

        public static Func<object> ParseVariant(string value) {
            return () => {
                try {
                    return variantType.Value == null ? null : Enum.Parse(variantType.Value, value);
                }
                catch (Exception e) {
                    return null;
                }
            };
        }

        public static bool UpsideDown => GetCurrentVariantValue(upsideDownVariant) is { } value && (bool)value;

        public static bool UltraJumpMode => GetCurrentVariantValue(ultraJumpVariant) is { } value && (bool)value;

        public static object GetCurrentVariantValue(Lazy<object> variant) {
            if (variant.Value is null) return null;
            return getCurrentVariantValue.Value?.Invoke(triggerManager.Value, variant.Value);
        }

        public static void TryCeilingUltraJump(Player self, int dir) {
            if (UltraJumpMode) {
                self.DashDir.X = Math.Sign(self.DashDir.X);
                self.DashDir.Y = 0f;
                self.Speed.X *= (float)(GetCurrentVariantValue(ultraSpeedMultiplier) ?? 1.2f);
                self.TryCeilingDuck(dir);
            }
        }

        public static void TryVerticalUltraJump(Player self, int dir) {
            if (UltraJumpMode) {
                self.TrySqueezeHitbox(-dir, self.Speed.Y);
                self.DashDir.Y = Math.Sign(self.DashDir.Y);
                self.DashDir.X = 0f;
                self.Speed.Y *= (float)(GetCurrentVariantValue(ultraSpeedMultiplier) ?? 1.2f);
            }
        }
    }
}