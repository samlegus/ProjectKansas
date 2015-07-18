using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System;

public class Moment
{
    public string Title { get; set; }
    public string Line { get; set; }
    public float Duration { get; set;}
    public string SFXName {get;set;}
    public Vector3 Location {get;set;}

    public Moment(string a_title, string a_line, float a_duration, Vector3 a_location, string a_sfxName)
    {
        Title = a_title;
        Line = a_line;
        Duration = a_duration;
        SFXName = a_sfxName;
        Location = a_location;
    }
}

public class Scene
{
    public List<Moment> moments;
    public int Number { get; set; }

    public Scene()
    {
        moments = new List<Moment>();
    }

    public Scene(List<Moment> momentList)
    {
        moments = momentList;
    }
    
    public bool ContainsMoment(Moment moment)
    {
    	return moments.Contains (moment);
    }
}

public class Act
{
    public List<Scene> scenes;
    public int Number { get; set; }

    public Act()
    {
        scenes = new List<Scene>();
    }

    public Act(List<Scene> sceneList)
    {
        scenes = sceneList;
    }

    public Scene GetScene(int sceneNum)
    {
        foreach (Scene scene in scenes)
        {
            if (scene.Number == sceneNum)
            {
                return scene;
            }
        }
        return null;
    }
    
}

public class Script
{
    public List<Act> acts;

    public Script()
    {
        acts = new List<Act>();
    }

    public Script(List<Act> actList)
    {
        acts = actList;
    }

    public Act GetAct(int actNumber)
    {
        foreach (Act act in acts)
        {
            if (act.Number == actNumber)
            {
                return act;
            }
        }
        return null;
    }
}


//public class DataManager : MonoBehaviour
public class DataManager
{

    public List<Act> Acts
    {
        get
        {
            return m_Script.acts;
        }
    }

    public Act GetAct(int actNumber)
    {
        return m_Script.GetAct(actNumber);
    }

    private XmlDocument mDataDoc = new XmlDocument();
    private Script m_Script = new Script();


    public DataManager()
    {
        mDataDoc.Load(@"./Assets/Code/Data/SceneBreakdown.xml");
        if (mDataDoc.ChildNodes.Count != 1)
        {
            Debug.Log("Data file did not load.");
        }

        //loop acts and add to script
        XmlNodeList actsNodeList = mDataDoc.SelectNodes("script/act");
        foreach (XmlElement act in actsNodeList)
        {
            Act newAct = new Act();
            newAct.Number = Int32.Parse(act.Attributes.GetNamedItem("number").Value);
            //loop scenes and add to act
            XmlNodeList scenesNodeList = act.SelectNodes("scene");
            foreach (XmlElement scene in scenesNodeList)
            {
                Scene newScene = new Scene();
                newScene.Number = Int32.Parse(scene.Attributes.GetNamedItem("number").Value);
                //loop moments and add to scene
                XmlNodeList momentsNodeList = scene.SelectNodes("moment");
                foreach (XmlElement moment in momentsNodeList)
                {
                    string title = moment.Attributes.GetNamedItem("title").Value;
                    string line = moment.Attributes.GetNamedItem("line").Value;
                    string locationAsString = ("0.0, 0.0, 0.0");
                    string durationAsString = "";
                    string sfxName = "";
                    float duration = 0f;
                    Vector3 location = Vector3.zero;
                    
					if(moment.HasAttribute("duration"))
					{
						durationAsString = moment.Attributes.GetNamedItem ("duration").Value;
						if(!Single.TryParse(durationAsString, out duration))
							Debug.Log ("XML moment earsing error, duration had invalid format");
					}
					
					if(moment.HasAttribute ("location"))
					{
						locationAsString = moment.Attributes.GetNamedItem ("location").Value;
						location = Vector3Helper.StringToVector3(locationAsString);
					}
						
							
					if(moment.HasAttribute ("sfx"))
					   sfxName = moment.Attributes.GetNamedItem ("sfx").Value;
					   
                    Moment newMoment = new Moment(title, line, duration, location ,sfxName);
                    newScene.moments.Add(newMoment);
                }
                newAct.scenes.Add(newScene);
            }
            m_Script.acts.Add(newAct);
        }


    }

    /// <summary>
    /// Use this to get the index of a combined list of all moments from act, scene format.
    /// </summary>
    /// <param name="act">Act of moment to find index for</param>
    /// <param name="scene">Scene of moment to find index for</param>
    /// <param name="momentIndex">Moment to find index for</param>
    /// <returns></returns>
    public int GetCombinedIndex(int act, int scene, int momentIndex)
    {
        //get all previous moments from previous acts if any
        int previousActsMoments = mDataDoc.SelectNodes("script/act[@number < '" + act + "']/scene/moment").Count;
        //get all previous moments from scenes before given in given act
        int previousSceneMoments = mDataDoc.SelectNodes("script/act[@number = '" + act + "']/scene[@number < '" + scene + "']/moment").Count;
        return previousActsMoments + previousSceneMoments + momentIndex;
    }
    
    public int GetRelativeIndex(int act, int scene, int momentIndex)
    {
		//get all previous moments from previous acts if any
		int previousActsMoments = mDataDoc.SelectNodes("script/act[@number < '" + act + "']/scene/moment").Count;
		//get all previous moments from scenes before given in given act
		int previousSceneMoments = mDataDoc.SelectNodes("script/act[@number = '" + act + "']/scene[@number < '" + scene + "']/moment").Count;
		return momentIndex - previousSceneMoments - previousActsMoments;
    }
    
	public static class Vector3Helper
	{
		public static Vector3 StringToVector3(string value)
		{
			string[] temp = value.Replace(" ", "").Split(',');
			return new Vector3(float.Parse(temp[0]),float.Parse(temp[1]),float.Parse(temp[2]));
		}
	}
     
}
