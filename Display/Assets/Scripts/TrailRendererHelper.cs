using UnityEngine;
using System.Collections;

public class TrailRendererHelper : MonoBehaviour 
{
	protected TrailRenderer mTrail;
	protected float mTime;
	
	// Use this for initialization
	void Start () 
	{
		mTrail = gameObject.GetComponent<TrailRenderer>();
		mTime = mTrail.time;
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void Reset()
	{
		StartCoroutine(ResetTrails());
	}

	IEnumerator ResetTrails()
	{
		mTrail.time = -1f;
		yield return new WaitForEndOfFrame();
		mTrail.time = mTime;
	}
}
