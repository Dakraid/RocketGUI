namespace RocketGUI.Core.Graphs;

using Listings;

using System;
using System.Collections.Generic;

using UnityEngine;

using Verse;

public class Grapher
{
    public const int GraphMaxPointsNum = 1500;

    public const int Scales = 4;

    private readonly ListingCollapsible _collapsible = new(scrollViewOnOverflow: false);

    public string _description = string.Empty;

    private readonly List<Action<Rect>> _header;

    private bool _mouseIsOver;

    private GraphPoint _mouseIsOverPoint = new(0, 0, Color.white);

    private readonly GraphPointCollection _points = new();

    private readonly List<GraphPoint> _pointsQueue = new();

    public string _title = string.Empty;

    public Grapher(string title, string description = null)
    {
        _title       = title;
        _description = description ?? string.Empty;

        _header = new List<Action<Rect>>
        {
            rect =>
            {
                Text.Font   =  GameFont.Tiny;
                Text.Anchor =  TextAnchor.MiddleLeft;
                rect.xMin   += 25;
                Widgets.Label(rect, $"Min T:<color=cyan>{Math.Round(MinT, 4)}</color>");
            },
            rect =>
            {
                if (_mouseIsOver)
                {
                    Text.Font   = GameFont.Tiny;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect, $"Current:(<color=cyan>{Math.Round(_mouseIsOverPoint._t, 4)}</color>,<color=cyan>{Math.Round(_mouseIsOverPoint._y, 4)}</color>)");
                }
            },
            rect =>
            {
                Text.Font   = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleRight;
                Widgets.Label(rect, $"Max T:<color=cyan>{Math.Round(MinT + RangeT, 4)}</color>");
            }
        };
    }

    private IEnumerable<GraphPoint> Range => _points.Points;

    private float RangeT => _points.RangeT;

    private float RangeY => _points.RangeY;

    public float MinY => _points.MinY;

    public float MaxY => _points.MaxY;

    public float MinT => _points.MinT;

    public float MaxT => _points.MaxT;

    public float TimeWindowSize
    {
        get => _points.TargetTimeWindowSize;
        set => _points.TargetTimeWindowSize = value;
    }

    public ListingCollapsible.GroupCollapsible Group
    {
        get => _collapsible.Group;
        set => _collapsible.Group = value;
    }

    public float this[float t]
    {
        set => Add(t, value);
    }

    public void Add(float t, float y) => Add(t, y, Color.cyan);

    public void Add(float t, float y, Color color)
    {
        var point = new GraphPoint();
        point._t     = t;
        point._y     = y;
        point._color = color;
        _pointsQueue.Add(point);
    }

    public void Dirty() => _points.Rebuild();

    public void Plot(ref Rect inRect)
    {
        if (_pointsQueue.Count > 0 && _collapsible.Expanded)
        {
            foreach (var point in _pointsQueue) { _points.Add(point); }
            _points.Rebuild();
            _pointsQueue.Clear();
        }
        _collapsible.Begin(inRect, _title);

        if (_points.Ready && _points.Count > 24)
        {
            GUI.color = Color.white;
            _collapsible.Columns(15, _header);
            _collapsible.Line(1);
            _collapsible.Lambda(100, Draw);

            if (!_description.NullOrEmpty())
            {
                _collapsible.Line(1);
                _collapsible.Label(_description);
            }
        }
        _collapsible.End(ref inRect);
    }

    private void Draw(Rect rect)
    {
        Widgets.DrawBoxSolid(rect, Color.black);

        if (!_points.Ready)
        {
            Text.Font   = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, "Preparing");

            return;
        }
        GUI.color   = Color.white;
        Text.Font   = GameFont.Tiny;
        Text.Anchor = TextAnchor.MiddleLeft;

        rect = rect.ContractedBy(5);
        var width  = rect.width;
        var height = rect.height;

        var textRect   = new Rect(Vector2.zero, Text.CalcSize("0.00000"));
        var textOffset = textRect.width + 5;
        var x0         = rect.xMin;
        var x1         = rect.xMax;

        for (var i = 0; i <= 5; i++)
        {
            var y = height * i / 5;
            textRect.x = x0;
            textRect.y = rect.yMax - y - textRect.height / 2;
            Widgets.DrawLine(new Vector2(x0 + 2 + textOffset, rect.yMax - y), new Vector2(x1 - 2, rect.yMax - y), Color.gray, 1);
            Widgets.Label(textRect, $"{Math.Round(MinY + RangeY * (i / 5f), 3)}");
        }

        width     -= textOffset;
        rect.xMin += textOffset;

        _mouseIsOver = false;

        var v0 = new Vector2();
        var v1 = new Vector2();

        v0.x = rect.xMin;
        v0.y = rect.yMax - (_points.First._y - MinY) / RangeY * height;

        var hoverRect = new Rect(v0.x, rect.y + 2, 0, rect.height - 2);

        foreach (var p in Range)
        {
            v1.x = rect.xMin + (p._t - MinT) / RangeT * width;
            v1.y = rect.yMax - (p._y - MinY) / RangeY * height;

            hoverRect.xMin = v0.x;
            hoverRect.xMax = v1.x;
            Widgets.DrawLine(v0, v1, p._color, 1);

            if (Mouse.IsOver(hoverRect))
            {
                Widgets.DrawBoxSolid(hoverRect.RightPartPixels(1), Color.gray);
                _mouseIsOverPoint = p;
                _mouseIsOver      = true;
            }
            v0 = v1;
        }
    }

    public struct GraphPoint
    {
        public float _t;

        public float _y;

        public Color _color;

        public GraphPoint(float t, float y, Color color)
        {
            _t     = t;
            _y     = y;
            _color = color;
        }
    }
}
