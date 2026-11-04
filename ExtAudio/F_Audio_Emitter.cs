using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(StudioEventEmitter))]
public class F_Audio_Emitter : MonoBehaviour
{
    [SerializeField] bool playOnStart = true;
    [SerializeField] bool stopOnDestroy = true;

    private StudioEventEmitter emitter;
    [SerializeField] private EventReference soundEvent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emitter = AudioManager.instance.InitializeEventEmitter(soundEvent, gameObject);
        if(playOnStart)
            emitter.Play();
    }

    private void OnDestroy()
    {
        if(stopOnDestroy)
            emitter.Stop();
    }

    public void ChangeRef(EventReference eventRef)
    {
        emitter.Stop();
        
        emitter.enabled = false;
        emitter.EventReference = eventRef;
        emitter.enabled = true;  
        emitter.Play();
    }

    public void ClearRef()
    {
        
        emitter.Stop();
        emitter.EventReference = new EventReference();
    }


}
