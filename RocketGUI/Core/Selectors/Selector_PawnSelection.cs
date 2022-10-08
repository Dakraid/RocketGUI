namespace RocketGUI.Core.Selectors;

using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

public class SelectorPawnSelection : SelectorGenericSelection<Pawn>
{
    public SelectorPawnSelection(IEnumerable<Pawn> defs, Action<Pawn> selectionAction, bool integrated = false, Action closeAction = null) : base(defs, selectionAction, integrated, closeAction) { }

    protected override void DoSingleItem(Rect rect, Pawn item) =>
        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.DrawTextureFitted(rect.LeftPartPixels(50), PortraitsCache.Get(item, new Vector2(50, 50), item.Rotation), 1);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rect.position + new Vector2(60, 0), rect.size - new Vector2(60, 0)), item.Name.ToStringFull);
            }
        );

    protected override bool ItemMatchSearchString(Pawn item) => item.Name.ToStringFull.ToLower().Contains(_searchString.ToLower());
}
