using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameVersion = "1";
    [SerializeField] private MainPageUI mainPageUI;

    private void Start()
    {
        PhotonNetwork.GameVersion = gameVersion;
        TryConnect();
    }

    private void TryConnect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        TryConnect();
    }

    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            TryConnect();
            return;
        }

        int version = GameManager.Instance.CurrentVersion;

        PhotonNetwork.JoinRandomRoom(
            new Hashtable { { "Version", version } },
            0
        );
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {

        int version = GameManager.Instance.CurrentVersion;

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            CustomRoomProperties = new Hashtable { { "Version", version } },
            CustomRoomPropertiesForLobby = new[] { "Version" }
        };

        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        mainPageUI.MachingButton();
        UpdateWaitingText();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateWaitingText();

        if (!PhotonNetwork.IsMasterClient) return;

        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            GameManager.Instance.AssignRoles();

            photonView.RPC(nameof(StartGameForAll), RpcTarget.All, 1);
        }
    }

    private void UpdateWaitingText()
    {
        mainPageUI.machingText.text =
            $"Waiting for players... ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})";
    }

    [PunRPC]
    private void StartGameForAll(int mode)
    {
        GameManager.Instance.GameStart(mode);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
