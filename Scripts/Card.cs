using Godot;
using System;

public partial class Card : Area2D
{
	[Export]
	public RichTextLabel textLabel;
	public Tween tween;
    [Export]
    public Sprite2D cardArt;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		textLabel.Text = GetPath();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
    }

    public override void _MouseEnter()
    {        
        cardArt.Modulate = Colors.LightPink;
    }

    public override void _MouseExit()
    {        
        cardArt.Modulate = Colors.White;
    }
}
