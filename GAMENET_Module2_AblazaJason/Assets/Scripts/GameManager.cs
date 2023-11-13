using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;

    [SerializeField] private GameObject[] _spawnpoints;

    [SerializeField] private int _numberOfKillsToWin;

    public Dictionary<string, int> _playerKills;

    private int _highestKills;

    private TMP_Text killLeaderTextUI;

    private void Start()
    {
        //Find all spawnpoints and store the transform
        _spawnpoints = GameObject.FindGameObjectsWithTag("Spawnpoint");

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab != null)
            {
                StartCoroutine(CO_DelayPlayerSpawn());
            }
        }

        _playerKills = new Dictionary<string, int>();
        _highestKills = 0;
    }

    private IEnumerator CO_DelayPlayerSpawn()
    {
        yield return new WaitForSeconds(2f);

        PhotonNetwork.Instantiate(playerPrefab.name, GetRandomSpawnpointTransform().position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        DelayedUIInstantiate();
    }

    private void DelayedUIInstantiate()
    {
        killLeaderTextUI = GameObject.Find("Kill Leader Text").GetComponent<TMP_Text>();
        if (killLeaderTextUI != null)
        {
            killLeaderTextUI.text = "";
        }
    }

    public Transform GetRandomSpawnpointTransform()
    {
        GameObject t = _spawnpoints[Random.Range(0, _spawnpoints.Length - 1)];

        if (t != null)
            return t.transform;

        else return null;
    }

    public void UpdateKillLeaderUI(string key, int newNumberOfKills)
    {
        if (newNumberOfKills > _highestKills)
        {
            _highestKills = newNumberOfKills;

            if (killLeaderTextUI.text != null)
            {
                killLeaderTextUI.text = $"Kill Leader\n{key} - {_highestKills}";

                if (_highestKills >= _numberOfKillsToWin)
                {
                    killLeaderTextUI.text = $"{key} wins with {_highestKills} kills";
                }
            }
        }
    }
}