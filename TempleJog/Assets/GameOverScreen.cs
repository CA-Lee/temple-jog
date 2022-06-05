using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameOverScreen : MonoBehaviour
{
    public Text pointsText;
    public void Setup(int score)
    {
        gameObject.SetActive(true);
        pointsText.text = "Scores: " + score.ToString();
    }
    public void RestartButton()
    {
        SceneManager.LoadScene("Game");
    }
    public void ExitButton()
    {
        SceneManager.LoadScene("StartMenu");  
    }
}
