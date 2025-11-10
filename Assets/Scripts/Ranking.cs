using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class Ranking : MonoBehaviour
{
    public TextMeshProUGUI leaderboardText;
    public TextMeshProUGUI myRankText;

    private FirebaseFirestore firestore;

    private void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        LoadLeaderboardData();
    }

    private void LoadLeaderboardData()
    {
        leaderboardText.text = "Loading leaderboard...";
        firestore.Collection("users")
            .OrderByDescending("playerScore")
            .Limit(5)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Failed to load leaderboard data: {task.Exception}");
                    leaderboardText.text = "Failed to load leaderboard.";
                    return;
                }
                if (task.IsCanceled)
                {
                    leaderboardText.text = "Loading canceled.";
                    return;
                }

                var snapshot = task.Result;
                if (snapshot == null || snapshot.Count == 0)
                {
                    leaderboardText.text = "No leaderboard data available.";
                    return;
                }

                string leaderboardData = "";
                int rank = 1;
                foreach (var doc in snapshot.Documents)
                {
                    if (!doc.TryGetValue("playerScore", out int score)) continue;
                    leaderboardData += $"{rank}. score : {score}\n";
                    rank++;
                }

                leaderboardText.text = leaderboardData;
                LoadMyRankData();
            });
    }

    private void LoadMyRankData()
    {
        myRankText.text = "Loading my rank...";
        if (LoginManager.user == null)
        {
            myRankText.text = "Not logged in.";
            return;
        }

        string userId = LoginManager.user.UserId;

        // 모든 사용자 점수 가져와서 순위 계산 (데이터 많아지면 Cloud Function/집계 컬렉션 고려)
        firestore.Collection("users")
            .OrderByDescending("playerScore")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"Failed to load rank list: {task.Exception}");
                    myRankText.text = "Failed to load my rank.";
                    return;
                }
                if (task.IsCanceled)
                {
                    myRankText.text = "Loading canceled.";
                    return;
                }

                var snapshot = task.Result;
                if (snapshot == null || snapshot.Count == 0)
                {
                    myRankText.text = "No rank data available.";
                    return;
                }

                int rank = 1;
                foreach (var doc in snapshot.Documents)
                {
                    if (!doc.TryGetValue("userId", out string currentUserId)) continue;
                    if (currentUserId == userId)
                    {
                        myRankText.text = $"My Ranking: {rank}";
                        return;
                    }
                    rank++;
                }

                myRankText.text = "My Ranking: No Data";
            });
    }
}