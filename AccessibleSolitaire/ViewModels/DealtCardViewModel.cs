// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using Sa11ytaire4All.Source;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sa11ytaire4All.ViewModels
{
    public partial class DealtCardViewModel : ObservableObject
    {
        public DealtCardViewModel()
        {
            DealtCards = new ObservableCollection<DealtCard>[7];

            for (int i = 0; i < DealtCards.Count(); ++i)
            {
                DealtCards[i] = new ObservableCollection<DealtCard>();
            }

            NoScreenOrientationChangeWarning = "";
        }

        public ObservableCollection<DealtCard>? DealtCards1 { get => DealtCards?[0]; }
        public ObservableCollection<DealtCard>? DealtCards2 { get => DealtCards?[1]; }
        public ObservableCollection<DealtCard>? DealtCards3 { get => DealtCards?[2]; }
        public ObservableCollection<DealtCard>? DealtCards4 { get => DealtCards?[3]; }
        public ObservableCollection<DealtCard>? DealtCards5 { get => DealtCards?[4]; }
        public ObservableCollection<DealtCard>? DealtCards6 { get => DealtCards?[5]; }
        public ObservableCollection<DealtCard>? DealtCards7 { get => DealtCards?[6]; }

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
        public partial bool AddHintToTopmostCard { get; set; }

        [ObservableProperty]
        public partial int LongPressZoomDuration { get; set; }

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
    }
}
