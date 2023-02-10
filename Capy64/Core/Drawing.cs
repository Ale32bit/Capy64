using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.IO;

namespace Capy64.Core;

public class Drawing : IDisposable
{
    private SpriteBatch _spriteBatch;
    private GraphicsDevice _graphicsDevice;
    private readonly FontSystem _fontSystem;
    private Texture2D _whitePixel;
    private RenderTarget2D _canvas;
    private bool _isDrawing;
    private HashSet<Texture2D> _disposeTextures = new();
    public RenderTarget2D Canvas
    {
        get => _canvas;
        set
        {
            var isDrawing = _isDrawing;
            if (isDrawing)
                End();
            _canvas = value;
            _spriteBatch = new SpriteBatch(_canvas.GraphicsDevice);
            _graphicsDevice = _canvas.GraphicsDevice;

            _whitePixel = new Texture2D(_spriteBatch.GraphicsDevice, 1, 1, mipmap: false, SurfaceFormat.Color);
            _whitePixel.SetData(new Color[1] { Color.White });
            if (isDrawing)
                Begin();
        }
    }

    public Drawing()
    {
        _fontSystem = new FontSystem();
        _fontSystem.AddFont(File.ReadAllBytes(@"Assets/font.ttf"));
    }

    public void Begin()
    {
        if (_isDrawing)
            return;

        _isDrawing = true;
        _graphicsDevice.SetRenderTarget(_canvas);
        _graphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true, };
        _spriteBatch.Begin();
    }

    public void End()
    {
        if (!_isDrawing)
            return;

        _spriteBatch.End();
        _graphicsDevice.SetRenderTarget(null);

        foreach (var t in _disposeTextures)
            t.Dispose();
        _disposeTextures.Clear();

        _isDrawing = false;
    }

    public void DrawString(Vector2 pos, string text, Color color, int size = 13)
    {
        var font = _fontSystem.GetFont(size);
        _spriteBatch.DrawString(font, text, pos, color, layerDepth: 0);
    }

    public Vector2 MeasureString(string text, int size = 13)
    {
        var font = _fontSystem.GetFont(size);
        return font.MeasureString(text);
    }

    public void Plot(Point point, Color color)
    {
        if (point.X < 0 || point.Y < 0) return;
        if (point.X >= _canvas.Width || point.Y >= _canvas.Height) return;

        var grid = new Color[_canvas.Width * _canvas.Height];
        _canvas.GetData(grid);

        grid[point.X + (point.Y * _canvas.Width)] = color;

        _canvas.SetData(grid);
    }

    public void Plot(IEnumerable<Point> points, Color color)
    {
        var grid = new Color[_canvas.Width * _canvas.Height];
        _canvas.GetData(grid);
        foreach (var point in points)
        {
            if (point.X < 0 || point.Y < 0) continue;
            if (point.X >= _canvas.Width || point.Y >= _canvas.Height) continue;
            grid[point.X + (point.Y * _canvas.Width)] = color;
        }
        _canvas.SetData(grid);
    }

    public Color GetPixel(Point point)
    {
        if (point.X < 0 || point.Y < 0) return Color.Black;
        if (point.X >= _canvas.Width || point.Y >= _canvas.Height) return Color.Black;

        var grid = new Color[_canvas.Width * _canvas.Height];
        return grid[point.X + (point.Y * _canvas.Width)];
    }

    public void UnsafePlot(Point point, Color color)
    {
        var grid = new Color[_canvas.Width * _canvas.Height];
        _canvas.GetData(grid);
        grid[point.X + (point.Y * _canvas.Width)] = color;
        _canvas.SetData(grid);
    }

    public void UnsafePlot(IEnumerable<Point> points, Color color)
    {
        var grid = new Color[_canvas.Width * _canvas.Height];
        _canvas.GetData(grid);
        foreach (var point in points)
        {
            grid[point.X + (point.Y * _canvas.Width)] = color;
        }
        _canvas.SetData(grid);
    }

    public void DrawPoint(Vector2 point, Color color, int size = 1)
    {
        _spriteBatch.DrawPoint(point, color, size);
    }

    public void DrawCircle(Vector2 pos, int radius, Color color, int thickness = 1)
    {
        _spriteBatch.DrawCircle(pos, radius, radius * 4, color, thickness);
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1)
    {
        _spriteBatch.DrawLine(start, end, color, thickness);
    }

    public void DrawRectangle(Vector2 pos, Size2 size, Color color, int thickness = 1, float rotation = 0f)
    {
        DrawRectangle(new(pos, size), color, thickness, rotation, 0f);
    }

    public void DrawPolygon(Vector2 pos, Vector2[] points, Color color, int thickness = 1)
    {
        _spriteBatch.DrawPolygon(pos, points, color, thickness);
    }

    public void DrawEllipse(Vector2 pos, Vector2 radius, Color color, int thickness = 1)
    {
        var sides = (int)Math.Max(radius.X, radius.Y) * 4;
        _spriteBatch.DrawEllipse(pos, radius, sides, color, thickness);
    }

    public void Clear(Color? color = default)
    {
        Color finalColor = color ?? Color.Black;
        _graphicsDevice.Clear(finalColor);
    }

    public void DrawRectangle(RectangleF rectangle, Color color, float thickness = 1f, float rotation = 0f, float layerDepth = 0f)
    {
        Vector2 position = new(rectangle.X, rectangle.Y);
        Vector2 position2 = new(rectangle.Right - thickness, rectangle.Y);
        Vector2 position3 = new(rectangle.X, rectangle.Bottom - thickness);
        Vector2 scale = new(rectangle.Width, thickness);
        Vector2 scale2 = new(thickness, rectangle.Height);
        _spriteBatch.Draw(_whitePixel, position, null, color, rotation, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        _spriteBatch.Draw(_whitePixel, position, null, color, rotation, Vector2.Zero, scale2, SpriteEffects.None, layerDepth);
        _spriteBatch.Draw(_whitePixel, position2, null, color, rotation, Vector2.Zero, scale2, SpriteEffects.None, layerDepth);
        _spriteBatch.Draw(_whitePixel, position3, null, color, rotation, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
    }

    public void DrawTexture(Texture2D texture, Vector2 pos)
    {
        _spriteBatch.Draw(texture, pos, null, Color.White, 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
    }

    public void DrawBuffer(uint[] buffer, Rectangle rect)
    {
        var texture = new Texture2D(_graphicsDevice, rect.Width, rect.Height, false, SurfaceFormat.Color);
        texture.SetData(buffer);

        DrawTexture(texture, new(rect.X, rect.Y));
        _disposeTextures.Add(texture);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _spriteBatch.Dispose();
        _whitePixel.Dispose();
    }
}
