using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Big_Mushroom : MonoBehaviour {

	private Animator animator;
	public string clickMushroomAnimation;
	public string resetMushroomAnimation;

	public bool isPressed;
	
	void Start ()
	{
		animator = GetComponent<Animator>();
		isPressed = false;
	}

	public void Clicked()
	{
		if (!isPressed)
			PressMushroom();
		else
			ResetMushroom();
	}

	private void PressMushroom()
	{
		isPressed = true;
		animator.Play(clickMushroomAnimation);
	}

	public void ResetMushroom()
	{
		isPressed = false;
		animator.Play(resetMushroomAnimation);
	}
}
