using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour {
    public GameObject pausePanel;

    // Function for pausing the game 
    public void PauseLearning() {
        // Display the pause panel
        pausePanel.SetActive(true);
        // Freeze the game
        Time.timeScale = 0;
    }

    // Function for resuming the game 
    public void ResumeLearning() {
        // Unfreeze the game
        Time.timeScale = 1; 
        pausePanel.SetActive(false);
    }

    // Function for ending the game
    public void EndLearning() {
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        SceneManager.LoadScene("Game Over");
    }
}
