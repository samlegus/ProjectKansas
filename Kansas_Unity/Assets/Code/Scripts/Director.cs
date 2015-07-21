using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

//Class was getting too big but I didn't wanna take the time to do a re-structure.
//Split it into 4 files with a partial class instead.
public partial class Director : MonoBehaviour
{

#region Private Variables

	private bool allowInput = true;
	
#endregion
	
#region Unity Messages
	
	private void Start()
	{
		LoadButtons();
		//RegisterCallbacksToButtons();
		directorData.nextSceneMomentID = GetNextSceneMomentID();
		primaryInfoText.text = "";
		secondaryInfoText.text = "";
		selectedMomentButtonID = directorData.currentMomentID;
		//selectedSceneID = directorData.currentScene;
		SetMomentButtons(directorData.currentAct, directorData.currentScene);
		SetDirectorMode (DirectorMode.MOMENT);
	}
	
	private void Update()
	{
		HandleButtonTransitions();
		
		if(allowInput)
			HandleInputForGUI ();
			
		SyncMomentButtons ();
	}
	
	private void OnApplicationQuit()
	{
		//This is so the data resets when we're done in play mode.
		directorData.currentAct = 1;
		directorData.currentScene = 1;
		directorData.currentMomentID = 0;
		directorData.nextSceneMomentID = 0;
	}
	
#endregion

}
