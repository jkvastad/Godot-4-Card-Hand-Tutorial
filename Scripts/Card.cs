using Godot;
using System;

public partial class Card : Control
{
	[Export]
	public Label text;
	public Tween tween;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		text.Text = Name;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
