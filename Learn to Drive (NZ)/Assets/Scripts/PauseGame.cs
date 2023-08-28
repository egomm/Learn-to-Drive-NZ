using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour {
    public GameObject pausePanel;
    public void PauseLearning() {
        pausePanel.SetActive(true);
        // Freeze the game
        Time.timeScale = 0;
    }

    public void ResumeLearning() {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

    public void EndLearning() {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        SceneManager.LoadScene("Game Over");
    }
}
