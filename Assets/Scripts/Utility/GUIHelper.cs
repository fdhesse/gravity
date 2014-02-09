using UnityEngine;
using System.Collections;

static class GUIHelper
{
    /// TODO: change DrawRect and DrawRectWithBorder, not performance friendly, i.e. textures are created per call,
    /// we would create objects of type DrawableRect, DrawableGradientRect and DrawableBorderedRect
    public static void DrawRect(Rect position, Color color) // acho que esta função não tá a 100%
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.SetPixels(0, 0, 2, 2, new Color[] { color, color, color, color});
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.skin.box.border = new RectOffset(0, 0, 0, 0);
        GUI.Box(position, GUIContent.none);
    }

    public static void DrawGradientRect(Rect position, Color filling, Color border)
    {
        //GUI.skin = Resources.Load("TestSkin") as GUISkin;
        Texture2D texture = new Texture2D(3, 3);
        texture.SetPixels(0, 0, 3, 3, new Color[] { border, border, border, border, filling, border, border, border, border });
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.skin.box.border = new RectOffset(1, 1, 1, 1);
        GUI.Box(position, GUIContent.none);
    }

    public static void DrawHorizontalSeparator(Rect position, Color filling, Color border)
    {
        //GUI.skin = Resources.Load("TestSkin") as GUISkin;
        Texture2D texture = new Texture2D(3, 3);
        texture.SetPixels(0, 0, 3, 3, new Color[] { border, filling, border, border, filling, border, border, filling, border });
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.skin.box.border = new RectOffset(1, 1, 1, 1);
        GUI.Box(position, GUIContent.none);
    }

    public static void DrawBorderedRect(Rect position, Color filling, Color border)
    {
        //GUI.skin = Resources.Load("TestSkin") as GUISkin;
        Texture2D texture = getBorderedTex(filling, border);
        GUI.skin.box.normal.background = texture;
        GUI.skin.box.border = new RectOffset(2, 2, 2, 2);
        GUI.Box(position, GUIContent.none);
    }

    public static Texture2D getBorderedTex(Color filling, Color border)
    {
        Texture2D texture = new Texture2D(4, 4);
        texture.SetPixels(0, 0, 4, 4, new Color[] { border, border, border, border,
                                                    border, filling, filling, border,
                                                    border, filling, filling, border,
                                                    border, border, border, border,
                                                  });
        texture.Apply();
        return texture;
    }

    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    public static string ColorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }

    public static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
}
