using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour {

	private Animator animator;
	public string openChestAnimation;
	public string closeChestAnimation;

	private bool isOpen;

	void Start()
	{
		animator = GetComponent<Animator>();
		isOpen = false;
	}

	public void Clicked()
	{
		if (!isOpen)
			OpenChest();
		else
			CloseChest();
	}

	private void OpenChest()
	{
		isOpen = true;
		animator.Play(openChestAnimation);
	}

	private void CloseChest()
	{
		isOpen = false;
		animator.Play(closeChestAnimation);
	}
}

