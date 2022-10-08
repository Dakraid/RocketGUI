namespace RocketGUI.Core.Selectors;

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

public abstract class SelectorGenericSelection<T> : Selector
{
    private readonly IEnumerable<T> _items;

    protected string _searchString = "";

    protected readonly Action<T> _selectionAction;

    private readonly bool _useSearchBar = true;

    protected SelectorGenericSelection(IEnumerable<T> defs, Action<T> selectionAction, bool integrated = false, Action closeAction = null) : base(integrated, closeAction)
    {
        _items           = defs;
        _selectionAction = selectionAction;
    }

    protected virtual float RowHeight => 28f;

    protected abstract void DoSingleItem(Rect rect, T item);

    protected abstract bool ItemMatchSearchString(T item);

    protected override void DoContent(Rect inRect)
    {
        if (_useSearchBar)
        {
            var searchRect = inRect.TopPartPixels(25);
            Text.Font = GameFont.Tiny;

            if (Widgets.ButtonImage(searchRect.LeftPartPixels(25), TexButton.OpenInspector)) { }
            searchRect.xMin += 25;
            _searchString   =  Widgets.TextField(searchRect, _searchString).ToLower();
            inRect.yMin     += 30;
        }

        try
        {
            Core.GUIUtility.ScrollView(
                inRect, ref _scrollPosition, _items, item => !_searchString.NullOrEmpty() ? ItemMatchSearchString(item) ? RowHeight : -1f : RowHeight, (rect, item) =>
                {
                    DoSingleItem(rect, item);

                    if (!Widgets.ButtonInvisible(rect)) { return; }
                    _selectionAction.Invoke(item);

                    if (!_integrated) { Close(); }
                }
            );
        }
        catch (Exception er) { Log.Error(er.ToString()); }
    }
}
