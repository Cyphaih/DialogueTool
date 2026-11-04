using UnityEngine;
using UnityEngine.UIElements;

public class PortView : VisualElement
{
    public DialogueNodeView parentNode{ get; private set; }
    public bool isOutput { get; private set; }
    public bool canConnect {  get; private set; }

    public  ConnectionView conView { get; set; }
    public PortView(DialogueNodeView nv, bool _isOutput) 
    {
        AddToClassList("port");
        parentNode = nv;
        isOutput = _isOutput;
    }

    

    public void ClearConnection()
    {
        conView = null;
    }

}
