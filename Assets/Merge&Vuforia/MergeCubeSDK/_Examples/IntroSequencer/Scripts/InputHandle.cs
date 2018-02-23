using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputHandle : MonoBehaviour
{
	public void Click()
	{
		transform.localScale = Vector3.one * 1.2f;
		CancelInvoke("SetBack");
		Invoke("SetBack", .4f);
	}

	void SetBack()
	{
		transform.localScale = Vector3.one;
	}
	// Use this for initialization
	void Start()
	{
		
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
}
