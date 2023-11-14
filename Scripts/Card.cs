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
		text.Text = GetPath();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
            GD.Print("Card " + text.Text + " received gui input " + mouseEvent.AsText());
        base._GuiInput(@event);
    }
}
