using GUILambda = System.Action<UnityEngine.Rect>;

namespace RocketGUI.Core.Listings;

using Core;

using System;
using System.Collections.Generic;

using UnityEngine;

using Verse;

public sealed class ListingCollapsible : ListingCustom
{
    private bool _expanded;
    private GroupCollapsible _group;

    public ListingCollapsible(bool expanded = false, bool scrollViewOnOverflow = true) : base(scrollViewOnOverflow)
    {
        _expanded = expanded;
        _group    = new GroupCollapsible();
    }

    public ListingCollapsible(GroupCollapsible group, bool expanded = false, bool scrollViewOnOverflow = true) : base(scrollViewOnOverflow)
    {
        _expanded = expanded;
        _group    = group;
        _group.Register(this);
    }

    public GroupCollapsible Group
    {
        get => _group;

        set
        {
            _group.AllCollapsibles.RemoveAll(c => c == this);
            _group = value;
            _group.Register(this);
        }
    }

    public bool Expanded
    {
        get => _expanded;

        private set
        {
            _group.CollapseAll();
            _expanded = value;
        }
    }

    public void Begin(Rect inRect, TaggedString title, bool drawInfo = true, bool drawIcon = true, bool highlightIfMouseOver = true)
    {
        base.Begin(inRect);

        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                Text.Font   = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleLeft;
                var slice = Slice(title.GetTextHeight(InsideWidth - 30f));

                if (highlightIfMouseOver) { Widgets.DrawHighlightIfMouseover(slice._outside); }
                var titleRect = slice._inside;

                Core.GUIUtility.ExecuteSafeGUIAction(
                    () =>
                    {
                        GUI.color = _collapsibleBgColor;
                        GUI.color = Color.gray;

                        if (!drawInfo) { return; }
                        Text.Font   = GameFont.Tiny;
                        Text.Anchor = TextAnchor.MiddleRight;
                        Widgets.Label(titleRect, !_expanded ? "Collapsed" : "Expanded");
                    }
                );

                Core.GUIUtility.ExecuteSafeGUIAction(
                    () =>
                    {
                        Text.Font                   = GameFont.Small;
                        Text.CurFontStyle.fontStyle = FontStyle.Normal;
                        Text.CurFontStyle.fontSize  = 12;
                        Text.Anchor                 = TextAnchor.MiddleLeft;
                        GUI.color                   = _collapsibleBgColor;
                        GUI.color                   = Color.gray;

                        if (drawIcon)
                        {
                            Widgets.DrawTextureFitted(titleRect.LeftPartPixels(25), _expanded ? TexButton.Collapse : TexButton.Reveal, 0.65f);
                            titleRect.xMin += 35;
                        }
                        GUI.color = Color.white;
                        Widgets.Label(titleRect, title);
                    }
                );

                if (Widgets.ButtonInvisible(slice._outside)) { Expanded = !Expanded; }
                GUI.color = _collapsibleBgColor;
                Widgets.DrawBox(slice._outside);
            }
        );

        if (Expanded) { Gap(2); }
        Start();
    }

    public void Label(TaggedString text, string tooltip = null, bool invert = false, bool highlightIfMouseOver = true, GameFont fontSize = GameFont.Tiny, FontStyle fontStyle = FontStyle.Normal)
    {
        if (invert == _expanded) { return; }
        base.Label(text, tooltip, highlightIfMouseOver, fontSize, fontStyle);
    }

    public bool CheckboxLabeled(
        TaggedString text,
        ref bool checkOn,
        string tooltip = null,
        bool invert = false,
        bool disabled = false,
        bool highlightIfMouseOver = true,
        GameFont fontSize = GameFont.Tiny,
        FontStyle fontStyle = FontStyle.Normal
    ) => invert != _expanded && base.CheckboxLabeled(text, ref checkOn, tooltip, disabled, highlightIfMouseOver, fontSize, fontStyle);

    public void DropDownMenu<T>(
        string text,
        T selection,
        Func<T, string> labelLambda,
        Action<T> selectedLambda,
        IEnumerable<T> options,
        bool invert = false,
        bool disabled = false,
        GameFont fontSize = GameFont.Tiny,
        FontStyle fontStyle = FontStyle.Normal
    )
    {
        if (invert == _expanded) { return; }
        base.DropDownMenu(text, selection, labelLambda, selectedLambda, options, disabled, fontSize, fontStyle);
    }

    public void Columns(float height, IEnumerable<GUILambda> lambdas, float gap = 5, bool invert = false, bool useMargins = false, Action fallback = null)
    {
        if (invert == _expanded) { return; }
        base.Columns(height, lambdas, gap, useMargins, fallback);
    }

    public void Lambda(float height, GUILambda contentLambda, bool invert = false, bool useMargins = false, Action fallback = null)
    {
        if (invert == _expanded) { return; }
        base.Lambda(height, contentLambda, useMargins, fallback);
    }

    public bool ButtonText(TaggedString text, bool disabled = false, bool invert = false, bool drawBackground = true) => invert != _expanded && base.ButtonText(text, disabled, drawBackground);

    private void Gap(float height = 9f, bool invert = false)
    {
        if (_expanded != invert) { base.Gap(height); }
    }

    public void Line(float thickness, bool invert = false)
    {
        if (_expanded != invert) { base.Line(thickness); }
    }

    public override void End(ref Rect inRect) => base.End(ref inRect);

    protected override RectSlice Slice(float height, bool includeMargins = true) => base.Slice(height, includeMargins);

    public class GroupCollapsible
    {
        private List<ListingCollapsible> _collapsibles;

        public List<ListingCollapsible> AllCollapsibles => _collapsibles ??= new List<ListingCollapsible>();

        public void CollapseAll()
        {
            foreach (var collapsible in AllCollapsibles) { collapsible._expanded = false; }
        }

        public void Register(ListingCollapsible collapsible)
        {
            AllCollapsibles.Add(collapsible);

            collapsible._expanded = false;
        }
    }
}
