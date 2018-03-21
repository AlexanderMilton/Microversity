using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleMushroomWin : MonoBehaviour {

	private Animator animator;
	public string WinAnimationName;

	// Use this for initialization
	void Start ()
	{
		animator = GetComponent<Animator>();
	}
	
	public void DoTheThing()
	{
		Debug.Log("Do The Thing");
		animator.Play(WinAnimationName);
	}
}
