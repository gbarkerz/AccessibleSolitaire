﻿// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Globalization;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;

namespace Sa11ytaire4All.Views
{
    // Barker Important: Some of the converters here don't actual convert anything.
    // Rather the exist to set properties based on the current orientation of the
    // device. At some point, replace these unusual converters with whatever the
    // correct way of setting orientation-specific properties is.

    public class IsToggledToBorderPaddingConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            var padding = 0;

#if WINDOWS
            padding = 4;
#endif

            return padding;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class IsButtonFocusedToOuterBorderBackgroundColor : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Avoid build warnings re: Application.Current being null.
            if (Application.Current == null)
            {
                return null;
            }

            if (values == null || (values.Length < 3))
            {
                return null;
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return null;
            }

            var isFocused = (bool)values[1];

            if (isFocused)
            {
                return (Application.Current.RequestedTheme != AppTheme.Dark ?
                            Colors.Black : Colors.White);
            }

            var card = values[2];

            // Not focused, and no card on CardButton.
            if (card != null)
            {
                return (Application.Current.RequestedTheme != AppTheme.Dark ?
                            Colors.White : Colors.Black);
            }

            var automationId = (string)values[0];

            var colour = Colors.Transparent;

            switch (automationId)
            {
                case "CardDeckUpturnedObscuredLower":
                case "CardDeckUpturnedObscuredHigher":
                case "CardDeckUpturned":

                    colour = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                Color.FromRgb(0xC3, 0xC3, 0xC3) : Color.FromRgb(0x88, 0x88, 0x88));
                    break;

                default:

                    colour = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                Colors.LightGreen : Colors.DarkGreen);
                    break;
            }

            // Barker Todo: Account for dark app theme here.
            return colour;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsButtonFocusedToInnerBorderBackgroundColor : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Avoid build warnings re: Application.Current being null.
            if (Application.Current == null)
            {
                return null;
            }

            if (values == null || (values.Length < 3))
            {
                return null;
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return null;
            }

            var isFocused = (bool)values[1];

            if (isFocused)
            {
                return (Application.Current.RequestedTheme != AppTheme.Dark ?
                            Colors.White : Colors.Black);
            }

            var card = values[2];

            // Not focused, and no card on CardButton.
            if (card != null)
            {
                return (Application.Current.RequestedTheme != AppTheme.Dark ?
                            Colors.White : Colors.Black);
            }

            var automationId = (string)values[0];

            var colour = Colors.Transparent;

            switch (automationId)
            {
                case "CardDeckUpturnedObscuredLower":
                case "CardDeckUpturnedObscuredHigher":
                case "CardDeckUpturned":

                    colour = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                Color.FromRgb(0xC3, 0xC3, 0xC3) : Color.FromRgb(0x88, 0x88, 0x88));
                    break;

                default:

                    colour = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                Colors.LightGreen : Colors.DarkGreen);
                    break;
            }

            // Barker Todo: Account for dark app theme here.
            return colour;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class LongPressZoomDurationToActualDuration : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            int timeout = (int)value;

            // Assume ten minutes is sufficient for the "Never" timeout.
            return (timeout > 0 ? timeout : 600000);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SuitSuitColoursToColor : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Avoid build warnings re: Application.Current being null.
            if (Application.Current == null)
            {
                return null;
            }

            if (values == null || (values.Length < 6))
            {
                return null;
            }

            // We must have at least one suit colour supplied.
            if ((values[2] == null) && (values[3] == null) &&
                (values[4] == null) && (values[5] == null))
            {
                return null;
            }

            Color? suitColor = null;

            // We were supplied with a card?
            if (values[0] == null)
            {
                // No Card supplied, so perhaps this is an empty target card pile.
                if (values[1] == null)
                {
                    // No AutomationId supplied.
                    return Colors.Transparent;
                }

                var automationId = values[1] as string;

                //Debug.WriteLine("SuitSuitColoursToColor: automationId " + automationId);

                switch (automationId)
                {
                    case "TargetPileC":
                        suitColor = (Color)values[2];
                        break;

                    case "TargetPileD":
                        suitColor = (Color)values[3];
                        break;

                    case "TargetPileH":
                        suitColor = (Color)values[4];
                        break;

                    case "TargetPileS":
                        suitColor = (Color)values[5];
                        break;

                    default:
                        break;
                }
            }
            else
            {
                var card = (Card)values[0];
                switch (card.Suit)
                {
                    case Suit.Clubs:
                        suitColor = (Color)values[2];
                        break;

                    case Suit.Diamonds:
                        suitColor = (Color)values[3];
                        break;

                    case Suit.Hearts:
                        suitColor = (Color)values[4];
                        break;

                    case Suit.Spades:
                        suitColor = (Color)values[5];
                        break;

                    default:
                        suitColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                            Colors.LightGrey : Colors.Grey);
                        break;
                }
            }

            return suitColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // DealtCardSuitSuitColoursToColor() has no supplied AutomationId.
    public class DealtCardSuitSuitColoursToColor : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Avoid build warnings re: Application.Current being null.
            if (Application.Current == null)
            {
                return null;
            }

            if (values == null || (values.Length < 5))
            {
                return null;
            }

            // We must have at least one suit colour supplied.
            if ((values[1] == null) && (values[2] == null) &&
                (values[3] == null) && (values[4] == null))
            {
                return null;
            }

            Color? suitColor = null;

            // We were supplied with a card?
            if (values[0] == null)
            {
                return null;
            }

            var card = (Card)values[0];
            switch (card.Suit)
            {
                case Suit.Clubs:
                    suitColor = (Color)values[1];
                    break;

                case Suit.Diamonds:
                    suitColor = (Color)values[2];
                    break;

                case Suit.Hearts:
                    suitColor = (Color)values[3];
                    break;

                case Suit.Spades:
                    suitColor = (Color)values[4];
                    break;

                default:
                    suitColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                        Colors.LightGrey : Colors.Grey);
                    break;
            }

            return suitColor;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardWidthToDealtCardPileCollectionViewWidthConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 2))
            {
                return 0;
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var cardWidth = (double)values[0];

            var scrollViewWidth = (double)values[1];

            if ((cardWidth <= 0) || (scrollViewWidth <= 0))
            {
                return 0;
            }

            var isPortrait = MainPage.IsPortrait();

            return isPortrait ? scrollViewWidth : cardWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterForCardImageHorizontalVerticalOptions : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 3))
            {
                return LayoutOptions.Start;
            }

            if ((values[0] == null) || (values[1] == null) || (values[2] == null))
            {
                return LayoutOptions.Start;
            }

            var IsLastCardInPile = (bool)values[0];
            var CardSelected = (bool)values[1];
            var extendDealtCardHitTarget = (bool)values[2];

            var isPortrait = MainPage.IsPortrait();

            var isHorizontalOption = (string?)parameter == "0";

            // By default, show the top left corner of the card.
            var option = LayoutOptions.Start;

            // If we're extending cards across the screen, then centre it horizontally.
            if (isPortrait && isHorizontalOption && IsLastCardInPile && extendDealtCardHitTarget)
            {
                option =  LayoutOptions.Center;
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FlipGameLayoutHorizontallyToFlowDirection : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            var flipGameLayoutHorizontally = (bool)value;

            return flipGameLayoutHorizontally ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ConverterForDealtCardLabelHorizontalOptions : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var verticalOptions = ((string?)parameter == "0");

            var options = LayoutOptions.Start;

            if (MainPage.IsPortrait())
            {
                if (!verticalOptions)
                {
                    options = LayoutOptions.Center;
                }
            }
            else
            {
                if (verticalOptions)
                {
                    options = LayoutOptions.Center;
                }
            }

            return options;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    // The toolkit's InvertedBoolConverter doesn't appear to be available on iOS, so include this instead.
    public class IsFaceDownToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            var isFaceDown = (bool)value;

            return !isFaceDown;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // This convert is for the background for the CardButton, not a dealt card.
    public class CardToBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return Colors.White;
            }

            // Note that the card can be null here, for example, for an empty target card pile.
            var card = (Card?)value;

            Color backgroundColor;

            if (Application.Current.RequestedTheme != AppTheme.Dark)
            {
                backgroundColor = (card == null ? Color.FromRgb(0xC0, 0xFF, 0xC0) : Colors.White);
            }
            else
            {
                backgroundColor = (card == null ? Colors.Black : Color.FromRgb(0x20, 0x20, 0x20));
            }

            return backgroundColor;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardWidthSelectedToCardImageWidth : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 2))
            {
                return 0;
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var cardSelected = (bool)values[0];
            var cardWidth = (double)values[1];

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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardHeightSelectedToCardImageHeight : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 2))
            {
                return 0;
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var cardSelected = (bool)values[0];
            var cardHeight = (double)values[1];

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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class ScrollViewHeightToDealtCardPileCollectionViewConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || (values.Length < 2))
            {
                return 0;
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return 0;
            }

            var scrollViewHeight = (double)values[0];
            if (scrollViewHeight <= 0)
            {
                return 0;
            }

            var cardHeight = (double)values[1];
            if (cardHeight <= 0)
            {
                return 0;
            }

            var isPortrait = MainPage.IsPortrait();

            return isPortrait ? cardHeight : scrollViewHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsObscuredCardToMargin : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Thickness();
            }

            var cardWidth = (double)value;

            return new Thickness(0, 0, -4 * cardWidth / 5, 0);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardHeightToLabelFontSizeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }

            // The label's font size must never be more than the card height.
            var cardHeight = (double)value;

            if (cardHeight <= 0)
            {
                return 0;
            }

#if WINDOWS
            cardHeight = (2 * cardHeight) / 3;
#endif
            return (cardHeight / 6) - 1;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsCardButtonCardToIsEnabled : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            var card = (Card)value;

            return (card != null);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsFaceDownToBackgroundColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return Colors.Transparent;
            }

            if (value == null)
            {
                return Colors.Transparent;
            }

            var isFaceDown = (bool)value;

            Color backgroundColor;

            if (isFaceDown)
            {
                backgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                    Color.FromRgb(0x0E, 0xD1, 0x45) : Color.FromRgb(0x0B, 0xA9, 0x38));

            }
            else
            {
                // The only time this colour shows is when the dealt card is selected. When
                // selected, the image on the card shrinks to reveal this background colour.
                backgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                    Colors.Purple : Colors.Yellow);
            }

            return backgroundColor;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InSelectedSetToBackgroundConverter : IValueConverter
    {
        private static readonly Color inSelectedSetLightColor = Color.FromRgb(0xEB, 0xDB, 0xFD);
        private static readonly Color inSelectedSetDarkColor = Color.FromRgb(0x30, 0x30, 0x00);

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return Colors.Transparent;
            }

            if (value == null)
            {
                return Colors.Transparent;
            }

            var inSelectedSet = (bool)value;

            Color? color = Colors.Transparent;

            if (Application.Current != null)
            {
                if (inSelectedSet)
                {
                    color = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                inSelectedSetLightColor : inSelectedSetDarkColor);
                }
                else
                {
                    color = (Application.Current.RequestedTheme != AppTheme.Dark ? Colors.White : Colors.Black);
                }
            }

            return color;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NextCardPileStateToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return null;
            }

            if (value == null)
            {
                return null;
            }

            NextCardPileState state = (NextCardPileState)value;

            string cardAsset;

            if (state != NextCardPileState.Active)
            {
                cardAsset = "EmptyDealtCardPile";
            }
            else
            {
                cardAsset = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                "cardback" : "darkcardback");
            }

            var imageFileName = cardAsset.ToLower() + ".png";

            var imageSource = ImageSource.FromFile(imageFileName);

            return imageSource;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NextCardPileStateToAccessibleName : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return NextCardPileState.Active;
            }

            NextCardPileState state = (NextCardPileState)value;

            var stateStringId = "NextCardPile_NextCard";

            if (state == NextCardPileState.Empty)
            {
                stateStringId = "NextCardPile_TurnOverCards";
            }
            else if (state == NextCardPileState.Finished)
            {
                stateStringId = "NextCardPile_FinishedCards";
            }

            return MainPage.MyGetString(stateStringId);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsToggledToMarginConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Thickness();
            }

            var isToggled = (bool)value;

            Thickness margin;

            if (isToggled && (parameter != null))
            {
                var paramValue = parameter.ToString();

#if WINDOWS
                margin = new Thickness(8);
#else
                // Note: We currently show the same margin sizes for the dealt cards and all other cards.
                if (MainPage.IsPortrait())
                {
                    margin = new Thickness(0, 8, 0, 8);
                }
                else
                {
                    margin = new Thickness(8, 0, 8, 0);
                }
#endif
            }
            else
            {
                margin = new Thickness();
            }

            return margin;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardWidthToHamburgerWidthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }

            var cardWidth = (double)value;

            return cardWidth / 3;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardWidthToTargetCardWidthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0;
            }

            var cardWidth = (double)value;

            return cardWidth - 3;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    
    public class CardDimensionsToHamburgerHeightConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return 0;
            }

            if ((values[0] == null) || (values[1] == null) || (values[2] == null))
            {
                return 0;
            }

            var cardWidth = (double)values[0];
            var cardHeight = (double)values[1];
            var ShowScreenReaderAnnouncementButtons = (bool)values[2];

            double buttonHeight = (cardWidth / 3);

            if (ShowScreenReaderAnnouncementButtons)
            {
                buttonHeight = Math.Min(buttonHeight, cardHeight / 3);
            }

            return 30; // buttonHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsToggledToBackgroundColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return null;
            }

            if (value == null)
            {
                return null;
            }

            var isToggled = (bool)value;

            Color backgroundColor;

            if (Application.Current.RequestedTheme != AppTheme.Dark)
            {
                backgroundColor = isToggled ? Colors.Purple : Colors.White;
            }
            else
            {
                backgroundColor = isToggled ? Colors.Yellow : Colors.Black;
            }

            return backgroundColor;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PictureCardToCardImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return null;
            }

            if (value == null)
            {
                return null;
            }

            if (MainPage.ShowRankSuitLarge)
            {
                return null;
            }

            Card card = (Card)value;

            string cardAsset;

            switch (card.Suit)
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

            switch (card.Rank)
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
                    cardAsset = "";
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

            if (Application.Current.RequestedTheme == AppTheme.Dark)
            {
                imageFileName = "dark" + imageFileName;
            }

            var imageSource = ImageSource.FromFile(imageFileName);

            return imageSource;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CardToCardPopupAccessibleName : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            Card card = (Card)value;

            return MainPage.MyGetString("Popup") + ", " + card.GetCardAccessibleName();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Barker Todo: CardToCardImageConverter() and PictureCardToCardImageConverter() in this file are only
    // used by the CardPopup. They should be removed to avoid duplication with the code in DealtCard.cs,
    // GetImageSourceFromDealtCard() and GetImageSourceFromPictureDealtCard().
    public class CardToCardImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return null;
            }

            if (value == null)
            {
                return null;
            }

            Card card = (Card)value;

            string cardAsset;

            bool largeVersionAvailable = true;

            switch (card.Suit)
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

            switch (card.Rank)
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

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // This sets the visibility of the dealt card pile Label showing the count of face-down cards in the pile.
    public class FaceDownCountLabelToIsVisibleConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
            {
                return false;
            }

            if ((values[0] == null) || (values[1] == null) || (values[2] == null))
            {
                return false;
            }

            var faceDown = (bool)values[0];
            var currentCardIndexInDealtCardPile = (int)values[1];
            var mergeFaceDownCards = (bool)values[2];

            return (mergeFaceDownCards && faceDown && (currentCardIndexInDealtCardPile == 0));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    
    public class MergedCardToIsVisibleConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isVisible = true;

#if ANDROID
            if (values == null || values.Length < 4)
            {
                return false;
            }

            if (values[0] == null)
            {
                return false;
            }

            var isLastCardInPile = (bool)values[0];

            if (values[1] == null)
            {
                return false;
            }

            var faceDown = (bool)values[1];

            if (values[2] == null)
            {
                return false;
            }

            var currentCardIndexInDealtCardPile = (int)values[2];

            if (values[3] == null)
            {
                return false;
            }

            var mergeFaceDownCards = (bool)values[3];

            if (mergeFaceDownCards && faceDown && !isLastCardInPile && (currentCardIndexInDealtCardPile > 0))
            {
                isVisible = false;
            }
#endif

            return isVisible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // This reduces the height of all merged face-down cards in a dealt card pile down to zero,
    // (except the bottom-most face-down card).
    public class MergedToHeightRequestConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 5)
            {
                return 0;
            }

            if ((values[0] == null) || (values[1] == null) || (values[2] == null) ||
                (values[3] == null) || (values[4] == null))
            {
                return 0;
            }

            var isLastCardInPile = (bool)values[0];
            var faceDown = (bool)values[1];
            var currentCardIndexInDealtCardPile = (int)values[2];
            var mergeFaceDownCards = (bool)values[3];
            var cardHeight = (double)values[4];

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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MergedToWidthRequestConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 8)
            {
                return 0;
            }

            if ((values[0] == null) || (values[1] == null) || (values[2] == null) ||
                (values[3] == null) || (values[4] == null) || (values[5] == null) ||
                (values[6] == null) || (values[7] == null))
            {
                return 0;
            }

            var isLastCardInPile = (bool)values[0];
            var faceDown = (bool)values[1];
            var currentCardIndexInDealtCardPile = (int)values[2];
            var mergeFaceDownCards = (bool)values[3];
            var cardWidth = (double)values[4];
            var scrollViewWidth = (double)values[5];
            var currentDealtCardPileIndex = (int)values[6];
            var mainPageBindingContext = values[7];

            if ((cardWidth <= 0) || (scrollViewWidth <= 0) ||
                (mainPageBindingContext == null) ||
                (currentDealtCardPileIndex < 0) || (currentDealtCardPileIndex > 6))
            {
                return 0;
            }

            var vm = mainPageBindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MergedToLabelPaddingConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new Thickness(0);
            }

            var cardHeight = (double)value;

            var padding = new Thickness(0);

#if ANDROID
            // Barker Todo: Understand why the adjustment is required to move the Label text up
            // on Android but not on iOS.

            // Barker Todo: Trying to set a negative top padding on Windows causes an exception,
            // so I need to figure out an alternative way to shift the displayed number us.

            var partiallyShownHeight = (cardHeight / 6) - 1;
            padding.Top = -partiallyShownHeight / 4;
#endif

            return padding;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}