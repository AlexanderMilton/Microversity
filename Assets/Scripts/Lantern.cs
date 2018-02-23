using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lantern : MonoBehaviour {

	public Light light;
	public float maxLuminosity;

	// Use this for initialization
	void Start () {
		light.intensity = 0;
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void Clicked()
	{
		if (light.intensity == 0)
			light.intensity = maxLuminosity;
		else
			light.intensity = 0;
	}
}
