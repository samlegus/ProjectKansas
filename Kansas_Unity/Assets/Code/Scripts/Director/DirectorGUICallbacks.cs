
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public partial class Director : MonoBehaviour 
{
	#region GUI Callback Functions
	
	public void HandleActContextClick()
	{
		if(allowInput)
		{
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
	}
	
	public void HandleSceneContextClick()
	{
		if(allowInput)
		{
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
	}
	
	public void HandleMomentContextClick()
	{
		if(allowInput)
			SetDirectorMode (DirectorMode.MOMENT);
	}
	
	private void HandleActButtonClick(Button clickedButton)
	{
		if(allowInput)
		{
			SetAct(GetNumberFromButton(clickedButton));
			StartCoroutine (ExecuteSceneTransition("Act" + currentAct.number + "Scene" + 1, .25f));
		}
	}
	
	private void HandleSceneButtonClick(Button clickedButton)
	{
		if(allowInput)
		{
			SetScene(GetNumberFromButton(clickedButton));
			StartCoroutine (ExecuteSceneTransition("Act" + currentAct.number + "Scene" + currentScene.number, sceneTransitionDelay));
		}
	}
	
	//Button that calls this is disabled if there is no next scene
	public void HandleNextSceneButtonClick()
	{
		SetScene (directorData.currentScene++);
		StartCoroutine(ExecuteSceneTransition(nextSceneIndex, sceneTransitionDelay));
	}
	
	//Button that calls this is disabled if there is no prev scene
	public void HandlePrevSceneButtonClick()
	{
		SetScene (directorData.currentScene--);
		StartCoroutine(ExecuteSceneTransition(nextSceneIndex, sceneTransitionDelay));
	}
	
	#endregion
}

