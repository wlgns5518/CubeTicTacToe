using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    private const string PlayerPrefsUserIdKey = "UserId";

    public static LoginManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    public static FirebaseUser user { get; private set; }

    [SerializeField] private LoginUI loginUI;

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
        Debug.Log("[Firebase] 의존성 확인 시작...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"[Firebase] 의존성 확인 중 예외 발생: {task.Exception}");
                return;
            }

            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("[Firebase] 초기화 완료.");
                auth = FirebaseAuth.DefaultInstance;
                firestore = FirebaseFirestore.DefaultInstance;

                Debug.Log($"[Firebase] Auth 인스턴스: {(auth != null ? "OK" : "NULL")}");
                Debug.Log($"[Firebase] Firestore 인스턴스: {(firestore != null ? "OK" : "NULL")}");

                TryAutoLogin();
            }
            else
            {
                Debug.LogError($"[Firebase] 의존성 해결 실패 - 상태: {task.Result}");
            }
        });
    }

    private void TryAutoLogin()
    {
        Debug.Log("[AutoLogin] 자동 로그인 시도...");

        if (auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            Debug.Log($"[AutoLogin] 성공 - UserId: {user.UserId}, Anonymous: {user.IsAnonymous}, Provider: {user.ProviderId}");
            SaveUserDocument(user.UserId);
        }
        else
        {
            Debug.Log("[AutoLogin] 저장된 세션 없음 → 로그인 UI 표시");
            loginUI.Login();
        }
    }
    #endregion

    #region Google 로그인 (Firebase Federated OAuth)
    public void SignInWithGoogle()
    {
        if (auth == null)
        {
            Debug.LogError("[Google] Firebase Auth가 초기화되지 않았습니다. CheckFirebaseDependencies()가 완료되었는지 확인하세요.");
            return;
        }

        Debug.Log($"[Google] SignInWithGoogle() 호출 - ProviderId: {GoogleAuthProvider.ProviderId}");

        var providerData = new FederatedOAuthProviderData
        {
            ProviderId = GoogleAuthProvider.ProviderId,
            Scopes = new List<string> { "email", "profile" }
        };

        Debug.Log($"[Google] FederatedOAuthProviderData 생성 완료 - Scopes: {string.Join(", ", providerData.Scopes)}");

        var provider = new FederatedOAuthProvider();
        provider.SetProviderData(providerData);

        Debug.Log("[Google] SignInWithProviderAsync 호출 중...");

        auth.SignInWithProviderAsync(provider).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"[Google] 로그인 결과 수신 - IsFaulted: {task.IsFaulted}, IsCanceled: {task.IsCanceled}, IsCompleted: {task.IsCompleted}");

            if (task.IsFaulted)
            {
                Debug.LogError("[Google] 로그인 실패 (IsFaulted)");
                if (task.Exception != null)
                {
                    Debug.LogError($"[Google] AggregateException: {task.Exception.Message}");
                    foreach (var inner in task.Exception.InnerExceptions)
                    {
                        Debug.LogError($"[Google] InnerException 타입: {inner.GetType().Name}");
                        Debug.LogError($"[Google] InnerException 메시지: {inner.Message}");

                        if (inner is FirebaseException firebaseEx)
                            Debug.LogError($"[Google] Firebase 에러 코드: {firebaseEx.ErrorCode}");
                    }
                }
                return;
            }

            if (task.IsCanceled)
            {
                Debug.LogWarning("[Google] 로그인 취소됨 (IsCanceled) - 사용자가 팝업을 닫았거나 플랫폼에서 지원하지 않을 수 있습니다.");
                return;
            }

            var result = task.Result;
            if (result == null)
            {
                Debug.LogError("[Google] task.Result가 null입니다.");
                return;
            }

            user = result.User;
            if (user != null)
            {
                Debug.Log($"[Google] 로그인 성공 - UserId: {user.UserId}, Email: {user.Email}, DisplayName: {user.DisplayName}");
                Debug.Log($"[Google] Provider: {user.ProviderId}, IsAnonymous: {user.IsAnonymous}");
                SaveUserDocument(user.UserId);
            }
            else
            {
                Debug.LogError("[Google] 로그인은 성공했으나 User 객체가 null입니다.");
            }
        });
    }

    public void SignOutFromGoogle()
    {
        if (auth != null)
        {
            Debug.Log("[Google] 로그아웃 시작...");
            ClearUserPrefs();
            auth.SignOut();
            user = null;
            Debug.Log("[Google] 로그아웃 완료");
        }
        else
        {
            Debug.LogWarning("[Google] 로그아웃 시도했으나 auth가 null입니다.");
        }
    }
    #endregion

    #region 익명 로그인
    public void SignInAnonymously()
    {
        Debug.Log("[Anonymous] 익명 로그인 시도...");

        if (auth == null)
        {
            Debug.LogError("[Anonymous] Firebase Auth가 초기화되지 않았습니다.");
            return;
        }

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log($"[Anonymous] 결과 수신 - IsFaulted: {task.IsFaulted}, IsCanceled: {task.IsCanceled}");

            if (task.IsCanceled)
            {
                Debug.LogWarning("[Anonymous] 익명 로그인 취소됨");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError($"[Anonymous] 익명 로그인 실패: {task.Exception?.Message}");
                if (task.Exception != null)
                {
                    foreach (var inner in task.Exception.InnerExceptions)
                    {
                        Debug.LogError($"[Anonymous] InnerException: {inner.Message}");
                        if (inner is FirebaseException firebaseEx)
                            Debug.LogError($"[Anonymous] Firebase 에러 코드: {firebaseEx.ErrorCode}");
                    }
                }
                return;
            }

            user = task.Result.User;
            if (user != null)
            {
                Debug.Log($"[Anonymous] 로그인 성공 - UserId: {user.UserId}");
                SaveUserDocument(user.UserId);
            }
            else
            {
                Debug.LogError("[Anonymous] 로그인 성공했으나 User 객체가 null입니다.");
            }
        });
    }
    #endregion

    #region 공통 유틸
    private void SaveUserDocument(string userId)
    {
        if (firestore == null)
        {
            Debug.LogError("[Firestore] 초기화되지 않았습니다. SaveUserDocument() 실패.");
            return;
        }

        Debug.Log($"[Firestore] 유저 문서 저장 시작 - UserId: {userId}");

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
                Debug.Log($"[Firestore] 유저 문서 저장/병합 성공 - UserId: {userId}");
                SaveUserPrefs(userId);
                OnLoginSuccess();
            }
            else
            {
                Debug.LogError($"[Firestore] 유저 문서 저장 실패 - UserId: {userId}");
                Debug.LogError($"[Firestore] 예외: {t.Exception?.Message}");
                if (t.Exception != null)
                {
                    foreach (var inner in t.Exception.InnerExceptions)
                        Debug.LogError($"[Firestore] InnerException: {inner.Message}");
                }
            }
        });
    }

    private void SaveUserPrefs(string userId)
    {
        PlayerPrefs.SetString(PlayerPrefsUserIdKey, userId);
        PlayerPrefs.Save();
        Debug.Log($"[PlayerPrefs] UserId 저장 완료 - {userId}");
    }

    private void ClearUserPrefs()
    {
        PlayerPrefs.DeleteKey(PlayerPrefsUserIdKey);
        PlayerPrefs.Save();
        Debug.Log("[PlayerPrefs] UserId 삭제 완료");
    }

    private void OnLoginSuccess()
    {
        Debug.Log("[Login] OnLoginSuccess() 호출 - GameManager.GetUserData() 실행");
        GameManager.Instance.GetUserData();
        loginTasksCompleted.TrySetResult(true);
    }

    public void OnDisconnect()
    {
        if (user == null)
        {
            Debug.LogWarning("[Disconnect] 로그인된 유저 없음.");
            return;
        }

        Debug.Log(user.IsAnonymous
            ? "[Disconnect] 익명 유저 연결 해제"
            : $"[Disconnect] Google 유저 연결 해제 - UserId: {user.UserId}");

        ClearUserPrefs();
        auth.SignOut();
        user = null;
        Debug.Log("[Disconnect] 완료");
    }
    #endregion
}