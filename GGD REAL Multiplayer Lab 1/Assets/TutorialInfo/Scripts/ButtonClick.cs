using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    public void OnButtonClicked()
    {
        Application.Quit();
        Debug.Log("Application is quitting.");
    }

}
