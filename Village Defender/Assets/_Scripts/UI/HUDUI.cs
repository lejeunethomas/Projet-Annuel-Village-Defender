using TMPro;
using UnityEngine;

public class HUDUI : MonoBehaviour
{
    [Header("Références")]
    public TMP_Text goldText;
    public TMP_Text waveText;

    void Update()
    {
        if (GameManager.Instance == null)
            return;

        if (goldText != null)
            goldText.text = "Or : " + GameManager.Instance.gold;

        if (waveText != null)
            waveText.text = "Vague : " + (GameManager.Instance.currentWaveIndex + 1);
    }
}
