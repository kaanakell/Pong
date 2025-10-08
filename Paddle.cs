using Godot;
using System;

public partial class Paddle : CharacterBody2D
{
	[Export] public float speed = 400f;
	[Export] public Vector2 size = new Vector2(16, 100);
	[Export] public string upAction = "p1_up";
	[Export] public string downAction = "p1_down";
	[Export] public bool isPlayer = true;

	public override void _Ready()
	{
		var shape = GetNode<CollisionShape2D>("CollisionShape2D");
		var rect = shape.Shape as RectangleShape2D;
		if (rect != null)
		{
			size = rect.Size;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isPlayer)
		{
			float dir = Input.GetActionStrength(downAction) - Input.GetActionStrength(upAction);
			Velocity = new Vector2(0, dir * speed);
		}

		MoveAndSlide();

		var viewport = GetViewportRect();
		float halfHeight = size.Y * 0.5f;

		float clampedY = Mathf.Clamp(Position.Y, halfHeight, viewport.Size.Y - halfHeight);
		Position = new Vector2(Position.X, clampedY);
	}
}
