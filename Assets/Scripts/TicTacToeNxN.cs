using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToeNxN : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private GameUI gameUI;
    [SerializeField] private CameraMoveAndroid CameraMoveAndroid;

    public GameObject[,,] cubes;
    public int[,,] board;                       // 0: empty, 1: O, 2: X (보드 값은 기존 그대로 유지)
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
    private Vector3[,,] fromPositions;
    private Cube[,,] cubeComps;
    private const float spacing = 1.1f;
    private const float expandFactor = 2.5f;
    private const float moveDuration = 0.5f;

    // --- 승리 라인 계산 ---
    private List<Vector3Int[]> allLines;
    private List<int>[,,] cellToLines;
    private int[] lineSums;                     // 각 라인별 합: O=+1, X=-1 → N이면 O 승, -N이면 X 승

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
                    cubeComps[x, y, z] = cube.GetComponent<Cube>();

                    cube.AddComponent<CubeClickHandler>().Init(x, y, z);

                    count++;
                }
            }
        }
        if (count > 0) centerPosition /= count;

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

        PrecomputeLines();
    }

    private void Start()
    {
        isOTurn = GameManager.Instance.isOTurnFirst;
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

    private void PrecomputeLines()
    {
        allLines = new List<Vector3Int[]>();
        cellToLines = new List<int>[N, N, N];

        for (int x = 0; x < N; x++)
            for (int y = 0; y < N; y++)
                for (int z = 0; z < N; z++)
                    cellToLines[x, y, z] = new List<int>();

        // 축 방향
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                AddLine(k => new Vector3Int(k, i, j));
                AddLine(k => new Vector3Int(i, k, j));
                AddLine(k => new Vector3Int(i, j, k));
            }
        }

        // 각 평면 대각선
        for (int i = 0; i < N; i++)
        {
            // XY (z=i)
            AddLine(k => new Vector3Int(k, k, i));
            AddLine(k => new Vector3Int(k, N - 1 - k, i));
            // XZ (y=i)
            AddLine(k => new Vector3Int(k, i, k));
            AddLine(k => new Vector3Int(k, i, N - 1 - k));
            // YZ (x=i)
            AddLine(k => new Vector3Int(i, k, k));
            AddLine(k => new Vector3Int(i, k, N - 1 - k));
        }

        // 3D 공간 대각선
        AddLine(k => new Vector3Int(k, k, k));
        AddLine(k => new Vector3Int(k, k, N - 1 - k));
        AddLine(k => new Vector3Int(k, N - 1 - k, k));
        AddLine(k => new Vector3Int(k, N - 1 - k, N - 1 - k));

        // 단일 합 배열 사용
        lineSums = new int[allLines.Count];
    }

    private void AddLine(Func<int, Vector3Int> generator)
    {
        Vector3Int[] cells = new Vector3Int[N];
        for (int k = 0; k < N; k++)
            cells[k] = generator(k);

        int lineIndex = allLines.Count;
        allLines.Add(cells);

        for (int k = 0; k < N; k++)
        {
            Vector3Int c = cells[k];
            cellToLines[c.x, c.y, c.z].Add(lineIndex);
        }
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

        for (int x = 0; x < N; x++)
            for (int y = 0; y < N; y++)
                for (int z = 0; z < N; z++)
                    fromPositions[x, y, z] = cubes[x, y, z].transform.position;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);

            for (int x = 0; x < N; x++)
                for (int y = 0; y < N; y++)
                    for (int z = 0; z < N; z++)
                    {
                        Vector3 target = isExpanded ? expandedPositions[x, y, z] : originalPositions[x, y, z];
                        cubes[x, y, z].transform.position =
                            Vector3.LerpUnclamped(fromPositions[x, y, z], target, t);
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
            int playerVal = isOTurn ? 1 : -1; // O = +1, X = -1

            if (ProcessMoveAndCheckWin(x, y, z, playerVal))
            {
                gameOver = true;
                gameUI.GameResult();
            }
            else
            {
                isOTurn = !isOTurn;
                gameUI.StartTurnTimer();
                if (!isOTurn) OnAITurnStarted?.Invoke();
                
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

            int playerVal = isOTurn ? 1 : -1;
            if (ProcessMoveAndCheckWin(x, y, z, playerVal))
                photonView.RPC(nameof(HandleGameOver), RpcTarget.All);
            else
                photonView.RPC(nameof(ToggleTurn), RpcTarget.All);
        }
    }

    // 단일 라인 합을 활용한 승리 판정
    private bool ProcessMoveAndCheckWin(int x, int y, int z, int playerVal)
    {
        foreach (int lineIdx in cellToLines[x, y, z])
        {
            lineSums[lineIdx] += playerVal;
            if (lineSums[lineIdx] == N || lineSums[lineIdx] == -N)
                return true;
        }
        return false;
    }

    private void ApplyMoveLocal(int x, int y, int z, bool turnIsO)
    {
        board[x, y, z] = turnIsO ? 1 : 2; // 기존 보드 값 유지 (AI 코드 영향 최소화)

        Cube c = cubeComps[x, y, z];
        c.cubeMesh.enabled = false;

        if (turnIsO)
            c.oObj.gameObject.SetActive(true);
        else
            c.xObj.gameObject.SetActive(true);
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
    public bool HasCompletedLine(int[,,] boardState, int player)
    {
        // 기존 AI용: player는 1(O) 또는 2(X)로 가정 (라인 합과 무관)
        for (int i = 0; i < allLines.Count; i++)
        {
            Vector3Int[] line = allLines[i];
            bool complete = true;
            for (int k = 0; k < line.Length; k++)
            {
                Vector3Int c = line[k];
                if (boardState[c.x, c.y, c.z] != player)
                {
                    complete = false;
                    break;
                }
            }
            if (complete) return true;
        }
        return false;
    }
}