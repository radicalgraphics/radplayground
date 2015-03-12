#pragma strict
var _bullet:Bullet;							//Contains bullet to be fired when shooting
var _clickToPickup:boolean;					//Enable if user can touch/click pickup objects to pick them up
var _shipModel:GameObject;					//Model of the spaceship
var _shield:GameObject;						//Shield model
var rocket:ParticleSystem;					//Rocket particle system
var _explosion:ParticleSystem;				//Explosion particle system when player dies
var _moveTo:GameObject;						//Player always follows this around
var _moveLook:GameObject;					//Rotation of this controls direction of players movement, added to make the ship be able to rotate towards the mouse/touch but move forward based on this object
var _lives:int = 3;							//Players lifes
var _maxLives:int = 3;						//Players maximum allowed lives (Remember to change the GUI prefab lives if this is increased)
var _bulletPoint:GameObject;				//GameObject that controls where the bullets spawn (look in player prefab)
var _bullets:Bullet[];						//List of bullets
var _currentBullet:int;						//Index of current bullet in list
var _audioDeath:AudioClip;					//Audio clip when player dies
var hit : RaycastHit;						//Mouse/touch position
var _speed:float = 0.5;						//Ship movement speed
var _rotateSpeed:float = 3;					//Ship rotation speed
private var _invunerable:boolean;			//Can the player be hurt
public var _inControl:boolean = true;		//User control of the ship
private var _shooting:boolean;				//Ship shooting

private var pos:Vector3;					//Raycast position
static var instance : Player;				// Player is a singleton. Player.instance.DoSomeThing();
instance =  FindObjectOfType(Player);

function OnApplicationQuit() {				// Ensure that the instance is destroyed when the game is stopped in the editor.
    instance = null;
}

function Start () {
	//Setting parent to null is very tough on mobile, if player is instantiated this has to be changed
	_moveTo.transform.parent = null;		//Put follow object in the stage root to avoid it flailing around with the players movement
	_explosion.transform.parent = null;
	_bulletPoint.SetActive(false);			//Disables the bullet spawn point model
	//_shield.active = false;				//Disables the shield model
	Invunerable(5);							//Make player invunerable to damage for 5 seconds when game starts
	yield(WaitForSeconds(.1));
	GameController.instance.CheckPlayer();	//Update player stats (GUI)
}

//Adds bullets to the stage(note: add multiple weapon types? _type if(_type))
function Shoot () {
	if(_inControl){							//Cannot shoot while disabled
		var b:Bullet = Instantiate(_bullet, _bulletPoint.transform.position, transform.rotation);	//Create a bullet on the stage
		_shooting = true;					//Player is shooting
	}
}

function StopShoot () {
	CancelInvoke();							//Stops shooting
}

function OnCollisionEnter (col:Collision) {
	//If player hits a asteroid and is not invunerable/shielded
	if(!_invunerable){
		if (col.gameObject.tag == "Asteroid"){
			col.gameObject.GetComponent(Asteroid)._hitpoints = -1;	//Destroy/break the asteroid
			DestroyShip();	//Disable ship for a while to reset position
		}else if (col.gameObject.tag == "Bullet"){
			DestroyShip();	//Disable ship for a while to reset position
		}
	}
}

//Disables the ship and player controll for a while and removes one life (note: add smooth reset position?)
function DestroyShip () {
	_explosion.transform.position = transform.position;		//Positiones the explosion ontop of player
	_explosion.Play();										//Play explosion particle system
	GameController.instance.SetScoreMultiplier(1);			//Reset the score multiplier when dead
	_lives -=1;												//Remove a life
	if(_audioDeath){										//If there is audio on death
		SoundController.instance.StopAll();					//Stop all other sounds (to make this sound more audiable)
		SoundController.instance.Play(_audioDeath, 2, 1);	//Play the death sound
	}
	_invunerable = true;									//Make ship take no damage for a limited time while resetting
	collider.enabled = false;								//Disable ship collider
	_inControl = false;										//Player can no longer be controlled by user
	//_moveTo.SetActiveRecursively(false);					//Disable the moveto object (GUI)
	GameController.instance.CheckPlayer();					//Update player stats (GUI)
	rocket.enableEmission = false;							//Disable the rocket particle system
	//yield(WaitForSeconds(.1));							//Delay 
	this._shipModel.renderer.enabled = false;
	transform.position = Vector3.zero;						//Set position to absolute zero
	//GameController.instance.DestroyAll("Pickup");
	_moveTo.transform.position = transform.position;		//Reset the moveTo gameObject
	if(this._lives >= 0){									//Check if player has any lives left
		yield(WaitForSeconds(1));							//Delay
		this._shipModel.renderer.enabled = true;
		Invunerable(2);										//Ship will not take damage for 2 seconds
		_inControl = true;									//Player is now controllable again
		rocket.enableEmission = true;						//Enable rockets
		ResetWeapon();										//Set to starter weapon(bullet)
		StopShoot();										//Stops shooting
	}else{
		GameController.instance.GameOver(false);			//GameOver if lives are less than zero (false indicates that the level was not completed)
	}
}

function Invunerable (sec:float) {	
	_shield.SetActive(true);									//Show shield model
	yield(WaitForSeconds(sec));								//Delay
		_shield.SetActive(false);								//Disable shield
		_invunerable = false;								//Player is no longer invunerable
		collider.enabled = true;							//Enable collider
}

//Make the ship invunerable for a duration (Note: in progress)
function invunerable(){
	_invunerable = true;
	yield(WaitForSeconds(1));
	_invunerable = false;
}

//Upgrade weapon by picking the next Bullet in the _bullets list
function UpgradeWeapon () {
	if(_currentBullet < _bullets.Length-1){
		StopShoot();													//Stops shooting so that the Shoot function get updated
		_currentBullet++;
		this._bullet = this._bullets[this._currentBullet];
		if(_shooting){													//Shoot if user touches for a while/holds
    		InvokeRepeating("Shoot", 0.1, this._bullet._fireRate);
    		}
	}

}

//Set weapon to the firs Bullet in the _bullets list (Happens when player dies)
function ResetWeapon () {
	_currentBullet=0;
	this._bullet = this._bullets[0];
}


function LateUpdate () {
	if(hit.point != transform.position){
		var rotation:Quaternion = Quaternion.LookRotation(hit.point - transform.position);		//Calculate rotation
		transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * _rotateSpeed);	//Rotate Player towards the touching point
	}
    _moveLook.transform.LookAt(_moveTo.transform.position, transform.up);						//Player movement direction 
	 var d:float = Vector3.Distance(transform.position, _moveTo.transform.position);			//Distance between Player and where it is moving to
   	rigidbody.velocity = _moveLook.transform.forward*_speed*d;									//Move the Player based on _moveLook rotation
    if(d < .5){																					//Disable _moveTo model if Player is close (GUI)
    	if(_moveTo.activeInHierarchy)_moveTo.SetActive(false);														
    }else if(_inControl){																		//Enable _moveTo model if not
    	if(!_moveTo.activeInHierarchy) _moveTo.SetActive(true);
   		_moveTo.transform.Rotate(Vector3(0,100*Time.deltaTime,0));								//Rotate _moveTo model (visual)
   	}
	if(Input.GetMouseButtonDown(0) &&!_shooting){												//Shoot if user touches for a while/holds
		InvokeRepeating("Shoot", 0.1, this._bullet._fireRate);
	}else if(Input.GetMouseButtonUp(0)){														//Move if user clicks/single touches game area
		StopShoot();																			//Stops shooting
		if((!_shooting && (!hit.collider.GetComponent(Pickup)||!_clickToPickup)) && Time.timeScale == 1){				
			_moveTo.transform.position = hit.point;												//Move the _moveTo to the touch position if no pickup is registered
		}else if(_clickToPickup && hit.collider.GetComponent(Pickup)){
//    		Debug.Log("Pickup:"+hit.collider.GetComponent(Pickup)._type);
			hit.collider.GetComponent(Pickup).Pickup();											//Pickup item		
		}	
		_shooting = false;																		//Player is not shooting
	}
}


function Update () {
   	#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_STANDALONE || UNITY_WEBPLAYER
       pos = Input.mousePosition ;			//Get position based on mouse
    #endif
    #if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
    	if (Input.touchCount > 0)
    	pos = Input.GetTouch(0).position ;		//Get position based on touch  
    #endif
    if(Physics.Raycast(Camera.main.ScreenPointToRay(pos), hit)){  
   		 hit.point.y = 0;	
    }
    Debug.DrawLine(transform.position, hit.point);												//Draw a line to visualize where the user is touching on the game area (editor visual)	
}