using Godot;
using System.Linq;

public partial class CardHand : Node2D
{
    [Export]
    public float sortSpeed = 1.0f;
    Card selectedCard;
    Vector2 selectionOffset;
    
    public override void _Ready()
    {
        Godot.Collections.Array<Node> cards = GetChildren();
        foreach (Card card in cards.Cast<Card>())
        {
            card.InputEvent += (Node viewport, InputEvent @event, long shapeIdx) => CardIsClicked(viewport, @event, shapeIdx, card);
        }
        SortCards();
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

    public void CardIsClicked(Node viewport, InputEvent @event, long shapeIdx, Card card)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                selectedCard = card;                                
                selectionOffset = mouseEvent.GlobalPosition - card.GlobalPosition;
                GD.Print("Card " + card.textLabel.Text + " clicked with " + @event.AsText()); //TODO remove
                card.tween?.Kill();

            }
            if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                //kolla om det finns en selected card, om ja kolla om ett annat kort ngonstans blivit traffat av left mouse release?
                selectedCard = null;
                GD.Print("Card " + card.textLabel.Text + " released with " + @event.AsText()); //TODO remove                                                                                                              
                SortCards();
            }
        }
    }
}