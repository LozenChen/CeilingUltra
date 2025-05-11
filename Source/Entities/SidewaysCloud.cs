using Celeste.Mod.CeilingUltra.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System.Reflection;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity(CustomEntityName)]
[TrackedAs(typeof(MaxHelpingHand.Entities.SidewaysJumpThru))]
// Everest.Loader.LoadModAssembly / CeilingUltra.AttributeUtils, both use Assembly.GetTypesSafe(), which checks if the BaseType is safe
// so when MaxHelpingHand is not loaded, this class will be considered unsafe, and thus will be skipped
public class SidewaysCloud : MaxHelpingHand.Entities.SidewaysJumpThru {
    private const string CustomEntityName = "CeilingUltra/SidewaysCloud";

    public static readonly HashSet<string> SidewaysJumpthruNames = new() {
        CustomEntityName,
        "MaxHelpingHand/SidewaysJumpThru",
        "MaxHelpingHand/AttachedSidewaysJumpThru",
        "MaxHelpingHand/OneWayInvisibleBarrierHorizontal",
        "MaxHelpingHand/SidewaysMovingPlatform"
    };

    private Solid playerInteractingSolid;

    public readonly bool IsLeft;

    public Facings expectedPlayerFacing;

    public int playerFacingX;

    [Initialize]
    internal static void Initialize() {
        if (typeof(MaxHelpingHand.Entities.SidewaysJumpThru).GetMethodInfo("onLevelLoad") is { } methodInfo) {
            methodInfo.IlHook(il => {
                ILCursor cursor = new ILCursor(il);
                cursor.Index = -4;
                Instruction target = cursor.Next;
                cursor.Index = 5;
                cursor.Emit(OpCodes.Ldarg_2);
                // to avoid double foreach loop, we also check MaxHelpingHand sidewaysJumpthrus, and (de)activate hooks on our own
                cursor.EmitDelegate(OnLevelLoad);
                cursor.Emit(OpCodes.Ret);
            });
        }
        else {
            Logger.Log(LogLevel.Error, "CeilingUltra", "Fail to hook MaxHelpingHand, SidewaysCloud won't work properly!");
        }
    }

    private static void OnLevelLoad(Session session) {
        if (ShouldActivateHooks(session)) {
            MaxHelpingHand.Entities.SidewaysJumpThru.activateHooks();
        }
        else {
            MaxHelpingHand.Entities.SidewaysJumpThru.deactivateHooks();
        }
    }

    private static bool ShouldActivateHooks(Session session) {
        if (session.MapData?.Levels is { } levels) {
            foreach (LevelData level in levels) {
                if (level.Entities?.Any((EntityData entity) => SidewaysJumpthruNames.Contains(entity.Name)) ?? false) {
                    return true;
                }
            }
        }
        return false;
    }
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

    public float CoyoteTime = 0.1f;

    public SidewaysCloud(EntityData data, Vector2 offset)
        : base(Modifier(data), offset) {
        Collider.Position = new Vector2(-2f, -Height / 2f);
        // don't use CenterOrigin(), coz width is not even
        // and if use that, then maddy can't climb up a left-facing cloud
        Small = data.Bool("small");
        Position.Y -= 16f;
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
        playerInteractingSolid.Collider.Position = Collider.Position;

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
    }

    private static EntityData Modifier(EntityData data) {
        data.Width = 5;
        data.Height = data.Bool("small") ? 26 : 32;
        data.Values["surfaceIndex"] = 4;
        data.Values["allowClimbing"] = true;
        data.Values["allowWallJumping"] = true;
        return data;
    }

    public override void Awake(Scene scene) {
        // do nothing, so the original SidewaysJumpthru sprite is not added
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

    public Player GetPlayerRider() {
        bool orig = playerInteractingSolid.Collidable;
        playerInteractingSolid.Collidable = true;
        Player player = null;
        foreach (Player entity in Scene.Tracker.GetEntities<Player>()) {
            if (IsRiding(entity)) {
                player = entity;
                break;
            }
        }
        playerInteractingSolid.Collidable = orig;
        return player;
    }

    public bool IsRiding(Player player) {
        if (player.StateMachine.State == 23 || player.StateMachine.State == 9) {
            return false;
        }
        // some conditions like Speed / Retention / MoveX check can be removed, if we had a On(Dash)Collide for it. Unluckily, no.
        if (player.Speed.X * playerFacingX > 0f || (player.wallSpeedRetentionTimer > 0f && player.wallSpeedRetained * playerFacingX > 0f) ||
            (expectedPlayerFacing == player.Facing &&
                (player.StateMachine.State == 1 || player.climbTriggerDir == playerFacingX || Input.MoveX.Value * playerFacingX > 0f))) {
            return player.CollideCheckOutside(playerInteractingSolid, player.Position + Vector2.UnitX * playerFacingX);
        }
        return false;
    }

    public bool HasPlayerRider() => GetPlayerRider() != null;

    public override void Update() {
        base.Update();
        scale.X = Calc.Approach(scale.X, 1f, 1f * Engine.DeltaTime);
        scale.Y = Calc.Approach(scale.Y, 1f, 1f * Engine.DeltaTime);
        timer += Engine.DeltaTime;
        if (HasPlayerRider()) {
            sprite.Position = Vector2.Zero;
        }
        else {
            sprite.Position = Calc.Approach(sprite.Position, new Vector2((float)Math.Sin(timer * 2f) * playerFacingX, 0f), Engine.DeltaTime * 4f);
        }
        if (respawnTimer > 0f) {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f) {
                waiting = true;
                X = startX;
                playerInteractingSolid.X = startX;
                speed = 0f;
                scale = Vector2.One;
                Collidable = true;
                sprite.Play("spawn");
                sfx.Play("event:/game/04_cliffside/cloud_pink_reappear");
            }
            return;
        }
        if (waiting) {
            Player playerRider = GetPlayerRider();
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
        if (fragile && Collidable && !HasPlayerRider()) {
            Collidable = false;
            sprite.Play("fade");
        }
        if (speed < 0f && canRumble) {
            canRumble = false;
            if (HasPlayerRider()) {
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
                Player playerRider2 = GetPlayerRider();
                if (playerRider2 != null && playerRider2.Speed.X * playerFacingX >= 0f) {
                    PushOffPlayer(playerRider2);
                }
                if (fragile) {
                    Collidable = false;
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

    public void PushOffPlayer(Player player) {
        player.Speed.X = - ExitSpeed * playerFacingX;
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
        MaxHelpingHand.Entities.SidewaysMovingPlatform.SidewaysJumpthruOnMove(this, playerInteractingSolid, IsLeft, move);
    }
}
