using System;
using System.Collections;
using UnityEngine;

public enum GameEra
{
    Era1,
    Era2
}

public class EraManager : MonoBehaviour
{
    [Header("Décors")]
    [SerializeField] private GameObject era1Environment;
    [SerializeField] private GameObject era2Environment;

    [Header("Flash blanc")]
    [SerializeField] private CanvasGroup whiteFlashCanvasGroup;
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float whiteHoldDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.45f;

    [Header("Déblocage")]
    [SerializeField] private int era2UnlockWaveIndex = 5;

    private GameEra currentEra;
    private bool transitionInProgress;
    private Coroutine transitionCoroutine;

    private void OnValidate()
    {
        fadeInDuration = Mathf.Max(0f, fadeInDuration);
        whiteHoldDuration = Mathf.Max(0f, whiteHoldDuration);
        fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
        era2UnlockWaveIndex = Mathf.Max(0, era2UnlockWaveIndex);
    }

    public GameEra GetEraForWaveIndex(int waveIndex)
    {
        return waveIndex >= era2UnlockWaveIndex ? GameEra.Era2 : GameEra.Era1;
    }

    public bool NeedsTransitionForWaveIndex(int waveIndex)
    {
        return currentEra != GetEraForWaveIndex(waveIndex);
    }

    public void ApplyEraInstantly(int waveIndex)
    {
        GameEra targetEra = GetEraForWaveIndex(waveIndex);
        ApplyEnvironment(targetEra);

        if (whiteFlashCanvasGroup != null)
        {
            whiteFlashCanvasGroup.alpha = 0f;
            whiteFlashCanvasGroup.interactable = false;
            whiteFlashCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("EraManager : WhiteFlash CanvasGroup n'est pas assigné.");
        }
    }

    public IEnumerator PlayEraTransition(int targetWaveIndex, Action onScreenFullyWhite)
    {
        if (transitionInProgress)
        {
            Debug.LogWarning("EraManager : transition d'ère déjà en cours.");
            yield break;
        }

        transitionInProgress = true;
        transitionCoroutine = StartCoroutine(PlayEraTransitionRoutine(targetWaveIndex, onScreenFullyWhite));
        yield return transitionCoroutine;
        transitionCoroutine = null;
        transitionInProgress = false;
    }

    private IEnumerator PlayEraTransitionRoutine(int targetWaveIndex, Action onScreenFullyWhite)
    {
        GameEra targetEra = GetEraForWaveIndex(targetWaveIndex);

        if (whiteFlashCanvasGroup == null)
        {
            Debug.LogError("EraManager : transition impossible, WhiteFlash CanvasGroup n'est pas assigné. Application instantanée de l'ère.");
            ApplyEnvironment(targetEra);
            onScreenFullyWhite?.Invoke();
            yield break;
        }

        whiteFlashCanvasGroup.interactable = false;
        whiteFlashCanvasGroup.blocksRaycasts = true;
        whiteFlashCanvasGroup.alpha = 0f;

        yield return FadeFlash(0f, 1f, fadeInDuration);
        whiteFlashCanvasGroup.alpha = 1f;

        ApplyEnvironment(targetEra);
        onScreenFullyWhite?.Invoke();

        if (whiteHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(whiteHoldDuration);

        yield return FadeFlash(1f, 0f, fadeOutDuration);

        whiteFlashCanvasGroup.alpha = 0f;
        whiteFlashCanvasGroup.interactable = false;
        whiteFlashCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeFlash(float fromAlpha, float toAlpha, float duration)
    {
        if (whiteFlashCanvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            whiteFlashCanvasGroup.alpha = toAlpha;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            whiteFlashCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, progress);
            yield return null;
        }

        whiteFlashCanvasGroup.alpha = toAlpha;
    }

    private void ApplyEnvironment(GameEra targetEra)
    {
        if (era1Environment == null)
            Debug.LogError("EraManager : Era1Environment n'est pas assigné.");

        if (era2Environment == null)
            Debug.LogError("EraManager : Era2Environment n'est pas assigné.");

        if (era1Environment != null)
            era1Environment.SetActive(targetEra == GameEra.Era1);

        if (era2Environment != null)
            era2Environment.SetActive(targetEra == GameEra.Era2);

        currentEra = targetEra;
    }
}
