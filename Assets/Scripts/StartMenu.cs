// AI-assisted: written by Claude (Anthropic) at the student's request.
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public Text highScoreText;
    public Text bestTimeText;

    void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        highScoreText.text = "High Score: " + highScore.ToString("000000");
        bestTimeText.text = "Best Time: " + GameManager.FormatTime(bestTime);
    }
}
