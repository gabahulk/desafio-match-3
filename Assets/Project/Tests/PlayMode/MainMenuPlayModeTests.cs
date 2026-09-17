using System.Collections;
using Gazeus.DesafioMatch3.Controllers;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Tests.PlayMode
{
    public sealed class MainMenuPlayModeTests
    {
        [UnityTest]
        public IEnumerator MainMenu_PlaysIntroBeforeEnablingPlay()
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            yield return null;

            GameObject canvas = GameObject.Find("Canvas");
            GameObject suitGroup = GameObject.Find("SuitGroup");
            GameObject joker = GameObject.Find("Joker");
            GameObject playButtonObject = GameObject.Find("PlayButton");
            Assert.That(canvas.GetComponent<MainMenuController>(), Is.Not.Null);
            Assert.That(canvas.GetComponent<MainMenuResponsiveController>(), Is.Not.Null);
            Assert.That(GameObject.Find("Background").GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(GameObject.Find("Title").GetComponent<TMP_Text>().text, Is.EqualTo("MATCH 3"));
            Assert.That(suitGroup.transform.Find("Heart"), Is.Not.Null);
            Assert.That(suitGroup.transform.Find("Diamond"), Is.Not.Null);
            Assert.That(suitGroup.transform.Find("Club"), Is.Not.Null);
            Assert.That(suitGroup.transform.Find("Spade"), Is.Not.Null);
            Assert.That(joker.GetComponent<Image>().sprite, Is.Not.Null);

            Button playButton = playButtonObject.GetComponent<Button>();
            Assert.That(playButton.interactable, Is.False);

            yield return new WaitForSecondsRealtime(1.2f);

            Assert.That(playButton.interactable, Is.True);
            Assert.That(playButtonObject.GetComponent<RectTransform>().rect.width, Is.EqualTo(420.0f));
            Assert.That(playButtonObject.transform.Find("Plaque").GetComponent<Image>().sprite, Is.Not.Null);
            Assert.That(playButtonObject.transform.Find("Text").GetComponent<TMP_Text>().text, Is.EqualTo("PLAY"));
        }
    }
}
