using System;
using System.Collections.Generic;
using Maze.Model;

namespace Maze.Generators;

/// <summary>
/// Gemeinsame Schnittstelle aller Erstellungsalgorithmen.
/// </summary>
public interface IMazeGenerator
{
    string Id { get; }
    string Name { get; }

    IEnumerable<GenerationStep> Generate(global::Maze.Model.Maze maze, Random random);
}