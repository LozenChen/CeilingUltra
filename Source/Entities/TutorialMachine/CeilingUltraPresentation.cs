using System.Collections;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.CeilingUltra.Entities.TutorialMachine;


public class CeilingUltraPresentation : Entity {
    public Vector2 ScaleInPoint = new Vector2(1920f, 1080f) / 2f;

    public readonly int ScreenWidth = 1920;

    public readonly int ScreenHeight = 1080;

    private float ease;

    private float waitingForInputTime;

    private VirtualRenderTarget screenBuffer;

    private VirtualRenderTarget prevPageBuffer;

    private VirtualRenderTarget currPageBuffer;

    private int pageIndex;

    private List<CeilingUltraPage> pages = new List<CeilingUltraPage>();

    private float pageEase;

    private bool pageTurning;

    private bool pageUpdating;

    private bool waitingForPageTurn;

    private VertexPositionColorTexture[] verts = new VertexPositionColorTexture[6];

    private EventInstance usingSfx;

    public bool Viewing { get; private set; }

    public class FakeAtlas {

        public FakeAtlas() { }
        public MTexture this[string id] {
            get => GFX.Game[$"CeilingUltra/Presentation/{id}"];
        }
    }

    public FakeAtlas Gfx { get; private set; }

    public bool ShowInput {
        get {
            if (!waitingForPageTurn) {
                if (CurrPage != null) {
                    return CurrPage.WaitingForInput;
                }
                return false;
            }
            return true;
        }
    }

    private CeilingUltraPage PrevPage {
        get {
            if (pageIndex <= 0) {
                return null;
            }
            return pages[pageIndex - 1];
        }
    }

    private CeilingUltraPage CurrPage {
        get {
            if (pageIndex >= pages.Count) {
                return null;
            }
            return pages[pageIndex];
        }
    }

    public CeilingUltraPresentation(EventInstance usingSfx = null) {
        base.Tag = Tags.HUD;
        Viewing = true;
        Add(new Coroutine(Routine()));
        this.usingSfx = usingSfx;
        Gfx = new();
    }

    private IEnumerator Routine() {
        /*
        pages.Add(new CeilingUltraPage00());

        pages.Add(new CeilingUltraPage01());

        pages.Add(new CeilingUltraPage02());
        */

        /*
        pages.Add(new CeilingUltraPage03());

        pages.Add(new CeilingUltraPage04());

        pages.Add(new CeilingUltraPage05());
        */

        // pages.Add(new CeilingUltraPage03b());

        pages.Add(new CeilingUltraPage04b());

        pages.Add(new CeilingUltraPage05b());


        pages.Add(new CeilingUltraPage06());

        foreach (CeilingUltraPage page in pages) {
            page.Added(this);
        }
        Add(new BeforeRenderHook(BeforeRender));
        while (ease < 1f) {
            ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
            yield return null;
        }
        while (pageIndex < pages.Count) {
            pageUpdating = true;
            yield return CurrPage.Routine();
            if (!CurrPage.AutoProgress) {
                waitingForPageTurn = true;
                while (!Input.MenuConfirm.Pressed) {
                    yield return null;
                }
                waitingForPageTurn = false;
                Audio.Play("event:/new_content/game/10_farewell/ppt_mouseclick");
            }
            pageUpdating = false;
            pageIndex++;
            if (pageIndex < pages.Count) {
                float num = 0.5f;
                if (CurrPage.Transition == CeilingUltraPage.Transitions.Rotate3D) {
                    num = 1.5f;
                }
                else if (CurrPage.Transition == CeilingUltraPage.Transitions.Blocky) {
                    num = 1f;
                }
                pageTurning = true;
                pageEase = 0f;
                Add(new Coroutine(TurnPage(num)));
                yield return num * 0.8f;
            }
        }
        if (usingSfx != null) {
            Audio.SetParameter(usingSfx, "end", 1f);
            usingSfx.release();
        }
        Audio.Play("event:/new_content/game/10_farewell/cafe_computer_off");
        while (ease > 0f) {
            ease = Calc.Approach(ease, 0f, Engine.DeltaTime * 2f);
            yield return null;
        }
        Viewing = false;
        RemoveSelf();
    }

    private IEnumerator TurnPage(float duration) {
        if (CurrPage.Transition != 0 && CurrPage.Transition != CeilingUltraPage.Transitions.FadeIn) {
            if (CurrPage.Transition == CeilingUltraPage.Transitions.Rotate3D) {
                Audio.Play("event:/new_content/game/10_farewell/ppt_cube_transition");
            }
            else if (CurrPage.Transition == CeilingUltraPage.Transitions.Blocky) {
                Audio.Play("event:/new_content/game/10_farewell/ppt_dissolve_transition");
            }
            else if (CurrPage.Transition == CeilingUltraPage.Transitions.Spiral) {
                Audio.Play("event:/new_content/game/10_farewell/ppt_spinning_transition");
            }
        }
        while (pageEase < 1f) {
            pageEase += Engine.DeltaTime / duration;
            yield return null;
        }
        pageTurning = false;
    }

    private void BeforeRender() {
        if (screenBuffer == null || screenBuffer.IsDisposed) {
            screenBuffer = VirtualContent.CreateRenderTarget("CeilingUltra-Buffer", ScreenWidth, ScreenHeight, depth: true);
        }
        if (prevPageBuffer == null || prevPageBuffer.IsDisposed) {
            prevPageBuffer = VirtualContent.CreateRenderTarget("CeilingUltra-Screen1", ScreenWidth, ScreenHeight);
        }
        if (currPageBuffer == null || currPageBuffer.IsDisposed) {
            currPageBuffer = VirtualContent.CreateRenderTarget("CeilingUltra-Screen2", ScreenWidth, ScreenHeight);
        }
        if (pageTurning && PrevPage != null) {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(prevPageBuffer);
            Engine.Graphics.GraphicsDevice.Clear(PrevPage.ClearColor);
            Draw.SpriteBatch.Begin();
            PrevPage.Render();
            Draw.SpriteBatch.End();
        }
        if (CurrPage != null) {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(currPageBuffer);
            Engine.Graphics.GraphicsDevice.Clear(CurrPage.ClearColor);
            Draw.SpriteBatch.Begin();
            CurrPage.Render();
            Draw.SpriteBatch.End();
        }
        Engine.Graphics.GraphicsDevice.SetRenderTarget(screenBuffer);
        Engine.Graphics.GraphicsDevice.Clear(Color.Black);
        if (pageTurning) {
            if (CurrPage.Transition == CeilingUltraPage.Transitions.ScaleIn) {
                Draw.SpriteBatch.Begin();
                Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                Vector2 scale = Vector2.One * pageEase;
                Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, ScaleInPoint, currPageBuffer.Bounds, Color.White, 0f, ScaleInPoint, scale, SpriteEffects.None, 0f);
                Draw.SpriteBatch.End();
            }
            else if (CurrPage.Transition == CeilingUltraPage.Transitions.FadeIn) {
                Draw.SpriteBatch.Begin();
                Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White * pageEase);
                Draw.SpriteBatch.End();
            }
            else if (CurrPage.Transition == CeilingUltraPage.Transitions.Rotate3D) {
                float num = -MathF.PI / 2f * pageEase;
                RenderQuad((RenderTarget2D)prevPageBuffer, pageEase, num);
                RenderQuad((RenderTarget2D)currPageBuffer, pageEase, MathF.PI / 2f + num);
            }
            else if (CurrPage.Transition == CeilingUltraPage.Transitions.Blocky) {
                Draw.SpriteBatch.Begin();
                Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                uint seed = 1u;
                int num2 = ScreenWidth / 60;
                for (int i = 0; i < ScreenWidth; i += num2) {
                    for (int j = 0; j < ScreenHeight; j += num2) {
                        if (PseudoRandRange(ref seed, 0f, 1f) <= pageEase) {
                            Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, new Rectangle(i, j, num2, num2), new Rectangle(i, j, num2, num2), Color.White);
                        }
                    }
                }
                Draw.SpriteBatch.End();
            }
            else if (CurrPage.Transition == CeilingUltraPage.Transitions.Spiral) {
                Draw.SpriteBatch.Begin();
                Draw.SpriteBatch.Draw((RenderTarget2D)prevPageBuffer, Vector2.Zero, Color.White);
                Vector2 scale2 = Vector2.One * pageEase;
                float rotation = (1f - pageEase) * 12f;
                Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, global::Celeste.Celeste.TargetCenter, currPageBuffer.Bounds, Color.White, rotation, global::Celeste.Celeste.TargetCenter, scale2, SpriteEffects.None, 0f);
                Draw.SpriteBatch.End();
            }
        }
        else {
            Draw.SpriteBatch.Begin();
            Draw.SpriteBatch.Draw((RenderTarget2D)currPageBuffer, Vector2.Zero, Color.White);
            Draw.SpriteBatch.End();
        }
    }

    private void RenderQuad(Texture texture, float ease, float rotation) {
        float num = (float)screenBuffer.Width / (float)screenBuffer.Height;
        float num2 = num;
        float num3 = 1f;
        Vector3 position = new Vector3(0f - num2, num3, 0f);
        Vector3 position2 = new Vector3(num2, num3, 0f);
        Vector3 position3 = new Vector3(num2, 0f - num3, 0f);
        Vector3 position4 = new Vector3(0f - num2, 0f - num3, 0f);
        verts[0].Position = position;
        verts[0].TextureCoordinate = new Vector2(0f, 0f);
        verts[0].Color = Color.White;
        verts[1].Position = position2;
        verts[1].TextureCoordinate = new Vector2(1f, 0f);
        verts[1].Color = Color.White;
        verts[2].Position = position3;
        verts[2].TextureCoordinate = new Vector2(1f, 1f);
        verts[2].Color = Color.White;
        verts[3].Position = position;
        verts[3].TextureCoordinate = new Vector2(0f, 0f);
        verts[3].Color = Color.White;
        verts[4].Position = position3;
        verts[4].TextureCoordinate = new Vector2(1f, 1f);
        verts[4].Color = Color.White;
        verts[5].Position = position4;
        verts[5].TextureCoordinate = new Vector2(0f, 1f);
        verts[5].Color = Color.White;
        float num4 = 4.15f + Calc.YoYo(ease) * 1.7f;
        Matrix value = Matrix.CreateTranslation(0f, 0f, num) * Matrix.CreateRotationY(rotation) * Matrix.CreateTranslation(0f, 0f, 0f - num4) * Matrix.CreatePerspectiveFieldOfView(MathF.PI / 4f, num, 1f, 10f);
        Engine.Instance.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        Engine.Instance.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        Engine.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
        Engine.Instance.GraphicsDevice.Textures[0] = texture;
        GFX.FxTexture.Parameters["World"].SetValue(value);
        foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes) {
            pass.Apply();
            Engine.Instance.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length / 3);
        }
    }

    public override void Update() {
        base.Update();
        if (ShowInput) {
            waitingForInputTime += Engine.DeltaTime;
        }
        else {
            waitingForInputTime = 0f;
        }
        if (CurrPage != null && pageUpdating) {
            CurrPage.Update();
        }
    }

    public override void Render() {
        if (screenBuffer != null && !screenBuffer.IsDisposed) {
            float num = (float)ScreenWidth * Ease.CubeOut(Calc.ClampedMap(ease, 0f, 0.5f));
            float num2 = (float)ScreenHeight * Ease.CubeInOut(Calc.ClampedMap(ease, 0.5f, 1f, 0.2f));
            Rectangle rectangle = new Rectangle((int)((1920f - num) / 2f), (int)((1080f - num2) / 2f), (int)num, (int)num2);
            Draw.SpriteBatch.Draw((RenderTarget2D)screenBuffer, rectangle, Color.White);
            if (ShowInput && waitingForInputTime > 0.2f) {
                GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1856f, 1016 + ((base.Scene.TimeActive % 1f < 0.25f) ? 6 : 0)), Color.Black);
            }
            if ((base.Scene as Level).Paused) {
                Draw.Rect(rectangle, Color.Black * 0.7f);
            }
        }
    }

    public override void Removed(Scene scene) {
        base.Removed(scene);
        Dispose();
    }

    public override void SceneEnd(Scene scene) {
        base.SceneEnd(scene);
        Dispose();
    }

    private void Dispose() {
        if (screenBuffer != null) {
            screenBuffer.Dispose();
        }
        screenBuffer = null;
        if (prevPageBuffer != null) {
            prevPageBuffer.Dispose();
        }
        prevPageBuffer = null;
        if (currPageBuffer != null) {
            currPageBuffer.Dispose();
        }
        currPageBuffer = null;
    }

    private static uint PseudoRand(ref uint seed) {
        uint num = seed;
        num ^= num << 13;
        num ^= num >> 17;
        return seed = num ^ (num << 5);
    }

    public static float PseudoRandRange(ref uint seed, float min, float max) {
        return min + (float)(PseudoRand(ref seed) % 1000) / 1000f * (max - min);
    }
}