using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterCreatorPanel : MonoBehaviour
{
    private const int TotalStages = 3;
    private const string UxmlPath = "UI/CharacterCreator/CharacterCreator";
    private const string UssPath = "UI/CharacterCreator/CharacterCreator";
    private const string NoneOption = "<None>";

    private UIDocument document;
    private PanelSettings panelSettings;
    private List<SpriteOption> spriteOptions = new List<SpriteOption>();

    private int currentStage;

    private VisualElement stage1;
    private VisualElement stage2;
    private VisualElement stage3;
    private Label progressLabel;
    private VisualElement progressFill;
    private Label statusLabel;
    private Button backButton;
    private Button nextButton;

    private TextField nameField;
    private TextField displayNameField;
    private Toggle noNameTagToggle;
    private Toggle notVisibleToggle;

    private DropdownField faceSpriteField;
    private VisualElement emotionsList;

    private Vector2Field sizeField;
    private Vector2Field offsetField;
    private FloatField faceHeightField;

    public void Show()
    {
        EnsureCreated();
        document.rootVisualElement.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (document != null)
            document.rootVisualElement.style.display = DisplayStyle.None;
    }

    [ContextMenu("Show Panel")]
    private void ShowFromInspector() => Show();

    [ContextMenu("Hide Panel")]
    private void HideFromInspector() => Hide();

    private void Start()
    {
        Show();
    }

    private void EnsureCreated()
    {
        if (document != null) return;

        panelSettings = ScriptableObject.CreateInstance<PanelSettings>();

        document = gameObject.AddComponent<UIDocument>();
        document.panelSettings = panelSettings;

        VisualTreeAsset tree = Resources.Load<VisualTreeAsset>(UxmlPath);
        StyleSheet style = Resources.Load<StyleSheet>(UssPath);

        VisualElement root = document.rootVisualElement;
        if (tree != null)
            tree.CloneTree(root);

        if (style != null)
            root.styleSheets.Add(style);

        Bind(root);
        RefreshSpriteOptions();
        ShowStage(0);
    }

    private void Bind(VisualElement root)
    {
        stage1 = root.Q("stage-1");
        stage2 = root.Q("stage-2");
        stage3 = root.Q("stage-3");
        progressLabel = root.Q<Label>("progress-label");
        progressFill = root.Q("progress-fill");
        statusLabel = root.Q<Label>("status-label");

        nameField = root.Q<TextField>("name-field");
        displayNameField = root.Q<TextField>("display-name-field");
        noNameTagToggle = root.Q<Toggle>("no-name-tag-field");
        notVisibleToggle = root.Q<Toggle>("not-visible-field");

        faceSpriteField = root.Q<DropdownField>("face-sprite-field");
        emotionsList = root.Q("emotions-list");

        sizeField = root.Q<Vector2Field>("size-field");
        offsetField = root.Q<Vector2Field>("offset-field");
        faceHeightField = root.Q<FloatField>("face-height-field");

        backButton = root.Q<Button>("back-button");
        nextButton = root.Q<Button>("next-button");

        backButton.clicked += () => ShowStage(currentStage - 1);
        nextButton.clicked += OnNext;

        root.Q<Button>("add-emotion-button").clicked += AddEmotionRow;

        sizeField.value = new Vector2(1f, 1f);
        offsetField.value = Vector2.zero;
        faceHeightField.value = 1.5f;
    }

    private void RefreshSpriteOptions()
    {
        foreach (var option in spriteOptions)
        {
            if (option.SourcePath == null) continue;
            if (option.Texture != null) Object.Destroy(option.Texture);
            if (option.Sprite != null) Object.Destroy(option.Sprite);
        }
        spriteOptions = CharacterIO.LoadAvailableSprites();

        string[] choices = new[] { NoneOption }.Concat(spriteOptions.Select(o => o.Label)).ToArray();

        faceSpriteField.choices = choices.ToList<string>();
        if (string.IsNullOrEmpty(faceSpriteField.value))
            faceSpriteField.value = NoneOption;

        foreach (var row in emotionsList.Children())
        {
            var dropdown = row.Q<DropdownField>("emotion-sprite");
            dropdown.choices = choices.ToList<string>();
        }
    }

    private void ShowStage(int stage)
    {
        currentStage = Mathf.Clamp(stage, 0, TotalStages - 1);

        stage1.style.display = currentStage == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        stage2.style.display = currentStage == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        stage3.style.display = currentStage == 2 ? DisplayStyle.Flex : DisplayStyle.None;

        backButton.style.display = currentStage == 0 ? DisplayStyle.None : DisplayStyle.Flex;
        nextButton.text = currentStage == TotalStages - 1 ? "Create Character" : "Next";

        progressLabel.text = "Step " + (currentStage + 1) + " of " + TotalStages;
        progressFill.style.width = Length.Percent((currentStage + 1f) / TotalStages * 100f);
    }

    private void OnNext()
    {
        if (!ValidateStage())
        {
           return; 
        }
            

        if (currentStage == TotalStages - 1)
            CreateCharacter();
        else
            ShowStage(currentStage + 1);
    }

    private bool ValidateStage()
    {
        if (currentStage == 0)
        {
            if (string.IsNullOrWhiteSpace(nameField.value))
            {
                SetStatus("Name is required before continuing.", true);
                return false;
            }
        }

        SetStatus(string.Empty, false);
        return true;
    }

    private void AddEmotionRow()
    {
        var row = new VisualElement();
        row.AddToClassList("emotion-row");

        var nameField = new TextField("Emotion") { name = "emotion-name", label = "Emotion" };
        nameField.AddToClassList("emotion-name");
        row.Add(nameField);

        var spriteField = new DropdownField("Sprite") { name = "emotion-sprite" };
        spriteField.AddToClassList("emotion-sprite");
        spriteField.choices = new[] { NoneOption }.Concat(spriteOptions.Select(o => o.Label)).ToList();
        spriteField.value = NoneOption;
        row.Add(spriteField);

        var removeButton = new Button { text = "X" };
        removeButton.AddToClassList("emotion-remove");
        removeButton.clicked += () => emotionsList.Remove(row);
        row.Add(removeButton);

        emotionsList.Add(row);
    }

    private void CreateCharacter()
    {
        string characterName = nameField.value.Trim();
        string sanitizedName = CharacterIO.Sanitize(characterName);

        var warnings = new List<string>();
        var emotions = new List<CharacterStateJson>();
        var usedEmotionNames = new HashSet<string>();

        foreach (var element in emotionsList.Children())
        {
            string emotionName = element.Q<TextField>("emotion-name").value?.Trim();
            if (string.IsNullOrEmpty(emotionName))
                continue;

            SpriteOption option = FindOption(element.Q<DropdownField>("emotion-sprite").value);
            string spriteRef = null;
            if (option != null)
            {
                string filename = EmotionFileName(emotionName, usedEmotionNames);
                if (CharacterIO.SaveSpriteImage(option, sanitizedName, filename, out string warning))
                {
                    spriteRef = filename;
                }
                else
                {
                    spriteRef = option.Label;
                    if (warning != null) warnings.Add(warning);
                }
            }

            emotions.Add(new CharacterStateJson { name = emotionName, sprite = spriteRef });
        }

        string faceFile = null;
        SpriteOption faceOption = FindOption(faceSpriteField.value);
        if (faceOption != null)
        {
            if (CharacterIO.SaveSpriteImage(faceOption, sanitizedName, "face.png", out string warning))
                faceFile = "face.png";
            else
                faceFile = faceOption.Label;
            if (warning != null) warnings.Add(warning);
        }

        var data = new CharacterJson
        {
            id = sanitizedName,
            name = sanitizedName,
            displayName = string.IsNullOrWhiteSpace(displayNameField.value) ? sanitizedName : displayNameField.value,
            noNameTag = noNameTagToggle.value,
            notVisible = notVisibleToggle.value,
            faceSprite = faceFile,
            emotions = emotions,
            worldConfig = new CharacterWorldConfigJson
            {
                size = sizeField.value,
                offset = offsetField.value,
                faceHeight = faceHeightField.value
            }
        };

        CharacterIO.Save(data);

        string message = "Character '" + data.displayName + "' saved to:\n" + CharacterIO.CharacterFolder(sanitizedName);
        if (warnings.Count > 0)
            message += "\n" + string.Join("\n", warnings.Distinct());
        Debug.Log(message);

        SetStatus("Saved! Ready for the next character.", false);
        ResetForm();
    }

    private static string EmotionFileName(string emotionName, HashSet<string> used)
    {
        string safeName = CharacterIO.Sanitize(emotionName);
        string name = safeName + ".png";
        int index = 1;
        while (!used.Add(name.ToLowerInvariant()))
        {
            name = safeName + " (" + index + ").png";
            index++;
        }
        return name;
    }

    private SpriteOption FindOption(string label)
    {
        if (label == NoneOption) return null;
        return spriteOptions.FirstOrDefault(o => o.Label == label);
    }

    private void ResetForm()
    {
        nameField.value = string.Empty;
        displayNameField.value = string.Empty;
        noNameTagToggle.value = false;
        notVisibleToggle.value = false;

        faceSpriteField.value = NoneOption;
        while (emotionsList.childCount > 0)
            emotionsList.RemoveAt(0);

        sizeField.value = new Vector2(1f, 1f);
        offsetField.value = Vector2.zero;
        faceHeightField.value = 1.5f;

        RefreshSpriteOptions();
        ShowStage(0);
    }

    private void SetStatus(string message, bool isError)
    {
        statusLabel.text = message;
        statusLabel.EnableInClassList("error", isError);
    }

    private void OnDestroy()
    {
        if (panelSettings != null) Destroy(panelSettings);
        foreach (var option in spriteOptions)
        {
            if (option.SourcePath == null) continue;
            if (option.Texture != null) Destroy(option.Texture);
        }
    }
}