using Godot;

namespace Maze.Views;

/// <summary>
/// Frei steuerbare 3D-Kamera fuer die Maze-Ansicht.
/// Bewegung: WASD horizontal in Blickrichtung, QE vertikal in Welt-Y,
/// Shift verdoppelt die Geschwindigkeit. Drehung: RMB + Maus oder Pfeiltasten.
/// Zoom: Mausrad als Dolly entlang der Blickrichtung.
/// </summary>
public partial class CameraController3D : Camera3D
{
    [Export] public float MoveSpeed = 8f;
    [Export] public float SprintMultiplier = 2f;
    [Export] public float MouseSensitivity = 0.003f;
    [Export] public float KeyTurnSpeed = 1.5f;
    [Export] public float ZoomStep = 1.5f;
    [Export] public float ZoomSprintMultiplier = 3f;

    private float _yaw;
    private float _pitch;
    private bool _mouseLook;

    public override void _Ready()
    {
        Vector3 euler = Basis.GetEuler();
        _pitch = euler.X;
        _yaw = euler.Y;
    }

    public override void _Process(double delta)
    {
        HandleMovement(delta);
        HandleKeyboardLook(delta);
        ApplyRotation();
    }

    private void HandleMovement(double delta)
    {
        Vector3 input = Vector3.Zero;
        if (Input.IsPhysicalKeyPressed(Key.W)) input += Vector3.Forward;
        if (Input.IsPhysicalKeyPressed(Key.S)) input += Vector3.Back;
        if (Input.IsPhysicalKeyPressed(Key.A)) input += Vector3.Left;
        if (Input.IsPhysicalKeyPressed(Key.D)) input += Vector3.Right;

        Vector3 worldVertical = Vector3.Zero;
        if (Input.IsPhysicalKeyPressed(Key.E)) worldVertical += Vector3.Up;
        if (Input.IsPhysicalKeyPressed(Key.Q)) worldVertical += Vector3.Down;

        if (input == Vector3.Zero && worldVertical == Vector3.Zero)
        {
            return;
        }

        float speed = MoveSpeed;
        if (Input.IsPhysicalKeyPressed(Key.Shift))
        {
            speed *= SprintMultiplier;
        }

        if (input != Vector3.Zero)
        {
            Translate(input.Normalized() * speed * (float)delta);
        }

        if (worldVertical != Vector3.Zero)
        {
            Position += worldVertical.Normalized() * speed * (float)delta;
        }
    }

    private void HandleKeyboardLook(double delta)
    {
        float yawDelta = 0f;
        float pitchDelta = 0f;
        if (Input.IsPhysicalKeyPressed(Key.Left)) yawDelta += KeyTurnSpeed * (float)delta;
        if (Input.IsPhysicalKeyPressed(Key.Right)) yawDelta -= KeyTurnSpeed * (float)delta;
        if (Input.IsPhysicalKeyPressed(Key.Up)) pitchDelta += KeyTurnSpeed * (float)delta;
        if (Input.IsPhysicalKeyPressed(Key.Down)) pitchDelta -= KeyTurnSpeed * (float)delta;

        _yaw += yawDelta;
        _pitch = Mathf.Clamp(_pitch + pitchDelta, -1.4f, 1.4f);
    }

    private void ApplyRotation()
    {
        Basis = Basis.FromEuler(new Vector3(_pitch, _yaw, 0f));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                _mouseLook = mouseButton.Pressed;
                Input.MouseMode = mouseButton.Pressed ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
                return;
            }

            if (mouseButton.Pressed && (mouseButton.ButtonIndex == MouseButton.WheelUp || mouseButton.ButtonIndex == MouseButton.WheelDown))
            {
                float step = ZoomStep;
                if (Input.IsPhysicalKeyPressed(Key.Shift))
                {
                    step *= ZoomSprintMultiplier;
                }

                Vector3 direction = mouseButton.ButtonIndex == MouseButton.WheelUp ? Vector3.Forward : Vector3.Back;
                Translate(direction * step);
                return;
            }
        }

        if (@event is InputEventMouseMotion motion && _mouseLook)
        {
            _yaw -= motion.Relative.X * MouseSensitivity;
            _pitch = Mathf.Clamp(_pitch - motion.Relative.Y * MouseSensitivity, -1.4f, 1.4f);
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut && _mouseLook)
        {
            _mouseLook = false;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    public void FitToMaze(global::Maze.Model.Maze maze)
    {
        float width = maze.Width;
        float height = maze.Height;
        float centerX = width / 2f;
        float centerZ = height / 2f;
        float fitHeight = Mathf.Max(width, height) * 0.8f;

        Position = new Vector3(centerX, fitHeight, centerZ + fitHeight * 0.7f);
        LookAt(new Vector3(centerX, 0f, centerZ), Vector3.Up);

        Vector3 euler = Basis.GetEuler();
        _pitch = euler.X;
        _yaw = euler.Y;
    }
}