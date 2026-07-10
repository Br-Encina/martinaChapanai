using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Image optionsMenu;
    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OpenOptionsMenu()
    {
        optionsMenu.enabled = true;
    }

    public void CloseOptionsMenu()
    {
        optionsMenu.enabled = false;
    }
    
    public void QuitGame()
    {
        Application.Quit(); 
    }
}
