using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class CardHand : Node2D
{
    [Export]
    public float sortSpeed = 1.0f;
    Card selectedCard;
    Vector2 selectionOffset = Vector2.Zero;
    List<(TaskCompletionSource<Array<Dictionary>> query, Func<Array<Dictionary>> action)> raycastQueries = new();
    List<(Card card, CollisionObject2D.InputEventEventHandler cardInputHandler)> cardInputHandlers = new();
    public override void _Ready()
    {
        Array<Node> cards = GetChildren();
        foreach (Card card in cards.Cast<Card>())
        {
            SubscribeToCardInputEvent(card);
        }
        SortCards();
    }

    private void SubscribeToCardInputEvent(Card card)
    {
        CollisionObject2D.InputEventEventHandler cardInputHandler = (Node viewport, InputEvent @event, long shapeIdx) => CardIsClicked(viewport, @event, shapeIdx, card);
        cardInputHandlers.Add((card, cardInputHandler));
        card.InputEvent += cardInputHandler;
    }

    private void UnSubscribeToCardInputEvent(Card card)
    {
        (Card card, CollisionObject2D.InputEventEventHandler cardInputHandler) cardAndInputHandler = cardInputHandlers.Single(item => item.card == card);
        cardInputHandlers.Remove(cardAndInputHandler);
        card.InputEvent -= cardAndInputHandler.cardInputHandler;
    }

    public override void _PhysicsProcess(double delta)
    {
        DoRaycastQueries();
        if (selectedCard != null)
        {
            selectedCard.GlobalPosition = GetGlobalMousePosition() - selectionOffset;
        }
    }

    private void DoRaycastQueries()
    {
        for (int i = raycastQueries.Count - 1; i >= 0; i--)
        {
            raycastQueries[i].query.SetResult(raycastQueries[i].action());
            raycastQueries.RemoveAt(i);
        }
    }

    public void AddCard(Card card, int index)
    {
        GD.Print($"Card position before adding {card.Position}, global position {card.GlobalPosition}, offset is {selectionOffset}");
        Vector2 oldGlobal = card.GlobalPosition;        
        AddChild(card);
        Vector2 newGlobal = card.GlobalPosition;
        //card.Position -= newGlobal - oldGlobal;
        GD.Print($"Card position after adding {card.Position}, global position {card.GlobalPosition}, offset is {selectionOffset}");
        //Vector2 oldPosition = card.Position;
        //card.GlobalPosition = card.Position;
        //GD.Print($"Card position is {card.Position} after setting global position {card.GlobalPosition} to {oldPosition}");
        MoveChild(card, index);
        SubscribeToCardInputEvent(card);
    }

    public void RemoveCard(Card card)
    {        
        RemoveChild(card);                
        UnSubscribeToCardInputEvent(card);
    }

    public void SortCards()
    {
        Array<Node> cards = GetChildren();
        var i = 0;
        foreach (Card card in cards)
        {
            Vector2 position = new Vector2(128f * i, 0);
            i++;
            if (card.tween == null || !card.tween.IsValid())
            {
                card.tween = CreateTween();
                //GD.Print($"card {card.textLabel.Text} global position is {card.GlobalPosition}, position is {card.Position}, target relative position is {position}");
                card.tween.TweenProperty(card, "position", position, sortSpeed);
            }
        }
    }

    public async void CardIsClicked(Node viewport, InputEvent @event, long shapeIdx, Card card)
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
            if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && card == selectedCard)
            {
                selectionOffset = Vector2.Zero;
                Task<Array<Dictionary>> raycastQuery = getRaycastQuery(new PhysicsPointQueryParameters2D()
                {
                    Position = GetGlobalMousePosition(),
                    CollideWithAreas = true,
                    CollideWithBodies = false
                });
                Array<Dictionary> raycastHits = await raycastQuery;

                foreach (var hit in raycastHits)
                {
                    Card hitCard = hit["collider"].AsGodotObject() as Card;
                    if (hitCard != selectedCard)
                    {
                        if (GetChildren().Contains(hitCard))
                        {                            
                            MoveChild(selectedCard, GetCardIndexLeftOrRight(hitCard)); //add left/right drop
                        }
                        else
                        {
                            CardHand cardHand = hitCard.GetParent<CardHand>();
                            if (cardHand != null)
                            {                                                   
                                RemoveCard(selectedCard);
                                cardHand.AddCard(selectedCard, GetCardIndexLeftOrRight(hitCard));
                                cardHand.SortCards();
                            }
                        }
                    }
                }
                selectedCard = null;
                GD.Print($"Card {card.textLabel.Text} released with {@event.AsText()} at global mouse position {GetGlobalMousePosition()} and card.globalPosition {card.GlobalPosition}"); //TODO remove                                                                                                              
                SortCards();
            }
        }
    }

    private int GetCardIndexLeftOrRight(Card hitCard)
    {
        int cardIndex;
        bool isMouseOnCardLeftSide = GetGlobalMousePosition().X < hitCard.GlobalPosition.X;
        if (isMouseOnCardLeftSide)
            cardIndex = hitCard.GetIndex();
        else
            cardIndex = hitCard.GetIndex() + 1;
        return cardIndex;
    }

    private async Task<Array<Dictionary>> getRaycastQuery(PhysicsPointQueryParameters2D physicsPointQueryParameters2D)
    {
        TaskCompletionSource<Array<Dictionary>> query = new();
        Array<Dictionary> action() { return GetWorld2D().DirectSpaceState.IntersectPoint(physicsPointQueryParameters2D); }
        raycastQueries.Add((query, action));

        return await query.Task;
    }
}