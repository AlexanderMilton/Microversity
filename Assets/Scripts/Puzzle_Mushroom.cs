using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Puzzle_Mushroom : MonoBehaviour {

	public List<Big_Mushroom> Mushrooms;
	public PuzzleMushroomWin WinMushroom;

	private int pressed;
	private Animator animator;
	private bool isComplete;

	// Use this for initialization
	void Start () {
		// Randomize order of mushrooms in their list
		Mushrooms = new List<Big_Mushroom> { Mushrooms[0], Mushrooms[1], Mushrooms[2], Mushrooms[3], Mushrooms[4] };
		var rnd = new System.Random();
		Mushrooms = Mushrooms.OrderBy(i => rnd.Next()).ToList();

		isComplete = false;
	}
	
	// Update is called once per frame
	void Update () {

		// Reset count of currently pressed shrooms
		pressed = 0;

		// Check if puzzle is complete
		if (isComplete)
			return;

		// Count the number of currently pressed shrooms
		foreach (Big_Mushroom M in Mushrooms)
		{
			if (M.isPressed)
				pressed++;
		}

		// Check if we have pressed the shrroms in correct order
		switch (pressed)
		{
			case 0:
			case 1:
				break;
			case 2:
				if (Mushrooms[2].isPressed || Mushrooms[3].isPressed || Mushrooms[4].isPressed)
					ResetPuzzle();
				break;
			case 3:
				if (Mushrooms[3].isPressed || Mushrooms[4].isPressed)
					ResetPuzzle();
				break;
			case 4:
				if (Mushrooms[4].isPressed)
					ResetPuzzle();
				break;
			case 5:
				PuzzleComplete();
				break;
		}
	}

	// Reset all the shrooms
	void ResetPuzzle()
	{
		foreach (Big_Mushroom Shroom in Mushrooms)
		{
			Shroom.ResetMushroom();
		}
	}

	// Start puzzle complete effects and mark puzzle as complete
	void PuzzleComplete()
	{
		WinMushroom.DoTheThing();
		isComplete = true;
	}
}

/* Puzzle notes
 * 
 * void Reset puzzle
 * 
 * void randomize order (5 numbers)
 * 
 * puzzle finished, unlock basket of tasty ass mushroom
 * 
 * 
 * 
 * Musroom notes
 * 
 * when clicked, ask puzzle if correct (and more than one has been clicked!)
 * 
 * void reset
 * 
 * void lock
 * 
 */