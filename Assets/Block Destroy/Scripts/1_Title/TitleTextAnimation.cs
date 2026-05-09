using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// Animated title text "BLOCK DESTROY" with bounce and glow effects
/// Replaces Spine animation with Unity UI + DOTween
/// </summary>
public class TitleTextAnimation : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI[] topLetters; // B L O C K
    public TextMeshProUGUI[] bottomLetters; // D E S T R O Y
    public CanvasGroup backgroundGlow;

    [Header("Animation Settings")]
    public float letterDelay = 0.0667f; // Delay between each letter (matches original)
    public float bounceDuration = 0.3f;
    public float bounceHeight = 5.42f;
    public Vector3 bounceScale = new Vector3(1.119f, 1.066f, 1f);

    [Header("Glow Settings")]
    public float glowStartTime = 3.1f;
    public float glowDuration = 0.3f;

    private void Start()
    {
        // Hide all letters initially
        foreach (var letter in topLetters)
        {
            letter.transform.localScale = Vector3.zero;
        }
        foreach (var letter in bottomLetters)
        {
            letter.transform.localScale = Vector3.zero;
        }

        if (backgroundGlow != null)
        {
            backgroundGlow.alpha = 1f;
        }

        // Start animation
        StartCoroutine(AnimateTitle());
    }

    private IEnumerator AnimateTitle()
    {
        // Animate top row: B L O C K
        for (int i = 0; i < topLetters.Length; i++)
        {
            AnimateLetter(topLetters[i], i * letterDelay);
            yield return new WaitForSeconds(letterDelay);
        }

        // Animate bottom row: D E S T R O Y
        for (int i = 0; i < bottomLetters.Length; i++)
        {
            AnimateLetter(bottomLetters[i], (topLetters.Length + i) * letterDelay);
            yield return new WaitForSeconds(letterDelay);
        }

        // Wait for glow effect
        yield return new WaitForSeconds(glowStartTime - (topLetters.Length + bottomLetters.Length) * letterDelay);

        // Glow effect (background flash)
        if (backgroundGlow != null)
        {
            AnimateGlow();
        }
    }

    private void AnimateLetter(TextMeshProUGUI letter, float startDelay)
    {
        if (letter == null) return;

        Sequence sequence = DOTween.Sequence();

        // Scale up with bounce
        sequence.Append(letter.transform.DOScale(bounceScale, bounceDuration * 0.5f).SetEase(Ease.OutCubic));

        // Move up
        sequence.Join(letter.transform.DOLocalMoveY(letter.transform.localPosition.y + bounceHeight, bounceDuration * 0.5f).SetEase(Ease.OutCubic));

        // Scale back and move down
        sequence.Append(letter.transform.DOScale(Vector3.one, bounceDuration * 0.5f).SetEase(Ease.InOutCubic));
        sequence.Join(letter.transform.DOLocalMoveY(letter.transform.localPosition.y, bounceDuration * 0.5f).SetEase(Ease.InOutCubic));
    }

    private void AnimateGlow()
    {
        Sequence glowSequence = DOTween.Sequence();

        // Flash out
        glowSequence.Append(backgroundGlow.DOFade(0f, glowDuration * 0.5f).SetEase(Ease.OutCubic));

        // Flash in
        glowSequence.Append(backgroundGlow.DOFade(1f, glowDuration * 0.5f).SetEase(Ease.InCubic));

        // Repeat flash
        glowSequence.Append(backgroundGlow.DOFade(0f, glowDuration * 0.5f).SetEase(Ease.OutCubic));
        glowSequence.Append(backgroundGlow.DOFade(1f, glowDuration * 0.5f).SetEase(Ease.InCubic));
    }

    private void OnDestroy()
    {
        // Clean up tweens
        DOTween.Kill(transform);
        if (backgroundGlow != null)
        {
            DOTween.Kill(backgroundGlow.transform);
        }
        foreach (var letter in topLetters)
        {
            if (letter != null)
                DOTween.Kill(letter.transform);
        }
        foreach (var letter in bottomLetters)
        {
            if (letter != null)
                DOTween.Kill(letter.transform);
        }
    }
}
