
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class F_Audio_EventSound : MonoBehaviour
{
    [SerializeField] bool isSingleUse = true;
    bool used = false;
    [SerializeField] EventReference soundReference;
    EventInstance soundInstance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundInstance = AudioManager.instance.CreateInstance(soundReference);
        AudioManager.instance.AttachInstance(soundInstance, transform);
    }

    public void PlaySound()
    {
        if (!isSingleUse || !used)
        {
            used = true;
            soundInstance.start();
        }
        
    }
}
