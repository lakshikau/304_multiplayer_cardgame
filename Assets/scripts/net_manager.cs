using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine.UI;

public class net_manager : MonoBehaviourPunCallbacks
{
    public static net_manager network_manager;

    public byte maxPlayers;
    public GameObject connectButtonObject;
    public GameObject leaveButtonObject;
    public InputField playerName;
    
    private Button connectButton;
    private Button leaveButton;


    private void Awake()
    {
        network_manager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // connect to server at load time
        PhotonNetwork.ConnectUsingSettings(); // no need to send the game version
        connectButton = connectButtonObject.GetComponent<Button>();
        leaveButton = leaveButtonObject.GetComponent<Button>();
        connectButton.interactable = false;
        leaveButton.interactable = false;
    }
    public override void OnConnectedToMaster()
    {
        // call back when connection is made
        Debug.Log("Connected to server");
        connectButton.interactable = true;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // call back when disconneced from server
        Debug.Log("Disconnected from server " + cause.ToString());
    }

    public void OnConnectButtonClicked()
    {
        // look for exesting room and join
        // create and join if no room found 
        RoomOptions options = new RoomOptions() {
            MaxPlayers =maxPlayers,
            IsOpen = true,
            IsVisible = true,
            PublishUserId = true
        };
        PhotonNetwork.JoinOrCreateRoom("playRoom", options, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        // call back when a new room is created
        Debug.Log("Created room successfully");
    }

    public override void OnJoinedRoom()
    {
        // callback when joined to a room
        Debug.Log("Joined room");
        CreatePlayer();
        leaveButton.interactable = true;
        connectButton.interactable = false;
        playerName.interactable = false;
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // callback for room creation error
        Debug.Log("Created room failed" + message);
    }
    public void OnLeaveButtonClicekd()
    {
        // leave the current room
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        // callback when leaving a room
        Debug.Log("plyer left the room");
        leaveButton.interactable = false;
        connectButton.interactable = true;
        playerName.interactable = true;
    }

    private void CreatePlayer()
    {
        Debug.Log("creating the player");
        GameObject local_player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "NetPlayer"), Vector3.zero, Quaternion.identity);
        PhotonNetwork.LocalPlayer.NickName = playerName.text;
        // this nick name is only used to set the wiining bid so far
        // but will be used for other stuff
    }
}
