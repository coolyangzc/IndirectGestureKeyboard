using UnityEngine;
using System.Collections;

public class TrailRendererHelper : MonoBehaviour 
{
	protected TrailRenderer mTrail;
	
	// Use this for initialization
	void Start () 
	{
		mTrail = gameObject.GetComponent<TrailRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void Reset(float time = 1f)
	{
		StartCoroutine(ResetTrails(time));
	}

	IEnumerator ResetTrails(float time)
	{
		mTrail.time = -1f;
		yield return new WaitForEndOfFrame();
		mTrail.time = time;
	}
}
