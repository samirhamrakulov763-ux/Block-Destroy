using UnityEngine;
using TMPro;

public class BestScore : MonoBehaviour
{
    public TextMeshProUGUI bestScoreText;

    void Start()
    {
        UpdateBestScoreDisplay();
    }

    void Update()
    {
        // Обновляем отображение при изменении счета
        if (PlayManager.Instance != null && bestScoreText != null)
        {
            UpdateBestScoreDisplay();
        }
    }

    public void UpdateBestScoreDisplay()
    {
        // Используем обычный BestScore для обычного режима
        int bestScore = GameData.BestScore;
        bestScoreText.text = Utility.ChangeThousandsSeparator(bestScore);
    }
}