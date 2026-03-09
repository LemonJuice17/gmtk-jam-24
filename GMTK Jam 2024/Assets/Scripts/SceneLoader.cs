using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R)) { ReloadScene(); }
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
