using UnityEngine;
using System.Collections;

public class SCR_stage : MonoBehaviour {

    public bool stageTimer; 
	// set stage timer on or off in the Inspector 
	
	private bool startTimer; 
	// trigger to start timer after stage intro countdown
	
    public float timer = 60; 
	// set duration time in seconds in the Inspector  

	public GameObject stageCamera;
	//the camera used for this stage.
	
	public AudioClip stageMusic;
	//the music that plays during this stage.
	
	public Material skyBoxMaterial;
	//fill this in if you want to use a skybox for the stage.
	
	public static int[] sX;
	public static int[] sZ;
	
	SCR_camStage cam;
	SCR_character player;
	SCR_main main;
	SCR_music music;
	GUIText guitimer;
	
	static int state=0;
	static float stateCounter=0f;
	int scoreRec=0;
	
	SCR_text scoreObj;
	
	static bool pauseAllowed=false;
	public static bool paused=false;
	float pauseCD=0f;
	SCR_screenCover pauseCover;
	
	Vector2 optionPos;
	Vector2 stageFinishOptionPos=new Vector2(0.5f,0.35f);

	private static SCR_stage curStage;		// [DGT] last loaded stage reference
 	


	void Awake () {
		SCR_stage.curStage = this;		// [DGT]


		GameObject m=GameObject.Find ("MAIN");
		main=m.GetComponent<SCR_main>();
		music=m.GetComponent<SCR_music>();
		
		if(stageMusic&&SCR_main.hMusic){
			music.PlayMusic(stageMusic);
		}
		
		SCR_input.playerControlActive=false;
		
		pauseAllowed=false;
		paused=false;
		SCR_main.scoreRec=SCR_main.score;
		scoreRec=SCR_main.score;
		
		GameObject camInst=Instantiate(stageCamera,Vector3.zero,Quaternion.identity) as GameObject;
	
		GameObject spawnPointPlayer=GameObject.Find ("SpawnPointPlayer");
		Vector3 playerPos=new Vector3(spawnPointPlayer.transform.position.x,0f,spawnPointPlayer.transform.position.z);
		
		if(main.playerSelect==true){
			GameObject playerInst=Instantiate(main.playerObj1,playerPos,Quaternion.identity) as GameObject;		
			Destroy(spawnPointPlayer);
			player=playerInst.GetComponent<SCR_character>();
			
			SetupStageSize();
			
			cam=camInst.GetComponent<SCR_camStage>();
			cam.SetTarget(playerInst.transform);
			cam.InitiateStage(0);		
     	}
		else if(main.playerSelect==false){
			GameObject playerInst=Instantiate(main.playerObj2,playerPos,Quaternion.identity) as GameObject;
			Destroy(spawnPointPlayer);
			player=playerInst.GetComponent<SCR_character>();
			
			SetupStageSize();
			
			cam=camInst.GetComponent<SCR_camStage>();
			cam.SetTarget(playerInst.transform);
			cam.InitiateStage(0);		
		}
		
		
		if(skyBoxMaterial){
			RenderSettings.skybox=skyBoxMaterial;
		}
		
		if(SCR_main.hCursor){
			Screen.showCursor=false;
		}
		
		SetState (10);
		
		SCR_gui.CreateScreenCover(0);


	}
	

	void Start()
		{
		// [DGT] Init controller...
		
		if (this.main.RunningOnMobile())
			//TouchController.IsSupported() && (this.main.cfCtrl != null))
			//if (CFInput.ControllerActive())
			{
			this.main.ActivateGameplayController();

			this.main.SetControllerMode(player.melee, true);
			}
		}
	

	// ------------------
	void OnDestroy()
		{
		// [DGT] Completly hide the controller on level unload...

		this.main.DisactivateGameplayController();
		}


	void SetupStageSize(){
		GameObject[] corners = GameObject.FindGameObjectsWithTag("StageCorner");
		
		float[] xx=new float[2];
		float[] zz=new float[2];
		
		foreach (GameObject c in corners)  {
			if(c.transform.position.x<xx[0]){
				xx[0]=c.transform.position.x;
			}
			if(c.transform.position.x>xx[1]){
				xx[1]=c.transform.position.x;
			}
			if(c.transform.position.z<zz[0]){
				zz[0]=c.transform.position.z;
			}
			if(c.transform.position.z>zz[1]){
				zz[1]=c.transform.position.z;
			}
			
			Destroy(c);
		}
		
		sX=new int[2]{(int)xx[0],(int)xx[1]};
		sZ=new int[2]{(int)zz[0],(int)zz[1]};
	}
	

	void Update () {
		
		if(pauseCD==0f){
			if(pauseAllowed){

#if CONTROL_FREAK_INSTALLED
				if(CFInput.GetButtonDown("Pause") || Input.GetKeyDown(KeyCode.Escape)){
#else
				if(Input.GetButtonDown("Pause")){
#endif
					if(paused==false){
						PauseEvent(true);	
					}	else {
						PauseEvent(false);
					}
				}
			}
		}	else {
			pauseCD=Mathf.MoveTowards(pauseCD,0f,SCR_main.counterMult);
		}
		
		if(state>0){
			UpdateState();	
		}
		
		if(SCR_main.score!=scoreRec){
			DisplayScore();
		}
	}
	
	void UpdateState(){
		stateCounter+=Time.deltaTime;
		
		//INTRO
		if(state==10){
			if(stateCounter>=0.5f){
				CreateCountDown(0);
				StateNext ();
			}
		}
		if(state==11){
			if(stateCounter>=1f){
				CreateCountDown(1);
				StateNext ();
			}
		}
		if(state==12){
			if(stateCounter>=1f){
				cam.InitiateStage(1);
				CreateCountDown(2);
				StateNext ();
			}
		}
		if(state==13){
			if(stateCounter>=1f){
				CreateCountDown(3);
				pauseAllowed=true;
				SCR_input.playerControlActive=true;
				SCR_enemySpawner.spawnOn=true;
					
				// [DGT] Hide the reticle if the player is using melee weapon on mobile  

				player.GetComponent<SCR_characterControl>().ReticleOn(
					(player.melee && this.main.RunningOnMobile()) ? false : true);
					//true);


				StateNext ();
			}
		}
		if(state==14){
			if(stateCounter>=1f){
				ShowHUD(true);
				SetState (0);
			}
		}
		
		//FAIL
		if(state==20){
			if(stateCounter>=0.6f){
				SCR_gui.CreateIcon("GameOver",Vector3.zero);
				
				if(SCR_main.hMusic){
					music.FadeOut(1);	
				}
				
				StateNext();
			}
		}
		
		if(state==21){
			if(stateCounter>=0.3f){
				if(SCR_main.hCursor){
					Screen.showCursor=true;
				}
				
				optionPos=stageFinishOptionPos;
				SCR_gui.CreateOption("StageFailRetry",optionPos);
				optionPos+=SCR_gui.optionSpacing;
				SCR_gui.CreateOption("StageFailExit",optionPos);
				SetState (0);
			}
		}
		
		
		//SUCCESS
		if(state==30){
			if(stateCounter>=1f){
				SCR_input.playerControlActive=false;
				player.GetComponent<SCR_characterControl>().ReticleOn(false);
				
				player.AttackCancel();
				player.speed[1]=Vector3.zero;
				player.PlayAnim(0);
				
				if(SCR_main.hMusic){
					music.FadeOut(1);	
				}
				
				StateNext ();
			}
		}
		
		if(state==31){
			if(stateCounter>=0.3f){
				startTimer=false;
				SCR_gui.CreateIcon("StageComplete",Vector3.zero);
				StateNext ();
			}
		}
		
		if(state==32){
			if(stateCounter>=0.2f){
				
				if(SCR_main.hCursor){
					Screen.showCursor=true;
				}
				
				optionPos=stageFinishOptionPos;
				SCR_gui.CreateOption("StageCompleteContinue",optionPos);
				optionPos+=SCR_gui.optionSpacing;
				SCR_gui.CreateOption("StageCompleteExit",optionPos);
				SetState(0);
			}
		}
	}
	
	static void SetState(int com){
		state=com;
		stateCounter=0f;
	}
	
	void StateNext(){
		state++;
		stateCounter=0f;
	}
	
	
	void ShowHUD(bool com){
		if(com){
			SCR_gui.CreateText("ScoreText",Vector3.zero);
			scoreObj=SCR_gui.CreateText("ScoreNumber",Vector3.zero);
			
			if (stageTimer==true){
				SCR_gui.CreateText("TimerText",Vector3.zero);
				SCR_gui.CreateText("TimerNumber",Vector3.zero);
				startTimer=true;
			    GameObject t=GameObject.Find ("TimerNumber");
		        guitimer=t.GetComponent<GUIText>();	
				StartCoroutine(timerUpdate());
			}
			
			DisplayScore();
		}	else {
			
			SCR_gui.RemoveText("ScoreText");
			SCR_gui.RemoveText("ScoreNumber");
			SCR_gui.CreateText("TimerText",Vector3.zero);
			SCR_gui.CreateText("TimerNumber",Vector3.zero);
			startTimer=false;
		}
	}	
	
	void DisplayScore(){
		scoreRec=SCR_main.score;
		
		if(scoreObj){
			scoreObj.UpdateText(SCR_main.score.ToString());	
		}
	}
	
	void CreateCountDown(int com){
		string iconName="CountDown";
		
		if(com==3){
			iconName="Start";
		}
		
		SCR_icon countDownInst = SCR_gui.CreateIcon(iconName,Vector3.zero);
		
		if(com<3){
			string countDownStr="Textures/Gui/_Icon/Text_";
			Texture[] countDownTexArray=new Texture[3]{
					Resources.Load((countDownStr+"3"),typeof(Texture)) as Texture,
					Resources.Load((countDownStr+"2"),typeof(Texture)) as Texture,
					Resources.Load((countDownStr+"1"),typeof(Texture)) as Texture
			};
			
			countDownInst.GetComponent<GUITexture>().texture=countDownTexArray[com];
		}
	}
	
	IEnumerator timerUpdate () {

	while(true){	
	
		if (stageTimer==true&&startTimer==true){	
			timer -= Time.deltaTime;
			
			if (timer > 0){
				guitimer.text = timer.ToString("F0");
			} else {
				GameObject.FindWithTag ("Player").GetComponent<SCR_character>().Kill();
				SetState (20);
				StageFinish();
				stageTimer=false;
				startTimer=false;
				Debug.Log("timer reached 0!");
			}
			
		}
		
	   yield return null;

	}
	
	}
	
	public static void PlayerDefeated(){
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
		
		if(enemies.Length>0) {
			foreach (GameObject e in enemies)  {
				e.GetComponent<SCR_enemyAI>().PlayerDefeated();
			}
		}
		
		SetState(20);

		if (SCR_stage.curStage != null)
			SCR_stage.curStage.StageFinish();		// [DGT]
	}
	
	public void AllEnemiesDefeated(){
		SetState (30);
		StageFinish();
	}

	
	/// [DGT] Bugfix for enemies attacking the player after he entered a portal.

	public void EnteredPortal()
		{
		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
		
		if(enemies.Length>0) {
			foreach (GameObject e in enemies)  {
				e.GetComponent<SCR_enemyAI>().PlayerDefeated();
			}
		}

		SetState(30);
		StageFinish();
		}
	
	/*static*/		/*  [DGT] Not static any more... */
	 void StageFinish(){
		pauseAllowed=false;


		// [DGT] Hide controller on stage finish...

		this.main.ShowGameplayController(false);


	}
	
	public void PauseEvent(bool com){
		if(com!=paused){
			paused=com;
			pauseCD=1f;
	
					
			this.main.ShowGameplayController(!com);		// [DGT] Disable controller on pause...



			if(paused){
				
				if(SCR_main.hCursor){
					Screen.showCursor=true;
				}
				
				pauseCover=SCR_gui.CreateScreenCover(2);
				SCR_gui.CreateIcon("Paused",Vector3.zero);
				optionPos=new Vector2(0.5f,0.55f);
				
				SCR_gui.CreateOption("PausedResume",optionPos);
				optionPos+=SCR_gui.optionSpacing;
				SCR_gui.CreateOption("ChangeSoundFX",optionPos);
				optionPos+=SCR_gui.optionSpacing;
				
				if(SCR_main.hMusic){
					SCR_gui.CreateOption("ChangeMusic",optionPos);
					optionPos+=SCR_gui.optionSpacing;
				}
				
				SCR_gui.CreateOption("PausedExit",optionPos);
				
				Time.timeScale=0f;
					
			}	else {
				
				if(SCR_main.hCursor){
					Screen.showCursor=false;
				}
				
				if(pauseCover){
					pauseCover.FadeOut();
				}
				SCR_gui.RemoveIcon("Paused");
				SCR_input.OptionCycle(3);
				
				Time.timeScale=1f;
			}
		}
	}
}
