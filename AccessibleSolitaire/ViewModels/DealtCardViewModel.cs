// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using Sa11ytaire4All.Source;
using System.Collections.ObjectModel;

namespace Sa11ytaire4All.ViewModels
{
    public class DealtCardViewModel : BindableObject
    {
        public DealtCardViewModel()
        {
            DealtCards = new ObservableCollection<DealtCard>[7];

            for (int i = 0; i < DealtCards.Count(); ++i)
            {
                DealtCards[i] = new ObservableCollection<DealtCard>();
            }
        }

        private ObservableCollection<DealtCard>[]? dealtCards = null;

        public ObservableCollection<DealtCard>[]? DealtCards
        {
            get => dealtCards;
            set
            {
                dealtCards = value;

                OnPropertyChanged("DealtCards");
            }
        }

        public ObservableCollection<DealtCard>? DealtCards1 { get => dealtCards?[0]; }
        public ObservableCollection<DealtCard>? DealtCards2 { get => dealtCards?[1]; }
        public ObservableCollection<DealtCard>? DealtCards3 { get => dealtCards?[2]; }
        public ObservableCollection<DealtCard>? DealtCards4 { get => dealtCards?[3]; }
        public ObservableCollection<DealtCard>? DealtCards5 { get => dealtCards?[4]; }
        public ObservableCollection<DealtCard>? DealtCards6 { get => dealtCards?[5]; }
        public ObservableCollection<DealtCard>? DealtCards7 { get => dealtCards?[6]; }

        public static readonly BindableProperty CardHeightProperty =
            BindableProperty.Create(nameof(CardHeight), typeof(double), typeof(MainPage));

        public double CardHeight
        {
            get => (double)GetValue(CardHeightProperty);
            set
            {
                SetValue(CardHeightProperty, value);

                this.OnPropertyChanged("CardHeight");
            }
        }

        public static readonly BindableProperty CardWidthProperty =
            BindableProperty.Create(nameof(CardWidth), typeof(double), typeof(MainPage));

        public double CardWidth
        {
            get => (double)GetValue(CardWidthProperty);
            set
            {
                SetValue(CardWidthProperty, value);

                this.OnPropertyChanged("CardWidth");
            }
        }

        public static readonly BindableProperty SuitColoursClubsProperty =
            BindableProperty.Create(nameof(SuitColoursClubs), typeof(Color), typeof(DealtCardViewModel));

        public Color SuitColoursClubs
        {
            get => (Color)GetValue(SuitColoursClubsProperty);
            set
            {
                SetValue(SuitColoursClubsProperty, value);

                this.OnPropertyChanged("SuitColoursClubs");
            }
        }

        public static readonly BindableProperty SuitColoursDiamondsProperty =
            BindableProperty.Create(nameof(SuitColoursDiamonds), typeof(Color), typeof(DealtCardViewModel));

        public Color SuitColoursDiamonds
        {
            get => (Color)GetValue(SuitColoursDiamondsProperty);
            set
            {
                SetValue(SuitColoursDiamondsProperty, value);

                this.OnPropertyChanged("SuitColoursDiamonds");
            }
        }

        public static readonly BindableProperty SuitColoursHeartsProperty =
            BindableProperty.Create(nameof(SuitColoursHearts), typeof(Color), typeof(DealtCardViewModel));

        public Color SuitColoursHearts
        {
            get => (Color)GetValue(SuitColoursHeartsProperty);
            set
            {
                SetValue(SuitColoursHeartsProperty, value);

                this.OnPropertyChanged("SuitColoursHearts");
            }
        }

        public static readonly BindableProperty SuitColoursSpadesProperty =
            BindableProperty.Create(nameof(SuitColoursSpades), typeof(Color), typeof(DealtCardViewModel));

        public Color SuitColoursSpades
        {
            get => (Color)GetValue(SuitColoursSpadesProperty);
            set
            {
                SetValue(SuitColoursSpadesProperty, value);

                this.OnPropertyChanged("SuitColoursSpades");
            }
        }


        public static readonly BindableProperty ZoomLevelProperty =
            BindableProperty.Create(nameof(ZoomLevel), typeof(int), typeof(DealtCardViewModel));

        public int ZoomLevel
        {
            get => (int)GetValue(ZoomLevelProperty);
            set
            {
                SetValue(ZoomLevelProperty, value);

                this.OnPropertyChanged("ZoomLevel");
            }
        }

        public static readonly BindableProperty CardBrightnessProperty =
            BindableProperty.Create(nameof(CardBrightness), typeof(int), typeof(DealtCardViewModel));

        public int CardBrightness
        {
            get => (int)GetValue(CardBrightnessProperty);
            set
            {
                SetValue(CardBrightnessProperty, value);

                this.OnPropertyChanged("CardBrightness");
            }
        }

        public static readonly BindableProperty ShowStateAnnouncementButtonProperty =
            BindableProperty.Create(nameof(ShowStateAnnouncementButton), typeof(bool), typeof(DealtCardViewModel));

        public bool ShowStateAnnouncementButton
        {
            get => (bool)GetValue(ShowStateAnnouncementButtonProperty);
            set
            {
                SetValue(ShowStateAnnouncementButtonProperty, value);

                this.OnPropertyChanged("ShowStateAnnouncementButton");
            }
        }

        public static readonly BindableProperty ShowZoomCardButtonProperty =
            BindableProperty.Create(nameof(ShowZoomCardButton), typeof(bool), typeof(DealtCardViewModel));

        public bool ShowZoomCardButton
        {
            get => (bool)GetValue(ShowZoomCardButtonProperty);
            set
            {
                SetValue(ShowZoomCardButtonProperty, value);

                this.OnPropertyChanged("ShowZoomCardButton");
            }
        }

        public static readonly BindableProperty HighlightSelectedCardSetProperty =
            BindableProperty.Create(nameof(HighlightSelectedCardSet), typeof(bool), typeof(DealtCardViewModel));

        public bool HighlightSelectedCardSet
        {
            get => (bool)GetValue(HighlightSelectedCardSetProperty);
            set
            {
                SetValue(HighlightSelectedCardSetProperty, value);

                this.OnPropertyChanged("HighlightSelectedCardSet");
            }
        }

        public static readonly BindableProperty MergeFaceDownCardsProperty =
            BindableProperty.Create(nameof(MergeFaceDownCards), typeof(bool), typeof(DealtCardViewModel));

        public bool MergeFaceDownCards
        {
            get => (bool)GetValue(MergeFaceDownCardsProperty);
            set
            {
                SetValue(MergeFaceDownCardsProperty, value);

                this.OnPropertyChanged("MergeFaceDownCards");
            }
        }

        public static readonly BindableProperty FlipGameLayoutHorizontallyProperty =
            BindableProperty.Create(nameof(FlipGameLayoutHorizontally), typeof(bool), typeof(DealtCardViewModel));

        public bool FlipGameLayoutHorizontally
        {
            get => (bool)GetValue(FlipGameLayoutHorizontallyProperty);
            set
            {
                SetValue(FlipGameLayoutHorizontallyProperty, value);

                this.OnPropertyChanged("FlipGameLayoutHorizontally");
            }
        }

        public static readonly BindableProperty ExtendDealtCardHitTargetProperty =
            BindableProperty.Create(nameof(ExtendDealtCardHitTarget), typeof(bool), typeof(DealtCardViewModel));

        public bool ExtendDealtCardHitTarget
        {
            get => (bool)GetValue(ExtendDealtCardHitTargetProperty);
            set
            {
                SetValue(ExtendDealtCardHitTargetProperty, value);

                this.OnPropertyChanged("ExtendDealtCardHitTarget");
            }
        }
    }
}
