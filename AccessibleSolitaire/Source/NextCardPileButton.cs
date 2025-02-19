// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

using System.ComponentModel;

namespace Sa11ytaire4All.Source
{
    public class NextCardPileButton : ImageButton, INotifyPropertyChanged
    {
        public NextCardPileButton()
        {
        }

        private bool isEmpty = false;
        public bool IsEmpty
        {
            get
            {
                return this.isEmpty;
            }
            set
            {
                this.isEmpty = value;

                this.OnPropertyChanged("IsEmpty");
            }
        }
    }
}
