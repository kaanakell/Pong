using Godot;
using System;

public partial class Ball : RigidBody2D
{
	[Export] public float Speed = 400f;

	public override void _Ready()
	{
		// Random initial direction
		var direction = new Vector2((float)GD.RandRange(-1f, 1f), (float)GD.RandRange(-0.5f, 0.5f)).Normalized();
		LinearVelocity = direction * Speed;
	}

	private void _on_body_entered(Node body)
	{
		if (body is Paddle paddle)
		{
			// Add a bit of paddle influence to bounce angle
			var offset = (GlobalPosition.Y - paddle.GlobalPosition.Y) / (paddle.size.Y / 2f);
			var bounceAngle = new Vector2(Mathf.Sign(LinearVelocity.X), offset).Normalized();
			LinearVelocity = bounceAngle * Speed;
		}
		else if (body.Name == "TopWall" || body.Name == "BottomWall")
		{
			// Reflect vertically
			LinearVelocity = new Vector2(LinearVelocity.X, -LinearVelocity.Y);
		}
	}
}
