namespace RocketGUI.Core;

using UnityEngine;
using Verse;

public static partial class GUIUtility
{
    private static int _depth;
    private static GUIState _initialState;

    public static void StashGUIState()
    {
        if (GUIUtility._depth == 0) { GUIUtility._initialState = GUIState.Copy(); }
        GUIUtility._depth++;
    }

    public static void RestoreGUIState()
    {
        GUIUtility._initialState.Restore();
        GUIUtility._depth--;
    }

    public static void ClearGUIState()
    {
        GUIUtility._depth = 0;
        GUIUtility._initialState.Restore();
    }

    private readonly struct FontState
    {
        private readonly GameFont _font;
        private readonly GUIStyle _curStyle;

        public FontState(GameFont font)
        {
            var a = Text.Font;
            Text.Font  = font;
            _font = font;
            _curStyle  = new GUIStyle(Text.CurFontStyle);
            Text.Font  = a;
        }

        public void Restore()
        {
            Text.Font                   = _font;
            Text.CurFontStyle.fontSize  = _curStyle.fontSize;
            Text.CurFontStyle.fontStyle = _curStyle.fontStyle;
            Text.CurFontStyle.alignment = _curStyle.alignment;
        }
    }

    private struct GUIState
    {
        private GameFont _gameFont;
        private FontState[] _fonts;
        private Color _color;
        private Color _contentColor;
        private Color _backgroundColor;
        private bool _wordWrap;

        public static GUIState Copy() =>
            new()
            {
                _gameFont = Text.Font,
                _fonts = new FontState[3]
                {
                    new(GameFont.Tiny), new(GameFont.Small), new(GameFont.Medium)
                },
                _color           = GUI.color,
                _contentColor    = GUI.contentColor,
                _backgroundColor = GUI.backgroundColor,
                _wordWrap        = Text.WordWrap
            };

        public void Restore()
        {
            for (var i = 0; i < 3; i++) { _fonts[i].Restore(); }
            Text.Font           = _gameFont;
            GUI.color           = _color;
            GUI.contentColor    = _contentColor;
            GUI.backgroundColor = _backgroundColor;
            Text.WordWrap       = _wordWrap;
            Text.Anchor         = TextAnchor.UpperLeft;
        }
    }
}
