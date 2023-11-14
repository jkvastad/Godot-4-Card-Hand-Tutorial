using Godot;
using System.Linq;

public partial class CardHand : Container
{
    [Export]
    public float sortSpeed = 1.0f;
    Card selectedCard;
    Vector2 selectionOffset;
    
    public override void _Ready()
    {
        SortChildren += SortCards;
        Godot.Collections.Array<Node> cards = GetChildren();
        foreach (Card card in cards.Cast<Card>())
        {
            card.GuiInput += (InputEvent theEvent) => CardIsClicked(theEvent, card);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (selectedCard != null)
        {
            selectedCard.GlobalPosition = GetGlobalMousePosition() - selectionOffset;            
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
            if (card.tween == null || !card.tween.IsValid())
            {
                card.tween = CreateTween();
                card.tween.TweenProperty(card, "position", position, sortSpeed);
            }
        }
    }

    public void CardIsClicked(InputEvent @event, Card card)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                selectedCard = card;                                
                selectionOffset = mouseEvent.Position;                
                GD.Print("Card " + card.text.Text + " clicked with " + @event.AsText()); //TODO remove
                card.tween?.Kill();

            }
            if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                selectedCard = null;
                GD.Print("Card " + card.text.Text + " released with " + @event.AsText()); //TODO remove
                EmitSignal(SignalName.SortChildren);
            }
        }
    }
}