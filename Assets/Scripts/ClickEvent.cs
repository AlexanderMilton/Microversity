using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickEvent : MonoBehaviour {

	RaycastHit hit;
	Ray ray;

	// Use this for initialization
	void Start () {
		
	}

	void Update()
	{
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if(Input.GetKeyDown(KeyCode.Mouse0))
		{
			if (Physics.Raycast(ray, out hit))
			{
				//Debug.Log(" you clicked on " + hit.collider.gameObject.name);

				hit.collider.gameObject.SendMessage("Clicked");
			}
		}
	}
}
