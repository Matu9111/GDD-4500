using UnityEngine;
using GDD4500.LAB01;
using UnityEngine.SceneManagement;

public class WinZoneManager : MonoBehaviour
{
    [SerializeField] private WinZone winZone1;
    [SerializeField] private WinZone winZone2;

    public static WinZoneManager Instance;

    private void Awake()
    {
        Instance = this;
    }


    public void CheckWin()
    {
        if (winZone1.playerReached && winZone2.playerReached)
        {
            PlayerManager.Instance.ReturnToLobby("Win");

        }
    }

}
