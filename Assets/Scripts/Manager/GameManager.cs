using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private int currentMode; // 현재 게임 모드 저장
    private int currentVersion;
    public int CurrentVersion => currentVersion + 3;
    public bool isOTurnFirst; // O가 먼저 시작하는지 여부
    void Awake()
    {
        // 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void GameStart(int version, int mode)
    {
        currentMode = mode; // 현재 모드 저장
        currentVersion = version;
        // O와 X 순서를 랜덤으로 배정
        isOTurnFirst = Random.value > 0.5f;
        if (version == 0)
        {
            // 3x3 게임을 위한 씬 번호 1 실행
            SceneManager.LoadScene(1);
        }
        else if (version == 1)
        {
            // 4x4 게임을 위한 씬 번호 2 실행
            SceneManager.LoadScene(2);
        }
    }
    public void GameSet()
    {
        SceneManager.LoadScene(0);
    }

    public bool IsAIMode()
    {
        return currentMode == 0; // 현재 모드가 AI 대결인지 확인
    }
}