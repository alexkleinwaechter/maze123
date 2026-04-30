namespace Maze.Model;

/// <summary>
/// Visueller / logischer Zustand einer Zelle. Wird vom Renderer auf eine Farbe gemappt.
/// </summary>
public enum CellState
{
    Untouched = 0,
    Carving = 1,
    Open = 2,
    Frontier = 3,
    Visited = 4,
    Path = 5,
    Start = 6,
    Goal = 7,
    Filled = 8
}