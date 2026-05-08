using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void LoadLevel(string id)
    {
        SceneManager.LoadScene(id);
    }

}
