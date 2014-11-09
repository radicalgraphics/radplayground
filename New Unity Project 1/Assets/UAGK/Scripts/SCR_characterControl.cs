
using UnityEngine;
using System.Collections;


public class SCR_characterControl : MonoBehaviour {
	
	public bool standAttack;
	public float invulnerableTime;
	//the amount of time the player is invulnerable after being hit.
	
	float invulnerableCounter=0f;
	
	SCR_character character;
	SCR_characterHealth characterHealth;
	SCR_reticle reticle;
	
	[HideInInspector]
	public bool controlActive;
	
	//CLICK
	bool rightClickHeld;
	int clickCurrent=0;
	float doubleClickTimer=0f;
	float doubleClickLimit=0.15f;
	
	
	// [DGT] New local aim mode...

	private bool localAimMode;		///< When true the gun will fire in the direction specified by localAimAngle
									///< When false, gun will point to mouse world position.
	private float	localAimAngle;	
	private float	localAimTargetAngle;	
	public float	localAimSmoothing = 0.1f;	///< Smoothing time for local aim angle
	private float	localAimVel 	= 0;

	private Vector3 mouseAimPoint;	///< World-space target point.	
	
	const float JOY_DEAD_ZONE 	= 0.3f;
	const float JOY_MAX_ZONE 	= 0.9f;

	public float minTurnSpeed 	= 0.0f;		///< Turn speed when the stick is tilted just above the dead zone
	public float maxTurnSpeed 	= 500.0f;	///< Turn speed when the stick is tilted above max zone
	
	private SCR_main	main;			// [DGT] Reference to the main script


	public void StartUp () {
		character=GetComponent<SCR_character>();
		characterHealth=GetComponent<SCR_characterHealth>();
		

		GetComponent<SCR_characterHealth>().StartUp(1000);

		this.localAimMode = true;	// [DGT]
		this.localAimAngle = 0;
		this.localAimTargetAngle = 0;
		this.localAimVel = 0;

		this.main = GameObject.Find("MAIN").GetComponent<SCR_main>();	// [DGT]

		if(this.main.reticleOn){
			GameObject reticleInst=Instantiate(Resources.Load ("Objects/Gui/OBJ_reticle",typeof(GameObject)),Vector3.zero,Quaternion.identity) as GameObject;
			reticle=reticleInst.GetComponent<SCR_reticle>();
		}
	}
	
	
	void Update () {

		if(SCR_input.playerControlActive&&SCR_stage.paused==false){
			UpdateControl();
		}
		
		if(invulnerableCounter>0f){
			invulnerableCounter=Mathf.MoveTowards(invulnerableCounter,0f,Time.deltaTime);
			
			if(invulnerableCounter==0f){
				character.StopFlash();
			}
		}
	}



	void UpdateControl(){
		bool canMove=true;
		bool dirPressed=false;
		//bool planeHit=false;		// [DGT]
		//Vector3 destination=Vector3.zero;
		
// [DGT] Start

		const string
			ROLL_BUTTON		= "Jump",		  
			ATTACK_BUTTON	= "Fire",
			R_ANALOG_X 		= "R-Analog X",	
			R_ANALOG_Y 		= "R-Analog Y";
		

		bool 	inputRollKeyOn		= false;
		bool 	inputRollKeyDown	= false;
		bool 	inputAttackKeyDown	= false;
		
		//bool 	inputAimAtPoint		= false;
		//Vector2	inputAimTargetPoint	= Vector2.zero;
		
		//bool	inputAimAtAngle		= false;
		//float	inputAimTargetAngle	= 0;
		


		// Handle mouse input only if the game is running on a desktop 
 
		if (!this.main.RunningOnMobile() && 
			(SystemInfo.deviceType == DeviceType.Desktop))
			{
			// Check mouse buttons...

			if (Input.GetMouseButton(1))
				inputRollKeyOn = true;
			if (Input.GetMouseButtonDown(1))
				inputRollKeyDown = true;

			if (Input.GetMouseButtonDown(0))
				inputAttackKeyDown = true;
			
			// Use mouse aiming only when mouse cursor moved to not interfere with joystick input.

			const float MOUSE_DELTA_THRESH = 0.00001f;
 
			if (!this.localAimMode ||
				(Mathf.Abs(Input.GetAxis("Mouse X")) > MOUSE_DELTA_THRESH) ||
				(Mathf.Abs(Input.GetAxis("Mouse Y")) > MOUSE_DELTA_THRESH)) 
				{
				// If mouse has been moved, enter world-space aiming mode...

				this.localAimMode = false;
				
				// Project point onto the floor plane...
				
				Plane	plane	= new Plane(Vector3.up, Vector3.zero);
				float	hitDist	= 0f;
				Ray		ray;
				
				ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				
				if (plane.Raycast(ray, out hitDist))
					{
					this.mouseAimPoint = /* destination = */ ray.GetPoint(hitDist);

					//inputAimAtPoint 	= true;
					////inputAimTargetPoint = new Vector2(destination.x, destination.z);

					//this.localAimMode = false;
					}
				}
			}


		// Handle joystick input...

#if CONTROL_FREAK_INSTALLED
		if (CFInput.GetButton(ROLL_BUTTON))
			inputRollKeyOn = true;
		if (CFInput.GetButtonDown(ROLL_BUTTON))
			inputRollKeyDown = true;

		//if (CFInput.GetButtonDown(ATTACK_BUTTON))
		//	inputAttackKeyDown = true;
		

		// If we are using touch-screen and the character is holding a gun...

		if (!character.melee && this.main.RunningOnMobile())
			{
			if (CFInput.GetButton(ATTACK_BUTTON))	// Auto fire on mobile
				inputAttackKeyDown = true;
			}
		else
			{
			if (CFInput.GetButtonDown(ATTACK_BUTTON))
				inputAttackKeyDown = true;
			}


#else
		if (Input.GetButton(ROLL_BUTTON))
			inputRollKeyOn = true;
		if (Input.GetButtonDown(ROLL_BUTTON))
			inputRollKeyDown = true;

		if (Input.GetButtonDown(ATTACK_BUTTON))
			inputAttackKeyDown = true;
#endif		

		// Handle right analog stick...

		//const float aimStickThresh = 0.1f;

		float aimStickTilt = 0;
		float aimStickAngle = GetJoyAngle(
#if CONTROL_FREAK_INSTALLED
			CFInput.GetAxis(R_ANALOG_X), CFInput.GetAxis(R_ANALOG_Y),
#else
			Input.GetAxis(R_ANALOG_X), Input.GetAxis(R_ANALOG_Y),
#endif
			out aimStickTilt);
		
		
	
	
		if (aimStickTilt > JOY_DEAD_ZONE)
			{
			this.localAimMode 			= true;
			//this.localAimAngle 	= aimStickAngle;
			
			float turnSpeed = Mathf.Lerp(this.minTurnSpeed, this.maxTurnSpeed, 
				Mathf.Clamp01((aimStickTilt - JOY_DEAD_ZONE) / (JOY_MAX_ZONE - JOY_DEAD_ZONE)));

			this.localAimTargetAngle = Mathf.MoveTowardsAngle(
				this.localAimTargetAngle, aimStickAngle, turnSpeed * Time.deltaTime);


			//inputAimAtAngle 	= true;
			//inputAimTargetAngle	= aimStickAngle; 

			//destination = this.transform.position + 
			//	((Quaternion.Euler(0, inputAimTargetAngle, 0) * Vector3.forward) * JOY_AIM_RAD);
			}


// [DGT] End


		if(character.attacking){
			//prevent movement if the character is performing a standing attack.
			if(character.melee){
				if(character.characterMelee.attackAnimSlot==5||character.characterMelee.attackAnimSlot==6){
					canMove=false;
				}
			}	else {
				if(character.characterRanged.attackAnimSlot==5||character.characterRanged.attackAnimSlot==6){
					canMove=false;
				}
			}
		}

		if(standAttack==true&&character.attacking){
			//prevent movement if the character is performing a standing attack.
			if(character.melee){
				if(character.characterMelee.attackAnimSlot==7||character.characterMelee.attackAnimSlot==8){
					canMove=false;
				    character.speed[1]=Vector3.zero;
				    character.running=false;
				}
			}	else {
				if(character.characterRanged.attackAnimSlot==7||character.characterRanged.attackAnimSlot==8){
					canMove=false;
				    character.speed[1]=Vector3.zero;
				    character.running=false;
				}
			}
		}
		
		//Click or Double Click
		int newClick=0;
		
		if(SCR_input.cType==0){
			if (inputRollKeyOn) {  //if(Input.GetMouseButton(1)){		// [DGT]
				if(rightClickHeld==false){
					rightClickHeld=true;
					
					if(clickCurrent==0){
						clickCurrent=1;
						newClick=1;
					}	else {
						if(clickCurrent==1&&doubleClickTimer>0f){
							clickCurrent=0;
							doubleClickTimer=0f;
							newClick=2;
						}
					}
				}
			}	else {
				if(rightClickHeld){
					rightClickHeld=false;
					
					if(clickCurrent==1){
						doubleClickTimer=doubleClickLimit;
					}
				}
			}
			
			if(doubleClickTimer>0f){
				doubleClickTimer=Mathf.MoveTowards(doubleClickTimer,0f,Time.deltaTime);
				
				if(doubleClickTimer==0f){
					clickCurrent=0;	
				}
			}
		}	else {
			if(SCR_input.cType==1){
				if (inputRollKeyDown) {		//if(Input.GetMouseButtonDown(1)){	// [DGT]
					newClick=2;	
				}
			}
		}
		
	//	[DGT] Cut this out...
 /*	

		Plane plane=new Plane(Vector3.up,Vector3.zero);
		float hitDist=0f;
		Ray ray;
		
		ray=Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if (plane.Raycast(ray,out hitDist)) {
			destination=ray.GetPoint(hitDist);
			planeHit=true;
			
			if(reticle&&Time.timeScale==1f){
				reticle.SetPos(new Vector2(destination.x,destination.z));	
			}
		}

	// [DGT] End of cut.
*/		
	// [DGT] Alt implementation Start.

		if (this.localAimMode)
			{
			if (this.localAimSmoothing < 0.001f)
				this.localAimAngle = this.localAimTargetAngle;
			else
				this.localAimAngle = Mathf.SmoothDampAngle(this.localAimAngle, this.localAimTargetAngle,
					ref this.localAimVel, this.localAimSmoothing);
			}	

	// [DGT] Alt implementation End.


		
		if(SCR_input.cType==0){
			/*
			if (planeHit) {		// [DGT] cut
				character.SetRotAngle(destination);
			}
			*/

			if (this.localAimMode)
				this.character.SetRotAngleEx(this.localAimAngle);
			else
				this.character.SetRotAngle(this.mouseAimPoint);
				
			if (inputRollKeyOn)	{	// if(Input.GetMouseButton(1)){		// [DGT]
				dirPressed=true;
			}
			
		}	else {
			
#if CONTROL_FREAK_INSTALLED
			float inputX=CFInput.GetAxis("Horizontal");
			float inputY=CFInput.GetAxis("Vertical");
#else
			float inputX=Input.GetAxis("Horizontal");
			float inputY=Input.GetAxis("Vertical");
#endif			
			if(inputX!=0f||inputY!=0f){
				dirPressed=true;
				
				Vector3 inputDir=new Vector3(inputX,0f,inputY).normalized;
				character.SetRotAngle(transform.position+inputDir);
				
			}
			
		}
		

		// [DGT] Calculate gun target pos and reticle pos
		
		Vector3 reticlePos;
		Vector3 attackPos;

		if (this.localAimMode)	
			{
			Vector3 charaPos = this.character.transform.position;
			Vector3 localDir = Quaternion.Euler(0, this.localAimAngle, 0) * Vector3.forward;
			
			const float JOY_ATTACK_POS_DIST = 100.0f;
			const float JOY_RETICLE_DIST = 2.0f;

			attackPos = charaPos + (localDir * JOY_ATTACK_POS_DIST);
			reticlePos = charaPos + (localDir * JOY_RETICLE_DIST);
			}
		else
			{
			attackPos = this.mouseAimPoint;
			reticlePos = this.mouseAimPoint;
			}

		

		if(character.rolling==0&&character.stunned==0){
			//Move to destination
			if(dirPressed){
				if(canMove){
					character.speed[1]=(character.dir*character.runSpeed);
				
					character.running=true;
					
					if(character.attacking==false){
						character.PlayAnim(1);
					}
				}
				bool canRotate=true;
				
				if(character.attacking){
					canRotate=false;	
				}
				
				if(character.melee){
					if(character.characterMelee.lockActive){
						canRotate=false;
					}
				}
				
				if(canRotate){
					character.SetRotTarget();
				}
				
			}	else {
				
				character.speed[1]=Vector3.zero;
				character.running=false;
				
				if(character.attacking==false){
					character.PlayAnim(0);
				}	else {
					
					//if character is performing a running attack and stops moving, the attack is cancelled.
					if(character.melee){
						if(character.characterMelee.attackAnimSlot==7||character.characterMelee.attackAnimSlot==8){
							character.PlayAnim(0);
							character.AttackCancel();
						}
					}	else {
						if(character.characterRanged.attackAnimSlot==7||character.characterRanged.attackAnimSlot==8){
							character.PlayAnim(0);
							character.AttackCancel();
						}
					}
				}
			}
			

			
	
			//Attack
			
			if (inputAttackKeyDown)	{	// if(Input.GetMouseButtonDown (0)){	// [DGT]
				
				if(character.rolling==0&&character.stunned==0){
					character.SetRotAngle(attackPos); // [DGT] // destination);
					character.SetRotTarget();
				}
				
				character.attackPoint= attackPos;  // [DGT] // character.attackPoint= destination;
				character.AttackStart();
			}
		}
			
		if(newClick==2){
			//Roll
			character.RollStart();
		}

// [DGT] Start...

	if ((this.reticle != null))
		this.reticle.SetPos(new Vector2(reticlePos.x, reticlePos.z)); // [DGT] // new Vector2(destination.x, destination.z));

// [DGT] End.

	}
	
	public bool StartInvulnerability(){
		bool invulnerableSuccess=false;
		
		if(invulnerableTime>0f){
			invulnerableCounter=invulnerableTime;
			character.StartFlash(1,10000);
			invulnerableSuccess=true;
		}
		
		return invulnerableSuccess;
	}
	
	void OnTriggerEnter(Collider col){
		if(character.stunned<2){
			if(col.gameObject.tag=="Collectable"){
				SCR_collectable c=col.gameObject.GetComponent<SCR_collectable>();
				
				if(c.isActive){
					c.Collect();
					
					if(c.healthRecovery>0){
						characterHealth.AddHealth(c.healthRecovery);
					}
					
					if(c.powerBoost>0){
						character.PowerBoostStart(c.powerBoost,c.powerBoostDuration);	
					}
				}
			}
		}
	}
	
	void OnTriggerStay(Collider por){
		if(character.stunned<2){
			if(por.gameObject.tag=="Portal"){
				SCR_portal p=por.gameObject.GetComponent<SCR_portal>();
				
				if(p.isActive){
					p.Collect();
				}
			}
		}
	}	
	
	public void ReticleOn(bool com){
		if(reticle){
			reticle.Appear(com);	
		}
	}


// [DGT] New function.
	
	// -------------------------
	/// Calculate joystick's angle and tilt...
	// -------------------------
	static private float GetJoyAngle(float x, float y, out float tilt)
		{
		Vector2 v = new Vector2(x, y);
		float d = v.magnitude;
		if (d < 0.0001f)
			{
			tilt = 0;
			return 0;
			}

		v /= d;

		tilt = Mathf.Min(d, 1.0f);

		return (Mathf.Rad2Deg * Mathf.Atan2(v.x, v.y));
		}

// [DGT] End

}
