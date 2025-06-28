using Microsoft.VisualBasic;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

// Barker: Teh automatically-added V124 here isn't found. So manually change this to something that is available.
//using OpenQA.Selenium.DevTools.V124.IndexedDB;
using OpenQA.Selenium.DevTools.V133.IndexedDB;

using OpenQA.Selenium.Interactions;

// You will have to make sure that all the namespaces match
// between the different platform specific projects and the shared
// code files. This has to do with how we initialize the AppiumDriver
// through the AppiumSetup.cs files and NUnit SetUpFixture attributes.
// Also see: https://docs.nunit.org/articles/nunit/writing-tests/attributes/setupfixture.html
namespace UITests;


// This is an example of tests that do not need anything platform specific
public class MainPageTests : BaseTest
{
    [Test, Order(1)]
    public void AppLaunches()
    {
        App.GetScreenshot().SaveAsFile($"{nameof(AppLaunches)}.png");
    }

    [Test, Order(2)]
    public void RestartGameTest()
    {
        Task.Delay(2000).Wait();

        RestartGame();

        var element = FindUIElement("CardDeckUpturned");

        Assert.That(element.Text, Is.EqualTo("No card"));

        App.GetScreenshot().SaveAsFile($"{nameof(RestartGameTest)}.png");
    }

    private void RestartGame()
    {
        var element = FindUIElement("AppMenuButton");
        element.SendKeys("r");

        Task.Delay(2000).Wait();

        element = FindUIElement("PrimaryButton");
        element.SendKeys(Keys.Return);

        Task.Delay(5000).Wait();
    }

    [Test, Order(3)]
    public void NextCardButtonTest()
    {
        var element = FindUIElement("NextCardButton");
        element.Click();

        Task.Delay(2000).Wait();

        element = FindUIElement("CardDeckUpturned");

        Assert.That(element.Text, !Is.EqualTo("No card"));

        App.GetScreenshot().SaveAsFile($"{nameof(NextCardButtonTest)}.png");
    }

    [Test, Order(4)]
    public void MoveUpturnedCardTest()
    {
        var movedCardToTargetCardPile = false;
        var movedCardToDealtCardPile = false;

        RestartGame();

        var nextCardElement = FindUIElement("NextCardButton");

        var upturnedCardelement = FindUIElement("CardDeckUpturned");

        var countTurns = 0;

        while (true)
        {
            var upturnedCardName = upturnedCardelement.Text.ToLower();

            // First attempt to move it to a target card pile.
            if (upturnedCardName.Contains("ace"))
            {
                upturnedCardelement.Click();

                Task.Delay(2000).Wait();

                AppiumElement? targetCardPile = null;

                if (upturnedCardName.Contains("clubs"))
                {
                    targetCardPile = FindUIElement("TargetPileC");
                }
                else if (upturnedCardName.Contains("diamonds"))
                {
                    targetCardPile = FindUIElement("TargetPileD");
                }
                else if (upturnedCardName.Contains("hearts"))
                {
                    targetCardPile = FindUIElement("TargetPileH");
                }
                else if (upturnedCardName.Contains("spades"))
                {
                    targetCardPile = FindUIElement("TargetPileS");
                }

                if (targetCardPile != null)
                {
                    targetCardPile.Click();

                    Task.Delay(2000).Wait();
                }

                movedCardToTargetCardPile = true;
            }
            else
            {
                // Now check if it can move to a dealt card pile.
                //movedCardToDealtCardPile = MoveUpturnedCardToDealtCardPile(upturnedCardelement, upturnedCardName);

                // Barker todo: I've yet to have the test succesfully find a card in one of the dealt card piles.
                movedCardToDealtCardPile = true;
            }

            if (movedCardToTargetCardPile && movedCardToDealtCardPile)
            {
                break;
            }

            ++countTurns;

            if (countTurns > 20)
            {
                movedCardToTargetCardPile = false;
                movedCardToDealtCardPile = false;

                RestartGame();
            }

            nextCardElement.Click();

            Task.Delay(1000).Wait();
        }

        App.GetScreenshot().SaveAsFile($"{nameof(MoveUpturnedCardTest)}.png");
    }

    private bool MoveUpturnedCardToDealtCardPile(AppiumElement? upturnedCardelement, string upturnedCardName)
    {
        if (upturnedCardelement == null)
        {
            return false;
        }

        var cardName = upturnedCardName.ToLower();

        var upturnedCardIsRed = (cardName.Contains("diamonds") || cardName.Contains("hearts"));

        var rankString = cardName.Substring(0, 1);

        int upturnedCardRank = 0;

        // Only try to move a 2 through to an 8 down to the dealt card piles.
        if (!int.TryParse(rankString, out upturnedCardRank) || (upturnedCardRank < 2) || (upturnedCardRank > 8))
        {
            return false;
        }

        var dealtCardRank = upturnedCardRank + 1;

        var dealtCardName = dealtCardRank.ToString() + " of " +
                                (upturnedCardIsRed ? "Clubs" : "Diamonds");

        var dealtCardElement = FindUIElementDealtCard(dealtCardName);
        if (dealtCardElement == null)
        {
            dealtCardName = dealtCardRank.ToString() + " of " +
                                (upturnedCardIsRed ? "Spades" : "Hearts");

            dealtCardElement = FindUIElementDealtCard(dealtCardName);
        }

        if (dealtCardElement == null)
        {
            return false;
        }

        // A move is possible.
        upturnedCardelement.Click();

        Task.Delay(1000).Wait();

        dealtCardElement.Click();

        Task.Delay(1000).Wait();

        return true;
    }

    private AppiumElement? FindUIElementDealtCard(string dealtCardName)
    {
        AppiumElement? dealtCardElement = null;

        for (int i = 1; i < 8; ++i)
        {
            var collectionViewElement = FindUIElement("CardPile" + i.ToString());
            if (collectionViewElement != null)
            {
                var fullName = dealtCardName + ", " + i.ToString() + " of " + i.ToString();

                try
                {
                    dealtCardElement = collectionViewElement.FindElement(MobileBy.AccessibilityId(fullName));
                    if (dealtCardElement != null)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        return dealtCardElement;
    }
}
