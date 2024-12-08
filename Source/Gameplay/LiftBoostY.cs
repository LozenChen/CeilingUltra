namespace Celeste.Mod.CeilingUltra.Gameplay;

public static class LiftBoostY {

    // support positive liftboost (from extended variants)

    // it's hard to decide when we should apply the liftboost in opposite direction
    // i think liftboost should not point towards solids, so ceiling jump and ceiling hyper should not accept opposite liftboost
    // upward / downward jump is some kind of ... to leave the wall strongly, and btw it's intended to be a boost, so it should not
    // vertical hyper, this touches the wall for a long time, and it seams reasonable to have a opposite liftboost

    public static void OnCeilingJump(Player player) {
        if (player.LiftBoost.Y > 0f) {
            player.Speed.Y += player.LiftBoost.Y;
        }
    }

    public static void OnCeilingHyper(Player player) {
        if (player.LiftBoost.Y > 0f) {
            player.Speed.Y += player.LiftBoost.Y;
        }
    }

    public static void OnUpwardJump(Player player) {
        if (player.LiftBoost.Y < 0f) {
            player.Speed.Y += player.LiftBoost.Y;
        }
    }

    public static void OnDownwardJump(Player player) {
        if (player.LiftBoost.Y > 0f) {
            player.Speed.Y += player.LiftBoost.Y;
        }
    }

    public static void OnVerticalHyper(Player player) {
        player.Speed.Y += player.LiftBoost.Y; // even if that's not in same dir with your speed
    }
}