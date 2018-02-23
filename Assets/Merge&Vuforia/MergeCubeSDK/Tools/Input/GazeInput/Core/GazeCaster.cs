using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using MergeCube;

public class GazeCaster : MonoBehaviour
{
	public static GazeCaster instance;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			DestroyImmediate(this.gameObject);
	}

	[HideInInspector]
	public RaycastHit hit;
	public LayerMask lMask;

	bool currentlyGazing = false;

	public bool GetCurrentGazeState()
	{
		return currentlyGazing;
	}

	GameObject gazedObject = null;
	GazeResponder gazeResponder = null;
	GazeResponder pressedObject = null;

	public delegate void GazeEvent();

	public event GazeEvent OnGaze_Start;
	public event GazeEvent OnGaze_End;
	public event GazeEvent OnGaze_InputDown;
	public event GazeEvent OnGaze_InputUp;

	public enum GazeModeEnum
	{
		tapFirst,
		tapOnly,
		gazeFirst,
		gazeOnly,
		hoverOnly,
		off
	}

	bool isVRMode = false;
	public GazeModeEnum currMode = GazeModeEnum.tapFirst;
	[Header("Tag of button can receive tap input. If null everything will.")]
	public string tapInputIdentifier = "TapInput";

	public void SwapScreenViewMode(bool isVRModeTp)
	{
		isVRMode = isVRModeTp;
	}

	void Start()
	{
		MergeCubeSDK.instance.OnViewModeSwap += SwapScreenViewMode;
		Invoke("DelayStart", .5f);
	}

	void DelayStart()
	{
		ChangeInputMode(currMode);
	}

	void OnDestroy()
	{
		MergeCubeSDK.instance.OnViewModeSwap -= SwapScreenViewMode;
	}

	void Update()
	{
		if (currMode != GazeModeEnum.off)
		{
			CheckInput();
		}
	}

	public void ChangeInputMode(GazeModeEnum changeTo)
	{
		currMode = changeTo;
		if (currMode == GazeModeEnum.tapOnly || currMode == GazeModeEnum.off)
		{
			if (MergeReticle.instance != null)
			{
				MergeReticle.instance.EnableReticle(false);
			}
		}
		else
		{
			if (MergeReticle.instance != null)
			{
				MergeReticle.instance.EnableReticle(true);
			}
		}
	}

	void CheckInput()
	{
		if (isVRMode)
		{
			//Always use gaze input
			TryGazeInput();
		}
		else
		{
			if (currMode == GazeModeEnum.tapOnly)
			{
				TryTapInput();
			}
			else if (currMode == GazeModeEnum.tapFirst)
			{
				if (!TryTapInput())
				{
					TryGazeInput();
				}
			}
			else if (currMode == GazeModeEnum.gazeFirst)
			{
				if (!TryGazeInput())
				{
					TryTapInput();
				}
			}
			else if (currMode == GazeModeEnum.gazeOnly || currMode == GazeModeEnum.hoverOnly)
			{
				TryGazeInput();
			}
		}

		if (Input.GetMouseButtonDown(0) && IsValidClick())
		{
			if (currMode != GazeModeEnum.hoverOnly)
			{
				TriggerPressed();
			}
		}

		if (Input.GetMouseButtonUp(0) && IsValidClick())
		{
			if (currMode != GazeModeEnum.hoverOnly)
			{
				TriggerReleased();
				if (isVRMode)
				{
					currentlyGazing = false;
					gazedObject = null;
					gazeResponder = null;
				}
			}
		}
	}

	bool TryTapInput()
	{
		if (Input.GetMouseButton(0))
		{
			Ray ray = new Ray();
//			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			ray = Camera.main.ViewportPointToRay(new Vector3(Input.mousePosition.x / (float)Screen.width, Input.mousePosition.y / (float)Screen.height, 0));
			return CheckRay(ray, true);
		}
		else
		{
			ResetGaze();
		}
		return false;
	}

	bool TryGazeInput()
	{
		Ray ray = new Ray();
		ray.origin = this.transform.position;
		ray.direction = this.transform.forward;
		return CheckRay(ray);
	}

	bool CheckRay(Ray ray, bool isTapTp = false)
	{
		if (Physics.Raycast(ray, out hit, 100000f, lMask))
		{             
			if (isTapTp)
			{
				if (tapInputIdentifier != "" && hit.transform.tag != tapInputIdentifier)
				{
					return false;
				}
			}
			if (hit.transform.gameObject != gazedObject)
			{
				if (gazeResponder != null)
				{
					gazeResponder.OnGazeExit();

					if (MergeReticle.instance != null)
						MergeReticle.instance.OffHoverAction();

					if (OnGaze_End != null)
					{
						OnGaze_End.Invoke();
					}
				}
				currentlyGazing = false;
				gazedObject = hit.transform.gameObject;
				gazeResponder = hit.transform.GetComponent<GazeResponder>();
			}

			//We were  NOT previously looking at something last tick, so lets have it do stuff
			//I must be looking at something new, look at the new thing.
			if (!currentlyGazing && gazedObject != null && gazeResponder != null)
			{
				gazeResponder.OnGazeEnter();

				if (MergeReticle.instance != null)
					MergeReticle.instance.OnHoverAction();

				if (OnGaze_Start != null)
				{
					OnGaze_Start.Invoke();
				}
				currentlyGazing = true;
			}
			return true;
		}
		else// if (!isTapTp)
		{
			ResetGaze();
		}
		return false;
	}

	void ResetGaze()
	{
		if (currentlyGazing && gazeResponder != null && gazedObject != null)
		{
			gazeResponder.OnGazeExit();

			if (MergeReticle.instance != null)
				MergeReticle.instance.OffHoverAction();

			if (OnGaze_End != null)
			{
				OnGaze_End.Invoke();
			}
		}

		currentlyGazing = false;
		gazedObject = null;
		gazeResponder = null;
	}

	public void TriggerPressed()
	{
		if (!enabled)
		{
			return;
		}

		if (MergeReticle.instance != null)
			MergeReticle.instance.OnClickAction();

		if (currentlyGazing && gazedObject != null && gazeResponder != null)
		{
			gazeResponder.OnGazeTrigger();
			pressedObject = gazeResponder;
		}
		if (OnGaze_InputDown != null)
		{
			OnGaze_InputDown.Invoke();
		}
	}

	public void TriggerReleased()
	{
		if (!enabled)
		{
			return;
		}
		if (MergeReticle.instance != null)
			MergeReticle.instance.OffClickAction();
		if (pressedObject != null)
		{
			pressedObject.OnGazeTriggerEnd();
			pressedObject = null;
		}

		if (OnGaze_InputUp != null)
		{
			OnGaze_InputUp.Invoke();
		}
	}

	public bool IsValidClick()
	{
//		if (MergeUserAccount.instance != null)
//		{
//			if (!MergeUserAccount.instance.conf.shouldUIBtnBlockGazeClick)
//			{
//				return true;
//			}
//		}
		int pointerID = -1;
		for (int i = 0; i < Input.touchCount; i++)
		{
			if (Input.GetTouch(i).phase == TouchPhase.Began)
			{
				pointerID = Input.GetTouch(i).fingerId;
			}
		}
		return !EventSystem.current.IsPointerOverGameObject(pointerID);
	}
}
