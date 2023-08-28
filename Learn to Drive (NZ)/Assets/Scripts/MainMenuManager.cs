using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    public void StartGame() {
        SceneManager.LoadScene("Learn");
    }

    public void ShowInformation() {
        SceneManager.LoadScene("Information");
    }
}