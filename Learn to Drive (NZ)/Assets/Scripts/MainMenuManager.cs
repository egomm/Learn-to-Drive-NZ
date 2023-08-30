using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    // Function for loading the learn scene 
    public void StartGame() {
        SceneManager.LoadScene("Learn");
    }

    // Function for loading the information scene
    public void ShowInformation() {
        SceneManager.LoadScene("Information");
    }
}