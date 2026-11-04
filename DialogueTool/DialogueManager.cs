
using FMOD.Studio;
using FMODUnity;
using System;
using System.Data;
using UnityEngine;



/*Dialogue manager was created for the specific use of automatic playing dialogue 
 * would need adjustments or general replacement for other use cases
 * Most important use is traveling along the BaseNodeData for all dialogue and playing audio
 * I included this to show the basic implementation I added for our use case
 */

public class DialogueManager : MonoBehaviour
{
    
    BaseNodeData currentData;
    EventInstance evInstance;
    [HideInInspector]public EventReference reference;

    //These are for the specific use of this project, I made these public later to setup this implementaion easier outside the project
    //To show use 
    public Transform bro;
    public Transform player;

    string dialogueText;

    DIA_STATE diaState;

    float timer = 0f;


    public Action<string> updateText;

    PLAYBACK_STATE state;
    public static DialogueManager instance;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        //      This was in the project, Commented out to show usecase
        //bro = GameManager.instance?.brotherObject?.transform;
    }

    public void PlayDialogue(BaseNodeData dialogueStart)
    {
        if (dialogueStart == null) return;

        ClearDialogue();
        SetDialogue(dialogueStart);
            
        
        
    }
    private void Start()
    {
        //      Also project specific 
        //if (GameManager.instance != null )
        //{
        //    if(GameManager.instance.brotherObject != null)
        //     bro = GameManager.instance.brotherObject.transform;
        //    if (GameManager.instance.playerObject != null) 
        //        player = GameManager.instance.playerObject.transform;
        //}
    }

    private void Update()
    {
        if(evInstance.isValid())
            evInstance.getPlaybackState(out state);

        DialogueNodeData data = (DialogueNodeData)currentData;

        

        switch (diaState)
        {
            case (DIA_STATE.none):
                return;
                
            case DIA_STATE.inDialogue:
                
                if (state == PLAYBACK_STATE.STOPPED || !evInstance.isValid() )
                {
                    diaState = DIA_STATE.inText;
                    timer = 0f;
                }

                break;
            case DIA_STATE.inText:
                if (data != null)
                {
                    if (timer >= data.extraTextTime)
                    {
                        diaState = DIA_STATE.noText;
                        timer = 0f;
                        if(updateText != null)
                            updateText.Invoke(string.Empty);
                    }

                    else
                    {
                        timer += Time.deltaTime;
                    }
                }
                else { }
                    break;
            case DIA_STATE.noText:
                if (data != null)
                {
                    if (timer >= data.noTextTime)
                    {

                        diaState = DIA_STATE.done;
                        timer = 0f;

                    }
                    else
                    {
                        timer += Time.deltaTime;
                    }
                }
                
                break;
            case DIA_STATE.done:

                DialogueNext();
                break;
        }
        
        
        
    }
    private void DialogueNext()
    {
        ClearDialogue();
        SetDialogue(currentData.nextNode);
        
    }

    private void ClearDialogue()
    {
        if (evInstance.isValid()) 
        { 
            evInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            evInstance.release();
        }

        diaState = DIA_STATE.none;
        
    }

    private void SetDialogue(BaseNodeData bnd)
    {
        currentData = bnd;
        if (currentData is DialogueNodeData data)
        {
            dialogueText = data.dialogueText;
            if(updateText != null)
                updateText.Invoke(dialogueText);
            diaState = DIA_STATE.inDialogue;
            FMOD.RESULT result = RuntimeManager.StudioSystem.getEventByID(data.dialogueAudio.Guid, out FMOD.Studio.EventDescription eventDesc);
            //FMOD.RESULT result = RuntimeManager.StudioSystem.getEvent(data.dialogueAudio.Guid, out FMOD.Studio.EventDescription eventDesc);
            if (result == FMOD.RESULT.OK && eventDesc.isValid())
            {
               
                evInstance = AudioManager.instance.CreateInstance(data.dialogueAudio);

                if (evInstance.isValid())
                {
                    if (data.isBroTalking)
                    {
                        //if (bro == null && GameManager.instance != null && GameManager.instance.brotherObject != null)
                        //{
                        //    bro = GameManager.instance.brotherObject.transform;
                        //}
                        if (bro != null) 
                        { 
                            AudioManager.instance.AttachInstance(evInstance, bro);
                        }
                    }
                    else
                    {
                        //Gamemanager was used in project, commented out to show functionallity 
                        //without dependance on project
                        //if (player == null &&GameManager.instance.playerObject != null)
                        //{
                        //    player = GameManager.instance.playerObject.transform;
                        //}
                        if(player != null)
                        {
                            AudioManager.instance.AttachInstance(evInstance, player);
                        }
                       
                    }


                        evInstance.start();
                    
                    
                }
            }
            
        }
        if (bnd == null)
        {
            diaState = DIA_STATE.none;
            ClearDialogue();
        }
    }

    enum DIA_STATE
    {
        none,
        inDialogue,
        inText,
        noText,
        done
    }
}
