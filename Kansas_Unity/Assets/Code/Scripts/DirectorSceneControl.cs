using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public partial class Director : MonoBehaviour
{
	
#region Private Variables
	
	private IEnumerator currentMomentTimer;
	private IEnumerator sceneTransition;
	
#endregion
	
#region Coroutines
	
	//Info on Coroutines: http://docs.unity3d.com/Manual/Coroutines.html
	
	IEnumerator ExecuteSceneTransition(string sceneName, float delayDuration)
	{
		allowInput = false;
		float newDelay = delayDuration - Time.deltaTime;
		secondaryInfoText.text = "Transitioning to " + " in " + delayDuration.ToString () + " seconds...";
		yield return new WaitForSeconds(delayDuration - newDelay);
		if(delayDuration <= 0)
		{
			Application.LoadLevel (sceneName);
		}		
		else
		{
			delayDuration = newDelay;
			sceneTransition = ExecuteSceneTransition(sceneName, delayDuration);
			StartCoroutine(sceneTransition);
		}	
	}
	
	//This is kind of a wierd recursive combo, sorry if it's confusing.
	IEnumerator PlayCurrentMoment()
	{
		currentMomentSlider.minValue = 0f;
		currentMomentSlider.maxValue = currentMoment.Duration;
		currentMomentTimer = MomentTimer(0f);
		
		selectedMomentButtonID = directorData.currentMomentID;
		
		Color originalColor = momentButtons[directorData.currentMomentID].image.color;
		momentButtons[directorData.currentMomentID].image.color = Color.yellow;
		
		StartCoroutine(currentMomentTimer);
		
		int relativeIndex = dataManager.GetRelativeIndex(directorData.currentAct, directorData.currentScene, directorData.currentMomentID);
		if(!System.String.IsNullOrEmpty (currentScene.moments[relativeIndex].SFXName))
			specialEffects[currentScene.moments[relativeIndex].SFXName].SetTrigger ("activate");
		
		if(currentMoment.Duration > 0)
			yield return new WaitForSeconds(currentMoment.Duration);
		else
			yield return null;
		
		momentButtons[directorData.currentMomentID].image.color = originalColor;
		StopCoroutine(currentMomentTimer);
		currentMomentTimer = null;
		ShiftButtonsUp();
		selectedMomentButtonID = directorData.currentMomentID;
		//selectedMomentButtonID = directorData.currentMomentID;
		if(directorData.currentMomentID == directorData.nextSceneMomentID && sceneTransition == null && IsNextScene ())
		{
			//StartCoroutine(ExecuteSceneTransition("Act1Scene2", 1.5f));
			string actSubstring = "Act" + directorData.currentAct;
			string sceneSubstring = "Scene" + directorData.currentScene++;
			
			if(!IsNextScene ())
				actSubstring = "Act" + directorData.currentAct++;
			
			string sceneName = actSubstring + sceneSubstring;
			
			if(!IsNextScene () && directorData.currentAct < dataManager.Acts.Count)
				SetAct (directorData.currentAct++);
			
			directorData.currentMomentID++;
			StartCoroutine(ExecuteSceneTransition(sceneName, 1.5f));
		}
	}
	
	//Calls itself, stopped by PlayCurrentMoment
	IEnumerator MomentTimer(float momentTime)
	{
		currentMomentSlider.value = currentMoment.Duration - momentTime;
		yield return new WaitForFixedUpdate();	//WaitForEndOfFrame() is laggier? Huh?
		currentMomentTimer = MomentTimer(momentTime + Time.deltaTime);
		StartCoroutine (currentMomentTimer);
	}
	
#endregion
}