namespace RocketGUI.Core;

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

public static partial class GUIUtility
{
    private static readonly Color _altGray = new(0.2f, 0.2f, 0.2f);
    private static float[] _heights = new float[5000];

    public static Exception ExecuteSafeGUIAction(Action function, Action fallbackAction = null, bool catchExceptions = false)
    {
        Core.GUIUtility.StashGUIState();
        Exception exception = null;

        try { function.Invoke(); }
        catch (Exception er)
        {
            Log.Error($"ROCKETMAN:UI error in ExecuteSafeGUIAction {er}");
            exception = er;
        }
        finally { Core.GUIUtility.RestoreGUIState(); }

        if (exception == null || catchExceptions) { return exception; }

        if (fallbackAction != null) { exception = Core.GUIUtility.ExecuteSafeGUIAction(fallbackAction, catchExceptions: false); }

        if (exception != null) { throw exception; }

        return null;
    }

    public static void ScrollView<T>(
        Rect rect,
        ref Vector2 scrollPosition,
        IEnumerable<T> elements,
        Func<T, float> heightLambda,
        Action<Rect, T> elementLambda,
        Func<T, IComparable> orderByLambda = null,
        bool drawBackground = true,
        bool showScrollbars = true,
        bool catchExceptions = false,
        bool drawMouseOverHighlights = true
    )
    {
        Core.GUIUtility.StashGUIState();
        Exception exception = null;

        try
        {
            if (drawBackground)
            {
                Widgets.DrawMenuSection(rect);
                rect = rect.ContractedBy(2);
            }
            var contentRect = new Rect(0, 0, showScrollbars ? rect.width - 23 : rect.width, 0);
            var elementsInt = orderByLambda == null ? elements : elements.OrderBy(orderByLambda);

            var elementList = elementsInt.ToList();

            if (Core.GUIUtility._heights.Length < elementList.Count()) { Core.GUIUtility._heights = new float[elementList.Count() * 2]; }
            var   w      = showScrollbars ? rect.width - 16 : rect.width;
            var   j      = 0;
            var   k      = 0;
            var   inView = true;

            foreach (var h in elementList.Select(heightLambda.Invoke))
            {
                Core.GUIUtility._heights[j++] =  h;
                contentRect.height            += Math.Max(h, 0f);
            }
            j = 0;
            Widgets.BeginScrollView(rect, ref scrollPosition, contentRect, showScrollbars);
            var currentRect = new Rect(1, 0, w, 0);

            foreach (var element in elementList)
            {
                if (Core.GUIUtility._heights[j] <= 0.00f)
                {
                    j++;

                    continue;
                }
                currentRect.height = Core.GUIUtility._heights[j];

                if (scrollPosition.y - 50 > currentRect.yMax || scrollPosition.y + 50 + rect.height < currentRect.yMin) { inView = false; }

                if (inView)
                {
                    if (drawBackground && k % 2 == 0) { Widgets.DrawBoxSolid(currentRect, Core.GUIUtility._altGray); }

                    if (drawMouseOverHighlights) { Widgets.DrawHighlightIfMouseover(currentRect); }
                    elementLambda.Invoke(currentRect, element);
                }
                currentRect.y += Core.GUIUtility._heights[j];
                k++;
                j++;
                inView = true;
            }
        }
        catch (Exception er)
        {
            Log.Error($"ROCKETMAN:UI error in ScrollView {er}");
            exception = er;
        }
        finally
        {
            Core.GUIUtility.RestoreGUIState();
            Widgets.EndScrollView();
        }

        if (exception != null && !catchExceptions) { throw exception; }
    }

    public static void GridView<T>(Rect rect, int columns, List<T> elements, Action<Rect, T> cellLambda, bool drawBackground = true, bool drawVerticalDivider = false) =>
        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                if (drawBackground) { Widgets.DrawMenuSection(rect); }
                rect = rect.ContractedBy(1);
                var rows       = (int) Math.Ceiling((decimal) elements.Count / columns);
                var columnStep = rect.width / columns;
                var rowStep    = rect.height / rows;
                var curRect    = new Rect(0, 0, columnStep, rowStep);
                var k          = 0;

                for (var i = 0; i < columns && k < elements.Count; i++)
                {
                    curRect.x = i * columnStep + rect.x;

                    for (var j = 0; j < rows && k < elements.Count; j++)
                    {
                        curRect.y = j * rowStep + rect.y;
                        cellLambda(curRect, elements[k++]);
                    }
                }
            }
        );

    public static void DropDownMenu<T>(Func<T, string> labelLambda, Action<T> selectedLambda, T[] options) => Core.GUIUtility.DropDownMenu(labelLambda, selectedLambda, options.AsEnumerable());

    public static void DropDownMenu<T>(Func<T, string> labelLambda, Action<T> selectedLambda, IEnumerable<T> options) =>
        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                Text.Font = GameFont.Small;

                FloatMenuUtility.MakeMenu(
                    options, labelLambda, option =>
                    {
                        return () => selectedLambda(option);
                    }
                );
            }
        );

    public static void Row(Rect rect, List<Action<Rect>> contentLambdas, bool drawDivider = true, bool drawBackground = false) =>
        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                if (drawBackground) { Widgets.DrawMenuSection(rect); }
                var step    = rect.width / contentLambdas.Count;
                var curRect = new Rect(rect.x - 5, rect.y, step - 10, rect.height);

                for (var i = 0; i < contentLambdas.Count; i++)
                {
                    var lambda = contentLambdas[i];

                    if (drawDivider && i + 1 < contentLambdas.Count)
                    {
                        var start = new Vector2(curRect.xMax + 5, curRect.yMin + 1);
                        var end   = new Vector2(curRect.xMax + 5, curRect.yMax - 1);
                        Widgets.DrawLine(start, end, Color.white, 1);
                    }

                    Core.GUIUtility.ExecuteSafeGUIAction(
                        () =>
                        {
                            lambda.Invoke(curRect);
                            curRect.x += step;
                        }
                    );
                }
            }
        );

    public static void CheckBoxLabeled(
        Rect rect,
        string label,
        ref bool checkOn,
        bool disabled = false,
        bool monotone = false,
        float iconWidth = 20,
        GameFont font = GameFont.Tiny,
        FontStyle fontStyle = FontStyle.Normal,
        bool placeCheckboxNearText = false,
        bool drawHighlightIfMouseover = true,
        Texture2D texChecked = null,
        Texture2D texUnchecked = null
    )
    {
        var checkOnInt = checkOn;

        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                Text.Font                   = font;
                Text.Anchor                 = TextAnchor.MiddleLeft;
                Text.CurFontStyle.fontStyle = fontStyle;

                if (placeCheckboxNearText) { rect.width = Mathf.Min(rect.width, Text.CalcSize(label).x + 24f + 10f); }
                Widgets.Label(rect, label);

                if (!disabled && Widgets.ButtonInvisible(rect))
                {
                    checkOnInt = !checkOnInt;

                    if (checkOnInt) { SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(); }
                    else { SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(); }
                }
                var iconRect = new Rect(0f, 0f, iconWidth, iconWidth)
                {
                    center = rect.RightPartPixels(iconWidth).center
                };
                var color = GUI.color;

                if (disabled || monotone) { GUI.color = Widgets.InactiveColor; }

                GUI.DrawTexture(
                    image: checkOnInt ? texChecked != null ? texChecked : Widgets.CheckboxOnTex :
                           texUnchecked != null ? texUnchecked : Widgets.CheckboxOffTex, position: iconRect
                );

                if (disabled || monotone) { GUI.color = color; }

                if (drawHighlightIfMouseover) { Widgets.DrawHighlightIfMouseover(rect); }
            }
        );
        checkOn = checkOnInt;
    }

    public static void ColorBoxDescription(Rect rect, Color color, string description)
    {
        var textRect = new Rect(rect.x + 30, rect.y, rect.width - 30, rect.height);
        var boxRect  = new Rect(0, 0, 10, 10)
        {
            center = new Vector2(rect.xMin + 15, rect.yMin + rect.height / 2)
        };

        Core.GUIUtility.ExecuteSafeGUIAction(
            () =>
            {
                Text.Anchor                 = TextAnchor.MiddleLeft;
                Text.Font                   = GameFont.Tiny;
                Text.CurFontStyle.fontStyle = FontStyle.Normal;
                Widgets.DrawBoxSolid(boxRect, color);
                Widgets.Label(textRect, description);
            }
        );
    }
}
