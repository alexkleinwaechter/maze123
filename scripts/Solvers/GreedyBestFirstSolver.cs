#nullable enable

using System;
using System.Collections.Generic;
using Maze.Model;

namespace Maze.Solvers;

/// <summary>
/// Greedy Best-First nutzt nur die Heuristik h(n), keine Pfadkosten g(n).
/// Sehr schnell, aber findet meist nicht den kuerzesten Pfad.
/// </summary>
public sealed class GreedyBestFirstSolver : IMazeSolver
{
    public string Id => "greedy";
    public string Name => "Greedy Best-First";

    public IEnumerable<SolverStep> Solve(global::Maze.Model.Maze maze, Cell start, Cell goal)
    {
        long counter = 0;
        SortedSet<(int h, long counter, Cell cell)> openSet = new(
            Comparer<(int h, long counter, Cell cell)>.Create((a, b) =>
            {
                int byH = a.h.CompareTo(b.h);
                if (byH != 0)
                {
                    return byH;
                }

                return a.counter.CompareTo(b.counter);
            }));

        Dictionary<Cell, Cell?> cameFrom = new() { [start] = null };
        openSet.Add((Heuristic(start, goal), counter++, start));
        yield return new SolverStep(start, CellState.Frontier, 0, "Start");

        while (openSet.Count > 0)
        {
            (int _, long __, Cell current) = openSet.Min;
            openSet.Remove(openSet.Min);

            if (current.State != CellState.Start && current.State != CellState.Goal)
            {
                yield return new SolverStep(current, CellState.Visited, Heuristic(current, goal), "Expand");
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
                if (neighbor is null || cameFrom.ContainsKey(neighbor))
                {
                    continue;
                }

                int heuristic = Heuristic(neighbor, goal);
                cameFrom[neighbor] = current;
                openSet.Add((heuristic, counter++, neighbor));

                if (neighbor != goal)
                {
                    yield return new SolverStep(neighbor, CellState.Frontier, heuristic, "Open");
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