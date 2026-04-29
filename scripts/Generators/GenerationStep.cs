#nullable enable

using Maze.Model;

namespace Maze.Generators;

/// <summary>
/// Beschreibt einen einzelnen Animationsschritt eines Generators.
/// </summary>
public sealed record GenerationStep(
    Cell Cell,
    Cell? Neighbor,
    Direction? RemoveWallTowards,
    CellState NewState,
    string Description
);