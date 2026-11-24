using Celeste.Mod.CeilingUltra.ModInterop;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Utils;

public static class CollideCheckHelper {

    public static bool CanStand(this Entity entity, Vector2 direciton, Entity platform, Vector2 platformAt) {
        direciton.Y *= GravityImports.InvertY;

        Vector2 orig_Position = entity.Position;
        Vector2 orig_platformPosition = platform.Position;
        entity.Position += direciton;
        platform.Position = platformAt;
        bool result = false;
        if (platform is Solid solid) {
            result = entity.CollideCheck(solid);
        }
        else if (direciton != Vector2.Zero && IsJumpThru(platform)) {
            if (platform.GetDirection() == GetDir(direciton) && entity.CollideCheck(platform)) {
                entity.Position = orig_Position;
                if (!entity.CollideCheck(platform)) {
                    result = true;
                }
            }
        }
        entity.Position = orig_Position;
        platform.Position = orig_platformPosition;
        return result;
    }

    public static bool CanStand(this Entity entity, Vector2 direction, Entity platform) {
        direction.Y *= GravityImports.InvertY;

        Vector2 orig_Position = entity.Position;
        entity.Position += direction;
        bool result = false;
        if (platform is Solid solid) {
            result = entity.CollideCheck(solid);
        }
        else if (direction != Vector2.Zero && IsJumpThru(platform)) {
            if (platform.GetDirection() == GetDir(direction) && entity.CollideCheck(platform)) {
                entity.Position = orig_Position;
                if (!entity.CollideCheck(platform)) {
                    result = true;
                }
            }
        }
        entity.Position = orig_Position;
        return result;
    }

    public static bool CanStand(this Entity entity, Vector2 direciton) {
        return CanStand(entity, direciton, out Entity _);
    }
    public static bool CanStand(this Entity entity, Vector2 direciton, out Entity platform) {
        direciton.Y *= GravityImports.InvertY;
        platform = null;
        Vector2 orig_Position = entity.Position;
        entity.Position += direciton;
        bool result = false;
        foreach (Solid solid in entity.Scene.Tracker.GetEntities<Solid>()) {
            if (entity.CollideCheck(solid)) {
                result = true;
                platform = solid;
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
                        platform = jumpthru;
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
        if (GravityImports.IsPlayerInverted) { // well we should use IsEntityInverted here, but ...
            alignment = InvertY(alignment);
        }
        List<Vector2> local_wigglelist = GravityImports.IsPlayerInverted ? wiggleList.Select(vec => new Vector2(vec.X, -vec.Y)).ToList() : wiggleList;

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
        if (local_wigglelist.IsNullOrEmpty()) {
            local_wigglelist = new List<Vector2>() { Vector2.Zero };
        }
        bool success = false;
        foreach (Vector2 wiggle in local_wigglelist) {
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


    public static Alignment InvertY(Alignment align) {
        return align switch {
            Alignment.Top => Alignment.Bottom,
            Alignment.Bottom => Alignment.Top,
            Alignment.TopLeft => Alignment.BottomLeft,
            Alignment.TopCenter => Alignment.BottomCenter,
            Alignment.TopRight => Alignment.BottomRight,
            Alignment.BottomLeft => Alignment.TopLeft,
            Alignment.BottomCenter => Alignment.TopCenter,
            Alignment.BottomRight => Alignment.TopRight,
            _ => align
        };
    }


    private static bool IsJumpThru(Entity entity) => JumpthruClassifier.IsJumpThru(entity);


    internal static List<Entity> GetJumpThrus() => JumpthruClassifier.GetJumpThrus();


    public static Direction GetDirection(this Entity jumpThru) => JumpthruClassifier.GetDirection(jumpThru);

    public static bool CheckIceCeiling(Player player) => Entities.IceCeilingHelper.CheckCeiling(player);
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