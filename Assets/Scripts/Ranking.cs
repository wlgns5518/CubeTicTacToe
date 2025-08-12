using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ranking : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText; // 리더보드를 표시할 Text
    public TextMeshProUGUI myRankText; // 내 랭킹을 표시할 Text

    private DatabaseReference databaseRef; // Firebase Realtime Database 레퍼런스

    private void Start()
    {
        // Firebase Realtime Database 레퍼런스 초기화
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        // 리더보드 데이터 로드 및 UI 업데이트
        LoadLeaderboardData();
    }
    private void LoadLeaderboardData()
    {
        // 리더보드 데이터 조회
        databaseRef.Child("users").OrderByChild("playerScore").LimitToLast(10).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to load leaderboard data.");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string leaderboardData = "";

                // 점수 내림차순으로 정렬
                List<DataSnapshot> sortedList = new List<DataSnapshot>(snapshot.Children);
                sortedList.Sort((a, b) => int.Parse(b.Child("playerScore").Value.ToString())
                                          .CompareTo(int.Parse(a.Child("playerScore").Value.ToString())));

                int rank = 1;
                foreach (var childSnapshot in sortedList)
                {
                    string userId = childSnapshot.Child("userId").Value.ToString();
                    int score = int.Parse(childSnapshot.Child("playerScore").Value.ToString());

                    leaderboardData += $"{rank}. {score}\n";
                    rank++;
                }

                leaderboardText.text = leaderboardData;

                // 내 랭킹 데이터 로드
                LoadMyRankData();
            }
        });
    }

    private void LoadMyRankData()
    {
        string userId = LoginManager.user.UserId;

        databaseRef.Child("users").OrderByChild("playerScore").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to load my rank data.");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // 점수 내림차순으로 정렬
                List<DataSnapshot> sortedList = new List<DataSnapshot>(snapshot.Children);
                sortedList.Sort((a, b) => int.Parse(b.Child("playerScore").Value.ToString())
                                          .CompareTo(int.Parse(a.Child("playerScore").Value.ToString())));

                int rank = 1;
                foreach (var childSnapshot in sortedList)
                {
                    string currentUserId = childSnapshot.Child("userId").Value.ToString();
                    if (currentUserId == userId)
                    {
                        myRankText.text = $"내 랭킹: {rank}";
                        return;
                    }
                    rank++;
                }

                myRankText.text = "내 랭킹: 데이터 없음";
            }
        });
    }
}