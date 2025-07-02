// Copyright(c) Guy Barker. All rights reserved.
// Licensed under the MIT License.

namespace Sa11ytaire4All.Source
{
    // A Card is the simplest representation of a card, containing only its suit, rank, and
    // the accessible name of the card.
    public sealed class Card
    {
        public Suit Suit { get; set; }
        public int Rank { get; set; }

        public string GetCardAccessibleName()
        {
            string rank;

            if (Suit == Suit.NoSuit)
            {
                return MainPage.MyGetString("Empty");
            }
            else
            {
                var stringOfResourceId = "NameCardOf";

                switch (Rank)
                {
                    case 1:
                        {
                            rank = MainPage.MyGetString("Ace");
                            break;
                        }
                    case 11:
                        {
                            rank = MainPage.MyGetString("Jack");
                            break;
                        }
                    case 12:
                        {
                            rank = MainPage.MyGetString("Queen");
                            break;
                        }
                    case 13:
                        {
                            rank = MainPage.MyGetString("King");
                            break;
                        }
                    default:
                        {
                            stringOfResourceId = "Of";

                            rank = MainPage.MyGetString(Rank.ToString());
                            break;
                        }
                }

                string ofString = MainPage.MyGetString(stringOfResourceId);

                string formattedString = "{0}" + // Rank.
                                            " " + 
                                            "{1}" + // Optional "of". (Has a trailing space if not empty string.)
                                            "{2}";

                string suitString;

                switch (Suit)
                {
                    case Suit.Clubs:
                        suitString = MainPage.MyGetString("Clubs");
                        break;
                    case Suit.Diamonds:
                        suitString = MainPage.MyGetString("Diamonds");
                        break;
                    case Suit.Hearts:
                        suitString = MainPage.MyGetString("Hearts");
                        break;
                    default:
                        suitString = MainPage.MyGetString("Spades");
                        break;
                }

                return string.Format(formattedString, rank, ofString, suitString);
            }
        }
    }
}
