using MonoMod.ModInterop;

namespace Celeste.Mod.CeilingUltra.ModInterop;

public static class MaddieEntityNameRegistry {

    private static bool Installed = false;

    public static void RegisterSidewaysJumpThru(string name) {
        if (Installed) {
            MaddieEntityNameRegistryImport.RegisterSidewaysJumpThru(name);
        }
    }

    public static void RegisterUpsideDownJumpThru(string name) {
        if (Installed) {
            MaddieEntityNameRegistryImport.RegisterUpsideDownJumpThru(name);
        }
    }

    [InitializeAtFirst]
    public static void Initialize() {
        typeof(MaddieEntityNameRegistryImport).ModInterop();
        Installed = MaddieEntityNameRegistryImport.RegisterSidewaysJumpThru is not null;
    }
}

[ModImportName("MaddieHelpingHand/EntityIdRegistry")]
internal static class MaddieEntityNameRegistryImport {

    public static Action<string> RegisterSidewaysJumpThru;

    public static Action<string> RegisterUpsideDownJumpThru;
}