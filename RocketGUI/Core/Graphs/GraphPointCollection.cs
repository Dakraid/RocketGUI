using static RocketGUI.Core.Graphs.Grapher;

namespace RocketGUI.Core.Graphs;

using JetBrains.Annotations;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class GraphPointCollection
{
    private readonly List<GraphPoint> _points = new();

    private int _maxAge;

    private int _minAge;

    private int _streak;
    private float _timeWindow = 250;

    public bool Ready => Count > 16;

    public int Count => _points.Count;

    public float TargetTimeWindowSize
    {
        get => _timeWindow;

        set
        {
            if (_timeWindow == value) { return; }
            _timeWindow = value;
            Rebuild();
        }
    }

    public float MinY { get; private set; } = float.MaxValue;

    public float MaxY { get; private set; } = float.MinValue;

    public float MinT => First._t;

    public float MaxT => Last._t;

    public float RangeT => Mathf.Min(Last._t - First._t, _timeWindow);

    public float RangeY => MaxY - MinY;

    public GraphPoint First => _points.First();

    public GraphPoint Last => _points.Last();

    public IEnumerable<GraphPoint> Points => _points;

    public void Add(GraphPoint point)
    {
        if (Count < 16)
        {
            Commit(point);

            return;
        }

        if (_points.Count >= 1500) { _points.RemoveAt(0); }

        if (Last._t == point._t)
        {
            Commit(point);

            return;
        }
        var pNm1 = Last;
        var pNm2 = _points[_points.Count - 2];

        if (pNm1._t == pNm2._t)
        {
            Commit(point);

            return;
        }
        var m1 = (pNm1._y - pNm2._y) / (pNm1._t - pNm2._t);
        var m0 = (point._y - pNm1._y) / (point._t - pNm1._t);

        if (Mathf.Abs(m1 - m0) < 1e-3)
        {
            if (_streak++ > 1 && point._color == pNm1._color)
            {
                _points[_points.Count - 1] = point;

                return;
            }
            Commit(point);

            return;
        }
        _streak = 0;

        Commit(point);
    }

    public void Rebuild()
    {
        if (Count < 3) { return; }

        var position = 0;

        while (position < _points.Count - 3 && Last._t - _points[position]._t > _timeWindow) { position++; }

        if (position > 0 && position < _points.Count)
        {
            var p0 = _points[position - 1];
            var p1 = _points[position];

            if (p0._t != p1._t)
            {
                var t1 = Last._t - _timeWindow;
                var m  = (p1._y - p0._y) / (p1._t - p0._t);

                p0._y = m * (t1 - p0._t) + p0._y;
                p0._t = t1;

                _points[position - 1] = p0;
            }
            position -= 2;

            while (position >= 0)
            {
                _points.RemoveAt(position);
                position--;
            }
        }

        if (_maxAge > 0 && _minAge > 0)
        {
            _maxAge = Math.Max(_maxAge - 1, 0);
            _minAge = Math.Max(_minAge - 1, 0);
        }
        else { UpdateCriticalPoints(); }
    }

    private void Commit(GraphPoint point)
    {
        _points.Add(point);

        if (point._y > MaxY)
        {
            _maxAge = Mathf.Min(15, _points.Count);
            MaxY    = point._y;
        }

        if (!(point._y < MinY)) { return; }
        _minAge = Mathf.Min(15, _points.Count);
        MinY    = point._y;
    }

    private void UpdateCriticalPoints()
    {
        var last = Last;

        MinY = last._y;
        MaxY = last._y;

        for (var i = 0; i < Count; i++)
        {
            var point = _points[i];

            if (MinY > point._y)
            {
                _minAge = Math.Min(i, 15);
                MinY    = point._y;
            }

            if (!(MaxY < point._y)) { continue; }
            _maxAge = Math.Min(i, 15);
            MaxY    = point._y;
        }
    }
}
