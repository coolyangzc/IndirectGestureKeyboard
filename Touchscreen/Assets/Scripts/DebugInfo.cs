using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugInfo : MonoBehaviour 
{
	public Text info;
	public Image keyboard;
	int num;
	
	// Use this for initialization
	void Start () 
	{
		info.text = "debug";
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.touchCount > 0) 
		{
			info.text = "";
			for (int i = 0; i < Input.touchCount; ++i)
			{
				info.text += Input.GetTouch(i).position.x.ToString() + " " + Input.GetTouch(i).position.y.ToString() + " ";
				Vector2 local;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					keyboard.transform as RectTransform, Input.GetTouch(i).position, Camera.main, out local);
				info.text += local.x.ToString() + " " + local.y.ToString() + "\n";
			}
				
		}

		//info.text = Input.touchCount.ToString();
	}

}
