using Microsoft.VisualBasic;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.DevTools.V124.IndexedDB;
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
        // Barker Todo: Figure out how to send keystrokes to the app. So far I've not been 
        // able to use FindUIElement() to find a higher level UI element in the app.
        //element.SendKeys("R");

        Task.Delay(2000).Wait();

        var element = FindUIElement("AppMenuButton");
        element.SendKeys("r");

        Task.Delay(2000).Wait();

        element = FindUIElement("PrimaryButton");
        element.SendKeys(Keys.Return);

        Task.Delay(5000).Wait();

        element = FindUIElement("CardDeckUpturned");

        Assert.That(element.Text, Is.EqualTo("No card"));

        App.GetScreenshot().SaveAsFile($"{nameof(RestartGameTest)}.png");
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
    public void MoveUpturnedCardToTargetCardPileTest()
    {
        var nextCardElement = FindUIElement("NextCardButton");

        var element = FindUIElement("CardDeckUpturned");

        var countTurns = 0;

        while (true)
        {
            var upturnedCardName = element.Text.ToLower();

            if (upturnedCardName.Contains("ace"))
            {
                element.Click();

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

                break;
            }

            nextCardElement.Click();

            ++countTurns;

            if (countTurns > 20)
            {
                break;
            }

            Task.Delay(1000).Wait();
        }
    }
}
