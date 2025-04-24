// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using Sa11ytaire4All.ViewModels;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sa11ytaire4All.Source
{
    public class CardIndices
    {
        public int pileIndex;
        public int cardIndex;
    }

    public partial class DealtCard : ObservableObject
    {
        [ObservableProperty, NotifyPropertyChangedFor(nameof(CurrentCardIndices))]
        private int currentCardIndexInDealtCardPile;

        [ObservableProperty, NotifyPropertyChangedFor(nameof(CurrentCardIndices))]
        private int currentDealtCardPileIndex;


        public CardIndices CurrentCardIndices
        {
            get
            {
                return new CardIndices()
                    {
                        pileIndex = CurrentDealtCardPileIndex,
                        cardIndex = CurrentCardIndexInDealtCardPile
                    };
            }
        }

        [ObservableProperty] private Card card;

        [ObservableProperty] private CardState cardState;

        // Barker Future: Remove FaceDown and replace with use of CardState only.
        [ObservableProperty, NotifyPropertyChangedFor(nameof(CardIsInAccessibleTree))]
        private bool faceDown;

        public string AccessibleNameWithoutSelectionAndMofN
        {
            get
            {
                string name = "";

                if (this.FaceDown)
                {
                    name = MainPage.MyGetString("FaceDown");
                }
                else if (this.Card.Rank != 0)
                {
                    name = this.Card.GetCardAccessibleName();
                }
                else
                {
                    name = MainPage.MyGetString("Empty") + " " +
                            MainPage.MyGetString("DealtCardPile");
                }

                return name;
            }
        }

        public string? AccessibleName
        {
            get
            {
                // If we want the card to be removed from the accessibility tree, it must have
                // no accessible name.
                if (!this.CardIsInAccessibleTree)
                {
                    return null;
                }

                string name;

                bool addMofN = true;

                if (this.FaceDown)
                {
                    // Always include the count of face-down cards in the first face-down card
                    // regardless of whether the face-down cards are merged.
                    name = (this.CurrentCardIndexInDealtCardPile == 0 ? 
                                this.CountFaceDownCardsInPile.ToString() + " " : "") + 
                            MainPage.MyGetString("FaceDown");
                }
                else if (this.Card.Rank != 0)
                {
                    name = this.Card.GetCardAccessibleName();

                    // TalkBack doesn't seem to include the CollectionView's selected state in its announcement,
                    // so include it explicitly here.
                    if (this.CardSelected)
                    {
                        name += ", " + MainPage.MyGetString("Selected");
                    }
                    else if (this.InSelectedSet)
                    {
                        name += ", " + MainPage.MyGetString("InSelectedSet");
                    }
                }
                else
                {
                    addMofN = false;

                    name = MainPage.MyGetString("Empty") + " " +
                            MainPage.MyGetString("DealtCardPile");
                }

                if (addMofN)
                {
                    // TalkBack doesn't seem to reliably announce the index of the card
                    // in the CollectionView, so include it explicitly here.

                    var collectionView = MainPage.FindCollectionView(this);
                    if (collectionView != null)
                    {
                        name += ", " + (this.CurrentCardIndexInDealtCardPile + 1).ToString() + " " +
                                 MainPage.MyGetString("Of") + " " + 
                                 collectionView.ItemsSource.Cast<DealtCard>().Count();
                    }
                }

                Debug.WriteLine("DealtCard AccessibleName: " + name);

                return name;
            }
        }

        // Barker Todo: Remove this once I know how to use multi-binding in XAML with an attached property.
        public bool CardIsInAccessibleTree
        {
            get
            {
                // Assume the card is accessible to a screen reader.
                var cardIsInAccessibleTree = true;

// Do not perform this on Windows, otherwise the merged face-down cards are still keyboard accessible but
// don't get a custom accessible name set. Instead they're left with the default class name-based names.
#if (IOS || Android)
                // Barker: Remove the use of the singleton here.
                if (MainPage.MainPageSingleton != null)
                {
                    var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
                    if (vm != null)
                    {
                        // If we've not merged the face-down cards, the card is accessible.
                        if (vm.MergeFaceDownCards)
                        {
                            // All face-up cards and the first face-down card is accessible.
                            cardIsInAccessibleTree = (!FaceDown || (this.CurrentCardIndexInDealtCardPile == 0));
                        }
                    }
                }
#endif

                return cardIsInAccessibleTree;
            }
        }

        [ObservableProperty] private bool isLastCardInPile;

        [ObservableProperty, NotifyPropertyChangedFor(nameof(AccessibleName))]
        private bool cardSelected;

        [ObservableProperty, NotifyPropertyChangedFor(nameof(AccessibleName))]
        private bool inSelectedSet;

        [ObservableProperty] private int countFaceDownCardsInPile;

        public void RefreshVisuals()
        {
            if (this.Card != null)
            {
                OnPropertyChanged("Card");
            }
        }

        public void RefreshAccessibleName()
        {
            if (this.Card != null)
            {
                OnPropertyChanged("AccessibleName");
            }
        }

        public void RefreshCardIsInAccessibleTree()
        {
            if (this.Card != null)
            {
                OnPropertyChanged("CardIsInAccessibleTree");
                OnPropertyChanged("AccessibleName");
            }
        }
    }
}
