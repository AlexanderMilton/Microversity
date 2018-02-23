using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchColor : MonoBehaviour
{
	public Button btn;
	Text thisText;

	void Start()
	{
		thisText = this.GetComponent<Text>();
	}

	void Update()
	{
		if ( btn.interactable )
		{
			
		}
	}
}
