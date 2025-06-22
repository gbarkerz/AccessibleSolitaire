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
        }

        [ObservableProperty] private ObservableCollection<DealtCard>[]? dealtCards;

        public ObservableCollection<DealtCard>? DealtCards1 { get => DealtCards?[0]; }
        public ObservableCollection<DealtCard>? DealtCards2 { get => DealtCards?[1]; }
        public ObservableCollection<DealtCard>? DealtCards3 { get => DealtCards?[2]; }
        public ObservableCollection<DealtCard>? DealtCards4 { get => DealtCards?[3]; }
        public ObservableCollection<DealtCard>? DealtCards5 { get => DealtCards?[4]; }
        public ObservableCollection<DealtCard>? DealtCards6 { get => DealtCards?[5]; }
        public ObservableCollection<DealtCard>? DealtCards7 { get => DealtCards?[6]; }

        [ObservableProperty] private double cardHeight;

        [ObservableProperty] private double cardWidth;

        [ObservableProperty] private Color? suitColoursClubs;

        [ObservableProperty] private Color? suitColoursDiamonds;

        [ObservableProperty] private Color? suitColoursHearts;

        [ObservableProperty] private Color? suitColoursSpades;

        [ObservableProperty] private bool showScreenReaderAnnouncementButtons;

        // Trial removal of this setting.
        //[ObservableProperty] private bool highlightSelectedCardSet;

        [ObservableProperty] private bool mergeFaceDownCards;

        [ObservableProperty] private bool flipGameLayoutHorizontally;

        [ObservableProperty] private bool extendDealtCardHitTarget;

        [ObservableProperty] private int longPressZoomDuration;
    }
}
