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
		if (isPlayer)
		{
			float dir = Input.GetActionStrength(downAction) - Input.GetActionStrength(upAction);
			Velocity = new Vector2(0, dir * speed);
		}

		MoveAndSlide();

		// Clamp position inside playfield bounds
		var viewport = GetViewportRect();
		float halfHeight = size.Y * 0.5f;

		// Limit top and bottom movement
		float clampedY = Mathf.Clamp(Position.Y, halfHeight, viewport.Size.Y - halfHeight);
		Position = new Vector2(Position.X, clampedY);
	}

}
