#nullable enable

using Godot;
using Maze.Model;

namespace Maze.Views;

/// <summary>
/// 3D-Visualisierung des Labyrinths. Nutzt MultiMeshes fuer grosse Wandmengen,
/// damit auch sehr grosse Labyrinthe ohne Node-Flut aufgebaut werden koennen.
/// </summary>
public partial class MazeView3D : Node3D
{
    [Export] public float CellSize = 1.0f;
    [Export] public float WallHeight = 1.4f;
    [Export] public float WallThickness = 0.1f;

    private CameraController3D _camera = null!;
    private Node3D _wallContainer = null!;
    private MeshInstance3D _floor = null!;
    private MultiMeshInstance3D _wallsHorizontal = null!;
    private MultiMeshInstance3D _wallsVertical = null!;

    private static readonly StandardMaterial3D WallMaterial = new()
    {
        AlbedoColor = new Color("#dcdcdc")
    };

    private static readonly StandardMaterial3D FloorMaterial = new()
    {
        AlbedoColor = new Color("#2c2c2c")
    };

    private global::Maze.Model.Maze? _maze;

    public override void _Ready()
    {
        _camera = GetNode<CameraController3D>("Camera3D");
        _wallContainer = GetNode<Node3D>("WallContainer");
        _floor = GetNode<MeshInstance3D>("Floor");
        _wallsHorizontal = GetNode<MultiMeshInstance3D>("WallContainer/WallsHorizontal");
        _wallsVertical = GetNode<MultiMeshInstance3D>("WallContainer/WallsVertical");

        _wallsHorizontal.MaterialOverride = WallMaterial;
        _wallsVertical.MaterialOverride = WallMaterial;
    }

    public void SetMaze(global::Maze.Model.Maze maze)
    {
        _maze = maze;
        Rebuild();
        _camera.FitToMaze(maze);
    }

    public void ClearMaze()
    {
        _maze = null;
        _floor.Mesh = null;
        ResetMultiMeshes();
    }

    public void Refresh()
    {
        // In dieser einfachen Variante reicht ein vollstaendiger Neubau.
        if (_maze is not null)
        {
            Rebuild();
        }
    }

    private void Rebuild()
    {
        if (_maze is null)
        {
            _floor.Mesh = null;
            ResetMultiMeshes();
            return;
        }

        BuildFloor(_maze);
        BuildWalls(_maze);
    }

    private void ResetMultiMeshes()
    {
        _wallsHorizontal.Multimesh.InstanceCount = 0;
        _wallsHorizontal.Multimesh.VisibleInstanceCount = 0;
        _wallsVertical.Multimesh.InstanceCount = 0;
        _wallsVertical.Multimesh.VisibleInstanceCount = 0;
    }

    private void BuildFloor(global::Maze.Model.Maze maze)
    {
        Vector3 size = new(maze.Width * CellSize, 0.05f, maze.Height * CellSize);
        _floor.Mesh = new BoxMesh { Size = size };
        _floor.MaterialOverride = FloorMaterial;
        _floor.Position = new Vector3(maze.Width * CellSize / 2f, -0.025f, maze.Height * CellSize / 2f);
    }

    private void BuildWalls(global::Maze.Model.Maze maze)
    {
        ConfigureWallMeshes();

        int maxHorizontal = maze.Width * maze.Height + maze.Width;
        int maxVertical = maze.Width * maze.Height + maze.Height;

        MultiMesh horizontal = _wallsHorizontal.Multimesh;
        MultiMesh vertical = _wallsVertical.Multimesh;

        horizontal.InstanceCount = maxHorizontal;
        vertical.InstanceCount = maxVertical;

        int horizontalIndex = 0;
        int verticalIndex = 0;

        for (int y = 0; y < maze.Height; y++)
        {
            for (int x = 0; x < maze.Width; x++)
            {
                Cell cell = maze.GetCell(x, y);

                if (cell.HasWall(Direction.North))
                {
                    horizontal.SetInstanceTransform(horizontalIndex++, HorizontalWallTransform(x * CellSize + CellSize / 2f, y * CellSize));
                }

                if (cell.HasWall(Direction.West))
                {
                    vertical.SetInstanceTransform(verticalIndex++, VerticalWallTransform(x * CellSize, y * CellSize + CellSize / 2f));
                }

                if (y == maze.Height - 1 && cell.HasWall(Direction.South))
                {
                    horizontal.SetInstanceTransform(horizontalIndex++, HorizontalWallTransform(x * CellSize + CellSize / 2f, (y + 1) * CellSize));
                }

                if (x == maze.Width - 1 && cell.HasWall(Direction.East))
                {
                    vertical.SetInstanceTransform(verticalIndex++, VerticalWallTransform((x + 1) * CellSize, y * CellSize + CellSize / 2f));
                }
            }
        }

        horizontal.VisibleInstanceCount = horizontalIndex;
        vertical.VisibleInstanceCount = verticalIndex;
    }

    private void ConfigureWallMeshes()
    {
        _wallsHorizontal.Multimesh.Mesh = new BoxMesh { Size = new Vector3(CellSize, WallHeight, WallThickness) };
        _wallsVertical.Multimesh.Mesh = new BoxMesh { Size = new Vector3(WallThickness, WallHeight, CellSize) };
    }

    private Transform3D HorizontalWallTransform(float centerX, float centerZ) =>
        new(Basis.Identity, new Vector3(centerX, WallHeight / 2f, centerZ));

    private Transform3D VerticalWallTransform(float centerX, float centerZ) =>
        new(Basis.Identity, new Vector3(centerX, WallHeight / 2f, centerZ));
}