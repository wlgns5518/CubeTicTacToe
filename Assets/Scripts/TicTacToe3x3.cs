using System.Collections;
using UnityEngine;

public class TicTacToe3x3 : MonoBehaviour
{
    public GameObject cubePrefab; // 에디터에서 할당
    public GameObject[,,] cubes = new GameObject[3, 3, 3];
    public Vector3[,,] originalPositions = new Vector3[3, 3, 3]; // 큐브의 원래 위치 저장
    public int[,,] board = new int[3, 3, 3]; // 0: 비어있음, 1: O, 2: X
    public bool isOTurn = true;
    public bool gameOver = false; // 게임 종료 상태
    private bool isExpanded = false; // 큐브가 펼쳐진 상태인지 여부
    public bool IsExpanded => isExpanded;
    public bool isMoving = false; // 코루틴 실행 여부 확인

    void Start()
    {
        float spacing = 1.1f;
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    Vector3 pos = new Vector3(x * spacing, y * spacing, z * spacing);
                    GameObject cube = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    cubes[x, y, z] = cube;
                    originalPositions[x, y, z] = pos; // 원래 위치 저장
                    int cx = x, cy = y, cz = z;
                    cube.AddComponent<BoxCollider>();
                    cube.AddComponent<CubeClickHandler>().Init(this, cx, cy, cz);
                }
            }
        }
    }

    public void MoveCube()
    {
        if(!isMoving)
        {
            isExpanded = !isExpanded; // 상태 전환
            StartCoroutine(MoveCubes());
        }
    }

    private IEnumerator MoveCubes()
    {
        isMoving = true; // 코루틴 실행 중 상태 설정
        float elapsedTime = 0f;
        float duration = 1f; // 이동 시간

        // 중앙 값 계산 (모든 큐브의 원래 위치의 평균)
        Vector3 centerPosition = Vector3.zero;
        int cubeCount = 0;

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (cubes[x, y, z] != null)
                    {
                        centerPosition += originalPositions[x, y, z];
                        cubeCount++;
                    }
                }
            }
        }

        if (cubeCount > 0)
        {
            centerPosition /= cubeCount; // 평균 위치 계산
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        GameObject cube = cubes[x, y, z];
                        if (cube != null)
                        {
                            Vector3 targetPosition = originalPositions[x, y, z];
                            if (isExpanded)
                            {
                                // 중앙 값 기준으로 상대적인 위치를 유지하며 거리 증가
                                Vector3 relativePosition = originalPositions[x, y, z] - centerPosition;
                                targetPosition = centerPosition + relativePosition * 2.5f; // 거리 비율을 3배로 증가
                            }

                            // Lerp를 사용하여 부드럽게 이동
                            cube.transform.position = Vector3.Lerp(cube.transform.position, targetPosition, t * 0.1f);
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
        MeshRenderer cubeMesh = cube.GetComponent<MeshRenderer>();
        cubeMesh.enabled = false;
        // 자식 오브젝트에서 "O"와 "X"를 찾아 활성화
        Transform oObj = cube.transform.Find("O");
        Transform xObj = cube.transform.Find("X");
        if (oObj != null) oObj.gameObject.SetActive(isOTurn);
        if (xObj != null) xObj.gameObject.SetActive(!isOTurn);

        int player = isOTurn ? 1 : 2;
        int completedLines = CheckCompletedLines(player);
        if (completedLines == 1)
        {
            gameOver = true;
            Debug.Log((player == 1 ? "O" : "X") + "가 승리하였습니다!");
        }
        else
        {
            isOTurn = !isOTurn;
        }
    }

    // 3줄 완성 체크 함수
    public int CheckCompletedLines(int player)
    {
        int lines = 0;

        // 각 축에 대해 체크
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                // x축
                if (board[0, i, j] == player && board[1, i, j] == player && board[2, i, j] == player) lines++;
                // y축
                if (board[i, 0, j] == player && board[i, 1, j] == player && board[i, 2, j] == player) lines++;
                // z축
                if (board[i, j, 0] == player && board[i, j, 1] == player && board[i, j, 2] == player) lines++;
            }
        }

        // 대각선 체크 (3D 대각선 포함)
        // 2D 평면 대각선
        for (int i = 0; i < 3; i++)
        {
            // xy 평면
            if (board[0, 0, i] == player && board[1, 1, i] == player && board[2, 2, i] == player) lines++;
            if (board[0, 2, i] == player && board[1, 1, i] == player && board[2, 0, i] == player) lines++;
            // xz 평면
            if (board[0, i, 0] == player && board[1, i, 1] == player && board[2, i, 2] == player) lines++;
            if (board[0, i, 2] == player && board[1, i, 1] == player && board[2, i, 0] == player) lines++;
            // yz 평면
            if (board[i, 0, 0] == player && board[i, 1, 1] == player && board[i, 2, 2] == player) lines++;
            if (board[i, 0, 2] == player && board[i, 1, 1] == player && board[i, 2, 0] == player) lines++;
        }

        // 3D 대각선
        if (board[0, 0, 0] == player && board[1, 1, 1] == player && board[2, 2, 2] == player) lines++;
        if (board[0, 0, 2] == player && board[1, 1, 1] == player && board[2, 2, 0] == player) lines++;
        if (board[0, 2, 0] == player && board[1, 1, 1] == player && board[2, 0, 2] == player) lines++;
        if (board[0, 2, 2] == player && board[1, 1, 1] == player && board[2, 0, 0] == player) lines++;

        return lines;
    }
}