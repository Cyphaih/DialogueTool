using UnityEngine;

public class DialogueEvent : MonoBehaviour
{
    [SerializeField] BaseNodeData data;
    bool isSingleUse;
    bool used;
    public void PlayDialogue()
    {
        if (DialogueManager.instance == null) return;

        if(!isSingleUse || !used)
        {
            used = true;
            DialogueManager.instance.PlayDialogue(data);
        }
        
    }
}
