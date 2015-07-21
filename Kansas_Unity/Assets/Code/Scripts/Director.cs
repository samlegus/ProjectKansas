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
	private static int callOrder = 0;
	
#endregion
	
#region Unity Messages
	
	private void Start()
	{
		directorData = Resources.Load ("DirectorData") as DirectorData;
		dataManager = new DataManager();
		
		#if UNITY_EDITOR
		if(Startup.levelLoadedFromEditor)
		{
			SetDirectorDataForThisScene();
		}
		#endif
		
		directorData.nextSceneMomentID = GetNextSceneMomentID();
		selectedMomentButtonID = directorData.currentMomentID;
		
		LoadButtons();
		primaryInfoText.text = "";
		secondaryInfoText.text = "";
		//RegisterCallbacksToButtons();
		
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

#region Functions
	
	#if UNITY_EDITOR
	//So that when you start a scene from the editor it will update the DirectorData to the appropriate values
	//This still assumes we have perfect naming, "Act1Scene2" "Act2Scene6" etc...
	[UnityEditor.InitializeOnLoad]
	public class Startup
	{
		public static bool levelLoadedFromEditor = false;
		static Startup()
		{
			levelLoadedFromEditor = true;
			//UnityEditor.EditorApplication.update += Update;
		}
	}
	#endif
	
	
	private void SetDirectorDataForThisScene()
	{
		
		//dataManager = new DataManager();
		//directorData = Resources.Load ("DirectorData") as DirectorData;
		string sceneName = Application.loadedLevelName;
		//"Act1Scene2" after this would result in "12"
		//string[] numbersOnly = sceneName.Split ("Act".ToCharArray ());
		
		char[] numbersOnly = new System.String(sceneName.Where(Char.IsDigit).ToArray()).ToCharArray ();
		
		foreach(char c in numbersOnly)
			print (c);
		
		int act = numbersOnly[0];
		int scene = numbersOnly[1];
		int.TryParse(numbersOnly[0].ToString(), out act);
		int.TryParse(numbersOnly[1].ToString(), out scene);
		
		print ("Current Act/Scene : " + act + " / " + scene);
		
		directorData.currentAct = act;
		directorData.currentScene = scene;
		//directorData.currentMomentID = 	dataManager.GetCombinedIndex(act, scene, 0);
		//directorData.nextSceneMomentID = GetNextSceneMomentID ();	
	}
	
	
	#endregion
}
