using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour {
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    // Function for formatting time in seconds to minutes:seconds
    private string FormatTime(float time) {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Start is called before the first frame update
    void Start() {
        // Score text (the colour of this text is based on the player score)
        string scoreColour = "green";
        if (MoveCar.playerScore < 50) {
            scoreColour = "red";
        } else if (MoveCar.playerScore < 70) {
            scoreColour = "orange";
        } else if (MoveCar.playerScore < 90) {
            scoreColour = "yellow";
        }
        scoreText.text = $"Score: <color={scoreColour}>" + MoveCar.playerScore + "</color>";

        // Time text (the colour of this text is based on the elapsed time)
        string timeColour = "red";
        if (MoveCar.elapsedTime >= 120) {
            timeColour = "orange";
        } else if (MoveCar.elapsedTime >= 240) {
            timeColour = "yellow";
        } else if (MoveCar.elapsedTime >= 360) {
            timeColour = "green";
        }
        timerText.text = $"Time: <color={timeColour}>" + FormatTime(MoveCar.elapsedTime) + "</color>";
    }

    public void MainMenu() {
        // Load the scene of the main menu
        SceneManager.LoadScene("Main Menu");
    }
}
