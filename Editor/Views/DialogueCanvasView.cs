using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class DialogueCanvasView : VisualElement
{
    private VisualElement _contentContainer;


    //For panning and zooming
    private bool _isPanning;
    private Vector2 _panStart;
    private Vector2 _containerStartPos;

    //All connections are saved here
    private VisualElement _connectionsLayer;

    //All nodes are saved her
    private VisualElement _nodesLayer;
    private List<ConnectionView> _connectionViews = new List<ConnectionView>();
    private PortView _pendingConnectionSource;
    private Vector2 _pendingConnectionTarget;
    private bool _isMakingConnection;



    public DialogueCanvasView()
    {
        // Apply Styles
        var styleSheet = Resources.Load<StyleSheet>("DialogueToolStyles");
        if (styleSheet != null)
            styleSheets.Add(styleSheet);
        else
            Debug.LogWarning("DialogueToolStyles not found in Resources");

        AddToClassList("grid-background");
        style.flexGrow = 1;
        style.overflow = Overflow.Hidden; // Clip content that moves outside

        // Container for everything
        _contentContainer = new VisualElement();
        _contentContainer.name = "graph-content";
        _contentContainer.style.position = Position.Absolute;
        _contentContainer.style.left = 0;
        _contentContainer.style.top = 0;
        _contentContainer.style.width = 10000; // Large area
        _contentContainer.style.height = 10000;
        _contentContainer.style.translate = new Translate(0, 0, 0);
        _contentContainer.style.scale = new Scale(Vector3.one);
        _contentContainer.style.transformOrigin = new TransformOrigin(0, 0, 0);
        Add(_contentContainer);

        // Layer for connections (Before nodes so lines are behind)
        _connectionsLayer = new VisualElement();
        _connectionsLayer.name = "connections";
        // ConnectionsLayer now needs to Pick children (ConnectionViews).
        // We set pickingMode to Ignore so the layer itself (transparent) doesn't block clicks,
        // but its children (lines) can still be picked.
        _connectionsLayer.pickingMode = PickingMode.Ignore;

        _connectionsLayer.style.position = Position.Absolute;
        _connectionsLayer.style.left = 0;
        _connectionsLayer.style.top = 0;
        _connectionsLayer.style.width = 10000;
        _connectionsLayer.style.height = 10000;
        _contentContainer.Add(_connectionsLayer);

        // Layer for nodes
        _nodesLayer = new VisualElement();
        _nodesLayer.name = "nodes";
        _nodesLayer.pickingMode = PickingMode.Ignore;
        _nodesLayer.style.position = Position.Absolute;
        _nodesLayer.style.left = 0;
        _nodesLayer.style.top = 0;
        _nodesLayer.style.width = 10000;
        _nodesLayer.style.height = 10000;
        _contentContainer.Add(_nodesLayer);

        // For pending connection only
        _connectionsLayer.generateVisualContent += OnGenerateConnections;

        // Register Callbacks
        RegisterCallback<MouseDownEvent>(OnMouseDown);
        RegisterCallback<MouseMoveEvent>(OnMouseMove);
        RegisterCallback<MouseUpEvent>(OnMouseUp);
        RegisterCallback<WheelEvent>(OnWheel);
        RegisterCallback<KeyDownEvent>(OnKeyDown);

        // Focusable for keyboard events
        focusable = true;



        // Right Click Context Menu
        this.AddManipulator(new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Add Node", action => AddNode(action.eventInfo.localMousePosition));
        }));
    }

    private void OnKeyDown(KeyDownEvent evt)
    {

    }



    private void OnGenerateConnections(MeshGenerationContext mgc)
    {
        var painter = mgc.painter2D;
        painter.lineWidth = 3.0f;


        // Draw pending connection
        if (_isMakingConnection && _pendingConnectionSource != null)
        {
            Vector2 startPos = _pendingConnectionSource.parent.LocalToWorld(_pendingConnectionSource.layout.center);
            startPos = _connectionsLayer.WorldToLocal(startPos);

            Vector2 endPos = _connectionsLayer.WorldToLocal(_pendingConnectionTarget);

            painter.strokeColor = Color.yellow;
            DrawBezier(painter, startPos, endPos);
        }
    }

    private void DrawBezier(Painter2D painter, Vector2 start, Vector2 end)
    {
        painter.BeginPath();
        painter.MoveTo(start);

        float tangentLength = Mathf.Abs(start.x - end.x) * 0.5f;
        if (tangentLength < 50) tangentLength = 50;

        Vector2 control1 = start + new Vector2(tangentLength, 0);
        Vector2 control2 = end - new Vector2(tangentLength, 0);

        painter.BezierCurveTo(control1, control2, end);
        painter.Stroke();
    }


    private void OnMouseDown(MouseDownEvent evt)
    {


        if (evt.button == 2 || (evt.button == 0 && evt.altKey)) // Middle mouse or Alt+Left
        {
            _isPanning = true;
            _panStart = evt.localMousePosition;
            Translate currentTranslate = _contentContainer.style.translate.value;

            float x = currentTranslate.x.value;
            float y = currentTranslate.y.value;

            _containerStartPos = new Vector2(x, y);
            evt.StopPropagation();
        }
    }



    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (_isPanning)
        {
            var delta = evt.localMousePosition - _panStart;
            Vector2 newPos = _containerStartPos + delta;
            _contentContainer.style.translate = new Translate(newPos.x, newPos.y);
            evt.StopPropagation();
        }

        if (_isMakingConnection)
        {
            _pendingConnectionTarget = this.LocalToWorld(evt.localMousePosition);
            
            _connectionsLayer.MarkDirtyRepaint();
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (_isPanning)
        {
            _isPanning = false;
            evt.StopPropagation();
        }

        if(_isMakingConnection)
        {
            _isMakingConnection = false;
            _connectionsLayer.MarkDirtyRepaint();
        }

        
    }

    private void OnWheel(WheelEvent evt)
    {
        float zoomSpeed = 0.05f;

        float currentScale = _contentContainer.style.scale.value.value.x;

        float newScaleVal = currentScale - (evt.delta.y * zoomSpeed);
        newScaleVal = Mathf.Clamp(newScaleVal, 0.2f, 3f);


        Vector2 mousePos = evt.localMousePosition;
        Vector2 currentTranslate = new Vector2(_contentContainer.style.translate.value.x.value, _contentContainer.style.translate.value.y.value);

        Vector2 contentPointUnderMouse = (mousePos - currentTranslate) / currentScale;
        Vector2 newTranslate = mousePos - (contentPointUnderMouse * newScaleVal);

        _contentContainer.style.translate = new Translate(newTranslate.x, newTranslate.y);
        _contentContainer.style.scale = new Scale(new Vector3(newScaleVal, newScaleVal, 1));
        evt.StopPropagation();
    }

    private void AddNode(Vector2 mPos)
    {
        // Convert mouse pos to local container pos
        Vector2 localPos = _contentContainer.WorldToLocal(this.LocalToWorld(mPos));

        var node = new DialogueNodeView(System.Guid.NewGuid().ToString(), "New Node", localPos);
        _nodesLayer.Add(node);

        GenNodeCallbacks(node);
    }

    private void GenNodeCallbacks(DialogueNodeView nv)
    {
        nv.outputPort.RegisterCallback<MouseDownEvent>(e => OnPortMouseDown(e, nv.outputPort));
        nv.inputPort.RegisterCallback<MouseDownEvent>(e => OnPortMouseDown(e, nv.inputPort));
        nv.inputPort.RegisterCallback<MouseUpEvent>(e => OnPortMouseUp(e, nv.inputPort));

        // Trigger redraw when nodes move
        nv.RegisterCallback<MouseMoveEvent>(e =>
        {
            if (e.pressedButtons > 0)
            {
                foreach (var conn in _connectionViews) conn.MarkDirtyRepaint();
            }
        });

        nv.RegisterCallback<MouseDownEvent>(e => OnNodeMouseDown(e, nv));
    }
    private void OnPortMouseDown(MouseDownEvent evt, PortView port)
    {
        if (evt.button == 0 && !evt.ctrlKey)
        {
            _isMakingConnection = true;
            _pendingConnectionSource = port;
            
            evt.StopPropagation();
        }
        if (evt.ctrlKey && evt.button == 0)
        {
            RemoveOneConnection(port.conView);
            evt.StopPropagation();
        }

    }

    private void OnPortMouseUp(MouseUpEvent evt, PortView port)
    {

        if (_isMakingConnection && _pendingConnectionSource != null && _pendingConnectionSource != port)
        {
            // Validate connection
            bool sourceIsOutput = _pendingConnectionSource.isOutput;
            bool targetIsInput = !port.isOutput;



            // Simple validation: must connect Output to Input
            if (sourceIsOutput && targetIsInput )
            {
                CreateConnection(_pendingConnectionSource, port);
            }

            _isMakingConnection = false;
            _pendingConnectionSource = null;
            
            _connectionsLayer.MarkDirtyRepaint();
            evt.StopPropagation();
        }
    }


    private void OnNodeMouseDown(MouseDownEvent evt, VisualElement node)
    {
        if (evt.ctrlKey && evt.button == 0)
        {
            RemoveConnectionViewsOnNode(node);
            _nodesLayer.Remove(node);
            evt.StopPropagation();
        }
    }
    public List<ConnectionView> GetConnections()
    {
        return _connectionViews;
    }

    public void ClearGraph()
    {
        _nodesLayer.Clear();
        _connectionsLayer.Clear();
        _connectionViews.Clear();
        _pendingConnectionSource = null;
        _isMakingConnection = false;
    }

    public DialogueNodeView CreateNodeView(string id, string title, Vector2 pos)
    {
        var node = new DialogueNodeView(id, title, pos);
        _nodesLayer.Add(node);

        GenNodeCallbacks(node);
        return node;
    }


    public void CreateConnectionBetween(PortView output, PortView  input)
    {
        CreateConnection(output, input);
    }

    private void CreateConnection(PortView start, PortView end)
    {
        if (start.conView != null)
        {
            RemoveOneConnection(start.conView);
        }
        if (end.conView != null)
        {
            RemoveOneConnection(end.conView);
        }
        ConnectionView connView = new ConnectionView(start, end);
        start.conView = connView;
        end.conView = connView;

        _connectionViews.Add(connView);
        _connectionsLayer.Add(connView);
    }
    private void RemoveOneConnection(ConnectionView view)
    {
        if (view == null) return;
        view.input.ClearConnection();
        view.output.ClearConnection();
        if(_connectionsLayer.Contains(view))
            _connectionsLayer.Remove(view);
        if(_connectionViews.Contains(view))
            _connectionViews.Remove(view);
        
    }

    private void RemoveConnectionViewsOnNode(VisualElement nv)
    {

        for (int i = _connectionViews.Count - 1; i >= 0; i--)
        {
            ConnectionView connView = _connectionViews[i];
            if (connView.input.parent.parent == nv || connView.output.parent.parent == nv)
            {
                _connectionsLayer.Remove(connView);
                _connectionViews.Remove(connView);
            }
        }
    }

}


