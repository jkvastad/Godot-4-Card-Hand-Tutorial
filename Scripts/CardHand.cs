using Godot;
using System;

public partial class CardHand : Container
{
    Card selectedCard;
    Vector2 selectionOffset;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {        
        SortChildren += SortCards;
        Godot.Collections.Array<Node> cards = GetChildren();
        foreach (Card card in cards)
        {
            card.GuiInput += (InputEvent theEvent) => CardIsClicked(theEvent, card);
        }
    }
    

    public override void _PhysicsProcess(double delta)
    {
        if (selectedCard != null)
        {
            selectedCard.Position = GetGlobalMousePosition() - selectionOffset;
        }        
    }

    public void SortCards()
    {
        Godot.Collections.Array<Node> cards = GetChildren();
        var i = 0;
        foreach (Card card in cards)
        {
            Vector2 position = new Vector2(128f * i, 0f);
            i++;                        
            card.tween = CreateTween();            
            card.tween.TweenProperty(card, "position", position, 0.5f);
        }
    }

    public void CardIsClicked(InputEvent @event, Card card)
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
                EmitSignal(SignalName.SortChildren);
            }
        }
    }
}
