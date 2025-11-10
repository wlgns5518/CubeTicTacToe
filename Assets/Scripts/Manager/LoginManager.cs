using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Google;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    [Header("Google Sign-In Settings")]
    [SerializeField]
    private string webClientId =
        "547845580475-mp6sgdqiqc439t0jme0hsv03dfe8e8re.apps.googleusercontent.com";

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private GoogleSignInConfiguration configuration;

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

        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestEmail = true,
            RequestIdToken = true
        };
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
                AutoLogin();
            }
            else
            {
                Debug.LogError($"Firebase dependencies not available: {task.Result}");
            }
        });
    }

    private void AutoLogin()
    {
        if (!PlayerPrefs.HasKey("UserId"))
        {
            Debug.Log("자동 로그인 실패: 저장된 정보 없음.");
            return;
        }

        // 메인 스레드에서 후속 처리가 수행되도록 변경
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                user = auth.CurrentUser;
                // 자동 로그인 경로에서도 Firestore 사용자 문서를 저장하도록 추가
                if (user != null)
                {
                    SaveUserDocument(user.UserId);
                }
            }
            else
            {
                Debug.LogError("자동 로그인 실패: " + task.Exception?.Message);
            }
        });
    }
    #endregion

    #region Google 로그인
    public void SignInWithGoogle() => SignIn(false);
    public void OnSignInSilently() => SignIn(true);
    public void SignOutFromGoogle() => GoogleSignIn.DefaultInstance.SignOut();

    private void SignIn(bool silent)
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        Debug.Log(silent ? "Calling SignIn Silently" : "Calling SignIn");

        var signInTask = silent
            ? GoogleSignIn.DefaultInstance.SignInSilently()
            : GoogleSignIn.DefaultInstance.SignIn();

        signInTask.ContinueWith(OnAuthenticationFinished);
    }

    private void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            foreach (var e in task.Exception.InnerExceptions)
                Debug.LogError("Google Sign-In Error: " + e.Message);
        }
        else if (task.IsCanceled)
        {
            Debug.LogWarning("Google Sign-In Canceled");
        }
        else
        {
            SignInWithGoogleOnFirebase(task.Result.IdToken);
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        var credential = GoogleAuthProvider.GetCredential(idToken, null);

        // 메인 스레드에서 후속 처리 실행
        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                user = task.Result;

                if (user != null)
                {
                    SaveUserDocument(user.UserId);
                }
            }
            else
            {
                Debug.LogError("Google Firebase 로그인 실패: " + task.Exception?.Message);
            }
        });
    }
    #endregion

    #region 익명 로그인
    public void SignInAnonymously()
    {
        // 메인 스레드에서 후속 처리 실행
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                // 사용하는 SDK 버전에 따라 Result 타입이 다를 수 있으므로 기존 코드를 유지
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

        if (user.IsAnonymous)
        {
            Debug.Log("Disconnecting Anonymous User.");
        }
        else
        {
            Debug.Log("Disconnecting Google User.");
            GoogleSignIn.DefaultInstance.Disconnect();
        }
    }
    #endregion
}