using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SCR_main : MonoBehaviour {
    
	[HideInInspector]
    public bool playerSelect = true;
	//hidden object to be used for main character select.
	
	public GameObject playerObj1;
	//the object to be used for the main character.
	
	public GameObject playerObj2;
	//the object to be used for the main character.
	
	public GameObject powerBoostEffectObj;
	//the object used for when the player has collected a power boosting item.
	
	public string[] stageName;
	//a list of the names of the stage scenes, in order from 1st stage to last.
	
	public bool hasMusic = true;
	//set to true if the game has music.
	
	public bool pointsOn = true;
	//set to true to have points pop out of the enemies when they are defeated.
	
	public bool highScoresOn = true;
	//set to true to use high scores.
	
	public bool optionsOn = true;
	//set to true to use options.
	
	public bool exitGameOn = true;
	//set to true to use exit game.
	
	public bool reticleOn = true;
	//set to true to use the aiming reticle.
	
	public bool hideMouseCursor = true;
	//set to true to hide the mouse cursor during the game play.
	
	
	public static string[] sName;
	
	public static GameObject healthBarObj;
	public static GameObject soundObj;
	
	public static Material[] particleMat;
	
	
	int[] screenSize=new int[2];
	public static float screenMult = 0f;
	public static float resMult = 0f;
	public static float aspectShrink = 0f;
	public static float counterMult=0.1f;
	
	public static int score=0;
	public static int scoreRec=0;
	//public static int[] highScore=new int[10];
	public static List<int> highScore = new List<int>(){0,0,0,0,0,0,0,0,0,0};
	public static int level=0;
	
	public static bool fadeReady=false;
	
	public static bool pOn;
	
	SCR_music music;
	public static bool hMusic;
	public static bool hsOn;
	public static bool opOn;
	public static bool egOn;
	
	public static int fxOn=1;
	public static Vector2 soundCentre;
	
	public static bool hCursor;
	
	public static int musOn=1;
	
	public static string gotoScene="menu";
	SCR_screenCover screenFade;
	
	
	public bool		forceMobileController = true;	// [DGT] Use this to test mobile controller in the editor

#if CONTROL_FREAK_INSTALLED
	[HideInInspector]	
	public TouchController cfCtrl;			// [DGT]
#endif

	// Use this for initialization
	void Awake () {
		
		music=GetComponent<SCR_music>();
		GetComponent<SCR_gui>().StartUp();
		
		healthBarObj=Resources.Load("Objects/Gui/OBJ_healthBar",typeof(GameObject)) as GameObject;
		soundObj=Resources.Load("Objects/Control/OBJ_sound",typeof(GameObject)) as GameObject;
		
		particleMat=new Material[2]{	Resources.Load("Materials/Effect/MAT_particleAlpha",typeof(Material)) as Material,
										Resources.Load("Materials/Effect/MAT_particleAdditive",typeof(Material)) as Material
		};
		
		pOn=pointsOn;
		hsOn=highScoresOn;
		opOn=optionsOn;
		egOn=exitGameOn;
		hMusic=hasMusic;
		sName=stageName;
		hCursor=hideMouseCursor;
		
		DontDestroyOnLoad(transform.gameObject);
		Application.targetFrameRate=60;
		
		UpdateScreenMult();
		GetComponent<SCR_input>().StartUp();
		
		//ResetData();
		//SaveAll();
		
		if(PlayerPrefs.HasKey("fxOn")){
			LoadData();
		}	else	{
			ResetData();
			SaveAll();
		}
		

#if CONTROL_FREAK_INSTALLED

		// Initialize Control Freak Controller...

		this.cfCtrl = this.gameObject.GetComponentInChildren<TouchController>();
		if (this.cfCtrl != null)
			{
			this.cfCtrl.gameObject.SetActive(false);

			DontDestroyOnLoad(this.cfCtrl.gameObject);

			if (!this.cfCtrl.enabled)
				this.cfCtrl.enabled = true;

			this.cfCtrl.InitController();
			this.cfCtrl.HideController(0);
			
			if (TouchController.IsSupported()
#if UNITY_EDITOR
				|| this.forceMobileController  	// Forced mobile controller is allowed only inside Editor
#endif
				)
				{
				CFInput.ctrl = this.cfCtrl;
				}
			else
				{
				this.cfCtrl = null;		// Ignore CF controller.
				}
			}
#endif

		LoadScene();
	}
	
	void LoadScene(){
		SCR_input.rightClickAction=-1;
		Time.timeScale=1f;
		Application.LoadLevel(gotoScene);
	}
	

	void Update () {
		if(Screen.width!=screenSize[0]||Screen.height!=screenSize[1]){
			UpdateScreenMult();
		}
		
		if(screenFade){
			if(screenFade.fadeFinished){
				Destroy(screenFade); 
				LoadScene ();
			}
		}
		
		if(fadeReady){
			fadeReady=false;
			FadeOut();
		}
	}
	
	
	public void FadeOut(){
		if(hMusic){
			music.FadeOut(0);	
		}
		screenFade=SCR_gui.CreateScreenCover(1);
	}
	
	
	void UpdateScreenMult(){
		screenSize=new int[2]{Screen.width,Screen.height};
		
		screenMult=((float)screenSize[0]/(float)screenSize[1]);
		resMult=((float)screenSize[1]*0.001f);
		
		aspectShrink=((float)screenSize[1]/(float)screenSize[0]);
		
		GameObject[] icons = GameObject.FindGameObjectsWithTag("Icon");
        
		foreach (GameObject icon in icons){
			if(icon.GetComponent<SCR_icon>()){
				icon.GetComponent<SCR_icon>().SetScale();
			}
			if(icon.GetComponent<SCR_healthBar>()){
				icon.GetComponent<SCR_healthBar>().SetScale();
			}
		}
		
		GameObject[] text = GameObject.FindGameObjectsWithTag("Text");
		
		foreach (GameObject t in text){
			t.GetComponent<SCR_text>().SetScale();
		}
		
		GameObject[] options = GameObject.FindGameObjectsWithTag("Option");
		
		foreach (GameObject option in options){
			option.GetComponent<SCR_option>().SetScale();
		}
		
		if(GameObject.FindWithTag("MainCamera")){
			GameObject cam=GameObject.FindWithTag ("MainCamera");
			if(cam.GetComponent<SCR_camStage>()){
				cam.GetComponent<SCR_camStage>().UpdateEdgeLimit();	
			}
		}
	}
	
	public static float GetAngle(Vector2 startPos, Vector2 targetPos){
		float angle=(Mathf.Atan2((targetPos.y-startPos.y),(targetPos.x-startPos.x)))*(180f/Mathf.PI);
		return angle;
	}
	
	public static SCR_sound CreateSound(Transform parentTrans,AudioClip fxClip,bool randomPitch,bool distanceOn){
		GameObject soundInst=Instantiate(soundObj,parentTrans.transform.position,parentTrans.transform.rotation) as GameObject;
		SCR_sound sound=soundInst.GetComponent<SCR_sound>();
		sound.StartUp(fxClip,randomPitch,distanceOn);
		
		Transform soundTrans=soundInst.transform;
		soundTrans.parent=parentTrans;
		
		return sound;
	}
	
	public static float GetSoundDistanceMultiplier(Vector3 soundOrigin){
		float mult=1f;
		
		float centreDist=Vector2.Distance(soundCentre,new Vector2(soundOrigin.x,soundOrigin.z));
		
		mult=((centreDist*-0.075f)+1.5f);
		mult=Mathf.Clamp(mult,0f,1f);
		
		
		return mult;
	}
	
	public static SCR_sound[] SetupSoundArray(Transform parentTrans,AudioClip[] fxClips,bool randomPitch,bool distanceOn){
		SCR_sound[] SND=null;
		
		if(fxClips!=null){
			SND=new SCR_sound[fxClips.Length];
			for(int i=0; i<fxClips.Length; i++){
				SND[i]=SCR_main.CreateSound(parentTrans,fxClips[i],randomPitch,distanceOn);	
			}
		}
		
		return SND;
	}
	
	public static void PlayRandomSound(SCR_sound[] sound){
		if(sound!=null){
			int randomSound=Random.Range(0,sound.Length);
			sound[randomSound].PlaySound();	
		}
	}
	//////////////////
	//SAVE / LOAD
	//////////////////
	
	public void AlterSetting(int com){
		
		if(com==0){
			fxOn++;
			
			if(fxOn==2){
				fxOn=0;	
			}
		}
		
		if(com==1){
			musOn++;
			
			if(musOn==2){
				musOn=0;	
			}
			
			if(musOn==1){
				music.StartTrack();
			}
			if(musOn==0){
				music.FadeOut(0);	
			}
		}
		
		SaveData(0);
	}
	
	public static void SaveData(int com){
		if(com==0){
			PlayerPrefs.SetInt ("fxOn",fxOn);
			PlayerPrefs.SetInt ("musOn",musOn);
		}
		
		if(com==1){
			for(int i=0; i<highScore.Count; i++){
				string highScoreStr="highScore_"+i.ToString();
				PlayerPrefs.SetInt(highScoreStr, highScore[i]);
			}
		}
		
		PlayerPrefs.Save();
	}
	
	public static void LoadData(){
		
		for(int i=0; i<highScore.Count; i++){
			string highScoreStr="highScore_"+i.ToString();
			if(PlayerPrefs.HasKey(highScoreStr)){
				highScore[i]=PlayerPrefs.GetInt(highScoreStr);
			}
		}

		fxOn=PlayerPrefs.GetInt("fxOn");
		musOn=PlayerPrefs.GetInt("musOn");
	}
	
	public static void SaveAll(){
		for(int i=0; i<2; i++){
			SaveData(i);
		}
	}
	
	public static void ResetData(){
		highScore=new List<int>(){0,0,0,0,0,0,0,0,0,0};
	}
	
	public static void SetNewScore(){
		if(hsOn){
			bool scoreSet=false;
			
			for(int i=0; i<highScore.Count; i++){
				if(scoreSet==false){	
					if(score>highScore[i]){
						highScore.Insert(i,score);
						highScore.RemoveAt(highScore.Count-1);
						scoreSet=true;
					}
				}
			}
			
			if(scoreSet){
				SaveData (1);	
			}
		}
	}
	
	////////////////////////
	//OPTION EVENTS
	////////////////////////
	
	public static void OptionAction(int com){
		
		//TITLE MENU OPTIONS
		
		if(com==0){
			//Go to Title Screen
			GameObject.FindWithTag ("GameController").GetComponent<SCR_menu>().GotoMenu(0);
		}
		
		if(com==1){
			//Start Game
			GameObject.FindWithTag ("GameController").GetComponent<SCR_menu>().FinishMenu();	
			SCR_main.score=0;
			SCR_main.level=0;
			
			SCR_main.gotoScene=SCR_main.sName[0];
			SCR_main.fadeReady=true;
		}
		
		if(com==2){
			//Go to Options Screen
			GameObject.FindWithTag ("GameController").GetComponent<SCR_menu>().GotoMenu(1);
		}
		
		if(com==3){
			//Go to Wipe Data Screen
			GameObject.FindWithTag ("GameController").GetComponent<SCR_menu>().GotoMenu(2);
		}
		
		if(com==4){
			//Wipe Data CONFIRM
			GameObject.FindWithTag ("GameController").GetComponent<SCR_menu>().FinishMenu();
			SCR_main.ResetData();
			SCR_main.gotoScene="menu";
			SCR_main.fadeReady=true;
		}
		
		if(com==5){
			//Go to High Scores Screen
			GameObject.FindWithTag ("GameController").GetComponent<SCR_menu>().GotoMenu(3);
		}
		
		if(com==6){
			//Exit Game
			Application.Quit();
		}
		
		
		//STAGE OPTIONS
		
		if(com==10){
			//Continue to the next stage
			SCR_main.level++;
			if(SCR_main.level<SCR_main.sName.Length){
				SCR_main.gotoScene=SCR_main.sName[SCR_main.level];	
			}	else {
				SCR_main.gotoScene="game complete";
			}
			SCR_main.fadeReady=true;
		}
		
		if(com==11){
			//Resume from being paused
			GameObject.FindWithTag("GameController").GetComponent<SCR_stage>().PauseEvent(false);
		}
		
		if(com==12){
			//Return to menu scene
			SetNewScore();
			SCR_main.gotoScene="menu";
			SCR_main.fadeReady=true;
		}
		
		if(com==13){
			//Retry stage;
			SCR_main.score=SCR_main.scoreRec;
			SCR_main.fadeReady=true;
		}
	}
	
	
	public static void DestroyObj(GameObject objToDestroy){
		Destroy (objToDestroy);
	}

	


	// Return true if the game is running on a mobile...

	public bool RunningOnMobile()
		{
#if CONTROL_FREAK_INSTALLED
		if (this.cfCtrl != null)
			return true;
#endif

		return false;
		}


	// Activate gameplay controller...

	public void ActivateGameplayController()
		{
#if CONTROL_FREAK_INSTALLED
		if (this.cfCtrl == null)
			return;

		this.cfCtrl.gameObject.SetActive(true);

		CFInput.ctrl = this.cfCtrl;

		this.cfCtrl.ResetController();
		this.cfCtrl.HideController(0);
		this.cfCtrl.EnableController();
		this.cfCtrl.ShowController(1);

#endif

		}
	


	// Hide or show gameplay controller...

	public void ShowGameplayController(bool showIt)
		{
#if CONTROL_FREAK_INSTALLED
		if (this.cfCtrl == null)
			return;
		
		if (showIt)
			{
			this.cfCtrl.ResetController();
			this.cfCtrl.ShowController(1.0f);
			}
		else
			{
			this.cfCtrl.HideController(1.0f);
			}
#endif
		}
	

	// Disactivate gameplay controller...

	public void DisactivateGameplayController()
		{
#if CONTROL_FREAK_INSTALLED
		if (this.cfCtrl == null)
			return;

		this.cfCtrl.gameObject.SetActive(false);

#endif

		}
	


	/// Switch Control Freak controller's mode between GUN/MELEE...

	public void SetControllerMode(bool meleeMode, bool skipAnim)
		{
#if CONTROL_FREAK_INSTALLED
		if (this.cfCtrl == null)
			return;
		
		// Show or hide affected controls...

		int ctrlCount = this.cfCtrl.GetControlCount();
		for (int ci = 0; ci < ctrlCount; ++ci)
			{
			TouchableControl c = this.cfCtrl.GetControl(ci);
			
			bool isMeleeControl = false;

			if (c.name.IndexOf("Melee") >= 0)
				isMeleeControl = true;
			else if (c.name.IndexOf("Gun") >= 0)
				isMeleeControl = false;
			else
				continue;

			if (isMeleeControl == meleeMode)
				{
				c.Enable();
				c.Show(skipAnim);
				}
			else
				{
				c.Disable();
				c.Hide(skipAnim);
				}
			}

#endif
		}


}
