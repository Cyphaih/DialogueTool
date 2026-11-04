using UnityEngine;
using FMODUnity;
public class AudioPlayOnTrigger : MonoBehaviour
{
    [SerializeField] private EventReference triggerSound;
    private void OnTriggerEnter(Collider other)
    {
        if(!triggerSound.IsNull && AudioManager.instance != null )
            AudioManager.instance.PlayOneShot(triggerSound,transform.position);
    }
}
