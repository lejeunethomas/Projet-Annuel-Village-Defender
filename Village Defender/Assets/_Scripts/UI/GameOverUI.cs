using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("Références UI")]
    public TMP_Text titleText;
    public TMP_Text resultText;
    public TMP_Text buttonText;
    public Button actionButton;

    private bool wasVictory;

    private void Awake()
    {
        if (actionButton != null)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(OnMainButtonClicked);
        }
    }

    public void ShowVictory()
    {
        wasVictory = true;

        if (titleText != null)
            titleText.text = "Fin de l'assaut";

        if (resultText != null)
            resultText.text = "Vague éliminée";

        if (buttonText != null)
            buttonText.text = "Retour au village";

        gameObject.SetActive(true);
    }

    public void ShowDefeat()
    {
        wasVictory = false;

        if (titleText != null)
            titleText.text = "Fin de l'assaut";

        if (resultText != null)
            resultText.text = "Machine détruite";

        if (buttonText != null)
            buttonText.text = "Retour au village";

        gameObject.SetActive(true);
    }

    private void OnMainButtonClicked()
    {
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToVillageAfterEndScreen(wasVictory);
        }
    }
}