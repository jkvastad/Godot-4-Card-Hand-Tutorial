using Godot;

public partial class Card : Area2D
{
	[Export]
	public Label textLabel;
	public Tween tween;
    [Export]
    public Sprite2D cardArt;
	
	public override void _Ready()
	{
		textLabel.Text = GetPath();
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
