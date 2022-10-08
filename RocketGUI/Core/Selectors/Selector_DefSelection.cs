namespace RocketGUI.Core.Selectors;

using System;
using System.Collections.Generic;

using UnityEngine;

using Verse;

public class SelectorDefSelection : SelectorGenericSelection<Def>
{
    public SelectorDefSelection(IEnumerable<Def> defs, Action<Def> selectionAction, bool integrated = false, Action closeAction = null) : base(defs, selectionAction, integrated, closeAction) { }

    protected override void DoSingleItem(Rect rect, Def item)
    {
        Widgets.DrawHighlightIfMouseover(rect);
        Widgets.DefLabelWithIcon(rect, item);
    }

    protected override bool ItemMatchSearchString(Def item) => item.label?.ToLower()?.Contains(_searchString.ToLower()) ?? true;
}
