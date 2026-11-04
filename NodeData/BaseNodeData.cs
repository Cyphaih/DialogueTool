using UnityEngine;

public class BaseNodeData : ScriptableObject
{

    [HideInInspector] public string nodeGUID;
    [HideInInspector] public Vector2 position;

    [HideInInspector] public BaseNodeData nextNode;
    [HideInInspector] public BaseNodeData previousNode;

    
    private void OnValidate()
    {
        if(nextNode != null && nextNode.previousNode != this)
        {
            nextNode = null;
        }
        if (previousNode != null && previousNode.nextNode != this)
        {
            previousNode = null;
        }
    }


}
