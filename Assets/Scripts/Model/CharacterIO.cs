using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using SFB;

[Serializable]
public class CharacterJson
{
    public string id;
    public string name;
    public string displayName;
    public List<CharacterStateJson> emotions;
    public string faceSprite;
    public bool notVisible;
    public bool noNameTag;
    public Color textColor = Color.white;
    public CharacterWorldConfigJson worldConfig;
}

[Serializable]
public class CharacterStateJson
{
    public string name;
    public string sprite;
}

[Serializable]
public class CharacterWorldConfigJson
{
    public Vector2 size;
    public Vector2 offset;
    public float faceHeight;

    public static CharacterWorldConfigJson From(CharacterWorldConfig config)
    {
        if (config == null) return null;
        return new CharacterWorldConfigJson
        {
            size = config.size,
            offset = config.offset,
            faceHeight = config.faceHeight
        };
    }
}

public static class CharacterIO
{
    public const float DefaultPixelsPerUnit = 100f;

    public static string CharactersPath => Path.Combine(Application.persistentDataPath, "Characters");

    public static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Where(c => !invalid.Contains(c)).ToArray());
    }

    public static string CharacterFolder(string name) => Path.Combine(CharactersPath, Sanitize(name));

    public static bool Exists(string name)
    {
        return !string.IsNullOrEmpty(name)
            && File.Exists(Path.Combine(CharacterFolder(name), "character.json"));
    }

    public static CharacterJson CharacterToJson(Character character)
    {
        if (character == null) return null;

        return new CharacterJson
        {
            id = character.id,
            name = character.name,
            displayName = character.displayName,
            notVisible = character.notVisible,
            noNameTag = character.noNameTag,
            textColor = character.textColor,
            faceSprite = character.faceSprite != null ? character.faceSprite.name : null,
            emotions = character.emotions?.Select(e => new CharacterStateJson
            {
                name = e.name,
                sprite = e.sprite != null ? e.sprite.name : null
            }).ToList(),
            worldConfig = CharacterWorldConfigJson.From(character.worldConfig)
        };
    }

    public static Character JsonToCharacter(CharacterJson data)
    {
        if (data == null) return null;

        var character = new Character
        {
            id = data.id,
            name = data.name,
            displayName = data.displayName,
            notVisible = data.notVisible,
            noNameTag = data.noNameTag,
            textColor = data.textColor,
            faceSprite = LoadSpriteFile(data.name, data.faceSprite),
            emotions = new List<CharacterState>(),
            worldConfig = data.worldConfig == null
                ? null
                : new CharacterWorldConfig
                {
                    size = data.worldConfig.size,
                    offset = data.worldConfig.offset,
                    faceHeight = data.worldConfig.faceHeight
                }
        };

        if (data.emotions != null)
        {
            foreach (var emotion in data.emotions)
            {
                character.emotions.Add(new CharacterState
                {
                    name = emotion.name,
                    sprite = LoadSpriteFile(data.name, emotion.sprite)
                });
            }
        }

        return character;
    }

    public static void Save(Character character)
    {
        if (character == null) throw new ArgumentNullException(nameof(character));
        if (string.IsNullOrEmpty(character.name)) throw new ArgumentException("A character needs a name.", nameof(character));

        string folder = CharacterFolder(character.name);
        Directory.CreateDirectory(folder);

        SaveSpriteImage(character.faceSprite, folder, "face.png");
        if (character.emotions != null)
        {
            for (int i = 0; i < character.emotions.Count; i++)
            {
                CharacterState emotion = character.emotions[i];
                string fileName = string.IsNullOrEmpty(emotion.name) ? "emotion" + i : emotion.name;
                SaveSpriteImage(emotion.sprite, folder, Sanitize(fileName) + ".png");
            }
        }

        CharacterJson data = CharacterToJson(character);
        File.WriteAllText(Path.Combine(folder, "character.json"), JsonUtility.ToJson(data, true));
    }

    // Writes the sprite's texture as a PNG into the character folder and renames the
    // sprite to match, so CharacterToJson stores a file name LoadSpriteFile can find.
    private static void SaveSpriteImage(Sprite sprite, string folder, string fileName)
    {
        if (sprite == null || sprite.texture == null) return;

        File.WriteAllBytes(Path.Combine(folder, fileName), sprite.texture.EncodeToPNG());
        sprite.name = fileName;
    }

    public static Character Load(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        string path = Path.Combine(CharacterFolder(name), "character.json");
        if (!File.Exists(path)) return null;
        return JsonToCharacter(JsonUtility.FromJson<CharacterJson>(File.ReadAllText(path)));
    }

    public static string SpriteSelection()
    {
        var extensions = new[]
        {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };

        var paths = StandaloneFileBrowser.OpenFilePanel(
           "Select Image",
           "",
           extensions,
           false
        );

        if (paths.Length > 0)
        {
            string selectedPath = paths[0];

            return selectedPath;
        }

        return null;
    }

    public static Texture2D LoadTexture(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(File.ReadAllBytes(path)))
            {
                UnityEngine.Object.Destroy(tex);
                return null;
            }
            return tex;
        }
        catch
        {
            return null;
        }
    }

    // public static bool SaveSpriteImage(SpriteOption option, string characterName, string fileName, out string warning)
    // {
    //     warning = null;
    //     if (option == null) return false;

    //     string folder = CharacterFolder(characterName);
    //     Directory.CreateDirectory(folder);
    //     string dest = Path.Combine(folder, fileName);

    //     try
    //     {
    //         if (!string.IsNullOrEmpty(option.SourcePath) && File.Exists(option.SourcePath))
    //         {
    //             File.Copy(option.SourcePath, dest, true);
    //             return true;
    //         }

    //         if (option.Sprite != null)
    //         {
    //             Texture2D cropped = CropSprite(option.Sprite);
    //             if (cropped != null)
    //             {
    //                 File.WriteAllBytes(dest, cropped.EncodeToPNG());
    //                 UnityEngine.Object.Destroy(cropped);
    //                 return true;
    //             }
    //         }

    //         if (option.Texture != null && option.Texture.isReadable)
    //         {
    //             File.WriteAllBytes(dest, option.Texture.EncodeToPNG());
    //             return true;
    //         }
    //     }
    //     catch
    //     {
    //     }

    //     warning = "Could not extract an image for '" + option.Label + "'. Saved the sprite name as a reference instead.";
    //     return false;
    // }

    public static Sprite LoadSpriteFile(string characterName, string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;
        string path = Path.Combine(CharacterFolder(characterName), Sanitize(fileName));
        if (!File.Exists(path)) return null;

        Texture2D tex = LoadTexture(path);
        if (tex == null) return null;

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), DefaultPixelsPerUnit);
    }

    public static Character ToCharacter(CharacterJson data)
    {
        if (data == null) return null;

        var character = new Character
        {
            id = data.id,
            name = data.name,
            displayName = data.displayName,
            notVisible = data.notVisible,
            noNameTag = data.noNameTag,
            textColor = data.textColor,
            faceSprite = LoadSpriteFile(data.name, data.faceSprite),
            emotions = new List<CharacterState>(),
            worldConfig = data.worldConfig == null
                ? null
                : new CharacterWorldConfig
                {
                    size = data.worldConfig.size,
                    offset = data.worldConfig.offset,
                    faceHeight = data.worldConfig.faceHeight
                }
        };

        if (data.emotions != null)
        {
            foreach (var emotion in data.emotions)
            {
                character.emotions.Add(new CharacterState
                {
                    name = emotion.name,
                    sprite = LoadSpriteFile(data.name, emotion.sprite)
                });
            }
        }

        return character;
    }
}