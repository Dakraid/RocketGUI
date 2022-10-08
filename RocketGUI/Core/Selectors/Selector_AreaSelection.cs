namespace RocketGUI.Core.Selectors;

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

public class SelectorAreaSelection : SelectorGenericSelection<Area>
{
    public SelectorAreaSelection(IEnumerable<Area> areas, Action<Area> selectionAction, bool integrated = false, Action closeAction = null) : base(areas, selectionAction, integrated, closeAction) { }

    protected override float RowHeight => 25f;

    protected override void DoSingleItem(Rect rect, Area item)
    {
        var color = item.Color;

        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                color.a     = 0.5f;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.Label(rect.RightPart(0.8f), item.Label);
                Widgets.DrawBoxSolid(rect.LeftPartPixels(10f), color);
            }
        );
    }

    protected override bool ItemMatchSearchString(Area item) => item.Label.ToLower().Contains(_searchString.ToLower());
}
