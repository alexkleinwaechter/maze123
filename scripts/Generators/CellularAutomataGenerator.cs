#nullable enable

using System;
using System.Collections.Generic;
using Maze.Model;

namespace Maze.Generators;

/// <summary>
/// Cellular Automata-Generator nach Justin A. Parr (2018).
/// </summary>
public sealed class CellularAutomataGenerator : IMazeGenerator
{
    public string Id => "cellular-automata";
    public string Name => "Cellular Automata (Parr, true maze)";

    private const int BranchProbabilityPercent = 5;
    private const int TurnProbabilityPercent = 10;
    private const int SafetyMaxTicks = 1_000_000;

    private enum CaState : byte
    {
        Disconnected = 0,
        Seed = 1,
        Invite = 2,
        Connected = 3
    }

    private struct CellInfo
    {
        public CaState State;
        public Direction? ConnectVector;
        public Direction? InviteVector;
    }

    public IEnumerable<GenerationStep> Generate(global::Maze.Model.Maze maze, Random random)
    {
        int width = maze.Width;
        int height = maze.Height;
        CellInfo[,] current = new CellInfo[width, height];
        CellInfo[,] next = new CellInfo[width, height];

        int startX = random.Next(width);
        int startY = random.Next(height);
        current[startX, startY].State = CaState.Seed;

        Cell startCell = maze.GetCell(startX, startY);
        startCell.State = CellState.Carving;
        yield return new GenerationStep(startCell, null, null, CellState.Carving, "Initial seed");

        for (int tick = 0; tick < SafetyMaxTicks; tick++)
        {
            Array.Copy(current, next, current.Length);
            bool anyActive = AnyActive(current, width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    CellInfo me = current[x, y];
                    switch (me.State)
                    {
                        case CaState.Disconnected:
                            foreach (GenerationStep step in TryAcceptInvitation(maze, current, next, x, y))
                            {
                                yield return step;
                            }
                            break;

                        case CaState.Seed:
                            foreach (GenerationStep step in RunSeed(maze, current, next, x, y, me, random))
                            {
                                yield return step;
                            }
                            break;

                        case CaState.Invite:
                            foreach (GenerationStep step in RunInvite(maze, next, x, y, random))
                            {
                                yield return step;
                            }
                            break;

                        case CaState.Connected:
                            if (!anyActive)
                            {
                                foreach (GenerationStep step in TryRevive(maze, current, next, x, y, random))
                                {
                                    yield return step;
                                }
                            }
                            break;
                    }
                }
            }

            Array.Copy(next, current, next.Length);

            if (!AnyDisconnected(current, width, height) && !AnyActive(current, width, height))
            {
                yield break;
            }
        }
    }

    private static IEnumerable<GenerationStep> TryAcceptInvitation(
        global::Maze.Model.Maze maze,
        CellInfo[,] current,
        CellInfo[,] next,
        int x,
        int y)
    {
        foreach (Direction direction in DirectionHelper.All)
        {
            (int dx, int dy) = DirectionHelper.Offset(direction);
            int nx = x + dx;
            int ny = y + dy;
            if (!maze.IsInside(nx, ny))
            {
                continue;
            }

            CellInfo neighbor = current[nx, ny];
            if (neighbor.State != CaState.Invite)
            {
                continue;
            }

            if (neighbor.InviteVector != DirectionHelper.Opposite(direction))
            {
                continue;
            }

            next[x, y].State = CaState.Seed;
            next[x, y].ConnectVector = direction;
            next[x, y].InviteVector = null;

            Cell cell = maze.GetCell(x, y);
            Cell parent = maze.GetCell(nx, ny);
            maze.RemoveWallBetween(cell, direction);
            cell.State = CellState.Carving;

            yield return new GenerationStep(cell, parent, direction, CellState.Carving, "Accept invite");
            yield break;
        }
    }

    private static IEnumerable<GenerationStep> RunSeed(
        global::Maze.Model.Maze maze,
        CellInfo[,] current,
        CellInfo[,] next,
        int x,
        int y,
        CellInfo me,
        Random random)
    {
        int neighborMask = BuildDisconnectedMask(maze, current, x, y);
        if (neighborMask == 0)
        {
            next[x, y].State = CaState.Connected;
            next[x, y].InviteVector = null;
            Cell cell = maze.GetCell(x, y);
            cell.State = CellState.Open;
            yield return new GenerationStep(cell, null, null, CellState.Open, "Seed dies");
            yield break;
        }

        Direction picked = ChooseDirection(neighborMask, me.ConnectVector, random);
        next[x, y].State = CaState.Invite;
        next[x, y].InviteVector = picked;
    }

    private static IEnumerable<GenerationStep> RunInvite(
        global::Maze.Model.Maze maze,
        CellInfo[,] next,
        int x,
        int y,
        Random random)
    {
        if (random.Next(100) < BranchProbabilityPercent)
        {
            next[x, y].State = CaState.Seed;
            next[x, y].InviteVector = null;
            yield break;
        }

        next[x, y].State = CaState.Connected;
        next[x, y].InviteVector = null;
        Cell cell = maze.GetCell(x, y);
        cell.State = CellState.Open;
        yield return new GenerationStep(cell, null, null, CellState.Open, "Connected");
    }

    private static IEnumerable<GenerationStep> TryRevive(
        global::Maze.Model.Maze maze,
        CellInfo[,] current,
        CellInfo[,] next,
        int x,
        int y,
        Random random)
    {
        bool bordersDisconnected = false;
        foreach (Direction direction in DirectionHelper.All)
        {
            (int dx, int dy) = DirectionHelper.Offset(direction);
            int nx = x + dx;
            int ny = y + dy;
            if (!maze.IsInside(nx, ny))
            {
                continue;
            }

            if (current[nx, ny].State == CaState.Disconnected)
            {
                bordersDisconnected = true;
                break;
            }
        }

        if (!bordersDisconnected)
        {
            yield break;
        }

        if (random.Next(100) >= BranchProbabilityPercent)
        {
            yield break;
        }

        next[x, y].State = CaState.Seed;
        next[x, y].InviteVector = null;
        Cell cell = maze.GetCell(x, y);
        cell.State = CellState.Carving;
        yield return new GenerationStep(cell, null, null, CellState.Carving, "Revive");
    }

    private static int BuildDisconnectedMask(global::Maze.Model.Maze maze, CellInfo[,] grid, int x, int y)
    {
        int mask = 0;
        foreach (Direction direction in DirectionHelper.All)
        {
            (int dx, int dy) = DirectionHelper.Offset(direction);
            int nx = x + dx;
            int ny = y + dy;
            if (!maze.IsInside(nx, ny))
            {
                continue;
            }

            if (grid[nx, ny].State != CaState.Disconnected)
            {
                continue;
            }

            mask |= 1 << (int)direction;
        }

        return mask;
    }

    private static Direction ChooseDirection(int neighborMask, Direction? connectVector, Random random)
    {
        bool turnNow = random.Next(100) < TurnProbabilityPercent;
        if (!turnNow && connectVector.HasValue)
        {
            Direction straight = DirectionHelper.Opposite(connectVector.Value);
            if ((neighborMask & (1 << (int)straight)) != 0)
            {
                return straight;
            }
        }

        Direction[] available = new Direction[4];
        int count = 0;
        foreach (Direction direction in DirectionHelper.All)
        {
            if ((neighborMask & (1 << (int)direction)) == 0)
            {
                continue;
            }

            available[count] = direction;
            count++;
        }

        return available[random.Next(count)];
    }

    private static bool AnyActive(CellInfo[,] grid, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                CaState state = grid[x, y].State;
                if (state == CaState.Seed || state == CaState.Invite)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool AnyDisconnected(CellInfo[,] grid, int width, int height)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y].State == CaState.Disconnected)
                {
                    return true;
                }
            }
        }

        return false;
    }
}