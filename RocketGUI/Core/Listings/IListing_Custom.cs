using GUILambda = System.Action<UnityEngine.Rect>;

namespace RocketGUI.Core.Listings;

using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

public abstract class ListingCustom
{
    private const float ScrollViewWidthDelta = 25f;

    private readonly bool _scrollViewOnOverflow;

    // Not accessible anymore?
    // public Color _collapsibleBgBorderColor = Widgets.MenuSectionBGBorderColor;

    public readonly Color _collapsibleBgColor = Widgets.MenuSectionBGFillColor;

    private Rect _contentRect;

    private float _curYMin;

    private Rect _inRect;

    private float _inXMax;

    private float _inXMin;

    private float _inYMax;

    private float _inYMin;

    private bool _isOverflowing;

    private readonly Vector2 _margins = new(8, 4);

    private float _previousHeight;

    private Vector2 _scrollPosition = Vector2.zero;

    private bool _started;

    protected ListingCustom(bool scrollViewOnOverflow = true) => _scrollViewOnOverflow = scrollViewOnOverflow;

    protected virtual bool Overflowing => _isOverflowing;

    protected virtual float InsideWidth => _inXMax - _inXMin - _margins.x * 2f;

    public virtual Vector4 Margins => _margins;

    private Rect Rect
    {
        get => new(_inXMin, _curYMin, _inXMax - _inXMin, _inYMax - _curYMin);

        set
        {
            _inXMin  = value.xMin;
            _inXMax  = value.xMax;
            _curYMin = value.yMin;
            _inYMin  = value.yMin;
            _inYMax  = value.yMax;
        }
    }

    protected virtual void Begin(Rect inRect, bool scrollViewOnOverflow = true)
    {
        _inRect = inRect;

        if (_scrollViewOnOverflow && _started && inRect.height < _previousHeight)
        {
            Core.GUIUtility.ExecuteSafeGUIAction(
                () =>
                {
                    _isOverflowing = true;
                    Core.GUIUtility.StashGUIState();
                    GUI.color    = Color.white;
                    _contentRect = new Rect(0f, 0f, inRect.width - ListingCustom.ScrollViewWidthDelta, _previousHeight);
                    _inYMin      = _contentRect.yMin;
                    Rect         = _contentRect;
                    Widgets.BeginScrollView(inRect, ref _scrollPosition, _contentRect);
                    Core.GUIUtility.RestoreGUIState();
                }
            );
        }
        else
        {
            _inYMin = inRect.yMin;
            Rect    = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
        }
    }

    protected virtual void Start()
    {
        Core.GUIUtility.StashGUIState();
        Text.Font                   = GameFont.Tiny;
        Text.CurFontStyle.fontStyle = FontStyle.Normal;
    }

    protected virtual void Label(TaggedString text, string tooltip = null, bool highlightIfMouseOver = true, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal) =>
        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                var slice = Slice(text.GetTextHeight(InsideWidth));

                if (highlightIfMouseOver) { Widgets.DrawHighlightIfMouseover(slice._outside); }
                Text.Font                   = fontSize;
                Text.CurFontStyle.fontStyle = fontStyle;
                Widgets.Label(slice._inside, text);

                if (tooltip != null) { TooltipHandler.TipRegion(slice._outside, tooltip); }
            }
        );

    protected virtual bool CheckboxLabeled(TaggedString text, ref bool checkOn, string tooltip = null, bool disabled = false, bool highlightIfMouseOver = true, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal)
    {
        var checkOnInt = checkOn;

        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                Text.Font                   = fontSize;
                Text.CurFontStyle.fontStyle = fontStyle;
                var slice = Slice(text.GetTextHeight(InsideWidth - 23f));

                if (highlightIfMouseOver) { Widgets.DrawHighlightIfMouseover(slice._outside); }
                Core.GUIUtility.CheckBoxLabeled(slice._inside, text, ref checkOnInt, disabled, iconWidth: 23f, drawHighlightIfMouseover: false);

                if (tooltip != null) { TooltipHandler.TipRegion(slice._outside, tooltip); }
            }
        );

        if (checkOnInt == checkOn) { return false; }
        checkOn = checkOnInt;

        return true;
    }

    protected virtual void Columns(float height, IEnumerable<GUILambda> lambdas, float gap = 5, bool useMargins = false, Action fallback = null) =>
        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                var lambdaList = lambdas.ToList();

                if (lambdaList.Count() == 1)
                {
                    Lambda(height, lambdaList.First(), useMargins, fallback);

                    return;
                }
                var rect    = useMargins ? Slice(height)._inside : Slice(height)._outside;
                var columns = rect.Columns(lambdaList.Count(), gap);
                var i       = 0;

                foreach (var lambda in lambdaList)
                {
                    Core.GUIUtility.ExecuteSafeGUIAction(
                        () =>
                        {
                            lambda(columns[i++]);
                        }, fallback
                    );
                }
            }
        );

    protected virtual void DropDownMenu<T>(
        TaggedString text,
        T selection,
        Func<T, string> labelLambda,
        Action<T> selectedLambda,
        IEnumerable<T> options,
        bool disabled = false,
        GameFont fontSize = GameFont.Tiny,
        FontStyle fontStyle = FontStyle.Normal
    ) =>
        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                var selectedText = labelLambda(selection);

                Text.Font                   = fontSize;
                Text.CurFontStyle.fontStyle = fontStyle;

                var rect    = Slice(selectedText.GetTextHeight(InsideWidth - 23f))._inside;
                var columns = rect.Columns(2);

                Widgets.Label(columns[0], text);

                if (Widgets.ButtonText(columns[1], selectedText, active: !disabled)) { Core.GUIUtility.DropDownMenu(labelLambda, selectedLambda, options); }
            }
        );

    protected virtual void Lambda(float height, GUILambda contentLambda, bool useMargins = false, Action fallback = null)
    {
        var slice = Slice(height);

        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                contentLambda(useMargins ? slice._inside : slice._outside);
            }, fallback
        );
    }

    protected virtual void Gap(float height = 9f) => Slice(height, false);

    protected virtual void Line(float thickness)
    {
        Gap(3.5f);
        Widgets.DrawBoxSolid(Slice(thickness, false)._outside, _collapsibleBgColor);
        Gap(3.5f);
    }

    protected virtual bool ButtonText(TaggedString text, bool disabled = false, bool drawBackground = false)
    {
        var clicked = false;

        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                Text.Font                   = GameFont.Small;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                var slice = Slice(text.GetTextHeight(InsideWidth) + 4);

                if (!drawBackground)
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    GUI.color   = Mouse.IsOver(slice._inside) ? Color.white : Color.cyan;
                }
                clicked = Widgets.ButtonText(slice._inside, text, drawBackground);
            }
        );

        return clicked;
    }

    public virtual void End(ref Rect inRect)
    {
        Gap(5);
        GUI.color = _collapsibleBgColor;
        Widgets.DrawBox(new Rect(_inXMin, _inYMin, _inXMax - _inXMin, _curYMin - _inYMin));

        _started        = true;
        _previousHeight = Mathf.Abs(_inYMin - _curYMin);

        if (_isOverflowing)
        {
            Widgets.EndScrollView();

            if (_started && inRect.height < _previousHeight)
            {
                GUI.color = _collapsibleBgColor;
                Widgets.DrawBox(new Rect(inRect.xMin, inRect.yMin, inRect.width - 25f, 1));
                Widgets.DrawBox(new Rect(inRect.xMin, inRect.yMax - 1, inRect.width - 25f, 1));
            }
            inRect.yMin = Mathf.Min(_curYMin + _inRect.yMin, _inRect.yMax);
        }
        else { inRect.yMin = _curYMin; }
        _isOverflowing = false;
        Core.GUIUtility.RestoreGUIState();
    }

    protected virtual RectSlice Slice(float height, bool includeMargins = true)
    {
        var outside = new Rect(_inXMin, _curYMin, _inXMax - _inXMin, includeMargins ? height + _margins.y : height);
        var inside  = new Rect(outside);

        if (includeMargins)
        {
            inside.xMin += _margins.x * 2;
            inside.xMax -= _margins.x;
            inside.yMin += _margins.y / 2f;
            inside.yMax -= _margins.y / 2f;
        }
        _curYMin += includeMargins ? height + _margins.y : height;
        Widgets.DrawBoxSolid(outside, _collapsibleBgColor);

        return new RectSlice(inside, outside);
    }

    protected struct RectSlice
    {
        public Rect _inside;
        public Rect _outside;

        public RectSlice(Rect inside, Rect outside)
        {
            _outside = outside;
            _inside  = inside;
        }
    }
}
