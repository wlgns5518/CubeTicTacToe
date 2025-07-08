using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameButton : MonoBehaviour
{
    public GameObject resultPopup;
    public Button resultButton;
    public TextMeshProUGUI resultText; // 결과 메시지를 표시할 Text 컴포넌트

    public void OKButton()
    {
        GameManager.Instance.GameSet();
    }

    public void CloseButton()
    {
        if (resultPopup != null)
        {
            resultPopup.SetActive(false);
        }
    }

    public void ResultButton()
    {
        if (resultPopup != null)
        {
            resultPopup.SetActive(true);
        }
    }

    // 결과 메시지를 설정하는 메서드
    public void SetResultMessage(bool isVictory)
    {
        if (resultText != null)
        {
            resultText.text = isVictory ? "Victory!" : "Loss";
        }
    }
}