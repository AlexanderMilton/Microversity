using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour {

	public string openChestAnimation;
	public string closeChestAnimation;
	public Animator animator;
	public Animation chestAnimation;

	private bool isOpen = false;

	void Start()
	{
		animator = GetComponent<Animator>();
	}

	void Update()
	{
		Debug.Log(chestAnimation.clip.length);
	}

	private void OpenChest()
	{
		isOpen = true;
		animator.Play(openChestAnimation);
	}

	//private IEnumerator CloseChest()
	//{
	//	animator.Play(closeChestAnimation);
	//	yield return new WaitForSeconds(chestAnimation.clip.length);
	//}

	public void Clicked()
	{
		if (!isOpen)
			OpenChest();
	}
}

