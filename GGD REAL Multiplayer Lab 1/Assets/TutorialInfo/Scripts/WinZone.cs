using UnityEngine;
using GDD4500.LAB01;

public class WinZone : MonoBehaviour
{
    // check if a player enters the end zone
    public bool playerReached = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Made it");
            playerReached = true;
            WinZoneManager.Instance.CheckWin();
        }
    }

}
