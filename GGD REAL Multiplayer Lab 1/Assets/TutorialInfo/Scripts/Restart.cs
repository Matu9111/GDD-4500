using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public void OnButtonClicked()
    {
        SceneManager.LoadScene("Lobby");
    }
}
