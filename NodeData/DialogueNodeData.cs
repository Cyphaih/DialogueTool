using UnityEngine;

[System.Serializable]
public class DialogueNodeData : BaseNodeData
{
    public FMODUnity.EventReference dialogueAudio;
    public string dialogueText;

    public float extraTextTime;
    public float noTextTime;

    public bool isBroTalking;
}
