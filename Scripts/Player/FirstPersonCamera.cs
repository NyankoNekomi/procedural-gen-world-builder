using Godot;
using System;

public partial class FirstPersonCamera : Camera3D
{
    [Export] public float MouseSensitivity { get; set; } = 0.003f;

    private Node3D cameraPivot;
    private CharacterBody3D player;

    public override void _Ready()
    {
        cameraPivot = GetParent<Node3D>(); // "Camera Pivot"
        player = cameraPivot.GetParent<CharacterBody3D>(); // "Player"

        // Lock mouse on start
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        // ALT unlocks mouse (hover mode)
        if (Input.IsKeyPressed(Key.Alt))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            return;
        }

        // ESC unlocks mouse
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            return;
        }

        // CLICK locks mouse again
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.Pressed && Input.MouseMode == Input.MouseModeEnum.Visible)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                return;
            }
        }

        // Mouse look only when captured
        if (@event is InputEventMouseMotion motion &&
            Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            // Horizontal rotation (player)
            player.RotateY(-motion.Relative.X * MouseSensitivity);

            // Vertical rotation (camera pivot)
            cameraPivot.RotateX(-motion.Relative.Y * MouseSensitivity);

            // Clamp vertical rotation
            Vector3 rot = cameraPivot.Rotation;
            rot.X = Mathf.Clamp(rot.X, Mathf.DegToRad(-85), Mathf.DegToRad(85));
            cameraPivot.Rotation = rot;
        }
    }
}