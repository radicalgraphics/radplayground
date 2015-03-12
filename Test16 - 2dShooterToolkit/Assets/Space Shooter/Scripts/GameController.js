#pragma strict
var _startMusic:AudioClip[];					//Music to play when the game starts
//Difficulty modifiers are best used in survival mode
var _difficultyMultiplier:float = 1;			//Increases asteroid hitpoints and max asteroids
var _difficultyMultiplierIncrease:float = .1;	//Increase this amount everytime difficulty is increased
var _difficultyIncreaseAsteroidCount:int;		//Ever x amount of asteroids spawned will increase difficulty
var _maxDifficulty:float = 3;					//Difficulty will stop increasing after reaching this value

var _asteroidMinVelocity:float = 1;				//Keeps Asterois from stopping to prevent dull gameplay
var _asteroidMaxVelocity:float = 2;				//Keeps Asterois from gaining too much velocity
var _asteroids:Asteroid[];						//List of asteroids to spawn at random
var _asteroidMaterials:Material[];				//Materials will be changed randomly on asteroids if this list contains any (adds some visual varaity to the asteroids)
var _asteroidSpawnDelay:float = 0.5;			//Delay between each new asteroid
var _maxAsteroids:int = 3;						//Max asteroids and fragment on the screen (will not spawn new asteroids when there are this many in the game)
var _spawnAsteroidAmount:int = 5;				//-1 will spawn asteroids infinetly (survival mode)
var _asteroidAlignToPlaneSpeed:float = 1;		//How fast asteroids moves to the game area (bigger number makes it harder to avoid new asteroids, can be used for increasing difficulty)
var _spawnAsteroidsWhenEnemy:boolean;			//Turn this on if asteroids should spawn even if a enemy is on stage
var _spawnAsteroidsWhenEnemyDifficultyOveride:float = 2.5;
var _countFragments:boolean=true;				//Counts fragments as asteroids (disable this to ignore asteroid fragments when spawning new asteroids)

var _enemy:Enemy[];								//List of enemies like UFO that spawns less frequently than asteroids
var _maxEnemy:int = 1;							//Max enemies in the game at once
var _spawnEnemyAmount:int = 5;					//-1 will spawn enemies infinetly (survival mode)
var _spawnEnemyEverySecond:float = 3;
var _spawnEnemyChance:float = .1;				//Chance that an enemy will be spawned 1=always 0=never
//Powerups, lives and score multipliers
var _pickup:Pickup;								//Pickup objects that drops
var _dropMultiplier:float = 1;					//chance that items drop
var _bombDropRate:float	= 0.1;					//chance that bomb item drop
var _lifeDropRate:float = 0.05;					//chance that life item drop

var _score:int;

var _highScoreTextMesh:TextMesh;				//GUI score text
var _scoreTextMesh:TextMesh;					//GUI score text
var _scoreMultiplierTextMesh:TextMesh;			//GUI score text
var _maxScoreMultiplier:int=100;

var _lives:GameObject[];						//List of life gameobjects containing model (Future update note: make dynamic)
//Anchors (GUI)
//Attach gameObject containing 3D GUI elements that is to be aligned to the screen
var _guiTopLeft:GameObject;			
var _guiBottomLeft:GameObject;
var _guiTopRight:GameObject;
var _guiBottomRight:GameObject;

var _gameOverScene:String = "Game Over SCN";	//Scene to load when player dies or game is complete

@HideInInspector public var _scoreMultiplier:int = 1;			//Score is multiplied by this value
@HideInInspector public var _toPoints:int;
@HideInInspector public var _asteroidCounter:int;
@HideInInspector public var _enemyCounter:int;

private var _gameWidth:float;					//Width of the game area
private var _gameHeight:float;					//Height of the game area
//Calculated half of the screen; limits the calculation to once the game starts
private var _gameWidthHalf:float;	
private var _gameHeightHalf:float;
//These values contain information about the edges of the screen, calculated once the game start
private var _bottomLeft : RaycastHit;
private var _bottomRight : RaycastHit;
private var _topRight : RaycastHit;
private var _topLeft : RaycastHit;
private var _spawnEnemyCounter:int;		
private var _totalAsteroidsCounter:int;
private var _totalEnemyCounter:int;
private var _currentTrack:int;

static var instance : GameController;			// GameController is a singleton.	 GameController.instance.DoSomeThing();
instance =  FindObjectOfType(GameController);

function OnApplicationQuit() {					// Ensure that the instance is destroyed when the game is stopped in the editor.
    instance = null;
}

function Start () {
	CalcBounds();										//Calculates the game area based on camera edges by casting rays from all corners of the main camera
	_highScoreTextMesh.text = "" +PlayerPrefs.GetInt(Application.loadedLevelName+"highscore").ToString("000000000");
	//if(_difficultyMultiplier<1)_difficultyMultiplier=1;	//Make sure difficulty is not lower than 1
	//_scoreTextMesh.text = _score.ToString("000000000");	//Resets the score text
	GUIPosition ();										//Positiones the 3D GUI
	InvokeRepeating("CountPoints", 1, .01);				//Repeating function that counts the score
	InvokeAsteroids();									//Spawns asteroids until max spawn amount has been reached
	InvokeEnemies ();									//Spawns enemies until max spawn amount has been reached
	yield(WaitForSeconds(.01));							//Wait a little while before accessing SoundController to make sure it is there
	if(_startMusic.Length>0)
		SoundController.instance.PlayMusic(_startMusic[0], 1.5, 1, true);	//Play music
}

function IncreaseDifficulty() {
	if(_totalAsteroidsCounter >0 && _difficultyIncreaseAsteroidCount > 0 && _difficultyMultiplier<this._maxDifficulty && _totalAsteroidsCounter%_difficultyIncreaseAsteroidCount== 0){
		//Debug.Log("Increased Difficulty : " + _totalAsteroidsCounter);
		if(_difficultyMultiplier >= _spawnAsteroidsWhenEnemyDifficultyOveride)	//At this difficulty asteroids will spawn even if there is enemies in the game 
		this._spawnAsteroidsWhenEnemy = true;								
		_difficultyMultiplier+=_difficultyMultiplierIncrease;					//Increase the multiplier value
	}
}

//Called by asteroids to create new asteroids, prefabs instantiating prefab of same class can not be done
function SpawnAsteroid (go:GameObject, spawner:GameObject) {	
	var asteroid:GameObject = Instantiate(go, spawner.transform.position, spawner.transform.rotation);	
	var children:Asteroid[];
	children = asteroid.GetComponentsInChildren.<Asteroid>(true);			//Makes a list of all the asteroid children
	for (var i:int; i < children.length; i++) {
		children[i].renderer.sharedMaterial =spawner.renderer.sharedMaterial;
		children[i].rigidbody.velocity = spawner.rigidbody.velocity;		//Copy the velocity of main asteroid to children when it breaks	
		children[i].transform.localScale = spawner.transform.localScale;	//Resizes asteroids	//Might generate Android lag (Noticed some tiny lagspikes)
		children[i]._hitpoints *=_difficultyMultiplier;						//Increase hitpoints based on difficulty
	}
}

function SpawnEnemy () {	
		if(_asteroidCounter > 2 && _spawnEnemyChance >= Random.value && (_totalEnemyCounter < _spawnEnemyAmount || _spawnEnemyAmount < 0)){	//Check if all enemies in level has been spawned 
			//Debug.Log("running"  + _enemyCounter +" enemies "+_maxEnemy * Mathf.Floor(this._difficultyMultiplier));
			var d:int = 1;
			if(_difficultyMultiplier > 1)d= Mathf.Floor(this._difficultyMultiplier);
			if(_enemyCounter < _maxEnemy * d)	{		
				var enemy:GameObject = Instantiate(_enemy[Random.Range(0, _enemy.length)].gameObject, Vector3(Random.Range(-_gameWidthHalf, _gameWidthHalf)*.5, Random.Range(-10, -20), Random.Range(-_gameHeightHalf, _gameHeightHalf)*.5), Quaternion.identity);	
				_totalEnemyCounter++;							//Counts how many enemies have spawned
			}
		}
}

function SpawnAsteroids () {									
		if((_spawnAsteroidAmount < 0 || _totalAsteroidsCounter < _spawnAsteroidAmount) && (_enemyCounter <=0 || _spawnAsteroidsWhenEnemy)){	//Check if all asteroids in level has been spawned 
			if(_asteroidCounter < _maxAsteroids + _difficultyMultiplier-1)	{
				var asteroid:GameObject = Instantiate(_asteroids[Random.Range(0, _asteroids.length)].gameObject, Vector3(Random.Range(-_gameWidthHalf, _gameWidthHalf), Random.Range(-10, -20), Random.Range(-_gameHeightHalf, _gameHeightHalf)), Quaternion.identity);
				var a:Asteroid = asteroid.GetComponent(Asteroid);
				asteroid.transform.localScale *= Random.Range(a._maxAsteroidSize, a._minAsteroidSize);	//Resizes asteroids based on min/max variables	///Might generate Android lag (Noticed some tiny lagspikes)
				asteroid.rigidbody.AddForce(Random.Range(-5,5), 0 , Random.Range(-5,5));
				a._hitpoints *=_difficultyMultiplier;			//Increase hitpoints based on difficulty
				if(_asteroidMaterials.Length > 0){				//Changes material of asteroids if there are any in the array
					asteroid.gameObject.renderer.sharedMaterial = _asteroidMaterials[Random.Range(0,_asteroidMaterials.Length)];
					//Changes material of the asteroids children to match the new material
					var children:Renderer[];
					children = asteroid.GetComponentsInChildren.<Renderer>(true);	//Makes a list of all the renderers in the asteroid children
					for (var i:int; i < children.length; i++) {
						children[i].renderer.sharedMaterial =asteroid.gameObject.renderer.sharedMaterial;
					}
				}
				_totalAsteroidsCounter++;						//Counts how many asteroids have spawned in total (Fragments not included)
				IncreaseDifficulty();							//Increases difficulty based on how many asteroids have spawned
			}else{
				CancelInvoke("SpawnAsteroids");					//Stops spawning asteroids when max limit has been reached
			}
		}else if(_asteroidCounter <= 0 && _enemyCounter <= 0){	//Game Complete
			GameOver(true);
		}
}

function InvokeEnemies () {	//Spawns enemies until max spawn amount has been reached
	CancelInvoke("SpawnEnemy");
	if(_spawnEnemyChance > 0)
		InvokeRepeating("SpawnEnemy", _spawnEnemyEverySecond, _spawnEnemyEverySecond);	//Start spawning enemies
}

function InvokeAsteroids () {	//Spawns asteroids until max spawn amount has been reached
	CancelInvoke("SpawnAsteroids");
	InvokeRepeating("SpawnAsteroids", _asteroidSpawnDelay, _asteroidSpawnDelay);
}


function AddPoints (points:int) {						//Adds points to score
	_toPoints+=points*_scoreMultiplier;
}

function SetScoreMultiplier (amount:int) {
	_scoreMultiplier = amount;
	_scoreMultiplierTextMesh.text = "x" + _scoreMultiplier;
}

function AddScoreMultiplier (amount:int) {
	if(_scoreMultiplier < _maxScoreMultiplier){
		_scoreMultiplier += amount;
			_scoreMultiplierTextMesh.text = "x" + _scoreMultiplier;
	}
}

function CountPoints() {								//Adds points to score
	if(_toPoints > _score){
		_score+=  Mathf.Ceil((_toPoints-_score)*.1);
		_scoreTextMesh.text = _score.ToString("000000000");
	}
}

function Drop (pos:Vector3) {							//Drop an item on the play field
		if(Player.instance._inControl)					//Won't drop anything if player has just died
		Instantiate(_pickup, pos , _pickup.transform.rotation);
}

// Calculates the game area based on camera edges by casting rays from all corners of the main camera
function CalcBounds () {
			if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0, 0, 0)),_bottomLeft)){
    			_bottomLeft.point.y = 0;   		
    		}		
    		if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(1, 0, 0)),_bottomRight)){
    			_bottomRight.point.y = 0;		
    		}
    		if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(1, 1, 0)),_topRight)){
    			_topRight.point.y = 0;
    		}
    		if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0, 1, 0)),_topLeft)){
    			_topLeft.point.y = 0;
    		}
    		//Set game are area
			_gameWidth = Vector3.Distance(_bottomLeft.point, _bottomRight.point);
			_gameHeight = Vector3.Distance(_bottomLeft.point, _topLeft.point);
 			//Calculate half sizes once to avoid doing this multiple times in other functions
 			_gameWidthHalf = _gameWidth*.5 +.75;
			_gameHeightHalf = _gameHeight*.5 +.75;
}

//Positiones the 3D GUI
function GUIPosition () {
	_guiTopLeft.transform.position = _topLeft.point;
	_guiBottomRight.transform.position = _bottomRight.point;
	_guiTopRight.transform.position = _topRight.point;
	_guiBottomLeft.transform.position = _bottomLeft.point;
}

//Visuals to be able to see the play area and other information in the editor
function OnDrawGizmos () {
	if(!Application.isPlaying)									//Only calculate repeatedly when not in play mode
		CalcBounds();
		Gizmos.DrawCube(_bottomLeft.point, Vector3.one*.1);			//Draw cubes on game area corners (without padding)
		Gizmos.DrawCube(_bottomRight.point, Vector3.one*.1);	
		Gizmos.DrawCube(_topRight.point, Vector3.one*.1);
		Gizmos.DrawCube(_topLeft.point, Vector3.one*.1); 		
	 	Gizmos.DrawWireCube (transform.position, Vector3 (_gameWidth,0,_gameHeight));	//Draw wires for padded game area
}

//Check if a gameObject is outside the game area (this is run by objects that wraps edges when leaving area)
//CheckBounds (	obj: 	object to wrap, 
//				ps: 	particle system to disable then re enable, (world particle systems sometimes leaves a trail when moved instantly)(set to null if no particle system)
function CheckBounds (obj:GameObject, ps:ParticleSystem) { 	
		if(obj.transform.position.x > _gameWidthHalf){	
			obj.transform.position.x = -_gameWidthHalf ;
			if(ps)DisableEnablePS(ps);
		}else if(obj.transform.position.x < -_gameWidthHalf){
			obj.transform.position.x = _gameWidthHalf;
			if(ps)DisableEnablePS(ps);
		}
		if(obj.transform.position.z > _gameHeightHalf){	
			obj.transform.position.z = -_gameHeightHalf;
			if(ps)DisableEnablePS(ps);
		}else if(obj.transform.position.z < -_gameHeightHalf){
			obj.transform.position.z = _gameHeightHalf;
			if(ps)DisableEnablePS(ps);
		}
}
//padding: some objects like pickups need to be further inside the screen to be visible and touchable/clickable, added some padding to fix
function CheckBoundsInverted (obj:GameObject, ps:ParticleSystem, padding:float) { 	
		if(obj.transform.position.x > _gameWidthHalf+-padding){	
			obj.transform.position.x = _gameWidthHalf -padding;
			if(ps)DisableEnablePS(ps);
		}else if(obj.transform.position.x < -_gameWidthHalf+padding){
			obj.transform.position.x = -_gameWidthHalf +padding;
			if(ps)DisableEnablePS(ps);
		}
		if(obj.transform.position.z > _gameHeightHalf-padding){	
			obj.transform.position.z = _gameHeightHalf -padding;
			if(ps)DisableEnablePS(ps);
		}else if(obj.transform.position.z < -_gameHeightHalf+padding){
			obj.transform.position.z = -_gameHeightHalf +padding;
			if(ps)DisableEnablePS(ps);
		}
}
//disables then adds delay to re enable, fixes particle trails
function DisableEnablePS(ps:ParticleSystem){
	if(ps.enableEmission){
		ps.enableEmission = false;
 		yield(WaitForSeconds(.05));
 		if(ps && ps.transform.parent.collider.enabled)
 		ps.enableEmission = true;
 	}
}

//destroy all gameobjects with tag (Use with causion, Find is heavy on mobile)
function DestroyAll(tag:String) {
	var gameObjects:GameObject[] =  GameObject.FindGameObjectsWithTag (tag);
    for(var i = 0 ; i < gameObjects.length ; i ++)
        Destroy(gameObjects[i]);
}

//Create a force explosion
function Explosion (explosionPos : Vector3, radius:float, power:float) {
        var colliders : Collider[] = Physics.OverlapSphere (explosionPos, radius);     
        for (var hit : Collider in colliders) {
            if (!hit)
                continue;           
            if (hit.rigidbody && hit.gameObject.tag!="Bullet")
                hit.rigidbody.AddExplosionForce(power , explosionPos, radius);
        }
}

//Checks player status to correspond with GUI lives
function CheckPlayer () {
	for(var i:int; i < _lives.Length; i++){	
		if(i>=Player.instance._lives){		
			_lives[i].SetActive(false);	
		}else if(i<=Player.instance._lives){
			_lives[i].SetActive(true);
		}	
	}
}

//Function is run when player dies or game is complete
function GameOver (levelComplete:boolean) {
	SoundController.instance.PlayMusic(null, 1.5, 1, true);				//Fade down the music
	yield(WaitForSeconds(2));											//Wait a little while before changing level
	SoundController.instance.PlayMusic(_startMusic[1], 1.5, 1, true);	//Start playing game over music
	SaveStats.instance._score=this._score;								//Saves score
	SaveStats.instance._previousLevel = Application.loadedLevelName;	//Saves this levels name so that it can be used in a replay button
	SaveStats.instance._levelComplete = levelComplete;					//Did the player die or complete the level
	Application.LoadLevel(_gameOverScene);								//Load game over scene
}
//If game has many sounds this can used to change music trough the pause menu
function NextTrack () {													
	_currentTrack++;													//Next music track
	if(_currentTrack >= _startMusic.Length){							//Check music list lenght in case reached the end
		_currentTrack = 0;
	}
	if(Time.timeScale > 0)			//Check if in pause mode
	SoundController.instance.PlayMusic(_startMusic[_currentTrack], 1.5, 1, true);	//Fade = true
	else
	SoundController.instance.PlayMusic(_startMusic[_currentTrack], 1.5, 1, false);	//Fade = false (can't fade when timeScale = 0)
}