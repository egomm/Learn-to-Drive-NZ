using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InformationManager : MonoBehaviour {
    // Function for loading the main menu scene
    public void MainMenu() {
        SceneManager.LoadScene("Main Menu");
    }
}
