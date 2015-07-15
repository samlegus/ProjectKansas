using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;

public class ButtonScroll_02 : MonoBehaviour {

	public GameObject[] sockets;
	public GameObject[] buttons;
	public GameObject[] sceneSockets;
	public GameObject[] scenes;
	public GameObject ticker;

	Transform[] children;
	Transform[] sceneChildren;
	Animator[] animator;
	Animator[] sceneAnimator;
	Animator tickAnimator;
	

	void Start () {
		tickAnimator = ticker.GetComponent<Animator> () as Animator;
		animator = new Animator[buttons.Length];
		for(int i = 0; i < buttons.Length; i++){
			animator[i] = buttons[i].GetComponent<Animator>() as Animator;
			sizeMethod (buttons[i], sockets[(i)]);
		}

		children = new Transform[sockets.Length];
		for (int i = 0; i< sockets.Length; i++) {
			if (sockets [i].transform.childCount > 0) {
				children[i] = sockets [i].transform.GetChild (0);
			} else {
				children[i] = null;
			}
		}

		sceneAnimator = new Animator[scenes.Length];
		for(int i = 0; i < scenes.Length; i++){
			sceneAnimator [i] = scenes[i].GetComponent<Animator>() as Animator;
			sizeMethod (scenes[i], sceneSockets[i]);
		}

		sceneChildren = new Transform[scenes.Length];
		for (int i = 0; i< sceneSockets.Length; i++) {
			if (sceneSockets [i].transform.childCount > 0) {
				sceneChildren[i] = sceneSockets [i].transform.GetChild (0);
			} else {
				sceneChildren[i] = null;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown (KeyCode.UpArrow) || Input.GetAxis("Mouse ScrollWheel")> 0){
			for(int i = 0; i < sockets.Length; i++){
				int x = 1;
				buttonMovement (sockets[i], x, i);
			}
			for (int i = 0; i< sockets.Length; i++) {
				if (sockets [i].transform.childCount > 0) {
					children[i] = sockets [i].transform.GetChild (0);
				} else {
					children[i] = null;
				}
			}
		}

		if(Input.GetKeyDown (KeyCode.DownArrow) || Input.GetAxis("Mouse ScrollWheel")< 0){
			for(int i = 0; i < sockets.Length; i++){
				int x = -1;
				buttonMovement (sockets[i], x, i);
			}
			for (int i = 0; i < sockets.Length; i++) {
				if (sockets [i].transform.childCount > 0) {
					children[i] = sockets [i].transform.GetChild (0);
				} else {
					children[i] = null;
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			for(int i = 0; i < sceneSockets.Length; i++){
				int x = 1;
				sceneMovement (sceneSockets[i], x, i);
			}
			for (int i = 0; i < sceneSockets.Length; i++) {
				if (sceneSockets [i].transform.childCount > 0) {
					sceneChildren[i] = sceneSockets [i].transform.GetChild (0);
				} else {
					sceneChildren[i] = null;
				}
			}
		}

		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			for(int i = 0; i < sceneSockets.Length; i++){
				int x = -1;
				sceneMovement (sceneSockets[i], x, i);
			}
			for (int i = 0; i < sceneSockets.Length; i++) {
				if (sceneSockets [i].transform.childCount > 0) {
					sceneChildren[i] = sceneSockets [i].transform.GetChild (0);
				} else {
					sceneChildren[i] = null;
				}
			}
		}
	}

	public void eventSelect (int y){
		int x = y;
		if (y > 1) {
			x = 1;
		} else if (y < -1) {
			x=-1;
		}
		for(int a = 0; a < Mathf.Abs(y); a++){
			for (int i = 0; i < sockets.Length; i++) {
				buttonMovement (sockets [i], x, i);
			}
			for (int i = 0; i< sockets.Length; i++) {
				if (sockets [i].transform.childCount > 0) {
					children [i] = sockets [i].transform.GetChild (0);
				} else {
					children [i] = null;
				}
			}
		}
	}

	public void sceneSelect (int y){
		Debug.Log ("test");
		int x = y;
		if (y > 1) {
			x = 1;
		} else if (y < -1) {
			x=-1;
		}
		for(int a = 0; a < Mathf.Abs(y); a++){
			for(int i = 0; i < sceneSockets.Length; i++){
				sceneMovement (sceneSockets[i], x, i);
			}
			for (int i = 0; i < sceneSockets.Length; i++) {
				if (sceneSockets [i].transform.childCount > 0) {
					sceneChildren[i] = sceneSockets [i].transform.GetChild (0);
				} else {
					sceneChildren[i] = null;
				}
			}
		}
	}


	
	void buttonMovement (GameObject socket, int x, int index){
		if(socket.transform.childCount > 0){
			if(children.Length >= index){
				Transform buttonTransform = children[index];
				GameObject button;
				if(x == 1){
					tickAnimator.SetTrigger("Up Key");
					animator[index].SetTrigger("Up Key");
				} else if(x == -1){
					tickAnimator.SetTrigger("Down Key");
					animator[index].SetTrigger("Down Key");
				}
				if(buttonTransform){
					button = buttonTransform.gameObject;
					if (index == (sockets.Length-1) && x == 1) {
						sizeMethod (button, sockets[0]);
						return;
					}
					if (index == 0 && x == -1) {
						sizeMethod (button , sockets[(sockets.Length -1)]);
						return;
					}
					sizeMethod (button, sockets[(index + x)]);
				}
			}
		}
	}

	void sceneMovement (GameObject socket, int x, int index){
		if(socket.transform.childCount > 0){
			if(sceneChildren.Length >= index){
				Transform sceneTransform = sceneChildren[index];
				GameObject scene;
				if(x == 1){
					sceneAnimator[index].SetTrigger("Right Key");
				} else if(x == -1){
					sceneAnimator[index].SetTrigger("Left Key");
				}
				if(sceneTransform){
					scene = sceneTransform.gameObject;
					if (index == (sceneSockets.Length - 1) && x == 1) {
						sizeMethod (scene, sceneSockets[0]);
						return;
					}
					if (index == 0 && x == -1) {
						sizeMethod (scene , sceneSockets[(sceneSockets.Length - 1)]);
						return;
					}
					sizeMethod (scene, sceneSockets[(index + x)]);
				}
			}
		}
	}


	void sizeMethod (GameObject item, GameObject Destination){
		item.transform.SetParent (Destination.transform);
		item.GetComponent<RectTransform>().anchoredPosition = Destination.GetComponent<RectTransform>().anchoredPosition;
		item.GetComponent<RectTransform>().pivot = Destination.GetComponent<RectTransform>().pivot;
		item.GetComponent<RectTransform>().anchorMax = Vector2.one;
		item.GetComponent<RectTransform>().anchorMin = Vector2.zero;
		item.GetComponent<RectTransform>().offsetMax = Vector2.zero;
		item.GetComponent<RectTransform>().offsetMin = Vector2.zero;
	}

}
































