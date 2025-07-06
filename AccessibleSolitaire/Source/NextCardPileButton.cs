// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;

namespace Sa11ytaire4All.Source
{
    public enum NextCardPileState
    {
        Finished,
        Empty,
        Active
    }

    public class NextCardPileButton : ImageButton, INotifyPropertyChanged
    {
        public NextCardPileButton()
        {
        }

        private NextCardPileState state = NextCardPileState.Active;
        public NextCardPileState State
        {
            get
            {
                return this.state;
            }
            set
            {
                if (this.state != value)
                {
                    this.state = value;

                    this.OnPropertyChanged("State");
                }
            }
        }
    }
}
