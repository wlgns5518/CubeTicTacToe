using Firebase.Extensions;
using Firebase.Firestore;
using Photon.Pun;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    private int currentMode;
    private int currentVersion;
    private int playerScore;
    public int PlayerScore => playerScore;

    public int CurrentVersion => currentVersion + 3;
    public bool isOTurnFirst;
    public string PlayerRole { get; private set; }

    public TicTacToeNxN tictactoe;

    private DocumentReference userDocRef;
    private FirebaseFirestore firestore;
    private readonly TaskCompletionSource<bool> gameTasksCompleted = new();

    public Task GameTasksCompleted => gameTasksCompleted.Task;

    #region Unity Lifecycle
    private void Awake()
    {
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

    #region Firestore
    public void GetUserData()
    {
        if (LoginManager.user == null) return;

        firestore = FirebaseFirestore.DefaultInstance;
        string userId = LoginManager.user.UserId;
        userDocRef = firestore.Collection("users").Document(userId);

        LoadPlayerScore();
    }

    public void SavePlayerScore(int score)
    { 
        if (userDocRef == null)
        {
            Debug.LogWarning("User DocumentReference가 초기화되지 않았습니다.");
            return;
        }

        var data = new Dictionary<string, object>
        {
            { "playerScore", score },
            { "userId", LoginManager.user.UserId }
        };

        userDocRef.SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log($"PlayerScore 저장 완료: {score}");
            else
                Debug.LogError($"PlayerScore 저장 실패: {task.Exception}");
        });
    }

    private void LoadPlayerScore()
    {
        if (userDocRef == null) return;

        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompletedSuccessfully)
            {
                Debug.LogError($"PlayerScore 로드 실패: {task.Exception}");
                return;
            }

            var snapshot = task.Result;
            if (snapshot.Exists && snapshot.TryGetValue("playerScore", out int score))
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

            gameTasksCompleted.TrySetResult(true);
        });
    }
    #endregion

    #region Score
    public int[] UpdatePlayerScore(bool win)
    {
        int score = currentVersion switch
        {
            0 => Random.Range(1, 4),
            1 => Random.Range(3, 6),
            _ => Random.Range(1, 3)
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
            photonView.RPC(nameof(SyncIsOTurnFirst), RpcTarget.All, isOTurnFirst);

        SceneManager.LoadScene(2);
    }

    public void GameSet()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene(1);
    }

    public bool IsAIMode() => currentMode == 0;

    public void SetVersion(int version) => currentVersion = version;
    #endregion
}