using UnityEngine;
using System.Collections;

public class PCControl : MonoBehaviour {

	public GameObject trackingSpace;

	private bool mouseHidden = true;
	private float rotationX, rotationY;
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			MouseControl();
	}

	void MouseControl() 
	{
		if (Input.GetKeyDown(KeyCode.Escape)) 
			mouseHidden ^= true;

		if (mouseHidden) 
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;

			rotationX = trackingSpace.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * 5f;
			rotationY += Input.GetAxis("Mouse Y") * 5f; 
			rotationY = Mathf.Clamp(rotationY, -60f, 60f);
			trackingSpace.transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
		} else 
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		
	}
}
