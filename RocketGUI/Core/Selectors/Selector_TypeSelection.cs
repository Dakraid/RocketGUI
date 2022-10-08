namespace RocketGUI.Core.Selectors;

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

public class SelectorTypeSelection : SelectorGenericSelection<Type>
{
    private static readonly Dictionary<Type, string> _cache = new();
    private readonly int _count;
    private readonly Type[] _types;
    private Rect _viewRect = Rect.zero;

    public SelectorTypeSelection(Type t, Action<Type> selectionAction, bool integrated = false, Action closeAction = null) : base(t.AllSubclassesNonAbstract(), selectionAction, integrated, closeAction)
    {
        _types = t.AllSubclassesNonAbstract().ToArray();
        _count = _types.Length;
    }

    protected override float RowHeight => 24f;

    protected override void DoContent(Rect inRect) => FillTypeContent(inRect);

    private void FillTypeContent(Rect inRect)
    {
        try
        {
            Core.GUIUtility.ScrollView(
                inRect, ref _scrollPosition, _types, type => !_searchString.NullOrEmpty() ? ItemMatchSearchString(type) ? -1f : RowHeight : RowHeight, (rect, type) =>
                {
                    DoSingleItem(rect, type);

                    if (Widgets.ButtonInvisible(rect))
                    {
                        _selectionAction.Invoke(type);

                        if (!_integrated) { Close(); }
                    }
                }
            );
        }
        catch (Exception er) { Log.Error(er.ToString()); }
    }

    protected override void DoSingleItem(Rect rect, Type item)
    {
        if (!SelectorTypeSelection._cache.TryGetValue(item, out var name)) { name = SelectorTypeSelection._cache[item] = item.Name.Translate(); }
        Widgets.DrawHighlightIfMouseover(rect);
        Widgets.Label(rect, name);
    }

    protected override bool ItemMatchSearchString(Type item) => true;
}
