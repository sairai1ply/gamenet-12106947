using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject playerPrefab;

    public static GameManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }

        else
        {
            instance = this;
        }
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (playerPrefab != null)
            {
                StartCoroutine(CO_DelayedPlayerSpawn());
            }
        }
    }

    IEnumerator CO_DelayedPlayerSpawn()
    {
        yield return new WaitForSeconds(3);
        int xRandomPoint = Random.Range(-20, 20);
        int zRandomPoint = Random.Range(-20, 20);

        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(xRandomPoint, 0, zRandomPoint), Quaternion.identity);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.NickName} has joined the {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} has entered room {PhotonNetwork.CurrentRoom.Name}\nRoom has {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers} players.");
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("GameLauncherScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
