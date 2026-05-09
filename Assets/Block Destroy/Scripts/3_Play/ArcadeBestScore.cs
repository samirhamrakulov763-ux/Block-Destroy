using UnityEngine;
using TMPro;

public class ArcadeBestScore : MonoBehaviour
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
        // Всегда используем BestScoreArcade для Arcade режима
        int bestScore = GameData.BestScoreArcade;
        bestScoreText.text = Utility.ChangeThousandsSeparator(bestScore);
    }
}
