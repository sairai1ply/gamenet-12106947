using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status Panel")]
    public Text connectionStatusText;

    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject loginUIPanel;

    [Header("Game Options Panel")]
    public GameObject gameOptionsPanel;

    [Header("Create Room Panel")]
    public GameObject createRoomPanel;
    public InputField roomNameInputField;
    public InputField playerCountInputField;

    [Header("Inside Room Panel")]
    public GameObject insideRoomPanel;
    public Text roomInfoText;
    public GameObject playerListItemPrefab;
    public GameObject playerListViewParent;
    public GameObject startGameButton;

    [Header("Room List Panel")]
    public GameObject roomListPanel;
    public GameObject roomItemPrefab;
    public GameObject roomListParent;

    [Header("Join Random Room Panel")]
    public GameObject joinRandomRoomPanel;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomlistGameObjects;
    private Dictionary<int, GameObject> playerListGameObjects;

    #region Unity Functions
    private void Start()
    {
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomlistGameObjects = new Dictionary<string, GameObject>();
        ActivatePanel(loginUIPanel);

        //Load the same level for other players
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Update()
    {
        connectionStatusText.text = $"Connection status: {PhotonNetwork.NetworkClientState}";
    }

    #endregion

    #region UI Callbacks
    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;

        if (string.IsNullOrEmpty(playerName))
        {
            Debug.Log("Player name is invalid!");
        }

        else
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = $"Room {Random.Range(100, 100000)}";
        }
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(playerCountInputField.text);

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(gameOptionsPanel);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(roomListPanel);
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        ActivatePanel(gameOptionsPanel);
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomClicked()
    {
        ActivatePanel(joinRandomRoomPanel);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGameButtonClicked()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }
    #endregion

    #region PUN Callbacks
    public override void OnConnected()
    {
        Debug.Log("Connected to the Internet!");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} has connected to Photon Servers!");
        ActivatePanel(gameOptionsPanel);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"{PhotonNetwork.CurrentRoom.Name} created");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"{PhotonNetwork.LocalPlayer.NickName} has joined {PhotonNetwork.CurrentRoom.Name}");
        ActivatePanel(insideRoomPanel);

        roomInfoText.text = $"Room Name: {PhotonNetwork.CurrentRoom.Name}   |   Current Player Count: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerItem = Instantiate(playerListItemPrefab);
            playerItem.transform.SetParent(playerListViewParent.transform);
            playerItem.transform.localScale = Vector3.one;

            playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

            playerListGameObjects.Add(player.ActorNumber, playerItem);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListGameObjects();
        Debug.Log("OnRoomListUpdate() is called");

        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        foreach (RoomInfo info in roomList)
        {
            Debug.Log(info.Name);
            
            //conditions to remove certain from room list
            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }
            }

            else
            {
                //update existing rooms info
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList[info.Name] = info;
                }

                else
                {
                    cachedRoomList.Add(info.Name, info);
                }
            }
        }

        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject listItem = Instantiate(roomItemPrefab);

            listItem.transform.SetParent(roomListParent.transform);
            listItem.transform.localScale = Vector3.one;

            listItem.transform.Find("RoomNameText").GetComponent<Text>().text = info.Name;
            listItem.transform.Find("RoomPlayersText").GetComponent<Text>().text = $"Player count: {info.PlayerCount} / {info.MaxPlayers}";

            listItem.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinedRoomClicked(info.Name));

            roomlistGameObjects.Add(info.Name, listItem);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListGameObjects();
        cachedRoomList.Clear();
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        roomInfoText.text = $"Room Name: {PhotonNetwork.CurrentRoom.Name}   |   Current Player Count: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";

        GameObject playerItem = Instantiate(playerListItemPrefab);
        playerItem.transform.SetParent(playerListViewParent.transform);
        playerItem.transform.localScale = Vector3.one;

        playerItem.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

        //for the player to know who is themself 
        playerItem.transform.Find("PlayerIndicator").gameObject.SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);

        playerListGameObjects.Add(player.ActorNumber, playerItem);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        roomInfoText.text = $"Room Name: {PhotonNetwork.CurrentRoom.Name}   |   Current Player Count: {PhotonNetwork.CurrentRoom.PlayerCount} / {PhotonNetwork.CurrentRoom.MaxPlayers}";
        Destroy(playerListGameObjects[otherPlayer.ActorNumber]);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);
    }

    public override void OnLeftRoom()
    {
        foreach(var gameObject in playerListGameObjects.Values)
        {
            Destroy(gameObject);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;
        ActivatePanel(gameOptionsPanel);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        string roomName = "Room " + Random.Range(1000, 10000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    #endregion

    #region Private Methods
    private void OnJoinedRoomClicked(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    private void ClearRoomListGameObjects()
    {
        foreach (var item in roomlistGameObjects.Values)
        {
            Destroy(item);
        }

        roomlistGameObjects.Clear();
    }
    #endregion

    #region Public Methods
    public void ActivatePanel(GameObject panelToActivate)
    {
        loginUIPanel.SetActive(panelToActivate.Equals(loginUIPanel));
        gameOptionsPanel.SetActive(panelToActivate.Equals(gameOptionsPanel));
        createRoomPanel.SetActive(panelToActivate.Equals(createRoomPanel));
        insideRoomPanel.SetActive(panelToActivate.Equals(insideRoomPanel));
        roomListPanel.SetActive(panelToActivate.Equals(roomListPanel));
        joinRandomRoomPanel.SetActive(panelToActivate.Equals(joinRandomRoomPanel));
    }
    #endregion
}
