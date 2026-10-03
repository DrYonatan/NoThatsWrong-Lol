using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterCreatorPanel : MonoBehaviour, IEditorMenu
{
    public Character character = new Character();
    public UIDocument document;

    private VisualElement stage1;
    private VisualElement stage2;
    private VisualElement stage3;

    private TextField nameField;
    private TextField displayNameField;

    private Toggle noNameTag;
    private Toggle notVisible;


    private Button faceSpriteButton;
    private Button addEmotionButton;
    private VisualElement emotionsList;

    private Vector2Field sizeField;
    private Vector2Field offsetField;
    private FloatField faceHeightField;

    private Button nextButton;
    private Button backButton;
    private Label statusLabel;
    private Label progressLabel;
    private VisualElement progressFill;

    private const int StageCount = 3;
    private int currentStage = 1;

    void Start()
    {
        AssignUmlElements();
    }

    private void AssignUmlElements()
    {
        document = gameObject.GetComponent<UIDocument>();
        stage1 = document.rootVisualElement.Q("stage-1");
        stage2 = document.rootVisualElement.Q("stage-2");
        stage3 = document.rootVisualElement.Q("stage-3");
        nextButton = document.rootVisualElement.Q<Button>("next-button");
        nameField = document.rootVisualElement.Q<TextField>("name-field");
        displayNameField = document.rootVisualElement.Q<TextField>("display-name-field");
        noNameTag = document.rootVisualElement.Q<Toggle>("no-name-tag-field");
        notVisible = document.rootVisualElement.Q<Toggle>("not-visible-field");
        faceSpriteButton = document.rootVisualElement.Q<Button>("face-sprite-button");
        addEmotionButton = document.rootVisualElement.Q<Button>("add-emotion-button");
        emotionsList = document.rootVisualElement.Q("emotions-list");
        sizeField = document.rootVisualElement.Q<Vector2Field>("size-field");
        offsetField = document.rootVisualElement.Q<Vector2Field>("offset-field");
        faceHeightField = document.rootVisualElement.Q<FloatField>("face-height-field");
        backButton = document.rootVisualElement.Q<Button>("back-button");
        statusLabel = document.rootVisualElement.Q<Label>("status-label");
        progressLabel = document.rootVisualElement.Q<Label>("progress-label");
        progressFill = document.rootVisualElement.Q("progress-fill");

        nextButton.clicked += OnNextClicked;
        backButton.clicked += OnBackClicked;
        faceSpriteButton.clicked += SetFaceTexture;
        addEmotionButton.clicked += AddEmotion;
    }

    private void InitializeMenu()
    {
        ShowStage(1);
    }

    public void Show()
    {
        InitializeMenu();
    }
    public void Hide()
    {
        stage1.style.display = DisplayStyle.None;
        stage2.style.display = DisplayStyle.None;
        stage3.style.display = DisplayStyle.None;
    }

    private Sprite CreateSprite()
    {
        return CreateSprite(CharacterIO.SpriteSelection());
    }

    private Sprite CreateSprite(string path)
    {
        Texture2D tex = CharacterIO.LoadTexture(path);

        if (tex == null) return null;

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), CharacterIO.DefaultPixelsPerUnit);
    }

    private void SetFaceTexture()
    {
        character.faceSprite = CreateSprite();
    }

    private void AddEmotion()
    {
        if (character.emotions == null)
            character.emotions = new List<CharacterState>();

        CharacterState emotion = new CharacterState();
        character.emotions.Add(emotion);

        VisualElement row = new VisualElement();
        row.AddToClassList("emotion-row");

        TextField emotionNameField = new TextField("Name");
        emotionNameField.AddToClassList("emotion-name");
        emotionNameField.RegisterValueChangedCallback(evt => emotion.name = evt.newValue);

        Button spriteButton = new Button { text = "Choose Sprite" };
        spriteButton.AddToClassList("emotion-sprite");
        spriteButton.clicked += () =>
        {
            string path = CharacterIO.SpriteSelection();
            Sprite sprite = CreateSprite(path);
            if (sprite == null) return;

            emotion.sprite = sprite;
            spriteButton.text = Path.GetFileName(path);
        };

        Button removeButton = new Button { text = "X" };
        removeButton.AddToClassList("emotion-remove");
        removeButton.clicked += () =>
        {
            character.emotions.Remove(emotion);
            row.RemoveFromHierarchy();
        };

        row.Add(emotionNameField);
        row.Add(spriteButton);
        row.Add(removeButton);
        emotionsList.Add(row);
    }

    private void ShowStage(int stage)
    {
        currentStage = stage;

        stage1.style.display = stage == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        stage2.style.display = stage == 2 ? DisplayStyle.Flex : DisplayStyle.None;
        stage3.style.display = stage == 3 ? DisplayStyle.Flex : DisplayStyle.None;

        backButton.style.display = stage > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        nextButton.text = stage == StageCount ? "Create Character" : "Next";

        progressLabel.text = $"Step {stage} of {StageCount}";
        progressFill.style.width = Length.Percent(100f * stage / StageCount);

        SetStatus("", false);
    }

    private void OnNextClicked()
    {
        if (currentStage < StageCount)
            ShowStage(currentStage + 1);
        else
            SaveCharacter();
    }

    private void OnBackClicked()
    {
        if (currentStage > 1)
            ShowStage(currentStage - 1);
    }

    private void SetStatus(string message, bool isError)
    {
        statusLabel.text = message;
        statusLabel.EnableInClassList("error", isError);
    }

    public void SaveCharacter()
    {
        if (string.IsNullOrWhiteSpace(nameField.value))
        {
            SetStatus("The character needs a name (step 1).", true);
            return;
        }

        character.id = nameField.value;
        character.name = nameField.value;
        character.displayName = displayNameField.value;
        character.noNameTag = noNameTag.value;
        character.notVisible = notVisible.value;
        character.worldConfig = new CharacterWorldConfig
        {
            size = sizeField.value,
            offset = offsetField.value,
            faceHeight = faceHeightField.value
        };

        try
        {
            CharacterIO.Save(character);
            SetStatus($"Saved \"{character.name}\".", false);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            SetStatus("Failed to save: " + e.Message, true);
        }
    }
}