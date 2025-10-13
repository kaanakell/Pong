using Godot;
using System;

public partial class Pong : Node2D
{
	[Export] public float paddleOffset = 40f;
	[Export] public float aiSpeed = 220f;
	[Export] public float aiReactionChance = 85f;
	[Export] public Vector2 fallbackViewportSize = new(1280, 720);  // Adjust to match your project window size

	private const float AiDeadzone = 8f;
	
	private Paddle leftPaddle;
	private Paddle rightPaddle;
	private Ball ball;
	private Label scoreLeftLabel;
	private Label scoreRightLabel;
	private Timer serveTimer;
	private SoundManager soundManager;

	private Rect2 playfield;
	private int scoreLeft = 0;
	private int scoreRight = 0;
	private bool initialized = false;

	public override void _Ready()
	{
		CacheNodes();
		SetupEarlyNonViewportDependencies();
		DeferViewportInitialization();
	}

	private void CacheNodes()
	{
		// Cache all required nodes with null-safety
		var camera = GetNodeOrNull<Camera2D>("MainCamera");
		camera?.MakeCurrent();

		leftPaddle = GetNodeOrNull<Paddle>("PaddleLeft");
		rightPaddle = GetNodeOrNull<Paddle>("PaddleRight");
		ball = GetNodeOrNull<Ball>("Ball");
		soundManager = GetNodeOrNull<SoundManager>("SoundManager");

		var ui = GetNodeOrNull<CanvasLayer>("UI");
		if (ui != null)
		{
			scoreLeftLabel = ui.GetNodeOrNull<Label>("ScoreLeft");
			scoreRightLabel = ui.GetNodeOrNull<Label>("ScoreRight");
		}
		serveTimer = GetNodeOrNull<Timer>("ServeTimer");

		if (soundManager == null)
		{
			GD.PrintErr("SoundManager not found. Check scene hierarchy.");
		}
	}

	private void SetupEarlyNonViewportDependencies()
	{
		// Set up ball references and temp positions using fallback size
		if (ball != null)
		{
			ball.leftPaddle = leftPaddle;
			ball.rightPaddle = rightPaddle;
			ball.Reset(true);  // Temp reset; will refine later
		}

		if (leftPaddle != null && rightPaddle != null)
		{
			var tempPlayfieldSize = fallbackViewportSize;
			leftPaddle.Position = new Vector2(paddleOffset, tempPlayfieldSize.Y * 0.5f);
			rightPaddle.Position = new Vector2(tempPlayfieldSize.X - paddleOffset, tempPlayfieldSize.Y * 0.5f);
		}

		// Connect signals
		if (ball != null)
		{
			ball.Connect(Ball.SignalName.Hit, new Callable(this, nameof(OnBallHit)));
			ball.Connect(Ball.SignalName.Scored, new Callable(this, nameof(OnBallScored)));
		}
		if (serveTimer != null)
		{
			serveTimer.Timeout += OnServeTimerTimeout;
		}
	}

	private void DeferViewportInitialization()
	{
		// Defer full init to ensure viewport size is available
		CallDeferred(nameof(InitializeViewportAndPositions));
	}

	private void InitializeViewportAndPositions()
	{
		if (initialized) return;
		initialized = true;

		// Get viewport size (should be valid now)
		var viewportSize = GetViewportRect().Size;
		if (viewportSize == Vector2.Zero)
		{
			// Rare fallback: Retry next frame
			CallDeferred(nameof(InitializeViewportAndPositions));
			return;
		}

		playfield = new Rect2(Vector2.Zero, viewportSize);

		// Center camera
		var camera = GetNode<Camera2D>("MainCamera");
		camera.Position = playfield.Size / 2f;

		// Set final positions and reset ball with real bounds
		ResetPaddlePositions();
		if (ball != null)
		{
			ball.playfieldRect = playfield;
			ball.Reset(true);
		}

		UpdateScoreUI();
	}

	public override void _PhysicsProcess(double delta)
	{
		// Ensure init before processing
		if (!initialized)
		{
			InitializeViewportAndPositions();
			return;
		}

		if (ball == null || rightPaddle == null) return;

		UpdateAiPaddle();
		HandleRestartInput();
	}

	private void UpdateAiPaddle()
	{
		var targetY = ball.Position.Y;
		var diff = targetY - rightPaddle.Position.Y;

		if (Mathf.Abs(diff) <= AiDeadzone)
		{
			rightPaddle.Velocity = Vector2.Zero;
			return;
		}

		// AI reaction with chance to miss/jitter
		if (GD.Randi() % 100 >= aiReactionChance)
		{
			rightPaddle.Velocity = Vector2.Zero;
			return;
		}

		var jitter = (float)GD.RandRange(-0.12f, 0.12f);
		rightPaddle.Velocity = new Vector2(
			0,
			Mathf.Sign(diff) * aiSpeed * (1f + jitter)
		);

		// Clamp and move
		var halfHeight = rightPaddle.size.Y * 0.5f;
		rightPaddle.Position = new Vector2(
			rightPaddle.Position.X,
			Mathf.Clamp(
				rightPaddle.Position.Y,
				playfield.Position.Y + halfHeight,
				playfield.Position.Y + playfield.Size.Y - halfHeight
			)
		);
		rightPaddle.MoveAndSlide();
	}

	private void HandleRestartInput()
	{
		if (Input.IsActionJustPressed("restart"))
		{
			ResetGame();
		}
	}

	private void OnBallHit()
	{
		soundManager?.PlayPaddle();
	}

	private void OnBallScored(string side)
	{
		soundManager?.PlayScore();

		if (side == "left")
		{
			scoreLeft++;
		}
		else
		{
			scoreRight++;
		}

		UpdateScoreUI();
		ResetPaddlePositions();
		serveTimer?.Start();
	}

	private void OnServeTimerTimeout()
	{
		if (ball != null)
		{
			var kickRight = GD.Randi() % 2 == 0;
			ball.Reset(kickRight);
		}
	}

	private void ResetPaddlePositions()
	{
		if (leftPaddle != null && rightPaddle != null)
		{
			leftPaddle.Position = new Vector2(paddleOffset, playfield.Size.Y * 0.5f);
			rightPaddle.Position = new Vector2(playfield.Size.X - paddleOffset, playfield.Size.Y * 0.5f);
		}
	}

	private void UpdateScoreUI()
	{
		if (scoreLeftLabel != null)
			scoreLeftLabel.Text = scoreLeft.ToString();
		if (scoreRightLabel != null)
			scoreRightLabel.Text = scoreRight.ToString();
	}

	private void ResetGame()
	{
		scoreLeft = 0;
		scoreRight = 0;
		UpdateScoreUI();
		ResetPaddlePositions();
		if (ball != null)
		{
			ball.Reset(true);
		}
	}
}