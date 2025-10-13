using Godot;
using System;

public partial class SoundManager : Node
{
    private AudioStreamPlayer audioWall;
    private AudioStreamPlayer audioPaddle;
    private AudioStreamPlayer audioScore;

    public override void _Ready()
    {
        audioWall = GetNode<AudioStreamPlayer>("AudioWall");
        audioPaddle = GetNode<AudioStreamPlayer>("AudioPaddle");
        audioScore = GetNode<AudioStreamPlayer>("AudioScore");
    }

    public void PlayWall()
    {
        if (audioWall?.Stream != null)
            audioWall.Play();
    }

    public void PlayPaddle()
    {
        if (audioPaddle?.Stream != null)
            audioPaddle.Play();
    }

    public void PlayScore()
    {
        if (audioScore?.Stream != null)
            audioScore.Play();
    }
}


