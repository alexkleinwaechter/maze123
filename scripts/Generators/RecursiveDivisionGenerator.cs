#nullable enable

using System;
using System.Collections.Generic;
using Maze.Model;

namespace Maze.Generators;

/// <summary>
/// Recursive Division teilt offene Flaechen schrittweise mit Waenden und je einem Durchgang.
/// </summary>
public sealed class RecursiveDivisionGenerator : IMazeGenerator
{
    public string Id => "recursive-division";
    public string Name => "Recursive Division";

    public IEnumerable<GenerationStep> Generate(global::Maze.Model.Maze maze, Random random)
    {
        for (int y = 0; y < maze.Height; y++)
        {
            for (int x = 0; x < maze.Width; x++)
            {
                Cell cell = maze.GetCell(x, y);
                if (x < maze.Width - 1)
                {
                    maze.RemoveWallBetween(cell, Direction.East);
                }

                if (y < maze.Height - 1)
                {
                    maze.RemoveWallBetween(cell, Direction.South);
                }

                cell.State = CellState.Open;
            }
        }

        yield return new GenerationStep(maze.GetCell(0, 0), null, null, CellState.Open, "Cleared");

        Stack<(int x, int y, int width, int height)> work = new();
        work.Push((0, 0, maze.Width, maze.Height));

        while (work.Count > 0)
        {
            (int x, int y, int width, int height) = work.Pop();
            if (width < 2 || height < 2)
            {
                continue;
            }

            bool horizontal = ChooseOrientation(width, height, random);
            if (horizontal)
            {
                int wallRow = y + 1 + random.Next(height - 1);
                int passage = x + random.Next(width);

                for (int currentX = x; currentX < x + width; currentX++)
                {
                    if (currentX == passage)
                    {
                        continue;
                    }

                    Cell upper = maze.GetCell(currentX, wallRow - 1);
                    upper.SetWall(Direction.South, true);
                    Cell lower = maze.GetCell(currentX, wallRow);
                    lower.SetWall(Direction.North, true);
                    yield return new GenerationStep(upper, lower, Direction.South, CellState.Open, "Wall");
                }

                work.Push((x, y, width, wallRow - y));
                work.Push((x, wallRow, width, height - (wallRow - y)));
                continue;
            }

            int wallColumn = x + 1 + random.Next(width - 1);
            int passageRow = y + random.Next(height);

            for (int currentY = y; currentY < y + height; currentY++)
            {
                if (currentY == passageRow)
                {
                    continue;
                }

                Cell left = maze.GetCell(wallColumn - 1, currentY);
                left.SetWall(Direction.East, true);
                Cell right = maze.GetCell(wallColumn, currentY);
                right.SetWall(Direction.West, true);
                yield return new GenerationStep(left, right, Direction.East, CellState.Open, "Wall");
            }

            work.Push((x, y, wallColumn - x, height));
            work.Push((wallColumn, y, width - (wallColumn - x), height));
        }
    }

    private static bool ChooseOrientation(int width, int height, Random random)
    {
        if (width < height)
        {
            return true;
        }

        if (height < width)
        {
            return false;
        }

        return random.Next(2) == 0;
    }
}