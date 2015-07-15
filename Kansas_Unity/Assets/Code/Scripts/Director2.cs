using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class Director2 : MonoBehaviour 
{
	//Inspector Setup
	public DirectorData directorData;
	public Button primaryButton;
	public Button[] buttons;
	public Text[] buttonTexts;
	
	//Members
	private DataManager dataManager;
	

	enum eButtonPanelMode
	{
		SCENE,
		ACT,
		MOMENT
	}
	
	eButtonPanelMode panelMode;
	
	void Awake()
	{
		dataManager = new DataManager();
	}
	
	void Start()
	{
		directorData.nextSceneMomentID = dataManager.GetCombinedIndex(directorData.currentAct, directorData.currentScene + 1, 0);
		panelMode = eButtonPanelMode.ACT;
		RefreshButtons ();
	}
	
	void RefreshButtons()
	{
		switch (panelMode)
		{
			case eButtonPanelMode.ACT:
			{
				foreach(Text text in buttonTexts)
				{
					text.text = "N/A";
				}
				
				if(directorData.currentAct - 1 > 0)
					//buttonTexts[2].text = directorData.currentAct - 1;
					
				buttonTexts[3].text = "Act " + directorData.currentAct;
				
				
				break;
			}
			default:
				break;
		}
	}
	
	void ShiftButtons()
	{
		
	}
	
	//Jeff's
	private bool IsNextScene()
	{
		if (dataManager.GetAct(directorData.currentAct).scenes[dataManager.GetAct(directorData.currentAct).scenes.Count - 1].Number == directorData.currentScene)
		{
			return false;
		}
		return true;
	}
	
	private bool IsNextAct()
	{
		if (dataManager.Acts[dataManager.Acts.Count - 1].Number == directorData.currentAct)
		{
			return false;
		}
		return true;
	}
	
}
