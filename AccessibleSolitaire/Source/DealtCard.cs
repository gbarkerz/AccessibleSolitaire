// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using Sa11ytaire4All.ViewModels;
using System.Diagnostics;

namespace Sa11ytaire4All.Source
{
    public class CardIndices
    {
        public int pileIndex;
        public int cardIndex;
    }

    public class DealtCard : BindableObject
    {
        public static readonly BindableProperty CurrentCardIndicesProperty =
            BindableProperty.Create(nameof(CurrentCardIndices), typeof(CardIndices), typeof(DealtCard));

        public static readonly BindableProperty CurrentCardIndexInDealtCardPileProperty =
            BindableProperty.Create(nameof(CurrentCardIndexInDealtCardPile), typeof(int), typeof(DealtCard));

        public static readonly BindableProperty CurrentDealtCardPileIndexProperty =
            BindableProperty.Create(nameof(CurrentDealtCardPileIndex), typeof(int), typeof(DealtCard));

        public static readonly BindableProperty CardTintColourProperty =
            BindableProperty.Create(nameof(CardTintColour), typeof(Color), typeof(DealtCard));

        public Color CardTintColour
        {
            get 
            {
                Color suitColour;

                switch (this.Card.Suit)
                {
                    case Suit.Clubs:
                        suitColour = MainPage.SuitColorsClubs;
                        break;

                    case Suit.Diamonds:
                        suitColour = MainPage.SuitColorsDiamonds;
                        break;

                    case Suit.Hearts:
                        suitColour = MainPage.SuitColorsHearts;
                        break;

                    default: // Suit.Spades:
                        suitColour = MainPage.SuitColorsSpades;
                        break;
                }

                return suitColour; 
            }
        }


        public int CurrentCardIndexInDealtCardPile 
        {
            get 
            { 
                return (int)GetValue(CurrentCardIndexInDealtCardPileProperty); 
            }
            set
            {
                if (CurrentCardIndexInDealtCardPile != value)
                {
                    SetValue(CurrentCardIndexInDealtCardPileProperty, value);

                    OnPropertyChanged("CurrentCardIndexInDealtCardPile");
                    OnPropertyChanged("CurrentCardIndices");
                }
            }
        }

        public int CurrentDealtCardPileIndex
        {
            get
            {
                return (int)GetValue(CurrentDealtCardPileIndexProperty);
            }
            set
            {
                if (CurrentDealtCardPileIndex != value)
                {
                    SetValue(CurrentDealtCardPileIndexProperty, value);

                    OnPropertyChanged("CurrentDealtCardPileIndex");
                    OnPropertyChanged("CurrentCardIndices");
                }
            }
        }

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

        public static readonly BindableProperty CardProperty =
            BindableProperty.Create(nameof(Card), typeof(Card), typeof(DealtCard));

        public static readonly BindableProperty CardStateProperty =
            BindableProperty.Create(nameof(CardState), typeof(CardState), typeof(DealtCard));

        public static readonly BindableProperty FaceDownProperty =
            BindableProperty.Create(nameof(FaceDown), typeof(bool), typeof(DealtCard));

        public static readonly BindableProperty AccessibleNameProperty =
            BindableProperty.Create(nameof(AccessibleName), typeof(string), typeof(DealtCard));

        public static readonly BindableProperty IsLastCardInPileProperty =
            BindableProperty.Create(nameof(IsLastCardInPile), typeof(bool), typeof(DealtCard));

        public static readonly BindableProperty CardSelectedProperty =
            BindableProperty.Create(nameof(CardSelected), typeof(bool), typeof(DealtCard));

        public static readonly BindableProperty InSelectedSetProperty =
            BindableProperty.Create(nameof(InSelectedSet), typeof(bool), typeof(DealtCard));

        public static readonly BindableProperty CountFaceDownCardsInPileProperty =
            BindableProperty.Create(nameof(CountFaceDownCardsInPile), typeof(int), typeof(DealtCard));

        public Card Card
        {
            get { return (Card)GetValue(CardProperty); }
            set
            {
                if (Card != value)
                {
                    SetValue(CardProperty, value);

                    OnPropertyChanged("Card");
                }
            }
        }

        public CardState CardState
        {
            get { return (CardState)GetValue(CardStateProperty); }
            set
            {
                if (CardState != value)
                {
                    SetValue(CardStateProperty, value);

                    OnPropertyChanged("CardState");
                }
            }
        }

        // Barker Future: Remove FaceDown and replace with use of CardState only.
        public bool FaceDown
        {
            get { return (bool)GetValue(FaceDownProperty); }
            set
            {
                if (FaceDown != value)
                {
                    SetValue(FaceDownProperty, value);

                    OnPropertyChanged("FaceDown");
                    
                    // This is required to force a refresh of the height of the card.
                    OnPropertyChanged("CardIsInAccessibleTree");
                }
            }
        }

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

                return cardIsInAccessibleTree;
            }
        }

        public bool IsLastCardInPile
        {
            get { return (bool)GetValue(IsLastCardInPileProperty); }
            set
            {
                if (IsLastCardInPile != value)
                {
                    SetValue(IsLastCardInPileProperty, value);

                    OnPropertyChanged("IsLastCardInPile");
                }
            }
        }

        public bool CardSelected
        {
            get { return (bool)GetValue(CardSelectedProperty); }
            set
            {
                if (CardSelected != value)
                {
                    SetValue(CardSelectedProperty, value);

                    OnPropertyChanged("CardSelected");

                    // If a card becomes selected or unselected, its accessible name changes.
                    OnPropertyChanged("AccessibleName");
                }
            }
        }

        public bool InSelectedSet
        {
            get { return (bool)GetValue(InSelectedSetProperty); }
            set
            {
                if (InSelectedSet != value)
                {
                    SetValue(InSelectedSetProperty, value);

                    OnPropertyChanged("InSelectedSet");

                    // If a card becomes part of a selected or unselected set, its accessible name changes.
                    OnPropertyChanged("AccessibleName");
                }
            }
        }

        public int CountFaceDownCardsInPile
        {
            get { return (int)GetValue(CountFaceDownCardsInPileProperty); }
            set
            {
                if (CountFaceDownCardsInPile != value)
                {
                    SetValue(CountFaceDownCardsInPileProperty, value);

                    OnPropertyChanged("CountFaceDownCardsInPile");
                }
            }
        }

        public void RefreshVisuals()
        {
            if (this.Card != null)
            {
                OnPropertyChanged("Card");

                OnPropertyChanged("CardTintColour");
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
