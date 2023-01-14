namespace Capy64;

public static class Utils
{
    public struct Borders
    {
        public int Top, Bottom, Left, Right;
    }
    public static void UnpackRGB(uint packed, out byte r, out byte g, out byte b)
    {
        b = (byte)(packed & 0xff);
        g = (byte)((packed >> 8) & 0xff);
        r = (byte)((packed >> 16) & 0xff);
    }
}
