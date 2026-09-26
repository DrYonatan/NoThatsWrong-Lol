using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DIALOGUE;
using UnityEngine;
using UnityEngine.UI;

public class ImageScript : MonoBehaviour
{
    public static ImageScript instance { get; private set; }

    public Image overlayImage;
    public CanvasGroup blackFade;
    public Image whiteFlash;
    public CanvasGroup canvasGroup;
    public AudioClip flashSound;

    public Image background;
    public Dictionary<Character, GameObject> backgroundCharacters = new();
    public Image blackFadeUnderTextBox;

    public CanvasGroup animatedImageContainer;

    public OverlayTextBoxAnimator overlayTextBoxAnimator;

    private void Awake()
    {
        blackFade.alpha = 1f;
        instance = this;
    }

    public void Show(Sprite image, bool flash, float duration = 0.4f)
    {
        overlayImage.sprite = image;
        DialogueSystem.instance.SetTextBox(overlayTextBoxAnimator);
        DialogueSystem.instance.TextBoxAppear();

        ShowingOrHiding(canvasGroup, duration, 1f);

        if (flash)
        {
            Flash(duration, flashSound);
        }
    }

    public void Hide(bool flash, float duration = 0.4f)
    {
        DialogueSystem.instance.TextBoxDisappear();
        DialogueSystem.instance.UseInitialDialogueContainer();
        DialogueSystem.instance.TextBoxAppear();
        ShowingOrHiding(canvasGroup, duration, 0f);

        if (flash)
        {
            Flash(duration, flashSound);
        }
    }

    public void ShowBackground(Sprite sprite, float duration = 0.2f)
    {
        if (sprite == null)
            return;

        Image oldBackground = background;
        Image newBackground = Instantiate(background, background.transform.parent);
        newBackground.sprite = sprite;

        Sequence seq = DOTween.Sequence();

        if (duration != 0)
            seq.Append(newBackground.DOFade(0f, 0f)); // Avoid stutters if the background should appear immediately
        seq.Append(newBackground.DOFade(1f, duration).SetEase(Ease.Linear));
        seq.AppendCallback(() => Destroy(oldBackground.gameObject));
        seq.AppendCallback(() => background = newBackground);
    }

    public void HideBackground(float duration)
    {
        background.DOFade(0f, duration).SetEase(Ease.Linear).OnComplete(() => background.sprite = null);
    }

    public void Flash(float duration, AudioClip sound)
    {
        SoundManager.instance.PlaySoundEffect(sound);
        whiteFlash.DOKill();
        Color color = whiteFlash.color;
        color.a = 0;
        whiteFlash.color = color;

        whiteFlash.DOFade(1f, duration / 2).SetLoops(2, LoopType.Yoyo);
    }

    public void FadeToBlack(float duration)
    {
        ShowingOrHiding(blackFade, duration, 1f);
    }

    public void UnFadeToBlack(float duration)
    {
        ShowingOrHiding(blackFade, duration, 0f);
    }

    public void FadeUnderTextBoxBlack(bool fadeIn, float duration)
    {
        blackFadeUnderTextBox.DOFade(fadeIn ? 1 : 0, duration);
    }

    private void ShowingOrHiding(CanvasGroup canvasGroupToShowOrHide, float duration, float targetAlpha)
    {
        if (duration == 0f)
            canvasGroupToShowOrHide.alpha = targetAlpha;
        else
           canvasGroupToShowOrHide.DOFade(targetAlpha, duration).SetUpdate(true);
    }

}