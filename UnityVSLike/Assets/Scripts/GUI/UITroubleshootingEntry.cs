using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 트러블슈팅 내역 리스트의 개별 항목을 표시하는 UGUI 컴포넌트
/// </summary>
public class UITroubleshootingEntry : MonoBehaviour
{
    public Text textHeader;
    public Text textStatus;
    public Image imageStatusBg;
    public Text textDescription;
    public Text textCause;
    public Text textSolution;

    public void Setup(int index, GameManager.TroubleshootingEntry entry)
    {
        if (entry == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (textHeader != null)
        {
            textHeader.text = $"#{index + 1} [{entry.type}] ({entry.foundVersion} -> <color=#00FFFF>{entry.appliedVersion}</color>)";
        }

        if (textStatus != null)
        {
            textStatus.text = entry.status;
            switch (entry.status)
            {
                case "완료":
                    textStatus.color = Color.green;
                    break;
                case "수정완료":
                    textStatus.color = Color.cyan;
                    break;
                case "수정중":
                    textStatus.color = Color.yellow;
                    break;
                case "테스트요청":
                default:
                    textStatus.color = new Color(1f, 0.5f, 0f);
                    break;
            }
        }

        if (textDescription != null)
            textDescription.text = $"<b>현상:</b> {entry.description}";

        if (textCause != null)
        {
            textCause.gameObject.SetActive(!string.IsNullOrEmpty(entry.cause));
            textCause.text = $"<b>원인:</b> {entry.cause}";
        }

        if (textSolution != null)
        {
            textSolution.gameObject.SetActive(!string.IsNullOrEmpty(entry.solution));
            textSolution.text = $"<b>해결:</b> {entry.solution}";
        }
    }
}
