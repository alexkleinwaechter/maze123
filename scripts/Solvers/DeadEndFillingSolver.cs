#nullable enable

using System.Collections.Generic;
using Maze.Model;

namespace Maze.Solvers;

/// <summary>
/// Fuellt iterativ Sackgassen, bis nur noch der eindeutige Korridor zwischen
/// Start und Ziel uebrig bleibt.
/// </summary>
public sealed class DeadEndFillingSolver : IMazeSolver
{
    public string Id => "dead-end-filling";
    public string Name => "Dead-End Filling";

    public IEnumerable<SolverStep> Solve(global::Maze.Model.Maze maze, Cell start, Cell goal)
    {
        HashSet<Cell> filled = new();
        int distance = 0;
        bool changed;

        do
        {
            changed = false;

            foreach (Cell cell in maze.AllCells())
            {
                if (filled.Contains(cell) || cell == start || cell == goal)
                {
                    continue;
                }

                if (CountOpenNeighbors(maze, cell, filled) > 1)
                {
                    continue;
                }

                filled.Add(cell);
                changed = true;
                yield return new SolverStep(cell, CellState.Filled, distance, "Fill");
            }

            distance++;
        }
        while (changed);

        Dictionary<Cell, Cell?> cameFrom = new() { [start] = null };
        Queue<Cell> frontier = new();
        frontier.Enqueue(start);

        while (frontier.Count > 0)
        {
            Cell current = frontier.Dequeue();
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
                if (next is null || filled.Contains(next) || cameFrom.ContainsKey(next))
                {
                    continue;
                }

                cameFrom[next] = current;
                frontier.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            yield break;
        }

        List<Cell> path = new();
        for (Cell? cell = goal; cell is not null; cell = cameFrom[cell])
        {
            path.Add(cell);
        }

        path.Reverse();
        for (int index = 0; index < path.Count; index++)
        {
            yield return new SolverStep(path[index], CellState.Path, index, "Path");
        }
    }

    private static int CountOpenNeighbors(global::Maze.Model.Maze maze, Cell cell, HashSet<Cell> filled)
    {
        int count = 0;

        foreach (Direction direction in DirectionHelper.All)
        {
            if (cell.HasWall(direction))
            {
                continue;
            }

            Cell? neighbor = maze.GetNeighbor(cell, direction);
            if (neighbor is null || filled.Contains(neighbor))
            {
                continue;
            }

            count++;
        }

        return count;
    }
}