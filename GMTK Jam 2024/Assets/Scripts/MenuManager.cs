using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public string OptionsScreen;
    public string LoadingScene;
    public string GameScene;

    public GameObject MenuButtons;

    public void Awake()
    {
        if(instance != null) Destroy(this);
        else instance = this;
    }

    public async void StartGame()
    {
        Scene thisScene = SceneManager.GetActiveScene();

        await SceneManager.LoadSceneAsync(LoadingScene, LoadSceneMode.Additive);
        await SceneManager.LoadSceneAsync(GameScene, LoadSceneMode.Additive);
        await SceneManager.UnloadSceneAsync(LoadingScene);
        await SceneManager.UnloadSceneAsync(thisScene);
    }

    public async void OptionsMenu()
    {
        await SceneManager.LoadSceneAsync(OptionsScreen, LoadSceneMode.Additive);
        MenuButtons.SetActive(false);
    }

    public void ExitGame() => Application.Quit();
}
