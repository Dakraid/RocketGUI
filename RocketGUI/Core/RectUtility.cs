namespace RocketGUI.Core;

using System;
using UnityEngine;

public static class RectUtility
{
    public static Rect[] Columns(this Rect rect, int pieces, float gap = 5f)
    {
        if (pieces <= 1) { throw new InvalidOperationException("Can't divide into 1 or less pieces"); }
        var step    = rect.width / pieces - gap * (pieces - 1);
        var rects   = new Rect[pieces];
        var current = new Rect(rect.position, new Vector2(step, rect.height));

        for (var i = 0; i < pieces; i++)
        {
            rects[i]  =  new Rect(current);
            current.x += step;
        }

        return rects;
    }

    public static Rect SliceXPixels(this ref Rect inRect, float pixels)
    {
        var rect = new Rect(inRect.x, inRect.y, Mathf.Min(inRect.width, pixels), inRect.height);
        inRect.xMin += rect.width;

        return rect;
    }

    public static Rect SliceYPixels(this ref Rect inRect, float pixels)
    {
        var rect = new Rect(inRect.x, inRect.y, inRect.width, Mathf.Min(inRect.height, pixels));
        inRect.yMin += rect.height;

        return rect;
    }

    public static Rect SliceXPart(this ref Rect inRect, float part)
    {
        var rect = new Rect(inRect.x, inRect.y, inRect.width * part, inRect.height);
        inRect.xMin += rect.width;

        return rect;
    }

    public static Rect SliceYPart(this ref Rect inRect, float part)
    {
        var rect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height * part);
        inRect.yMin += rect.height;

        return rect;
    }

    public static Rect[] Rows(this Rect rect, int pieces, float gap = 5f)
    {
        if (pieces <= 1) { throw new InvalidOperationException("Can't divide into 1 or less pieces"); }
        var step    = rect.height / pieces - gap * (pieces - 1);
        var rects   = new Rect[pieces];
        var current = new Rect(rect.position, new Vector2(rect.width, step));

        for (var i = 0; i < pieces; i++)
        {
            rects[i]  =  new Rect(current);
            current.y += step;
        }

        return rects;
    }
}
