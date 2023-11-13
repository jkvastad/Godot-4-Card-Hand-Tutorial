using Godot;
using System;

public partial class CardHand : Container
{
    Card selectedCard;
    Vector2 selectionOffset;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Godot.Collections.Array<Node> cards = GetChildren();
        foreach (Card card in cards)
        {
            card.GuiInput += (InputEvent theEvent) => cardIsClicked(theEvent, card);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void _PhysicsProcess(double delta)
    {
        if (selectedCard != null)
        {
            selectedCard.Position = GetGlobalMousePosition() - selectionOffset;
        }
        base._PhysicsProcess(delta);
    }

    public override void _Notification(int notification)
    {
        if (notification == NotificationSortChildren)
        {
            Godot.Collections.Array<Node> children = GetChildren();
            var i = 0;
            foreach (Card child in children)
            {
                Vector2 position = new Vector2(128f * i, 0f);
                i++;
                child.SetPosition(position);
            }
        }
    }

    public void cardIsClicked(InputEvent @event, Card card)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                selectedCard = card;
                selectionOffset = card.GetLocalMousePosition();
                GD.Print("Card " + card.text.Text + " clicked with " + @event.AsText());
            }
            if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                selectedCard = null;
                GD.Print("Card " + card.text.Text + " released with " + @event.AsText());
            }
        }
    }
}
