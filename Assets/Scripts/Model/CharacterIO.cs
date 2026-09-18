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

public class SpriteOption
{
    public string Label;
    public Sprite Sprite;
    public Texture2D Texture;
    public string SourcePath;
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

    public static void Save(CharacterJson data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrEmpty(data.name)) throw new ArgumentException("A character needs a name.", nameof(data));

        string folder = CharacterFolder(data.name);
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "character.json"), JsonUtility.ToJson(data, true));
    }

    public static CharacterJson Load(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        string path = Path.Combine(CharacterFolder(name), "character.json");
        if (!File.Exists(path)) return null;
        return JsonUtility.FromJson<CharacterJson>(File.ReadAllText(path));
    }

    public static List<string> GetAllNames()
    {
        if (!Directory.Exists(CharactersPath)) return new List<string>();
        return Directory.GetDirectories(CharactersPath)
            .Select(d => Path.GetFileName(d))
            .Where(n => File.Exists(Path.Combine(CharactersPath, n, "character.json")))
            .ToList();
    }

    public static List<SpriteOption> LoadAvailableSprites()
    {
        var options = new List<SpriteOption>();
        var used = new HashSet<string>();

        foreach (var sprite in Resources.LoadAll<Sprite>("UserData/Characters"))
        {
            string label = sprite.name;
            int index = 2;
            while (!used.Add(label))
                label = sprite.name + " (" + (sprite.texture != null ? sprite.texture.name : string.Empty) + " " + index++ + ")";

            options.Add(new SpriteOption { Label = label, Sprite = sprite, Texture = sprite.texture });
        }

        if (Directory.Exists(CharactersPath))
        {
            foreach (string file in Directory.EnumerateFiles(CharactersPath, "*.png", SearchOption.AllDirectories))
            {
                string rel = file.Substring(CharactersPath.Length).TrimStart('\\', '/').Replace('\\', '/');
                if (!used.Add(rel)) continue;

                Texture2D tex = LoadTexture(file);
                if (tex == null) continue;

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), DefaultPixelsPerUnit);
                options.Add(new SpriteOption { Label = rel, Sprite = sprite, Texture = tex, SourcePath = file });
            }
        }

        return options;
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

    public static bool SaveSpriteImage(SpriteOption option, string characterName, string fileName, out string warning)
    {
        warning = null;
        if (option == null) return false;

        string folder = CharacterFolder(characterName);
        Directory.CreateDirectory(folder);
        string dest = Path.Combine(folder, fileName);

        try
        {
            if (!string.IsNullOrEmpty(option.SourcePath) && File.Exists(option.SourcePath))
            {
                File.Copy(option.SourcePath, dest, true);
                return true;
            }

            if (option.Sprite != null)
            {
                Texture2D cropped = CropSprite(option.Sprite);
                if (cropped != null)
                {
                    File.WriteAllBytes(dest, cropped.EncodeToPNG());
                    UnityEngine.Object.Destroy(cropped);
                    return true;
                }
            }

            if (option.Texture != null && option.Texture.isReadable)
            {
                File.WriteAllBytes(dest, option.Texture.EncodeToPNG());
                return true;
            }
        }
        catch
        {
        }

        warning = "Could not extract an image for '" + option.Label + "'. Saved the sprite name as a reference instead.";
        return false;
    }

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

    private static Texture2D CropSprite(Sprite sprite)
    {
        if (sprite == null || sprite.texture == null) return null;

        Rect rect = sprite.textureRect;
        int width = Mathf.Clamp(Mathf.RoundToInt(rect.width), 1, 4096);
        int height = Mathf.Clamp(Mathf.RoundToInt(rect.height), 1, 4096);
        int x = Mathf.RoundToInt(rect.x);
        int y = Mathf.RoundToInt(rect.y);

        var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        RenderTexture previous = RenderTexture.active;

        try
        {
            Graphics.CopyTexture(sprite.texture, 0, 0, x, y, width, height, rt, 0, 0, 0, 0);
            RenderTexture.active = rt;

            var result = new Texture2D(width, height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            return result;
        }
        catch
        {
            return null;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
        }
    }
}