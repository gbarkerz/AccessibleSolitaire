using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    public sealed partial class MainPage : ContentPage
    {
        private void SetSpiderDiscardedSequenceDetails(string count)
        {
            SpiderDiscardedSequenceCountLabel.Text = count;

            SemanticProperties.SetDescription(
                SpiderDiscardedSequenceCountLabel,
                MainPage.MyGetString("DiscardedSequenceCount") + " " + count);
        }

        private async void SpiderPerformNextCardAction()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // There should always be a multiple of 10 cards left.

            Debug.WriteLine("SpiderPerformNextCardAction: Remaining card count " + _deckRemaining.Count);

            // Check first that the NextCard action is available given the current state of the piles.
            if (!SpiderIsNextCardButtonActionAvailable())
            {
                await DisplayAlertAsync(
                    MainPage.MyGetString("AccessibleSolitaire"),
                    MainPage.MyGetString("SpiderCardsCannotBeDealt"),
                    MainPage.MyGetString("OK"));

                return;
            }

            var announceDealMessage = true;

            for (int i = 0; i < 10; ++i)
            {
                var card = _deckRemaining[_deckRemaining.Count - 1];

                _deckRemaining.Remove(card);

                var currentPileCount = vm.DealtCards[i].Count;
                if (currentPileCount > 0)
                {
                    if ((currentPileCount > 1) || (vm.DealtCards[i][0].CardState != CardState.KingPlaceHolder))
                    {
                        // The pile is not empty, so mark the existing topmost card in the pile as no longer topmost.
                        var currentDealtCard = vm.DealtCards[i][currentPileCount - 1] as DealtCard;
                        if (currentDealtCard != null)
                        {
                            currentDealtCard.IsLastCardInPile = false;
                        }
                    }
                    else
                    {
                        // The pile was empty, so remove the empty space placeholder.
                        vm.DealtCards[i].RemoveAt(0);
                    }
                }
                else
                {
                    Debug.WriteLine("SpiderPerformNextCardAction: Unexpected zero item count.");
                }

                // Now add the topmost card to the pile.
                var newDealtCard = new DealtCard();

                newDealtCard.FaceDown = false;
                newDealtCard.CardState = CardState.FaceUp;
                newDealtCard.Card = card;
                newDealtCard.IsLastCardInPile = true;

                newDealtCard.CurrentDealtCardPileIndex = i;
                newDealtCard.CurrentCardIndexInDealtCardPile = vm.DealtCards[i].Count;

                vm.DealtCards[i].Add(newDealtCard);

                // Make sure its image is loaded to appear in the dealt card pile.
                TryToAddCardImageWithPictureToDictionary(newDealtCard);

                // If the added card was an Ace, check whether that completes a King to Ace sequence.
                if (card.Rank == 1)
                {
                    // If a sequence is complete, all the subsequence action is taken below here.
                    var sequenceComplete = CheckForSpiderSequenceComplete(vm.DealtCards[i], i);
                    if (sequenceComplete)
                    {
                        announceDealMessage = false;
                    }

                    Debug.WriteLine("SpiderPerformNextCardAction: Did Ace complete sequence - " + sequenceComplete);
                }
            }

            // If we're not announcing a message about a sequence completing, announce the deal.
            if (announceDealMessage)
            {
                var announcement = MainPage.MyGetString("SpiderDealtTenCards");
                MakeDelayedScreenReaderAnnouncement(announcement, false);
            }
        }

        private bool CheckForSpiderSequenceComplete(ObservableCollection<DealtCard> dealtCards, int cardPileIndex)
        {
            if ((dealtCards == null) || (dealtCards.Count < 13))
            {
                return false;
            }

            var spiderSequenceComplete = false;

            var aceIndex = -1;
            var kingIndex = -1;

            if (currentGameType == SolitaireGameType.Spider)
            {
                // Does the target dealt card pile contain a sequence from King to Ace?
                // Start from the top of this pile working downwards until we find an Ace.
                for (int i = dealtCards.Count - 1; i >= 0; --i)
                {
                    var card = dealtCards[i].Card;

                    // Have we found an ace?
                    if ((dealtCards[i] != null) && (card != null) && (card.Rank == 1))
                    {
                        // We've found an Ace.
                        aceIndex = i;

                        var nextRankInSequence = 2;

                        for (int j = i - 1; j >= 0; --j)
                        {
                            card = dealtCards[j].Card;

                            if ((card != null) && (card.Rank != nextRankInSequence))
                            {
                                break;
                            }

                            ++nextRankInSequence;
                        }

                        // Did we find an entire sequence leading to a King?
                        if (nextRankInSequence == 14)
                        {
                            Debug.WriteLine("CheckForSpiderSequenceComplete: Found sequence found from Ace at index " +
                                aceIndex + ".");

                            // We did find a sequence.
                            kingIndex = aceIndex - 12;

                            spiderSequenceComplete = true;

                            break;
                        }
                        else
                        {
                            // We didn't find a sequence, so continue looking if there may still be a sequence.
                            if (i >= 13)
                            {
                                Debug.WriteLine("CheckForSpiderSequenceComplete: No sequence found from Ace at index " +  
                                    i + ". Check for another Ace in the pile.");

                                aceIndex = -1;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                if (spiderSequenceComplete)
                {
                    var sequenceIsAtTopOfPile = true;

                    // Is there a card higher in the pile above the sequence?
                    if (aceIndex < dealtCards.Count - 1)
                    {
                        sequenceIsAtTopOfPile = false;
                    }

                    // Remove the completed sequence.
                    for (int i = aceIndex; i >= kingIndex; --i)
                    {
                        dealtCards.RemoveAt(i);
                    }

                    // Check for the pile now being empty.
                    if (dealtCards.Count == 0)
                    {
                        AddEmptyCardToCollectionView(dealtCards, cardPileIndex);
                    }
                    else
                    {
                        // There are other cards still remaining in the pile from which the sequence has
                        // been removed. If all these cards were lower in the pile than the sequence, 
                        // we have a new topmost card in the pile.
                        if (sequenceIsAtTopOfPile)
                        {
                            var cardRevealed = (DealtCard)dealtCards[kingIndex - 1];
                            if (cardRevealed != null)
                            {
                                // Update the count of face-down cards in the pile in the bottom card in the pile.
                                UpdatePileFaceDownCount(dealtCards, cardRevealed);

                                // The card being revealed is no longer face-down, and is now the
                                // last card in the list.

                                TurnUpCardWithImage(cardRevealed);

                                cardRevealed.CardState = CardState.FaceUp;
                                cardRevealed.IsLastCardInPile = true;

                                cardRevealed.RefreshVisuals();
                            }
                        }
                    }

                    var countText = SpiderDiscardedSequenceCountLabel.Text;
                    
                    int count;                    
                    if (!int.TryParse(countText, out count))
                    {
                        count = 0;
                    }

                    SetSpiderDiscardedSequenceDetails((count + 1).ToString());

                    var announcement = MainPage.MyGetString("SpiderDiscardCompletedSequence") + " " + (count + 1);
                    MakeDelayedScreenReaderAnnouncement(announcement, false);

                    // Is the game over now?
                    if (GameOver())
                    {
                        ShowEndOfGameDialog(false);
                    }
                }
            }

            return spiderSequenceComplete;
        }

        private bool SpiderIsNextCardButtonActionAvailable()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return false;
            }

            var foundEmptyPile= false;
            var foundPileWithMultipleCards = false;

            // If there are no empty piles, then more cards can be dealt if they're available.
            for (int i = 0; i < 10; ++i)
            {
                if (vm.DealtCards[i] != null)
                {
                    var cardCountInPile = vm.DealtCards[i].Count;
                    if (cardCountInPile > 0)
                    {
                        // Get the bottom card in this pile.
                        var card = vm.DealtCards[i][0];
                        if (card != null)
                        {
                            // Is this a empty pile's placeholder item?
                            if ((cardCountInPile == 1) && (card.CardState == CardState.KingPlaceHolder))
                            {
                                foundEmptyPile = true;

                                // We've found an empty pile. If any other pile contains more than
                                // one card, then the NextCard button action is not available.

                                // If we've already found a pile with multiple cards, then we're done.
                                if (foundPileWithMultipleCards)
                                {
                                    break;
                                }
                                else
                                {
                                    // Continue by checking the remaining piles that follow the empty pile.
                                    for (int j = i + 1; j < 10; ++j)
                                    {
                                        if ((vm.DealtCards[j] != null) && (vm.DealtCards[j].Count > 1))
                                        {
                                            foundPileWithMultipleCards = true;

                                            break;
                                        }
                                    }
                                }
                            }
                            else if (cardCountInPile > 1)
                            {
                                // This pile has more than one card in it.
                                foundPileWithMultipleCards = true;
                            }
                        }
                    }
                }
            }

            // The NextCard action is available if no empty piles exist, or there are no piles
            // that contain multiple cards.
            return (!foundEmptyPile || !foundPileWithMultipleCards);
        }
    }
}
