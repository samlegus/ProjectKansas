using UnityEngine;
using System.Collections;

public class DirectorData2 : ScriptableObject 
{

#region Members

	[SerializeField] private int m_currentSceneNumber = 1;
	[SerializeField] private int m_currentActNumber = 1;
	[SerializeField] private int m_currentMomentID = 0;
	[SerializeField] private int m_nextSceneMomentID = 0;
	
#endregion

#region Properties

	public int currentScene { get{ return m_currentSceneNumber;} set{ m_currentSceneNumber = value;} }
	public int currentAct { get{ return m_currentActNumber;} set{ m_currentActNumber = value;} }
	public int currentMomentID { get{ return m_currentMomentID;} set{ m_currentMomentID = value;} }
	public int nextMomentID { get{ return m_nextSceneMomentID;} set{ m_nextSceneMomentID = value;} }	
	
#endregion

}
