using Celeste.Mod.CeilingUltra.Utils;
using MonoMod.Cil;

namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class ModCompatibility {

    [Initialize]
    public static void Initialize() {
        if (ModUtils.GetType("CommunalHelper", "Celeste.Mod.CommunalHelper.DashStates.DreamTunnelDash")?.GetMethodInfo("DreamTunnelDashEnd") is { } methodInfo) {
            methodInfo.IlHook(il => {
                ILCursor cursor = new ILCursor(il);
                cursor.Goto(-2);
                cursor.EmitDelegate(ClearCeilingJump);
            });
        }
        // this no longer work? coz implement change
    }

    private static void ClearCeilingJump() {
        CeilingTechMechanism.CeilingJumpGraceTimer = 0f;
    }
}