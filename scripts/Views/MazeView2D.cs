#nullable enable

using System.Collections.Generic;
using Godot;
using Maze.Model;

namespace Maze.Views;

/// <summary>
/// 2D-Visualisierung des Labyrinths. Zeichnet Zellfarben und Waende auf Basis des Modells.
/// </summary>
public partial class MazeView2D : Node2D
{
    [Export] public int CellSizePx = 24;
    [Export] public int WallThicknessPx = 2;
    [Export] public bool ShowDistances = false;

    public static readonly Dictionary<CellState, Color> StateColors = new()
    {
        { CellState.Untouched, new Color("#1e1e1e") },
        { CellState.Carving, new Color("#ffaa00") },
        { CellState.Open, new Color("#2c2c2c") },
        { CellState.Frontier, new Color("#8ab4f8") },
        { CellState.Visited, new Color("#3d5a80") },
        { CellState.Path, new Color("#f6c177") },
        { CellState.Start, new Color("#a3be8c") },
        { CellState.Goal, new Color("#bf616a") },
        { CellState.Filled, new Color("#000000") }
    };

    private static readonly Color WallColor = new("#dcdcdc");
    private static readonly Color HeatmapMin = new("#003366");
    private static readonly Color HeatmapMax = new("#ff6f3c");

    private const int ThrottleThreshold = 250;
    private const double ThrottledRefreshHz = 30.0;

    private CameraController2D _camera = null!;
    private global::Maze.Model.Maze? _maze;
    private bool _refreshDirty;
    private double _refreshAccumulator;

    public override void _Ready()
    {
        _camera = GetNode<CameraController2D>("Camera2D");
    }

    public void SetMaze(global::Maze.Model.Maze maze)
    {
        _maze = maze;
        _refreshDirty = false;
        _refreshAccumulator = 0;
        QueueRedraw();
        _camera.FitToMaze(maze);
    }

    public void Refresh()
    {
        if (_maze is null)
        {
            return;
        }

        if (_maze.Width <= ThrottleThreshold && _maze.Height <= ThrottleThreshold)
        {
            QueueRedraw();
            return;
        }

        _refreshDirty = true;
    }

    public void ForceRefresh()
    {
        _refreshDirty = false;
        _refreshAccumulator = 0;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        if (!_refreshDirty)
        {
            return;
        }

        _refreshAccumulator += delta;
        if (_refreshAccumulator >= 1.0 / ThrottledRefreshHz)
        {
            _refreshAccumulator = 0;
            _refreshDirty = false;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (_maze is null)
        {
            return;
        }

        int maxDistance = ComputeMaxDistance(_maze);

        foreach (Cell cell in _maze.AllCells())
        {
            Rect2 rect = new(
                cell.X * CellSizePx,
                cell.Y * CellSizePx,
                CellSizePx,
                CellSizePx);

            Color fill = ShowDistances && cell.Distance >= 0
                ? Heatmap(cell.Distance, maxDistance)
                : StateColors[cell.State];

            DrawRect(rect, fill, true);
        }

        foreach (Cell cell in _maze.AllCells())
        {
            float x0 = cell.X * CellSizePx;
            float y0 = cell.Y * CellSizePx;
            float x1 = x0 + CellSizePx;
            float y1 = y0 + CellSizePx;

            if (cell.HasWall(Direction.North))
            {
                DrawLine(new Vector2(x0, y0), new Vector2(x1, y0), WallColor, WallThicknessPx);
            }

            if (cell.HasWall(Direction.West))
            {
                DrawLine(new Vector2(x0, y0), new Vector2(x0, y1), WallColor, WallThicknessPx);
            }

            if (cell.Y == _maze.Height - 1 && cell.HasWall(Direction.South))
            {
                DrawLine(new Vector2(x0, y1), new Vector2(x1, y1), WallColor, WallThicknessPx);
            }

            if (cell.X == _maze.Width - 1 && cell.HasWall(Direction.East))
            {
                DrawLine(new Vector2(x1, y0), new Vector2(x1, y1), WallColor, WallThicknessPx);
            }
        }
    }

    private static int ComputeMaxDistance(global::Maze.Model.Maze maze)
    {
        int max = 0;
        foreach (Cell cell in maze.AllCells())
        {
            if (cell.Distance > max)
            {
                max = cell.Distance;
            }
        }

        return max;
    }

    private static Color Heatmap(int distance, int maxDistance)
    {
        if (maxDistance <= 0)
        {
            return HeatmapMin;
        }

        float t = (float)distance / maxDistance;
        return HeatmapMin.Lerp(HeatmapMax, t);
    }
}