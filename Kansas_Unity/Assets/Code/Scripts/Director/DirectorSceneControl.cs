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
	//private IEnumerator sceneTransition;
	public float sceneTransitionDelay = 1.0f;
	
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
		
		yield return new WaitForSeconds(delayDuration - newDelay);
		
		if(delayDuration <= 0)
		{
			secondaryInfoText.text = "Transitioning to scene NOW...";
			Application.LoadLevel (sceneName);
		}		
		else
		{
			secondaryInfoText.text = "Transitioning to scene in " + newDelay.ToString () + "seconds.";
			delayDuration = newDelay;
			IEnumerator sceneTransition = ExecuteSceneTransition(sceneName, delayDuration);
			StartCoroutine(sceneTransition);
		}	
	}
	
	//Overload w/ int instead of string
	IEnumerator ExecuteSceneTransition(int sceneIndex, float delayDuration)
	{
		allowInput = false;
		float newDelay = delayDuration - Time.deltaTime;
		
		yield return new WaitForSeconds(delayDuration - newDelay);
		
		if(delayDuration <= 0)
		{
			secondaryInfoText.text = "Transitioning to scene NOW...";
			Application.LoadLevel (sceneIndex);
		}		
		else
		{
			secondaryInfoText.text = "Transitioning to scene in " + newDelay.ToString () + "seconds.";
			delayDuration = newDelay;
			IEnumerator sceneTransition = ExecuteSceneTransition(sceneIndex, delayDuration);
			StartCoroutine(sceneTransition);
		}	
	}
	
	//See Diagram
	private void PlayCurrentMoment()
	{
		currentMomentSlider.minValue = 0f;
		currentMomentSlider.maxValue = currentMoment.Duration;
		
		IEnumerator momentTimer = MomentTimer (0f, currentMomentButton, currentMomentSlider);
		
		StartCoroutine(momentTimer);
	
		if(!System.String.IsNullOrEmpty (currentMoment.SFXName))
			specialEffects[currentMoment.SFXName].SetTrigger ("activate");
	}
	
	/*  Diagram for PlayCurrentMoment + MomentTimer
	
		float t = 0f;
		PlayCurrentMoment() ------> StartMomentTimer(t, button, slider) ---->  (t < currentMoment.Duration) ? yes : no ----> Finish
											^															 	   |
											|															 	   v
											--------------------- t = t + time.deltaTime -----------------------	
	*/
	
	IEnumerator MomentTimer(float momentTime, Button button, Slider slider)
	{	
		if(momentTime >= 0)
		{
			slider.transform.localScale = Vector3.one;
			button.image.color = Color.yellow;
		}

		slider.GetComponent<RectTransform>().sizeDelta = button.GetComponent<RectTransform>().rect.size;
		slider.value = slider.maxValue - momentTime;
	
		yield return null;	
		
		if(momentTime >= slider.maxValue)
		{
			slider.transform.localScale = Vector3.zero;
			slider.value = slider.minValue;
			button.image.color = Color.white;
		}
		else
		{
			IEnumerator momentTimer = MomentTimer(momentTime + Time.deltaTime, button, slider);
			StartCoroutine (momentTimer);
		}
	}
	
#endregion
}