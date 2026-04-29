#nullable enable

using System;
using System.Collections.Generic;
using Godot;
using Maze.Generators;
using Maze.Model;
using Maze.UI;
using Maze.Views;

namespace Maze;

public partial class Main : Node
{
    private Hud _hud = null!;
    private MazeView2D _view2D = null!;
    private AlgorithmRunner _runner = null!;
    private global::Maze.Model.Maze? _currentMaze;

    private readonly Dictionary<string, IMazeGenerator> _generators = new()
    {
        ["recursive-backtracker"] = new RecursiveBacktrackerGenerator(),
        ["growing-tree"] = new GrowingTreeGenerator(),
        ["recursive-division"] = new RecursiveDivisionGenerator(),
        ["cellular-automata"] = new CellularAutomataGenerator()
    };

    private readonly Random _random = new();

    public override void _Ready()
    {
        _hud = GetNode<Hud>("Hud");
        _view2D = GetNode<MazeView2D>("MazeView2D");
        _runner = GetNode<AlgorithmRunner>("Runner");

        _hud.GenerateRequested += OnGenerateRequested;
        _hud.SolveRequested += OnSolveRequested;
        _hud.SpeedChanged += OnSpeedChanged;
        _hud.PauseToggle += OnPauseToggled;
        _hud.StepRequested += OnStepRequested;
        _hud.ResetRequested += OnResetRequested;
        _hud.ViewToggleRequested += OnViewToggled;

        _runner.GenerationStepProduced += OnGenerationStepProduced;
        _runner.GenerationFinished += OnGenerationFinished;
        _runner.StepsPerSecond = 30f;

        GD.Print("[Main] HUD + 2D-View verbunden.");
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
        _view2D.SetMaze(_currentMaze);

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
        GD.Print("[Main] Generator fertig.");
    }

    private void OnSolveRequested(string solverId) =>
        GD.Print($"[Main] Solve mit {solverId}");

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
        _view2D.SetMaze(new global::Maze.Model.Maze(2, 2));
        GD.Print("[Main] Reset.");
    }

    private void OnViewToggled(bool use3D) =>
        GD.Print($"[Main] 3D-Ansicht = {use3D}");
}