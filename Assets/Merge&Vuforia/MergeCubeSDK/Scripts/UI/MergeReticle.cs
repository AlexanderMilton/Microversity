using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MergeCube;

public class MergeReticle : MonoBehaviour
{
	public static MergeReticle instance;
	public Transform reticle;
	public Sprite fullScreenSprite;
	public Sprite vrScreenSprite;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		defaultScale = reticle.localScale;
		MergeCubeSDK.instance.OnViewModeSwap += ViewModeSwitch;
	}

	void OnDestroy()
	{
		MergeCubeSDK.instance.OnViewModeSwap -= ViewModeSwitch;
	}


	bool isEnabledInPhoneMode = true;
	bool isVRMode = false;

	void ViewModeSwitch(bool isVRModeTp)
	{
		isVRMode = isVRModeTp;
		reticle.GetComponent<SpriteRenderer>().sprite = isVRMode ? vrScreenSprite : fullScreenSprite;
		SetBackState();
	}

	bool gameLock = false;

	public void GameOverwrite(bool lockTp, bool showReticleTp = false)
	{
		gameLock = lockTp;
		if (lockTp)
		{
			SetReticleActive(showReticleTp, true);
		}
		else
		{
			SetBackState();
		}
	}

	void SetBackState()
	{
		if (isVRMode)
		{
			SetReticleActive(true);
		}
		else
		{
			SetReticleActive(isEnabledInPhoneMode);
		}
	}

	public void EnableReticle(bool isEnableTp)
	{
		isEnabledInPhoneMode = isEnableTp;
		if (!isVRMode)
		{			
			SetReticleActive(isEnabledInPhoneMode);
		}
	}

	void SetReticleActive(bool isActive, bool isGameLockSet = false)
	{
		if (gameLock && !isGameLockSet)
		{
			return;
		}
		reticle.gameObject.SetActive(isActive);
	}
	//Animations
	public void OnHoverAction()
	{
		StartScaleLerp(maxScaleMult, scaleUpDuration);
	}

	public void OffHoverAction()
	{
		StartScaleLerp(minScaleMult, scaleDownDuration);
	}

	public void OnClickAction()
	{
		reticle.transform.localScale = defaultScale * .5f;
	}

	public void OffClickAction()
	{
		reticle.transform.localScale = defaultScale;
	}


	Vector3 defaultScale;
	public float maxScaleMult = 1.5f;
	public float minScaleMult = .8f;

	[Space(20)]
	public float scaleUpDuration = 1f;
	public float scaleDownDuration = 1f;


	IEnumerator ScaleLerp(float targetScaleMult, float timerDuration)
	{
		Vector3 startingScale = reticle.localScale;
		Vector3 targetScale = defaultScale * targetScaleMult;
		float time = 0f;

		while ((time / timerDuration) < 1f)
		{
			reticle.localScale = Vector3.Lerp(startingScale, targetScale, time / timerDuration);
			time += Time.deltaTime;
			yield return null;
		}
		reticle.localScale = targetScale;
	}


	Coroutine scaleLerpCo;

	void StartScaleLerp(float targetScaleMult, float timerDuration)
	{
		StopScaleLerp();
		scaleLerpCo = StartCoroutine(ScaleLerp(targetScaleMult, timerDuration));
	}


	void StopScaleLerp()
	{
		if (scaleLerpCo != null)
		{
			StopCoroutine(scaleLerpCo);
		}
	}
		
}
