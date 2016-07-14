using UnityEngine;
using System.Collections;

public class PCControl : MonoBehaviour {

	public GameObject trackingSpace;
	public Server server;
	public Canvas canvas;

	private bool mouseHidden = false;
	private float distance = 0;

	private float MinDistance = 4;
	private float MaxDistance = 12;
	private float ScrollKeySpeed = -1f;
	private float rotationX, rotationY;
	
	// Use this for initialization
	void Start() 
	{
		distance = canvas.transform.localPosition.z;
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			KeyControl();
			MouseControl();
		}
	}

	void KeyControl()
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetKeyDown(KeyCode.UpArrow))
				server.Send("TouchScreen Keyboard Size", "+");
			if (Input.GetKeyDown(KeyCode.DownArrow))
				server.Send("TouchScreen Keyboard Size", "-");
			if (Input.GetKeyDown(KeyCode.M))
				server.Send("Mode", "Change");
		}
	}

	void MouseControl() 
	{
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			if (Input.GetAxis("Mouse ScrollWheel") != 0)
			{
				distance += Input.GetAxis("Mouse ScrollWheel") * ScrollKeySpeed;
				if (distance < MinDistance)
					distance = MinDistance;
				else if (distance > MaxDistance)
					distance = MaxDistance;
			}
			Vector3 pos = canvas.transform.localPosition;
			canvas.transform.localPosition = new Vector3(pos.x, pos.y, distance);
		}
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
