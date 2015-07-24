using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Director : MonoBehaviour
{
  	
#region Inspector
	
	#region Scene
	public float buttonTransitionSpeed = 3f;
	
	public RectTransform buttonPanel;//parent to the buttons
	public Text actText;
	public Text sceneText;
	public Text secondaryInfoText;
	public Text primaryInfoText;

	public RectTransform[] momentPositions;
	public RectTransform[] sceneButtonsPosition;
	public Button nextSceneButton;
	public Button prevSceneButton;
	public Slider sceneSlider;
	public Slider timeSlider;
	public Text timeTextA;
	public Text timeTextB;
	//public RectTransform actButtonAbovePosition;
	
	#endregion
	
	public Slider momentSliderPrefab; 
	public Button buttonPrefab;//prefab will be cloning as child of btnPanel
	
	#region Prefab
	
	#endregion
	
	
	
#endregion
	
#region Private Variables
	
	private List<Button> momentButtons = new List<Button>();//a button for each moment in script in order
	private List<Button> sceneButtons = new List<Button>();//a button for each scene in script in order
	private List<Button> actButtons = new List<Button>();//a button for each act in script in order
	private List<ButtonTransition> buttonTransitions = new List<ButtonTransition>();
	private Dictionary<string, Animator> specialEffects = new Dictionary<string, Animator>();
	private DirectorMode directorMode;
	
#endregion
	
#region Properties

	Button currentMomentButton { get { return momentButtons[directorData.currentMomentID];}}
	Slider currentMomentSlider { get { return currentMomentButton.GetComponentInChildren<Slider>();}}
	
#endregion

#region Internal Structures
	
	private enum DirectorMode
	{
		ACT,
		SCENE,
		MOMENT
	}
	
	private enum MomentPosition
	{
		ABOVE,
		TOP,
		PREVIOUS,
		ACTIVE,
		NEXT,
		BOTTOM,
		BELOW
	}
	
	private class ButtonTransition 
	{
		public ButtonTransition(Button button, Vector3 start, Vector3 end)
		{
			this.button = button;
			this.startPos = start;
			this.endPos = end;
		}
		
		public Button button;
		public Vector3 startPos;
		public Vector3 endPos;
		public bool done = false;
	}
	
#endregion
	
#region GUI Manipulation Functions
	
	private void LoadButtons()
	{
		foreach(GameObject obj in GameObject.FindGameObjectsWithTag ("SpecialFX"))
		{
			specialEffects.Add (obj.name, obj.GetComponent<Animator>());
		}
		
		//TL;DR
		/*		foreach act in dataManger.xml thing
					make act button ( instantiate, setup onClick + onHover )
					foreach scene in act
						make scene button ( instantiate, setup onClick + onHover )
							foreach moment in scene
								make moment button ( instantiate, setup onClick ) */
								
		foreach (Act act in dataManager.Acts)
		{
			Button newActButton = Instantiate<Button>(buttonPrefab);
			actButtons.Add(newActButton);
			newActButton.transform.position = momentPositions[(int)MomentPosition.ABOVE].transform.position;
			newActButton.GetComponentInChildren<Text>().text = act.number.ToString();
			newActButton.transform.SetParent(buttonPanel);
			newActButton.onClick.AddListener( delegate { HandleActButtonClick(newActButton); });
			
			ButtonHover actHover = newActButton.gameObject.AddComponent<ButtonHover>();
			actHover.text = primaryInfoText;
			actHover.textOnHover = 
				"Act Info:\n\n"  +
				"Number: " + act.number + "\n" +
				"Number of Scenes:" + act.scenes.Count;
			
			newActButton.name = "ActButton";
			if(act.number == directorData.currentAct)
				newActButton.image.color = Color.yellow;
			
			foreach (Scene scene in dataManager.GetAct(act.number).scenes)
			{
				Button newSceneButton = Instantiate<Button>(buttonPrefab);
				newSceneButton.transform.position = momentPositions[(int)MomentPosition.BELOW].transform.position;
				newSceneButton.GetComponentInChildren<Text>().text = scene.number.ToString();
				newSceneButton.transform.SetParent(buttonPanel);
				newSceneButton.onClick.AddListener(delegate { HandleSceneButtonClick(newSceneButton); });
				
				ButtonHover sceneHover = newSceneButton.gameObject.AddComponent<ButtonHover>();
				sceneHover.text = primaryInfoText;
				sceneHover.textOnHover = 
					"Scene Info:\n\n"  +
						"Scene Number: " + scene.number + "\n" +
						"Number of Moments:" + scene.moments.Count +
						"Starts at:" + scene.endTime.ToString ();
				
				newSceneButton.name = "SceneButton Act" + act.number + "Scene" + scene.number;
				if(scene.number == currentScene.number)
					newSceneButton.image.color = Color.yellow;
				sceneButtons.Add(newSceneButton);
				
				int momentCounter = -1;
				foreach (Moment moment in dataManager.GetAct(act.number).GetScene(scene.number).moments)
				{
					momentCounter++;
					int momentIndex = dataManager.GetCombinedIndex(act.number, scene.number, 0) + momentCounter;
					Button newMomentButton = Instantiate<Button>(buttonPrefab);
					newMomentButton.GetComponentInChildren<Text>().text = moment.title;
					newMomentButton.transform.SetParent(buttonPanel);
					
					Slider slider = Instantiate<Slider>(momentSliderPrefab);
					RectTransform sliderRectTransform = slider.GetComponent<RectTransform>();
					sliderRectTransform.SetParent (newMomentButton.transform);
					sliderRectTransform.sizeDelta = newMomentButton.GetComponent<RectTransform>().rect.size;
					slider.transform.localPosition = Vector3.zero;
					slider.transform.localScale = Vector3.zero;
					
					UnityAction momentClickAction = () => 
					{
						if(directorData.currentMomentID == momentIndex)
						{
							//IEnumerator momentTimer = null;
							//StartCoroutine (PlayCurrentMoment ());
							PlayCurrentMoment ();
						}
						selectedMomentButtonID = momentIndex;
						//directorData.currentMomentID = momentIndex;
					};
					
					newMomentButton.onClick.AddListener( delegate { momentClickAction(); });
					
					//slider.gameObject.SetActive (false);
					
					slider.minValue = 0f;
					slider.maxValue = moment.duration;
					
//					ButtonHover momentHover = newMomentButton.gameObject.AddComponent<ButtonHover>();
//					momentHover.text = primaryInfoText;
//					momentHover.textOnHover = 
//						"Moment Info:\n\n"  +
//						"* Title: " + moment.Title + "\n" +
//						"* Line: " + moment.Line + "\n" +
//						"* Duration: " + moment.Duration + "\n" +
//						"* SFX: " + moment.SFXName + "\n";
					
					newMomentButton.name = "MomentButton (" + moment.title + ")";
					if(!currentScene.ContainsMoment(moment))
						newMomentButton.interactable = false;
					else
					{
						//System.Action onMoment = () => { specialEffects[moment.SFXName].SetTrigger ("activate");};
						//newButton.onClick.AddListener (delegate { onMoment(); });
					}
					momentButtons.Add(newMomentButton);
				}
			}
		}
	}
	
	private void AddButtonTransition(Button button, Vector3 startPos, Vector3 endPos)
	{
		for(int i = 0 ; i < buttonTransitions.Count ; ++i)
		{
			//New transitions override previous existing ones
			if(buttonTransitions[i].button == button)
				buttonTransitions.Remove (buttonTransitions[i]);
		}
		buttonTransitions.Add (new ButtonTransition(button, startPos, endPos));
	}
	
	private void HandleButtonTransitions()
	{
		foreach(ButtonTransition buttonTransition in buttonTransitions)
		{
			buttonTransition.button.transform.position = Vector3.Lerp (buttonTransition.button.transform.position, buttonTransition.endPos, Time.deltaTime * buttonTransitionSpeed);
		}
		
		for(int i = 0 ; i < buttonTransitions.Count ; ++i)
		{
			if(buttonTransitions[i].done)
			{
				buttonTransitions.Remove (buttonTransitions[i]);
			}	
		}
	}
	
	private void HandleInputForGUI()
	{
		if(allowInput)
		{
			float vAxis = Input.GetAxisRaw ("Vertical");		//-1 , 0, or 1 depending on Input (Neg, None, or Pos)
			bool vPressed = Input.GetButtonDown ("Vertical");	//True if button was pressed, false on release, hold, or no input
			bool submitPressed = Input.GetButtonDown ("Submit");
			
			float mouseWheelAxis = Input.GetAxisRaw ("Mouse ScrollWheel");
			
			if(directorMode == DirectorMode.MOMENT)
			{
				if((vPressed && vAxis < 0) || mouseWheelAxis < 0 && directorData.currentMomentID < momentButtons.Count && directorData.currentMomentID < directorData.nextSceneMomentID - 1 )
				{
					UpdatePrimaryInfo();
					selectedMomentButtonID = directorData.currentMomentID + 1;
				}
				
				if((vPressed && vAxis > 0) || mouseWheelAxis > 0 && directorData.currentMomentID > 0 && directorData.currentMomentID > dataManager.GetCombinedIndex(directorData.currentAct, directorData.currentScene, 0))
				{
					UpdatePrimaryInfo();
					selectedMomentButtonID = directorData.currentMomentID - 1;
				}	
			}
			
			if(submitPressed)
			{
				//StartCoroutine (PlayCurrentMoment());
				PlayCurrentMoment();
			}
		}
	}
	
	/// <summary>
	/// sets the positions of the moment buttons with given act , scene's first moment at active position.
	/// note:this also updates the directorData.currentMomentID, directorData.nextSceneMomentID vars.
	/// </summary>
	/// <param name="act"></param>
	/// <param name="scene"></param>
	private void SetMomentButtons(int act, int scene)
	{
		directorData.currentMomentID = dataManager.GetCombinedIndex(act, scene, 0);
		selectedMomentButtonID = directorData.currentMomentID;
		
		for(int i = 0; i < momentButtons.Count; ++i)
		{
			if(i < directorData.currentMomentID)
				momentButtons[i].transform.position = momentPositions[(int)MomentPosition.ABOVE].transform.position;
			else
				momentButtons[i].transform.position = momentPositions[(int)MomentPosition.BELOW].transform.position;
			
			momentButtons[i].gameObject.SetActive (false);
		}
		
		int posIndex = 0;
		
		for(int i = directorData.currentMomentID - 2; i <= directorData.currentMomentID + 3; ++i)
		{
			posIndex++;
			if(i >= 0 && i < momentButtons.Count)
			{	
				momentButtons[i].gameObject.SetActive(true);
				momentButtons[i].transform.position = momentPositions[posIndex].transform.position;
				momentButtons[i].GetComponent<RectTransform>().sizeDelta = momentPositions[posIndex].GetComponent<RectTransform>().rect.size;
			}
		}
		
		directorData.nextSceneMomentID = GetNextSceneMomentID();
		
	}
	
	private void ShiftButtonsUp()
	{
		if(directorData.currentMomentID == momentButtons.Count)
		{
			Debug.Log("nothing to shift up.");
			return;
		}
		
		if (directorData.currentMomentID - 2 > -1)
		{
			//Top -> Above
			AddButtonTransition (momentButtons[directorData.currentMomentID - 2], momentButtons[directorData.currentMomentID - 2].transform.position, momentPositions[(int)MomentPosition.ABOVE].transform.position);
			//momentButtons[directorData.currentMomentID + -2].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Above].rect.size;
		}
		if (directorData.currentMomentID - 1 > -1)
		{
			//Prev -> Top
			AddButtonTransition (momentButtons[directorData.currentMomentID - 1], momentButtons[directorData.currentMomentID - 1].transform.position, momentPositions[(int)MomentPosition.TOP].transform.position);
			momentButtons[directorData.currentMomentID + -1].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.TOP].rect.size;
		}
		
		//Active -> Prev
		momentButtons[directorData.currentMomentID].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.PREVIOUS].rect.size;
		AddButtonTransition(momentButtons[directorData.currentMomentID], momentButtons[directorData.currentMomentID].transform.position, momentPositions[(int)MomentPosition.PREVIOUS].transform.position);
		
		if (directorData.currentMomentID + 1 < momentButtons.Count)
		{
			//Next - Active
			AddButtonTransition(momentButtons[directorData.currentMomentID + 1], momentButtons[directorData.currentMomentID + 1].transform.position, momentPositions[(int)MomentPosition.ACTIVE].transform.position);
			momentButtons[directorData.currentMomentID + 1].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.ACTIVE].rect.size;
			
		}
		if (directorData.currentMomentID + 2 < momentButtons.Count)
		{
			//Bottom -> Next
			AddButtonTransition (momentButtons[directorData.currentMomentID + 2], momentButtons[directorData.currentMomentID + 2].transform.position, momentPositions[(int)MomentPosition.NEXT].transform.position);
			momentButtons[directorData.currentMomentID + 2].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.NEXT].rect.size;
		}
		if (directorData.currentMomentID + 3 < momentButtons.Count)
		{
			//Bottom -> Below
			AddButtonTransition (momentButtons[directorData.currentMomentID + 3], momentButtons[directorData.currentMomentID + 3].transform.position, momentPositions[(int)MomentPosition.BOTTOM].transform.position);
			momentButtons[directorData.currentMomentID + 3].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.BOTTOM].rect.size;
		}
		directorData.currentMomentID++;
	}
	
	private void ShiftButtonsDown()
	{
		if(directorData.currentMomentID == 0)
		{
			Debug.Log("nothing to shift down.");
			return;
		}
		
		if (directorData.currentMomentID - 3 > -1)
		{
			//Top -> Prev
			AddButtonTransition (momentButtons[directorData.currentMomentID - 3], momentButtons[directorData.currentMomentID - 3].transform.position, momentPositions[(int)MomentPosition.TOP].transform.position);
			//momentButtons[directorData.currentMomentID - 3].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Top].rect.size;
		}
		
		if (directorData.currentMomentID - 2 > -1)
		{
			//Top -> Prev
			AddButtonTransition (momentButtons[directorData.currentMomentID - 2], momentButtons[directorData.currentMomentID - 2].transform.position, momentPositions[(int)MomentPosition.PREVIOUS].transform.position);
			momentButtons[directorData.currentMomentID - 2].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.PREVIOUS].rect.size;
		}
		
		if (directorData.currentMomentID - 1 > -1)
		{
			//Prev -> Active
			AddButtonTransition (momentButtons[directorData.currentMomentID - 1], momentButtons[directorData.currentMomentID - 1].transform.position, momentPositions[(int)MomentPosition.ACTIVE].transform.position);
			momentButtons[directorData.currentMomentID - 1].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.ACTIVE].rect.size;
		}
		
		//Active -> Next
		momentButtons[directorData.currentMomentID].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.NEXT].rect.size;
		AddButtonTransition(momentButtons[directorData.currentMomentID], momentButtons[directorData.currentMomentID].transform.position, momentPositions[(int)MomentPosition.NEXT].transform.position);
		
		if (directorData.currentMomentID + 1 < momentButtons.Count)
		{
			//Next -> Bottom
			AddButtonTransition (momentButtons[directorData.currentMomentID + 1], momentButtons[directorData.currentMomentID + 1].transform.position, momentPositions[(int)MomentPosition.BOTTOM].transform.position);
			momentButtons[directorData.currentMomentID + 1].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.BOTTOM].rect.size;
		}
		
		if (directorData.currentMomentID + 2 < momentButtons.Count)
		{
			//Bottom -> Below
			AddButtonTransition (momentButtons[directorData.currentMomentID + 2], momentButtons[directorData.currentMomentID + 2].transform.position, momentPositions[(int)MomentPosition.BELOW].transform.position);
		}
		
		directorData.currentMomentID--;
	}
	
	private void UpdateButtons()
	{
		if(selectedMomentButtonID != directorData.currentMomentID)
		{
			if(selectedMomentButtonID > directorData.currentMomentID)
			{
				for(int i = 0 ; i < selectedMomentButtonID - directorData.currentMomentID; ++i)
				{
					ShiftButtonsUp ();
				}
			}
			if(selectedMomentButtonID < directorData.currentMomentID)
			{
				for(int i = 0 ; i < directorData.currentMomentID - selectedMomentButtonID; ++i)
				{
					ShiftButtonsDown ();
				}
			}
		}
		else
		{
			directorData.currentMomentID = selectedMomentButtonID;
		}
		
		if(!IsNextScene ())
		{
			if(nextSceneButton.gameObject.activeSelf)
			{
				nextSceneButton.gameObject.SetActive (false);
			}
		}
		if(directorData.currentScene <= 1)
		{
			if(prevSceneButton.gameObject.activeSelf)
			{
				prevSceneButton.gameObject.SetActive (false);
			}
		}
	}
	
	private void UpdatePrimaryInfo()
	{
		string text = "";
		switch(directorMode)
		{
			case DirectorMode.ACT:
			{
				text =
					"CONTROL MODE: ACT\n\n" +
						"Current Act: " + currentAct.number + "\n" +
						"Number of Scenes:" + currentAct.scenes.Count;
				break;
			}
			case DirectorMode.SCENE:
			{
				text =
					"CONTROL MODE: SCENE\n\n" +
						"Current Act: " + currentAct.number + "\n" +
					"Current Scene: " + currentScene.number + "\n" +
						"Number of Moments:" + currentScene.moments.Count;
				break;
			}
			case DirectorMode.MOMENT:
			{
				text =
					"CONTROL MODE: MOMENT\n\n" +
						"Current Act: " + directorData.currentAct + "\n" +
						"Current Scene: " + directorData.currentScene + "\n\n" +
						"Moment Info:\n\n"  +
						"* Title: " + currentMoment.title + "\n" +
						"* Line: " + currentMoment.line + "\n" +
						"* Duration: " + currentMoment.duration + "\n" +
						"* SFX: " + currentMoment.sfxName + "\n";
				break;
			}
		}
		primaryInfoText.text = text;
	}
	
	private void UpdateTimeElements()
	{
		switch(directorMode)
		{
			case DirectorMode.SCENE:
			{
				timeSlider.minValue = 0;
				timeSlider.maxValue = directorData.totalDuration;
				timeSlider.value = directorData.totalSceneTime;
				timeSlider.handleRect.GetComponentInChildren<Text>().text = new TimeSpan(0,0,(int)directorData.totalSceneTime).ToString ();
				
				timeTextA.text = TimeSpan.Zero.ToString();
				timeTextB.text = dataManager.script.totalSpan.ToString ();
				break;
			}
			case DirectorMode.MOMENT:
			{
				timeSlider.minValue = (float)currentScene.startTime.TotalSeconds;
				timeSlider.maxValue = (float)currentScene.endTime.TotalSeconds;
				timeSlider.value = directorData.elapsedSceneTime;
				timeSlider.handleRect.GetComponentInChildren<Text>().text = new TimeSpan(0,0,(int)directorData.elapsedSceneTime).ToString ();
				
				timeTextA.text = currentScene.startTime.ToString ();
				timeTextB.text = currentScene.endTime.ToString ();
				break;
			}
		}
	}
	
#endregion
	

}