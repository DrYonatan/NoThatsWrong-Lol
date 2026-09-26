using System;
using System.Collections;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

[Serializable]
public class DiscussionNode : DialogueNode
{
    // public bool usePrevCamera;
    // public CourtRoomCharacter courtCharacter;
    
    // public CameraPreset cameraPreset;
    
    // public List<CameraEffect> cameraEffects = new List<CameraEffect>();
    // public float fovOffset;
    // public Vector3 positionOffset;
    // public Vector3 rotationOffset;

    // public Vector3 EffectivePositionOffset => cameraPreset != null ? cameraPreset.positionOffset : positionOffset;
    // public Vector3 EffectiveRotationOffset => cameraPreset != null ? cameraPreset.rotationOffset : rotationOffset;
    // public float EffectiveFovOffset => cameraPreset != null ? cameraPreset.fovOffset : fovOffset;
    // public List<CameraEffect> EffectiveCameraEffects => cameraPreset != null ? cameraPreset.cameraEffects : cameraEffects;

    public override IEnumerator Play()
    {
        // VNTextData data = textData as VNTextData;
        
        // if(data == null)
        //     yield break;

        // if (this.courtCharacter == null)
        //     yield break;

        // yield return DialogueSystem.instance.RunBeforeCommands(data.commands);
        
        // CourtRoomCharacter courtCharacter = TrialManager.instance.characterStands.Find(stand => stand.character == character);

        // if (!usePrevCamera)
        // {
        //     TrialDialogueManager.instance.effectController.Reset();
        // }
        
        // if (courtCharacter != null)
        // {
        //     HandleVisibleCharacter(courtCharacter);
        // }

        // else
        // {
        //     HandleNonVisibleCharacter();
        // }

        
        // foreach (CameraEffect cameraEffect in EffectiveCameraEffects)
        // {
        //     TrialDialogueManager.instance.effectController.StartEffect(cameraEffect);
        // }

        // if (!DialogueSystem.instance.GetIsSkip())
        // {
        //     SoundManager.instance.PlaySoundEffect(voiceLine);
        // }
        
        // yield return DialogueSystem.instance.Say(this);
        yield return null;
    }

    // private void HandleVisibleCharacter(CourtRoomCharacter courtCharacter)
    // {
    //     if (!usePrevCamera)
    //     {
    //         TrialDialogueManager.instance.cameraController.TeleportToTarget(courtCharacter.transform, courtCharacter.heightPivot, EffectivePositionOffset, EffectiveRotationOffset, EffectiveFovOffset);
    //     }
        
    //     courtCharacter.SetSprite(character.emotions[expressionIndex]);
            
    //     IFaceable animator = DialogueSystem.instance.dialogueBoxAnimator as IFaceable;

    //     if (animator == null)
    //         return;
        
    //     if (!animator.IsVisible() && !GameLoop.instance.isActive)
    //     {
    //         animator.FaceAppear();
    //     }
    //     animator.ChangeFace(character.faceSprite);
    // }

    // private void HandleNonVisibleCharacter()
    // {
    //     CourtTextBoxAnimator animator = DialogueSystem.instance.dialogueBoxAnimator as CourtTextBoxAnimator;

    //     if (animator == null)
    //         return;
        
        
        
    //     if(animator.characterFace.isVisible)
    //        animator.characterFace.DiscussionFaceContainerDisappear(animator.duration);
    //     animator.ChangeFace(null);
    // }
    
}