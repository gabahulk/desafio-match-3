using System.Globalization;
using TMPro;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Views
{
    public sealed class ScoreView : MonoBehaviour
    {
        private const string ScorePrefix = "<color=#FFC96E><size=55%><cspace=8>SCORE</cspace></size></color>\n<size=130%>";
        [SerializeField] private TMP_Text _valueText;

        public void UpdateScore(int score)
        {
            _valueText.text = $"{ScorePrefix}{score.ToString("N0", CultureInfo.InvariantCulture)}</size>";
        }
    }
}
