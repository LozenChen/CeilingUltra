using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.CeilingUltra.Utils;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity(CustomEntityName)]
[TrackedAs(typeof(SidewaysJumpThru))]
public class SidewaysCloud : MaxHelpingHand.Entities.SidewaysJumpThru {
    private const string CustomEntityName = "CeilingUltra/SidewaysCloud";

    private Solid playerInteractingSolid;

    public new readonly bool Left;

    public Facings expectedPlayerFacing;

    public int playerFacingX;

    [Initialize]
    private static void Initialize() {
        if (typeof(SidewaysJumpThru).GetMethodInfo("onLevelLoad") is { } methodInfo) {
            methodInfo.IlHook(il => {
                ILCursor cursor = new ILCursor(il);
                cursor.Index = -4;
                Instruction target = cursor.Next;
                cursor.Index = 5;
                cursor.Emit(OpCodes.Ldarg_2);
                cursor.EmitDelegate(ShouldActivateHooks);
                cursor.Emit(OpCodes.Brtrue, target);

            });
        }
        else {
            Logger.Log(LogLevel.Error, "CeilingUltra", "Fail to hook MaxHelpingHand, SidewaysCloud won't work properly!");
        }
    }

    private static bool ShouldActivateHooks(Session session) {
        if (session.MapData?.Levels is { } levels) {
            foreach (LevelData level in levels) {
                if (level.Entities?.Any((EntityData entity) => entity.Name == CustomEntityName) ?? false) {
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

    public SidewaysCloud(EntityData data, Vector2 offset)
        : base(Modifier(data), offset) {
        collider.Position = new Vector2(- Width / 2f, -Height / 2f);
        Small = data.Bool("small");
        Position.Y -= 16f;
        Left = data.Bool("left");
        expectedPlayerFacing = Left ? Facings.Right : Facings.Left;
        playerFacingX = Left ? 1 : -1;
        playerInteractingSolid = new Solid(Position, 5f, 32f, safe: false);
        playerInteractingSolid.Collidable = false;
        playerInteractingSolid.Visible = false;
        if (Small) {
            Position.Y += 2f;
            playerInteractingSolid.Position.Y += 2f;
            playerInteractingSolid.Collider.Height -= 6f;
        }
        playerInteractingSolid.collider.Position = collider.Position;

        fragile = data.Bool("fragile");
        startX = base.X;
        timer = Calc.Random.NextFloat() * 4f;
        Add(wiggler = Wiggler.Create(0.3f, 4f));
        particleType = fragile ? P_FragileCloud : P_Cloud;
        surfaceIndex = 4;
        Add(new LightOcclude(0.2f));
        scale = Vector2.One;
        Add(sfx = new SoundSource());
    }

    private static EntityData Modifier(EntityData data) {
        data.Width = 5;
        data.Height = data.Bool("small") ? 26 : 32;
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
        sprite.Rotation = Left ? -MathF.PI/ 2f : MathF.PI / 2f;
        if (!Left) {
            sprite.FlipX = true;
        }
        sprite.Position = Vector2.Zero ;
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
        foreach (Player entity in base.Scene.Tracker.GetEntities<Player>()) {
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
                base.X = startX;
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
        if (speed < 0f && base.Scene.OnInterval(0.02f)) {
            (base.Scene as Level).ParticlesBG.Emit(particleType, 1, Position + new Vector2(2f * playerFacingX, 0f), new Vector2(playerFacingX, base.Collider.Height / 2f), MathF.PI);
        }
        if (fragile && speed < 0f) {
            sprite.Scale.Y = Calc.Approach(sprite.Scale.Y, 0f, Engine.DeltaTime * 4f);
        }
        if ((base.X - startX) * playerFacingX >= 0f) {
            speed -= 1200f * Engine.DeltaTime;
        }
        else {
            speed += 1200f * Engine.DeltaTime;
            if (speed >= -100f) {
                Player playerRider2 = GetPlayerRider();
                if (playerRider2 != null && playerRider2.Speed.X * playerFacingX >= 0f) {
                    playerRider2.Speed.X = -200f * playerFacingX;
                    playerRider2.StartJumpGraceTime();
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
        SidewaysMovingPlatform.SidewaysJumpthruOnMove(this, playerInteractingSolid, Left, move);
    }
}
