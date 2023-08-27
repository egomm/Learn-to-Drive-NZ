using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame() {
        Debug.Log("Start Game button clicked.");
        SceneManager.LoadScene("Learn");
    }

    public void ShowInformation() {

    }
}