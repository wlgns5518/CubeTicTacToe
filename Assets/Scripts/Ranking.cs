using Firebase.Database;
using Firebase.Extensions;
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
        leaderboardText.text = "Loading leaderboard..."; // 초기화 메시지
        databaseRef.Child("users").OrderByChild("playerScore").LimitToLast(5).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to load leaderboard data: {task.Exception}");
                leaderboardText.text = "Failed to load leaderboard.";
                return;
            }
            if (task.IsCanceled)
            {
                Debug.LogError("Leaderboard data loading was canceled.");
                leaderboardText.text = "Loading canceled.";
                return;
            }
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot == null || !snapshot.HasChildren)
                {
                    leaderboardText.text = "No leaderboard data available.";
                    return;
                }

                string leaderboardData = "";
                List<DataSnapshot> sortedList = new List<DataSnapshot>(snapshot.Children);
                sortedList.Sort((a, b) => int.Parse(b.Child("playerScore").Value.ToString())
                                          .CompareTo(int.Parse(a.Child("playerScore").Value.ToString())));

                int rank = 1;
                foreach (var childSnapshot in sortedList)
                {
                    if (childSnapshot.Child("playerScore").Value == null || childSnapshot.Child("userId").Value == null)
                    {
                        Debug.LogWarning("Invalid data found in leaderboard.");
                        continue;
                    }

                    string userId = childSnapshot.Child("userId").Value.ToString();
                    int score = int.Parse(childSnapshot.Child("playerScore").Value.ToString());

                    leaderboardData += $"{rank}. score : {score}\n";
                    rank++;
                }

                leaderboardText.text = leaderboardData;
                LoadMyRankData();
            }
        });
    }

    private void LoadMyRankData()
    {
        myRankText.text = "Loading my rank..."; // 초기화 메시지
        string userId = LoginManager.user.UserId;

        databaseRef.Child("users").OrderByChild("playerScore").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Failed to load my rank data: {task.Exception}");
                myRankText.text = "Failed to load my rank.";
                return;
            }
            if (task.IsCanceled)
            {
                Debug.LogError("My rank data loading was canceled.");
                myRankText.text = "Loading canceled.";
                return;
            }
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot == null || !snapshot.HasChildren)
                {
                    myRankText.text = "No rank data available.";
                    return;
                }

                List<DataSnapshot> sortedList = new List<DataSnapshot>(snapshot.Children);
                sortedList.Sort((a, b) => int.Parse(b.Child("playerScore").Value.ToString())
                                          .CompareTo(int.Parse(a.Child("playerScore").Value.ToString())));

                int rank = 1;
                foreach (var childSnapshot in sortedList)
                {
                    if (childSnapshot.Child("userId").Value == null)
                    {
                        Debug.LogWarning("Invalid user data found.");
                        continue;
                    }

                    string currentUserId = childSnapshot.Child("userId").Value.ToString();
                    if (currentUserId == userId)
                    {
                        myRankText.text = $"My Ranking: {rank}";
                        return;
                    }
                    rank++;
                }

                myRankText.text = "My Ranking: No Data";
            }
        });
    }
}