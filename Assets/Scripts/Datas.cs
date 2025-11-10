using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using UnityEngine;

public class Datas : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static Datas Instance { get; private set; }

    private FirebaseFirestore db;

    #region Unity Lifecycle
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
    }

    private void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
    }
    #endregion

    #region Firestore Methods
    public void WriteData()
    {
        if (LoginManager.user == null)
        {
            Debug.LogWarning("LoginManager.user가 null입니다.");
            return;
        }

        Debug.Log("데이터를 저장합니다");
        DocumentReference docRef = db.Collection("users").Document(LoginManager.user.UserId);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"ID", LoginManager.user.UserId }
        };

        docRef.SetAsync(data, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
                Debug.Log("쓰기 완료");
            else
                Debug.LogError($"쓰기 실패: {task.Exception}");
        });
    }

    public void ReadData()
    {
        Debug.Log("데이터를 불러옵니다");
        CollectionReference userRef = db.Collection("users");

        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompletedSuccessfully)
            {
                Debug.LogError($"읽기 실패: {task.Exception}");
                return;
            }

            QuerySnapshot snapshot = task.Result;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.TryGetValue("ID", out string id))
                {
                    Debug.LogFormat("userId: {0}, ID: {1}", document.Id, id);
                }
                else
                {
                    Debug.LogFormat("userId: {0}, ID 없음", document.Id);
                }
            }
        });
    }
    #endregion
}
