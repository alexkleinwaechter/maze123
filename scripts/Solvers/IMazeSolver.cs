using System.Collections.Generic;
using Maze.Model;

namespace Maze.Solvers;

public interface IMazeSolver
{
    string Id { get; }
    string Name { get; }

    IEnumerable<SolverStep> Solve(global::Maze.Model.Maze maze, Cell start, Cell goal);
}