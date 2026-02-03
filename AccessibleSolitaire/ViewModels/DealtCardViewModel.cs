// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using Sa11ytaire4All.Source;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace Sa11ytaire4All.ViewModels
{
    public partial class DealtCardViewModel : ObservableObject
    {
        public DealtCardViewModel()
        {
            DealtCards = new ObservableCollection<DealtCard>[13];

            for (int i = 0; i < DealtCards.Count(); ++i)
            {
                DealtCards[i] = new ObservableCollection<DealtCard>();
            }

            NoScreenOrientationChangeWarning = "";

            CardButtonsHeadingState = false;
        }

        public ObservableCollection<DealtCard>? DealtCards1 { get => DealtCards?[0]; }
        public ObservableCollection<DealtCard>? DealtCards2 { get => DealtCards?[1]; }
        public ObservableCollection<DealtCard>? DealtCards3 { get => DealtCards?[2]; }
        public ObservableCollection<DealtCard>? DealtCards4 { get => DealtCards?[3]; }
        public ObservableCollection<DealtCard>? DealtCards5 { get => DealtCards?[4]; }
        public ObservableCollection<DealtCard>? DealtCards6 { get => DealtCards?[5]; }
        public ObservableCollection<DealtCard>? DealtCards7 { get => DealtCards?[6]; }
        public ObservableCollection<DealtCard>? DealtCards8 { get => DealtCards?[7]; }
        public ObservableCollection<DealtCard>? DealtCards9 { get => DealtCards?[8]; }
        public ObservableCollection<DealtCard>? DealtCards10 { get => DealtCards?[9]; }
        public ObservableCollection<DealtCard>? DealtCards11 { get => DealtCards?[10]; }
        public ObservableCollection<DealtCard>? DealtCards12 { get => DealtCards?[11]; }
        public ObservableCollection<DealtCard>? DealtCards13 { get => DealtCards?[12]; }

        [ObservableProperty] 
        public partial ObservableCollection<DealtCard>[]? DealtCards { get; set; }

        private double cardWidth;
        public double CardWidth
        {
            get
            {
                return cardWidth;
            }
            set
            {
                if (cardWidth != value)
                {
                    cardWidth = value;

                    OnPropertyChanged("CardWidth");
                    OnPropertyChanged("DealtCardPileWidth");
                }
            }
        }

        private double cardHeight;
        public double CardHeight
        {
            get
            {
                return cardHeight;
            }
            set
            {
                if (cardHeight != value)
                {
                    cardHeight = value;

                    OnPropertyChanged("CardHeight");
                    OnPropertyChanged("DealtCardPileHeight");
                }
            }
        }

        [ObservableProperty]
        public partial Color? SuitColoursClubs { get; set; }

        [ObservableProperty]
        public partial Color? SuitColoursDiamonds { get; set; }

        [ObservableProperty]
        public partial Color? SuitColoursHearts { get; set; }

        [ObservableProperty]
        public partial Color? SuitColoursSpades { get; set; }

        [ObservableProperty]
        public partial bool ShowScreenReaderAnnouncementButtons { get; set; }

        [ObservableProperty]
        public partial bool MergeFaceDownCards { get; set; }

        [ObservableProperty]
        public partial bool FlipGameLayoutHorizontally { get; set; }

        [ObservableProperty]
        public partial bool ExtendDealtCardHitTarget { get; set; }

        [ObservableProperty]
        public partial bool CardButtonsHeadingState { get; set; }

        [ObservableProperty]
        public partial bool AddHintToTopmostCard { get; set; }

        [ObservableProperty]
        public partial int LongPressZoomDuration { get; set; }

        [ObservableProperty]
        public partial bool GamePausedKlondike { get; set; }

        [ObservableProperty]
        public partial bool GamePausedBakersdozen { get; set; }

        [ObservableProperty]
        public partial bool GamePausedPyramid { get; set; }

        [ObservableProperty]
        public partial bool GamePausedTripeaks { get; set; }

        public double DealtCardPileWidth 
        { 
            get
            {
                var dealtCardPileWidth = this.CardWidth;

                if (MainPage.IsPortrait() && (MainPage.MainPageSingleton != null))
                {
                    dealtCardPileWidth = MainPage.MainPageSingleton.GetCardPileGridWidth();
                }

                return dealtCardPileWidth;
            }
        }

        public double DealtCardPileHeight 
        { 
            get
            {
                var dealtCardPileHeight = this.CardHeight;

                if (!MainPage.IsPortrait() && (MainPage.MainPageSingleton != null))
                {
                    dealtCardPileHeight = MainPage.MainPageSingleton.GetCardPileGridHeight();
                }

                return dealtCardPileHeight;
            }
        }

        [ObservableProperty]
        public partial string NoScreenOrientationChangeWarning { get; set; }

        private SolitaireGameType currentGameType;
        public SolitaireGameType CurrentGameType
        {
            get
            {
                return currentGameType;
            }
            set
            {
                if (currentGameType != value)
                {
                    currentGameType = value;

                    OnPropertyChanged("CurrentGameType");
                    OnPropertyChanged("CardWidth");
                }
            }
        }
    }
}
