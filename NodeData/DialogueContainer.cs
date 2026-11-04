using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue Tool/Dialogue Container")]
public class DialogueContainer : ScriptableObject
{
    [SerializeField] public List<BaseNodeData> AllNodes = new List<BaseNodeData>();
}
