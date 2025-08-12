using Firebase;
using Firebase.Auth;
using Firebase.Database; // Firebase Realtime Database 추가
using Firebase.Extensions;
using Google;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; } // 싱글톤 인스턴스

    public TextMeshProUGUI infoText;
    public string webClientId = "547845580475-6re3mh59m484pu68thvgpc896v6gtogi.apps.googleusercontent.com";

    private FirebaseAuth auth;
    private FirebaseDatabase database; // Firebase Realtime Database 인스턴스
    private GoogleSignInConfiguration configuration;

    public static FirebaseUser user;

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지
        }
        else
        {
            Destroy(gameObject); // 중복 인스턴스 제거
            return;
        }

        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();
    }

    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    Debug.Log("Firebase dependencies are available.");
                    FirebaseApp app = FirebaseApp.DefaultInstance;

                    // FirebaseApp 초기화 후 DatabaseUrl 설정
                    if (app.Options.DatabaseUrl == null)
                    {
                        app.Options.DatabaseUrl = new Uri("https://cubetictactoe-default-rtdb.firebaseio.com");
                    }
                    auth = FirebaseAuth.DefaultInstance;
                    database = FirebaseDatabase.DefaultInstance; // Realtime Database 초기화
                }
                else
                {
                    Debug.Log("Firebase dependencies are not available: " + task.Result.ToString());
                }
            }
            else if (task.IsFaulted)
            {
                Debug.Log("Failed to check Firebase dependencies. Exception: " + task.Exception?.ToString());
            }
            else if (task.IsCanceled)
            {
                Debug.Log("Firebase dependency check was canceled.");
            }
        });
    }

    public void SignInWithGoogle() { OnSignIn(); }
    public void SignOutFromGoogle() { OnSignOut(); }

    private void OnSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        Debug.Log("Calling SignIn");
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        Debug.Log("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        Debug.Log("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    Debug.Log("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    Debug.Log("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Canceled");
        }
        else
        {
            SignInWithGoogleOnFirebase(task.Result.IdToken);
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Task Status: IsFaulted={task.IsFaulted}, IsCanceled={task.IsCanceled}, IsCompleted={task.IsCompleted}");
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("SignInWithCredentialAsync failed.");
            }
            else
            {
                user = task.Result;

                // Firebase Realtime Database에 user.UserId 저장
                SaveUserIdToDatabase(user.UserId);

                GameManager.Instance.GameSet();
                GameManager.Instance.GetUserData();
            }
        });
    }

    private void SaveUserIdToDatabase(string userId)
    {
        if (database != null)
        {
            DatabaseReference userRef = FirebaseDatabase.DefaultInstance.GetReference($"users/{userId}");
            userRef.Child("userId").SetValueAsync(userId).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"User ID {userId} successfully saved to Firebase Realtime Database.");
                }
                else
                {
                    Debug.LogError($"Failed to save User ID {userId} to Firebase Realtime Database: {task.Exception?.Message}");
                }
            });
        }
        else
        {
            Debug.LogError("Firebase Realtime Database is not initialized.");
        }
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        Debug.Log("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Anonymous Sign-In failed: " + task.Exception?.Message);
            }
            else
            {
                user = task.Result.User;

                // Firebase Realtime Database에 user.UserId 저장
                SaveUserIdToDatabase(user.UserId);

                Debug.Log("Anonymous Sign-In Successful.");
                GameManager.Instance.GameSet();
                GameManager.Instance.GetUserData();
            }
        });
    }
}