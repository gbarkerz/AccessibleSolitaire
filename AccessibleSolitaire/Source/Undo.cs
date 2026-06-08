using CommunityToolkit.Maui.Behaviors;
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

        private CardButton? undoPyramidCardButtonClicked;
        private DealtCard? undoPyramidDealtCardClicked;
        private CardButton? undoPyramidCardButtonAlreadySelected;
        private DealtCard? undoPyramidDealtCardAlreadySelected;
        private CardButton? undoPyramidCardButtonUpturned;
        private Card? undoPyramidCardUpturned;
        private Card? undoPyramidCardObscuredHigher;

        private bool undoRemoveFromUpturnedPile;
        private CardButton? undoPyramidCardButtonKing;
        private Card? undoPyramidCardKing;

        private int undoDealtCardClickedPyramidRow = -1;
        private int undoDealtCardClickedPyramidCardOriginalIndexInRow = -1;
        private int undoDealtCardAlreadySelectedPyramidRow = -1;
        private int undoDealtCardAlreadySelectedPyramidCardOriginalIndexInRow = -1;
        private DealtCard? undoDealtCardClicked;
        private DealtCard? undoDealtCardAlreadySelected;
        private CardButton? undoCardButtonClicked;
        private CardButton? undoCardButtonAlreadySelected;
        private bool undoRoyalParadeNextCardAction;

        private void SetUndoButtonState(bool enabled)
        {
            if (UndoButton.IsEnabled == enabled)
            {
                return;
            }

            UndoButton.Behaviors.Clear();

            var behavior = new IconTintColorBehavior
            {
                TintColor = (enabled ? Colors.White : Color.FromRgb(0xA0, 0xA0, 0xA0))
            };

            UndoButton.Behaviors.Add(behavior);

            UndoButton.IsEnabled = enabled;
        }

        private void ClearUndoState()
        {
            SetUndoButtonState(false);

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

            undoPyramidCardButtonClicked = null;
            undoPyramidDealtCardClicked = null;
            undoPyramidCardButtonAlreadySelected = null;
            undoPyramidDealtCardAlreadySelected = null;
            undoPyramidCardButtonUpturned = null;
            undoPyramidCardUpturned = null;
            undoPyramidCardObscuredHigher = null;
            undoPyramidCardButtonKing = null;
            undoPyramidCardKing = null;
            undoRemoveFromUpturnedPile = false;

            undoDealtCardClickedPyramidRow = -1;
            undoDealtCardClickedPyramidCardOriginalIndexInRow = -1;
            undoDealtCardAlreadySelectedPyramidRow = -1;
            undoDealtCardAlreadySelectedPyramidCardOriginalIndexInRow = -1;
            undoDealtCardAlreadySelected = null;
            undoCardButtonClicked = null;
            undoCardButtonAlreadySelected = null;
            undoRoyalParadeNextCardAction = false;
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

            SetUndoButtonState(true);
        }

        public void UndoLastMove()
        {
            SetUndoButtonState(false);

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
            else if (currentGameType == SolitaireGameType.Royalparade)
            {
                UndoRoyalPyramidMove();
            }
            else if (currentGameType == SolitaireGameType.Pyramid)
            {
                UndoPyramidMove();
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

            SetUndoButtonState(true);
        }

        private void RememberRoyalParadeCardStateForUndo(
            bool clearUndoState,
            CardButton? cardButtonClicked,
            DealtCard? dealtCardClicked,
            int dealtCardClickedPyramidRow,
            int dealtCardClickedPyramidCardOriginalIndexInRow,
            CardButton? cardButtonAlreadySelected,
            DealtCard? dealtCardAlreadySelected,
            int dealtCardAlreadySelectedPyramidRow,
            int dealtCardAlreadySelectedPyramidCardOriginalIndexInRow)
        {
            if (clearUndoState)
            {
                ClearUndoState();
            }

            DealtCard? newCard = null;

            if (dealtCardClicked != null)
            {
                var cardJson = JsonSerializer.Serialize(dealtCardClicked);
                if (!string.IsNullOrEmpty(cardJson))
                {
                    newCard = JsonSerializer.Deserialize<DealtCard>(cardJson);
                    if (newCard != null)
                    {
                        undoDealtCardClicked = newCard;

                        undoCardButtonClicked = cardButtonClicked;

                        undoDealtCardClickedPyramidRow = dealtCardClickedPyramidRow;
                        undoDealtCardClickedPyramidCardOriginalIndexInRow = dealtCardClickedPyramidCardOriginalIndexInRow;
                    }
                }
            }

            if (dealtCardAlreadySelected != null)
            {
                var cardJson = JsonSerializer.Serialize(dealtCardAlreadySelected);
                if (!string.IsNullOrEmpty(cardJson))
                {
                    newCard = JsonSerializer.Deserialize<DealtCard>(cardJson);
                    if (newCard != null)
                    {
                        undoDealtCardAlreadySelected = newCard;

                        undoCardButtonAlreadySelected = cardButtonAlreadySelected;

                        undoDealtCardAlreadySelectedPyramidRow = dealtCardAlreadySelectedPyramidRow;
                        undoDealtCardAlreadySelectedPyramidCardOriginalIndexInRow = dealtCardAlreadySelectedPyramidCardOriginalIndexInRow;
                    }
                }
            }

            SetUndoButtonState(true);
        }

        private void RememberRoyalParadeNextCardAction()
        {
            ClearUndoState();

            undoRoyalParadeNextCardAction = true;

            SetUndoButtonState(true);
        }

        private void RememberPyramidCardStateForUndo(
            bool clearUndoState,
            CardButton? cardButtonClicked,
            DealtCard? dealtCardClicked,
            CardButton? cardAlreadySelected,
            DealtCard? dealtCardAlreadySelected)
        {
            if (clearUndoState)
            {
                ClearUndoState();
            }

            undoPyramidCardButtonClicked = cardButtonClicked;
            undoPyramidDealtCardClicked = dealtCardClicked;

            undoPyramidCardButtonAlreadySelected = cardAlreadySelected;
            undoPyramidDealtCardAlreadySelected = dealtCardAlreadySelected;

            SetUndoButtonState(true);
        }

        private void RememberPyramidCardDeckUpturnedForUndo(
            bool clearUndoState,
            bool isUpturnedPile, 
            CardButton cardDeckUpturned)
        {
            if (clearUndoState)
            {
                ClearUndoState();
            }

            if (cardDeckUpturned.Card != null)
            {
                undoRemoveFromUpturnedPile = isUpturnedPile;

                undoPyramidCardUpturned = cardDeckUpturned.Card;

                undoPyramidCardButtonUpturned = cardDeckUpturned;
            }

            SetUndoButtonState(true);
        }

        private void RememberPyramidCardRemoveKing(bool isUpturnedPile, CardButton cardDeckUpturned)
        {
            ClearUndoState();

            undoRemoveFromUpturnedPile = isUpturnedPile;

            undoPyramidCardButtonKing = cardDeckUpturned;
            undoPyramidCardKing = cardDeckUpturned.Card;

            SetUndoButtonState(true);
        }

        private void RememberPyramidCardUpturnedAndObscuredForUndo(Card? cardUpturned, Card? cardObscuredHigher)
        {
            ClearUndoState();

            undoPyramidCardUpturned = cardUpturned;
            undoPyramidCardObscuredHigher = cardObscuredHigher;

            SetUndoButtonState(true);
        }

        private void RememberSpiderDiscardedSequenceCount()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            undoSpiderDiscardedSequenceCount = vm.SpiderDiscardedSequenceCount;

            SetUndoButtonState(true);
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

        private void UndoRoyalPyramidMove()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            if (undoRoyalParadeNextCardAction)
            {
                var gridCards = CardPileGridPyramid.Children;

                // Remove the top ten cards from the fourth row.

                for (var cardIndexInPile = 0; cardIndexInPile < 8; ++cardIndexInPile)
                {
                    for (var fourthRowPileIndex = 9; fourthRowPileIndex > 0; --fourthRowPileIndex)
                    {
                        var index = ((8 * fourthRowPileIndex) - 1) - cardIndexInPile;
                        
                        var fourthRowDealtCard = vm.DealtCards[3][index];
                        if (fourthRowDealtCard != null)
                        {
                            _deckRemaining.Add(fourthRowDealtCard.Card);

                            vm.DealtCards[3][index] = null;

                            if (fourthRowPileIndex > 0)
                            {
                                CardButton? cardButton = null;

                                var revealedCardIndex = index - 8;
                                if (revealedCardIndex >= 0)
                                {
                                    cardButton = gridCards[24 + (revealedCardIndex % 8)] as CardButton;
                                    if (cardButton != null)
                                    {
                                        var revealedDealtCard = vm.DealtCards[3][revealedCardIndex];
                                        if (revealedDealtCard != null)
                                        {
                                            cardButton.Card = revealedDealtCard.Card;

                                            cardButton.RefreshVisuals();
                                        }
                                    }
                                }
                                else
                                {
                                    cardButton = gridCards[24 + index] as CardButton;
                                    if (cardButton != null)
                                    {
                                        cardButton.Card = null;
                                        cardButton.IsVisible = false;
                                    }
                                }
                            }

                            break;
                        }
                    }
                }

                return;
            }

            if ((undoCardButtonClicked != null) && (undoDealtCardClicked != null))
            {
                undoDealtCardClicked.PyramidRow = undoDealtCardClickedPyramidRow;
                undoDealtCardClicked.PyramidCardOriginalIndexInRow = undoDealtCardClickedPyramidCardOriginalIndexInRow;

                vm.DealtCards[undoDealtCardClickedPyramidRow][undoDealtCardClickedPyramidCardOriginalIndexInRow] =
                    undoDealtCardClicked;

                undoCardButtonClicked.Card = undoDealtCardClicked?.Card;
                undoCardButtonClicked.StackDetails = undoDealtCardClicked?.StackDetails;

                if (undoDealtCardClickedPyramidRow == 3)
                {
                    undoCardButtonClicked.IsVisible = true;
                }

                undoCardButtonClicked.RefreshVisuals();
            }

            if ((undoCardButtonAlreadySelected != null) && (undoDealtCardAlreadySelected != null))
            {
                undoDealtCardAlreadySelected.PyramidRow = undoDealtCardAlreadySelectedPyramidRow;
                undoDealtCardAlreadySelected.PyramidCardOriginalIndexInRow = undoDealtCardAlreadySelectedPyramidCardOriginalIndexInRow;

                vm.DealtCards[undoDealtCardAlreadySelectedPyramidRow][undoDealtCardAlreadySelectedPyramidCardOriginalIndexInRow] =
                    undoDealtCardAlreadySelected;

                undoCardButtonAlreadySelected.Card = undoDealtCardAlreadySelected?.Card;
                undoCardButtonAlreadySelected.StackDetails = undoDealtCardAlreadySelected?.StackDetails;

                if (undoDealtCardAlreadySelectedPyramidRow == 3)
                {
                    undoCardButtonAlreadySelected.IsVisible = true;
                }
            }
        }

        private void UndoPyramidMove()
        {
            // Has a card now been revealed? 
            if (undoPyramidDealtCardClicked != null)
            {
                SetOnTopStateFollowingMove(undoPyramidDealtCardClicked, false, true);
            }

            if (undoPyramidDealtCardAlreadySelected != null)
            {
                SetOnTopStateFollowingMove(undoPyramidDealtCardAlreadySelected, false, true);
            }

            if ((undoPyramidCardButtonKing != null) && (undoPyramidCardKing != null))
            {
                undoPyramidCardButtonKing.Card = undoPyramidCardKing;

                if (undoRemoveFromUpturnedPile)
                {
                    _deckUpturned.Add(undoPyramidCardKing);

                    CardDeckUpturned.Card = undoPyramidCardKing;
                }
                else
                {
                    if (CardDeckUpturned.Card == null)
                    {
                        _deckUpturned.Add(undoPyramidCardKing);
                    }
                    else
                    {
                        var upturnedCount = _deckUpturned.Count;
                        if (upturnedCount > 0)
                        {
                            _deckUpturned.Insert(upturnedCount - 1, undoPyramidCardKing);
                        }
                    }
                }

                SetUpturnedCardsVisuals();
            }

            if (undoPyramidCardObscuredHigher != null)
            {
                _deckUpturned.Add(undoPyramidCardObscuredHigher);

                // The upturned card will be added below.
                undoRemoveFromUpturnedPile = true;
            }

            if (undoPyramidCardUpturned != null)
            {
                undoPyramidCardButtonUpturned?.Card = undoPyramidCardUpturned;

                if (undoRemoveFromUpturnedPile)
                {
                    _deckUpturned.Add(undoPyramidCardUpturned);
                }
                else
                {
                    if (CardDeckUpturned.Card == null)
                    {
                        _deckUpturned.Add(undoPyramidCardUpturned);
                    }
                    else
                    {
                        var upturnedCount = _deckUpturned.Count;
                        if (upturnedCount > 0)
                        {
                            _deckUpturned.Insert(upturnedCount - 1, undoPyramidCardUpturned);
                        }
                    }
                }

                SetUpturnedCardsVisuals();
            }

            undoPyramidCardButtonClicked?.IsVisible = true;

            undoPyramidCardButtonAlreadySelected?.IsVisible = true;

            undoPyramidDealtCardClicked?.Card = undoPyramidCardButtonClicked?.Card;
            undoPyramidDealtCardAlreadySelected?.Card = undoPyramidCardButtonAlreadySelected?.Card;
        }

        private DealtCard? moveTriPeakCard = null;

        private void RememberTriPeaksStateForUndo(DealtCard? triPeakCard)
        {
            ClearUndoState();

            moveTriPeakCard = triPeakCard;

            SetUndoButtonState(true);
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
                        // Has a card now been revealed? 
                        SetOnTopStateFollowingMove(dealtCard, false, true);

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

                        //RefreshCardButtonMofNInRow(cardButtonClicked);
                    }
                }
            }

            moveTriPeakCard = null;
        }
    }
}