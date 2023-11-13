using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class Shooting : MonoBehaviourPunCallbacks
{
    public Camera _camera;
    public GameObject hitEffectPrefab;

    private Animator _animator;

    private Text killfeedText;
    private GameManager _gameManager;

    private bool _isAlive = true;

    [Header("HP Related")]
    public float startHealth = 100;
    private float health;
    public Image healthBar;
    public TMP_Text playerNameText;

    private float killfeedTime = 0;

    private void Start()
    {
        health = startHealth;
        healthBar.fillAmount = health / startHealth;
        _animator = GetComponent<Animator>();

        playerNameText.text = photonView.Owner.NickName;

        _gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void Fire()
    {
        RaycastHit hit;
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        if (Physics.Raycast(ray, out hit, 200))
        {
            //Debug.Log(hit.collider.gameObject.name);

            //Instantiate effects for when an object is hit, sent via RPC to be seen throughout all players
            photonView.RPC("CreateHitEffects", RpcTarget.All, hit.point);

            //For when another player gets hit and prevent player from hitting self
            if (hit.collider.gameObject.CompareTag("Player") && !hit.collider.gameObject.GetComponent<PhotonView>().IsMine)
            {
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered, 25);
            }
        }
    }

    //PunRPC (remote procedure call)
    //Allows us to use PhotonMessageInfo by default
    [PunRPC] 
    public void TakeDamage(int damage, PhotonMessageInfo info)
    {
        //modify health value
        this.health -= damage;

        //Update self's UI Healthbar
        this.healthBar.fillAmount = health / startHealth;

        if (health <= 0 && _isAlive)
        {
            Die();

            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner.NickName);

            //killfeed display code
            killfeedTime = 4f;
            string killfeedTextString = info.Sender.NickName + " killed " + info.photonView.Owner.NickName;
            StartCoroutine(CO_DisplayKillfeed(killfeedTextString));

            AddKillToGM(info.Sender.NickName);
        }
    }

    private void AddKillToGM(string killerNickName)
    {
        if (_gameManager._playerKills.ContainsKey(killerNickName))
        {
            _gameManager._playerKills[killerNickName] += 1;
            Debug.Log("Contains key " + killerNickName);
        }

        else
        {
            _gameManager._playerKills.Add(killerNickName, 1);
        }

        _gameManager.UpdateKillLeaderUI(killerNickName, _gameManager._playerKills[killerNickName]);
    }

    public void Die()
    {
        if (photonView.IsMine)
        {
            _animator.SetBool("IsDead", true);
            StartCoroutine(CO_SpawnDeathCounter());
        }

        this._isAlive = false;
    }

    private IEnumerator CO_SpawnDeathCounter()
    {
        Debug.Log("Started CO_SpawnDeathCounter");
        GameObject respawnText = GameObject.Find("Respawn Text");
        float respawnTime = 5.0f;
        
        while(respawnTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            respawnTime--;

            transform.GetComponent<PlayerMovementController>().enabled = false;

            if (respawnText != null)
                respawnText.GetComponent<TMP_Text>().text = "You are killed. Respawning in " + respawnTime.ToString(".00");
            else
                Debug.Log("respawnText is null");
        }

        //initiate respawn
        _animator.SetBool("IsDead", false);

        if (respawnText != null)
            respawnText.GetComponent<TMP_Text>().text = "";

        //New Position Upon Respawn
        this.transform.position = _gameManager.GetRandomSpawnpointTransform().position;

        transform.GetComponent<PlayerMovementController>().enabled = true;

        photonView.RPC("RegainHealth", RpcTarget.AllBuffered);
    }

    private IEnumerator CO_DisplayKillfeed(string text)
    {
        GameObject killfeedTextUI = GameObject.Find("Killfeed Text");

        while (killfeedTime > 0) 
        { 
            yield return new WaitForSeconds(1.0f);
            killfeedTime--;

     
            if (killfeedTextUI != null)
                killfeedTextUI.GetComponent<TMP_Text>().text = text;
            else
                Debug.Log("killfeedUI is null");
        }

        killfeedTextUI.GetComponent<TMP_Text>().text = "";
    }

    [PunRPC]
    public void CreateHitEffects(Vector3 position)
    {
        GameObject hitEffectGameObject = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(hitEffectGameObject, 0.2f);
    }

    [PunRPC]
    public void RegainHealth()
    {
        health = 100;
        healthBar.fillAmount = health / startHealth;
        _isAlive = true;
    }
}
