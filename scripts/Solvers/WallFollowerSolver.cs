#nullable enable

using System.Collections.Generic;
using Maze.Model;

namespace Maze.Solvers;

/// <summary>
/// Linke-Hand-Regel. Funktioniert garantiert in einfach zusammenhaengenden Labyrinthen.
/// Der Agent hat eine Blickrichtung; er versucht zuerst links abzubiegen,
/// dann geradeaus, dann rechts, dann zurueck.
/// </summary>
public sealed class WallFollowerSolver : IMazeSolver
{
    public string Id => "wall-follower";
    public string Name => "Wall Follower (Linke Hand)";

    public IEnumerable<SolverStep> Solve(global::Maze.Model.Maze maze, Cell start, Cell goal)
    {
        Cell current = start;
        Direction facing = Direction.North;
        int step = 0;
        HashSet<Cell> visited = new() { start };
        Dictionary<Cell, Cell?> cameFrom = new() { [start] = null };

        yield return new SolverStep(start, CellState.Frontier, 0, "Start");

        while (current != goal && step < maze.Width * maze.Height * 4)
        {
            bool moved = false;

            foreach (Direction direction in new[] { TurnLeft(facing), facing, TurnRight(facing), TurnAround(facing) })
            {
                if (current.HasWall(direction))
                {
                    continue;
                }

                Cell? next = maze.GetNeighbor(current, direction);
                if (next is null)
                {
                    continue;
                }

                facing = direction;
                if (!cameFrom.ContainsKey(next))
                {
                    cameFrom[next] = current;
                }

                current = next;
                moved = true;

                if (visited.Add(current))
                {
                    step++;
                    yield return new SolverStep(current, CellState.Visited, step, "Walk");
                }
                else
                {
                    yield return new SolverStep(current, CellState.Frontier, step, "Revisit");
                }

                break;
            }

            if (!moved)
            {
                yield break;
            }
        }

        if (current != goal)
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

    private static Direction TurnLeft(Direction direction) =>
        (Direction)(((int)direction + 3) % 4);

    private static Direction TurnRight(Direction direction) =>
        (Direction)(((int)direction + 1) % 4);

    private static Direction TurnAround(Direction direction) =>
        (Direction)(((int)direction + 2) % 4);
}