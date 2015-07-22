using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public partial class Director : MonoBehaviour
{
	
#region Private Variables
	
	//private IEnumerator currentMomentTimer;
	private IEnumerator sceneTransition;
	
#endregion

#region Properties

	public int nextSceneIndex { get { return Application.loadedLevel + 1;}}
	public int prevSceneIndex { get { return Application.loadedLevel - 1;}}

#endregion
	
#region Coroutines
	
	//Info on Coroutines: http://docs.unity3d.com/Manual/Coroutines.html
	
	IEnumerator ExecuteSceneTransition(string sceneName, float delayDuration)
	{
		allowInput = false;
		float newDelay = delayDuration - Time.deltaTime;
		secondaryInfoText.text = "Transitioning to " + sceneName + " in " + delayDuration.ToString () + " seconds...";
		
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
	
	IEnumerator ExecuteSceneTransition(int sceneIndex, float delayDuration)
	{
		allowInput = false;
		float newDelay = delayDuration - Time.deltaTime;
		//secondaryInfoText.text = "Transitioning to " + sceneName + " in " + delayDuration.ToString () + " seconds...";
		
		yield return new WaitForSeconds(delayDuration - newDelay);
		
		if(delayDuration <= 0)
		{
			Application.LoadLevel (sceneIndex);
		}		
		else
		{
			delayDuration = newDelay;
			sceneTransition = ExecuteSceneTransition(sceneIndex, delayDuration);
			StartCoroutine(sceneTransition);
		}	
	}
	
	//This is kind of a wierd recursive combo, sorry if it's confusing.
	IEnumerator PlayCurrentMoment()
	{
		
		Button button = momentButtons[directorData.currentMomentID];
		Slider slider = button.GetComponentInChildren<Slider>();
		slider.transform.localScale = Vector3.one;
		
		IEnumerator momentTimer = MomentTimer (null, 0f, button, slider);
		
		selectedMomentButtonID = directorData.currentMomentID;
		
		Color originalColor = momentButtons[directorData.currentMomentID].image.color;
		button.image.color = Color.yellow;
		
		StartCoroutine(momentTimer);
		
		int relativeIndex = dataManager.GetRelativeIndex(directorData.currentAct, directorData.currentScene, directorData.currentMomentID);
		
		if(!System.String.IsNullOrEmpty (currentScene.moments[relativeIndex].SFXName))
			specialEffects[currentScene.moments[relativeIndex].SFXName].SetTrigger ("activate");
		
		if(currentMoment.Duration > 0)
			yield return new WaitForSeconds(currentMoment.Duration);
		else
			yield return null;
		
		button.image.color = originalColor;
	
		slider.transform.localScale = Vector3.zero;
		slider.GetComponent<RectTransform>().sizeDelta = button.GetComponent<RectTransform>().rect.size;
	
		//selectedMomentButtonID = directorData.currentMomentID;
//		
//		if(directorData.currentMomentID == directorData.nextSceneMomentID && sceneTransition == null && IsNextScene ())
//		{
//			//StartCoroutine(ExecuteSceneTransition("Act1Scene2", 1.5f));
//			string actSubstring = "Act" + directorData.currentAct;
//			string sceneSubstring = "Scene" + directorData.currentScene++;
//			
//			if(!IsNextScene ())
//				actSubstring = "Act" + directorData.currentAct++;
//			
//			string sceneName = actSubstring + sceneSubstring;
//			
//			if(!IsNextScene () && directorData.currentAct < dataManager.Acts.Count)
//				SetAct (directorData.currentAct++);
//			
//			directorData.currentMomentID++;
//			StartCoroutine(ExecuteSceneTransition(sceneName, 1.5f));
//		}
	}
	
	//Calls itself, stopped by PlayCurrentMoment
	IEnumerator MomentTimer(IEnumerator momentTimer, float momentTime, Button button, Slider slider)
	{
		slider.GetComponent<RectTransform>().sizeDelta = button.GetComponent<RectTransform>().rect.size;
		slider.value = slider.maxValue - momentTime;
		yield return new WaitForFixedUpdate();	
		momentTimer = MomentTimer(momentTimer, momentTime + Time.deltaTime, button, slider);
		StartCoroutine (momentTimer);
	}
	
#endregion
}