using Firebase;
using Firebase.Auth;
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
    private GoogleSignInConfiguration configuration;

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
                    AddToInformation("Firebase dependencies are available.");
                    auth = FirebaseAuth.DefaultInstance;
                }
                else
                {
                    AddToInformation("Firebase dependencies are not available: " + task.Result.ToString());
                }
            }
            else if (task.IsFaulted)
            {
                AddToInformation("Failed to check Firebase dependencies. Exception: " + task.Exception?.ToString());
            }
            else if (task.IsCanceled)
            {
                AddToInformation("Firebase dependency check was canceled.");
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
        AddToInformation("Calling SignIn");
        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    private void OnSignOut()
    {
        AddToInformation("Calling SignOut");
        GoogleSignIn.DefaultInstance.SignOut();
    }

    public void OnDisconnect()
    {
        AddToInformation("Calling Disconnect");
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
                    AddToInformation("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            AddToInformation("Canceled");
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
            AddToInformation($"Task Status: IsFaulted={task.IsFaulted}, IsCanceled={task.IsCanceled}, IsCompleted={task.IsCompleted}");
            if (task.IsFaulted || task.IsCanceled)
            {
                AddToInformation("SignInWithCredentialAsync failed.");
            }
            else
            {
                AddToInformation($"Sign In Successful. User: {task.Result.DisplayName}, Email: {task.Result.Email}");
                GameManager.Instance.GameSet();
            }
        });
    }

    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToInformation("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void RegisterWithEmail(string email,string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            AddToInformation("Email or Password is empty.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                AddToInformation("Registration failed: " + task.Exception?.Message);
            }
            else
            {
                AddToInformation($"Registration successful. User: {email}");
            }
        });
    }

    public void SignInWithEmail(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            AddToInformation("Email or Password is empty.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                AddToInformation("Sign-In failed: " + task.Exception?.Message);
            }
            else
            {
                AddToInformation($"Sign-In successful. User: {email}");
                GameManager.Instance.GameSet();
            }
        });
    }
    public void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                AddToInformation("Anonymous Sign-In failed: " + task.Exception?.Message);
            }
            else
            {
                AddToInformation("Anonymous Sign-In Successful.");
                GameManager.Instance.GameSet();
            }
        });
    }
    private void AddToInformation(string str) { infoText.text += "\n" + str; }
}