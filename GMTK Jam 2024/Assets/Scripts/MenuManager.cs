using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public string OptionsScreen;
    public string LoadingScene;
    public string GameScene;

    public GameObject MenuButtons;
    public GameObject OptionsBackButton;

    public AudioMixer AudioMixer;

    public void Awake()
    {
        if(instance != null) Destroy(this);
        else instance = this;

        if(!PlayerPrefs.HasKey("SFXVolume")) PlayerPrefs.SetFloat("SFXVolume", 0);
        if(!PlayerPrefs.HasKey("MusicVolume")) PlayerPrefs.SetFloat("MusicVolume", 0);
    }

    public void Start()
    {
        if(OptionsBackButton.activeSelf) OptionsBackButton.SetActive(false);

        AudioMixer.SetFloat("SFXVolume", PlayerPrefs.GetFloat("SFXVolume"));
        AudioMixer.SetFloat("MusicVolume", PlayerPrefs.GetFloat("MusicVolume"));
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
        OptionsBackButton.SetActive(true);
    }

    public async void MainMenu()
    {
        await SceneManager.UnloadSceneAsync(OptionsScreen);
        MenuButtons.SetActive(true);
        OptionsBackButton.SetActive(false);
    }

    public void ExitGame() => Application.Quit();
}
