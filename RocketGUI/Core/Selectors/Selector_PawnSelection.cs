﻿using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RocketGUI
{
    public class Selector_PawnSelection : ISelector_GenericSelection<Pawn>
    {
        public Selector_PawnSelection(IEnumerable<Pawn> defs, Action<Pawn> selectionAction, bool integrated = false,
            Action closeAction = null) : base(defs, selectionAction, integrated, closeAction)
        {
        }

        protected override void DoSingleItem(Rect rect, Pawn item)
        {
            GUIUtility.ExecuteSafeGUIAction(() =>
            {
                Widgets.DrawHighlightIfMouseover(rect);
                Widgets.DrawTextureFitted(rect.LeftPartPixels(50), PortraitsCache.Get(item, new Vector2(50, 50)), 1);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(rect.position + new Vector2(60, 0), rect.size - new Vector2(60, 0)),
                    item.Name.ToStringFull);
            });
        }

        protected override bool ItemMatchSearchString(Pawn item)
        {
            return item.Name.ToStringFull.ToLower().Contains(searchString.ToLower());
        }
    }
}