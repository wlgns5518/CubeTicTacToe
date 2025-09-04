using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Google;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; } // 싱글톤 인스턴스

    [Header("Google Sign-In Settings")]
    [SerializeField]
    private string webClientId =
        "547845580475-6re3mh59m484pu68thvgpc896v6gtogi.apps.googleusercontent.com";

    private FirebaseAuth auth;
    private FirebaseDatabase database;
    private GoogleSignInConfiguration configuration;

    public static FirebaseUser user { get; private set; }

    private readonly TaskCompletionSource<bool> loginTasksCompleted = new TaskCompletionSource<bool>();
    public Task LoginTasksCompleted => loginTasksCompleted.Task;

    private void Awake()
    {
        // 싱글톤 설정
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

        // Firebase 준비
        CheckFirebaseDependencies();

        // Google 로그인 설정
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

                if (app.Options.DatabaseUrl == null)
                {
                    app.Options.DatabaseUrl = new Uri("https://cubetictactoe-default-rtdb.firebaseio.com");
                }

                auth = FirebaseAuth.DefaultInstance;
                database = FirebaseDatabase.DefaultInstance;

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

        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                user = auth.CurrentUser;
                OnLoginSuccess("자동 로그인 성공");
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

        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                user = task.Result;
                SaveUserIdToDatabase(user.UserId);
                OnLoginSuccess("Google 로그인 성공");
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
        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                user = task.Result.User;
                SaveUserIdToDatabase(user.UserId);
                OnLoginSuccess("익명 로그인 성공");
            }
            else
            {
                Debug.LogError("익명 로그인 실패: " + task.Exception?.Message);
            }
        });
    }
    #endregion

    #region 공통 유틸
    private void SaveUserIdToDatabase(string userId)
    {
        if (database == null)
        {
            Debug.LogError("Firebase Database not initialized.");
            return;
        }

        var userRef = database.GetReference($"users/{userId}");
        userRef.Child("userId").SetValueAsync(userId).ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log($"User ID {userId} 저장 성공");
            else
                Debug.LogError($"User ID {userId} 저장 실패: {task.Exception?.Message}");
        });
    }

    private void OnLoginSuccess(string message)
    {
        Debug.Log(message);
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
            FirebaseDatabase.DefaultInstance.GoOffline();
        }
        else
        {
            Debug.Log("Disconnecting Google User.");
            GoogleSignIn.DefaultInstance.Disconnect();
        }
    }
    #endregion
}
