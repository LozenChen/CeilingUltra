using Celeste.Mod.CeilingUltra.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Reflection;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity("CeilingUltra/SidewaysCloud")]
public class SidewaysCloud : Entity {

    private Entity BaseSidewaysJumpthru;


    private static ConstructorInfo SidewaysJumpthruCtor;

    [Initialize]
    private static void Initialize() {
        // previously i implement SidewaysCloud as a subclass of SidewaysJumpThru
        // but a bug caused by circular optional dependencies, making our EntityLoader not registered
        // coz this type was not safe at that time (and forever, due to CLR stores this)
        // https://discord.com/channels/403698615446536203/429775439423209472/1408079156406910998

        // so we use Composition instead of Inheritance

        if (ModUtils.GetType("MaxHelpingHand", "Celeste.Mod.MaxHelpingHand.Entities.SidewaysJumpThru")?.
            GetConstructor(new Type[] { typeof(EntityData), typeof(Vector2) }) is { } info) {
            SidewaysJumpthruCtor = info;
        }
        else {
            SidewaysJumpthruCtor = null;
        }
    }

    private Solid playerInteractingSolid;

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

    private static Vector2 scale_stretched = new Vector2(0.7f, 1.3f);

    public float ExitSpeed = 90f;
    // when the cloud shakes off maddy, give this speed
    // this makes \[ wallJumpSpeed = 130f = ExitSpeed + 40f (jumpBoost) \] (liftBoost omitted)

    public float CoyoteTime = 0.1f;

    public SidewaysCloud(EntityData data, Vector2 offset)
        : base(Vector2.Zero) {

        if (SidewaysJumpthruCtor is null) {
            string error = "[CeilingUltra/SidewaysCloud] Fails to create an instance of MaxHelpingHand.Entities.SidewaysJumpThru. Add MaxHelpingHand to your map's dependency!";
            Logger.Log(LogLevel.Debug, "CeilingUltra", error);
            throw new Exception(error);
        }

        data.Width = 5;
        data.Height = data.Bool("small") ? 26 : 32;
        data.Values["surfaceIndex"] = 4;
        data.Values["allowClimbing"] = true;
        data.Values["allowWallJumping"] = true;
        bool allowLeftToRight = !data.Bool("left");

        BaseSidewaysJumpthru = (Entity)SidewaysJumpthruCtor.Invoke(new object[] { data, offset });
        BaseSidewaysJumpthru.Active = true;
        BaseSidewaysJumpthru.Visible = false;
        this.Collider = new Hitbox(5f, data.Height, allowLeftToRight ? 3f : 0f); // purely visible (so light occlude can work), don't work
        this.Collidable = false;
        this.Position = data.Position + offset;
        this.Depth = -60;

        BaseSidewaysJumpthru.Collider.Position = new Vector2(-2f, -BaseSidewaysJumpthru.Height / 2f);
        // don't use CenterOrigin(), coz width is not even
        // and if use that, then maddy can't climb up a left-facing cloud
        Position.Y -= 16f;
        Small = data.Bool("small");
        IsLeft = data.Bool("left");
        expectedPlayerFacing = IsLeft ? Facings.Right : Facings.Left;
        playerFacingX = IsLeft ? 1 : -1;
        playerInteractingSolid = new Solid(Position, 5f, 32f, safe: false);
        playerInteractingSolid.Collidable = false;
        playerInteractingSolid.Visible = false;
        if (Small) {
            Position.Y += 2f;
            playerInteractingSolid.Position.Y += 2f;
            playerInteractingSolid.Collider.Height -= 6f;
        }
        playerInteractingSolid.Collider.Position = BaseSidewaysJumpthru.Collider.Position;

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

        BaseSidewaysJumpthru.Position = this.Position;
    }

    public override void Awake(Scene scene) {
        if (BaseSidewaysJumpthru is not null) {
            scene.Add(BaseSidewaysJumpthru);
        }
    }

    public override void Added(Scene scene) {
        base.Added(scene);
        scene.Add(playerInteractingSolid);
        string text = (fragile ? "cloudFragile" : "cloud");
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
        // do nothing
    }

    public Player GetPlayerRider(bool strict = true) {
        bool orig = playerInteractingSolid.Collidable;
        playerInteractingSolid.Collidable = true;
        Player player = null;
        foreach (Player entity in Scene.Tracker.GetEntities<Player>()) {
            if (IsRiding(entity, strict)) {
                player = entity;
                break;
            }
        }
        playerInteractingSolid.Collidable = orig;
        return player;
    }

    public bool IsRiding(Player player, bool strict = true) {
        if (player.StateMachine.State == 21 || player.StateMachine.State == 9) {
            return false;
        }
        if (!strict) {
            return player.CollideCheckOutside(playerInteractingSolid, player.Position + Vector2.UnitX * playerFacingX);
        }
        // some conditions like Speed / Retention / MoveX check can be removed, if we had a On(Dash)Collide for it. Unluckily, no.
        if (player.Speed.X * playerFacingX > 0f || (player.wallSpeedRetentionTimer > 0f && player.wallSpeedRetained * playerFacingX > 0f) ||
            (expectedPlayerFacing == player.Facing &&
                (player.StateMachine.State == 1 || player.climbTriggerDir == playerFacingX || Input.MoveX.Value * playerFacingX > 0f))) {
            return player.CollideCheckOutside(playerInteractingSolid, player.Position + Vector2.UnitX * playerFacingX);
        }
        return false;
    }

    public bool HasPlayerRider(bool strict = true) => GetPlayerRider(strict) != null;

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
                waiting = true;
                BaseSidewaysJumpthru.X = X = startX;
                playerInteractingSolid.X = startX;
                speed = 0f;
                scale = Vector2.One;
                BaseSidewaysJumpthru.Collidable = true;
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
            BaseSidewaysJumpthru.Collidable = false;
            sprite.Play("fade");
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
                    BaseSidewaysJumpthru.Collidable = false;
                    sprite.Play("fade");
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

    public float ExactPositionX => playerInteractingSolid.ExactPosition.X;

    public float LiftSpeedX {
        get => playerInteractingSolid.LiftSpeed.X;
        set {
            playerInteractingSolid.LiftSpeed.X = value;
        }
    }

    public float movementCounterX {
        get => playerInteractingSolid.movementCounter.X;
        set {
            playerInteractingSolid.movementCounter.X = value;
        }
    }

    public void MoveTowardsX(float x, float amount) {
        float x2 = Calc.Approach(ExactPositionX, x, amount);
        MoveToX(x2);
    }

    public void MoveToX(float x) {
        MoveH((float)((double)x - (double)playerInteractingSolid.Position.X - (double)movementCounterX));
    }

    public void MoveH(float moveH) {
        if (Engine.DeltaTime == 0f) {
            LiftSpeedX = 0f;
        }
        else {
            LiftSpeedX = moveH / Engine.DeltaTime;
        }
        movementCounterX += moveH;
        int num = (int)Math.Round(movementCounterX);
        if (num != 0) {
            movementCounterX -= num;
            MoveHExact(num);
        }
    }

    public void MoveH(float moveH, float liftSpeedH) {
        LiftSpeedX = liftSpeedH;
        movementCounterX += moveH;
        int num = (int)Math.Round(movementCounterX);
        if (num != 0) {
            movementCounterX -= num;
            MoveHExact(num);
        }
    }

    public void MoveHExact(int move) {
        MoveImpl(Vector2.UnitX * move);
    }

    public void MoveImpl(Vector2 move) {
        MaxHelpingHand.Entities.SidewaysMovingPlatform.SidewaysJumpthruOnMove(BaseSidewaysJumpthru, playerInteractingSolid, IsLeft, move);
        this.Position = BaseSidewaysJumpthru.Position;
    }
}