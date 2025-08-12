using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    private int currentMode; // 현재 게임 모드 저장
    private int currentVersion;
    private int playerScore;
    public int CurrentVersion => currentVersion + 3;
    public bool isOTurnFirst; // O가 먼저 시작하는지 여부
    public TicTacToeNxN tictactoe;

    public string PlayerRole { get; private set; } // "O" 또는 "X"
    private DatabaseReference databaseReference;

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
    public void GetUserData()
    {
        // Firebase Database 참조 초기화
        if (LoginManager.user != null)
        {
            string userId = LoginManager.user.UserId;
            databaseReference = FirebaseDatabase.DefaultInstance.GetReference($"users/{userId}");
        }
        LoadPlayerScore();
    }
    public void SavePlayerScore(int playerScore)
    {
        if (databaseReference == null)
        {
            Debug.LogError("DatabaseReference가 초기화되지 않았습니다.");
            return;
        }

        databaseReference.Child("playerScore").SetValueAsync(playerScore).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("PlayerScore 저장 완료: " + playerScore);
            }
            else
            {
                Debug.LogError("PlayerScore 저장 실패: " + task.Exception);
            }
        });
    }

    public void LoadPlayerScore()
    {
        if (databaseReference == null)
        {
            Debug.LogError("DatabaseReference가 초기화되지 않았습니다.");
            return;
        }

        databaseReference.Child("playerScore").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result.Exists)
                {
                    int score = int.Parse(task.Result.Value.ToString());
                    playerScore = score;
                    Debug.Log("PlayerScore 로드 완료: " + score);
                }
                else
                {
                    Debug.Log("PlayerScore가 존재하지 않습니다. 기본값을 설정합니다.");
                    SavePlayerScore(1000); // 기본값 저장
                }
            }
            else
            {
                Debug.LogError("PlayerScore 로드 실패: " + task.Exception);
            }
        });
    }
    public int[] UpdatePlayerScore(bool win)
    {
        int score = 0;
        // CurrentVersion에 따라 점수 랜덤 할당
        if (currentVersion == 0)
        {
            score = Random.Range(1, 4); // 1~3점
        }
        else if (currentVersion == 1)
        {
            score = Random.Range(3, 6); // 3~5점
        }
        if (win)
            playerScore += score;
        else
            playerScore -= score;
        if(playerScore<0)
            playerScore = 0;
        SavePlayerScore(playerScore);
        int[] Scores = { playerScore, score };
        return Scores;
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
            SceneManager.LoadScene(2); // 4x4 게임
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