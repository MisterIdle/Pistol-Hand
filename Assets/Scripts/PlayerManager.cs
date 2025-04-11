using UnityEngine;

public class PlayersManager : MonoBehaviour
{
    PlayerController playerController;
    private int _playerID = 0;

    public void OnPlayerJoin()
    {
        playerController = FindAnyObjectByType<PlayerController>();
        playerController.PlayerID = _playerID;
        GameManager.Instance.PlayerCount++;

        Debug.Log("Player " + _playerID + " joined the game!");

        playerController.name = "Player " + _playerID;

        _playerID++;
    }
}