// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using Sa11ytaire4All.ViewModels;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Sa11ytaire4All.Source
{
    public class CardIndices
    {
        public int pileIndex;
        public int cardIndex;
    }

    public partial class DealtCard : ObservableObject
    {
        [JsonIgnore]
        public int PyramidRow { get; set; }
        [JsonIgnore]
        public int PyramidCardOriginalIndexInRow { get; set; }
        [JsonIgnore]
        public int PyramidCardCurrentIndexInRow { get; set; }
        [JsonIgnore]
        public int PyramidCardCurrentCountOfCardsOnRow { get; set; }

        // We do persist the next few...
        public bool Open{ get; set; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(CurrentCardIndices))]
        public partial int CurrentCardIndexInDealtCardPile { get; set; }

        [ObservableProperty, NotifyPropertyChangedFor(nameof(CurrentCardIndices))]
        public partial int CurrentDealtCardPileIndex { get; set; }

        [JsonIgnore]
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

        // *** Persist these... ***
        [ObservableProperty]
        public partial Card? Card { get; set; }

        [ObservableProperty]
        public partial CardState CardState { get; set; }


        // Barker Future: Remove FaceDown and replace with use of CardState only.
        [ObservableProperty, NotifyPropertyChangedFor(nameof(CardIsInAccessibleTree))]
        public partial bool FaceDown { get; set; }

        public static readonly BindableProperty DealtCardTintColourProperty =
            BindableProperty.Create(nameof(DealtCardTintColour), typeof(Color), typeof(DealtCard));

        [JsonIgnore]
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

        public string AccessibleNameWithoutSelectionAndMofN
        {
            get
            {
                var name = "";

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

        [JsonIgnore]
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

                var name = "";

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

        [JsonIgnore]
        public string? AccessibleHint
        {
            get
            {
                if (MainPage.currentGameType == SolitaireGameType.Pyramid)
                {
                    return "";
                }

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
        [JsonIgnore]
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

        [JsonIgnore]
        public double DealtCardWidth
        {
            get => GetDealtCardWidth();
        }

        private double GetDealtCardWidth()
        {
            if (MainPage.MainPageSingleton == null)
            {
                return 0;
            }

            var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return 0;
            }

            var isLastCardInPile = this.IsLastCardInPile;
            var faceDown = this.FaceDown;
            var currentCardIndexInDealtCardPile = this.CurrentCardIndexInDealtCardPile;
            var currentDealtCardPileIndex = this.CurrentDealtCardPileIndex;

            var mergeFaceDownCards = vm.MergeFaceDownCards;
            var cardWidth = vm.CardWidth;

            var scrollViewWidth = MainPage.MainPageSingleton.GetCardPileGridWidth();

            if ((cardWidth <= 0) || (scrollViewWidth <= 0) ||
                (currentDealtCardPileIndex < 0) || (currentDealtCardPileIndex > 6))
            {
                return 0;
            }

            var extendDealtCardHitTarget = vm.ExtendDealtCardHitTarget;

            var itemsSource = vm.DealtCards[currentDealtCardPileIndex];

            var width = cardWidth;

            var isPortrait = MainPage.IsPortrait();

            // In landscape orientation, all dealt cards have the same width.
            if (!isPortrait)
            {
                return width;
            }

            var partiallyShownWidth = (width / 4) - 1;

            // The last card in the pile is always full width.
            if (!isLastCardInPile)
            {
                // Any other face-up cards are always partially shown.
                if (!faceDown)
                {
                    width = partiallyShownWidth;
                }
                else
                {
                    // The card is face-down. If it's the first card in the pile,
                    // it's always partially shown.
                    if (currentCardIndexInDealtCardPile == 0)
                    {
                        width = partiallyShownWidth;
                    }
                    else
                    {
                        // Are we merging all other face-down cards?
                        if (mergeFaceDownCards)
                        {
                            // .NET 9 assumes that if a cell is zero-height, that's unintentional 
                            // and takes action which means nothing gets rendered. So give a merged
                            // card a height of 1. This might actually be useful to users, as it 
                            // means at a glance a pile with many merged cards seems to give the
                            // merged card a shadow.

                            // Note: On Android we can remove the shadow by setting the height to be 
                            // (say) 0.1. But on iOS this leads to the height of the bound UI element
                            // sometimes ending up being set to -1, and the whole CollectionView gets
                            // a height of zero and vanishes. So stick with a height of 1 here.
                            width = 1;
                        }
                        else
                        {
                            // We're not merging, so partially shown the card.
                            width = partiallyShownWidth;
                        }
                    }
                }
            }
            else
            {
                // This is the topmost card in the dealt card pile. Should we extend the hit target 
                // for the dealt card? (This setting only applies at 100% zoom level.)
                if (extendDealtCardHitTarget)
                {
                    // Start with the card almost reaching the far side of the screen.
                    width = scrollViewWidth - 20;

                    // If there are no other cards in the pile, there's nothing more to be done.
                    if (currentCardIndexInDealtCardPile > 0)
                    {
                        double totalWidthOfOtherCards = 0;

                        var lowerCardInPile = true;

                        var cardIndexInPile = 0;

                        // Reduce the width to account for all other cards in the pile.
                        var items = itemsSource;
                        foreach (var item in items)
                        {
                            var dealtCard = item as DealtCard;
                            if (dealtCard != null)
                            {
                                // If we've reached the topmost card, there's no more to be done.
                                if (cardIndexInPile == currentCardIndexInDealtCardPile)
                                {
                                    break;
                                }

                                // The lowest card in the pile never has a 1-pixel width.
                                if (lowerCardInPile)
                                {
                                    lowerCardInPile = false;

                                    totalWidthOfOtherCards = partiallyShownWidth;
                                }
                                else
                                {
                                    totalWidthOfOtherCards += mergeFaceDownCards ?
                                                                (dealtCard.FaceDown ? 1 : partiallyShownWidth) :
                                                                partiallyShownWidth;
                                }

                                ++cardIndexInPile;
                            }
                        }

                        // The topmost card must never be narrower than the regular card width.
                        width = Math.Max(cardWidth, width - totalWidthOfOtherCards);
                    }
                }
            }

            return width;
        }

        [JsonIgnore]
        public double FaceDownCountLabelSize
        {
            get => GetFaceDownCountLabelSize();
        }

        private double GetFaceDownCountLabelSize()
        {
            if (MainPage.MainPageSingleton == null)
            {
                return 0;
            }

            var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return 0;
            }

            // The label's font size must never be more than the card height.
            var cardHeight = vm.CardHeight;

            if (cardHeight <= 0)
            {
                return 0;
            }

#if WINDOWS
            cardHeight = (2 * cardHeight) / 3;
#endif
            return (cardHeight / 6) - 1;
        }

        [JsonIgnore]
        public bool FaceDownCountLabelIsVisible
        {
            get => GetFaceDownCountLabelIsVisible();
        }

        private bool GetFaceDownCountLabelIsVisible()
        {
            if (MainPage.MainPageSingleton == null)
            {
                return false;
            }

            var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return false;
            }

            var faceDown = this.FaceDown;
            var currentCardIndexInDealtCardPile = this.CurrentCardIndexInDealtCardPile;
            var mergeFaceDownCards = vm.MergeFaceDownCards;

            return (mergeFaceDownCards && faceDown && (currentCardIndexInDealtCardPile == 0));
        }

        [JsonIgnore]
        public LayoutOptions DealtCardImageHorizontalOptions
        {
            get => GetDealtCardImageHorizontalVerticalOptions(true);
        }

        [JsonIgnore]
        public LayoutOptions DealtCardImageVerticalOptions
        {
            get => GetDealtCardImageHorizontalVerticalOptions(false);
        }

        private LayoutOptions GetDealtCardImageHorizontalVerticalOptions(bool isHorizontalOption)
        {
            if (MainPage.MainPageSingleton == null)
            {
                return LayoutOptions.Start;
            }

            var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return LayoutOptions.Start;
            }

            var IsLastCardInPile = this.IsLastCardInPile;
            var CardSelected = this.CardSelected;
            var extendDealtCardHitTarget = vm.ExtendDealtCardHitTarget;

            var isPortrait = MainPage.IsPortrait();

            // By default, show the top left corner of the card.
            var option = LayoutOptions.Start;

            // If we're extending cards across the screen, then centre it horizontally.
            if (isPortrait && isHorizontalOption && IsLastCardInPile && extendDealtCardHitTarget)
            {
                option = LayoutOptions.Center;
            }

            if (CardSelected)
            {
                if ((isPortrait && !isHorizontalOption) ||
                    (!isPortrait && isHorizontalOption))
                {
                    option = LayoutOptions.Center;
                }

#if WINDOWS
                if (IsLastCardInPile || isHorizontalOption)
                {
                    option = LayoutOptions.Center;
                }
#endif
            }

            return option;
        }

        [JsonIgnore]
        public double DealtCardImageWidth
        {
            get => GetDealtCardImageWidth();
        }

        private double GetDealtCardImageWidth()
        {
            if (MainPage.MainPageSingleton == null)
            {
                return 0;
            }

            var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return 0;
            }

            var cardSelected = this.CardSelected;

            var cardWidth = vm.CardWidth;

            if (cardWidth <= 0)
            {
                return 0;
            }

            var isPortrait = MainPage.IsPortrait();

            cardWidth -= 2; // '2' here to account for the margins between CollectionViews.

#if WINDOWS
            if (cardSelected)
            {
                cardWidth -= 20;
            }
#else
            if (cardSelected && !isPortrait)
            {
                cardWidth -= 12;
            }
#endif

            return cardWidth;
        }

        [JsonIgnore]
        public double DealtCardImageHeight
        {
            get => GetDealtCardImageHeight();
        }

        private double GetDealtCardImageHeight()
        {
            if (MainPage.MainPageSingleton == null)
            {
                return 0;
            }

            var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return 0;
            }

            var cardSelected = this.CardSelected;

            var cardHeight = vm.CardHeight;

            if (cardHeight <= 0)
            {
                return 0;
            }

            if (cardSelected)
            {
#if WINDOWS
                cardHeight -= 20;
#else
                // When a dealt card is selected in Portrait, wide horizontal lines appear at the top and bottom
                // of the card. This is achieved by reducing the height of the card, and centring the image in its
                // container. By doing this, the BackgroundColor of a containing element is revealed above and 
                // below the card.
                var isPortrait = MainPage.IsPortrait();
                if (isPortrait)
                {
                    cardHeight -= 16;
                }
#endif
            }

            return cardHeight;
        }

        [JsonIgnore]
        public double DealtCardHeight
        {
            get => GetDealtCardHeight();
        }

        private double GetDealtCardHeight()
        {
            if (MainPage.MainPageSingleton == null)
            {
                return 0;
            }

            var vm = MainPage.MainPageSingleton.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return 0;
            }

            var isLastCardInPile = this.IsLastCardInPile;
            var faceDown = this.FaceDown;
            var currentCardIndexInDealtCardPile = this.CurrentCardIndexInDealtCardPile;
            var mergeFaceDownCards = vm.MergeFaceDownCards;
            var cardHeight = vm.CardHeight;

            if (cardHeight <= 0)
            {
                return 0;
            }

            var height = (double)cardHeight;

            var isPortrait = MainPage.IsPortrait();

            // In portrait orientation, all dealt cards have the same height.
            if (isPortrait)
            {
                return height;
            }

            var partiallyShownHeight = (height / 6) - 1;

            // The last card in the pile is always full height.
            if (!isLastCardInPile)
            {
                // Any other face-up cards are always partially shown.
                if (!faceDown)
                {
                    height = partiallyShownHeight;
                }
                else
                {
                    // The card is face-down. If it's the first card in the pile,
                    // it's always partially shown.
                    if (currentCardIndexInDealtCardPile == 0)
                    {
                        height = partiallyShownHeight;
                    }
                    else
                    {
                        // Are we merging all other face-down cards?
                        if (mergeFaceDownCards)
                        {
                            // .NET 9 assumes that if a cell is zero-height, that's unintentional 
                            // and takes action which means nothing gets rendered. So give a merged
                            // card a height of 1. This might actually be useful to users, as it 
                            // means at a glance a pile with many merged cards seems to give the
                            // merged card a shadow.

                            // Note: On Android we can remove the shadow by setting the height to be 
                            // (say) 0.1. But on iOS this leads to the height of the bound UI element
                            // sometimes ending up being set to -1, and the whole CollectionView gets
                            // a height of zero and vanishes. So stick with a height of 1 here.
                            height = 1;
                        }
                        else
                        {
                            // We're not merging, so partially shown the card.
                            height = partiallyShownHeight;
                        }
                    }
                }
            }

            return height;
        }

        [JsonIgnore]
        [ObservableProperty,
            NotifyPropertyChangedFor(nameof(DealtCardWidth)),
            NotifyPropertyChangedFor(nameof(DealtCardHeight)),
            NotifyPropertyChangedFor(nameof(DealtCardImageWidth)),
            NotifyPropertyChangedFor(nameof(DealtCardImageHeight))]
        public partial bool IsLastCardInPile { get; set; }

        [JsonIgnore]
        [ObservableProperty,
            NotifyPropertyChangedFor(nameof(DealtCardWidth)),
            NotifyPropertyChangedFor(nameof(DealtCardHeight)),
            NotifyPropertyChangedFor(nameof(DealtCardImageWidth)),
            NotifyPropertyChangedFor(nameof(DealtCardImageHeight)),
            NotifyPropertyChangedFor(nameof(DealtCardImageHorizontalOptions)),
            NotifyPropertyChangedFor(nameof(DealtCardImageVerticalOptions)),
            NotifyPropertyChangedFor(nameof(AccessibleName))]
        public partial bool CardSelected { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonIgnore]
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

                OnPropertyChanged("DealtCardWidth");
                OnPropertyChanged("DealtCardHeight");

                OnPropertyChanged("DealtCardImageWidth");
                OnPropertyChanged("DealtCardImageHeight");

                OnPropertyChanged("DealtCardImageHorizontalOptions");
                OnPropertyChanged("DealtCardImageVerticalOptions");

                OnPropertyChanged("FaceDownCountLabelIsVisible");
                OnPropertyChanged("FaceDownCountLabelSize");
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
