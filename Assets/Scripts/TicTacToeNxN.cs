using System;
using System.Collections;
using UnityEngine;

public class TicTacToeNxN : MonoBehaviour
{
    private int N;
    [SerializeField] private GameObject cubePrefab; // 에디터에서 할당
    public GameObject[,,] cubes;
    public Vector3[,,] originalPositions;
    public int[,,] board; // 0: 비어있음, 1: O, 2: X
    public bool isOTurn;
    public bool gameOver = false; // 게임 종료 상태
    public bool isExpanded = false; // 큐브가 펼쳐진 상태인지 여부
    private bool isMoving = false; // 코루틴 실행 여부 확인
    public event Action OnAITurnStarted; // AI 턴 시작 이벤트
    public bool isAI = false;
    [SerializeField] private GameUI gameUI;

    // 중앙 값 계산 (모든 큐브의 원래 위치의 평균)
    public Vector3 centerPosition = Vector3.zero;
    int cubeCount = 0;
    private void Awake()
    {
        N = GameManager.Instance.CurrentVersion;

        cubes = new GameObject[N, N, N];
        board = new int[N, N, N]; // 0: 비어있음, 1: O, 2: X
        originalPositions = new Vector3[N, N, N]; // 큐브의 원래 위치 저장
        GameManager.Instance.tictactoe = this;
        float spacing = 1.1f;
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                for (int z = 0; z < N; z++)
                {
                    Vector3 pos = new Vector3(x * spacing, y * spacing, z * spacing);
                    GameObject cube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    cubes[x, y, z] = cube;
                    originalPositions[x, y, z] = pos; // 원래 위치 저장
                    cube.AddComponent<CubeClickHandler>().Init(this, x, y, z);
                    centerPosition += originalPositions[x, y, z];
                    cubeCount++;
                }
            }
        }
        if (cubeCount > 0)
        {
            centerPosition /= cubeCount; // 평균 위치 계산
        }
        // 초기 턴 설정
        isOTurn = GameManager.Instance.isOTurnFirst;
        if (GameManager.Instance.IsAIMode())
        {
            isAI = true;
        }
        gameUI.StartTurnTimer();// 첫 턴 타이머 시작
    }

    public void MoveCube()
    {
        if (!isMoving)
        {
            isExpanded = !isExpanded; // 상태 전환
            StartCoroutine(MoveCubes());
        }
    }

    private IEnumerator MoveCubes()
    {
        isMoving = true; // 코루틴 실행 중 상태 설정
        float elapsedTime = 0f;
        float duration = 2f; // 이동 시간
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / duration); // 부드러운 보간

            for (int x = 0; x < N; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    for (int z = 0; z < N; z++)
                    {
                        GameObject cube = cubes[x, y, z];
                        if (cube != null)
                        {
                            Vector3 targetPosition = originalPositions[x, y, z];
                            if (isExpanded)
                            {
                                // 중앙 값 기준으로 상대적인 위치를 유지하며 거리 증가
                                Vector3 relativePosition = originalPositions[x, y, z] - centerPosition;
                                targetPosition = centerPosition + relativePosition * 2.5f; // 거리 비율을 2.5배로 증가
                            }

                            // Lerp를 사용하여 부드럽게 이동
                            cube.transform.position = Vector3.Lerp(cube.transform.position, targetPosition, t);
                        }
                    }
                }
            }

            yield return null; // 다음 프레임까지 대기
        }

        isMoving = false; // 코루틴 실행 완료 상태 설정
    }

    public void OnCubeClicked(int x, int y, int z, GameObject cube)
    {
        if (gameOver) return; // 게임이 끝났으면 무시
        if (board[x, y, z] != 0) return; // 이미 선택된 칸은 무시

        board[x, y, z] = isOTurn ? 1 : 2;
        Cube cubeObj = cube.GetComponent<Cube>();
        cubeObj.cubeMesh.enabled = false;

        if (isOTurn)
        {
            Transform oObj = cubeObj.oObj;
            oObj.gameObject.SetActive(isOTurn);
        }
        else
        {
            Transform xObj = cubeObj.xObj;
            xObj.gameObject.SetActive(!isOTurn);
        }

        int player = isOTurn ? 1 : 2;
        int completedLines = CheckCompletedLines(player);
        if (completedLines == 1)
        {
            gameOver = true;
            gameUI.GameResult();
        }
        else
        {
            isOTurn = !isOTurn;
            if (isAI && !isOTurn)
            {
                OnAITurnStarted?.Invoke();
            }
            gameUI.StartTurnTimer(); // 다음 턴 타이머 시작
        }
    }

    public int CheckCompletedLines(int player)
    {
        int lines = 0;

        // 각 축에 대해 체크
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                // x축, y축, z축
                if (IsLineCompleted(player, 0, i, j, 1, 0, 0)) lines++;
                if (IsLineCompleted(player, i, 0, j, 0, 1, 0)) lines++;
                if (IsLineCompleted(player, i, j, 0, 0, 0, 1)) lines++;
            }
        }

        // 대각선 체크 (2D 평면 대각선)
        for (int i = 0; i < N; i++)
        {
            if (IsLineCompleted(player, 0, 0, i, 1, 1, 0)) lines++;
            if (IsLineCompleted(player, 0, N - 1, i, 1, -1, 0)) lines++;
            if (IsLineCompleted(player, 0, i, 0, 1, 0, 1)) lines++;
            if (IsLineCompleted(player, 0, i, N - 1, 1, 0, -1)) lines++;
            if (IsLineCompleted(player, i, 0, 0, 0, 1, 1)) lines++;
            if (IsLineCompleted(player, i, 0, N - 1, 0, 1, -1)) lines++;
        }

        // 3D 대각선
        if (IsLineCompleted(player, 0, 0, 0, 1, 1, 1)) lines++;
        if (IsLineCompleted(player, 0, 0, N - 1, 1, 1, -1)) lines++;
        if (IsLineCompleted(player, 0, N - 1, 0, 1, -1, 1)) lines++;
        if (IsLineCompleted(player, 0, N - 1, N - 1, 1, -1, -1)) lines++;

        return lines;
    }

    private bool IsLineCompleted(int player, int startX, int startY, int startZ, int stepX, int stepY, int stepZ)
    {
        for (int k = 0; k < N; k++)
        {
            int x = startX + stepX * k;
            int y = startY + stepY * k;
            int z = startZ + stepZ * k;

            if (board[x, y, z] != player)
            {
                return false;
            }
        }
        return true;
    }
}