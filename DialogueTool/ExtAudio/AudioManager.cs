using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;


//I cut out all references to the rest of our project to decaosulate and show implementaion

public class AudioManager : MonoBehaviour
{

    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume   = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float ambienceVolume = 1;
    [Range(0, 1)]
    public float sfxVolume      = 1;
    [Range(0, 1)]
    public float dialogueVolume = 1;

    private Bus masterBus;
    private Bus musicBus;
    private Bus ambienceBus;
    private Bus sfxBus;
    private Bus dialogueBus;

    List<StudioEventEmitter> eventEmitters;
    List<EventInstance> eventInstances;

    EventInstance music;
    public EventReference musicRef;
    public static AudioManager instance { get; private set; }

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        /*
        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");
        dialogueBus = RuntimeManager.GetBus("bus:/Dialogue");
        */

    }
    private void Start()
    {
        
        //music = CreateInstance(musicRef);
    }


    public void MusicOn()
    {
        music.start();
    }
    public void MusicOff()
    {
        music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    public void RefreshVolume()
    {
        
        
         
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        ambienceBus.setVolume(ambienceVolume);
        sfxBus.setVolume(sfxVolume);
        dialogueBus.setVolume(dialogueVolume);
        
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
        
    }

    
    

    public EventInstance CreateInstance(EventReference sound)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(sound);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }
    public void AttachInstance(EventInstance evtInstance, Transform trans)
    {
        RuntimeManager.AttachInstanceToGameObject(evtInstance, trans);
        
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventRef, GameObject emitterObject)
    {
        StudioEventEmitter emitter = emitterObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventRef;
        eventEmitters.Add(emitter);
        return emitter;
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    private void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances) 
        { 
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        foreach(StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }
}
