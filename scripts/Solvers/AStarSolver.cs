#nullable enable

using System;
using System.Collections.Generic;
using Maze.Model;

namespace Maze.Solvers;

/// <summary>
/// A*-Solver mit Manhattan-Heuristik fuer 4er-Nachbarschaft.
/// f(n) = g(n) + h(n), expandiert immer das niedrigste f.
/// </summary>
public sealed class AStarSolver : IMazeSolver
{
    public string Id => "a-star";
    public string Name => "A* (Manhattan)";

    public IEnumerable<SolverStep> Solve(global::Maze.Model.Maze maze, Cell start, Cell goal)
    {
        long counter = 0;
        SortedSet<(int f, long counter, Cell cell)> openSet = new(
            Comparer<(int f, long counter, Cell cell)>.Create((a, b) =>
            {
                int byF = a.f.CompareTo(b.f);
                if (byF != 0)
                {
                    return byF;
                }

                return a.counter.CompareTo(b.counter);
            }));

        Dictionary<Cell, int> gScore = new() { [start] = 0 };
        Dictionary<Cell, Cell?> cameFrom = new() { [start] = null };

        openSet.Add((Heuristic(start, goal), counter++, start));
        yield return new SolverStep(start, CellState.Frontier, 0, "Start");

        while (openSet.Count > 0)
        {
            (int _, long __, Cell current) = openSet.Min;
            openSet.Remove(openSet.Min);

            if (current.State != CellState.Start && current.State != CellState.Goal)
            {
                yield return new SolverStep(current, CellState.Visited, gScore[current], "Expand");
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

                Cell? neighbor = maze.GetNeighbor(current, direction);
                if (neighbor is null)
                {
                    continue;
                }

                int tentativeG = gScore[current] + 1;
                if (gScore.TryGetValue(neighbor, out int known) && tentativeG >= known)
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeG;
                int f = tentativeG + Heuristic(neighbor, goal);
                openSet.Add((f, counter++, neighbor));

                if (neighbor != goal)
                {
                    yield return new SolverStep(neighbor, CellState.Frontier, tentativeG, $"Open f={f}");
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

    private static int Heuristic(Cell a, Cell b) =>
        Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
}