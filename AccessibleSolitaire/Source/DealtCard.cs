﻿// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Sa11ytaire4All.ViewModels;
using System.Diagnostics;

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
        public partial int CurrentCardIndexInDealtCardPile { get; set; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(CurrentCardIndices))]
        public partial int CurrentDealtCardPileIndex { get; set; }

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

        [ObservableProperty] 
        public partial Card? Card { get; set; }

        [ObservableProperty] 
        public partial CardState CardState { get; set; }

        public static readonly BindableProperty DealtCardTintColourProperty =
            BindableProperty.Create(nameof(DealtCardTintColour), typeof(Color), typeof(DealtCard));

        public Color? DealtCardTintColour
        {
            get => GetDealtCardColourTint();
        }

        // Barker Todo: The app has 3 version of this method, one for each of the 
        // dealt cards, card buttons, and card popup. Consolidate all these into
        // a single place.
        private Color? GetDealtCardColourTint()
        {
            Color? suitColor = null;

            if (this.Card == null)
            {
                return null;
            }

            if (MainPage.MainPageSingleton == null)
            {
                return null;
            }

            var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return null;
            }

            switch (this.Card.Suit)
            {
                case Suit.Clubs:
                    suitColor = vm.SuitColoursClubs;
                    break;

                case Suit.Diamonds:
                    suitColor = vm.SuitColoursDiamonds;
                    break;

                case Suit.Hearts:
                    suitColor = vm.SuitColoursHearts;
                    break;

                case Suit.Spades:
                    suitColor = vm.SuitColoursSpades;
                    break;

                default:
                    if (Application.Current != null)
                    {
                        suitColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                            Colors.LightGrey : Colors.Grey);
                    }

                    break;
            }

            return suitColor;
        }

        // Barker Future: Remove FaceDown and replace with use of CardState only.
        [ObservableProperty, NotifyPropertyChangedFor(nameof(CardIsInAccessibleTree))]
        public partial bool FaceDown { get; set; }

        public string AccessibleNameWithoutSelectionAndMofN
        {
            get
            {
                string name = "";

                if (this.FaceDown)
                {
                    name = MainPage.MyGetString("FaceDown");
                }
                else if ((this.Card != null) && (this.Card.Rank != 0))
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
                else if ((this.Card != null) && (this.Card.Rank != 0))
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

#if ANDROID || WINDOWS
                // (July 2025) TalkBack and NVDA now automatically includes the m of n in its announcement 
                // despite never doing so before. So do not manually add the m of n ourselves here on Android.
                addMofN = false;
#endif

                if (addMofN)
                {
                    // TalkBack doesn't seem to reliably announce the index of the card
                    // in the CollectionView, so include it explicitly here.

                    var collectionView = MainPage.FindCollectionView(this);
                    if (collectionView != null)
                    {
                        name += ", " + (this.CurrentCardIndexInDealtCardPile + 1).ToString() + " " +
                                 MainPage.MyGetString("Of") + // Has a trailing space. 
                                 collectionView.ItemsSource.Cast<DealtCard>().Count();
                    }
                }

                //Debug.WriteLine("DealtCard AccessibleName: " + name);

                return name;
            }
        }

        public string? AccessibleHint
        {
            get
            {
                if (MainPage.MainPageSingleton != null)
                {
                    // Check that the setting for adding the hint is on.
                    var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
                    if ((vm == null) || (!vm.AddHintToTopmostCard))
                    {
                        return null;
                    }
                }

                string? hint = null;

                // If we want the card to be removed from the accessibility tree, it must have
                // no accessible hint.
                if (this.CardIsInAccessibleTree)
                {
                    // Only add a hint to the topmost card as that must be face-up unless its an empty placeholder.
                    if (!this.FaceDown && this.IsLastCardInPile && (this.CardState != CardState.KingPlaceHolder))
                    {
                        var collectionView = MainPage.FindCollectionView(this);
                        if (collectionView != null)
                        {
                            var items = collectionView.ItemsSource;
                            foreach (var item in items)
                            {
                                var dealtCard = item as DealtCard;
                                if ((dealtCard != null) && (dealtCard.Card != null))
                                {
                                    if (!dealtCard.FaceDown)
                                    {
                                        if (dealtCard.IsLastCardInPile)
                                        {
                                            hint = MainPage.MyGetString("NoMoreFaceupCards");
                                        }
                                        else
                                        {
                                            hint = MainPage.MyGetString("HintBottommostFaceupCard") + " " +
                                                        dealtCard.Card.GetCardAccessibleName();
                                        }

                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                return hint;
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

        [ObservableProperty] 
        public partial bool IsLastCardInPile { get; set; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(AccessibleName))]
        public partial bool CardSelected { get; set; }

        public ImageSource? FaceupDealtCardImageSource
        {
            get
            {
                ImageSource? imageSource = null;

                if (!this.FaceDown)
                {
                    imageSource = GetImageSourceFromDealtCard();
                }

                return imageSource;
            }
        }

        private ImageSource? GetImageSourceFromDealtCard()
        {
            if (Application.Current == null)
            {
                return null;
            }

            if (Card == null)
            {
                return null;
            }

            string cardAsset;

            bool largeVersionAvailable = true;

            switch (Card.Suit)
            {
                case Suit.Clubs:
                    cardAsset = "Clubs";
                    break;

                case Suit.Diamonds:
                    cardAsset = "Diamonds";
                    break;

                case Suit.Hearts:
                    cardAsset = "Hearts";
                    break;

                case Suit.Spades:
                    cardAsset = "Spades";
                    break;

                default:
                    cardAsset = "";
                    break;
            }

            switch (Card.Rank)
            {
                case 1:
                    cardAsset += "1";
                    break;

                case 2:
                    cardAsset += "2";
                    break;

                case 3:
                    cardAsset += "3";
                    break;

                case 4:
                    cardAsset += "4";
                    break;

                case 5:
                    cardAsset += "5";
                    break;

                case 6:
                    cardAsset += "6";
                    break;

                case 7:
                    cardAsset += "7";
                    break;

                case 8:
                    cardAsset += "8";
                    break;

                case 9:
                    cardAsset += "9";
                    break;

                case 10:
                    cardAsset += "10";
                    break;

                case 11:
                    cardAsset += "11";
                    break;

                case 12:
                    cardAsset += "12";
                    break;

                case 13:
                    cardAsset += "13";
                    break;

                default:
                    cardAsset = "EmptyDealtCardPile";

                    // There is no large version of the empty dealy card image.
                    largeVersionAvailable = false;

                    break;
            }

            if (string.IsNullOrEmpty(cardAsset))
            {
                return null;
            }

            var imageFileName = cardAsset.ToLower();

            // Many of the images in the project are .svg files. They get converted to .png by MAUI.
            // While Android can reference the original .svg here, iOS cannot. So reference instead
            // the .png equivalent, as both Android and iOS are happy with that.
            imageFileName += ".png";

            if (largeVersionAvailable && MainPage.ShowRankSuitLarge)
            {
                imageFileName = "large" + imageFileName;
            }

            if (Application.Current.RequestedTheme == AppTheme.Dark)
            {
                imageFileName = "dark" + imageFileName;
            }

            var imageSource = ImageSource.FromFile(imageFileName);

            return imageSource;
        }

        public ImageSource? FaceupPictureDealtCardImageSource
        {
            get
            {
                ImageSource? imageSource = null;

                if (!this.FaceDown)
                {
                    imageSource = GetImageSourceFromPictureDealtCard();
                }

                return imageSource;
            }
        }

        private ImageSource? GetImageSourceFromPictureDealtCard()
        {
            if (Application.Current == null)
            {
                return null;
            }

            if (Card == null)
            {
                return null;
            }

            if (MainPage.ShowRankSuitLarge)
            {
                return null;
            }

            string cardAsset;

            switch (Card.Suit)
            {
                case Suit.Clubs:
                case Suit.Spades:
                    cardAsset = "black";
                    break;

                case Suit.Diamonds:
                case Suit.Hearts:
                    cardAsset = "red";
                    break;

                default:
                    cardAsset = "";
                    break;
            }

            switch (Card.Rank)
            {
                case 11:
                    cardAsset += "jack";
                    break;

                case 12:
                    cardAsset += "queen";
                    break;

                case 13:
                    cardAsset += "king";
                    break;

                default:
                    return null;
            }

            if (string.IsNullOrEmpty(cardAsset))
            {
                return null;
            }

            var imageFileName = cardAsset.ToLower();

            // Many of the images in the project are .svg files. They get converted to .png by MAUI.
            // While Android can reference the original .svg here, iOS cannot. So reference instead
            // the .png equivalent, as both Android and iOS are happy with that.
            imageFileName += ".png";

            if (Application.Current.RequestedTheme == AppTheme.Dark)
            {
                imageFileName = "dark" + imageFileName;
            }

            var imageSource = ImageSource.FromFile(imageFileName);

            return imageSource;
        }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(AccessibleName))]
        public partial bool InSelectedSet { get; set; }

        [ObservableProperty] 
        public partial int CountFaceDownCardsInPile { get; set; }

        public void RefreshVisuals()
        {
            if (this.Card != null)
            {
                OnPropertyChanged("Card");
                
                OnPropertyChanged("DealtCardTintColour");

                // Barker Note: Ok, here's the deal. Originally the two Images inside the DealtCard were bound
                // to the Card, with converters to generate the appropriate ImageSource from the Card. This worked
                // fine, but meant that all DealtCards had their Source set, even those that were face down.
                // Sentry reported hangs which manifested as crashes when rapidly and repeatedly rotating 
                // an iPad, apparently related to all the work being done to resize all the ImageSources.
                // One step to reduce the likelihood of that hang (and a good change anyway) is to not set 
                // up the DealtCard Images' Sources until the card is face up. The first attempt to achieve
                // this involved replacing the Images' Source Binding from only the Card, to multibinding 
                // the Card and FaceDown. While this worked to show the card image, the image didn't get 
                // resized when the card got selected. (The image shrinks a little to account for the large
                // selection borders.) That resizing still didn't work regardless of the raised property
                // changes for the Card and FaceDown properties at various places, (including here).

                // So, following all that, two new properties were added to the DealtCard, these being:
                // FaceupDealtCardImageSource and FaceupPictureDealtCardImageSource, and each of those
                // uses single binding to the DealtCards' Source. With that in place, when the visuals
                // for a dealt card changes on Windows, RefreshVisuals() can be called, and when the 
                // property changes are raised, the cad image is resized as required.

                OnPropertyChanged("FaceupDealtCardImageSource");
                OnPropertyChanged("FaceupPictureDealtCardImageSource");
            }
        }

        public void RefreshAccessibleName()
        {
            if (this.Card != null)
            {
                OnPropertyChanged("AccessibleName");
                OnPropertyChanged("AccessibleHint");
            }
        }

        public void RefreshCardIsInAccessibleTree()
        {
            if (this.Card != null)
            {
                OnPropertyChanged("CardIsInAccessibleTree");
                OnPropertyChanged("AccessibleName");
                OnPropertyChanged("AccessibleHint");
            }
        }
    }
}
