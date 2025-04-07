using Sa11ytaire4All.Source;
using Sa11ytaire4All.Views;
using System.Collections.ObjectModel;

namespace Sa11ytaire4All
{
    public sealed partial class MainPage : ContentPage
    {
        // A card in one of the Target Card piles or the Upturned Card has been selected.
        public void CardButtonClicked(Button clickedCardButton)
        {
            CardButton? cardButton = null;
            CardButton? obscuredCardButton = null;

            switch (clickedCardButton.AutomationId)
            {
                case "TargetPileC":
                    cardButton = TargetPileC;
                    break;

                case "TargetPileD":
                    cardButton = TargetPileD;
                    break;
                case "TargetPileH":
                    cardButton = TargetPileH;
                    break;
                case "TargetPileS":
                    cardButton = TargetPileS;
                    break;

                // Barker Todo: Move reacting to the upturned card click to somewhere unrelated to the target card piles.
                case "CardDeckUpturned":
                    CardDeckUpturned_Clicked(CardDeckUpturned);
                    return;

                case "CardDeckUpturnedObscuredLower":
                    obscuredCardButton = CardDeckUpturnedObscuredLower;
                    break;

                case "CardDeckUpturnedObscuredHigher":
                    obscuredCardButton = CardDeckUpturnedObscuredHigher;
                    break;

                default:
                    break;
            }

            if (obscuredCardButton != null)
            {
                obscuredCardButton.SetToggledState(false);

                MakeDelayedScreenReaderAnnouncement(
                    MainPage.MyGetString("ObscuredUpturnedCardCannotBeSelected"));

                return;
            }

            if (cardButton == null)
            {
                return;
            }

            // Toggle the state of the CardButton clicked.
            cardButton.IsToggled = !cardButton.IsToggled;

            // If we've just untoggled a target card pile, there's nothing more to do here.
            if (!cardButton.IsToggled)
            {
                var accessibleName = SemanticProperties.GetDescription(cardButton);

                string announcement =
                    accessibleName + " " + MainPage.MyGetString("Unselected");

                MakeDelayedScreenReaderAnnouncement(announcement);

                return;
            }

            // If multiple target buttons are selected, unselect them and return.
            int countTargetButtonsSelected = 0;
            countTargetButtonsSelected += (TargetPileC.IsToggled ? 1 : 0);
            countTargetButtonsSelected += (TargetPileD.IsToggled ? 1 : 0);
            countTargetButtonsSelected += (TargetPileH.IsToggled ? 1 : 0);
            countTargetButtonsSelected += (TargetPileS.IsToggled ? 1 : 0);

            if (countTargetButtonsSelected > 1)
            {
                ClearAllSelections(true);

                return;
            }

            var cardWasMoved = false;

            // Is the top card in the Upturned Card pile checked?
            if (CardDeckUpturned.IsToggled && (_deckUpturned.Count > 0))
            {
                // Attempt to move the upturned card to the target pile. This action is synchronous.
                cardWasMoved = MoveUpturnedCardToTargetPileAsAppropriate(cardButton);

                // If a card wasn't moved, then leave only the selected target pile card selected.
                if (!cardWasMoved)
                {
                    ClearSelectionStatesAfterTargetCardSelectionChange(cardButton);
                }
                else if (GameOver()) // A card was moved from the Upturned Card Pile to a Target Card Pile.
                {
                    ShowEndOfGameDialog();
                }
            }
            else
            {
                // Attempt to move a card from from of the Dealt Card piles to the Target Card pile.
                // We use a timer here to delay the moving of the card to the target card pile
                // asynchronously.
                cardWasMoved = MoveDealtCardToTargetPileAsAppropriate(cardButton);
            }
        }

        private void ClearSelectionStatesAfterTargetCardSelectionChange(CardButton? cardButton)
        {
            CardDeckUpturned.SetToggledState(false);

            ClearDealtCardPileSelections();

            if (cardButton.Card != null)
            {
                string announcement =
                    cardButton.Card.GetCardAccessibleName() + " " + 
                        MainPage.MyGetString("Selected");

                MakeDelayedScreenReaderAnnouncement(announcement);
            }
            else
            {
                // Do not leave an empty target card pile selected when no card was moved to it.
                cardButton.SetToggledState(false);
            }

            if (GameOver())
            {
                ShowEndOfGameDialog();
            }
        }

        // A Target Card pile button has been checked while the top card in the Upturned Card pile is checked.
        private bool MoveUpturnedCardToTargetPileAsAppropriate(CardButton cardButton)
        {
            bool movedCard = false;

            // Clear all selection from the card pile lists.
            ClearDealtCardPileSelections();

            if (_deckUpturned.Count == 0)
            {
                return false;
            }

            // If the upturned card is not checked, there's nothing to do here.
            if (CardDeckUpturned.IsToggled)
            {
                // cardAbove here is the upturned card.
                DealtCard cardAbove = new DealtCard();
                cardAbove.CardState = CardState.FaceUp;
                cardAbove.Card = _deckUpturned[_deckUpturned.Count - 1];

                // Figure out which TargetPile has been invoked.
                int targetPileIndex = GetTargetPileIndex(cardButton);

                // No action required if the upturned card doesn't match the suit of the TargetPile.
                if (((targetPileIndex == 0) && (cardAbove.Card.Suit != Suit.Clubs)) ||
                    ((targetPileIndex == 1) && (cardAbove.Card.Suit != Suit.Diamonds)) ||
                    ((targetPileIndex == 2) && (cardAbove.Card.Suit != Suit.Hearts)) ||
                    ((targetPileIndex == 3) && (cardAbove.Card.Suit != Suit.Spades)))
                {
                    PlaySound(false);

                    return false;
                }

                // Figure out if we should move the card.
                bool moveCard = false;

                if (cardAbove.Card.Rank == 1)
                {
                    if (_targetPiles[targetPileIndex].Count == 0)
                    {
                        moveCard = true;
                    }
                }
                else
                {
                    List<Card> list = _targetPiles[targetPileIndex];

                    try
                    {
                        if ((cardAbove.Card.Rank == list[list.Count - 1].Rank + 1) &&
                            (cardAbove.Card.Suit == list[list.Count - 1].Suit))
                        {
                            moveCard = true;
                        }
                    }
                    catch (Exception)
                    {
                        moveCard = false;
                    }
                }

                if (moveCard)
                {
                    // Move the upturned card to the Target Pile.
                    _targetPiles[targetPileIndex].Add(cardAbove.Card);

                    // Couldn't seem to get the binding to work for the suit colours, so set them explicitly here. 
                    SetCardSuitColours(cardButton);

                    cardButton.Card = cardAbove.Card;

                    _deckUpturned.Remove(cardAbove.Card);

                    SetUpturnedCardsVisuals();

                    movedCard = true;

                    // Have screen readers make a related announcement.

                    string suitPile = "";

                    switch (targetPileIndex)
                    {
                        case 0:
                            suitPile = MainPage.MyGetString("ClubsPile");
                            break;

                        case 1:
                            suitPile = MainPage.MyGetString("DiamondsPile");
                            break;
                            
                        case 2:
                            suitPile = MainPage.MyGetString("HeartsPile");
                            break;

                        case 3:
                            suitPile = MainPage.MyGetString("SpadesPile");
                            break;

                        default:
                            break;
                    }

                    string announcement =
                        MainPage.MyGetString("Moved") + " " +
                        cardAbove.Card.GetCardAccessibleName() + " " +
                        MainPage.MyGetString("To") + " " + suitPile;

                    MakeDelayedScreenReaderAnnouncement(announcement);
                }
            }

            ClearCardButtonSelections(true);

            PlaySound(movedCard);

            return movedCard;
        }

        // A Target Card pile button has been selected while the top card in the Upturned Card pile is not selected,
        // so attempt to move a card from one of the Dealt Card piles.
        private bool MoveDealtCardToTargetPileAsAppropriate(CardButton cardButton)
        {
            // Determine which TargetPile has been invoked.
            int targetPileIndex = GetTargetPileIndex(cardButton);

            // Get the already-selected card from the other list if there is one.
            CollectionView? listAlreadySelected;
            int listAlreadySelectedIndex;
            var cardAbove = GetSelectedDealtCard(null, // List to be ignored.
                                                 out listAlreadySelected,
                                                 out listAlreadySelectedIndex);

            if (listAlreadySelected == null)
            {
                ClearSelectionStatesAfterTargetCardSelectionChange(cardButton);

                return false;
            }

            if (cardAbove != null)
            {
                // Always deselect the selected item prior to moving anything between lists.
                listAlreadySelected.SelectedItem = null;

                var movingCardData = new MovingCardData
                {
                    CardAbove = cardAbove,
                    ListAlreadySelected = listAlreadySelected,
                    ListAlreadySelectedIndex = listAlreadySelectedIndex,
                    TargetPileCardButton = cardButton,
                    TargetPileIndex = targetPileIndex
                };

                if (timerDelayAttemptToMoveCard == null)
                {
                    timerDelayAttemptToMoveCard = new Timer(
                        new TimerCallback((s) => TimedDelayAttemptToMoveDealtCardToTargetPile(movingCardData)),
                            null,
                            TimeSpan.FromMilliseconds(500),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }

                DeselectCard(cardAbove);
            }

            if (_targetPiles[targetPileIndex].Count == 0)
            {
                cardButton.SetToggledState(false);
            }

            return false;
        }

        private void UpdatePileFaceDownCount(ObservableCollection<DealtCard> dealtCardCollection, DealtCard cardRevealed)
        {
            if (dealtCardCollection.Count == 0)
            {
                return;
            }

            // If the card revealed following a move is already face-up, no action is required here.
            if (!cardRevealed.FaceDown)
            {
                return;
            }

            // The revealed card will turned up, so decrement the count of face-down cards in the pile.
            --dealtCardCollection[0].CountFaceDownCardsInPile;

            // Force a refresh of the height of the revealed card if necessary.
            if (dealtCardCollection.Count > 0)
            {
                // Barker Todo: The count in this card will never be used, so find a cleaner way of forcing the height refresh.
                --cardRevealed.CountFaceDownCardsInPile;
            }
        }
    }
}
