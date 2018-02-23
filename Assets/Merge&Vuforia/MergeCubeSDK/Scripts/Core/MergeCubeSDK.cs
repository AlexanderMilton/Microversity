
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Vuforia;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace MergeCube
{
	[ExecuteInEditMode]
	public class MergeCubeSDK : MergeCubeBase
	{
		public static MergeCubeSDK instance;

		void Awake()
		{
//			Debug.LogWarning("Key = "+MergeCubeCore.developmentKey);

			if ( instance == null )
			{
				instance = this;
			}
			else if ( instance != this )
			{
				DestroyImmediate( this );
				return;
			}
			#if !UNITY_EDITOR
			if (SystemInfo.deviceModel.Contains("iPad") || (Application.platform != RuntimePlatform.IPhonePlayer && DeviceDiagonalSizeInInches() >= 7.5f))
			{
//				Debug.Log("This is a tablet!");
				deviceIsTablet = true;
			}
			#else
			deviceIsTablet = false;
			#endif
			deviceScreenWidth = Screen.width;
			deviceScreenHeight = Screen.height;

			if ( ( Screen.width == 2436 && Screen.height == 1125 ) || ( Screen.width == 1125 && Screen.height == 2436 ) )
			{
				menuButton.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2( 135f, -65f );
				recordingButton.parent.GetComponent<RectTransform>().anchoredPosition = new Vector2( -135f, -65f );
			}
			else
			{
				menuButton.parent.GetComponent<RectTransform>().anchoredPosition = new Vector3( 90f, -65f );
				recordingButton.parent.GetComponent<RectTransform>().anchoredPosition = new Vector3( -90f, -65f );
			}

			if ( Application.isPlaying )
			{
				gameObject.AddComponent<MergeCubeScreenRotateManager>();
			}
		}

		int deviceScreenWidth = 0;
		int deviceScreenHeight = 0;

		private static ViewMode currentViewMode = ViewMode.FULLSCREEN;

		public static bool deviceIsTablet { get; private set; }

		private Transform arCameraRef;

		private VideoBackgroundBehaviour leftVidBackBehaviour;
		private VideoBackgroundBehaviour rightVidBackBehaviour;

		public GameObject headsetViewSetup;
		private RenderTexture headsetViewRenderTexture;

		public RenderTexture GetHeadsetTexture()
		{
			return headsetViewRenderTexture;
		}

		private bool isActive = false;

		public CanvasScaler canvasScaler;

		public Transform[] allUIPositions;
		//		public Transform[] dynamicUIPositions;

		[SerializeField]
		public List<MenuButton> menuButtons = new List<MenuButton>();

		//fixed position buttons
		public Transform menuButton;
		//0
		public Transform userAccountsButton;
		//5

		//dynamic position buttons
		public Transform flashlightButton;
		// almost always, may have edge cases
		public Transform viewSwitchButton;
		//not on ipad
		public Transform headsetCompatabilityButton;
		//Only visible when in headset mode.
		public Transform recordingButton;
		// not if there aren't plugins for it

		//Fixed Positions:
		private int menuButtonPriority = 0;
		private int recordingPriority = 5;

		//Dynamic Positions:
		private int flashlightPriority = 1;
		private int viewSwitchPriority = 2;
		private int headsetCompatabilityPriority = 3;
		private int userAccountsPriority = 4;

		public UnityEngine.UI.Image viewSwitchGraphic;
		public Sprite fullscreenSprite;
		public Sprite headsetViewSprite;
		public Sprite disabledSprite;

		public Text userAccountName;
		public Color defaultPurple;

		public bool isUsingMergeReticle = true;
		private GameObject reticleInstance;
		public GameObject reticlePrefab;

		public Animator mainPanelAnimator;
		bool menuIsOpen = false;

		public delegate void ViewModeSwapEvent( bool swappedToHeadsetView );

		public Callback OnInitializationComplete;

		public ViewModeSwapEvent OnViewModeSwap;

		//		bool useMergeUserAccount = false;
		//		public bool userAccountDebugMode = false;

		public MergeConfigurationFile confFile;

		void Start()
		{
			if ( confFile != null )
			{
				RegisterListeners();

				if ( Application.isPlaying )
				{
					MergeCubeCore.instance.StartCore( confFile.licenseKey, confFile.chosenScaleFactor );
				}
			}

			//We only want the configuration file setup to run in edit mode, the rest of the setup is for runtime.
			if ( Application.isPlaying )
			{
				//unity base settings setup:
				Application.targetFrameRate = 60;
				MergeCubeScreenRotateManager.instance.OnOrientationEvent += HandleCanvasScalerOnOrientation;

				//unity base scene setup:
				if ( GameObject.Find( "EventSystem" ) == null )
				{
					GameObject tmp = new GameObject( "EventSystem" );
					EventSystem evnSys = tmp.AddComponent<EventSystem>();
					StandaloneInputModule inpModule = tmp.AddComponent<StandaloneInputModule>();
				}

				//our setup:
				Initialize();
			}
			InitValidClick();
		}


		//When user click on UserAccountButton.
		public void LaunchUserLoginPage()
		{

		}

		void HandleUserLogin(string result)
		{

		}

		public void UpdateUserAccountInit()
		{

		}

		void Initialize()
		{
			InitVuforia();

//			#if UNITY_ANDROID
//			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

//			Debug.Log("Screen dimensions:" + Screen.width + ", " + Screen.height);

//			canvasScaler.referenceResolution = new Vector2( Screen.width, Screen.height );
//			canvasScaler.matchWidthOrHeight = ( Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight ) ? 0f : 1f;

//			canvasScaler.matchWidthOrHeight = ( Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight ) ? 0f : 1f;

//			#elif UNITY_IOS

			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
			canvasScaler.scaleFactor = 0.8f;

//			#endif

			AddMenuElement( flashlightButton, flashlightPriority );

			if ( !deviceIsTablet )
			{
				AddMenuElement( viewSwitchButton, viewSwitchPriority );
			}

			if ( RecordingManager.isUsingRecordingPlugin )
			{
				AddMenuElement( recordingButton, recordingPriority );
			}
				
			//sort menubuttons by priority
			RefreshMenuButtonPositions();

			arCameraRef = Camera.main.transform;

			viewMode = currentViewMode;

			if ( headsetViewRenderTexture == null )
			{
				CreateRenderTexture();
			}

			if ( currentViewMode == ViewMode.HEADSET )
			{
				Camera.main.targetTexture = headsetViewRenderTexture;
				headsetViewSetup.SetActive( true );
			}
			else
			{
				headsetViewSetup.SetActive( false );
			}

			//This is to make sure that this version of the sdk has a way to push the contents of the target render texture to the screen in mobile mode.
//			if (Camera.main.GetComponent<Blitter>() == null)
//			{
//				Camera.main.gameObject.AddComponent<Blitter>();	
//			}

			if ( isUsingMergeReticle )
			{
				if ( reticleInstance == null && reticlePrefab != null )
				{
					reticleInstance = GameObject.Instantiate( reticlePrefab, arCameraRef.position, arCameraRef.rotation, arCameraRef ) as GameObject; 
				}
			}

			if ( OnInitializationComplete != null )
			{
				OnInitializationComplete.Invoke();
			}
		}

		void HandleCanvasScalerOnOrientation(ScreenOrientation grabbedOrientation)
		{
			int screenWidth = ( grabbedOrientation == ScreenOrientation.Portrait ) ? deviceScreenWidth : deviceScreenHeight;
			int screenHeight = ( grabbedOrientation == ScreenOrientation.Portrait ) ? deviceScreenHeight : deviceScreenWidth;
			canvasScaler.referenceResolution = new Vector2( screenWidth, screenHeight );
			canvasScaler.matchWidthOrHeight = ( Screen.orientation == ScreenOrientation.Landscape || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight ) ? 0f : 1f;
		}

		public void RefreshMenuButtonPositions()
		{
			//sort menubuttons by priority
			menuButtons.Sort( (x, y) => x.targetPosition.CompareTo( y.targetPosition ) );

			for ( int index = 0; index < menuButtons.Count; index++ )
			{
				if ( menuButtons[ index ].useFixedPosition )
				{
					menuButtons[ index ].button.SetParent( allUIPositions[ menuButtons[ index ].targetPosition ] );
				}
				else
				{
					menuButtons[ index ].button.SetParent( allUIPositions[ index ] );
				}

				menuButtons[ index ].button.localPosition = Vector3.zero;
			}
					
		}

		public void AddMenuElement(Transform button, int targetPosition, bool useFixedPosition = false)
		{
			
			button.GetComponent<Button>().interactable = true;

			if ( menuButtons.Find( x => x.button == button ) != null )
			{
				return;
			}

			//button.gameObject.SetActive( true );

			MenuButton buttonToAdd = new MenuButton( button, targetPosition, useFixedPosition );
			menuButtons.Add( buttonToAdd );

			RefreshMenuButtonPositions();
		}

		public void RemoveMenuElement(Transform button)
		{
			MenuButton buttonToRemove = menuButtons.Find( x => x.button == button ); 

			if ( buttonToRemove != null )
			{
				//We don't want the buttons to go away now, instead we want to set them to a disabled state
//				menuButtons.Remove( buttonToRemove );
//				button.gameObject.SetActive( false );

				button.GetComponent<Button>().interactable = false;
			}

			RefreshMenuButtonPositions();
		}

		public Material videoTexture;

		void CreateRenderTexture()
		{
			headsetViewRenderTexture = new RenderTexture( 1488, 750, 24, RenderTextureFormat.ARGB32 );
			headsetViewRenderTexture.Create();

			videoTexture.SetTexture( "_Texture", headsetViewRenderTexture );
		}

		#if UNITY_EDITOR
		void OnValidate()
		{
			if ( confFile != null )
			{
				
				RegisterListeners();
			}
			else
			{
				TryFindConfFile();
			}
		}

		void TryFindConfFile()
		{
			MergeCubeCore core = GameObject.FindObjectOfType<MergeCubeCore>();

			MergeConfigurationFile[] results = ( MergeConfigurationFile[] )Resources.FindObjectsOfTypeAll( typeof( MergeConfigurationFile ) );

			if ( results.Length > 0 )
			{
				confFile = results[ 0 ];
				RegisterListeners();
			}
			else
			{

				string path = "Assets/Resources";
				string assetPathAndName = path + "/" + typeof( MergeConfigurationFile ).ToString() + ".asset";

				//If the Assets/Resources folder does not exist, make it
				if ( !AssetDatabase.IsValidFolder( path ) )
				{
					AssetDatabase.CreateFolder( "Assets", "Resources" );
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}

				if ( core != null )
				{
					MergeConfigurationFile genConf = MergeConfigurationFile.CreateInstance<MergeConfigurationFile>();

					//Generate Configuration File
					AssetDatabase.CreateAsset( genConf, assetPathAndName );
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();

					//Go regrab our configuration file.
					results = ( MergeConfigurationFile[] )Resources.FindObjectsOfTypeAll( typeof( MergeConfigurationFile ) );

					if ( results.Length > 0 )
					{
						confFile = results[ 0 ];

						//populate it with existing data.

						if ( core.cubeScaleFactor == 0.072f )
						{
							confFile.scaleFactor = MergeConfigurationFile.ScaleFactor.PHYS_SIZE;
						}
						else if ( core.cubeScaleFactor == 1f )
						{
							confFile.scaleFactor = MergeConfigurationFile.ScaleFactor.ONE_METER;
						}
						else
						{
							confFile.scaleFactor = MergeConfigurationFile.ScaleFactor.CUSTOM;
							confFile.customScaleFactor = Mathf.Clamp( ( int )core.cubeScaleFactor, 1, 10 );
						}

						confFile.licenseKey = core.developmentKey;

						RegisterListeners();
					}
				}

			}
		}
		#endif

		public AnimationToggle menuButtonHandler;

		public void ToggleMenu()
		{
			if ( menuIsOpen )
			{
				mainPanelAnimator.Play( "Close" );
				menuButtonHandler.PlayDefaultStateAnimation( false );
			}
			else
			{
				mainPanelAnimator.Play( "Open" );
				menuButtonHandler.PlayDefaultStateAnimation( true );
			}
				
			menuIsOpen = !menuIsOpen;

			//if the menu close from some other button, swap the menu button back to the off state.
//			if ( !menuButton.GetComponentInChildren<ToggleSprite>().isOn && menuIsOpen == false )
//			{
//				menuButton.GetComponentInChildren<ToggleSprite>().ToggleSpriteVisuals();
//			}

			//And same here, make sure the menu's aren't out of sync with each other. The viewmode check is to make sure the menu actually is active.
			//Don't need to worry about this in the fullscreen mode.
			if ( currentViewMode == ViewMode.HEADSET )
			{
				if ( ExpandingMenuManager.instance.isOpen && menuIsOpen == false )
				{
					ExpandingMenuManager.instance.CollapseMenu();
				}
			}
		}

		public void SwitchView()
		{
			if ( currentViewMode == ViewMode.HEADSET )
			{
				currentViewMode = ViewMode.FULLSCREEN;
				viewMode = ViewMode.FULLSCREEN;
			}
			else
			{
				currentViewMode = ViewMode.HEADSET;
				viewMode = ViewMode.HEADSET;
			}

			if ( currentViewMode == ViewMode.HEADSET )
			{
				SetToHeadsetView();
				viewSwitchGraphic.sprite = headsetViewSprite;
			}
			else
			{
				SetToFullscreenView();
				viewSwitchGraphic.sprite = fullscreenSprite;
			}
				
//			RemoveMenuElement(viewSwitchButton);

			if ( OnViewModeSwap != null )
			{
				OnViewModeSwap.Invoke( ( viewMode == ViewMode.HEADSET ) );
			}

//			EnableViewChangeBtn();
		}

		//		void EnableViewChangeBtn()
		//		{
		//			AddMenuElement(viewSwitchButton, viewSwitchPriority);
		//		}


		void SetToFullscreenView()
		{
			MergeCubeScreenRotateManager.instance.SetOrientation( false );


			Camera.main.targetTexture = null;
			headsetViewSetup.SetActive( false );

//			canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
//			canvasScaler.matchWidthOrHeight = (Screen.orientation == ScreenOrientation.Landscape || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight) ? 0f : 1f;

			RemoveMenuElement( headsetCompatabilityButton );

		}

		public GameObject rotatePhoneGraphic;

		void SetToHeadsetView()
		{
			MergeCubeScreenRotateManager.instance.SetOrientation( true );

			if ( Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown )
			{
//				Debug.LogError("Swapping to headset view: was a portrait orientation");
				#if UNITY_EDITOR
//				Debug.Log("Forcing flip event for unity editor");
				FinishHeadsetViewSetup();
				#else
//				Debug.LogError("Trying to set rotate phone graphic");
				rotatePhoneGraphic.SetActive(true);
				MergeCubeScreenRotateManager.instance.OnOrientationEvent += HandleOrientationEvent;
				#endif
			}
			else
			{
//				Debug.LogError("was not a portrait view!!! " + Screen.orientation );
				FinishHeadsetViewSetup();
			}

//			canvasScaler.referenceResolution = new Vector2(Screen.width, Screen.height);
//			canvasScaler.matchWidthOrHeight = (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight) ? 0f : 1f;
		}

		public void HandleOrientationEvent(ScreenOrientation grabbedOrientation)
		{
			if ( grabbedOrientation == ScreenOrientation.Landscape || grabbedOrientation == ScreenOrientation.LandscapeLeft || grabbedOrientation == ScreenOrientation.LandscapeRight )
			{
				//Clean event handler
				#if !UNITY_EDITOR
				MergeCubeScreenRotateManager.instance.OnOrientationEvent -= HandleOrientationEvent;
				#endif

				//Finish setup after a half second to give the device time to finish rotating.
//				Invoke("FinishHeadsetViewSetup",0.5f);
				FinishHeadsetViewSetup();
				rotatePhoneGraphic.SetActive( false );
			}
		}

		void FinishHeadsetViewSetup()
		{
			if ( headsetViewRenderTexture == null )
			{
				CreateRenderTexture();
			}

			Camera.main.targetTexture = headsetViewRenderTexture;

			headsetViewSetup.SetActive( true );

			AddMenuElement( headsetCompatabilityButton, headsetCompatabilityPriority );

		}

		bool isFlashOn = false;

		void OnApplicationPause(bool pauseStatus)
		{
			if ( !pauseStatus )
			{
				if ( isFlashOn )
				{
					CancelInvoke( "DelayTurnFlshOn" );
					Invoke( "DelayTurnFlshOn", .1f );
				}
			}
//			Debug.LogWarning( "Application Pasue = " + pauseStatus );
		}

		void DelayTurnFlshOn()
		{
			TurnFlashOn();
		}

		public void SwitchFlashLight()
		{
			isFlashOn = !isFlashOn;

			if ( isFlashOn )
			{
				TurnFlashOn();
			}
			else
			{
				TurnFlashOff();
			}
		}

		void TurnFlashOff()
		{
			Vuforia.CameraDevice.Instance.SetFlashTorchMode( false );
		}

		void TurnFlashOn()
		{
//			Debug.LogWarning( "Should Turn On Flash" );
			Vuforia.CameraDevice.Instance.SetFlashTorchMode( true );
		}

		//Used to determine if the device is a tablet. iOS types check if it is an iPad via SystemInfo.deviceModel.Contains("iPad") in Awake check
		float DeviceDiagonalSizeInInches()
		{
			float screenWidth = Screen.width / Screen.dpi;
			float screenHeight = Screen.height / Screen.dpi;
			float diagonalInches = Mathf.Sqrt( Mathf.Pow( screenWidth, 2 ) + Mathf.Pow( screenHeight, 2 ) );

//			Debug.Log ("Getting device inches: " + diagonalInches);

			return diagonalInches;
		}

		bool vuforiaInited = false;

		public void InitVuforia()
		{
			bool shouldStart = true;
			#if UNITY_IOS && !UNITY_EDITOR
			shouldStart = MergeIOSBridge.CheckCamera ();
			#endif
			#if UNITY_ANDROID && !UNITY_EDITOR
			shouldStart = MergeAndroidBridge.HasPermission (AndroidPermission.CAMERA);
			#endif		
			if ( shouldStart && !vuforiaInited )
			{
				vuforiaInited = true;
				VuforiaRuntime.Instance.InitVuforia();
				StartCoroutine( InitVuforiaCamera() );
			}
		}

		IEnumerator InitVuforiaCamera()
		{
			yield return new WaitUntil( () => VuforiaRuntime.Instance.HasInitialized );
			Camera.main.transform.GetComponent<VuforiaBehaviour>().enabled = true;
		}



		float portraitY = 9999f;
		float portraitX = 0f;
		float landY = 9999f;
		float landX = 0f;

		void InitValidClick()
		{
			float basePY = 0;
			float baseLY = 0;
			//				if(Screen.orientation == 
			if ( Screen.orientation == ScreenOrientation.Landscape || Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight )
			{
				basePY = ( float )Screen.width;
				baseLY = ( float )Screen.height;
			}
			else
			{
				basePY = ( float )Screen.height;	 
				baseLY = ( float )Screen.width;
			}
			portraitY = basePY * 0.9025f;
			portraitX = basePY * 0.0975f * 1.2f;
			landY = baseLY * 0.8267f;
			landX = baseLY * 0.1733f * 1.2f;
		}

		public bool IsValidClick()
		{
			if ( Screen.orientation == ScreenOrientation.Portrait )
			{
				//1334 mode  .0975  (0.9025)
				if ( Input.mousePosition.y > portraitY )
				{
					if ( menuIsOpen )
					{
						return false;
					}
					else
					{
						if ( Input.mousePosition.x < portraitX )
						{
							return false;
						}
					}
				}		
			}
			else
			{
				//750 mode .1733   (0.8267);
				if ( viewMode == ViewMode.FULLSCREEN )
				{
					if ( Input.mousePosition.y > landY )
					{
						if ( menuIsOpen )
						{
							return false;
						}
						else
						{
							if ( Input.mousePosition.x < landX )
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}

		void RegisterListeners()
		{
			confFile.dataUpdateEvent = HandleDataUpdateEvent;

			//Send one event through regardless to set initial values.
			HandleDataUpdateEvent();
		}

		void OnDestroy()
		{
			//Sanitize Listeners
			confFile.dataUpdateEvent = null;
		}

		void HandleDataUpdateEvent()
		{
			//Push to Core
			MergeCubeCore.UpdateKey( confFile.licenseKey );
			MergeCubeCore.UpdateScale( confFile.chosenScaleFactor );
//			#if UNITY_EDITOR
//			UpdateScaleEditor( confFile.chosenScaleFactor );
//			#endif
			//		transform.localScale = new Vector3( confFile.chosenScaleFactor, confFile.chosenScaleFactor, confFile.chosenScaleFactor );
		}

		//		void UpdateScaleEditor(float scaleTp)
		//		{
		//			if (System.IO.File.Exists(Application.streamingAssetsPath + PATH_MultiFile + "MergeCube.xml"))
		//			{
		//				string xmlData = System.IO.File.ReadAllText(Application.persistentDataPath + PATH_MultiFile + "MergeCube.xml");
		//				xmlData = MergeCubeCore.ScaleFactorTheXML(xmlData);
		//				System.IO.File.WriteAllText(Application.persistentDataPath + PATH_MultiFile + "MergeCube.xml", xmlData);
		//				MergeUtility.SetFloat("cubeScaleFactor", scaleTp);
		//			}
		//			else
		//			{
		//				LoadInitData();
		//			}
		//		}
	}
}