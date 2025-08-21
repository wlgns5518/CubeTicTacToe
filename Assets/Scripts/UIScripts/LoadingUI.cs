using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public Slider loadingBar;
    public float fillSpeed = 1f; // 초당 로딩 바가 채워지는 속도
    private bool isTasksCompleted = false;

    private async void Start()
    {
        // LoginManager와 GameManager의 작업 완료를 기다림
        Task loginTask = LoginManager.Instance.LoginTasksCompleted;
        Task gameTask = GameManager.Instance.GameTasksCompleted;

        // 두 작업이 완료될 때까지 대기
        await Task.WhenAll(loginTask, gameTask);
        // 나머지 10%를 채움
        isTasksCompleted = true;
        PlayerPrefs.SetString("UserId",LoginManager.user.UserId);
    }

    private void Update()
    {
        // 모든 작업이 완료되면 로딩 바를 100%로 채움
        if (isTasksCompleted && loadingBar.value < 1f)
        {
            loadingBar.value += fillSpeed * Time.deltaTime;
        }
        if (isTasksCompleted && loadingBar.value >= 1f)
        {
            GameManager.Instance.GameSet();
        }
    }
}