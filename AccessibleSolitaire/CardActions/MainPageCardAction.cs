// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using Sa11ytaire4All.Source;
using Sa11ytaire4All.Views;
using System.Collections.ObjectModel;

namespace Sa11ytaire4All
{
    public sealed partial class MainPage : ContentPage
    {
        private void ClearAllSelections(bool includeUpturnedCard)
        {
            ClearCardButtonSelections(includeUpturnedCard);

            ClearDealtCardPileSelections();
        }

        private void ClearDealtCardPileSelections()
        {
            if (currentGameType == SolitaireGameType.Klondike)
            {
                for (int i = 0; i < cCardPiles; i++)
                {
                    var list = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));

                    DeselectAllCardsFromDealtCardPile(list);
                }
            }
            else if (currentGameType == SolitaireGameType.Pyramid)
            {
                ClearPyramidCardsSelection();
            }
        }

        private DealtCard? GetSelectedDealtCard(CollectionView? ignoreList, 
                                                out CollectionView? listAlreadySelected,
                                                out int listAlreadySelectedIndex)
        {
            listAlreadySelected = null;
            listAlreadySelectedIndex = 0;

            DealtCard? selectedCard = null;

            // Is any card selected in a CardPile list?
            for (int i = 0; i < cCardPiles; i++)
            {
                var list = (CollectionView?)CardPileGrid.FindByName("CardPile" + (i + 1));
                if (list != null)
                {
                    var items = list.ItemsSource;
                    foreach (var item in items)
                    {
                        var dealtCard = item as DealtCard;
                        if (dealtCard != null)
                        {
                            if (dealtCard.CardSelected)
                            {
                                selectedCard = dealtCard;

                                listAlreadySelected = list;
                                listAlreadySelectedIndex = i;

                                break;
                            }
                        }
                    }

                    if (selectedCard != null)
                    {
                        break;
                    }
                }
            }

            return selectedCard;
        }

        private void AddEmptyCardToCollectionView(ObservableCollection<DealtCard> cardList, int cardPileIndex)
        {
            DealtCard emptyCard = new DealtCard();

            emptyCard.Card = new Card();
            emptyCard.Card.Suit = Suit.NoSuit;
            emptyCard.Card.Rank = 0;
            emptyCard.CardState = CardState.KingPlaceHolder;
            emptyCard.IsLastCardInPile = true;

            // The card pile indices are always zero-based.
            emptyCard.CurrentDealtCardPileIndex = cardPileIndex;
            emptyCard.CurrentCardIndexInDealtCardPile = 0;

            cardList.Add(emptyCard);
        }

        private bool GameOver()
        {
            var gameIsOver = true;

            if (currentGameType == SolitaireGameType.Klondike)
            {
                // We've moved a card to the TargetPile. Now let's see if the game is over.
                for (int i = 0; i < cTargetPiles; i++)
                {
                    if (_targetPiles[i].Count != 13)
                    {
                        gameIsOver = false;
                    }
                }
            }
            else if (currentGameType == SolitaireGameType.Pyramid)
            {
                var gridCards = CardPileGridPyramid.Children;
                gameIsOver = (gridCards[0].Visibility != Visibility.Visible);
            }
            else if (currentGameType == SolitaireGameType.Tripeaks)
            {
                var gridCards = CardPileGridPyramid.Children;

                for (var i = 0; i < 3; ++i)
                {
                    if (gridCards[i].Visibility == Visibility.Visible)
                    {
                        gameIsOver = false;

                        break;
                    }
                }
            }

            return gameIsOver;
        }
    }
}