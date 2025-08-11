using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    private int currentMode; // 현재 게임 모드 저장
    private int currentVersion;
    public int CurrentVersion => currentVersion + 3;
    public bool isOTurnFirst; // O가 먼저 시작하는지 여부
    public TicTacToeNxN tictactoe;

    public string PlayerRole { get; private set; } // "O" 또는 "X"

    void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AssignRoles()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // MasterClient는 true(O), 다른 플레이어는 false(X)
            photonView.RPC("SetPlayerRole", RpcTarget.MasterClient, true);
            photonView.RPC("SetPlayerRole", RpcTarget.Others, false);
        }
    }

    [PunRPC]
    private void SetPlayerRole(bool isO)
    {
        PlayerRole = isO ? "O" : "X";
    }

    public void GameStart(int mode)
    {
        currentMode = mode; // 현재 모드 저장

        // O와 X 순서를 랜덤으로 배정
        isOTurnFirst = Random.value > 0.5f;
        if(currentMode != 0)
        {
            // isOTurnFirst 값을 모든 클라이언트에 동기화
            photonView.RPC("SyncIsOTurnFirst", RpcTarget.All, isOTurnFirst);
        }
        if (currentVersion == 0)
        {
            SceneManager.LoadScene(2); // 3x3 게임
        }
        else if (currentVersion == 1)
        {
            SceneManager.LoadScene(3); // 4x4 게임
        }
    }

    [PunRPC]
    private void SyncIsOTurnFirst(bool isOFirst)
    {
        isOTurnFirst = isOFirst;
    }
    public void GameSet()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        SceneManager.LoadScene(1);
    }

    public bool IsAIMode()
    {
        return currentMode == 0; // 현재 모드가 AI 대결인지 확인
    }
    public void SetVersion(int version)
    {
        currentVersion = version;
    }
}