#pragma strict
var _hitpoints:float = 3;			//How many hits a asteroid can take from a default bullet
var _points:int = 100;				//How many point player gets from destroying this asteroid
var _breakInto:GameObject;			//Contains the prefab that this asteroid spawnes when it is destroyed (can be blank)
var _debrisMultiplier:float = 1;	//Change how much debris should be emitted from the _debrisPS particle system
var _dropChance:float;				//Chance asteroid will drop an item (multiplied by the GameController._dropMultiplier)
var _maxAsteroidSize:float = 1.25;	//Asteroids are scaled at start for variety
var _minAsteroidSize:float = .75;
static var _debrisPS:ParticleSystem;//The particle system that emits debris (this must be present on the stage as "Debris PS"

function Start () {
	if(GameController.instance._countFragments||!GameController.instance._countFragments && transform.parent==null)//Only count if this is not a fragment
	GameController.instance._asteroidCounter++;				//Increase counter that checks how many asteroids are on the stage
	if(!_debrisPS){
	//Debug.Log("Found Debris > Should happen once");
	_debrisPS = GameObject.Find("Debris PS").GetComponent(ParticleSystem);
	}
}

function FixedUpdate() {
	//Max / Min Astreoid velocity (controlled by GameController)
	//Fixes issues where Asteroids builds up too much speed or stops completely, also good for altering difficulty
    if(rigidbody.velocity.sqrMagnitude > GameController.instance._asteroidMaxVelocity){ 
        rigidbody.velocity = rigidbody.velocity.normalized * GameController.instance._asteroidMaxVelocity;
    } else if(rigidbody.velocity.sqrMagnitude < GameController.instance._asteroidMinVelocity){
        rigidbody.velocity = rigidbody.velocity.normalized * GameController.instance._asteroidMinVelocity;
    }
    if(transform.position.y > -0.5)
    GameController.instance.CheckBounds(this.gameObject, null); 	// Check if the asteroid is in the game area, moves it to the closest edge if it is outside
}

function Update () {
	if(_hitpoints < 0){											//When asteroid no longer has any hitpoints left
		var emit:int;											
		if(this._breakInto){			
			GameController.instance.SpawnAsteroid(_breakInto, gameObject);
			emit = 10*_debrisMultiplier;						//How many debris pieces to emit when asteroid breaks into pieces 
		}else{
			emit = 25*_debrisMultiplier;						//How many debris pieces to emit when asteroid is destroyed	
		}
		_debrisPS.transform.position = transform.position;		//Set Particle System position on asteroid position
		_debrisPS.startColor = renderer.sharedMaterial.color;	//Change color of debris to match asteroid
		_debrisPS.Emit(emit);									//Start emission	 
		if(GameController.instance._countFragments||!GameController.instance._countFragments && transform.parent==null)//Only count if this is not a fragment
		GameController.instance._asteroidCounter--;				//Decrease counter that checks how many asteroids are on the stage
		if(_dropChance * GameController.instance._dropMultiplier > Random.value){
			GameController.instance.Drop(transform.position);
		}
		GameController.instance.AddPoints(_points);				//Adds points to score
		GameController.instance.InvokeAsteroids();				//Start routine to check if game should spawn more asteroids
		Destroy(gameObject);
	}else{	
		if(transform.position.y < -0.01){						//Checks to see if asteroid is on the game plane
			transform.position.y = Mathf.Lerp(transform.position.y, 0, Time.deltaTime*GameController.instance._asteroidAlignToPlaneSpeed); //Gradually positiones the asteroid closer to the game plane
		}else{
			
			transform.position.y = 0;										// Keeps the asteroid on the game plane
		}
	}
}
//NOT USED
//function RandomForce (amount:float) {
//	
//	rigidbody.AddTorque(Vector3(Random.Range(100,-100),Random.Range(100,-100),Random.Range(100,-100))*amount);
//	rigidbody.AddForce(Vector3(Random.Range(1000,-1000),Random.Range(1000,-1000),Random.Range(1000,-1000))*amount);
//
//}
