using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject menuPanel;
    public GameObject infoPanel;

    [Header("Music")]
    public AudioSource bgMusic;

    void Start()
    {
        menuPanel.SetActive(true);
        infoPanel.SetActive(false);

        if (bgMusic != null)
        {
            bgMusic.loop = true;
            bgMusic.Play();
        }
    }

    // ================= BUTTON EVENTS =================

    public void PlayGame()
    {
        SceneManager.LoadScene("Lore");   // rename to your game scene
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void OpenInfo()
    {
        menuPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        infoPanel.SetActive(false);
        menuPanel.SetActive(true);
    }
}
