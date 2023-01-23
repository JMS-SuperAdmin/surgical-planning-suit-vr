using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using WebSocketSharp;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status")]
    [SerializeField] private Text _connectionStatusText;
    [SerializeField] private Animator _fadeUIAnimator;

    [Header("Login UI Panel")]
    [SerializeField] private InputField _playerNameInput;
    [SerializeField] private GameObject _loginUIPanel;

    [Header("Game Options UI Panel")]
    [SerializeField] private GameObject _gameOptionsUIPanel;

    [Header("Create Room UI Panel")]
    [SerializeField] private GameObject _createRoomUIPanel;
    [SerializeField] private InputField _roomNameInputField;
    [SerializeField] private InputField _maxPlayerInput;

    [Header("Inside Room UI Panel")]
    [SerializeField] private GameObject _insideRoomUIPanel;
    [SerializeField] private Text _roomInfoText;
    [SerializeField] private GameObject _playerListContent;
    [SerializeField] private GameObject _playerListPrefab;
    [SerializeField] private GameObject _startGameButton;

    [Header("Room List UI Panel")]
    [SerializeField] private GameObject _roomListUIPanel;
    [SerializeField] private GameObject _roomListContent;
    [SerializeField] private GameObject _roomListPrefab;

    [Header("Join Random Room UI Panel")]
    [SerializeField] private GameObject _joinRandomRoomUIPanel;

    private Dictionary<string, RoomInfo> _cachedRoomList;
    private Dictionary<string, GameObject> _roomListGameObjects;
    private Dictionary<int, GameObject> _playerListGameObjects;

    #region Unity Methods

    private void Start()
    {
        ActivatePanel(_loginUIPanel.name);
        _cachedRoomList = new Dictionary<string, RoomInfo>();
        _roomListGameObjects = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;
        SetRandomName();
    }

    private void Update()
    {
        _connectionStatusText.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }

    #endregion

    #region UI Callbacks
    public void OnLoginButtonClicked()
    {
        string playerName = _playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Player name is invalid");
        }
    }

    public void OnRoomCreateButtonClicked()
    {
        string roomName = _roomNameInputField.text;

        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 9999);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = string.IsNullOrEmpty(_maxPlayerInput.text) ? (byte)3 : (byte)int.Parse(_maxPlayerInput.text);
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(_gameOptionsUIPanel.name);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(_roomListUIPanel.name);
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(_gameOptionsUIPanel.name);
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomButtonClicked()
    {
        ActivatePanel(_joinRandomRoomUIPanel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon");
        ActivatePanel(_gameOptionsUIPanel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(_insideRoomUIPanel.name);

        // Show start button only if player is master
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            _startGameButton.SetActive(true);
        }
        else
        {
            _startGameButton.SetActive(false);
        }

        _roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
            "Players/Max.players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
            PhotonNetwork.CurrentRoom.MaxPlayers;

        if (_playerListGameObjects == null)
        {
            _playerListGameObjects = new Dictionary<int, GameObject>();
        }

        // Instantiating player list gameobjects
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListGameObject = Instantiate(_playerListPrefab);
            playerListGameObject.transform.SetParent(_playerListContent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
            }

            _playerListGameObjects.Add(player.ActorNumber, playerListGameObject);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " is joined to " + PhotonNetwork.CurrentRoom.Name);

        //Update room info text
        _roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
            "Players/Max.players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
            PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject playerListGameObject = Instantiate(_playerListPrefab);
        playerListGameObject.transform.SetParent(_playerListContent.transform);
        playerListGameObject.transform.localScale = Vector3.one;

        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;

        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }

        _playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);
    }

    // Called when remote player left
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Update room info text
        _roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + " " +
            "Players/Max.players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" +
            PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(_playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        _playerListGameObjects.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            _startGameButton.SetActive(true);
        }
    }

    // Called when local player left
    public override void OnLeftRoom()
    {
        ActivatePanel(_gameOptionsUIPanel.name);
        foreach (GameObject playerListGameObject in _playerListGameObjects.Values)
        {
            Destroy(playerListGameObject);
        }
        _playerListGameObjects.Clear();
        _playerListGameObjects = null;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);

            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList)
            {
                if (_cachedRoomList.ContainsKey(room.Name))
                {
                    _cachedRoomList.Remove(room.Name);
                }
            }
            else
            {
                //Update cachedRoomList list
                if (_cachedRoomList.ContainsKey(room.Name))
                {
                    _cachedRoomList[room.Name] = room;
                }
                else //Add the new room to the cached room list
                {
                    _cachedRoomList.Add(room.Name, room);
                }
            }
        }

        foreach (RoomInfo room in _cachedRoomList.Values)
        {
            GameObject roomListPrefabClone = Instantiate(_roomListPrefab);
            roomListPrefabClone.transform.SetParent(_roomListContent.transform);
            roomListPrefabClone.transform.localScale = Vector3.one;

            roomListPrefabClone.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListPrefabClone.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + " / " + room.MaxPlayers;
            roomListPrefabClone.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));

            _roomListGameObjects.Add(room.Name, roomListPrefabClone);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        _cachedRoomList.Clear();
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

    private void OnJoinRoomButtonClicked(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    private void ClearRoomListView()
    {
        foreach (var roomListGameObject in _roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }
        _roomListGameObjects.Clear();
    }

    #endregion

    #region Public Methods
    public void ActivatePanel(string panelName)
    {
        _loginUIPanel.SetActive(panelName.Equals(_loginUIPanel.name));
        _gameOptionsUIPanel.SetActive(panelName.Equals(_gameOptionsUIPanel.name));
        _createRoomUIPanel.SetActive(panelName.Equals(_createRoomUIPanel.name));
        _insideRoomUIPanel.SetActive(panelName.Equals(_insideRoomUIPanel.name));
        _roomListUIPanel.SetActive(panelName.Equals(_roomListUIPanel.name));
        _joinRandomRoomUIPanel.SetActive(panelName.Equals(_joinRandomRoomUIPanel.name));
        _fadeUIAnimator.SetTrigger("IsShow");
    }
    #endregion

    #region Temp Methods
    //private void SetRandomName()
    //{
    //    int nameLength = 6;
    //    string KEY = "0123456789abcdef9876543210fedcba0123456789abcdef";
    //    string randomStr = "";
    //    System.Random rnd = new System.Random();
    //    int iRandom;
    //    for (int i = 1; i <= nameLength; i++)
    //    {
    //        iRandom = rnd.Next(1, KEY.Length);
    //        randomStr += KEY.Substring(iRandom, 1);
    //    }

    //    _playerNameInput.text = randomStr;

    //}

    public void SetRandomName()
    {
        StartCoroutine(GetRandomName());
    }

    private IEnumerator GetRandomName()
    {
        string uri = "https://randomuser.me/api/?nat=in&gender=male";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("ConnectionError: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("DataProcessingError: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("ProtocolError: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    JSONNode jsonDataFromWebservice = JSONNode.Parse(webRequest.downloadHandler.text);
                    _playerNameInput.text = "Dr. " + jsonDataFromWebservice[0][0]["name"]["first"].Value;
                    //Debug.Log(jsonDataFromWebservice[0][0]["name"]["first"].Value);
                    break;
            }
        }
    }
    #endregion
}
