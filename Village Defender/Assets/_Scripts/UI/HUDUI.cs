using TMPro;
using UnityEngine;

public class HUDUI : MonoBehaviour
{
    [Header("Références")]
    public TMP_Text goldText;
    public TMP_Text woodText;
    public TMP_Text stoneText;
    public TMP_Text ironText;
    public TMP_Text waveText;

    void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (GameManager.Instance == null)
            return;

        if (goldText != null)
            goldText.text = "Or : " + GameManager.Instance.gold;

        if (woodText != null)
            woodText.text = "Bois : " + GameManager.Instance.wood;

        if (stoneText != null)
            stoneText.text = "Pierre : " + GameManager.Instance.stone;

        if (ironText != null)
            ironText.text = "Fer : " + GameManager.Instance.iron;

        if (waveText != null)
            waveText.text = "Vague : " + (GameManager.Instance.currentWaveIndex + 1);
    }
}
