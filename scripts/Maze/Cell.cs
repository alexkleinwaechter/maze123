namespace Maze.Model;

/// <summary>
/// Eine einzelne Zelle des Labyrinths.
/// Enthaelt Position, Wandstatus zu allen vier Nachbarn und einen visuellen Zustand.
/// </summary>
public sealed class Cell
{
    public int X { get; }
    public int Y { get; }

    private readonly bool[] _walls = { true, true, true, true };

    public CellState State { get; set; } = CellState.Untouched;

    /// <summary>Distanzwert fuer Heatmap / Solver-Anzeigen. -1 = unbekannt.</summary>
    public int Distance { get; set; } = -1;

    public Cell(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool HasWall(Direction direction) => _walls[(int)direction];

    public void SetWall(Direction direction, bool present) =>
        _walls[(int)direction] = present;

    /// <summary>
    /// Entfernt die Wand zu einem Nachbarn auf dieser Zellseite.
    /// Die andere Seite erledigt Maze.RemoveWallBetween.
    /// </summary>
    public void RemoveWall(Direction direction) => SetWall(direction, false);

    public override string ToString() => $"Cell({X},{Y}) State={State}";
}