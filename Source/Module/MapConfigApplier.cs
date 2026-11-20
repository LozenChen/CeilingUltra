using System.Runtime.CompilerServices;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Module;

internal static class MapConfigApplier {

    private static readonly object AppliedMarker = new();
    private static ConditionalWeakTable<Session, object> appliedSessions;

    [Load]
    private static void Load() {
        appliedSessions = new();
        Everest.Events.Level.OnLoadLevel += OnLoadLevel;
    }

    [Unload]
    private static void Unload() {
        Everest.Events.Level.OnLoadLevel -= OnLoadLevel;
    }

    private static void OnLoadLevel(Level level, Player.IntroTypes introTypes, bool isFromLoader) {
        if (level?.Session is not { } session) {
            return;
        }
        TryApply(session);
    }

    private static bool TryApply(Session session) {
        if (session == null || appliedSessions.TryGetValue(session, out _)) {
            return true;
        }
        CeilingUltraSession modSession = CeilingUltraSession.Instance;
        if (modSession == null) {
            return false;
        }

        if (!modSession.MapConfigKnown) {
            modSession.MapConfigKnown = true;
            if (MapConfigValues.TryRead(session, out MapConfigValues? values)) {
                values.ApplyTo(modSession);
                Logger.Info("CeilingUltra", $"MapConfig found for {session.Area.GetSID()}, settings will be locked.");
            }
        }

        appliedSessions.Add(session, AppliedMarker);
        return true;
    }
}
