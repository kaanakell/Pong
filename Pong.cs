using Godot;
using System;

public partial class Pong : Node2D
{
	[Export] public float paddleOffset = 40f;
	[Export] public float aiSpeed = 220f;
	[Export] public float aiReactionChance = 85f;

	private Paddle leftPaddle;
	private Paddle rightPaddle;
	private Ball ball;
	private Label scoreLeftLabel;
	private Label scoreRightLabel;
	private Timer serveTimer;

	private Rect2 playfield;
	private int scoreLeft = 0;
	private int scoreRight = 0;

	public override void _Ready()
	{
		var camera = GetNode<Camera2D>("MainCamera");
		camera.MakeCurrent(); // activates this camera

		// Optional: center the camera on the playfield
		camera.Position = GetViewportRect().Size / 2;

		leftPaddle = GetNode<Paddle>("PaddleLeft");
		rightPaddle = GetNode<Paddle>("PaddleRight");
		ball = GetNode<Ball>("Ball");

		var ui = GetNode<CanvasLayer>("UI");
		scoreLeftLabel = ui.GetNode<Label>("ScoreLeft");
		scoreRightLabel = ui.GetNode<Label>("ScoreRight");
		serveTimer = GetNode<Timer>("ServeTimer");

		playfield = new Rect2(Vector2.Zero, GetViewportRect().Size);

		leftPaddle.Position = new Vector2(paddleOffset, playfield.Size.Y * 0.5f);
		rightPaddle.Position = new Vector2(playfield.Size.X - paddleOffset, playfield.Size.Y * 0.5f);

		ball.playfieldRect = playfield;
		ball.leftPaddle = leftPaddle;
		ball.rightPaddle = rightPaddle;

		ball.Connect(Ball.SignalName.Hit, new Callable(this, nameof(OnBallHit)));
		ball.Connect(Ball.SignalName.Scored, new Callable(this, nameof(OnBallScored)));
		serveTimer.Timeout += OnServeTimerTimeout;

		ball.Reset(true);
		UpdateScoreUI();
	}

	public override void _PhysicsProcess(double delta)
	{
		float targetY = ball.Position.Y;
		float diff = targetY - rightPaddle.Position.Y;
		float deadzone = 8f;

		if (Mathf.Abs(diff) > deadzone)
		{
			if (GD.Randi() % 100 < aiReactionChance)
			{
				float jitter = (float)GD.RandRange(-0.12f, 0.12f);
				rightPaddle.Velocity = new Vector2(0, Mathf.Sign(diff) * aiSpeed * (1f + jitter));
			}
			else
			{
				rightPaddle.Velocity = Vector2.Zero;
			}
		}
		else
		{
			rightPaddle.Velocity = Vector2.Zero;
		}

		float half = rightPaddle.size.Y * 0.5f;
		rightPaddle.Position = new Vector2(
			rightPaddle.Position.X,
			Mathf.Clamp(rightPaddle.Position.Y, playfield.Position.Y + half, playfield.Position.Y + playfield.Size.Y - half)
		);

		rightPaddle.MoveAndSlide();

		if (Input.IsActionJustPressed("restart"))
			ResetGame();
	}

	private void OnBallHit()
	{
		// You can play sound or particle here
	}

	private void OnBallScored(string side)
	{
		if (side == "left") scoreLeft++;
		else scoreRight++;

		UpdateScoreUI();

		leftPaddle.Position = new Vector2(paddleOffset, playfield.Size.Y * 0.5f);
		rightPaddle.Position = new Vector2(playfield.Size.X - paddleOffset, playfield.Size.Y * 0.5f);

		serveTimer.Start();
	}

	private void OnServeTimerTimeout()
	{
		bool kickRight = GD.Randi() % 2 == 0;
		ball.Reset(kickRight);
	}

	private void UpdateScoreUI()
	{
		scoreLeftLabel.Text = scoreLeft.ToString();
		scoreRightLabel.Text = scoreRight.ToString();
	}

	private void ResetGame()
	{
		scoreLeft = 0;
		scoreRight = 0;
		UpdateScoreUI();
		leftPaddle.Position = new Vector2(paddleOffset, playfield.Size.Y * 0.5f);
		rightPaddle.Position = new Vector2(playfield.Size.X - paddleOffset, playfield.Size.Y * 0.5f);
		ball.Reset(true);
	}
}
