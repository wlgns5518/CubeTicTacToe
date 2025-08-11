using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "1";
    public TextMeshProUGUI connectionInfoText;
    public GameObject machingPopup;
    public TextMeshProUGUI machingText;

    void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogWarning("Already connected to Photon. Skipping ConnectUsingSettings.");
        }
    }

    // Called when the client is connected to the Master Server
    public override void OnConnectedToMaster()
    {
        connectionInfoText.text = "OnLine : Connected to Master Server";
        // You can now join a lobby or create/join a room
    }

    // Called when the client fails to connect to the server
    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionInfoText.text = $"Offline : Connection Disabled{cause.ToString()} = Try reconnection...";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            connectionInfoText.text = "Connecting to a Random Room";

            // GameManager의 현재 버전을 가져와 조건 설정
            int version = GameManager.Instance.CurrentVersion;

            // 버전에 맞는 방을 찾기 위해 ExpectedCustomRoomProperties 설정
            Hashtable expectedRoomProperties = new Hashtable
        {
            { "Version", version }
        };

            PhotonNetwork.JoinRandomRoom(expectedRoomProperties, 0);
        }
        else
        {
            connectionInfoText.text = "Offline : Connection Disabled = Try reconnection...";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionInfoText.text = "There is no empty room, Creating new Room";

        // GameManager의 현재 버전을 가져와 방 속성 설정
        int version = GameManager.Instance.CurrentVersion;

        // 방 생성 옵션 설정
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            CustomRoomProperties = new Hashtable
        {
            { "Version", version }
        },
            CustomRoomPropertiesForLobby = new string[] { "Version" } // 로비에서 검색 가능하도록 설정
        };

        // 방 생성
        PhotonNetwork.CreateRoom(null, roomOptions);
    }
    public override void OnJoinedRoom()
    {
        connectionInfoText.text = "Connected with Room";
        machingPopup.SetActive(true);
        machingText.text = $"Waiting for players... ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})";
    }
    [PunRPC]
    private void StartGameForAll(int mode)
    {
        GameManager.Instance.GameStart(mode);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        machingText.text = $"Waiting for players... ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})";
        // 방 생성자일 경우 추가 로직
        if (PhotonNetwork.IsMasterClient)
        {
            // 모든 플레이어가 방에 참가했는지 확인
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                connectionInfoText.text = "All players have joined. Starting the game...";

                // 역할 분배
                GameManager.Instance.AssignRoles();

                // 모든 클라이언트에서 GameStart 호출
                photonView.RPC("StartGameForAll", RpcTarget.All, 1);
            }
        }
    }
    public void MachingCancel()
    {
        machingPopup.SetActive(false);
    }
    public void LeaveRoom()
    {
        connectionInfoText.text = "Left the room. Disconnecting from Game Server...";
        PhotonNetwork.LeaveRoom();
    }


}