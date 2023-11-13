using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public GameObject fpsModel;
    public GameObject nonFpsModel;

    public GameObject playerUiPrefab;

    public PlayerMovementController playerMovementController;
    public Camera fpsCamera;

    private Animator _animator;
    public Avatar fpsAvatar, nonFpsAvatar;
    private Rigidbody _rigidbody;

    private Shooting shooting;


    private void Start()
    {
        playerMovementController = this.GetComponent<PlayerMovementController>();
        _animator = this.GetComponent<Animator>();
        _rigidbody = this.GetComponent<Rigidbody>();

        fpsModel.SetActive(photonView.IsMine);
        nonFpsModel.SetActive(!photonView.IsMine);
        _animator.SetBool("IsLocalPlayer", photonView.IsMine);
        _animator.avatar = photonView.IsMine ? fpsAvatar : nonFpsAvatar;

        shooting = this.GetComponent<Shooting>();
        //_gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        //For each player prefab, check which is the current player's and disable input for the others.
        if (photonView.IsMine)
        {
            GameObject playerUi = Instantiate(playerUiPrefab);
            playerMovementController.FixedTouchField = playerUi.transform.Find("RotationTouchField").GetComponent<FixedTouchField>();
            playerMovementController._joystick = playerUi.transform.Find("Fixed Joystick").GetComponent<Joystick>();
            fpsCamera.enabled = true;
            

            playerUi.transform.Find("Fire Button").GetComponent<Button>().onClick.AddListener(() => shooting.Fire());
        }

        else
        {
            playerMovementController.enabled = false;
            fpsCamera.enabled = false;
            fpsCamera.gameObject.SetActive(false);
            GetComponent<RigidbodyFirstPersonController>().enabled = false;
        }
    }
}
