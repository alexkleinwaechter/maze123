using System.Collections.Generic;
using Godot;
using Maze.Model;

namespace Maze.Views;

/// <summary>
/// Die im 3D-Maze sichtbare Spielfigur. Haelt eine Liste von Wegpunkten
/// (in Welt-Koordinaten) und interpoliert pro Frame zwischen ihnen.
/// </summary>
public partial class PlayerCharacter3D : Node3D
{
    [Signal] public delegate void GoalReachedEventHandler();

    [Export] public float MoveSpeed = 4f;
    [Export] public float StandHeight = 0.5f;

    private readonly List<Vector3> _waypoints = new();
    private int _currentIndex;
    private bool _isMoving;
    private float _cellSize = 1f;

    public bool IsMoving => _isMoving;

    public new void Hide()
    {
        _waypoints.Clear();
        _currentIndex = 0;
        _isMoving = false;
        Visible = false;
    }

    public void StartFollowingPath(List<Cell> path, float cellSize)
    {
        _cellSize = cellSize;
        _waypoints.Clear();

        foreach (Cell cell in path)
        {
            _waypoints.Add(CellToWorld(cell));
        }

        if (_waypoints.Count == 0)
        {
            Hide();
            return;
        }

        Position = _waypoints[0];
        Visible = true;
        _currentIndex = 1;
        _isMoving = _waypoints.Count > 1;

        if (!_isMoving)
        {
            EmitSignal(SignalName.GoalReached);
        }
    }

    public override void _Process(double delta)
    {
        if (!_isMoving)
        {
            return;
        }

        Vector3 target = _waypoints[_currentIndex];
        Vector3 toTarget = target - Position;
        float remaining = toTarget.Length();
        float step = MoveSpeed * _cellSize * (float)delta;

        if (step >= remaining)
        {
            Position = target;
            _currentIndex++;
            if (_currentIndex >= _waypoints.Count)
            {
                _isMoving = false;
                EmitSignal(SignalName.GoalReached);
            }

            return;
        }

        Position += toTarget.Normalized() * step;
    }

    private Vector3 CellToWorld(Cell cell) =>
        new(cell.X * _cellSize + _cellSize / 2f, StandHeight, cell.Y * _cellSize + _cellSize / 2f);
}