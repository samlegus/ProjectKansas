using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(DirectorDataManager))]
[RequireComponent(typeof(DirectorGUIManager))]
[RequireComponent(typeof(DirectorSceneEventsManager))]
public class Director2 : MonoBehaviour 
{
	enum DirectorMode
	{
		ACT,
		SCENE,
		MOMENT
	}
	
	private DirectorDataManager m_dataManager;
	private DirectorGUIManager m_guiManager;
	private DirectorSceneEventsManager m_sceneEventsManager;
	private DirectorMode m_directorMode;
	
	private Act currentAct { get { return m_dataManager.manager.Acts[m_dataManager.data.currentAct - 1];}}
	private Scene currentScene { get { return currentAct.scenes[m_dataManager.data.currentScene - 1];}}
	
	private void Awake()
	{
		m_dataManager = GetComponent<DirectorDataManager>();
		m_guiManager = GetComponent<DirectorGUIManager>();
		m_sceneEventsManager = GetComponent<DirectorSceneEventsManager>();
	}
	
	private void Start()
	{
		m_dataManager.data = Resources.Load ("DirectorData2") as DirectorData2;
		m_dataManager.manager = new DataManager();
		
		InitGUI ();
	}

	private void Update () 
	{
	
	}
	
	private void InitGUI()
	{
		for(int i = 0; i < m_dataManager.manager.Acts.Count; ++i)
		{
			Act act = m_dataManager.manager.Acts[i];
			Vector3 pos = m_guiManager.sceneButtonPositions[i].transform.position;
			Vector2 size = m_guiManager.sceneButtonPositions[i].rect.size;
			UnityAction onClickAction = () =>
			{
				m_dataManager.data.currentAct = act.Number;
				m_dataManager.data.currentScene = 1;
				StartCoroutine (m_sceneEventsManager.ExecuteSceneTransition("Act" + act.Number + "Scene" + 1, m_guiManager.secondaryInfoText));
			};
			m_guiManager.AddActButton (act, pos, size, onClickAction);
		}
	}
	
	
	
}
