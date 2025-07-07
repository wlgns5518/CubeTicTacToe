using UnityEngine;
using System.Collections.Generic;

public class ComputerPlayer : MonoBehaviour
{
    public TicTacToe3x3 gameManager;
    public bool isComputerO = false; // O가 컴퓨터면 true, X가 컴퓨터면 false

    void Update()
    {
        if (gameManager == null) return;
        if (gameManager.gameOver) return;

        // 현재 턴이 컴퓨터 차례인지 확인
        if ((gameManager.isOTurn && isComputerO) || (!gameManager.isOTurn && !isComputerO))
        {
            MakeMove();
        }
    }

    void MakeMove()
    {
        // 빈 칸 리스트 수집
        List<Vector3Int> emptyCells = new List<Vector3Int>();
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (gameManager.board[x, y, z] == 0)
                    {
                        emptyCells.Add(new Vector3Int(x, y, z));
                    }
                }
            }
        }

        if (emptyCells.Count == 0) return; // 둘 곳이 없음

        // 랜덤으로 한 칸 선택
        int idx = Random.Range(0, emptyCells.Count);
        Vector3Int move = emptyCells[idx];

        // 해당 칸의 큐브 오브젝트 가져오기
        GameObject cube = gameManager.cubes[move.x, move.y, move.z];

        // 게임 매니저에 클릭 전달
        gameManager.OnCubeClicked(move.x, move.y, move.z, cube);
    }
}