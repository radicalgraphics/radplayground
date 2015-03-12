#pragma strict
var _rocket:ParticleSystem;			//Bullet is built up entierly by particles
var _explosion:ParticleSystem;		//Explosion particle system on collision
var _rotationOffset:float;			//Shoot bullet sideways
var _RandomRotationOffset:float;	//Randomly shoot bullet sideways
var _spreadDelay:float = .25;		//How long until the bullets loose their trajectory (A more visually pleasing way to limit the bullets range alowing them to live longer but less effective after delay)
var _spreadAmount:float = .5;		//How much the bullets spread after spread delay
var _bulletLifetime:float = 3;		//How long the bullet is alowed to live without hitting anything (seconds)
var _bulletPower:float = 1;			//Hitpoints removed from hit object
var _fireRate:float = 0.1;			//Bullets rate of fire, low = fast
var _audioBirth:AudioClip;			//Audio clip to play when object is created
var _pitchRandom:Vector2 = Vector2(.5,1.5);
var _audioDeath:AudioClip;			//Audio clip to play when object is destroyed
var _wrapGameBorders:boolean;		//Bullets that leaves game area are moved to the oposite side
var _bulletSpeed:float = 3;			//Speed of bullets
private var _spread:boolean;
private var rotTarget:Quaternion;	//Rotation used for direction


function Start () {
	if(_audioBirth)		SoundController.instance.Play(_audioBirth, Random.Range(.5,1), Random.Range(_pitchRandom.x,_pitchRandom.y));	//Audio on birth
	//_rocket.startLifetime*=Random.Range(.5,1);	//Randomize the _rocket lifetime to make bullets look less clony
	rigidbody.rotation.eulerAngles.y+=_rotationOffset+Random.Range(-_RandomRotationOffset, _RandomRotationOffset);
	rotTarget = rigidbody.rotation;				//set rotation for direction
	
	Spread();									//Randomly rotates the gameobject to change direction (delayed)
	DestroyBullet ();							//Removes the bullet from the stage (delayed)
	
	yield(WaitForSeconds(_bulletLifetime));		//disable the particle emission before destroying (visual)
	_rocket.enableEmission = false;	
	collider.enabled = false;
}

//Removes the bullet from the stage
function DestroyBullet () {
	yield(WaitForSeconds(_bulletLifetime+.75));	//Wait for _bulletLifetime before destroying the bullet (delayed so that the rocket particle system has time to stop emission)
		Destroy(this.gameObject);
}

//Randomly rotates the gameobject to change direction (delayed)
function Spread () {
	yield(WaitForSeconds(_spreadDelay));	//Wait for _spreadDelay before making bullet steer off course
	randomRot ();							
	_spread = true;							//Indicate that the bullet now should move towards the random rotation
}

function OnCollisionEnter (col:Collision) {
	var e:GameObject = Instantiate(_explosion.gameObject, transform.position, rigidbody.rotation);
	Destroy(e, _explosion.duration);																				//Destroy the bullet (Delayed)
	_rocket.enableEmission = false;																					//Disable the rocket particle system
	transform.collider.enabled = false;																				//Disable collider to prevent more collsions since rocket needs to stop before destroying
	if(_audioDeath)		SoundController.instance.Play(_audioDeath, Random.Range(.5,1), Random.Range(.5,1.5));		//Audio on death	
	//GameController.instance.Explosion(transform.position , 1, 1000);												//Create and force explosion that effects all nearby objects(exept other bullets)
	//col.rigidbody.AddTorque(Vector3(Random.Range(100,-100),Random.Range(100,-100),Random.Range(100,-100)));		//Add some rotation to the hit object
	if(col.transform.tag == "Asteroid")																				//Hit an asteroid
		col.transform.GetComponent(Asteroid)._hitpoints-=_bulletPower;												//Reduce hitpoints of hit object based on bullet power
	else if(col.transform.tag == "Enemy")																			//Hit an Enemy
		col.transform.GetComponent(Enemy)._hitpoints-=_bulletPower;													//Reduce hitpoints of hit object based on bullet power
}

function FixedUpdate () {
	rigidbody.velocity = transform.forward*_bulletSpeed;															//Movement forward
	if(_spread)rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, rotTarget, Time.deltaTime*_spreadAmount);	//Rotates to a random rotation, rotation speed is based on _spreadAmount
	if(_wrapGameBorders) GameController.instance.CheckBounds(this.gameObject, _rocket);								//Checks if the bullet is outside the game area
}

//Randomly rotates the gameobject to change direction (smooth)
function randomRot () {
	 rotTarget = Random.rotation;
}