using UnityEngine;

public class CourtRoomCharacter : MonoBehaviour
{
    public Character character;
    public SpriteRenderer spriteRenderer;
    void Start()
    {
        SetSprite(character.FindStateByName("default"));
    }

    private void SetSprite(CharacterState state)
    {
        if (state != null)
        {
            spriteRenderer.sprite = state.sprite;
        }
    }
}
