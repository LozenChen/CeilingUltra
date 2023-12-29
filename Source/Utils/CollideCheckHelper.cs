using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Utils;

public static class CollideCheckHelper {

    public static bool CanStand(this Entity entity, Vector2 direciton) {
        Vector2 orig_Position = entity.Position;
        entity.Position += direciton;
        bool result = false;
        foreach (Solid solid in entity.Scene.Tracker.GetEntities<Solid>()) {
            if (entity.CollideCheck(solid)) {
                result = true;
                break;
            }
        }
        if (!result && direciton != Vector2.Zero) {
            Direction expectedJumpThruDir = GetDir(direciton);
            foreach (Entity jumpthru in GetJumpThrus()) {
                if (jumpthru.GetDirection() == expectedJumpThruDir && entity.CollideCheck(jumpthru)) {
                    entity.Position = orig_Position;
                    if (!entity.CollideCheck(jumpthru)) {
                        result = true;
                        break;
                    }
                    else {
                        entity.Position += direciton;
                    }
                }
            }
        }
        entity.Position = orig_Position;
        return result;
    }

    public static bool CanTransform(this Entity entity, Hitbox targetHitbox, Alignment alignment, out Vector2 offset) {
        return CanTransform(entity, targetHitbox, alignment, new List<Vector2>() { Vector2.Zero }, out offset);
    }

    public static bool CanTransform(this Entity entity, Hitbox targetHitbox, Alignment alignment, out Vector2 offset, params Vector2[] wiggleList) {
        return CanTransform(entity, targetHitbox, alignment, wiggleList.ToList(), out offset);
    }

    public static bool TryTransform(this Entity entity, Hitbox targetHitbox, List<Vector2> wiggleList) {
        if (CanTransform(entity, targetHitbox, Alignment.No, wiggleList, out Vector2 offset)) {
            entity.Collider = targetHitbox;
            entity.Position += offset;
            return true;
        }
        return false;
    }
    public static bool TryTransform(this Entity entity, Hitbox targetHitbox, Alignment alignment, List<Vector2> wiggleList) {
        if (CanTransform(entity, targetHitbox, alignment, wiggleList, out Vector2 offset)) {
            entity.Collider = targetHitbox;
            entity.Position += offset;
            return true;
        }
        return false;
    }

    public static bool CanTransform(this Entity entity, Hitbox targetHitbox, Alignment alignment, List<Vector2> wiggleList, out Vector2 offset) {
        if (entity.Collider == targetHitbox) {
            offset = Vector2.Zero;
            return true;
        }
        Vector2 orig_Position = entity.Position;
        Collider orig_Collider = entity.Collider;
        float orig_Top = entity.Top;
        float orig_Left = entity.Left;
        float orig_Right = entity.Right;
        float orig_Bottom = entity.Bottom;
        List<Entity> solids = entity.Scene.Tracker.GetEntities<Solid>();
        List<Entity> orig_outside_jumpthrus = GetJumpThrus().Where(x => x.Collidable && !entity.CollideCheck(x)).ToList(); // note that the "&&" in "x.Collidable && !entity.CollideCheck(x)" is not redundant
        entity.Transform(targetHitbox, out Vector2 trans_offset, alignment);
        Vector2 origin = entity.Position;
        Vector2 wiggle_offset = Vector2.Zero;
        if (wiggleList.IsNullOrEmpty()) {
            wiggleList = new List<Vector2>() { Vector2.Zero };
        }
        bool success = false;
        foreach (Vector2 wiggle in wiggleList) {
            entity.Position = origin + wiggle;
            bool solid_loop_break = false;
            foreach (Entity solid in solids) {
                if (entity.CollideCheck(solid)) {
                    solid_loop_break = true;
                    break;
                }
            }
            if (solid_loop_break) {
                continue;
            }

            bool has_left_blocking = false;
            bool has_bottom_blocking = false;
            bool has_right_blocking = false;
            bool has_top_blocking = false;

            orig_outside_jumpthrus.ForEach(x => {
                if (!entity.CollideCheck(x)) {
                    return;
                }
                Direction dir = x.GetDirection();
                if (dir == Direction.Up) {
                    has_top_blocking = true;
                }
                else if (dir == Direction.Down) {
                    has_bottom_blocking = true;
                }
                else if (dir == Direction.Left) {
                    has_left_blocking = true;
                }
                else if (dir == Direction.Right) {
                    has_right_blocking = true;
                }
            });

            if (has_top_blocking && entity.Top < orig_Top || has_bottom_blocking && entity.Bottom > orig_Bottom || has_left_blocking && entity.Left < orig_Left || has_right_blocking && entity.Right > orig_Right) {
                continue;
            }
            success = true;
            wiggle_offset = wiggle;
            break;
        }
        if (success) {
            offset = trans_offset + wiggle_offset;
        }
        else {
            offset = Vector2.Zero;
        }
        entity.Collider = orig_Collider;
        entity.Position = orig_Position;
        return success;
    }

    public static void Transform(this Entity entity, Hitbox targetHitbox, out Vector2 offset, Alignment alignment = Alignment.No) {
        Collider c = entity.Collider;
        if (c == targetHitbox) {
            offset = Vector2.Zero;
            return;
        }
        offset = alignment switch {
            Alignment.Top => Vector2.UnitY * (c.Top - targetHitbox.Top),
            Alignment.Bottom => Vector2.UnitY * (c.Bottom - targetHitbox.Bottom),
            Alignment.Left => Vector2.UnitX * (c.Left - targetHitbox.Left),
            Alignment.Right => Vector2.UnitX * (c.Right - targetHitbox.Right),
            Alignment.CenterX => Vector2.UnitX * (c.CenterX - targetHitbox.CenterX),
            Alignment.CenterY => Vector2.UnitY * (c.CenterY - targetHitbox.CenterY),
            Alignment.TopLeft => c.TopLeft - targetHitbox.TopLeft,
            Alignment.TopCenter => c.TopCenter - targetHitbox.TopCenter,
            Alignment.TopRight => c.TopRight - targetHitbox.TopRight,
            Alignment.CenterLeft => c.CenterLeft - targetHitbox.CenterLeft,
            Alignment.Center => c.Center - targetHitbox.Center,
            Alignment.CenterRight => c.CenterRight - targetHitbox.CenterRight,
            Alignment.BottomLeft => c.BottomLeft - targetHitbox.BottomLeft,
            Alignment.BottomCenter => c.BottomCenter - targetHitbox.BottomCenter,
            Alignment.BottomRight => c.BottomRight - targetHitbox.BottomRight,
            Alignment.No => Vector2.Zero,
            _ => Vector2.Zero,
        };
        entity.Collider = targetHitbox;
        entity.Position += offset;
    }

    internal static Direction GetDir(Vector2 vector) {
        if (vector == Vector2.Zero) {
            return Direction.Zero;
        }
        if (vector.X > 0f) {
            if (vector.Y > 0f) {
                return Direction.DownRight;
            }
            else if (vector.Y < 0f) {
                return Direction.UpRight;
            }
            else {
                return Direction.Right;
            }
        }
        else if (vector.X < 0f) {
            if (vector.Y > 0f) {
                return Direction.DownLeft;
            }
            else if (vector.Y < 0f) {
                return Direction.UpLeft;
            }
            else {
                return Direction.Left;
            }
        }
        else {
            return vector.Y > 0f ? Direction.Down : Direction.Up;
        }
    }

    public static Direction GetDirection(this Entity jumpThru) {
        // the direction in which the jumpThru is blocking you to go
        if (jumpThruDirections.TryGetValue(jumpThru, out Direction dir)) {
            return dir;
        }
        // we don't initialize this when load level, so we dont need to worry about SRT
        InitializeDictionary();
        return jumpThruDirections[jumpThru];
    }

    internal static List<Entity> GetJumpThrus() {
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

    [Initialize]
    private static void Initialize() {
        // Celeste.Mod.GravityHelper.Entities.UpsideDownJumpThru -> Up

        // Celeste.Mod.JungleHelper.Entities.ClimbableOneWayPlatform -> No, but it's just an Entity so no need to treat it

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


[Flags]
public enum Direction {
    Zero = 0,
    Left = 1,
    Right = 2,
    Up = 4,
    Down = 8,
    UpLeft = Up | Left,
    UpRight = Up | Right,
    DownLeft = Down | Left,
    DownRight = Down | Right,
    LeftRight = Left | Right
}

public enum Alignment {
    Left, Right, Top, Bottom, CenterX, CenterY, TopLeft, TopCenter, TopRight, CenterLeft, Center, CenterRight, BottomLeft, BottomCenter, BottomRight, No
}