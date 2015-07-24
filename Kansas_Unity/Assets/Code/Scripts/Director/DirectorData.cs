using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

public class DirectorData : ScriptableObject 
{
	public int currentScene = 0;
	public int currentAct = 0;
	public int currentMomentID = 0;
	public int nextSceneMomentID = 0;
	
	public float totalDuration = 0;	
	public float totalSceneTime = 0;
	public float elapsedSceneTime = 0;
	public float[] sceneTimeStamps;
	
}
