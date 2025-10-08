using Godot;
using System;

public partial class Ball : Node2D
{
	[Signal] public delegate void HitEventHandler();
	[Signal] public delegate void ScoredEventHandler(string side);

	[Export] public float radius = 8f;
	[Export] public float initialSpeed = 350f;
	[Export] public float speedIncrease = 20f;
	[Export] public float maxBounceAngleDegree = 45f;

	public Vector2 velocity = Vector2.Zero;
	public Rect2 playfieldRect = new Rect2();
	public Paddle leftPaddle;
	public Paddle rightPaddle;

	private bool scored = false;

	public override void _Ready()
	{
		GD.Randomize();
	}

	public void Reset(bool kickToRight = true)
	{
		scored = false;
		Position = playfieldRect.Position + playfieldRect.Size * 0.5f;

		float angle = (float)GD.RandRange(-Math.PI / 8f, Math.PI / 8f);
		int direction = kickToRight ? 1 : -1;
		velocity = new Vector2(Mathf.Cos(angle) * direction, Mathf.Sin(angle)).Normalized() * initialSpeed;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (scored) return;

		Position += velocity * (float)delta;

		// Top/bottom wall collisions
		if (Position.Y - radius <= playfieldRect.Position.Y)
		{
			Position = new Vector2(Position.X, playfieldRect.Position.Y + radius);
			velocity = new Vector2(velocity.X, -velocity.Y);
			EmitSignal(SignalName.Hit);
		}
		else if (Position.Y + radius >= playfieldRect.Position.Y + playfieldRect.Size.Y)
		{
			Position = new Vector2(Position.X, playfieldRect.Position.Y + playfieldRect.Size.Y - radius);
			velocity = new Vector2(velocity.X, -velocity.Y);
			EmitSignal(SignalName.Hit);
		}

		// Left paddle
		if (leftPaddle != null)
		{
			Rect2 leftRect = new Rect2(leftPaddle.Position - leftPaddle.size * 0.5f, leftPaddle.size);
			if (CircleIntersectsRect(Position, radius, leftRect))
			{
				Position = new Vector2(leftRect.Position.X + leftRect.Size.X + radius, Position.Y);
				BounceOffPaddle(leftPaddle, true);
				EmitSignal(SignalName.Hit);
			}
		}

		// Right paddle
		if (rightPaddle != null)
		{
			Rect2 rightRect = new Rect2(rightPaddle.Position - rightPaddle.size * 0.5f, rightPaddle.size);
			if (CircleIntersectsRect(Position, radius, rightRect))
			{
				Position = new Vector2(rightRect.Position.X - radius, Position.Y);
				BounceOffPaddle(rightPaddle, false);
				EmitSignal(SignalName.Hit);
			}
		}

		// Scoring
		if (Position.X < playfieldRect.Position.X - radius)
		{
			scored = true;
			velocity = Vector2.Zero;
			EmitSignal(SignalName.Scored, "right");
		}
		else if (Position.X > playfieldRect.Position.X + playfieldRect.Size.X + radius)
		{
			scored = true;
			velocity = Vector2.Zero;
			EmitSignal(SignalName.Scored, "left");
		}
	}

	private bool CircleIntersectsRect(Vector2 circlePos, float r, Rect2 rect)
	{
		float closestX = Mathf.Clamp(circlePos.X, rect.Position.X, rect.Position.X + rect.Size.X);
		float closestY = Mathf.Clamp(circlePos.Y, rect.Position.Y, rect.Position.Y + rect.Size.Y);

		float dx = circlePos.X - closestX;
		float dy = circlePos.Y - closestY;

		return (dx * dx + dy * dy) <= (r * r);
	}

	private void BounceOffPaddle(Paddle paddle, bool isLeftPaddle)
	{
		float relativeY = (Position.Y - paddle.Position.Y) / (paddle.size.Y * 0.5f);
		relativeY = Mathf.Clamp(relativeY, -1f, 1f);

		float angleRad = relativeY * maxBounceAngleDegree * Mathf.DegToRad(1);

		int dirX = isLeftPaddle ? 1 : -1;

		float newSpeed = velocity.Length() + speedIncrease;
		Vector2 newDir = new Vector2(Mathf.Cos(angleRad) * dirX, Mathf.Sin(angleRad)).Normalized();
		velocity = newDir * newSpeed;
	}
}
