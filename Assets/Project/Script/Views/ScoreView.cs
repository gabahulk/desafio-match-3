using UnityEngine;
using UnityEngine.UI;

namespace Gazeus.DesafioMatch3.Views
{
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private Text _scoreText;

        public void UpdateScore(int score)
        {
            _scoreText.text = $"SCORE\n{score}";
        }
    }
}
