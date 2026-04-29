#nullable enable

using System;
using System.Collections.Generic;
using Godot;
using Maze.Generators;
using Maze.Model;
using Maze.Solvers;
using Maze.UI;
using Maze.Views;

namespace Maze;

public partial class Main : Node
{
    private Hud _hud = null!;
    private MazeView2D _view2D = null!;
    private MazeView3D _view3D = null!;
    private AlgorithmRunner _runner = null!;
    private global::Maze.Model.Maze? _currentMaze;
    private global::Maze.Model.Maze? _lastMazeBuiltFor3D;
    private Cell _solverStart = null!;
    private Cell _solverGoal = null!;

    private readonly Dictionary<string, IMazeGenerator> _generators = new()
    {
        ["recursive-backtracker"] = new RecursiveBacktrackerGenerator(),
        ["growing-tree"] = new GrowingTreeGenerator(),
        ["recursive-division"] = new RecursiveDivisionGenerator(),
        ["cellular-automata"] = new CellularAutomataGenerator()
    };

    private readonly Dictionary<string, IMazeSolver> _solvers = new()
    {
        ["bfs"] = new BreadthFirstSolver(),
        ["dfs"] = new DepthFirstSolver(),
        ["a-star"] = new AStarSolver(),
        ["greedy"] = new GreedyBestFirstSolver()
    };

    private readonly Random _random = new();

    public override void _Ready()
    {
        _hud = GetNode<Hud>("Hud");
        _view2D = GetNode<MazeView2D>("MazeView2D");
        _view3D = GetNode<MazeView3D>("MazeView3D");
        _runner = GetNode<AlgorithmRunner>("Runner");

        _view2D.Visible = true;
        _view3D.Visible = false;

        _hud.GenerateRequested += OnGenerateRequested;
        _hud.SolveRequested += OnSolveRequested;
        _hud.SpeedChanged += OnSpeedChanged;
        _hud.PauseToggle += OnPauseToggled;
        _hud.StepRequested += OnStepRequested;
        _hud.ResetRequested += OnResetRequested;
        _hud.ViewToggleRequested += OnViewToggled;

        _runner.GenerationStepProduced += OnGenerationStepProduced;
        _runner.GenerationFinished += OnGenerationFinished;
        _runner.SolverStepProduced += OnSolverStepProduced;
        _runner.SolverFinished += OnSolverFinished;
        _runner.StepsPerSecond = 30f;

        GD.Print("[Main] HUD, 2D-View und 3D-View verbunden.");
    }

    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {
    }

    public override void _ExitTree() => GD.Print("[Main] _ExitTree.");

    private void OnGenerateRequested(int width, int height, string generatorId)
    {
        if (!_generators.TryGetValue(generatorId, out IMazeGenerator? generator))
        {
            GD.PrintErr($"Unbekannter Generator: {generatorId}");
            return;
        }

        _runner.StopAll();
        _currentMaze = new global::Maze.Model.Maze(width, height);
        _lastMazeBuiltFor3D = null;
        _view2D.SetMaze(_currentMaze);
        _view3D.ClearMaze();

        _runner.StartGeneration(generator.Generate(_currentMaze, _random));
        GD.Print($"[Main] Generator {generator.Name} gestartet.");
    }

    private void OnGenerationStepProduced()
    {
        GenerationStep? step = _runner.LastGenerationStep;
        if (step is null || _currentMaze is null)
        {
            return;
        }

        step.Cell.State = step.NewState;
        _view2D.Refresh();
    }

    private void OnGenerationFinished()
    {
        if (_currentMaze is null)
        {
            return;
        }

        foreach (Cell cell in _currentMaze.AllCells())
        {
            cell.State = CellState.Open;
        }

        _view2D.Refresh();
        _view3D.SetMaze(_currentMaze);
        _lastMazeBuiltFor3D = _currentMaze;
        GD.Print("[Main] Generator fertig.");
    }

    private void OnSolveRequested(string solverId)
    {
        if (_currentMaze is null)
        {
            GD.PrintErr("Kein Maze.");
            return;
        }

        if (!_solvers.TryGetValue(solverId, out IMazeSolver? solver))
        {
            GD.PrintErr($"Unbekannter Solver: {solverId}");
            return;
        }

        _currentMaze.ResetSolverState();
        _solverStart = _currentMaze.GetCell(0, 0);
        _solverGoal = _currentMaze.GetCell(_currentMaze.Width - 1, _currentMaze.Height - 1);
        _solverStart.State = CellState.Start;
        _solverGoal.State = CellState.Goal;
        _view2D.Refresh();
        _view3D.Refresh();

        _runner.StopAll();
        _runner.StartSolver(solver.Solve(_currentMaze, _solverStart, _solverGoal));
    }

    private void OnSolverStepProduced()
    {
        SolverStep? step = _runner.LastSolverStep;
        if (step is null)
        {
            return;
        }

        if (step.Cell == _solverStart)
        {
            step.Cell.State = CellState.Start;
        }
        else if (step.Cell == _solverGoal)
        {
            step.Cell.State = CellState.Goal;
        }
        else
        {
            step.Cell.State = step.NewState;
        }

        step.Cell.Distance = step.Distance;
        _view2D.Refresh();
    }

    private void OnSolverFinished()
    {
        GD.Print("[Main] Solver fertig.");
        _view2D.Refresh();
    }

    private void OnSpeedChanged(float stepsPerSecond) =>
        _runner.StepsPerSecond = stepsPerSecond;

    private void OnPauseToggled(bool paused) =>
        _runner.IsPaused = paused;

    private void OnStepRequested() =>
        _runner.ForceSingleStep();

    private void OnResetRequested()
    {
        _runner.StopAll();
        _currentMaze = null;
        _lastMazeBuiltFor3D = null;
        _view2D.SetMaze(new global::Maze.Model.Maze(2, 2));
        _view3D.ClearMaze();
        GD.Print("[Main] Reset.");
    }

    private void OnViewToggled(bool use3D)
    {
        _view2D.Visible = !use3D;
        _view3D.Visible = use3D;

        if (use3D && _currentMaze is not null && !ReferenceEquals(_lastMazeBuiltFor3D, _currentMaze))
        {
            _view3D.SetMaze(_currentMaze);
            _lastMazeBuiltFor3D = _currentMaze;
        }

        GD.Print($"[Main] 3D-Ansicht = {use3D}");
    }
}