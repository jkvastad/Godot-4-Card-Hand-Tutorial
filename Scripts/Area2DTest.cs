using Godot;
using System;

public partial class Area2DTest : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        GD.Print("Node " + GetPath() + " received event: " + @event.AsText());
    }
}
