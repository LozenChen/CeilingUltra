using Celeste.Mod.CeilingUltra.Utils;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class TasInfo {

    [Initialize]

    private static void Initialize() {
        if (ModUtils.GetType("CelesteTAS", "TAS.GameInfo") is { } gameInfo && gameInfo.GetMethodInfo("GetStatuses") is { } method) {
            method.IlHook(il => {
                ILCursor cursor = new ILCursor(il);
                cursor.Index = -1;
                bool success = true;
                if (cursor.TryGotoPrev(ins => ins.MatchLdstr(" "), ins => ins.OpCode == OpCodes.Ldloc_1)) {
                    cursor.MoveAfterLabels();
                    cursor.Emit(OpCodes.Ldloc_1);
                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.Emit(OpCodes.Ldarg_1);
                    cursor.EmitDelegate(GetStatuses);
                }
                else {
                    success = false;
                }
                "TAS.GameInfo.GetStatuses".LogHookData("Build CeilingUltra TasInfo", success);
            });
        }
    }

    public static void GetStatuses(List<string> list, Level level, Player player) {
        if (!level.Transitioning) {
            if (player.IsSqueezed()) {
                list.Add("Squeezed");
            }
            if (player.InControl && level.unpauseTimer <= 0f) {
                if (CeilingTechMechanism.CeilingJumpGraceTimer.ToFloorFrames() is var ceilingCoyote and > 0) {
                    list.Add($"CeilingCoyote({ceilingCoyote})");
                }
                if (CeilingTechMechanism.LeftWallGraceTimer.ToFloorFrames() is var leftCoyote and > 0) {
                    list.Add($"LeftWallCoyote({leftCoyote})");
                }
                if (CeilingTechMechanism.RightWallGraceTimer.ToFloorFrames() is var rightCoyote and > 0) {
                    list.Add($"RightWallCoyote({rightCoyote})");
                }
            }

        }
    }

    private static int ToFloorFrames(this float seconds) {
        return (int)Math.Floor(seconds / Engine.RawDeltaTime / Engine.TimeRateB);
    }
}