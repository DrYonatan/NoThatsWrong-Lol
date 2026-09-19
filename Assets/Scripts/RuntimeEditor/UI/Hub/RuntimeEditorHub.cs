using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RuntimeEditorHub : MonoBehaviour
{
    [Serializable]
    public class MenuEntry
    {
        public string label;
        public MonoBehaviour panel;
    }

    private const string UxmlPath = "UI/Hub/EditorHub";
    private const string UssPath = "UI/Hub/EditorHub";

    [SerializeField] private List<MenuEntry> menuEntries = new List<MenuEntry>();

    private UIDocument document;
    private PanelSettings panelSettings;
    private VisualElement menuList;

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

        menuList = root.Q("menu-list");
        BuildMenu();
    }

    private void BuildMenu()
    {
        menuList.Clear();

        foreach (MenuEntry entry in menuEntries)
        {
            if (entry.panel == null || string.IsNullOrEmpty(entry.label))
                continue;

            var hubPanel = entry.panel as IHubPanel;
            if (hubPanel == null)
            {
                Debug.LogWarning("RuntimeEditorHub: '" + entry.panel.name + "' does not implement IHubPanel and will be skipped.");
                continue;
            }

            var button = new Button { text = entry.label };
            button.AddToClassList("menu-button");
            button.clicked += () => OpenPanel(hubPanel);
            menuList.Add(button);
        }
    }

    private void OpenPanel(IHubPanel panel)
    {
        Hide();
        panel.Show(Show);
    }

    private void OnDestroy()
    {
        if (panelSettings != null)
            Destroy(panelSettings);
    }
}
