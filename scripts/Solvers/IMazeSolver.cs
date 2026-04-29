using System.Collections.Generic;
using Maze.Model;

namespace Maze.Solvers;

public interface IMazeSolver
{
    string Id { get; }
    string Name { get; }

    /// <summary>
    /// Liefert Animationsschritte. Implementierung markiert am Ende den finalen Pfad
    /// ueber SolverStep-Eintraege mit CellState.Path.
    /// </summary>
    IEnumerable<SolverStep> Solve(global::Maze.Model.Maze maze, Cell start, Cell goal);
}