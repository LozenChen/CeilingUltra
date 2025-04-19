using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.CeilingUltra.Entities;

[CustomEntity("CeilingUltra/CustomCrumbleBlock")]

public class CustomCrumbleBlock : Solid {

    public const string TexturePath_Left = "CeilingUltra/CrumbleBlock/left";

    public const string TexturePath_Right = "CeilingUltra/CrumbleBlock/right";

    public const string TexturePath_Up = "CeilingUltra/CrumbleBlock/up";

    public const string TexturePath_Down = "CeilingUltra/CrumbleBlock/down";

    public const string TexturePath_Outline_Left = "CeilingUltra/CrumbleBlock/outline_left";

    public const string TexturePath_Outline_Right = "CeilingUltra/CrumbleBlock/outline_right";

    public const string TexturePath_Outline_Up = "CeilingUltra/CrumbleBlock/outline_up";

    public const string TexturePath_Outline_Down = "CeilingUltra/CrumbleBlock/outline_down";


    public static ParticleType P_Crumble => CrumblePlatform.P_Crumble;

    private List<Image> images;

    private List<Image> outline;

    private List<Coroutine> falls;

    private List<int> fallOrder;

    private ShakerList shaker;

    private LightOcclude occluder;

    private Coroutine outlineFader;

    public string TexturePath;

    public string OutlinePath;

    public int Length;

    public bool ActivateOnDashCollide = true;

    public bool triggered;

    private int dirX;

    private int dirY;

    public CustomCrumbleBlock(Vector2 position)
        : base(position, 8f, 8f, safe: false) {
        EnableAssistModeChecks = false;
    }


    public CustomCrumbleBlock(EntityData data, Vector2 offset)
        : this(data.Position + offset) {

        triggered = false;
        ActivateOnDashCollide = data.Bool("ActivateOnDashCollide", true);
        if (ActivateOnDashCollide) {
            OnDashCollide = ActivateOnDash;
            Add(new ActivateOnDashCollideComponent());
        }

        bool isHorizontal;

        string facing = data.String("Facing", "Down");
        switch (facing) {
            case "Down": {
                isHorizontal = true;
                TexturePath = TexturePath_Down;
                OutlinePath = TexturePath_Outline_Down;
                break;
            }
            case "Left": {
                isHorizontal = false;
                TexturePath = TexturePath_Left;
                OutlinePath = TexturePath_Outline_Left;
                break;
            }
            case "Right": {
                isHorizontal = false;
                TexturePath = TexturePath_Right;
                OutlinePath = TexturePath_Outline_Right;
                break;
            }
            default: {
                isHorizontal = true;
                TexturePath = TexturePath_Up;
                OutlinePath = TexturePath_Outline_Up;
                break;
            }
        }

        if (isHorizontal) {
            Length = Math.Max(8, data.Width);
            Collider.Width = Length;
            dirX = 1;
            dirY = 0;
        }
        else {
            Length = Math.Max(8, data.Height);
            Collider.Height = Length;
            dirX = 0;
            dirY = 1;
        }
    }

    public DashCollisionResults ActivateOnDash(Player player, Vector2 direction) {
        triggered = true;
        return DashCollisionResults.NormalCollision;
    }


    public override void Added(Scene scene) {
        base.Added(scene);
        MTexture mTexture = GFX.Game[OutlinePath];
        MTexture mTexture2 = GFX.Game[TexturePath];

        outline = new List<Image>();
        if (Length <= 8f) {
            Image image = new Image(mTexture.GetSubtexture(24 * dirX, 24 * dirY, 8, 8));
            image.Color = Color.White * 0f;
            Add(image);
            outline.Add(image);
        }
        else {
            for (int i = 0; i < Length; i += 8) {
                int num = ((i != 0) ? ((i > 0 && i < Length - 8f) ? 1 : 2) : 0);
                Image image2 = new Image(mTexture.GetSubtexture(num * 8 * dirX, num * 8 * dirY, 8, 8));
                image2.Position = new Vector2(i * dirX, i * dirY);
                image2.Color = Color.White * 0f;
                Add(image2);
                outline.Add(image2);
            }
        }
        Add(outlineFader = new Coroutine());
        outlineFader.RemoveOnComplete = false;
        images = new List<Image>();
        falls = new List<Coroutine>();
        fallOrder = new List<int>();
        for (int j = 0; j < Length; j += 8) {
            int num2 = (int)((Math.Abs(X) + j) / 8f) % 4;
            Image image3 = new Image(mTexture2.GetSubtexture(num2 * 8 * dirX, num2 * 8 * dirY, 8, 8));
            image3.Position = new Vector2(4f + j * dirX, 4f + j * dirY);
            image3.CenterOrigin();
            Add(image3);
            images.Add(image3);
            Coroutine coroutine = new Coroutine();
            coroutine.RemoveOnComplete = false;
            falls.Add(coroutine);
            Add(coroutine);
            fallOrder.Add(j / 8);
        }
        fallOrder.Shuffle();
        Add(new Coroutine(Sequence()));
        Add(shaker = new ShakerList(images.Count, on: false, (Vector2[] v) => {
            for (int k = 0; k < images.Count; k++) {
                images[k].Position = new Vector2(4f + k * 8 * dirX, 4f + k * 8 * dirY) + v[k];
            }
        }));
        Add(occluder = new LightOcclude(0.2f));
    }

    private IEnumerator Sequence() {
        while (true) {
            bool onTop = false;
            triggered = false;
            bool waiting = true;
            while (waiting) {
                if (GetPlayerOnTop() != null) {
                    onTop = true;
                    waiting = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                else {
                    if (GetPlayerClimbing() == null && !triggered) {
                        yield return null;
                        continue;
                    }
                    onTop = false;
                    waiting = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
            }
            
            Audio.Play("event:/game/general/platform_disintegrate", Center);
            shaker.ShakeFor(onTop ? 0.6f : 1f, removeOnFinish: false);
            foreach (Image image in images) {
                SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
            }
            for (int i = 0; i < (onTop ? 1 : 3); i++) {
                yield return 0.2f;
                foreach (Image image2 in images) {
                    SceneAs<Level>().Particles.Emit(P_Crumble, 2, Position + image2.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }
            }
            float timer = 0.4f;
            if (onTop) {
                while (timer > 0f && GetPlayerOnTop() != null) {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
            }
            else {
                while (timer > 0f) {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
            }
            outlineFader.Replace(OutlineFade(1f));
            occluder.Visible = false;
            Collidable = false;
            float num = 0.05f;
            for (int j = 0; j < 4; j++) {
                for (int k = 0; k < images.Count; k++) {
                    if (k % 4 - j == 0) {
                        falls[k].Replace(TileOut(images[fallOrder[k]], num * (float)j));
                    }
                }
            }
            yield return 2f;
            while (CollideCheck<Actor>() || CollideCheck<Solid>()) {
                yield return null;
            }
            outlineFader.Replace(OutlineFade(0f));
            occluder.Visible = true;
            Collidable = true;
            for (int l = 0; l < 4; l++) {
                for (int m = 0; m < images.Count; m++) {
                    if (m % 4 - l == 0) {
                        falls[m].Replace(TileIn(m, images[fallOrder[m]], 0.05f * (float)l));
                    }
                }
            }
        }
    }


    private IEnumerator OutlineFade(float to) {
        float from = 1f - to;
        for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f) {
            Color color = Color.White * (from + (to - from) * Ease.CubeInOut(t));
            foreach (Image item in outline) {
                item.Color = color;
            }
            yield return null;
        }
    }


    private IEnumerator TileOut(Image img, float delay) {
        img.Color = Color.Gray;
        yield return delay;
        float distance = (img.X * 7f % 3f + 1f) * 12f;
        Vector2 from = img.Position;
        for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.4f) {
            yield return null;
            img.Position = from + Vector2.UnitY * Ease.CubeIn(time) * distance;
            img.Color = Color.Gray * (1f - time);
            img.Scale = Vector2.One * (1f - time * 0.5f);
        }
        img.Visible = false;
    }


    private IEnumerator TileIn(int index, Image img, float delay) {
        yield return delay;
        Audio.Play("event:/game/general/platform_return", Center);
        img.Visible = true;
        img.Color = Color.White;
        img.Position = new Vector2(index * 8 * dirX + 4f, index * 8 * dirY + 4f);
        for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.25f) {
            yield return null;
            img.Scale = Vector2.One * (1f + Ease.BounceOut(1f - time) * 0.2f);
        }
        img.Scale = Vector2.One;
    }
}
