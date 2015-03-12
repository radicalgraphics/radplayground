#pragma strict
//Bomb pickup properties
var _bomb:GameObject;			//Model of Bomb
var _bombSound:AudioClip;		//Sound when Bomb picked up
//Life pickup properties
var _life:GameObject;			//Model of Life
var _lifeSound:AudioClip;		//Sound when Life picked up
//Point pickup properties
var _point:GameObject;			//Model of Point
var _pointSound:AudioClip;		//Sound when Point picked up
var _lifetime:float = 10.0;		//How long is the pickup item alowed to be on the stage
var _type:String;				//Name of the Pickup object (bomb, life, point etc)

private var _pickedUp:boolean;	//Has this object been picked up (start the movement towards player)
private var _moveFrom:Vector3;	//Postion of the Pickup once it gets picked up
private var _moveCounter:int;	//Counter for movement over time(Lerp)
private var _model:GameObject;	//Gameobject based on what type of item this is


function Start () {
	if(_type == ""){
		var r:float = Random.value;
		if(r < GameController.instance._lifeDropRate && Player.instance._lives < Player.instance._maxLives)	_type = "life";								//Check if life should drop
		else if(r < GameController.instance._bombDropRate && Player.instance._currentBullet <  Player.instance._bullets.Length-1)	_type = "bomb";		//If no life is dropped, check for bomb
		else _type = "point";							//If no other drops, drop point
	}	
	//Activate model based on _type
	if(_type == "bomb")	{
		_bomb.renderer.enabled = true;
		_model = _bomb;
	}else if(_type == "point"){
		_point.renderer.enabled = true;
		_model = _point;
	}else if(_type == "life"){
		_life.renderer.enabled = true;
		_model = _life;
	}
	AutoDestroy();													//Start the countdown to remove object automaticly
	GameController.instance.CheckBoundsInverted(gameObject, null, 1);	//Checks if gameObject is outside the game area
}

//Call to activate the movement towards player
function Pickup () {	
	_pickedUp = true;
	_moveFrom = this.transform.position;
}

//Blinking before destroyed
function AutoDestroy () {	
	yield(WaitForSeconds(_lifetime -3));
	_model.renderer.enabled = false;
	yield(WaitForSeconds(.1));
	_model.renderer.enabled = true;
	yield(WaitForSeconds(.5));
	_model.renderer.enabled = false;
	yield(WaitForSeconds(.2));
	_model.renderer.enabled = true;
	yield(WaitForSeconds(.4));
	_model.renderer.enabled = false;
	yield(WaitForSeconds(.3));
	_model.renderer.enabled = true;
	yield(WaitForSeconds(.3));
	_model.renderer.enabled = false;
	yield(WaitForSeconds(.2));
	_model.renderer.enabled = true;
	yield(WaitForSeconds(.2));
	_model.renderer.enabled = false;
	yield(WaitForSeconds(.1));
	_model.renderer.enabled = true;
	yield(WaitForSeconds(.1));
	_model.renderer.enabled = false;
	yield(WaitForSeconds(.1));
	_model.renderer.enabled = true;
	yield(WaitForSeconds(.1));
	_model.renderer.enabled = false;
	yield(WaitForSeconds(.1));
	_model.renderer.enabled = true;
	yield(WaitForSeconds(.1));
	_model.renderer.enabled = false;
	yield(WaitForSeconds(.1));
	_model.renderer.enabled = true;
	yield(WaitForSeconds(.1));
	Destroy(this.gameObject);
}

function FixedUpdate () {
	if(!Player.instance._inControl){		//Destroy pickup if player is dead
		Destroy(this.gameObject);
	}else if(!_pickedUp){
		//transform.Rotate(Vector3(0,0,100*Time.deltaTime));										//Rotates the gameobject (effect)		
		if(Vector3.Distance(Player.instance.transform.position, this.transform.position) < 2 ){	//Pickup if close to player
			this.Pickup();	
		}	
	}else{
		_moveCounter++;																										//Increase counter for movement over time(Lerp)
		_model.renderer.enabled = true;			
		StopCoroutine("AutoDestroy");																		
		transform.position = Vector3.Lerp(_moveFrom, Player.instance.transform.position, Time.deltaTime*_moveCounter*2);	//Move towards player
		if(Vector3.Distance(Player.instance.transform.position, this.transform.position) < .25){							//Check distance to player, if close enough it will trigger event based on _type	
			if(_type == "point"){																							//Adds to score multiplier
				if(_pointSound) SoundController.instance.Play(_pointSound, 2, Random.Range(1.5,1.25));						//Play sound
				GameController.instance.AddScoreMultiplier(1);
			}else if(_type == "bomb"){																						//Upgrade weapon
				if(_bombSound) SoundController.instance.Play(_bombSound, 2, 1.25);											//Play sound
				Player.instance.UpgradeWeapon();											
			}else if(_type == "life"){																						//Add life to player stats
				if(_lifeSound) SoundController.instance.Play(_lifeSound, 2, 1.25);											//Play sound
				if(Player.instance._lives < Player.instance._maxLives)
				Player.instance._lives++;				
			}
			GameController.instance.CheckPlayer();		//Update player stats (GUI)
			Destroy(this.gameObject);					//Remove gameObject
		}
	}
}