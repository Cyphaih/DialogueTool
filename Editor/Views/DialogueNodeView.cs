using System;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class DialogueNodeView : VisualElement
{
    public string NodeID { get; private set; }
   
    public bool IsSelected { get; private set; }

    private PortView _inputPort;
    private PortView _outputPort; // For simplicity, one output for now

    public FloatField extraTextTime;
    public FloatField noTextTime;
    public Toggle isBroTalking;


    public PortView inputPort => _inputPort;
    public PortView outputPort => _outputPort;

    private TextField titleTextField = new();
    private TextField _dialogueTextField = new();

    public string Title
    {
        get => titleTextField.value;
        set => titleTextField.value = value;

    }

    public string DialogueText
    {
        get => _dialogueTextField.value;
        set => _dialogueTextField.value = value;

    }

    private bool _isDragging;
    private Vector2 _dragStartPos;

    

    public PropertyField audioEventField;


    // Need this for SerialzeReference for the EventReference property field, seems clunky 
    public class DataHolder : ScriptableObject
    {
        public FMODUnity.EventReference FmodEvent;
    }


    public DataHolder _dataHolder;
    private SerializedObject _serializedObject;


   
    public void CreateFMODField()
    {
        // Create an instance in memory only (no asset file created)
        _dataHolder = ScriptableObject.CreateInstance<DataHolder>();
        _serializedObject = new SerializedObject(_dataHolder);

        // Now you can use PropertyField
        audioEventField = new PropertyField(_serializedObject.FindProperty("FmodEvent"), "Event");
        audioEventField.Bind(_serializedObject);
        SerializedProperty pathProp = _serializedObject.FindProperty("FmodEvent.Path");

        audioEventField.TrackPropertyValue(pathProp, (prop) =>
        {
            
            _serializedObject.ApplyModifiedProperties();

        });
        
        Add(audioEventField);

        
    }

    public DialogueNodeView(string id, string title, Vector2 position)
    {
        NodeID = id;
        Title = title;
        

        // Basic Positioning
        style.left = position.x;
        style.top = position.y;

        // Loading Styles
        AddToClassList("dialogue-node");

        // Build UI
        BuildUI();



    }


    public void CheckID()
    {
        if (string.IsNullOrEmpty(NodeID))
        {
            NodeID = System.Guid.NewGuid().ToString();
        }
    }

    private void BuildUI()
    {
        
        if (Title != null || Title == string.Empty) 
            titleTextField.value = Title;
        else
        {
            titleTextField.value = "New Dialogue Node";
        }
            titleTextField.AddToClassList("node-title");
        Add(titleTextField);

        CreateFMODField();

        extraTextTime = new FloatField("Extra Text Time");
        Add(extraTextTime);

        noTextTime = new FloatField("No Text Time");
        Add(noTextTime);

        isBroTalking = new Toggle("Bro Talking");
        Add(isBroTalking);
        // Body
        _dialogueTextField = new TextField();
        _dialogueTextField.multiline = true;
        _dialogueTextField.value = "Content goes here";
        _dialogueTextField.style.whiteSpace = WhiteSpace.Normal; // Allow wrap
        Add(_dialogueTextField);


        // Container for ports
        var portsContainer = new VisualElement();
        portsContainer.AddToClassList("port-container");
        Add(portsContainer);

        // Input Port
        _inputPort = new PortView(this, false);
        _inputPort.AddToClassList("input");
        _inputPort.tooltip = "Input";
        portsContainer.Add(_inputPort);



        // Output Port
        _outputPort = new PortView(this,true);
        _outputPort.AddToClassList("output");
        _outputPort.tooltip = "Output";
        portsContainer.Add(_outputPort);

        // Enable Dragging
        this.RegisterCallback<MouseDownEvent>(OnMouseDown);
        this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        this.RegisterCallback<MouseUpEvent>(OnMouseUp);
    }

    

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (evt.button == 0)
        {

            

            _isDragging = true;
            _dragStartPos = evt.localMousePosition;
            this.CaptureMouse();


            evt.StopPropagation();
        }
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (_isDragging)
        {
            // Calculate delta
            Vector2 delta = evt.localMousePosition - _dragStartPos;

            // Update Layout Position
            style.left = style.left.value.value + delta.x;
            style.top = style.top.value.value + delta.y;

            evt.StopPropagation();
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (_isDragging)
        {
            _isDragging = false;
            this.ReleaseMouse();

            evt.StopPropagation();
        }
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        if (selected)
            AddToClassList("selected");
        else
            RemoveFromClassList("selected");
    }
}

