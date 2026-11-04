//Ilyas Vakkas Celik
This is a small tool created in a few days for a a specific university project I would like to continue working on when working on a fitting project.
I have have not used UnityUI Toolkit prior to this and this was also an early approach to editor tools, there is still a lot to adjust.
To Test this Properly you need to use the FMOD integration for Unity, but you can still test the Dialogue without the audio.


It contains basic functionality for a Dialogue Graph:

---
Open the Dialogue Window through the window tab.

In the Dialogue Window you can:
-Right click empty space to create a new node
-Remove Nodes with ctrl+LMB on the node field
-Edit nodes 
-Connect nodes with left click on the ports of a node
-Remove connections of node when ctrl + LMB on one of the connecting ports
-Move screenspace when holding on empty space and dragging with either middle mouse or alt+LMB

-Select a DialogueObject to work on
-Save a DialogueObject in the toolbar (this will create a new dialogue object when saving without a selected Dialogue Object)
-Load a DialogueObject in the toolbar (this will not clear the board when selecting none currently)

Nodes contain the following editable fields:
-Node name , just the node name that can be used to start dialogue right now
-FMOD Event, dialogue event to connect the dialogue to audio
-Extra Text Time, a custom value used for this project to determine extra time the dialogue text should stay visible on screen after the Aduio is done playing
-No Text Time, a custom value to essentially pause the dialogue for the given amount after the Dialogue snippet finished playing
-BroTalking, a custom bool to determine if the player or brother is talking 
-Dialogue Content, Text Field to put the Dialogue text of the given Snippet/Node

---

Outside the Dialogue window you can play the dialogue by passing the given BaseNodeData and read out the content and go into the next connected node and repeat until done.
I have included the DialogueManager we used in our student project with slight changes.
Simply create a public / [SerializeField]  BaseNodeData Member to a proxy script that calls the Dialogue Manager member function: PlayDialogue(BaseNodeData)
and chose the BaseNodeData in scene from the prior created dialogue object.
To read out the current audio state, listen to the Action<string> updateText function in the dialogue Manager for hte callback.

You need both Audiomanager and DialogueManager present in the scene.

I included an ExtAudio folder that is not necessary for the graph utility to function, but I included to show the simple work flow in which the Dialogue was used.

---

Like mentioned, this tool was made for a very specific project and meant to be very easy to use
There is still a lot I want to work on when I can:
 
Core Functionality:
-Exchange the project specific aspects with more generalized utility for broader Dialogue usage
-Add Branches and other necessary Dialogue aspects (For our project we only needed auto playing dialogue without any choices)


Performance:
-The performance of the current Dialogue Window Editor is not as good as I would like it to be.

Reliablility:
-I want to make sure that this is stable 
-Better User friendly design to create a good overview of the created audio 
-Playback dialogue in editor window

And possibly more I want to add.


