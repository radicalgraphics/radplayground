#pragma strict
var _hitpoints:float = 3;			//How many hits can the enemy take before exploding
var _maxSpeed:float = 2;			//Enemy max velocity
var _shield:GameObject;				//Shield pops up when enemy collides with something (bullets still do damage)
var _explosion:ParticleSystem;		//Explosion when enemy dies
var _model:GameObject;				//Model of enemy
var _bullet:Bullet;					//Bullet that enemy shoots
var _shootEverySecond:float = 2;	//How often enemy shoots
var _shootChance:float = .5;		//Chance that enemy fires off a bullet when shooting
var _points:int = 1000;				//Points player gets when enemy is defeated
var _rotateBackForce:float = 300;	//How much force to add
var _dropPickupOnHit:boolean = true;//Drops loot when hit by a bullet
var _dropChance:float = 1;			//0=never 1=always

private var _shieldCounter:float;	//Counter to deactivate shield
private var _force:Vector3;			//Force added to enemy to move it
private var _dead:boolean;			

var _particleSystem:ParticleSystem;	//Particle system emitted from enemy
								
function Start () {
	GameController.instance._enemyCounter++;
	InvokeRepeating("RandomDirection", 0 , 2);	//Change force direction every 2 seconds
	if(_shield){								//Deactivate shield
		_shield.SetActive(false);
	}
	InvokeRepeating("Shoot", _shootEverySecond*2 , _shootEverySecond);	//Begins shooting routine
}


function Shoot () {
	if(Random.value<.5){																//Only shoots if value is smaller than _shootChance
		var rot:Quaternion;						
		rot.SetLookRotation(Player.instance.transform.position - transform.position); 	//Rotate towards player
		var b:Bullet = Instantiate(_bullet, transform.position, rot);					//Create a bullet on the stage
	}
}


function DestroyEnemy () {
	if(!_dead){													//Make sure that this function is only run once
		GameController.instance._enemyCounter--;				//Decrease enemycounter in GameController
		_explosion.transform.position = transform.position;		//Positiones the explosion on enemy position
		_explosion.Play();										//Play explosion particle system
		_dead = true;											//Enemy no longer alive but still active
		_model.SetActive(false);									
		this.collider.enabled = false;
		_particleSystem.Stop();
		_shield.SetActive(false);
		CancelInvoke();											//Stops shooting
		rigidbody.velocity*=.1;									//Slows down speed
		GameController.instance.AddPoints(_points);				//Add points to score
		GameController.instance.InvokeEnemies ();				//Starts creating new enemies
		yield(WaitForSeconds(1));
		Destroy(gameObject);									//Removes enemy completely
	}
}


function FixedUpdate () {
	
	rigidbody.AddForce(_force);								//Add movement to enemy	
	//Deactivated because enemy rigidbody is frozen on these axises
//	if(transform.rotation.z < 0 ){
//		transform.rigidbody.AddTorque(0,0,_rotateBackForce*Time.deltaTime *-transform.rotation.eulerAngles.z);
//	}else if(transform.rotation.eulerAngles.z > 0){
//		transform.rigidbody.AddTorque(0,0,_rotateBackForce*Time.deltaTime *-transform.rotation.eulerAngles.z);
//	}
//	if(transform.rotation.x < 0 ){
//		transform.rigidbody.AddTorque(_rotateBackForce*Time.deltaTime *-transform.rotation.x,0,0);
//	}else if(transform.rotation.x > 0){
//		transform.rigidbody.AddTorque(_rotateBackForce*Time.deltaTime *-transform.rotation.x,0,0);
//	}
	//Gradually rotates enemy to zero
	if(transform.rotation.y < 0 ){
		transform.rigidbody.AddTorque(0,_rotateBackForce*Time.deltaTime *-transform.rotation.y,0);
	}else if(transform.rotation.y > 0){
		transform.rigidbody.AddTorque(0,_rotateBackForce*Time.deltaTime *-transform.rotation.y,0);
	}
	
	//Makes sure the enemy velocity dont go faster than max speed
	if(rigidbody.velocity.sqrMagnitude > _maxSpeed){ 
        rigidbody.velocity = rigidbody.velocity.normalized * _maxSpeed;
    } 
}


function OnCollisionEnter (col:Collision) {
	if(_hitpoints<1){
		DestroyEnemy();			//Destroy enemy if it has no hitpoints left
	}else{
		ActivateShield();		//Activate shield (Shield is just visual and has no real effect)
		if(_dropPickupOnHit && col.gameObject.tag == "Bullet" && _dropChance > Random.value)	//Drops item when hit by bullet
			GameController.instance.Drop(transform.position);
	}
}


function DeActivateShield () {
	if(_shield){
		_shield.SetActive(false);	//Deactivates shield
	}
}


function ActivateShield () {	//Activate shield (Shield is just visual and has no real effect)
	if(_shield){
		_shieldCounter = 0;		//Resets the counter that deactivates shield
		_shield.SetActive(true);	
	}
}


function LateUpdate () {
	if(_shieldCounter > 1){					//Check if it is time to deactivate the shield
		DeActivateShield();
	}else{
		_shieldCounter += Time.deltaTime;	//Increase shield counter
	}
	if(transform.position.y < -0.01){													//Checks to see if enemy is on the game plane
		transform.position.y = Mathf.Lerp(transform.position.y, 0, Time.deltaTime*GameController.instance._asteroidAlignToPlaneSpeed); //Gradually positiones the asteroid closer to the game plane
	}else{
		GameController.instance.CheckBounds(this.gameObject, this._particleSystem); 	// Check if the enemy is in the game area, moves it to the closest edge if it is outside
		transform.position.y = 0;														// Keeps the enemy on the game plane
	}
}


function RandomDirection () {				//Change the direction of the enemy
	_force = Vector3(Random.Range(-1000, 1000), 0 ,Random.Range(-1000, 1000)); 
}