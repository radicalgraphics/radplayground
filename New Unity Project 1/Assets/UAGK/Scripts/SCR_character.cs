using UnityEngine;
using System.Collections;

public class SCR_character : MonoBehaviour {
	
	public float[] animSpeed=new float[9]{1f,1f,1f,1f,1f,1f,1f,1f,1f};
	//this alters the speed of each animation. 1 is the default speed, 2 is double the speed etc.
	
	public float runSpeed;
	//the character's basic movement speed.
	
	public float rollSpeed;
	//the character's rolling speed.
	float rollCounter=0f;
	public float rollSlowTime;
	//this is the amount of time that the character rolls before slowing down and becoming vulnerable again.
	
	public float acceleration;
	//how quickly the character reaches it's target speed.
	
	public AudioClip rollSound;
	//the sound of the character rolling;
	SCR_sound SND_roll;
	
	
	Renderer[] mesh;

	
	[HideInInspector]
	public string[] animArray;
	/*
	On the characters "Animation" component, the animations must be in this order:
	0	idle
	1	run
	2	damaged
	3	death
	4	roll
	5	standing slash 1	/	standing shot single
	6	standing slash 2	/	standing shot rapid fire
	7	running slash 1		/	running shot single
	8	running slash 2		/	running shot rapid fire
	*/
	
	//GENERAL
	[HideInInspector]
	public bool melee;
	[HideInInspector]
	public bool invulnerable;
	
	[HideInInspector]
	public SCR_characterMelee characterMelee;
	[HideInInspector]
	public SCR_characterRanged characterRanged;
	SCR_characterControl cc;
	[HideInInspector]
	public SCR_enemyAI ai;
	
	[HideInInspector]
	public bool isPlayer;
	[HideInInspector]
	public Vector3 dir;
	
	//MOVEMENT
	[HideInInspector]
	public int stunned;
	[HideInInspector]
	public bool running;
	[HideInInspector]
	public int rolling;
	[HideInInspector]
	public bool attacking;
	
	[HideInInspector]
	public Vector3[] speed=new Vector3[2];
	
	
	float rotSpeed=11.5f;
	[HideInInspector]
	public Quaternion rotTarget;
	[HideInInspector]
	public float rotAngle;
	[HideInInspector]
	public float hitBoxSize;
	[HideInInspector]
	public Vector3 attackPoint;
	
	//FLASH
	
	float flashCounter=0f;
	int flashTotal=0;
	int flashType=0;
	bool flashOn=false;
	
	//POWER BOOST
	[HideInInspector]
	SCR_particle powerBoostObj;
	public int powerBoost=0;
	float powerBoostCounter=0f;
	
	//ANIMATION
	
	int anim=0;
	float[] animBlendSpeed;
	
	float[] runSpeedArray;
	float runAnimSpeedPatrol;
	

	void Awake () {
		
		int i=0;
		
		//Mesh Setup
		
		int meshCount=0;
		
		for(i=0; i<2; i++){
			foreach (Renderer m in gameObject.GetComponentsInChildren<Renderer>()){
				if(i==1){
					mesh[meshCount]=m;	
				}
				meshCount++;
			}
			
			if(i==0){
				mesh=new Renderer[meshCount];	
			}
			
			meshCount=0;
		}
		
		//Player or Enemy
		
		if(GetComponent<SCR_characterControl>()){
			gameObject.tag="Player";
			isPlayer=true;
			cc=GetComponent<SCR_characterControl>();
			SetRotAngle(transform.position+(Vector3.forward*-10f));
			SetRotTarget();
		}	else {
			gameObject.tag="Enemy";
			isPlayer=false;
			invulnerable=true;
			ai=GetComponent<SCR_enemyAI>();
			StartFlash(1,4);
			float patrolMult=0.5f;
			runSpeedArray=new float[2]{	runSpeed,
										runSpeed*patrolMult
			};
			runAnimSpeedPatrol=(animSpeed[1]*patrolMult);
		}
		
		
		
		//Melee or Ranged
		
		if(GetComponent<SCR_characterMelee>()){
			characterMelee=GetComponent<SCR_characterMelee>();
			characterMelee.StartUp(isPlayer);
			melee=true;	
		}	else {
			characterRanged=GetComponent<SCR_characterRanged>();
			melee=false;
		}
		
		//Animation Setup
		
		int animCount=0;
		
		for(i=0; i<2; i++){
			foreach (AnimationState clip in animation) {
				if(i==1){
					animArray[animCount]=clip.name;
				}		
				animCount++;
			}
			
			if(i==0){
				animArray=new string[animCount];
				animCount=0;
			}
		}
		
		//Set the cross fade speed for each animation
		animBlendSpeed=new float[9]{		0.15f,
											0.15f,
											0.1f,
											0.15f,
											0.0f,
											0.0f,
											0.0f,
											0.0f,
											0.0f
		};
		
		//Set the speed for each animation
		for(i=0; i<animArray.Length; i++){
			if(animation[animArray[i]]){
				animation[animArray[i]].speed=animSpeed[i];
			}
		}
		
		if(rollSound){
			SND_roll=SCR_main.CreateSound(transform,rollSound,false,true);	
		}
		
		
		if(GetComponent<BoxCollider>()){
			hitBoxSize=(GetComponent<BoxCollider>().size.x*transform.localScale.x*0.5f);
		}
		transform.rotation=rotTarget;
		
		if(isPlayer){
			cc.StartUp();
		}	else {
			ai.StartUp();
		}
		
	}
	
	void Update () {
		UpdateMovement();
		UpdateAnimation();
		
		if(powerBoostCounter>0f){
			powerBoostCounter=Mathf.MoveTowards(powerBoostCounter,0f,Time.deltaTime);
			
			if(powerBoostCounter==0f){
				PowerBoostEnd();
			}
		}
		
		if(flashTotal>0){
			UpdateFlash ();	
		}
		
		if(stunned==1){
			if(!animation.IsPlaying (animArray[2])){
				stunned=0;
				
				if(isPlayer){
					invulnerable=cc.StartInvulnerability();	
				}	else {
					ai.SetAction(0);
				}
			}
		}
	}
	
	void FixedUpdate(){
		UpdateMovementFixed();	
	}
	
	
	void UpdateMovement(){
		transform.rotation=Quaternion.Slerp (transform.rotation,rotTarget,(Time.deltaTime*rotSpeed));
		
		speed[0]=Vector3.MoveTowards(speed[0],speed[1],(Time.deltaTime*acceleration));
	}
	
	void UpdateMovementFixed(){
		//cc.Move(speed[0]*Time.deltaTime);
		rigidbody.MovePosition(new Vector3(rigidbody.transform.position.x,0f,rigidbody.transform.position.z)+(speed[0]*Time.deltaTime));
	}
	
	public void SetRotAngle(Vector3 destination){
		dir=(destination-new Vector3(transform.position.x,0f,transform.position.z));
		float distTotal=dir.magnitude;
		dir/=distTotal;
		
		rotAngle=SCR_main.GetAngle(	new Vector2(transform.position.x,transform.position.z),
									new Vector2(destination.x,destination.z));
		rotAngle=(-rotAngle+90f);
	}
	
// [DGT] - terribly inefficient!!
/*
	public void SetRandomRotAngle(){
		rotAngle=Random.Range(0f,360f);
		SetRotTarget();
		
		GameObject newDir=new GameObject("newDir");
		
		newDir.transform.rotation=rotTarget;
		newDir.transform.position=(transform.position+(newDir.transform.forward*5f));
		
		dir=(newDir.transform.position-new Vector3(transform.position.x,0f,transform.position.z));
		float distTotal=dir.magnitude;
		dir/=distTotal;

		Destroy(newDir);
		}
*/
	

// [DGT] - just recalc direction vector
	
	// -----------------
	/// Set character's rot angle directly.
	// -----------------
	public void SetRotAngleEx(float angle)
		{		
		this.rotAngle	= angle;

		this.SetRotTarget();

		// Simply recalc direction vector.

		this.dir = this.rotTarget * Vector3.forward;
		if (!Mathf.Approximately(this.dir.y, 0))
			{
			this.dir.y = 0;
			this.dir.Normalize();
			}
		}	
	
	
	public void SetRandomRotAngle()
		{
		this.SetRotAngleEx(Random.Range(0f, 360f));
		}
	
// [DGT] - end

		
	
	public void SetRotTarget(){
		rotTarget=Quaternion.Euler(new Vector3(0f,rotAngle,0f));
	}
	
	/////////////////////
	//ANIMATION
	/////////////////////
	
	public void PlayAnim(int com){
		bool canRewind=true;
		
		if(melee==false){
			if(com==8&&anim==8){
				canRewind=false;	
			}
		}
		
		if(canRewind){
			if(animBlendSpeed[com]==0f||com==2||com>=5){
				animation.Rewind(animArray[com]);	
			}
		}
		
		if(animBlendSpeed[com]>0f){
			animation.CrossFade(animArray[com],animBlendSpeed[com]);	
		}	else {
			animation.Play (animArray[com]);
		}
		anim=com;
	}
	
	
	void UpdateAnimation(){
		if(rolling==1){
			rollCounter+=Time.deltaTime;
			
			if(rollCounter>=rollSlowTime){
				rolling=2;
				speed[1]=Vector3.zero;
			}
		}
		
		if(rolling>0&&!animation.IsPlaying(animArray[4])){
			rolling=0;
		}
	}
	
	
	public void AttackStart(){
		if(rolling==0&&stunned==0){
			if(melee){
				characterMelee.AttackStart();
			}	else {
				characterRanged.AttackStart();
			}
		}
	}
	
	public void RollStart(){
		if(rolling==0&&stunned==0){
			
			AttackCancel();
			
			rolling=1;
			rollCounter=0f;
			
			speed[1]=(dir*rollSpeed);
			speed[0]=speed[1];
			
			rotTarget=Quaternion.Euler(new Vector3(0f,rotAngle,0f));
			
			PlayAnim (4);
			
			if(SND_roll){
				SND_roll.PlaySound();	
			}
			
			SetRotTarget();
			
		}
	}
	
	////////////////
	//FLASH
	////////////////
	
	public void StartFlash(int _flashType, int _flashTotal){
		if(flashTotal>0){
			StopFlash();	
		}
		
		flashType=_flashType;
		flashTotal=_flashTotal;
		flashCounter=0f;
		flashOn=true;
		DisplayFlash();
	}
	
	public void StopFlash(){
		flashOn=false;
		flashTotal=0;
		DisplayFlash();
		invulnerable=false;
	}
	
	void DisplayFlash(){
		for(int i=0; i<mesh.Length; i++){
			if(flashType==0){
				if(flashOn){
					//mesh[i].material.SetColor("_Color",new Color(1f,0f,0f,0f));
					//mesh[i].material.SetColor("_Emission",new Color(1f,0f,0f,0f));
					//mesh[i].material.SetFloat("_Shininess",0f);
				}	else {
					//mesh[i].material.SetColor("_Color",new Color(1f,1f,1f,0f));
					//mesh[i].material.SetColor("_Emission",new Color(0f,0f,0f,0f));
					//mesh[i].material.SetFloat("_Shininess",0.7f);
				}
			}
			
			if(flashType==1){
				mesh[i].enabled=!flashOn;	
			}
		}
	}
	
	void UpdateFlash(){
		if(Time.timeScale>0f){
			flashCounter+=SCR_main.counterMult;
			
			float flashLimit=0.2f;
			
			if(flashType==1&&flashOn==false){
				flashLimit=0.4f;	
			}
			
			if(flashCounter>=flashLimit){
				flashCounter=0f;
				flashOn=!flashOn;
				
				DisplayFlash();
				
				if(flashOn==false){
					flashTotal--;
				}
			}
		}
	}
	
	////////////////
	//DAMAGE
	////////////////
	
	public void Hurt(){
		StartFlash(0,2);
		PlayAnim(2);
		stunned=1;
		speed[1]=Vector3.zero;
	}
	
	public void AttackCancel(){
		attacking=false;
		
		if(melee){
			characterMelee.AttackCancel();	
		}	else {
			characterRanged.AttackCancel();
		}
	}
	
	public void SetPatrolSpeed(bool com){
		if(com){
			animation[animArray[1]].speed=runAnimSpeedPatrol;
			runSpeed=runSpeedArray[1];
		}	else {
			animation[animArray[1]].speed=animSpeed[1];
			runSpeed=runSpeedArray[0];
		}
	}
	
	public void PowerBoostStart(int _powerBoost,float powerBoostDuration){
		powerBoost=_powerBoost;
		powerBoostCounter=powerBoostDuration;
		
		SCR_main main = GameObject.Find ("MAIN").GetComponent<SCR_main>();
		if(main.powerBoostEffectObj){
			GameObject powerBoostEffectInst=Instantiate(main.powerBoostEffectObj,transform.position,transform.rotation) as GameObject;
			
			Transform powerBoostEffectTrans=powerBoostEffectInst.transform;
			powerBoostEffectTrans.parent=transform;
			
			powerBoostObj=powerBoostEffectInst.GetComponent<SCR_particle>();
		}
	}
	
	public void PowerBoostEnd(){
		powerBoost=0;
		
		if(powerBoostObj){
			powerBoostObj.Kill();	
		}
	}
	
	public void Kill(){
		StartFlash (0,2);
		PlayAnim(3);
		stunned=2;
		speed[1]=Vector3.zero;
		
		if(powerBoost>0){
			PowerBoostEnd();	
		}
		
		if(isPlayer){
			cc.controlActive=false;
			cc.ReticleOn(false);
			SCR_stage.PlayerDefeated();
		}
		else{
			GameObject.FindWithTag ("GameController").GetComponent<SCR_enemySpawner>().enemyTotal--;		
			ai.Kill ();
			
			if(GameObject.FindWithTag ("GameController").GetComponent<SCR_enemySpawner>().enemyTotal==0&&GameObject.FindWithTag ("GameController").GetComponent<SCR_enemySpawner>().enemyStock==0&&GameObject.FindWithTag ("GameController").GetComponent<SCR_enemySpawner>().enemyBossFight == true){
                GameObject.FindWithTag ("Boss").GetComponent<SCR_enemySpawner>().enabled = true;			
			}
			else if(GameObject.FindWithTag ("GameController").GetComponent<SCR_enemySpawner>().enemyTotal==0&&GameObject.FindWithTag ("GameController").GetComponent<SCR_enemySpawner>().enemyStock==0&&GameObject.FindWithTag ("GameController").GetComponent<SCR_enemySpawner>().enemyBossFight == false){
				SCR_enemySpawner.spawnOn=false;
				GameObject.FindWithTag ("GameController").GetComponent<SCR_stage>().AllEnemiesDefeated();
			}
		}
	}
}