using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public string LoadingScene;
    public string GameScene;

    public void Awake()
    {
        if(instance != null) Destroy(this);
        else instance = this;
    }

    public async void StartGame()
    {
        Scene thisScene = SceneManager.GetActiveScene();

        await SceneManager.LoadSceneAsync(LoadingScene);
        await SceneManager.LoadSceneAsync(GameScene);

        await SceneManager.UnloadSceneAsync(LoadingScene);
        await SceneManager.UnloadSceneAsync(thisScene);
    }

    public void OptionsMenu()
    {
        
    }

    public void ExitGame() => Application.Quit();
}
