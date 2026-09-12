using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterState
{
    public string name;
    public Sprite sprite;
}

public class CharacterWorldConfig
{
    public Vector2 size;
    public Vector2 offset;

    public float faceHeight;
}

public class Character
{
    public string id;
    public string name;
    public string displayName;
    public GameObject vnObjectPrefab;
    public List<CharacterState> emotions;
    public Sprite faceSprite;
    public bool notVisible;
    public bool noNameTag;
    public Color textColor = Color.white;
    public CharacterWorldConfig worldConfig;
    
    public CharacterState FindStateByName(string stateName)
    {
        if (emotions == null)
            return null;

        return emotions.FirstOrDefault(e => e.name == stateName);
    }

    public CharacterState FindStateBySprite(Sprite sprite)
    {
        if (emotions == null)
            return null;

        return emotions.FirstOrDefault(e => e.sprite == sprite);
    }
}
