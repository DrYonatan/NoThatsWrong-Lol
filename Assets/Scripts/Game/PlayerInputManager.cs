using UnityEngine;

namespace DIALOGUE
{
    public class PlayerInputManager : MonoBehaviour
    {
        public static PlayerInputManager instance { get; private set; }

        public bool isInputActive;

        public bool isDialogueInputActive;

        void Awake()
        {
            instance = this;
            isDialogueInputActive = true;
            isInputActive = true;
        }

        void Update()
        {
            if (!isInputActive)
                return;
            
            if (isDialogueInputActive)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Confined;
                
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) ||
                        Input.GetKeyDown(KeyCode.LeftControl))
                    {
                        PromptAdvance();
                    }

                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        DialogueSystem.instance.SetSkip(true);
                    }
                    else
                    {
                        DialogueSystem.instance.SetSkip(false);
                    }
                
            }
        }


        public void EnableInput()
        {
            isInputActive = true;
        }

        public void DisableInput()
        {
            isInputActive = false;
        }

        public bool DefaultInput()
        {
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
        }

        public void PromptAdvance()
        {
            DialogueSystem.instance.OnUserPrompt_Next();  
        }
    }
}