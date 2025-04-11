using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    PlayerController playerController;
    int PlayerID = 0;


    public void OnPlayerJoin()
    {
        playerController = FindAnyObjectByType<PlayerController>();
        playerController.PlayerID = PlayerID;
        GameManager.Instance.currentPlayers++;

        Debug.Log("Player " + PlayerID + " joined the game!");

        PlayerID++;
    }
}