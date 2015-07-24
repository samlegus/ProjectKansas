using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

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
		if(!Startup.dataInitialized)
		{
			directorData = Resources.Load ("DirectorData") as DirectorData;
			dataManager = new DataManager();
			
			SetDirectorDataForThisScene();
			directorData.totalSceneTime = 0f;
			directorData.totalDuration = (float)dataManager.script.totalSpan.TotalSeconds;
			Startup.dataInitialized = true;
		}
		
		directorData.elapsedSceneTime = 0f;
		directorData.nextSceneMomentID = GetNextSceneMomentID();
		directorData.totalSceneTime = (float)currentScene.startTime.TotalSeconds;
		selectedMomentButtonID = directorData.currentMomentID;
		
		LoadButtons();
		
		timeSlider.minValue = 0;
		timeSlider.maxValue = directorData.totalDuration;
		
		primaryInfoText.text = "";
		secondaryInfoText.text = "";
		timeTextB.text = dataManager.script.totalSpan.ToString ();
		
		SetMomentButtons(directorData.currentAct, directorData.currentScene);
		SetDirectorMode (DirectorMode.MOMENT);
	}
	
	private void Update()
	{
		HandleButtonTransitions();
		UpdateButtons ();
		
		if(allowInput)
			HandleInputForGUI ();
			
		if(updateSceneTimer)
		{
			directorData.elapsedSceneTime = Time.timeSinceLevelLoad;
			directorData.totalSceneTime += Time.deltaTime;
			UpdateTimeElements ();
		}
	}
	
	private void OnApplicationQuit()
	{
		//This is so the data resets when we're done in play mode.
		directorData.currentAct = 1;
		directorData.currentScene = 1;
		directorData.currentMomentID = 0;
		directorData.nextSceneMomentID = 0;
		directorData.elapsedSceneTime = 0;
		directorData.totalSceneTime = 0;
	}
	
#endregion

#region EditorOnly
	
	
	//So that when you start a scene from the editor it will update the DirectorData to the appropriate values
	//This still assumes we have perfect naming, "Act1Scene2" "Act2Scene6" etc...
	[UnityEditor.InitializeOnLoad]
	public class Startup
	{
		//public static bool levelLoadedFromEditor = false;
		public static bool dataInitialized = false;
		static Startup()
		{
			//#if UNITY_EDITOR
			//levelLoadedFromEditor = true;
			//#endif
		}
	}
	
	private void SetDirectorDataForThisScene()
	{
		string sceneName = Application.loadedLevelName;
		char[] numbersOnly = new System.String(sceneName.Where(Char.IsDigit).ToArray()).ToCharArray ();
		
		//foreach(char c in numbersOnly)
			//print (c);
		
		int act = numbersOnly[0];
		int scene = numbersOnly[1];
		int.TryParse(numbersOnly[0].ToString(), out act);
		int.TryParse(numbersOnly[1].ToString(), out scene);
		
		//print ("Current Act/Scene : " + act + " / " + scene);
		
		directorData.currentAct = act;
		directorData.currentScene = scene;
	}
	
	#endregion
}
