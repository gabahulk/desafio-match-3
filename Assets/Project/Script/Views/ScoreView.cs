using System.Globalization;
using TMPro;
using UnityEngine;

namespace Gazeus.DesafioMatch3.Views
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _valueText;

        public void UpdateScore(int score)
        {
            _valueText.text = score.ToString("N0", CultureInfo.InvariantCulture);
        }
    }
}
