using UnityEngine;
using UnityEngine.UI;

public class ExpandButton : MonoBehaviour
{
    public Button button; // 버튼을 연결할 변수
    public Image buttonImage; // 버튼의 이미지 컴포넌트
    public Sprite expandCubeSprite; // ExpandCube 이미지
    public Sprite contractCubeSprite; // ContractCube 이미지
    [SerializeField] private TicTacToeNxN tictactoeNxN;
    public void OnButtonClick()
    {
        // 활성화된 TicTacToe 인스턴스 확인 및 처리
        if (tictactoeNxN != null)
        {
            tictactoeNxN.MoveCube();
            UpdateButtonImage(tictactoeNxN.IsExpanded);
        }
    }

    private void UpdateButtonImage(bool isExpanded)
    {
        // isMoving 상태에 따라 이미지 변경
        buttonImage.sprite = isExpanded ? contractCubeSprite : expandCubeSprite;
    }
}