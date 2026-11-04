using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using UnityEngine.UIElements;


public static class GraphSaveUtility
{
    public static void SaveGraph(DialogueContainer container, DialogueCanvasView canvasView)
    {
        if (container == null)
        {
            Debug.LogError("No Container to save to!");
            return;
        }

        
        var existingSubAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(container));
        var existingNodeMap = new Dictionary<string, BaseNodeData>();

        foreach (var asset in existingSubAssets)
        {
            if (asset is BaseNodeData nodeData)
            {
                
                if (string.IsNullOrEmpty(nodeData.nodeGUID))
                {
                    continue;
                }

                if (existingNodeMap.ContainsKey(nodeData.nodeGUID))
                {
             
                    Debug.LogWarning($"Duplicate GUID {nodeData.nodeGUID} found during save. Cleaning up.");
                    Object.DestroyImmediate(asset, true);
                }
                else
                {
                    existingNodeMap.Add(nodeData.nodeGUID, nodeData);
                }
            }
        }

        
        container.AllNodes.Clear();

        
        var nodeViews = canvasView.Query<DialogueNodeView>().ToList();
        var processingNodeMap = new Dictionary<string, BaseNodeData>();

        foreach (DialogueNodeView nodeView in nodeViews)
        {
            BaseNodeData currentNode = null;

            //In case ID is null or empty, give new ID 
            //This might kill some links but Somehow one of the IDs is null and now I want to prevent that
            nodeView.CheckID();

            
            if (existingNodeMap.TryGetValue(nodeView.NodeID, out BaseNodeData foundNode))
            {
                currentNode = foundNode;

                existingNodeMap.Remove(nodeView.NodeID); 
            }
            else
            {
                
                currentNode = ScriptableObject.CreateInstance<DialogueNodeData>();
                currentNode.name = nodeView.Title;
                currentNode.nodeGUID = nodeView.NodeID;

                AssetDatabase.AddObjectToAsset(currentNode, container);
            }

            
            currentNode.name = nodeView.Title;
            currentNode.position = new Vector2(nodeView.style.left.value.value, nodeView.style.top.value.value);
            if (currentNode is DialogueNodeData dnd)
            {
                dnd.dialogueText = nodeView.DialogueText;
                dnd.dialogueAudio = nodeView._dataHolder.FmodEvent;
                dnd.noTextTime = nodeView.noTextTime.value;
                dnd.extraTextTime = nodeView.extraTextTime.value;
                dnd.isBroTalking = nodeView.isBroTalking.value;
            }

            currentNode.hideFlags = HideFlags.None;
            container.AllNodes.Add(currentNode);
            processingNodeMap.Add(currentNode.nodeGUID, currentNode);

        }

       
        foreach (var nodeToDelete in existingNodeMap.Values)
        {
            Object.DestroyImmediate(nodeToDelete, true);
        }

       
        var connectionViews = canvasView.GetConnections();

       
        foreach (var node in container.AllNodes)
        {
            node.nextNode = null;
            node.previousNode = null;
        }

        foreach (var conn in connectionViews)
        {
            var inputNodeView = conn.input.parent.parent as DialogueNodeView;
            var outputNodeView = conn.output.parent.parent as DialogueNodeView;

            if (inputNodeView != null && outputNodeView != null)
            {
                if (processingNodeMap.TryGetValue(outputNodeView.NodeID, out BaseNodeData sourceNode) &&
                    processingNodeMap.TryGetValue(inputNodeView.NodeID, out BaseNodeData targetNode))
                {
                    sourceNode.nextNode = targetNode;
                    targetNode.previousNode = sourceNode;
                }
            }
        }


        EditorUtility.SetDirty(container);

        AssetDatabase.SaveAssets();

        UnityEditor.ActiveEditorTracker.sharedTracker.ForceRebuild();

    }

    public static void LoadGraph(DialogueContainer container, DialogueCanvasView canvasView)
    {
        if (container == null) return;

        
        canvasView.ClearGraph();

        var nodeMap = new Dictionary<string, DialogueNodeView>();

        
        foreach (var nodeData in container.AllNodes)
        {
            if (string.IsNullOrEmpty(nodeData.nodeGUID))
            {
                nodeData.nodeGUID = System.Guid.NewGuid().ToString();
            }
            
            if (nodeData is DialogueNodeData dialogData)
            {
                var view = canvasView.CreateNodeView(nodeData.nodeGUID, nodeData.name, nodeData.position);
                view.DialogueText = dialogData.dialogueText;
                view._dataHolder.FmodEvent = dialogData.dialogueAudio;
                view.extraTextTime.value = dialogData.extraTextTime;
                view.noTextTime.value = dialogData.noTextTime;
                view.isBroTalking.value = dialogData.isBroTalking;
            }
            
        }

  
        var allNodeViews = canvasView.Query<DialogueNodeView>().ToList();
        foreach (var nv in allNodeViews) nodeMap[nv.NodeID] = nv;

        foreach (var nodeData in container.AllNodes)
        {
            if (nodeData.nextNode != null)
            {
                if (nodeMap.ContainsKey(nodeData.nodeGUID) && nodeMap.ContainsKey(nodeData.nextNode.nodeGUID))
                {
                    var sourceView = nodeMap[nodeData.nodeGUID];
                    var targetView = nodeMap[nodeData.nextNode.nodeGUID];

                    canvasView.CreateConnectionBetween(sourceView.outputPort, targetView.inputPort);
                }
            }
        }
    }
}

