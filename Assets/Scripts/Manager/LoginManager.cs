using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    public static FirebaseUser user { get; private set; }

    private readonly TaskCompletionSource<bool> loginTasksCompleted = new();
    public Task LoginTasksCompleted => loginTasksCompleted.Task;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        CheckFirebaseDependencies();
    }

    private void Start()
    {
        SoundManager.Instance.PlayBGMSound();
    }

    #region Firebase 초기화 및 자동 로그인
    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase dependencies available.");
                FirebaseApp app = FirebaseApp.DefaultInstance;

                auth = FirebaseAuth.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;
                TryAutoLogin();
            }
            else
            {
                Debug.LogError($"Firebase dependencies not available: {task.Result}");
            }
        });
    }

    private void TryAutoLogin()
    {
        // Firebase Auth는 로그인 세션을 디바이스에 자동 보관합니다.
        // 앱 재시작 시 CurrentUser가 비어있지 않으면 이전 로그인 상태가 그대로 복원됩니다.
        if (auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            Debug.Log($"자동 로그인 성공: {user.UserId} (Anonymous: {user.IsAnonymous})");
            SaveUserDocument(user.UserId);
        }
        else
        {
            Debug.Log("자동 로그인 실패: 저장된 세션 없음.");
        }
    }
    #endregion

    #region Google 로그인 (Firebase Federated OAuth)
    public void SignInWithGoogle()
    {
        if (auth == null)
        {
            Debug.LogError("Firebase Auth not initialized.");
            return;
        }

        var providerData = new FederatedOAuthProviderData
        {
            ProviderId = GoogleAuthProvider.ProviderId,
            Scopes = new List<string> { "email", "profile" }
        };

        var provider = new FederatedOAuthProvider();
        provider.SetProviderData(providerData);

        Debug.Log("Calling Firebase SignInWithProvider (Google)");

        auth.SignInWithProviderAsync(provider).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                foreach (var e in task.Exception.InnerExceptions)
                    Debug.LogError("Google Sign-In Error: " + e.Message);
                return;
            }

            if (task.IsCanceled)
            {
                Debug.LogWarning("Google Sign-In Canceled");
                return;
            }

            user = task.Result.User;
            if (user != null)
            {
                SaveUserDocument(user.UserId);
            }
        });
    }

    public void SignOutFromGoogle()
    {
        if (auth != null)
        {
            auth.SignOut();
            user = null;
            Debug.Log("로그아웃 완료");
        }
    }
    #endregion

    #region 익명 로그인
    public void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                user = task.Result.User;
                if (user != null)
                {
                    SaveUserDocument(user.UserId);
                }
            }
            else
            {
                Debug.LogError("익명 로그인 실패: " + task.Exception?.Message);
            }
        });
    }
    #endregion

    #region 공통 유틸
    private void SaveUserDocument(string userId)
    {
        if (firestore == null)
        {
            Debug.LogError("Firestore not initialized.");
            return;
        }

        var userDoc = firestore.Collection("users").Document(userId);
        var data = new Dictionary<string, object>
        {
            { "userId", userId },
            { "createdAt", FieldValue.ServerTimestamp }
        };

        userDoc.SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                Debug.Log($"User document {userId} 저장/병합 성공");
                OnLoginSuccess();
            }
            else
                Debug.LogError($"User document 저장 실패: {t.Exception?.Message}");
        });
    }

    private void OnLoginSuccess()
    {
        GameManager.Instance.GetUserData();
        loginTasksCompleted.TrySetResult(true);
    }

    public void OnDisconnect()
    {
        if (user == null)
        {
            Debug.LogWarning("로그인된 유저 없음.");
            return;
        }

        Debug.Log(user.IsAnonymous
            ? "Disconnecting Anonymous User."
            : "Disconnecting Google User.");

        auth.SignOut();
        user = null;
    }
    #endregion
}