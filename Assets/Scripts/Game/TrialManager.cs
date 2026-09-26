using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DIALOGUE;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerStats
{
    public float maxHP = 5f;
    public float hp;

    public void InitializeMeters()
    {
        hp = maxHP;
        TrialManager.instance.barsAnimator.SetBarsFillAmount(TrialManager.instance.playerStats.hp,
            TimeManipulationManager.instance.concentration);
    }
}

public class TrialManager : MonoBehaviour
{
    public static TrialManager instance { get; private set; }
    public Trial trial;
    public int currentIndex = 0;
    public List<CourtRoomCharacter> courtRoomCharacters;
    public CourtRoomCharacter protagonistCharacter;

    public PlayerStats playerStats = new PlayerStats();
    public PlayerBarsAnimator barsAnimator;
    public RectTransform globalUI;
    public TrialIntro introAnimation;
    public PreTrialPrepMenu preTrialPrepMenu;

    public RectTransform failedScreen;
    public Image failedTextImage;
    public AudioClip healSound;

    void Awake()
    {
        instance = this;
    }

    public void StartNewTrial()
    {
        EvidenceManager.instance.Initialize(trial.evidences);
        StartCoroutine(StartPipeline());
    }

    private IEnumerator StartPipeline()
    {
        ImageScript.instance.FadeToBlack(0.2f);
        preTrialPrepMenu.Disappear();

        yield return new WaitForSeconds(0.4f);

        preTrialPrepMenu.gameObject.SetActive(false);
        ImageScript.instance.UnFadeToBlack(0.1f);
        playerStats.InitializeMeters();

        TrialIntro intro = Instantiate(introAnimation, globalUI);
        intro.transform.SetAsFirstSibling();
        yield return intro.Animate();
        ImageScript.instance.UnFadeToBlack(0.2f);
        DialogueSystem.instance.dialogueBoxAnimator.Initialize();
        yield return CameraController.instance.FovOutro();
        ContinueTrial();
    }

    private void ContinueTrial()
    {
        preTrialPrepMenu.gameObject.SetActive(false);
        TrialSegment segment = trial.trialSegments[currentIndex];
        segment.Play();
    }

    public void OnSegmentFinished()
    {
        currentIndex++;

        if (currentIndex < trial.trialSegments.Count)
        {
            TrialSegment segment = trial.trialSegments[currentIndex];
            segment.Play();
        }

        else
        {
            // Handle trial completion
        }
    }

    public void IncreaseHealth(float amount)
    {
        SoundManager.instance.PlaySoundEffect(healSound);

        if (playerStats.hp < playerStats.maxHP)
        {
            barsAnimator.IncreaseHealth(Math.Min(amount, playerStats.maxHP - playerStats.hp),
                amount / 2); // Fill either the amount, or what remains to fill before the meter if already full
        }

        playerStats.hp = Math.Min(playerStats.hp + amount, playerStats.maxHP);
    }

    public void DecreaseHealthDefault(float amount)
    {
        playerStats.hp = Math.Max(playerStats.hp - amount, 0);
        barsAnimator.DecreaseHealth(playerStats.hp, 0.5f);
        if(playerStats.hp == 0)
            trial.trialSegments[currentIndex].HandleGameOver();
    }

    public void DecreaseHealthFromMeter(Image meter, float amount)
    {
        playerStats.hp = Math.Max(playerStats.hp - amount, 0);
        barsAnimator.DecreaseHealthFromMeter(meter, playerStats.hp, 0.5f);
        if(playerStats.hp == 0)
            trial.trialSegments[currentIndex].HandleGameOver();
    }


    public IEnumerator GameOver()
    {
        TrialDialogueManager.instance.animator.FaceAppear();
        barsAnimator.globalHealthMeter.fillAmount = 0f;
        yield return TrialDialogueManager.instance.PlayNodeList(trial.utilityNodesCollection
            .gameOverNodes);
        barsAnimator.HideGlobalBars(0.2f);
        TrialDialogueManager.instance.ConversationEnd();
        TrialSegment segment = trial.trialSegments[currentIndex];
        segment.Play();
    }

    public void FadeCharactersExcept(Character character, float opacity, float duration)
    {
        foreach (CourtRoomCharacter courtRoomCharacter in courtRoomCharacters)
        {
            if (courtRoomCharacter.character != character)
            {
                courtRoomCharacter.spriteRenderer.DOFade(opacity, duration);
            }
        }
    }

    public IEnumerator ShowFailedScreen()
    {
        yield return new WaitForSeconds(0.5f);
        failedScreen.anchoredPosition = new Vector2(0, 1200);
        Color color = failedTextImage.color;
        color.a = 0f;
        failedTextImage.color = color;
        failedTextImage.rectTransform.localScale = Vector3.one;

        Sequence sequence = DOTween.Sequence();
        sequence.Append(failedScreen.DOAnchorPosY(0, 0.2f));
        sequence.Append(failedTextImage.DOFade(1f, 0.1f));
        sequence.AppendInterval(1f);
        sequence.Append(failedTextImage.rectTransform.DOScale(2f, 0.2f));
        sequence.Join(failedTextImage.DOFade(0, 0.2f));
        sequence.AppendCallback(() => ImageScript.instance.FadeToBlack(0.2f));

        yield return new WaitForSeconds(2.5f);

        failedScreen.anchoredPosition = new Vector2(0, 1200);
    }
}