using GUITextState = System.Tuple<string, Verse.GameFont, float, float, float>;

namespace RocketGUI.Core;

using System.Collections.Generic;
using UnityEngine;
using Verse;

public static partial class GUIUtility
{
    private static readonly Dictionary<GUITextState, float> _textHeightCache = new(512);

    public static string Fit(this string text, Rect rect)
    {
        var height = text.GetTextHeight(rect.width);

        if (height <= rect.height) { return text; }

        return text.Substring(0, (int) (text.Length * height / rect.height)) + "...";
    }

    public static float GetTextHeight(this string text, Rect rect) => text != null ? Core.GUIUtility.CalcTextHeight(text, rect.width) : 0;

    public static float GetTextHeight(this string text, float width) => text != null ? Core.GUIUtility.CalcTextHeight(text, width) : 0;

    public static float GetTextHeight(this TaggedString text, float width) => Core.GUIUtility.CalcTextHeight(text, width);

    public static float CalcTextHeight(string text, float width)
    {
        var key = Core.GUIUtility.GetGUIState(text, width);

        if (Core.GUIUtility._textHeightCache.TryGetValue(key, out var height)) { return height; }

        return Core.GUIUtility._textHeightCache[key] = Text.CalcHeight(text, width);
    }

    private static GUITextState GetGUIState(string text, float width) => new GUITextState(text, Text.Font, width, Prefs.UIScale, Text.CurFontStyle.fontSize);
}
