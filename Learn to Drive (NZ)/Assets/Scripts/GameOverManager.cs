using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour {
    public TextMeshProUGUI scoreText;
    // Start is called before the first frame update
    void Start() {
        string colour = "green";
        if (MoveCar.playerScore < 50) {
            colour = "red";
        } else if (MoveCar.playerScore < 70) {
            colour = "orange";
        } else if (MoveCar.playerScore < 90) {
            colour = "yellow";
        }
        scoreText.text = $"Score: <color={colour}>" + MoveCar.playerScore + "</color>";
    }

    public void MainMenu() {
        SceneManager.LoadScene("Main Menu");
    }
}
