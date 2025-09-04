using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class TicTacToeNxN : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameUI gameUI;
    [SerializeField] private CameraMoveAndroid CameraMoveAndroid;

    public GameObject[,,] cubes;
    public int[,,] board;                       // 0: empty, 1: O, 2: X
    public bool isOTurn;
    public bool gameOver = false;
    public bool isExpanded = false;
    public bool isPlayerO;
    public event Action OnAITurnStarted;

    public Vector3 centerPosition = Vector3.zero;

    private int N;
    private bool isMoving = false;
    private bool isAI = false;

    // 캐싱/애니메이션용 데이터
    private Vector3[,,] originalPositions;
    private Vector3[,,] expandedPositions;
    private Vector3[,,] fromPositions;          // 애니메이션 시작 시 위치 스냅샷
    private Cube[,,] cubeComps;                 // Cube 컴포넌트 캐시
    private const float spacing = 1.1f;
    private const float expandFactor = 2.5f;
    private const float moveDuration = 0.5f;

    private void Awake()
    {
        N = GameManager.Instance.CurrentVersion;

        cubes = new GameObject[N, N, N];
        board = new int[N, N, N];

        originalPositions = new Vector3[N, N, N];
        expandedPositions = new Vector3[N, N, N];
        fromPositions = new Vector3[N, N, N];
        cubeComps = new Cube[N, N, N];

        GameManager.Instance.tictactoe = this;

        // 큐브 생성 & 기본 정보 셋업
        int count = 0;
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                for (int z = 0; z < N; z++)
                {
                    Vector3 pos = new Vector3(x * spacing, y * spacing, z * spacing);
                    GameObject cube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);

                    cubes[x, y, z] = cube;
                    originalPositions[x, y, z] = pos;
                    centerPosition += pos;
                    cubeComps[x, y, z] = cube.GetComponent<Cube>(); // 매번 GetComponent 방지

                    // 클릭 핸들러 부착
                    cube.AddComponent<CubeClickHandler>().Init(x, y, z);

                    count++;
                }
            }
        }
        if (count > 0) centerPosition /= count;

        // 확장 위치 미리 계산 (매 프레임 계산 방지)
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                for (int z = 0; z < N; z++)
                {
                    Vector3 rel = originalPositions[x, y, z] - centerPosition;
                    expandedPositions[x, y, z] = centerPosition + rel * expandFactor;
                }
            }
        }
    }

    private void Start()
    {
        // 초기 턴
        isOTurn = GameManager.Instance.isOTurnFirst;

        // 모드 파악
        isAI = GameManager.Instance.IsAIMode();

        if (isAI)
        {
            gameUI.UpdateInfoText("your turn");
        }
        else
        {
            isPlayerO = GameManager.Instance.PlayerRole == "O";
            if ((isOTurn && isPlayerO) || (!isOTurn && !isPlayerO))
            {
                gameUI.UpdateInfoText("your turn");
                return;
            }
        }

        gameUI.StartTurnTimer();
    }

    public void MoveCube()
    {
        if (isMoving) return;

        isExpanded = !isExpanded;
        StartCoroutine(MoveCubes());
    }

    private IEnumerator MoveCubes()
    {
        isMoving = true;

        // 시작 위치 스냅샷
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                for (int z = 0; z < N; z++)
                {
                    fromPositions[x, y, z] = cubes[x, y, z].transform.position;
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);

            for (int x = 0; x < N; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    for (int z = 0; z < N; z++)
                    {
                        Vector3 target = isExpanded ? expandedPositions[x, y, z]
                                                    : originalPositions[x, y, z];
                        cubes[x, y, z].transform.position =
                            Vector3.LerpUnclamped(fromPositions[x, y, z], target, t);
                    }
                }
            }

            yield return null;
        }

        isMoving = false;
    }

    public void OnCubeClicked(int x, int y, int z, GameObject cubeGO)
    {
        if (gameOver) return;
        if (board[x, y, z] != 0) return;
        if (CameraMoveAndroid != null && CameraMoveAndroid.cameraMoving) return;

        if (isAI)
        {
            ApplyMoveLocal(x, y, z, isOTurn);
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
                if (!isOTurn) OnAITurnStarted?.Invoke();
                gameUI.StartTurnTimer();
            }
        }
        else
        {
            if ((isOTurn && !isPlayerO) || (!isOTurn && isPlayerO))
            {
                gameUI.UpdateInfoText("Not your turn");
                return;
            }

            photonView.RPC(nameof(BroadcastMove), RpcTarget.All, x, y, z, isOTurn);

            int player = isOTurn ? 1 : 2;
            int completedLines = CheckCompletedLines(player);

            if (completedLines == 1)
                photonView.RPC(nameof(HandleGameOver), RpcTarget.All);
            else
                photonView.RPC(nameof(ToggleTurn), RpcTarget.All);
        }
    }

    // --- 공통 로컬 적용(시각/보드) ---
    private void ApplyMoveLocal(int x, int y, int z, bool turnIsO)
    {
        board[x, y, z] = turnIsO ? 1 : 2;

        Cube c = cubeComps[x, y, z];
        c.cubeMesh.enabled = false;

        // 두 오브젝트를 모두 끌/켜는 대신 필요한 것만 On
        if (turnIsO)
        {
            c.oObj.gameObject.SetActive(true);
        }
        else
        {
            c.xObj.gameObject.SetActive(true);
        }
    }

    [PunRPC]
    public void HandleGameOver()
    {
        gameOver = true;
        gameUI.GameResult();
    }

    [PunRPC]
    public void ToggleTurn()
    {
        isOTurn = !isOTurn;
        gameUI.StartTurnTimer();
    }

    [PunRPC]
    public void BroadcastMove(int x, int y, int z, bool wasOTurn)
    {
        ApplyMoveLocal(x, y, z, wasOTurn);
    }

    // --- 승리 라인 체크 ---
    public int CheckCompletedLines(int player)
    {
        int lines = 0;

        // 축 방향
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                if (IsLineCompleted(player, 0, i, j, 1, 0, 0)) lines++;
                if (IsLineCompleted(player, i, 0, j, 0, 1, 0)) lines++;
                if (IsLineCompleted(player, i, j, 0, 0, 0, 1)) lines++;
            }
        }

        // 각 평면의 2D 대각
        for (int i = 0; i < N; i++)
        {
            if (IsLineCompleted(player, 0, 0, i, 1, 1, 0)) lines++;
            if (IsLineCompleted(player, 0, N - 1, i, 1, -1, 0)) lines++;

            if (IsLineCompleted(player, 0, i, 0, 1, 0, 1)) lines++;
            if (IsLineCompleted(player, 0, i, N - 1, 1, 0, -1)) lines++;

            if (IsLineCompleted(player, i, 0, 0, 0, 1, 1)) lines++;
            if (IsLineCompleted(player, i, 0, N - 1, 0, 1, -1)) lines++;
        }

        // 3D 대각
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
            if (board[x, y, z] != player) return false;
        }
        return true;
    }
}
