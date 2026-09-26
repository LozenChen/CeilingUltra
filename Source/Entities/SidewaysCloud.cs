using Celeste.Mod.CeilingUltra.ModInterop;
using Celeste.Mod.CeilingUltra.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity(CustomEntityName)]
public class SidewaysCloud : Entity {

    public const string CustomEntityName = "CeilingUltra/SidewaysCloud";

    private static ConstructorInfo SidewaysJumpThruCtorInfo;

    [Initialize]
    private static void Initialize() {
        // previously i implement SidewaysCloud as a subclass of SidewaysJumpThru
        // but a bug caused by circular optional dependencies, making our EntityLoader not registered
        // coz this type was not safe at that time (and forever, due to CLR stores this)
        // https://discord.com/channels/403698615446536203/429775439423209472/1408079156406910998

        // so we use Composition instead of Inheritance

        SidewaysJumpThruCtorInfo = ModUtils.GetType("MaxHelpingHand", "Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru")?.
            GetConstructor(new Type[] { typeof(EntityData), typeof(Vector2) });

        if (SidewaysJumpThruCtorInfo is not null) {
            MaddieEntityNameRegistry.RegisterSidewaysJumpThru(CustomEntityName);
        }
    }


    private Entity BaseSidewaysJumpthru;
    // 一方面其继承了 SidewaysJumpthru, 因此有单向板功能. 另一方面, 其运动由我们手写
    // 而其容器实体 (SidewaysCloud) 则只充当背景板, 不参与交互

    public readonly bool IsLeft;

    public Facings expectedPlayerFacing;

    public int playerFacingX;

    public static ParticleType P_Cloud => Cloud.P_Cloud;

    public static ParticleType P_FragileCloud => Cloud.P_FragileCloud;

    private Sprite sprite;

    private Wiggler wiggler;

    private ParticleType particleType;

    private SoundSource sfx;

    private bool waiting = true;

    private float speed;

    private float startX;

    private float respawnTimer;

    private bool returning;

    private bool fragile;

    private float timer;

    private Vector2 scale;

    private bool canRumble;

    public bool Small;

    private bool OneUse;

    private static Vector2 scale_stretched = new Vector2(0.7f, 1.3f);

    public float ExitSpeed = 90f;
    // when the cloud shakes off maddy, give this speed
    // this makes \[ wallJumpSpeed = 130f = ExitSpeed + 40f (jumpBoost) \] (liftBoost omitted)

    public float CoyoteTime = 0.1f;

    public SidewaysCloud(EntityData data, Vector2 offset) : base(Vector2.Zero) {
        if (SidewaysJumpThruCtorInfo is null) {
            string error = "[CeilingUltra/SidewaysCloud] Fails to create an instance of MaxHelpingHand.Entities.SidewaysJumpThru. Add MaxHelpingHand to your map's dependency!";
            Logger.Log(LogLevel.Debug, "CeilingUltra", error);
            throw new Exception(error);
        }

        data.Width = 5;
        float height = data.Height = data.Bool("small") ? 26 : 32;
        data.Values["surfaceIndex"] = 4;
        data.Values["allowClimbing"] = true;
        data.Values["allowWallJumping"] = true;
        bool allowLeftToRight = !data.Bool("left");

        BaseSidewaysJumpthru = (Entity)SidewaysJumpThruCtorInfo.Invoke(new object[] { data, offset });
        BaseSidewaysJumpthru.Active = true;
        BaseSidewaysJumpthru.Visible = false;
        this.Collider = new Hitbox(5f, height, allowLeftToRight ? 3f : 0f); // purely visible (so light occlude can work)
        this.Collidable = false;
        this.Position = data.Position + offset;
        this.Depth = -60;

        this.Collider.Position = new Vector2(-2f, -height / 2f);
        // don't use CenterOrigin(), coz width is not even
        // and if use that, then maddy can't climb up a left-facing cloud
        Small = data.Bool("small");
        IsLeft = data.Bool("left");
        expectedPlayerFacing = IsLeft ? Facings.Right : Facings.Left;
        playerFacingX = IsLeft ? 1 : -1;

        this.Position.Y -= 16f;
        if (Small) {
            this.Position.Y += 2f;
        }

        fragile = data.Bool("fragile");
        startX = X;
        timer = Calc.Random.NextFloat() * 4f;
        Add(wiggler = Wiggler.Create(0.3f, 4f));
        particleType = fragile ? P_FragileCloud : P_Cloud;
        Add(new LightOcclude(0.2f));
        scale = Vector2.One;
        Add(sfx = new SoundSource());
        ExitSpeed = data.Float("ExitSpeed", 90f);
        CoyoteTime = data.Float("CoyoteTime", 0.1f);
        OneUse = !data.Bool("Respawning", true);

        BaseSidewaysJumpthru.Collider.Position = this.Collider.Position;
        BaseSidewaysJumpthru.Position = this.Position;
        movementCounterX = 0f;
    }

    public override void Awake(Scene scene) {
        if (BaseSidewaysJumpthru is not null) {
            scene.Add(BaseSidewaysJumpthru);
        }
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        string text = fragile ? "cloudFragile" : "cloud";
        if (Small) {
            text += "Remix";
        }
        Add(sprite = GFX.SpriteBank.Create(text));
        sprite.Rotation = IsLeft ? -MathF.PI / 2f : MathF.PI / 2f;
        if (!IsLeft) {
            sprite.FlipX = true;
        }
        sprite.Position = Vector2.Zero;
        sprite.OnFrameChange = (string s) => {
            if (s == "spawn" && sprite.CurrentAnimationFrame == 6) {
                wiggler.Start();
            }
        };
    }

    public override void Render() {
        Vector2 vector = scale;
        vector *= 1f + 0.1f * wiggler.Value;
        sprite.Scale = vector;
        base.Render();
    }

    public override void DebugRender(Camera camera) {
        // do nothing, let BaseSidewaysJumpThru debug render instead
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        BaseSidewaysJumpthru?.RemoveSelf();
    }

    public Player GetPlayerRider(bool strict = true) {
        if (!BaseSidewaysJumpthru.Collidable) {
            return null;
        }
        foreach (Player player in Scene.Tracker.GetEntities<Player>()) {
            if (IsRiding_Relaxed(BaseSidewaysJumpthru, IsLeft, player, strict)) {
                return player;
            }
        }
        return null;
    }

    public bool HasPlayerRider(bool strict = true) => GetPlayerRider(strict) != null;

    public static bool IsRiding_Relaxed(Entity platform, bool isLeft, Player player, bool strict = true) {
        if (player.StateMachine.State == 21 || player.StateMachine.State == 9) {
            return false;
        }

        int playerFacingX = isLeft ? 1 : -1;

        if (!strict) {
            return player.CollideCheckOutside(platform, player.Position + Vector2.UnitX * playerFacingX);
        }

        // some conditions like Speed / Retention / MoveX check can be removed, if we had a On(Dash)Collide for it. Unluckily, no.
        if (player.Speed.X * playerFacingX > 0f
            || (player.wallSpeedRetentionTimer > 0f && player.wallSpeedRetained * playerFacingX > 0f)
            || ((isLeft ? Facings.Right : Facings.Left) == player.Facing &&
                (player.StateMachine.State == 1 || player.climbTriggerDir == playerFacingX || Input.MoveX.Value * playerFacingX > 0f)
               )) {
            return player.CollideCheckOutside(platform, player.Position + Vector2.UnitX * playerFacingX);
        }
        return false;
    }

    public override void Update() {
        base.Update();
        scale.X = Calc.Approach(scale.X, 1f, 1f * Engine.DeltaTime);
        scale.Y = Calc.Approach(scale.Y, 1f, 1f * Engine.DeltaTime);
        timer += Engine.DeltaTime;
        if (HasPlayerRider(strict: true)) {
            sprite.Position = Vector2.Zero;
        }
        else {
            sprite.Position = Calc.Approach(sprite.Position, new Vector2((float)Math.Sin(timer * 2f) * playerFacingX, 0f), Engine.DeltaTime * 4f);
        }
        if (respawnTimer > 0f) {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f) {
                if (OneUse) {
                    RemoveSelf();
                    return;
                }
                waiting = true;
                BaseSidewaysJumpthru.X = X = startX;
                speed = 0f;
                scale = Vector2.One;
                BaseSidewaysJumpthru.Collidable = true;
                BaseSidewaysJumpthru.Active = true;
                sprite.Play("spawn");
                sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
            }
            return;
        }
        if (waiting) {
            Player playerRider = GetPlayerRider(strict: true);
            if (playerRider != null && playerRider.Speed.X * playerFacingX >= 0f) {
                canRumble = true;
                speed = 180f;
                scale = scale_stretched;
                waiting = false;
                if (fragile) {
                    Audio.Play("event:/game/04_cliffside/cloud_pink_boost", Position);
                }
                else {
                    Audio.Play("event:/game/04_cliffside/cloud_blue_boost", Position);
                }
            }
            return;
        }
        if (returning) {
            speed = Calc.Approach(speed, 180f, 600f * Engine.DeltaTime);
            MoveTowardsX(startX, speed * Engine.DeltaTime);
            if (MathF.Abs(ExactPositionX - startX) < 0.01f) {
                returning = false;
                waiting = true;
                speed = 0f;
            }
            return;
        }
        if (fragile && BaseSidewaysJumpthru.Collidable && !HasPlayerRider(strict: false)) {
            Fade();
        }
        if (speed < 0f && canRumble) {
            canRumble = false;
            if (HasPlayerRider(strict: false)) {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
        }
        if (speed < 0f && Scene.OnInterval(0.02f)) {
            (Scene as Level).ParticlesBG.Emit(particleType, 1, Position + new Vector2(2f * playerFacingX, 0f), new Vector2(playerFacingX, Collider.Height / 2f), MathF.PI / 2f * (1 + playerFacingX));
        }
        if (fragile && speed < 0f) {
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 0f, Engine.DeltaTime * 4f);
        }
        if ((X - startX) * playerFacingX >= 0f) {
            speed -= 1200f * Engine.DeltaTime;
        }
        else {
            speed += 1200f * Engine.DeltaTime;
            if (speed >= -100f) {
                Player playerRider2 = GetPlayerRider(strict: false);
                if (playerRider2 != null) {
                    ShakeOffPlayer(playerRider2);
                }
                if (fragile) {
                    Fade();
                    respawnTimer = 2.5f;
                }
                else {
                    scale = scale_stretched;
                    returning = true;
                }
            }
        }
        float num = -playerFacingX * speed;
        if (speed < 0f) {
            num = -playerFacingX * 220f;
        }
        MoveH(playerFacingX * speed * Engine.DeltaTime, num);

        void Fade() {
            BaseSidewaysJumpthru.Collidable = false;
            BaseSidewaysJumpthru.Active = false;
            // MMH 的单向板即使 Uncollidable, 仍然会 pushPlayer, 这不应该 (可能 Maddie 压根没想到还有这种需求). 所以我们直接阻断其更新
            sprite.Play("fade");
        }
    }

    public void ShakeOffPlayer(Player player) {
        if (player.StateMachine.State == 1 && player.Facing == expectedPlayerFacing) {
            // cancel StClimb
            player.StateMachine.State = 0;
        }
        if (player.Speed.X * playerFacingX + ExitSpeed > 0f) {
            player.Speed.X = -ExitSpeed * playerFacingX;
        }
        player.jumpGraceTimer = MathF.Max(player.jumpGraceTimer, CoyoteTime);
    }

    public float movementCounterX;

    public float ExactPositionX => (float)((double)X + (double)movementCounterX);

    public void MoveTowardsX(float x, float amount) {
        float x2 = Calc.Approach(ExactPositionX, x, amount);
        MoveToX(x2);
    }

    public void MoveToX(float x) {
        MoveH((float)((double)x - (double)X - (double)movementCounterX));
    }

    public void MoveH(float move) {
        float liftSpeedX = Engine.DeltaTime == 0f ? 0f : move / Engine.DeltaTime;
        MoveH(move, liftSpeedX);
    }

    public void MoveH(float move, float liftSpeedX) {
        movementCounterX += move;
        int num = (int)Math.Round(movementCounterX);
        if (num != 0) {
            movementCounterX -= num;
            MoveHExact(num, liftSpeedX);
        }
    }

    public void MoveHExact(int move, float liftSpeedX) {
        int sign = Math.Sign(move);
        Vector2 LiftSpeed = new Vector2(liftSpeedX, 0f);
        while (move != 0) {
            OneMove(BaseSidewaysJumpthru, IsLeft, sign, LiftSpeed);
            X = BaseSidewaysJumpthru.X;
            move -= sign;
        }
    }

    public static void OneMove(Entity platform, bool left, int sign, Vector2 LiftSpeed) {
        if (Engine.Scene is not { } scene) {
            return;
        }

        if (!platform.Collidable) {
            platform.X += sign;
            return;
        }

        bool pushing = (left ? -1 : 1) == sign;
        foreach (Actor actor in scene.Tracker.GetEntities<Actor>()) {
            if (!actor.AllowPushing || actor.TreatNaive) {
                continue;
            }
            bool collidable = actor.Collidable;
            actor.Collidable = true;
            if (pushing && platform.CollideCheckOutside(actor, platform.Position + sign * Vector2.UnitX)) {
                // push
                actor.MoveHExact(sign, null, null);
                // 这里我们并不调用 SquishCallback, 因为是云! 所以按理来说不应该造成挤压, 并且允许这种特殊情况下去穿过云
                actor.LiftSpeed = LiftSpeed;
            }
            else if (actor is Player player && IsRiding_Relaxed(platform, left, player, strict: true)) {
                // 吸附过来
                actor.X += sign;
                actor.LiftSpeed = LiftSpeed;
            }
            actor.Collidable = collidable;
        }
        platform.X += sign;
    }
}