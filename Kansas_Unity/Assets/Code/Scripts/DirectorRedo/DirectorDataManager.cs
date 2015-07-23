using UnityEngine;
using System.Collections;

public class DirectorDataManager : MonoBehaviour 
{

#region Standard Members

	private DirectorData2 m_data;
	private DataManager m_manager;

#endregion

#region Properties

	public DirectorData2 data { get { return m_data;} set { m_data = value;} }
	public DataManager manager { get { return m_manager;} set { m_manager = value;}}

	public int nextSceneMomentID
	{
		get
		{
			int act = m_data.currentAct;
			int scene = m_data.currentScene;
			
			if (nextSceneExists)
			{
				scene++;
			}
			else if (nextActExists)
			{
				act++;
				scene = 1;
			}
			return m_manager.GetCombinedIndex(act, scene, 0);
		}
	}
	
	
	public bool nextSceneExists
	{
		get { return m_manager.GetAct(m_data.currentAct).scenes[m_manager.GetAct(m_data.currentAct).scenes.Count - 1].Number == m_data.currentScene;}
	}
	
	public bool nextActExists
	{
		get { return m_manager.Acts[m_manager.Acts.Count - 1].Number == m_data.currentAct;}
	}
	
	public bool currentActIsFinal
	{
		get { return m_data.currentAct == m_manager.Acts.Count;}
	}

#endregion

#region Methods

	public void SetScene(int a_sceneNumber)
	{
		m_data.currentScene = a_sceneNumber;
	}
	
	public void SetAct(int a_actNumber)
	{
		m_data.currentAct = a_actNumber;
		SetScene(1);
	}
	
	

#endregion
}
