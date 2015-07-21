using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class Director : MonoBehaviour
{

#region Inspector
	
	public float buttonTransitionSpeed = 3f;
    public Button buttonPrefab;//prefab will be cloning as child of btnPanel
    public RectTransform buttonPanel;//parent to the buttons
    public Text actText;
    public Text sceneText;
    public Text secondaryInfoText;
    public Text primaryInfoText;
    public Slider currentMomentSlider;
	public DirectorData directorData;	//ScriptableObject to allow us to persist information about the UI between scenes
	
	//button positions
	public RectTransform[] momentPositions;
	public RectTransform[] sceneButtonsPosition;
	public RectTransform actButtonAbovePosition;
	
#endregion
	
#region Private Variables

    private DataManager dataManager = new DataManager();//this loads xml data and objectify's it so can access data
    private List<Button> momentButtons = new List<Button>();//a button for each moment in script in order
    private List<Button> sceneButtons = new List<Button>();//a button for each scene in script in order
    private List<Button> actButtons = new List<Button>();//a button for each act in script in order
	private List<ButtonTransition> buttonTransitions = new List<ButtonTransition>();
	private Dictionary<string, Animator> specialEffects = new Dictionary<string, Animator>();
	
	private bool allowInput = true;
	private DirectorMode directorMode;
	private IEnumerator currentMomentTimer;
	private IEnumerator sceneTransition;

#endregion
	
#region Properties
	
	Act currentAct { get { return dataManager.GetAct (directorData.currentAct);}}
	Scene currentScene { get { return currentAct.scenes[directorData.currentScene - 1]; }}
	Moment currentMoment { get { return currentScene.moments[dataManager.GetRelativeIndex (directorData.currentAct, directorData.currentScene, directorData.currentMomentID)]; }}
	
	int selectedMomentButtonID { get ;set;}
	int selectedSceneID {get;set;}
	
#endregion
	
#region Internal Data-Structures

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

#region Unity Messages
	
	private void Start()
	{
		LoadButtons();
		RegisterCallbacksToButtons();
		directorData.nextSceneMomentID = GetNextSceneMomentID();
		primaryInfoText.text = "";
		secondaryInfoText.text = "";
		selectedMomentButtonID = directorData.currentMomentID;
		selectedSceneID = directorData.currentScene;
		SetMomentButtons(directorData.currentAct, directorData.currentScene);
		SetDirectorMode (DirectorMode.MOMENT);
	}
	
	private void Update()
	{
		
		HandleButtonTransitions();
		HandleInput ();
		SyncMomentButtons ();
		//UpdatePrimaryInfo();
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
	   
#region GUI Manipulation Functions
	
	private void HandleButtonTransitions()
	{
		foreach(ButtonTransition buttonTransition in buttonTransitions)
		{
			buttonTransition.button.transform.position = Vector3.Lerp (buttonTransition.button.transform.position, buttonTransition.endPos, Time.deltaTime * buttonTransitionSpeed);
		}
		
		for(int i = 0 ; i < buttonTransitions.Count ; ++i)
		{
			if(buttonTransitions[i].done)
				buttonTransitions.Remove (buttonTransitions[i]);
		}
	}
	
	private void HandleInput()
	{
		if(allowInput)
		{
			float vAxis = Input.GetAxisRaw ("Vertical");		//-1 , 0, or 1 depending on Input (Neg, None, or Pos)
			bool vPressed = Input.GetButtonDown ("Vertical");	//True if button was pressed, false on release, hold, or no input
			bool submitPressed = Input.GetButtonDown ("Submit");
			
			if(directorMode == DirectorMode.MOMENT)
			{
				if(vPressed && vAxis < 0 && directorData.currentMomentID < momentButtons.Count && directorData.currentMomentID < directorData.nextSceneMomentID - 1 )
				{
					UpdatePrimaryInfo();
					selectedMomentButtonID = directorData.currentMomentID + 1;
				}
					
				if(vPressed && vAxis > 0 && directorData.currentMomentID > 0 && directorData.currentMomentID > dataManager.GetCombinedIndex(directorData.currentAct, directorData.currentScene, 0))
				{
					UpdatePrimaryInfo();
					selectedMomentButtonID = directorData.currentMomentID - 1;
				}
					
			}
			
			if(submitPressed && currentMomentTimer == null)
			{
				StartCoroutine (PlayCurrentMoment());
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
        //ClearButtonPanel();
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
    
    private void DetermineActiveButtons()
    {
		
    }

    private void LoadButtons()
    {
    	foreach(GameObject obj in GameObject.FindGameObjectsWithTag ("SpecialFX"))
    	{
    		specialEffects.Add (obj.name, obj.GetComponent<Animator>());
    	}
    	
        //create a button for everymoment in script
        foreach (Act act in dataManager.Acts)
        {
        
            //load the act buttons
            Button newActButton = Instantiate<Button>(buttonPrefab);
			actButtons.Add(newActButton);
            newActButton.transform.position = actButtonAbovePosition.transform.position;
            newActButton.GetComponentInChildren<Text>().text = act.Number.ToString();
            newActButton.transform.SetParent(buttonPanel);
            newActButton.onClick.AddListener(delegate { HandleActButtonClick(newActButton); });
			
			ButtonHover actHover = newActButton.gameObject.AddComponent<ButtonHover>();
			actHover.text = primaryInfoText;
			actHover.textOnHover = 
					"Act Info:\n\n"  +
					"Number: " + act.Number + "\n" +
					"Number of Scenes:" + act.scenes.Count;
			
			
            newActButton.name = "ActButton";
            if(act.Number == directorData.currentAct)
            	newActButton.image.color = Color.yellow;
           // actButtons.Add(newActButton);
			
            foreach (Scene scene in dataManager.GetAct(act.Number).scenes)
            {
                //load the scene buttons
                Button newSceneButton = Instantiate<Button>(buttonPrefab);
				newSceneButton.transform.position = momentPositions[(int)MomentPosition.BELOW].transform.position;
                newSceneButton.GetComponentInChildren<Text>().text = scene.Number.ToString();
                newSceneButton.transform.SetParent(buttonPanel);
                //this is how can add parameter to onclick
                newSceneButton.onClick.AddListener(delegate { HandleSceneButtonClick(newSceneButton); });
				
				ButtonHover sceneHover = newSceneButton.gameObject.AddComponent<ButtonHover>();
				sceneHover.text = primaryInfoText;
				sceneHover.textOnHover = 
					"Scene Info:\n\n"  +
					"Scene Number: " + scene.Number + "\n" +
					"Number of Moments:" + scene.moments.Count;
						
                newSceneButton.name = "SceneButton Act" + act.Number + "Scene" + scene.Number;
				if(scene.Number == currentScene.Number)
					newSceneButton.image.color = Color.yellow;
                sceneButtons.Add(newSceneButton);
               
               	int momentCounter = -1;
                foreach (Moment moment in dataManager.GetAct(act.Number).GetScene(scene.Number).moments)
                {
                	momentCounter++;
                	int momentIndex = dataManager.GetCombinedIndex(act.Number, scene.Number, 0) + momentCounter;
                    Button newMomentButton = Instantiate<Button>(buttonPrefab);
                    newMomentButton.GetComponentInChildren<Text>().text = moment.Title;
                    newMomentButton.transform.SetParent(buttonPanel);
                    //newButton.onClick.AddListener(HandleMomentButtonClick);
                    
                    System.Action momentClickAction = () => 
                    {
                    	if(directorData.currentMomentID == momentIndex)
                    	{
                    		StartCoroutine (PlayCurrentMoment ());
                    	}
						selectedMomentButtonID = momentIndex;
                 	};
                 	
					newMomentButton.onClick.AddListener( delegate { momentClickAction(); });
					//ButtonHover momentButtonHoverScript =  newMomentButton.gameObject.AddComponent<ButtonHover>();
					//momentButtonHoverScript.onHover.AddListener (delegate { UpdatePrimaryInfoFor(moment);});
					
					ButtonHover momentHover = newMomentButton.gameObject.AddComponent<ButtonHover>();
					momentHover.text = primaryInfoText;
					momentHover.textOnHover = 
						"Moment Info:\n\n"  +
							"* Title: " + moment.Title + "\n" +
							"* Line: " + moment.Line + "\n" +
							"* Duration: " + moment.Duration + "\n" +
							"* SFX: " + moment.SFXName + "\n";
					
					newMomentButton.name = "MomentButton (" + moment.Title + ")";
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
    
    private void RegisterCallbacksToButtons()
    {
    	for(int i = 0 ; i < dataManager.Acts.Count; ++i)
    	{
    		//ButtonHover actHover = actButtons[i].gameObject.AddComponent<ButtonHover>();
    		//actHover.onHover.AddListener (delegate{ UpdatePrimaryInfoFor(dataManager.GetAct(i));});
    		//EventTrigger trigger =
    		
    	}
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
    
    private void SyncMomentButtons()
    {
    	if(selectedMomentButtonID != directorData.currentMomentID)
    	{
    		if(selectedMomentButtonID > directorData.currentMomentID)
    		{
				print (selectedMomentButtonID);
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
					"Current Act: " + currentAct.Number + "\n" +
					"Number of Scenes:" + currentAct.scenes.Count;
				break;
			}
			case DirectorMode.SCENE:
			{
				text =
					"CONTROL MODE: SCENE\n\n" +
					"Current Act: " + currentAct.Number + "\n" +
					"Current Scene: " + currentScene.Number + "\n" +
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
					"* Title: " + currentMoment.Title + "\n" +
					"* Line: " + currentMoment.Line + "\n" +
					"* Duration: " + currentMoment.Duration + "\n" +
					"* SFX: " + currentMoment.SFXName + "\n";
				break;
			}
		}
		primaryInfoText.text = text;
    }
    
    public void UpdatePrimaryInfoFor(Act act)
    {
    	string text = "";
		text =
			//"INFO MODE: ACT\n\n" +
			"Act Info:\n\n"  +
			"Number: " + act.Number + "\n" +
			"Number of Scenes:" + act.scenes.Count;
		primaryInfoText.text = text;
    }
    
	private void UpdatePrimaryInfoFor(Scene scene)
	{
		string text = "";
		text =
			//"Current Act: " + currentAct.Number + "\n" +
			"Scene Info:\n\n"  +
			"Scene Number: " + scene.Number + "\n" +
			"Number of Moments:" + scene.moments.Count;
		primaryInfoText.text = text;
	}
	
	private void UpdatePrimaryInfoFor(Moment moment)
	{
		string text = "";
		text =
			//"CONTROL MODE: MOMENT\n\n" +
				//"Current Act: " + directorData.currentAct + "\n" +
				//"Current Scene: " + directorData.currentScene + "\n\n" +
				"Moment Info:\n\n"  +
				"* Title: " + moment.Title + "\n" +
				"* Line: " + moment.Line + "\n" +
				"* Duration: " + moment.Duration + "\n" +
				"* SFX: " + moment.SFXName + "\n";
		primaryInfoText.text = text;
	}
    
#endregion
    
#region Data Related Functions
	
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
		if (dataManager.GetAct(directorData.currentAct).scenes[dataManager.GetAct(directorData.currentAct).scenes.Count - 1].Number == directorData.currentScene)
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
		if (dataManager.Acts[dataManager.Acts.Count - 1].Number == directorData.currentAct)
		{
			return false;
		}
		return true;
	}
	
	private bool IsFinalAct()
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
	
#region GUI Callback Functions
	
	private void HandleMomentButtonClick()
	{
		if(allowInput)
		{
//			//ShiftButtonsUp();
//			//ShiftButtonsDown();
//			
//			if(directorData.currentMomentID == directorData.nextSceneMomentID)
//			{
//				if (IsNextScene())
//				{
//					directorData.currentScene++;
//					//This is really ghetto, works for now. Assumes PERFECT naming syntax project side.
//					StartCoroutine(ExecuteSceneTransition("Act" + directorData.currentAct + "Scene" + directorData.currentScene, 1f));
//				}
//				else if (IsNextAct())
//				{
//					directorData.currentAct++;
//					directorData.currentScene = 1;
//				}
//				directorData.nextSceneMomentID = GetNextSceneMomentID();
//				sceneText.text = "Current Scene: " + directorData.currentScene;
//				sceneText.color = Color.white;
//				actText.text = "Current Act: " + directorData.currentAct;
//			}
		}
	}

	
	public void HandleActContextClick()
	{
		//actText.text = "Select Act";
		//actText.color = Color.yellow;
		
		//ClearButtonPanel();
		SetDirectorMode (DirectorMode.ACT);
		int actsCount = dataManager.Acts.Count;
		int placeCounter = 0;
		//loop through the scenes in this act only
		for (int i = 0; i < actsCount; i++)
		{
			//use the scene positions
			actButtons[i].transform.position = sceneButtonsPosition[placeCounter].transform.position;
			actButtons[i].GetComponent<RectTransform>().sizeDelta = sceneButtonsPosition[placeCounter].GetComponent<RectTransform>().rect.size;
			placeCounter++;
		}
	}
	
	public void HandleSceneContextClick()
	{
		//sceneText.text = "Select Scene";
		//sceneText.color = Color.yellow;
		
		//ClearButtonPanel();
		SetDirectorMode (DirectorMode.SCENE);
		
		//start at top position and place the current acts scenes
		int scenesInThisAct = currentAct.scenes.Count;
		int indexOffset = 0;
		if (directorData.currentAct != 1)
		{
			//need to offset the list index if second act
			//hacky would like more general
			indexOffset = dataManager.GetAct(1).scenes.Count;
		}
		
		int placeCounter = 0;
		//loop through the scenes in this act only
		for (int i = 0; i < scenesInThisAct; ++i)
		{
			sceneButtons[indexOffset + i].transform.position = sceneButtonsPosition[placeCounter].transform.position;
			sceneButtons[indexOffset + i].GetComponent<RectTransform>().sizeDelta = sceneButtonsPosition[placeCounter].rect.size;
			placeCounter++;
		}
		
	}
	
	public void HandleMomentContextClick()
	{
		//SetMomentButtons (directorData.currentAct, directorData.currentScene);
		//UpdatePrimaryInfo ();
		SetDirectorMode (DirectorMode.MOMENT);
	}
	
	private void HandleActButtonClick(Button clickedButton)
	{
		SetAct(GetNumberFromButton(clickedButton));
		StartCoroutine (ExecuteSceneTransition("Act" + currentAct.Number + "Scene" + 1, .25f));
	}
	
	private void HandleSceneButtonClick(Button clickedButton)
	{
		SetScene(GetNumberFromButton(clickedButton));
		StartCoroutine (ExecuteSceneTransition("Act" + currentAct.Number + "Scene" + currentScene.Number, .25f));
		//SetMomentButtons (directorData.currentAct, directorData.currentScene);
		//SetDirectorMode (DirectorMode.MOMENT);
	}
	
#endregion
    
}
