using Capy64.Core;
using Capy64.LuaRuntime.Libraries;
using Microsoft.Xna.Framework;

namespace Capy64.BIOS;

public class PanicScreen
{
    public Color ForegroundColor = Color.White;
    public Color BackgroundColor = new Color(0, 51, 187);

    private Drawing _drawing;
    public PanicScreen(Drawing drawing)
    {
        _drawing = drawing;
    }

    public void Render(string error, string details = null)
    {
        Term.ForegroundColor = ForegroundColor;
        Term.BackgroundColor = BackgroundColor;
        Term.SetCursorBlink(false);
        Term.SetSize(57, 23);
        Term.Clear();

        var title = " Capy64 ";
        var halfX = (Term.Width / 2) + 1;
        Term.SetCursorPosition(halfX - (title.Length / 2), 2);
        Term.ForegroundColor = BackgroundColor;
        Term.BackgroundColor = ForegroundColor;
        Term.Write(title);

        Term.ForegroundColor = ForegroundColor;
        Term.BackgroundColor = BackgroundColor;
        Term.SetCursorPosition(1, 4);
        Print(error + '\n');

        if (details is not null)
        {
            Print(details);
        }
    }

    private void Print(string txt)
    {
        foreach (var ch in txt)
        {
            Term.Write(ch.ToString());
            if (Term.CursorPosition.X >= Term.Width || ch == '\n')
            {
                Term.SetCursorPosition(1, (int)Term.CursorPosition.Y + 1);
            }
        }
        Term.SetCursorPosition(1, (int)Term.CursorPosition.Y + 1);
    }
}
