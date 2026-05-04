using Godot;

namespace Maze.Views;

/// <summary>
/// Frei steuerbare 2D-Kamera fuer die Maze-Ansicht.
/// Bewegung: WASD/Pfeiltasten pannen die Kamera, Shift verdoppelt das Tempo.
/// Drag: rechte Maustaste gedrueckt halten und Maus bewegen.
/// Zoom: Mausrad mit Mausposition als Pivot - der Punkt unter dem Cursor bleibt stehen.
/// </summary>
public partial class CameraController2D : Camera2D
{
    [Export] public float PanSpeed = 800f;
    [Export] public float SprintMultiplier = 2f;
    [Export] public float ZoomStep = 1.1f;
    [Export] public float ZoomSprintMultiplier = 1.3f;
    [Export] public float MinZoom = 0.01f;
    [Export] public float MaxZoom = 5.0f;

    private bool _isPanning;

    public override void _Ready()
    {
        MakeCurrent();
    }

    public override void _Process(double delta)
    {
        HandlePan(delta);
    }

    private void HandlePan(double delta)
    {
        Vector2 input = Vector2.Zero;
        if (Input.IsPhysicalKeyPressed(Key.W) || Input.IsPhysicalKeyPressed(Key.Up)) input += Vector2.Up;
        if (Input.IsPhysicalKeyPressed(Key.S) || Input.IsPhysicalKeyPressed(Key.Down)) input += Vector2.Down;
        if (Input.IsPhysicalKeyPressed(Key.A) || Input.IsPhysicalKeyPressed(Key.Left)) input += Vector2.Left;
        if (Input.IsPhysicalKeyPressed(Key.D) || Input.IsPhysicalKeyPressed(Key.Right)) input += Vector2.Right;

        if (input == Vector2.Zero)
        {
            return;
        }

        float speed = PanSpeed;
        if (Input.IsPhysicalKeyPressed(Key.Shift))
        {
            speed *= SprintMultiplier;
        }

        Position += input.Normalized() * speed * (float)delta / Zoom.X;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                _isPanning = mouseButton.Pressed;
                return;
            }

            if (mouseButton.Pressed && (mouseButton.ButtonIndex == MouseButton.WheelUp || mouseButton.ButtonIndex == MouseButton.WheelDown))
            {
                float step = ZoomStep;
                if (Input.IsPhysicalKeyPressed(Key.Shift))
                {
                    step *= ZoomSprintMultiplier;
                }

                float factor = mouseButton.ButtonIndex == MouseButton.WheelUp ? step : 1f / step;
                Vector2 mouseWorldBefore = GetGlobalMousePosition();
                Vector2 newZoom = Zoom * factor;
                newZoom.X = Mathf.Clamp(newZoom.X, MinZoom, MaxZoom);
                newZoom.Y = Mathf.Clamp(newZoom.Y, MinZoom, MaxZoom);
                Zoom = newZoom;

                Vector2 mouseWorldAfter = GetGlobalMousePosition();
                Position += mouseWorldBefore - mouseWorldAfter;
                return;
            }
        }

        if (@event is InputEventMouseMotion motion && _isPanning)
        {
            Position -= new Vector2(motion.Relative.X / Zoom.X, motion.Relative.Y / Zoom.Y);
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut)
        {
            _isPanning = false;
        }
    }

    public void FitToMaze(global::Maze.Model.Maze maze)
    {
        MazeView2D view = GetParent<MazeView2D>();
        float worldWidth = maze.Width * view.CellSizePx;
        float worldHeight = maze.Height * view.CellSizePx;

        Vector2 viewport = GetViewportRect().Size;
        float zoomX = viewport.X / worldWidth;
        float zoomY = viewport.Y / worldHeight;
        float zoomFit = Mathf.Min(zoomX, zoomY) * 0.9f;
        zoomFit = Mathf.Clamp(zoomFit, MinZoom, MaxZoom);

        Zoom = new Vector2(zoomFit, zoomFit);
        Position = new Vector2(worldWidth / 2f, worldHeight / 2f);
    }
}