using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class DialogueEditorWindow : EditorWindow
{
    private DialogueCanvasView _canvas;
    private DialogueContainer _currentContainer;

    ObjectField objectField;

    [MenuItem("Window/Dialogue Tool")]
    public static void OpenWindow()
    {
        DialogueEditorWindow wnd = GetWindow<DialogueEditorWindow>();
        wnd.titleContent = new GUIContent("Dialogue Tool");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // Toolbar
        var toolbar = new Toolbar();
        root.Add(toolbar);

        // Container Field
        var containerField = new ObjectField("Container")
        {
            objectType = typeof(DialogueContainer),
            allowSceneObjects = false
        };
        objectField = containerField;
        containerField.RegisterValueChangedCallback(evt =>
        {
            _currentContainer = evt.newValue as DialogueContainer;
            GraphSaveUtility.LoadGraph(_currentContainer, _canvas);
        });
        toolbar.Add(containerField);



        // Save Button
        toolbar.Add(new UnityEngine.UIElements.Button(() =>
        {
            if (objectField.value == null)
            {
                DialogueContainer newDC = ScriptableObject.CreateInstance<DialogueContainer>();
                string folderPath = "Assets/ScritpableObjects/DialogueContainers";
                string name = "NewDialogue.asset";
                
                if (!AssetDatabase.IsValidFolder(folderPath))
                {
                    folderPath = "Assets";
                }

                string uniquePath = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + name);
                AssetDatabase.CreateAsset(newDC, uniquePath);
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();
                Selection.activeObject = newDC;
                objectField.value = newDC;
                objectField.MarkDirtyRepaint();
                _currentContainer = newDC;
            }

            GraphSaveUtility.SaveGraph(_currentContainer, _canvas);
        })
        { text = "Save Data" });

        // Load Button
        toolbar.Add(new UnityEngine.UIElements.Button(() =>
        {
            GraphSaveUtility.LoadGraph(_currentContainer, _canvas);
        })
        { text = "Load Data" });

        // Add the canvas
        _canvas = new DialogueCanvasView();
        root.Add(_canvas);
    }





}

