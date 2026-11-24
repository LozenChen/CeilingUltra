using Monocle;


namespace Celeste.Mod.CeilingUltra.Utils;

internal static class JumpthruClassifier {

    public static bool IsJumpThru(Entity entity) {
        return entity is JumpThru || JumpThruIsNotJumpThruTypes.Contains(entity.GetType());
    }

    public static List<Entity> GetJumpThrus() {
        // why we need this: SideWaysJumpThru is not a JumpThru, nor a Solid, not even a Platform

        if (Engine.Scene is not Level level) {
            return new List<Entity>();
        }

        List<Entity> results = new List<Entity>();
        results.AddRange(level.Tracker.GetEntities<JumpThru>());

        foreach (Type type in JumpThruIsNotJumpThruTypes) {
            if (level.Tracker.Entities.TryGetValue(type, out List<Entity> otherJumpthrus)) {
                results.AddRange(otherJumpthrus);
            }
            else {
                LevelExtensions.AddToTracker(type);
                level.Tracker.Entities.Add(type, new List<Entity>());
            }
        }

        return results.Distinct().ToList();
    }


    public static Direction GetDirection(Entity jumpThru) {
        // the direction in which the jumpThru is blocking you to go
        if (jumpThruDirections.TryGetValue(jumpThru, out Direction dir)) {
            return dir;
        }
        // we don't initialize this when load level, so we dont need to worry about SRT ...
        // okay but we will need re-init after save load
        // TODO: Support SRT
        InitializeDictionary();
        return jumpThruDirections[jumpThru];
        // it will throw exceptions if there are some jumpthrus we haven't covered
    }

    [Initialize]
    private static void Initialize() {
        // Celeste.Mod.GravityHelper.Entities.UpsideDownJumpThru -> Up

        // Celeste.Mod.JungleHelper.Entities.ClimbableOneWayPlatform -> No, but it's just an Entity so no need to treat it
        // (it's just one-way-climbable, but you can go through it from both sides)

        // Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru -> Left/Right, depends on AllowLeftToRight 
        // Celeste.Mod.MaxHelpingHand.Entities.AttachedSidewaysJumpThru -> Subclass of above
        // !!!! SideWaysJumpThru is not a subclass of JumpThru

        // Celeste.Mod.MaxHelpingHand.Entities.UpsideDownJumpThru -> Up
        // Celeste.Mod.MaxHelpingHand.Entities.UpsideDownMovingPlatform -> Subclass of above

        // VivHelper.Entities.HoldableBarrierJumpThru -> No

        jumpThruTypeDirections.Clear();
        jumpThruDirections.Clear();
        if (ModUtils.GetType("GravityHelper", "Celeste.Mod.GravityHelper.Entities.UpsideDownJumpThru") is { } gravity) {
            jumpThruTypeDirections[gravity] = Direction.Up;
        }
        if (ModUtils.GetType("MaxHelpingHand", "Celeste.Mod.MaxHelpingHand.Entities.UpsideDownJumpThru") is { } maxupsidedown) {
            jumpThruTypeDirections[maxupsidedown] = Direction.Up;
            JumpThruIsNotJumpThruTypes.Add(maxupsidedown);
            // maddie hooks Tracker.GetEntities<JumpThru>() so you can't get UpsideDownJumpThru when searching JumpThru
            // even though UpsideDownJumpThru is Tracked(false), "Celeste.Mod.MaxHelpingHand.Entities.UpsideDownMovingPlatform" is tracked as UpsideDownJumpThru, so everything is ok
        }
        if (ModUtils.GetType("VivHelper", "VivHelper.Entities.HoldableBarrierJumpThru") is { } viv) {
            jumpThruTypeDirections[viv] = Direction.Zero;
        }
        if (ModUtils.GetType("MaxHelpingHand", "Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru") is { } sideways) {
            jumpThruTypeDirections[sideways] = Direction.LeftRight;
            JumpThruIsNotJumpThruTypes.Add(sideways);
        }

        if (ModUtils.GetType("SpringCollab2020", "Celeste.Mod.SpringCollab2020.Entities.SidewaysJumpThru") is { } sc2020sideways) {
            jumpThruTypeDirections[sc2020sideways] = Direction.LeftRight;
            JumpThruIsNotJumpThruTypes.Add(sc2020sideways);
            // basically an early version of Maddie jumpthru
        }

        if (ModUtils.GetType("SpringCollab2020", "Celeste.Mod.SpringCollab2020.Entities.UpsideDownJumpThru") is { } sc2020upsidedown) {
            jumpThruTypeDirections[sc2020upsidedown] = Direction.Up;
            JumpThruIsNotJumpThruTypes.Add(sc2020upsidedown);
            // basically an early version of Maddie jumpthru
        }

        if (ModUtils.GetType("ChroniaHelper", "ChroniaHelper.Entities.SampleJumpThroughDown") is { } chroniadown) {
            jumpThruTypeDirections[chroniadown] = Direction.Up;
        }

        if (ModUtils.GetType("ChroniaHelper", "ChroniaHelper.Entities.SampleJumpThroughSideways") is { } chroniasideways) {
            jumpThruTypeDirections[chroniasideways] = Direction.LeftRight;
            JumpThruIsNotJumpThruTypes.Add(chroniasideways);
        }

        if (ModUtils.GetType("ChroniaHelper", "ChroniaHelper.Entities.SampleJumpThroughUp") is { } chroniaup) {
            jumpThruTypeDirections[chroniaup] = Direction.Down;
        }

        foreach (KeyValuePair<Type, Direction> tuple in jumpThruTypeDirections) {
            Type t = tuple.Key;
            Direction dir = tuple.Value;
            Logger.Debug($"CeilingUltra {nameof(JumpthruClassifier)}", $"Register: {t} -> {dir}");
        }
    }

    private static void InitializeDictionary() {
        jumpThruDirections.Clear();
        if (Engine.Scene is Level) {
            foreach (Entity jumpthru in GetJumpThrus()) {
                Type type = jumpthru.GetType();
                bool success = false;
                foreach (Type key in jumpThruTypeDirections.Keys) {
                    if (type.IsSameOrSubclassOf(key)) {
                        success = true;
                        Direction dir = jumpThruTypeDirections[key];
                        if (dir != Direction.LeftRight) {
                            jumpThruDirections[jumpthru] = dir;
                        }
                        else {
                            jumpThruDirections[jumpthru] = jumpthru.GetFieldValue<bool>("AllowLeftToRight") ? Direction.Left : Direction.Right;
                        }
                        break;
                    }
                }
                if (!success) {
                    jumpThruDirections[jumpthru] = Direction.Down;
                }
            }
        }
    }

    private static Dictionary<Type, Direction> jumpThruTypeDirections = new();

    private static Dictionary<Entity, Direction> jumpThruDirections = new();

    private static List<Type> JumpThruIsNotJumpThruTypes = new();


}