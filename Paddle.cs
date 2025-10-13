using Godot;
using System;

public partial class Paddle : CharacterBody2D
{
    [Export] public float speed = 400f;
    [Export] public Vector2 size = new Vector2(16, 100);
    [Export] public string upAction = "p1_up";
    [Export] public string downAction = "p1_down";
    [Export] public bool isPlayer = true;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Vector2.Zero;
        if (isPlayer)
        {
            float dir = Input.GetActionStrength(downAction) - Input.GetActionStrength(upAction);
            velocity = new Vector2(0, dir * speed);
        }
        // For AI, velocity is set externally in Pong

        Velocity = velocity;
        MoveAndSlide();

        // Clamp after move
        var viewport = GetViewportRect();
        if (viewport.Size.Y > 0)  // Safety: Skip if viewport invalid
        {
            float halfHeight = size.Y * 0.5f;
            float clampedY = Mathf.Clamp(Position.Y, halfHeight, viewport.Size.Y - halfHeight);
            Position = new Vector2(Position.X, clampedY);
        }
    }
}