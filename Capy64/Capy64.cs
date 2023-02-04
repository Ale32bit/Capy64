using Capy64.API;
using Capy64.Core;
using Capy64.Eventing;
using Capy64.Extensions;
using Capy64.Runtime;
using Capy64.PluginManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Capy64.Utils;
using Capy64.Integrations;

namespace Capy64;

public class Capy64 : Game, IGame
{
    public const string Version = "0.0.7-alpha";
    public static string AppDataPath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolderOption.Create),
        "Capy64");
    public static Capy64 Instance;
    public Capy64 Game => this;
    public IList<IPlugin> NativePlugins { get; private set; }
    public IList<IPlugin> Plugins { get; private set; }
    public int Width { get; set; } = 400;
    public int Height { get; set; } = 300;
    public float Scale { get; set; } = 2f;
    public Drawing Drawing { get; private set; }
    public Audio Audio { get; private set; }
    public LuaState LuaRuntime { get; set; }
    public Eventing.EventEmitter EventEmitter { get; private set; }
    public DiscordIntegration Discord { get; set; }

    public Borders Borders = new()
    {
        Top = 0,
        Bottom = 0,
        Left = 0,
        Right = 0,
    };
    public SpriteBatch SpriteBatch;


    private readonly InputManager _inputManager;
    private RenderTarget2D renderTarget;
    private readonly GraphicsDeviceManager _graphics;
    private IServiceProvider _serviceProvider;
    private ulong _totalTicks = 0;

    public Capy64()
    {
        Instance = this;

        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        EventEmitter = new();
        _inputManager = new(this, EventEmitter);

        Drawing = new();
    }

    public void ConfigureServices(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void UpdateSize(bool resize = true)
    {
        if (resize)
        {
            _graphics.PreferredBackBufferWidth = (int)(Width * Scale) + Borders.Left + Borders.Right;
            _graphics.PreferredBackBufferHeight = (int)(Height * Scale) + Borders.Top + Borders.Right;
            _graphics.ApplyChanges();
        }

        renderTarget = new RenderTarget2D(
            GraphicsDevice,
            Width,
            Height,
            false,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents);

        Drawing.Canvas = renderTarget;

        _inputManager.Texture = renderTarget;
        _inputManager.WindowScale = Scale;

        EventEmitter.RaiseScreenSizeChange();
    }

    private void OnWindowSizeChange(object sender, EventArgs e)
    {
        var bounds = Window.ClientBounds;

        Width = (int)(bounds.Width / Scale);
        Height = (int)(bounds.Height / Scale);

        if (Window.IsMaximized())
        {
            var vertical = bounds.Height - Height * Scale;
            var horizontal = bounds.Width - Width * Scale;

            Borders.Top = (int)Math.Floor(vertical / 2d);
            Borders.Bottom = (int)Math.Ceiling(vertical / 2d);

            Borders.Left = (int)Math.Floor(horizontal / 2d);
            Borders.Right = (int)Math.Ceiling(horizontal / 2d);
        }
        else
        {
            Borders = new Borders();
        }

        UpdateSize(false);
    }

    protected override void Initialize()
    {
        Window.Title = "Capy64 " + Version;

        UpdateSize();

        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowSizeChange;

        Audio = new Audio();

        NativePlugins = GetNativePlugins();
        Plugins = PluginLoader.LoadAllPlugins("plugins", _serviceProvider);

        EventEmitter.RaiseInit();

        base.Initialize();
    }

    private List<IPlugin> GetNativePlugins()
    {
        var iType = typeof(IPlugin);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => iType.IsAssignableFrom(p) && !p.IsInterface);

        var plugins = new List<IPlugin>();

        foreach (var type in types)
        {
            var instance = (IPlugin)ActivatorUtilities.CreateInstance(_serviceProvider, type)!;
            plugins.Add(instance);
        }
        return plugins;
    }


    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        Drawing.Begin();

        // Register user input
        _inputManager.Update(IsActive);

        EventEmitter.RaiseTick(new()
        {
            GameTime = gameTime,
            TotalTicks = _totalTicks
        });

        Drawing.End();

        _totalTicks++;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        SpriteBatch.Draw(renderTarget, new(Borders.Left, Borders.Top), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0);

        EventEmitter.RaiseOverlay(new()
        {
            GameTime = gameTime,
            TotalTicks = _totalTicks,
        });

        SpriteBatch.End();

        base.Draw(gameTime);
    }
}