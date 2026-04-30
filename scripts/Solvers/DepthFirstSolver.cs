#nullable enable

using System.Collections.Generic;
using Maze.Model;

namespace Maze.Solvers;

/// <summary>
/// DFS-Solver. Findet einen Pfad, aber nicht zwingend den kuerzesten.
/// Visuell wie eine Schlange, die sich tief in einen Ast schlaengelt.
/// </summary>
public sealed class DepthFirstSolver : IMazeSolver
{
    public string Id => "dfs";
    public string Name => "Depth-First Search";

    public IEnumerable<SolverStep> Solve(global::Maze.Model.Maze maze, Cell start, Cell goal)
    {
        Stack<Cell> stack = new();
        Dictionary<Cell, Cell?> cameFrom = new();
        Dictionary<Cell, int> depth = new();

        stack.Push(start);
        cameFrom[start] = null;
        depth[start] = 0;
        yield return new SolverStep(start, CellState.Frontier, 0, "Start auf Stack");

        while (stack.Count > 0)
        {
            Cell current = stack.Pop();
            if (current.State != CellState.Start && current.State != CellState.Goal)
            {
                yield return new SolverStep(current, CellState.Visited, depth[current], "Visit");
            }

            if (current == goal)
            {
                break;
            }

            foreach (Direction direction in DirectionHelper.All)
            {
                if (current.HasWall(direction))
                {
                    continue;
                }

                Cell? next = maze.GetNeighbor(current, direction);
                if (next is null || cameFrom.ContainsKey(next))
                {
                    continue;
                }

                cameFrom[next] = current;
                depth[next] = depth[current] + 1;
                stack.Push(next);

                if (next != goal)
                {
                    yield return new SolverStep(next, CellState.Frontier, depth[next], "Push");
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            yield break;
        }

        List<Cell> path = new();
        for (Cell? cell = goal; cell != null; cell = cameFrom[cell])
        {
            path.Add(cell);
        }

        path.Reverse();
        for (int index = 0; index < path.Count; index++)
        {
            yield return new SolverStep(path[index], CellState.Path, index, "Path");
        }
    }
}