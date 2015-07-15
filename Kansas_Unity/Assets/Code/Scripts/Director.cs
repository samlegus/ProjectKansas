using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
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
    public Text infoText;
	public DirectorData directorData;	//ScriptableObject to allow us to persist information about the UI between scenes
	
	//button positions
	public RectTransform[] momentPositions;
	public RectTransform[] sceneButtonsPosition;
	public RectTransform actButtonAbovePosition;
	
	#endregion
	
	#region Standard Members

    private DataManager dataManager = new DataManager();//this loads xml data and objectify's it so can access data
    private List<Button> momentButtons = new List<Button>();//a button for each moment in script in order
    private List<Button> sceneButtons = new List<Button>();//a button for each scene in script in order
    private List<Button> actButtons = new List<Button>();//a button for each act in script in order
	private List<ButtonTransition> buttonTransitions = new List<ButtonTransition>();
	
	private bool allowInput = true;

	#endregion
	
	#region Internal 
	
    private enum MomentPosition
    {
        Above,
        Top,
        Previous,
        Active,
        Next,
        Bottom,
        Below
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
	
	void Start()
	{
		LoadButtons();
		SetMomentButtons(directorData.currentAct, directorData.currentScene);
		directorData.nextSceneMomentID = dataManager.GetCombinedIndex(directorData.currentAct, directorData.currentScene + 1, 0);
	}
	
	void Update()
	{
		HandleButtonTransitions();
		HandleInput ();
	}
	
	void OnApplicationQuit()
	{
		//This is so the data resets when we're done in play mode.
		directorData.currentAct = 1;
		directorData.currentScene = 1;
		directorData.currentMomentID = 0;
		directorData.nextSceneMomentID = 0;
	}
	
	#endregion
	   
	#region Functions
	
	/// <summary>
	/// Adds a new transition to the ButtonTransition list, new transitions override old ones.
	/// </summary>
	/// <param name="button">Button.</param>
	/// <param name="startPos">Start position of the button (not used atm)</param>
	/// <param name="endPos">Destination of the button</param>
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
				buttonTransitions.Remove (buttonTransitions[i]);
		}
	}
	
	private void HandleInput()
	{
		if(allowInput)
		{
			float vAxis = Input.GetAxisRaw ("Vertical");
			bool vButtonDown = Input.GetButtonDown ("Vertical");
			
			if(vButtonDown && vAxis > 0 && directorData.currentMomentID < momentButtons.Count)
				ShiftButtonsUp ();
			if(vButtonDown && vAxis < 0 && directorData.currentMomentID > 0)
				ShiftButtonsDown ();
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
        ClearButtonPanel();
        directorData.currentMomentID = dataManager.GetCombinedIndex(act, scene, 0);

        for (int i = 0; i < momentButtons.Count; i++)
        {
            MomentPosition pos = MomentPosition.Above;
            //set above position
            if (i < directorData.currentMomentID - 2)
            {

            }
            else if (i == directorData.currentMomentID - 2)
            {
                //set top position
                pos = MomentPosition.Top;
            }
            else if (i == directorData.currentMomentID - 1)
            {
                //set prev
                pos = MomentPosition.Previous;
            }
            else if (i == directorData.currentMomentID)
            {
                //set active
                pos = MomentPosition.Active;
            }
            else if (i == directorData.currentMomentID + 1)
            {
                //set next
                pos = MomentPosition.Next;
            }
            else if (i == directorData.currentMomentID + 2)
            {
                //set bottom
                pos = MomentPosition.Bottom;
            }
            else
            {
                //set below
                pos = MomentPosition.Below;
            }

            momentButtons[i].transform.position = momentPositions[(int)pos].transform.position;
            momentButtons[i].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)pos].rect.size;

        }

        //update nextScene
        directorData.nextSceneMomentID = GetNextSceneMomentId();

    }

    //load the buttons into memory and store in list

    private void LoadButtons()
    {
        //create a button for everymoment in script
        foreach (Act act in dataManager.Acts)
        {
            //load the act buttons
            Button newActButton = Instantiate<Button>(buttonPrefab);
            newActButton.transform.position = actButtonAbovePosition.transform.position;
            newActButton.GetComponentInChildren<Text>().text = "Act " + act.Number;
            newActButton.transform.SetParent(buttonPanel);
            newActButton.onClick.AddListener(delegate { HandleActButtonClick(newActButton); });
            actButtons.Add(newActButton);

            foreach (Scene scene in dataManager.GetAct(act.Number).scenes)
            {
                //load the scene buttons
                Button newSceneButton = Instantiate<Button>(buttonPrefab);
                newSceneButton.transform.position = sceneButtonsPosition[0].transform.position;
                newSceneButton.GetComponentInChildren<Text>().text = "Scene: " + scene.Number;
                newSceneButton.transform.SetParent(buttonPanel);
                //this is how can add parameter to onclick
                newSceneButton.onClick.AddListener(delegate { HandleSceneButtonClick(newSceneButton); });
                sceneButtons.Add(newSceneButton);

                foreach (Moment moment in dataManager.GetAct(act.Number).GetScene(scene.Number).moments)
                {
                    Button newButton = Instantiate<Button>(buttonPrefab);
                    newButton.GetComponentInChildren<Text>().text = moment.Title;
                    newButton.transform.SetParent(buttonPanel);
                    newButton.onClick.AddListener(HandleMomentButtonClick);
                    momentButtons.Add(newButton);
                }
            }
        }
    }
    
   	/// <summary>
   	/// Coroutine. Loads the next scene after a delay. Call using StartCoroutine(ExecuteSceneTransition(...))
   	/// </summary>
   	/// <param name="sceneName">Name of the scene in the UnityProject (Must be in the build settings!)</param>
   	/// <param name="delay">Delay in seconds before the next scene is loaded after the coroutine is called.</param>
	IEnumerator ExecuteSceneTransition(string sceneName, float delay)
	{
		allowInput = false;
		infoText.text = "Transitioning to next scene, please wait.";
		yield return new WaitForSeconds(delay);
		Application.LoadLevel (sceneName);
	}

    private void HandleMomentButtonClick()
    {
    	if(allowInput)
    	{
			//ShiftButtonsUp();
			//ShiftButtonsDown();
			if(directorData.currentMomentID == directorData.nextSceneMomentID)
			{
				if (IsNextScene())
				{
					directorData.currentScene++;
					//This is really ghetto, works for now. Assumes PERFECT naming syntax project side.
					StartCoroutine(ExecuteSceneTransition("Act" + directorData.currentAct + "Scene" + directorData.currentScene, 1f));
				}
				else if (IsNextAct())
				{
					directorData.currentAct++;
					directorData.currentScene = 1;
				}
				directorData.nextSceneMomentID = GetNextSceneMomentId();
				sceneText.text = "Current Scene: " + directorData.currentScene;
				sceneText.color = Color.white;
				actText.text = "Current Act: " + directorData.currentAct;
			}
    	}
        
    }

    public void HandleActContextClick()
    {
        actText.text = "Select Act";
        actText.color = Color.yellow;

        ClearButtonPanel();
        int actsCount = dataManager.Acts.Count;
        int placeCounter = 1;//start with 1 because 0 is the above position

        //loop through the scenes in this act only
        for (int i = 0; i < actsCount; i++)
        {
            //use the scene positions
            actButtons[i].transform.position = sceneButtonsPosition[placeCounter].transform.position;
            placeCounter++;
        }
    }

    public void HandleSceneContextClick()
    {
        sceneText.text = "Select Scene";
        sceneText.color = Color.yellow;

        ClearButtonPanel();

        //start at top position and place the current acts scenes
        int scenesInThisAct = dataManager.GetAct(directorData.currentAct).scenes.Count;
        int indexOffset = 0;
        if (directorData.currentAct != 1)
        {
            //need to offset the list index if second act
            //hacky would like more general
            indexOffset = dataManager.GetAct(1).scenes.Count;
        }

        int placeCounter = 1;

        //loop through the scenes in this act only
        for (int i = 0; i < scenesInThisAct; i++)
        {
            sceneButtons[indexOffset + i].transform.position = sceneButtonsPosition[placeCounter].transform.position;
            placeCounter++;
        }

    }

    private void HandleActButtonClick(Button clickedButton)
    {
        SetAct(GetNumberFromButton(clickedButton));
    }

    private void HandleSceneButtonClick(Button clickedButton)
    {
        SetScene(GetNumberFromButton(clickedButton));
		//We want to load the scene when this button is clicked right?

    }

    private void SetScene(int sceneNumber)
    {
        directorData.currentScene = sceneNumber;
        sceneText.text = "Scene: " + directorData.currentScene;
        sceneText.color = Color.white;
        SetMomentButtons(directorData.currentAct, directorData.currentScene);

    }

    private void SetAct(int actNumber)
    {
        directorData.currentAct = actNumber;
        actText.text = "Act: " + actNumber;
        actText.color = Color.white;
        SetScene(1);
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
			AddButtonTransition (momentButtons[directorData.currentMomentID - 2], momentButtons[directorData.currentMomentID - 2].transform.position, momentPositions[(int)MomentPosition.Above].transform.position);
			//momentButtons[directorData.currentMomentID + -2].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Above].rect.size;
		}
		if (directorData.currentMomentID - 1 > -1)
        {
           	//Prev -> Top
			AddButtonTransition (momentButtons[directorData.currentMomentID - 1], momentButtons[directorData.currentMomentID - 1].transform.position, momentPositions[(int)MomentPosition.Top].transform.position);
			momentButtons[directorData.currentMomentID + -1].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Top].rect.size;
        }
        
        //Active -> Prev
        momentButtons[directorData.currentMomentID].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Previous].rect.size;
		AddButtonTransition(momentButtons[directorData.currentMomentID], momentButtons[directorData.currentMomentID].transform.position, momentPositions[(int)MomentPosition.Previous].transform.position);
		
        if (directorData.currentMomentID + 1 < momentButtons.Count)
        {
            //Next - Active
			AddButtonTransition(momentButtons[directorData.currentMomentID + 1], momentButtons[directorData.currentMomentID + 1].transform.position, momentPositions[(int)MomentPosition.Active].transform.position);
            momentButtons[directorData.currentMomentID + 1].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Active].rect.size;

        }
        if (directorData.currentMomentID + 2 < momentButtons.Count)
        {
            //Bottom -> Next
			AddButtonTransition (momentButtons[directorData.currentMomentID + 2], momentButtons[directorData.currentMomentID + 2].transform.position, momentPositions[(int)MomentPosition.Next].transform.position);
			momentButtons[directorData.currentMomentID + 2].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Next].rect.size;
        }
        if (directorData.currentMomentID + 3 < momentButtons.Count)
        {
          	//Bottom -> Below
			AddButtonTransition (momentButtons[directorData.currentMomentID + 3], momentButtons[directorData.currentMomentID + 3].transform.position, momentPositions[(int)MomentPosition.Below].transform.position);
        }
        directorData.currentMomentID++;
    }
    
    private void ShiftButtonsDown()
    {
        if(directorData.currentMomentID == momentButtons.Count)
        {
            Debug.Log("nothing to shift down.");
            return;
        }
        
		if (directorData.currentMomentID - 3 > -1)
		{
			//Top -> Prev
			AddButtonTransition (momentButtons[directorData.currentMomentID - 3], momentButtons[directorData.currentMomentID - 3].transform.position, momentPositions[(int)MomentPosition.Top].transform.position);
			//momentButtons[directorData.currentMomentID - 3].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Top].rect.size;
		}
		
		if (directorData.currentMomentID - 2 > -1)
		{
			//Top -> Prev
			AddButtonTransition (momentButtons[directorData.currentMomentID - 2], momentButtons[directorData.currentMomentID - 2].transform.position, momentPositions[(int)MomentPosition.Previous].transform.position);
			momentButtons[directorData.currentMomentID - 2].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Previous].rect.size;
		}
		
		if (directorData.currentMomentID - 1 > -1)
		{
			//Prev -> Active
			AddButtonTransition (momentButtons[directorData.currentMomentID - 1], momentButtons[directorData.currentMomentID - 1].transform.position, momentPositions[(int)MomentPosition.Active].transform.position);
			momentButtons[directorData.currentMomentID - 1].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Active].rect.size;
		}
		
		//Active -> Next
		momentButtons[directorData.currentMomentID].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Next].rect.size;
		AddButtonTransition(momentButtons[directorData.currentMomentID], momentButtons[directorData.currentMomentID].transform.position, momentPositions[(int)MomentPosition.Next].transform.position);
		
		if (directorData.currentMomentID + 1 < momentButtons.Count)
		{
			//Next -> Bottom
			AddButtonTransition (momentButtons[directorData.currentMomentID + 1], momentButtons[directorData.currentMomentID + 1].transform.position, momentPositions[(int)MomentPosition.Bottom].transform.position);
			momentButtons[directorData.currentMomentID + 1].GetComponent<RectTransform>().sizeDelta = momentPositions[(int)MomentPosition.Bottom].rect.size;
		}
		
		if (directorData.currentMomentID + 2 < momentButtons.Count)
		{
			//Bottom -> Below
			AddButtonTransition (momentButtons[directorData.currentMomentID + 2], momentButtons[directorData.currentMomentID + 2].transform.position, momentPositions[(int)MomentPosition.Below].transform.position);
		}
       
        if(directorData.currentMomentID > 0)
        	directorData.currentMomentID--;
    }

    /// <summary>
    /// Returns the index of the first moment in the next scene.
    /// note:this is used to flag when a scene change takes place for UI purposes
    /// </summary>
    /// <returns></returns>
    private int GetNextSceneMomentId()
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

    private void ClearButtonPanel()
    {
        //loop thorugh buttons and put them away
        foreach (Button button in momentButtons)
        {
            button.transform.position = momentPositions[(int)MomentPosition.Below].transform.position;
        }
        foreach (Button button in sceneButtons)
        {
            button.transform.position = sceneButtonsPosition[0].transform.position;
        }
        foreach(Button button in actButtons)
        {
            button.transform.position = actButtonAbovePosition.transform.position;
        }
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
