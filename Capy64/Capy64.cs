using Capy64.API;
using Capy64.Core;
using Capy64.Eventing;
using Capy64.Extensions;
using Capy64.LuaRuntime;
using Capy64.PluginManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Capy64.Utils;

namespace Capy64;

public class Capy64 : Game, IGame
{
    public const string Version = "0.0.3-alpha";
    public static string AppDataPath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolderOption.Create),
        "Capy64");
    public Game Game => this;
    public IList<IPlugin> NativePlugins { get; private set; }
    public IList<IPlugin> Plugins { get; private set; }
    public int Width { get; set; } = 400;
    public int Height { get; set; } = 300;
    public float Scale { get; set; } = 2f;
    public Drawing Drawing { get; private set; }
    public Runtime LuaRuntime { get; set; }
    public EventEmitter EventEmitter { get; private set; }
    public Borders Borders = new()
    {
        Top = 0,
        Bottom = 0,
        Left = 0,
        Right = 0,
    };

    private readonly InputManager _inputManager;
    private RenderTarget2D renderTarget;
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private IServiceProvider _serviceProvider;
    private ulong _totalTicks = 0;

    public Capy64()
    {
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

    public void UpdateSize()
    {
        _graphics.PreferredBackBufferWidth = (int)(Width * Scale) + Borders.Left + Borders.Right;
        _graphics.PreferredBackBufferHeight = (int)(Height * Scale) + Borders.Top + Borders.Right;
        _graphics.ApplyChanges();

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
        Console.WriteLine(bounds);

        if (Window.IsMaximized())
        {

        }

        Width = (int)(bounds.Width / Scale);
        Height = (int)(bounds.Height / Scale);

        UpdateSize();
    }

    protected override void Initialize()
    {
        Window.Title = "Capy64 " + Version;

        UpdateSize();

        Window.AllowUserResizing = false;
        Window.ClientSizeChanged += OnWindowSizeChange;

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
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        Drawing.Begin();

        EventEmitter.RaiseTick(new()
        {
            GameTime = gameTime,
            TotalTicks = _totalTicks
        });

        // resume here

        Drawing.End();

        // Register user input
        _inputManager.Update(IsActive);

        base.Update(gameTime);
        _totalTicks++;
    }

    protected override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        //GraphicsDevice.Clear(Color.Blue);
        _spriteBatch.Draw(renderTarget, new(Borders.Left, Borders.Top), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}