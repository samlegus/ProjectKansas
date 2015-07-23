using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DirectorSceneEventsManager : MonoBehaviour 
{

#region Private Variables

	[SerializeField] private float m_sceneTransitionDelay = 1.0f;
	private Dictionary<string, Animator> m_specialEffects = new Dictionary<string, Animator>();

#endregion

#region Properties
	
	public int nextSceneIndex { get { return Application.loadedLevel + 1;}}
	public int prevSceneIndex { get { return Application.loadedLevel - 1;}}
	public bool nextSceneExists { get { return Application.loadedLevel < Application.levelCount;}}
	
#endregion
	
#region Methods

	public void LoadSpecialEffects()
	{
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag ("SpecialFX"))
		{
			m_specialEffects.Add (obj.name, obj.GetComponent<Animator>());
		}
	}

#endregion

#region Coroutines

	public IEnumerator ExecuteSceneTransition(string a_sceneName, Text a_text)
	{
		float newDelay = m_sceneTransitionDelay - Time.deltaTime;
		
		yield return new WaitForSeconds(m_sceneTransitionDelay - newDelay);
		
		if(m_sceneTransitionDelay <= 0)
		{
			
			a_text.text = "Transitioning to scene NOW...";
			Application.LoadLevel (a_sceneName);
		}		
		else
		{
			a_text.text = "Transitioning to scene in " + newDelay.ToString () + "seconds.";
			m_sceneTransitionDelay = newDelay;
			IEnumerator sceneTransition = ExecuteSceneTransition(a_sceneName, a_text);
			StartCoroutine(sceneTransition);
		}	
	}
	
	public IEnumerator ExecuteSceneTransition(int a_levelIndex, Text a_text)
	{
		float newDelay = m_sceneTransitionDelay - Time.deltaTime;
		
		yield return new WaitForSeconds(m_sceneTransitionDelay - newDelay);
		
		if(m_sceneTransitionDelay <= 0)
		{
			
			a_text.text = "Transitioning to scene NOW...";
			Application.LoadLevel (a_levelIndex);
		}		
		else
		{
			a_text.text = "Transitioning to scene in " + newDelay.ToString () + "seconds.";
			m_sceneTransitionDelay = newDelay;
			IEnumerator sceneTransition = ExecuteSceneTransition(a_levelIndex, a_text);
			StartCoroutine(sceneTransition);
		}	
	}
	
	public void PlayCurrentMoment(Moment a_moment, Button a_button, Slider a_slider)
	{
		a_slider.minValue = 0f;
		a_slider.maxValue = a_moment.Duration;
		
		IEnumerator momentTimer = MomentTimer (0f, a_button, a_slider);
		
		StartCoroutine(momentTimer);
		
		if(!System.String.IsNullOrEmpty (a_moment.SFXName))
			m_specialEffects[a_moment.SFXName].SetTrigger ("activate");
	}
	
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
