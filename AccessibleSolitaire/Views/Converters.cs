// Copyright(c) Guy Barker. All rights reserved.
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

    public class FlipGameLayoutHorizontallyToFlowDirection : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return FlowDirection.LeftToRight;
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
    public class CardToBackgroundConverter : IMultiValueConverter
    {
        private static readonly Color isToggledLightColor = Color.FromRgb(0xEB, 0xDB, 0xFD);
        private static readonly Color isToggledDarkColor = Color.FromRgb(0x30, 0x30, 0x00);

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return Colors.Transparent;
            }

            if (values == null || values.Length < 2)
            {
                return Colors.Transparent;
            }

            if (values[0] == null)
            {
                return Colors.Transparent;
            }

            var isToggled = (bool)values[0];

            // Note that the card can be null here, for example, for an empty target card pile.
            var card = (Card?)values[1];

            Color backgroundColor;

            if (Application.Current.RequestedTheme != AppTheme.Dark)
            {
                var cardBackground = (isToggled ? isToggledLightColor : Colors.White);

                backgroundColor = (card == null ? Color.FromRgb(0xC0, 0xFF, 0xC0) : cardBackground);
            }
            else
            {
                var cardBackground = (isToggled ? isToggledDarkColor : Color.FromRgb(0x20, 0x20, 0x20));

                backgroundColor = (card == null ? Colors.Black : cardBackground);
            }

            return backgroundColor;
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

    public class InSelectedSetToBackgroundConverter : IMultiValueConverter
    {
        private static readonly Color inSelectedSetLightColor = Color.FromRgb(0xEB, 0xDB, 0xFD);
        private static readonly Color inSelectedSetDarkColor = Color.FromRgb(0x30, 0x30, 0x00);

        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Application.Current == null)
            {
                return Colors.Transparent;
            }

            if (values == null || values.Length < 2)
            {
                return Colors.Transparent;
            }

            if ((values[0] == null) || (values[1] == null))
            {
                return Colors.Transparent;
            }

            var inSelectedSet = (bool)values[0];
            var cardSelected = (bool)values[1];

            Color? color = Colors.Transparent;

            if (Application.Current != null)
            {
                if (cardSelected || inSelectedSet)
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
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