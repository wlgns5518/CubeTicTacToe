using Firebase.Database;
using Firebase.Extensions;
using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    private int currentMode;              // 현재 게임 모드
    private int currentVersion;           // 게임 버전 (난이도/판 크기)
    private int playerScore; // 내부에서만 수정 가능
    public int PlayerScore => playerScore; // 외부에서 읽기 전용

    public int CurrentVersion => currentVersion + 3; // 최소 3x3부터 시작
    public bool isOTurnFirst;             // O가 먼저 시작하는지 여부
    public string PlayerRole { get; private set; }   // "O" 또는 "X"

    public TicTacToeNxN tictactoe;

    private DatabaseReference databaseReference;
    private readonly TaskCompletionSource<bool> gameTasksCompleted = new();

    public Task GameTasksCompleted => gameTasksCompleted.Task;

    #region Unity Lifecycle
    private void Awake()
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

    private void OnDestroy()
    {
        SavePlayerScore(playerScore);
    }
    #endregion

    #region Firebase
    public void GetUserData()
    {
        if (LoginManager.user == null) return;

        string userId = LoginManager.user.UserId;
        databaseReference = FirebaseDatabase.DefaultInstance.GetReference($"users/{userId}");

        LoadPlayerScore();
        gameTasksCompleted.TrySetResult(true); // Firebase 초기화 완료 신호
    }

    public void SavePlayerScore(int score)
    {
        if (databaseReference == null)
        {
            Debug.LogWarning("DatabaseReference가 초기화되지 않았습니다.");
            return;
        }

        databaseReference.Child("playerScore").SetValueAsync(score)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                    Debug.Log($"PlayerScore 저장 완료: {score}");
                else
                    Debug.LogError($"PlayerScore 저장 실패: {task.Exception}");
            });
    }

    private void LoadPlayerScore()
    {
        if (databaseReference == null) return;

        databaseReference.Child("playerScore").GetValueAsync().ContinueWith(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError($"PlayerScore 로드 실패: {task.Exception}");
                return;
            }

            if (task.Result.Exists && int.TryParse(task.Result.Value.ToString(), out int score))
            {
                playerScore = score;
                Debug.Log($"PlayerScore 로드 완료: {score}");
            }
            else
            {
                Debug.Log("PlayerScore 없음 → 기본값 1000 설정");
                playerScore = 1000;
                SavePlayerScore(playerScore);
            }
        });
    }
    #endregion

    #region Score
    public int[] UpdatePlayerScore(bool win)
    {
        int score = currentVersion switch
        {
            0 => Random.Range(1, 4), // 1~3
            1 => Random.Range(3, 6), // 3~5
            _ => Random.Range(1, 3)  // 기본값 (안정성)
        };

        playerScore = Mathf.Max(0, playerScore + (win ? score : -score));
        SavePlayerScore(playerScore);

        return new[] { playerScore, score };
    }
    #endregion

    #region Photon
    public void AssignRoles()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(SetPlayerRole), RpcTarget.MasterClient, true);
        photonView.RPC(nameof(SetPlayerRole), RpcTarget.Others, false);
    }

    [PunRPC]
    private void SetPlayerRole(bool isO) => PlayerRole = isO ? "O" : "X";

    [PunRPC]
    private void SyncIsOTurnFirst(bool isOFirst) => isOTurnFirst = isOFirst;
    #endregion

    #region Game Flow
    public void GameStart(int mode)
    {
        currentMode = mode;
        isOTurnFirst = Random.value > 0.5f;

        if (mode != 0)
        {
            photonView.RPC(nameof(SyncIsOTurnFirst), RpcTarget.All, isOTurnFirst);
        }

        SceneManager.LoadScene(2); // 게임씬 로드
    }

    public void GameSet()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene(1); // 로비로 이동
    }

    public bool IsAIMode() => currentMode == 0;

    public void SetVersion(int version) => currentVersion = version;
    #endregion
}
