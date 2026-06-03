using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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

        private void RememberCardStateForUndo(bool toUpturnedPile,
                                                bool fromUpturnedPile,
                                                int targetPileIndex,
                                                Card? targetCardPile,
                                                int indexSource,
                                                ObservableCollection<DealtCard?>? itemsMoveSource,
                                                int indexDestination,
                                                ObservableCollection<DealtCard?>? itemsMoveDestination)
        {
            undoSpiderNextCardsAction = false;

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
        }

        public void UndoLastMove()
        {
            if (undoSpiderNextCardsAction)
            {
                UndoSpiderNextCardsAction();

                return;
            }

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
            }
        }

        private void RememberSpiderCardStateForUndoNextCards()
        {
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
    }
}
