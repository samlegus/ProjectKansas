using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class DirectorGUIManager : MonoBehaviour 
{
#region Inspector
	
	#region Prefab
	
	[SerializeField] private Button m_buttonPrefab;
	[SerializeField] private Slider m_momentSliderPrefab; 
	
	#endregion
	
	#region Scene
	
	[SerializeField] private float m_buttonTransitionSpeed = 3f;
	
	[SerializeField] private RectTransform m_buttonPanel;
	[SerializeField] private Text m_actText;
	[SerializeField] private Text m_sceneText;
	[SerializeField] private Text m_secondaryInfoText;
	[SerializeField] private Text m_primaryInfoText;
	
	[SerializeField] private RectTransform[] m_momentPositions;
	[SerializeField] private RectTransform[] m_sceneButtonsPosition;
	
	[SerializeField] private Button m_nextSceneButton;
	[SerializeField] private Button m_prevSceneButton;
	
	#endregion
	
	
#endregion
	
#region Standard-Members
	
	private List<Button> m_momentButtons = new List<Button>();//a button for each moment in script in order
	private List<Button> m_sceneButtons = new List<Button>();//a button for each scene in script in order
	private List<Button> m_actButtons = new List<Button>();//a button for each act in script in order
	private List<ButtonTransition> m_buttonTransitions = new List<ButtonTransition>();
	private Dictionary<string, Animator> m_specialEffects = new Dictionary<string, Animator>();
	//private DirectorMode m_directorMode;
	
#endregion
	
#region Properties
	
	public RectTransform[] momentPositions { get { return m_momentPositions;}}
	public RectTransform[] sceneButtonPositions { get { return m_sceneButtonsPosition;}}
	
	public Text secondaryInfoText { get { return m_secondaryInfoText;}}
	
#endregion
	
#region Internal Structures
	
	public enum MomentPosition
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

#region Methods

	public void AddButtonTransition(Button button, Vector3 startPos, Vector3 endPos)
	{
		for(int i = 0 ; i < m_buttonTransitions.Count ; ++i)
		{
			//New transitions override previous existing ones
			if(m_buttonTransitions[i].button == button)
				m_buttonTransitions.Remove (m_buttonTransitions[i]);
		}
		m_buttonTransitions.Add (new ButtonTransition(button, startPos, endPos));
	}

	public void HandleButtonTransitions()
	{
		foreach(ButtonTransition buttonTransition in m_buttonTransitions)
		{
			float t = Time.deltaTime * m_buttonTransitionSpeed;
			buttonTransition.button.transform.position = Vector3.Lerp (buttonTransition.button.transform.position, buttonTransition.endPos, t );
		}
		
		for(int i = 0 ; i < m_buttonTransitions.Count ; ++i)
		{
			if(m_buttonTransitions[i].done)
			{
				m_buttonTransitions.Remove (m_buttonTransitions[i]);
			}	
		}
	}
	
	public void AddActButton(Act a_act, Vector3 a_position, Vector2 a_size, UnityAction a_onClickCallback)
	{
		Button button = Instantiate<Button>(m_buttonPrefab);
		m_actButtons.Add(button);
		button.transform.position = a_position; //m_momentPositions[(int)MomentPosition.ABOVE].transform.position;
		button.GetComponent<RectTransform>().sizeDelta = a_size;
		button.GetComponentInChildren<Text>().text = a_act.Number.ToString();
		button.transform.SetParent(m_buttonPanel);
		button.onClick.AddListener(a_onClickCallback);
		button.name = "ActButton" + a_act.Number;
		
		ButtonHover actHover = button.gameObject.AddComponent<ButtonHover>();
		actHover.text = m_primaryInfoText;
		actHover.textOnHover = 
			"Act Info:\n\n"  +
			"Number: " + a_act.Number + "\n" +
			"Number of Scenes:" + a_act.scenes.Count;
		
		m_actButtons.Add (button);
	}
	
	public void AddSceneButton(Scene a_scene, UnityAction a_onClickCallback)
	{
		Button button = Instantiate<Button>(m_buttonPrefab);
		
		button.transform.position = m_momentPositions[(int)MomentPosition.ABOVE].transform.position;
		button.GetComponentInChildren<Text>().text = a_scene.Number.ToString();
		button.transform.SetParent(m_buttonPanel);
		button.onClick.AddListener(a_onClickCallback);
		
		ButtonHover sceneHover = button.gameObject.AddComponent<ButtonHover>();
		sceneHover.text = m_primaryInfoText;
		sceneHover.textOnHover = 
			"Scene Info:\n\n" +
			"Scene Number: " + a_scene.Number + "\n" +
			"Number of Moments:" + a_scene.moments.Count;
		button.name = "SceneButton" + a_scene.Number;
		
		m_sceneButtons.Add (button);
	}
	
	public void AddMomentButton(Moment a_moment, UnityAction a_onClickCallback)
	{
		Button button = Instantiate<Button>(m_buttonPrefab);
		
		button.GetComponentInChildren<Text>().text = a_moment.Title;
		button.transform.SetParent(m_buttonPanel);
		button.onClick.AddListener(a_onClickCallback);
		button.name = "MomentButton (" + a_moment.Title + ")";
		
		m_momentButtons.Add (button);
	}
	
	public void SetMomentButtons(int a_actNumber, int a_sceneNumber, int a_currentMomentID)
	{
		for(int i = 0; i < m_momentButtons.Count; ++i)
		{
			if (i < a_currentMomentID)
			{
				m_momentButtons [i].transform.position = m_momentPositions [(int)MomentPosition.ABOVE].transform.position;
			}
			else
			{
				m_momentButtons [i].transform.position = m_momentPositions [(int)MomentPosition.BELOW].transform.position;
			}
			m_momentButtons[i].gameObject.SetActive (false);
		}
		
		int posIndex = 0;
		
		for(int i = a_currentMomentID - 2; i <= a_currentMomentID + 3; ++i)
		{
			posIndex++;
			if(i >= 0 && i < m_momentButtons.Count)
			{	
				m_momentButtons[i].gameObject.SetActive(true);
				m_momentButtons[i].transform.position = m_momentPositions[posIndex].transform.position;
				m_momentButtons[i].GetComponent<RectTransform>().sizeDelta = m_momentPositions[posIndex].GetComponent<RectTransform>().rect.size;
			}
		}
	}
	
	public Button GetActButton(Act a_act)
	{
		return m_actButtons[a_act.Number];
	}
	
	public Button GetActButton(int a_index)
	{
		return m_actButtons[a_index];
	}
		
#endregion
}
