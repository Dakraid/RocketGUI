namespace RocketGUI.Core.Selectors;

using System;
using UnityEngine;
using Verse;

public abstract class Selector : Window
{
    private readonly Action _closeAction;

    protected bool _integrated;

    protected Vector2 _scrollPosition = Vector2.zero;

    protected Selector(bool integrated = false, Action closeAction = null)
    {
        _integrated  = integrated;
        _closeAction = closeAction;
    }

    public override void DoWindowContents(Rect inRect)
    {
        _integrated = false;

        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                if (Widgets.ButtonText(inRect.BottomPartPixels(30), "Close")) { Close(); }
                inRect.yMax -= 35;
                DoContent(inRect);
            }
        );
    }

    public void DoIntegratedContents(Rect inRect) =>
        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                _integrated = true;
                DoContent(inRect);
            }
        );

    protected abstract void DoContent(Rect inRect);

    public override void Close(bool doCloseSound = true)
    {
        if (!_integrated) { base.Close(doCloseSound); }
        else { _closeAction.Invoke(); }
    }
}
