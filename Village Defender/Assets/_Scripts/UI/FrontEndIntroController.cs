using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FrontEndIntroController : MonoBehaviour
{
    [Header("Interfaces")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject introDialogueUI;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private TMP_Text startAdventureButtonText;
    [SerializeField] private GameObject restartButton;

    [Header("Dialogue")]
    [SerializeField] private RectTransform characterContainer;
    [SerializeField] private GameObject bubbleContainer;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Phrases")]
    [SerializeField] private List<string> introSentences = new List<string>();

    [Header("Animation d'entrée")]
    [SerializeField] private float characterEntranceDistance = 700f;
    [SerializeField] private float characterEntranceDuration = 0.8f;

    [Header("Écriture")]
    [SerializeField] private float characterDelay = 0.035f;

    [Header("Mouvement du personnage")]
    [SerializeField] private float bobAmplitude = 8f;
    [SerializeField] private float bobSpeed = 8f;

    private Vector2 _characterFinalPosition;
    private int _currentSentenceIndex;
    private bool _introStarted;
    private bool _entranceFinished;
    private bool _isWriting;
    private bool _isSentenceComplete;
    private bool _isFinishing;
    private Coroutine _introCoroutine;
    private Coroutine _typingCoroutine;

    private void Awake()
    {
        if (characterContainer != null)
            _characterFinalPosition = characterContainer.anchoredPosition;

        ValidateRequiredReferences();
        ResetInitialState();
    }

    private void Start()
    {
        ResolveSaveManagerReference();
        RefreshMainMenuSaveState();
    }

    private void OnValidate()
    {
        if (characterEntranceDistance < 0f)
            characterEntranceDistance = 0f;

        if (characterEntranceDuration < 0f)
            characterEntranceDuration = 0f;

        if (characterDelay < 0f)
            characterDelay = 0f;

        if (bobAmplitude < 0f)
            bobAmplitude = 0f;

        if (bobSpeed < 0f)
            bobSpeed = 0f;
    }

    public void StartAdventure()
    {
        if (_introStarted)
            return;

        ResolveSaveManagerReference();

        if (saveManager != null && saveManager.HasValidSave())
        {
            if (mainMenuUI == null)
            {
                Debug.LogError("FrontEndIntroController : impossible de continuer, MainMenuUI n'est pas assigne.");
                return;
            }

            _introStarted = true;
            _isFinishing = false;

            mainMenuUI.SetActive(false);

            if (introDialogueUI != null)
                introDialogueUI.SetActive(false);

            if (bubbleContainer != null)
                bubbleContainer.SetActive(false);

            if (dialogueText != null)
            {
                dialogueText.text = "";
                dialogueText.maxVisibleCharacters = 0;
            }

            bool loaded = saveManager.LoadGameAndContinue();
            if (!loaded)
            {
                _introStarted = false;
                ResetInitialState();
                RefreshMainMenuSaveState();
            }

            return;
        }

        if (!ValidateRequiredReferences())
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("FrontEndIntroController : impossible de lancer l'introduction, aucun GameManager.Instance n'est disponible.");
            return;
        }

        _introStarted = true;
        _isFinishing = false;
        _entranceFinished = false;
        _isWriting = false;
        _isSentenceComplete = false;
        _currentSentenceIndex = 0;

        GameManager.Instance.SetPhase(GameManager.GamePhase.Intro);

        mainMenuUI.SetActive(false);
        introDialogueUI.SetActive(true);
        bubbleContainer.SetActive(false);
        dialogueText.text = "";
        dialogueText.maxVisibleCharacters = 0;
        characterContainer.anchoredPosition = _characterFinalPosition + Vector2.down * characterEntranceDistance;

        if (_introCoroutine != null)
            StopCoroutine(_introCoroutine);

        _introCoroutine = StartCoroutine(PlayIntro());
    }

    public void RestartAdventureFromBeginning()
    {
        if (_introStarted)
            return;

        ResolveSaveManagerReference();

        if (saveManager != null)
            saveManager.DeleteSave();
        else
            Debug.LogWarning("FrontEndIntroController : impossible de supprimer la sauvegarde, aucun SaveManager n'est disponible.");

        RefreshMainMenuSaveState();
        StartAdventure();
    }

    public void OnDialogueClicked()
    {
        if (!_introStarted || !_entranceFinished || _isFinishing)
            return;

        if (_isWriting)
        {
            CompleteCurrentSentence(true);
            return;
        }

        if (!_isSentenceComplete)
            return;

        int nextSentenceIndex = GetNextValidSentenceIndex(_currentSentenceIndex + 1);
        if (nextSentenceIndex >= 0)
        {
            _currentSentenceIndex = nextSentenceIndex;
            StartTypingCurrentSentence();
            return;
        }

        FinishIntro();
    }

    private void ResetInitialState()
    {
        _currentSentenceIndex = 0;
        _introStarted = false;
        _entranceFinished = false;
        _isWriting = false;
        _isSentenceComplete = false;
        _isFinishing = false;

        if (mainMenuUI != null)
            mainMenuUI.SetActive(true);

        if (introDialogueUI != null)
            introDialogueUI.SetActive(false);

        if (restartButton != null)
            restartButton.SetActive(false);

        if (bubbleContainer != null)
            bubbleContainer.SetActive(false);

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = 0;
        }
    }

    private IEnumerator PlayIntro()
    {
        yield return AnimateCharacterEntrance();

        characterContainer.anchoredPosition = _characterFinalPosition;
        bubbleContainer.SetActive(true);

        yield return new WaitForSecondsRealtime(0.1f);

        _entranceFinished = true;

        int firstSentenceIndex = GetNextValidSentenceIndex(0);
        if (firstSentenceIndex < 0)
        {
            FinishIntro();
            yield break;
        }

        _currentSentenceIndex = firstSentenceIndex;
        StartTypingCurrentSentence();
        _introCoroutine = null;
    }

    private IEnumerator AnimateCharacterEntrance()
    {
        Vector2 startPosition = _characterFinalPosition + Vector2.down * characterEntranceDistance;
        float elapsedTime = 0f;

        if (characterEntranceDuration <= 0f)
        {
            characterContainer.anchoredPosition = _characterFinalPosition;
            yield break;
        }

        while (elapsedTime < characterEntranceDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedTime / characterEntranceDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            characterContainer.anchoredPosition = Vector2.Lerp(startPosition, _characterFinalPosition, smoothProgress);
            yield return null;
        }
    }

    private void StartTypingCurrentSentence()
    {
        if (!TryGetSentence(_currentSentenceIndex, out string sentence))
        {
            FinishIntro();
            return;
        }

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeSentence(sentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        _isWriting = true;
        _isSentenceComplete = false;

        dialogueText.text = sentence;
        dialogueText.ForceMeshUpdate();
        dialogueText.maxVisibleCharacters = 0;

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;

        for (int visibleCharacters = 0; visibleCharacters < totalVisibleCharacters; visibleCharacters++)
        {
            dialogueText.maxVisibleCharacters = visibleCharacters + 1;
            MoveCharacterWhileTyping();

            if (characterDelay <= 0f)
            {
                yield return null;
                continue;
            }

            float elapsedDelay = 0f;
            while (elapsedDelay < characterDelay)
            {
                elapsedDelay += Time.unscaledDeltaTime;
                MoveCharacterWhileTyping();
                yield return null;
            }
        }

        CompleteCurrentSentence(false);
    }

    private void CompleteCurrentSentence(bool stopTypingCoroutine)
    {
        if (stopTypingCoroutine && _typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _typingCoroutine = null;

        if (dialogueText != null)
        {
            dialogueText.ForceMeshUpdate();
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        }

        if (characterContainer != null)
            characterContainer.anchoredPosition = _characterFinalPosition;

        _isWriting = false;
        _isSentenceComplete = true;
    }

    private void MoveCharacterWhileTyping()
    {
        if (characterContainer == null)
            return;

        float offsetY = Mathf.Sin(Time.unscaledTime * bobSpeed) * bobAmplitude;
        characterContainer.anchoredPosition = _characterFinalPosition + Vector2.up * offsetY;
    }

    private int GetNextValidSentenceIndex(int startIndex)
    {
        if (introSentences == null)
            return -1;

        for (int i = startIndex; i < introSentences.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(introSentences[i]))
                return i;
        }

        return -1;
    }

    private bool TryGetSentence(int index, out string sentence)
    {
        sentence = "";

        if (introSentences == null || index < 0 || index >= introSentences.Count)
            return false;

        sentence = introSentences[index];
        return !string.IsNullOrWhiteSpace(sentence);
    }

    private void FinishIntro()
    {
        if (_isFinishing)
            return;

        _isFinishing = true;

        StopAllCoroutines();
        _introCoroutine = null;
        _typingCoroutine = null;

        if (dialogueText != null)
        {
            dialogueText.text = "";
            dialogueText.maxVisibleCharacters = 0;
        }

        if (characterContainer != null)
            characterContainer.anchoredPosition = _characterFinalPosition;

        if (introDialogueUI != null)
            introDialogueUI.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.SetPhase(GameManager.GamePhase.Village);
        else
            Debug.LogError("FrontEndIntroController : impossible de terminer l'introduction, aucun GameManager.Instance n'est disponible.");

        ResolveSaveManagerReference();

        if (saveManager != null)
            saveManager.BeginNewGame();
    }

    private void RefreshMainMenuSaveState()
    {
        ResolveSaveManagerReference();
        bool hasValidSave = saveManager != null && saveManager.HasValidSave();

        if (startAdventureButtonText != null)
            startAdventureButtonText.text = hasValidSave ? "Continuer" : "Commencer";

        if (restartButton != null)
            restartButton.SetActive(hasValidSave);
    }

    private void ResolveSaveManagerReference()
    {
        if (saveManager == null)
            saveManager = SaveManager.Instance;
    }

    private bool ValidateRequiredReferences()
    {
        bool isValid = true;

        if (mainMenuUI == null)
        {
            Debug.LogError("FrontEndIntroController : MainMenuUI n'est pas assigne.");
            isValid = false;
        }

        if (introDialogueUI == null)
        {
            Debug.LogError("FrontEndIntroController : IntroDialogueUI n'est pas assigne.");
            isValid = false;
        }

        if (characterContainer == null)
        {
            Debug.LogError("FrontEndIntroController : CharacterContainer n'est pas assigne.");
            isValid = false;
        }

        if (bubbleContainer == null)
        {
            Debug.LogError("FrontEndIntroController : BubbleContainer n'est pas assigne.");
            isValid = false;
        }

        if (dialogueText == null)
        {
            Debug.LogError("FrontEndIntroController : DialogueText n'est pas assigne.");
            isValid = false;
        }

        return isValid;
    }
}
