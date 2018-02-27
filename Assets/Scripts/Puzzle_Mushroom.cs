using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Puzzle_Mushroom : MonoBehaviour {

	public List<GameObject> Mushrooms;

	// Use this for initialization
	void Start () {
		// Randomize order of mushrooms in their list
		Mushrooms = new List<GameObject> { Mushrooms[0], Mushrooms[1], Mushrooms[2], Mushrooms[3], Mushrooms[4] };
		Mushrooms = Mushrooms.OrderBy(i => Random.value).ToList();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void ResetPuzzle()
	{

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