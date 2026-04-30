#nullable enable

using System.Collections.Generic;
using Maze.Model;

namespace Maze.Solvers;

/// <summary>
/// Klassische Breitensuche. Garantiert kuerzesten Pfad in ungewichteten Gittern.
/// Visualisiert sich als Welle vom Start aus.
/// </summary>
public sealed class BreadthFirstSolver : IMazeSolver
{
    public string Id => "bfs";
    public string Name => "Breadth-First Search";

    public IEnumerable<SolverStep> Solve(global::Maze.Model.Maze maze, Cell start, Cell goal)
    {
        Queue<Cell> queue = new();
        Dictionary<Cell, Cell?> cameFrom = new();
        Dictionary<Cell, int> distances = new();

        queue.Enqueue(start);
        cameFrom[start] = null;
        distances[start] = 0;
        yield return new SolverStep(start, CellState.Frontier, 0, "Start in Frontier");

        while (queue.Count > 0)
        {
            Cell current = queue.Dequeue();
            if (current.State != CellState.Start && current.State != CellState.Goal)
            {
                yield return new SolverStep(current, CellState.Visited, distances[current], "Visit");
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
                distances[next] = distances[current] + 1;
                queue.Enqueue(next);

                if (next != goal)
                {
                    yield return new SolverStep(next, CellState.Frontier, distances[next], "Enqueue");
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