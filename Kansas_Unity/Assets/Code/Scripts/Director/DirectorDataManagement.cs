using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public partial class Director : MonoBehaviour
{

#region Private Variables
	
	private static DirectorData directorData;	//ScriptableObject to allow us to persist information about the UI between scenes
	private static DataManager dataManager;// = new DataManager();//this loads xml data and objectify's it so can access data
	
#endregion
	
#region Properties
	
	private Act currentAct { get { return dataManager.GetAct (directorData.currentAct);}}
	private Scene currentScene { get { return currentAct.scenes[directorData.currentScene - 1]; }}
	private Moment currentMoment { get { return currentScene.moments[dataManager.GetRelativeIndex (directorData.currentAct, directorData.currentScene, directorData.currentMomentID)]; }}
	
	private int selectedMomentButtonID { get ;set;}
	//int selectedSceneID {get;set;}
	
#endregion
	
#region Data Related Functions
	
	
	
	private void SetScene(int sceneNumber)
	{
		directorData.currentScene = sceneNumber;
		//sceneText.text = "Scene: " + directorData.currentScene;
		//sceneText.color = Color.white;
		buttonTransitions.Clear ();
		//SetMomentButtons(directorData.currentAct, directorData.currentScene);
	}
	
	private void SetAct(int actNumber)
	{
		directorData.currentAct = actNumber;
		//actText.text = "Act: " + actNumber;
		//actText.color = Color.white;
		SetScene(1);
	}
	
	/// <summary>
	/// Returns the index of the first moment in the next scene.
	/// note:this is used to flag when a scene change takes place for UI purposes
	/// </summary>
	/// <returns></returns>
	private int GetNextSceneMomentID()
	{
		int act = directorData.currentAct;
		int scene = directorData.currentScene;
		
		if (IsNextScene())
		{
			scene++;
		}
		else if (IsNextAct())
		{
			act++;
			scene = 1;
		}
		return dataManager.GetCombinedIndex(act, scene, 0);
	}
	
	/// <summary>
	/// returns true if there is another scene after the current one in this act, else returns false.
	/// </summary>
	/// <returns></returns>
	private bool IsNextScene()
	{
		if (dataManager.GetAct(directorData.currentAct).scenes[dataManager.GetAct(directorData.currentAct).scenes.Count - 1].number == directorData.currentScene)
		{
			return false;
		}
		return true;
	}
	
	/// <summary>
	/// returns true if there is another act after the current one in this script, else returns false.
	/// </summary>
	/// <returns></returns>
	private bool IsNextAct()
	{
		if (dataManager.Acts[dataManager.Acts.Count - 1].number == directorData.currentAct)
		{
			return false;
		}
		return true;
	}
	
	private static bool IsFinalAct()
	{
		return directorData.currentAct == dataManager.Acts.Count;
	}
	
	private void SetDirectorMode(DirectorMode mode)
	{
		bool enableActButtons = false;
		bool enableSceneButtons = false;
		bool enableMomentButtons = false;
		
		switch(mode)
		{
			case DirectorMode.ACT:
			{
				enableActButtons = true;
				break;
			}
			case DirectorMode.SCENE:
			{
				enableSceneButtons = true;
				break;
			}
			case DirectorMode.MOMENT:
			{
				//SetMomentButtons (directorData.currentAct, directorData.currentScene);
				enableMomentButtons = true;
				break;
			}
		}
		
		foreach(Button button in actButtons)
			button.gameObject.SetActive (enableActButtons);
		foreach(Button button in sceneButtons)
			button.gameObject.SetActive (enableSceneButtons);
		foreach(Button button in momentButtons)
			button.gameObject.SetActive (enableMomentButtons);
		
		directorMode = mode;
		UpdatePrimaryInfo ();
	}
	
	/// <summary>
	/// returns the int value of the given buttons txt field
	/// this is for act and scene button help
	/// </summary>
	/// <param name="button"></param>
	/// <returns></returns>
	private int GetNumberFromButton(Button button)
	{
		string text = button.GetComponentInChildren<Text>().text;
		string numAsString = text.Substring(text.Length - 1);
		int number = -1;
		if (!int.TryParse(numAsString, out number))
		{
			Debug.Log("Error parsing the scene number. ");
		}
		return number;
	}
	
	
#endregion
}