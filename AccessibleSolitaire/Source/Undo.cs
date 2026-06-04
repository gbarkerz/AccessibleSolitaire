using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Sa11ytaire4All
{
    public sealed partial class MainPage : ContentPage
    {
        private bool moveToUpturnedPile;
        private Card? moveToUpturnedCard;

        private bool moveFromUpturnedPile;
        private Card? moveFromUpturnedCard;

        private int moveTargetPileIndex;
        private Card? moveTargetPileCard;

        private int moveIndexSource = -1;
        private int moveIndexDestination = -1;
        private ObservableCollection<DealtCard> dealtCardCollectionMoveSource = new ObservableCollection<DealtCard>();
        private ObservableCollection<DealtCard> dealtCardCollectionMoveDestination = new ObservableCollection<DealtCard>();

        private bool undoSpiderNextCardsAction = false;
        private int undoSpiderDiscardedSequenceCount = -1;

        private CardButton? undoGrandfatherClockCardButton;
        private DealtCard? undoGrandfatherClockDealtCard;

        private void ClearUndoState()
        {
            moveToUpturnedPile = false;
            moveToUpturnedCard = null;

            moveFromUpturnedPile = false;
            moveFromUpturnedCard = null;

            moveTargetPileIndex = -1;
            moveTargetPileCard = null;

            moveIndexSource = -1;
            moveIndexDestination = -1;

            dealtCardCollectionMoveSource.Clear();
            dealtCardCollectionMoveDestination.Clear();

            undoSpiderNextCardsAction = false;
            undoSpiderDiscardedSequenceCount = -1;

            moveTriPeakCard = null;

            undoGrandfatherClockCardButton = null;
            undoGrandfatherClockDealtCard = null;
        }

        private void RememberCardStateForUndo(bool toUpturnedPile,
                                              bool fromUpturnedPile,
                                              int targetPileIndex,
                                              Card? targetCardPile,
                                              CardButton? grandfatherClockCardButton,
                                              DealtCard? grandfatherClockDealtCard,
                                              int indexSource,
                                              ObservableCollection<DealtCard?>? itemsMoveSource,
                                              int indexDestination,
                                              ObservableCollection<DealtCard?>? itemsMoveDestination)
        {
            ClearUndoState();

            // Check the upturned cards.

            moveFromUpturnedPile = fromUpturnedPile;
            moveFromUpturnedCard = null;

            if (moveFromUpturnedPile)
            {
                moveFromUpturnedCard = CardDeckUpturned.Card;
            }

            moveToUpturnedPile = toUpturnedPile;
            moveToUpturnedCard = null;

            if (moveToUpturnedPile)
            {
                var remainingCount = _deckRemaining.Count;

                moveToUpturnedCard = (remainingCount > 0 ? _deckRemaining[remainingCount - 1] : null);
            }

            // Check the target card piles.

            moveTargetPileIndex = targetPileIndex;
            moveTargetPileCard = targetCardPile;

            // Check the dealt card piles.

            moveIndexSource = indexSource;
            moveIndexDestination = indexDestination;

            dealtCardCollectionMoveSource.Clear();
            dealtCardCollectionMoveDestination.Clear();

            if (itemsMoveSource != null)
            {
                foreach (var card in itemsMoveSource)
                {
                    DealtCard? newCard = null;

                    var cardJson = JsonSerializer.Serialize(card);
                    if (!string.IsNullOrEmpty(cardJson))
                    {
                        newCard = JsonSerializer.Deserialize<DealtCard>(cardJson);
                        if (newCard != null)
                        {
                            dealtCardCollectionMoveSource.Add(newCard);
                        }
                    }
                }
            }

            if (itemsMoveDestination != null)
            {
                foreach (var card in itemsMoveDestination)
                {
                    DealtCard? newCard = null;

                    var cardJson = JsonSerializer.Serialize(card);
                    if (!string.IsNullOrEmpty(cardJson))
                    {
                        newCard = JsonSerializer.Deserialize<DealtCard>(cardJson);
                        if (newCard != null)
                        {
                            dealtCardCollectionMoveDestination.Add(newCard);
                        }
                    }
                }
            }

            if (currentGameType == SolitaireGameType.Grandfathersclock)
            {
                DealtCard? newDealtCard = null;

                var cardJson = JsonSerializer.Serialize(grandfatherClockDealtCard);
                if (!string.IsNullOrEmpty(cardJson))
                {
                    newDealtCard = JsonSerializer.Deserialize<DealtCard>(cardJson);
                    if (newDealtCard != null)
                    {
                        undoGrandfatherClockDealtCard = newDealtCard;

                        undoGrandfatherClockCardButton = grandfatherClockCardButton;
                    }
                }
            }
        }

        public void UndoLastMove()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Check the upturned card pile.

            var countUpturned = -1;

            if (moveFromUpturnedPile && (moveFromUpturnedCard != null))
            {
                _deckUpturned.Add(moveFromUpturnedCard);

                countUpturned = _deckUpturned.Count;

                CardDeckUpturned.Card = moveFromUpturnedCard;

                if (countUpturned > 1)
                {
                    CardDeckUpturnedObscuredHigher.Card = _deckUpturned[countUpturned - 2];

                    if (countUpturned > 2)
                    {
                        CardDeckUpturnedObscuredLower.Card = _deckUpturned[countUpturned - 3];
                    }
                }
            }

            moveFromUpturnedPile = false;
            moveFromUpturnedCard = null;

            if (moveToUpturnedPile && (moveToUpturnedCard != null))
            {
                _deckRemaining.Add(moveToUpturnedCard);

                countUpturned = _deckUpturned.Count;

                if (countUpturned > 0)
                {
                    _deckUpturned.RemoveAt(countUpturned - 1);

                    --countUpturned;

                    CardDeckUpturned.Card = (countUpturned > 0 ? _deckUpturned[countUpturned - 1] : null);
                }

                CardDeckUpturnedObscuredHigher.Card = (countUpturned > 1 ? _deckUpturned[countUpturned - 2] : null);
                CardDeckUpturnedObscuredLower.Card = (countUpturned > 2 ? _deckUpturned[countUpturned - 3] : null);
            }

            moveToUpturnedPile = false;
            moveToUpturnedCard = null;

            // Check target card piles.

            if (moveTargetPileIndex >= 0)
            {
                var countTargetCards = _targetPiles[moveTargetPileIndex].Count;
                if (countTargetCards > 0)
                {
                    if (moveTargetPileCard == null)
                    {
                        _targetPiles[moveTargetPileIndex].RemoveAt(countTargetCards - 1);

                        --countTargetCards;
                    }
                    else
                    {
                        _targetPiles[moveTargetPileIndex].Add(moveTargetPileCard);

                        ++countTargetCards;
                    }

                    var newTopCard = (countTargetCards > 0 ? _targetPiles[moveTargetPileIndex][countTargetCards - 1] : null);

                    switch (moveTargetPileIndex)
                    {
                        case 0:
                            TargetPileC.Card = newTopCard;
                            break;
                        case 1:
                            TargetPileD.Card = newTopCard;
                            break;
                        case 2:
                            TargetPileH.Card = newTopCard;
                            break;
                        case 3:
                            TargetPileS.Card = newTopCard;
                            break;
                    }
                }
            }

            moveTargetPileIndex = -1;
            moveTargetPileCard = null;

            // Check the dealt card piles.

            if (moveIndexSource >= 0)
            {
                var sourceCardArray = vm.DealtCards[moveIndexSource];

                var sourceCount = dealtCardCollectionMoveSource.Count;
                if (sourceCount > 0)
                {
                    sourceCardArray.Clear();

                    for (int i = 0; i < sourceCount; ++i)
                    {
                        var card = dealtCardCollectionMoveSource[i];

                        card.IsLastCardInPile = (i == sourceCount - 1);

                        sourceCardArray.Add(card);
                    }
                }
            }

            if (moveIndexDestination >= 0)
            {
                var destinationCardArray = vm.DealtCards[moveIndexDestination];

                var destinationCount = dealtCardCollectionMoveDestination.Count;
                if (destinationCount > 0)
                {
                    destinationCardArray.Clear();

                    for (int i = 0; i < destinationCount; ++i)
                    {
                        var card = dealtCardCollectionMoveDestination[i];

                        card.IsLastCardInPile = (i == destinationCount - 1);

                        destinationCardArray.Add(card);
                    }
                }
            }

            dealtCardCollectionMoveSource.Clear();
            dealtCardCollectionMoveDestination.Clear();

            ClearDealtCardPileSelections();

            if (currentGameType == SolitaireGameType.Spider)
            {
                UndoCheckSpiderDiscardedSequenceCount();

                if (undoSpiderNextCardsAction)
                {
                    UndoSpiderNextCardsAction();
                }
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                UndoTriPeaksMove();
            }
            else if (currentGameType == SolitaireGameType.Grandfathersclock)
            {
                if ((undoGrandfatherClockCardButton != null) && (undoGrandfatherClockDealtCard != null))
                {
                    for (var i = 0; i < 12; ++i)
                    {
                        var clockDealtCard = vm.DealtCards[8][i];
                        if ((clockDealtCard != null) && (clockDealtCard.StackDetails == undoGrandfatherClockCardButton.StackDetails))
                        {
                            clockDealtCard.Card = undoGrandfatherClockDealtCard.Card;
                            clockDealtCard.StackDetails = undoGrandfatherClockDealtCard.StackDetails;

                            undoGrandfatherClockCardButton.StackDetails = undoGrandfatherClockDealtCard.StackDetails;
                            undoGrandfatherClockCardButton.Card = undoGrandfatherClockDealtCard.Card;

                            break;
                        }
                    }
                }
            }
        }

        private void RememberSpiderCardStateForUndoNextCards()
        {
            ClearUndoState();

            undoSpiderNextCardsAction = true;

            undoSpiderDiscardedSequenceCount = -1;
        }

        private void RememberSpiderDiscardedSequenceCount()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            undoSpiderDiscardedSequenceCount = vm.SpiderDiscardedSequenceCount;
        }

        private void UndoSpiderNextCardsAction()
        {
            undoSpiderNextCardsAction = false;

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            for (int i = 9; i >= 0; --i)
            {
                var pileCount = vm.DealtCards[i].Count;
                if (pileCount > 0)
                {
                    var dealtCard = vm.DealtCards[i][pileCount - 1];

                    _deckRemaining.Add(dealtCard.Card);

                    vm.DealtCards[i].RemoveAt(pileCount - 1);

                    --pileCount;

                    if (pileCount > 0)
                    {
                        var lastCard = vm.DealtCards[i][pileCount - 1];
                        if (lastCard != null)
                        {
                            lastCard.IsLastCardInPile = true;
                        }
                    }
                    else
                    {
                        // The pile is now empty again, so add the empty placeholder item.
                        AddEmptyCardToCollectionView(vm.DealtCards[i], i);
                    }
                }
            }

            UndoCheckSpiderDiscardedSequenceCount();
        }

        private void UndoCheckSpiderDiscardedSequenceCount()
        {
            if (undoSpiderDiscardedSequenceCount != -1)
            {
                var vm = this.BindingContext as DealtCardViewModel;
                if ((vm == null) || (vm.DealtCards == null))
                {
                    return;
                }

                vm.SpiderDiscardedSequenceCount = undoSpiderDiscardedSequenceCount;

                SetSpiderDiscardedSequenceDetails();

                undoSpiderDiscardedSequenceCount = -1;
            }
        }

        private DealtCard? moveTriPeakCard = null;

        private void RememberTriPeaksStateForUndo(DealtCard? triPeakCard)
        {
            ClearUndoState();

            moveTriPeakCard = triPeakCard;
        }

        private void UndoTriPeaksMove()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            if ((moveTriPeakCard != null) &&
                (moveTriPeakCard.PyramidRow >= 0) && (moveTriPeakCard.PyramidCardOriginalIndexInRow >= 0))
            {
                var dealtCard = vm.DealtCards[moveTriPeakCard.PyramidRow][moveTriPeakCard.PyramidCardOriginalIndexInRow];
                if (dealtCard != null)
                {
                    var upturnedCount = _deckUpturned.Count;
                    if (upturnedCount > 0)
                    {
                        // Get the card that was previously moved to the upturned card pile.
                        var card = _deckUpturned[upturnedCount - 1];

                        // Restore the card to the card that was moved.
                        moveTriPeakCard.Card = card;

                        // Find the CardButton that was previously made not visible.
                        int cardButtonPyramidIndex;
                        var cardButton = GetCardButtonFromPyramidDealtCard(moveTriPeakCard, out cardButtonPyramidIndex);
                        if (cardButton != null)
                        {
                            cardButton.IsVisible = true;

                            // Now restore the top card shown on the upturned card pile.
                            _deckUpturned.RemoveAt(upturnedCount - 1);

                            --upturnedCount;

                            if (upturnedCount > 0)
                            {
                                CardDeckUpturned.Card = _deckUpturned[upturnedCount - 1];
                            }
                        }

                        // Has a card now been revealed? 
                        SetOnTopStateFollowingMove(dealtCard, false, true);

                        //RefreshCardButtonMofNInRow(cardButtonClicked);
                    }
                }
            }

            moveTriPeakCard = null;
        }
    }
}