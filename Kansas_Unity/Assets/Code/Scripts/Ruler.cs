using UnityEngine;
using System.Collections;

public class Ruler : MonoBehaviour {

	public int rulerLength = 100;
	
	void Start()
	{
		for(int i = 0 ; i < rulerLength ; ++i)
		{
			GameObject positiveHorizontalMarker = new GameObject();
			positiveHorizontalMarker.transform.position = transform.position + Vector3.right * i;
			positiveHorizontalMarker.transform.SetParent (transform);
			
			TextMesh positiveHorizontalTextMesh = positiveHorizontalMarker.AddComponent<TextMesh>();
			positiveHorizontalTextMesh.characterSize = .25f;
			positiveHorizontalTextMesh.alignment = TextAlignment.Center;
			positiveHorizontalTextMesh.anchor = TextAnchor.MiddleCenter;
			positiveHorizontalTextMesh.text = i + "m";
			
			if(i != 0)
			{
				GameObject negativeHorizontalMarker = new GameObject();
				negativeHorizontalMarker.transform.position = transform.position + Vector3.left * i;
				negativeHorizontalMarker.transform.SetParent (transform);
				
				TextMesh negativeHorizontalTextMesh = negativeHorizontalMarker.AddComponent<TextMesh>();
				negativeHorizontalTextMesh.characterSize = .25f;
				negativeHorizontalTextMesh.alignment = TextAlignment.Center;
				negativeHorizontalTextMesh.anchor = TextAnchor.MiddleCenter;
				negativeHorizontalTextMesh.text =  "-"+ i + "m";
			}
			
			GameObject positiveVerticalMarker = new GameObject();
			positiveVerticalMarker.transform.position = transform.position + Vector3.up * i;
			positiveVerticalMarker.transform.SetParent (transform);
			
			TextMesh positiveVerticalTextMesh = positiveVerticalMarker.AddComponent<TextMesh>();
			positiveVerticalTextMesh.characterSize = .25f;
			positiveVerticalTextMesh.alignment = TextAlignment.Center;
			positiveVerticalTextMesh.anchor = TextAnchor.MiddleCenter;
			positiveVerticalTextMesh.text = i + "m";
			
			if(i != 0)
			{
				GameObject negativeVerticalMarker = new GameObject();
				negativeVerticalMarker.transform.position = transform.position + Vector3.down * i;
				negativeVerticalMarker.transform.SetParent (transform);
				
				TextMesh negativeVerticalTextMesh = negativeVerticalMarker.AddComponent<TextMesh>();
				negativeVerticalTextMesh.characterSize = .25f;
				negativeVerticalTextMesh.alignment = TextAlignment.Center;
				negativeVerticalTextMesh.anchor = TextAnchor.MiddleCenter;
				negativeVerticalTextMesh.text =  "-"+ i + "m";
			}
			
			
		}
		
	}
}
