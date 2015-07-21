using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;// Required when using Event data.

[RequireComponent(typeof(Button))]
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler// required interface when using the OnPointerEnter method.
{
	//public UnityAction onHover;
	//public UnityEvent onHover;
	public Text text;
	public Image image;
	public string textOnHover {get;set;}
	string textOnDeHover {get;set;}
	Color originalColor;
	
	//Do this when the cursor enters the rect area of this selectable UI object.
	public void OnPointerEnter (PointerEventData eventData) 
	{
		if(image == null)
		{
			image = text.GetComponentInParent <Image>();
			originalColor = image.color;
		}
		
		textOnDeHover = text.text;
		text.text = textOnHover;
		
		if(image)
		{
			Color clr = Color.cyan ;
			clr.a = .4f;
			image.color = clr; 
		}
			
	}
	
	public void OnPointerExit (PointerEventData eventData) 
	{
		text.text = textOnDeHover;
		
		if(image)
			image.color = originalColor;
	}	
}