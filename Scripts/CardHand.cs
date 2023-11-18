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

    List<(TaskCompletionSource<Array<Dictionary>> query, Func<Array<Dictionary>> raycastAction)> raycastQueries = new();
    List<(Card card, CollisionObject2D.InputEventEventHandler cardInputHandler)> cardInputHandlers = new();
    public override void _Ready()
    {
        // Subscribe to initial cards added from editor
        Array<Node> cards = GetChildren();
        foreach (Card card in cards.Cast<Card>())
        {
            SubscribeToCardInputEvent(card);
        }
        SortCards();
    }

    private void SubscribeToCardInputEvent(Card card)
    {
        // Lambda function matching the signature of the event InputEvent.
        //
        // Note that CardIsClicked is async but SubscribeToCardInputEvent is not - returning void stops the async zombie virus:
        // https://learn.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming#async-all-the-way
        //
        // Handle for the event handler is stored so we can unsubscribe later when removing the card - removing a child does not automatically unsubscribe a parent!
        // Using a closure we can provide a reference to "card" without passing it as a parameter (which would invalidate the InputEvent signature)
        CollisionObject2D.InputEventEventHandler cardInputHandler = (Node viewport, InputEvent @event, long shapeIdx) => CardIsClicked(@event,card);
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
            // Without a correct offset, a card's center point will snap to the mouse, regardless of where on the card we clicked it
            selectedCard.GlobalPosition = GetGlobalMousePosition() - selectionOffset;
        }
    }

    private void DoRaycastQueries()
    {
        for (int i = raycastQueries.Count - 1; i >= 0; i--)
        {
            // Using SetResult from a TaskCompletionSource we can, effectively, synchronously set a result for an async task
            raycastQueries[i].query.SetResult(raycastQueries[i].raycastAction());
            raycastQueries.RemoveAt(i);
        }
    }

    public void ReparentCard(Card card, int index, CardHand cardHand)
    {
        UnSubscribeToCardInputEvent(card);
        // We could manually reparent using AddChild and RemoveChild, but this changes a cards position as it is relative to its parent. Reparent handles this for us.
        card.Reparent(cardHand); 
        cardHand.MoveChild(card, index);
        cardHand.SubscribeToCardInputEvent(card);
    }

    public void SortCards()
    {
        Array<Node> cards = GetChildren();
        var i = 0;
        foreach (Card card in cards.Cast<Card>())
        {
            Vector2 position = new Vector2(128f * i, 0);
            i++;
            card.tween?.Kill();
            card.tween = CreateTween();
            card.tween.TweenProperty(card, "position", position, sortSpeed);            
        }
    }

    // Note the use of async since the method contains await.
    // Official explanation of async and await by cooking breakfast: https://learn.microsoft.com/en-US/dotnet/csharp/asynchronous-programming/
    public async void CardIsClicked(InputEvent @event, Card card)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            // Not only check for left mouse button pressed, but also only allow a single card to be selected.
            // (In case of cards overlapping within a hand we get multiple events per click)
            if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && selectedCard == null)
            {
                selectedCard = card;
                selectedCard.ZIndex = 1;
                selectionOffset = mouseEvent.GlobalPosition - card.GlobalPosition;                
                card.tween?.Kill(); // Immediately grab card, do not let existing animation finish
            }
            if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left && card == selectedCard)
            {
                // Queue up a raycast query.
                //
                // Basically we want to run "GetWorld2D().DirectSpaceState.IntersectPoint(physicsPointQueryParameters2D)", but...
                // ... IntersectPoint can only be run from _physicsProcess due to engine multithreading reasons.
                //
                // One solution (used here) is to queue up a raycast query and wait for _physicsProcess to run it for us.
                // There will be some input lag on the order of 1/60s, luckily this does not seem to be noticeable for card dropping.
                //
                // Another solution is to place all the input handling directly into _physicsProcess and poll all inputs at 60hz -                
                // See input examples in the docs: https://docs.godotengine.org/en/stable/tutorials/inputs/input_examples.html#events-versus-polling
                //
                // A raycast query queue is basically a hybrid between event and polling, only checking select querries instead of all inputs.
                Task<Array<Dictionary>> raycastQuery = GetRaycastQuery(new PhysicsPointQueryParameters2D()
                {
                    Position = GetGlobalMousePosition(),
                    CollideWithAreas = true,
                    CollideWithBodies = false
                });

                // Roll our thumbs until _physicsProcess completes our query
                Array<Dictionary> raycastHits = await raycastQuery;
                
                TryToDropSelectedCardOnACard(raycastHits);

                selectedCard.ZIndex = 0;
                selectedCard = null;
                SortCards();
            }
        }
    }

    private void TryToDropSelectedCardOnACard(Array<Dictionary> raycastHits)
    {
        foreach (var hit in raycastHits)
        {
            Card hitCard = hit["collider"].AsGodotObject() as Card;

            // Dropping ourself on ourself seems silly
            if (hitCard != selectedCard)
            {
                if (GetChildren().Contains(hitCard))
                {
                    MoveChild(selectedCard, GetCardIndexLeftOrRight(hitCard));
                }
                else
                {
                    //Assumes all cards have a CardHand parent, free-floating cards left as project fork exercise for the reader.
                    CardHand cardHand = hitCard.GetParent<CardHand>();
                    if (cardHand != null)
                    {
                        ReparentCard(selectedCard, GetCardIndexLeftOrRight(hitCard), cardHand);
                        cardHand.SortCards();
                    }
                }
                return;
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

    private async Task<Array<Dictionary>> GetRaycastQuery(PhysicsPointQueryParameters2D physicsPointQueryParameters2D)
    {
        // TaskCompletionSource allows us to pack a bit of code in a lunch box and let someone else execute it in their context (e.g. _physicsProcess)
        // Meanwhile we await the result.
        TaskCompletionSource<Array<Dictionary>> query = new();
        Array<Dictionary> raycast() { return GetWorld2D().DirectSpaceState.IntersectPoint(physicsPointQueryParameters2D); }
        raycastQueries.Add((query, raycast));

        return await query.Task;
    }
}