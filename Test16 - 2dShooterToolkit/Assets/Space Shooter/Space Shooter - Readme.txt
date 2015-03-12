FEATURES
	• Mobile Touch Game*
	• Touch and Mouse gameplay mechanics
	• Powerups - Score Counter - UFO's
	• Automaticly adjusts to any screen size
	• Sound Controller - No need to attach sound to gameObjects
	• Vertex colored models - Low memory useage (no textures)
	• Asteroids breaking into pieces**
	• Easy code - Well commented that's easy to build upon
	• Works on Unity and Unity Pro
	
	*Tested smoothly on Android Samsung Galaxy Tab™ 2 10.1
		http://www.samsung.com/us/mobile/galaxy-tab/GT-P5113TSYXAR-specs
	**Small fragments is a single particle system generating 1 draw call

INCLUDES
	• Everything in Demo (*Music not included)
	• 3D text alphabet and models 
	• Mobile Vertex shaders
	• Sound samples from pack "Retro Sound Effects"
		https://www.assetstore.unity3d.com/#/content/2887
	• Particle FX samples from pack "FX Mega Pack"
		https://www.assetstore.unity3d.com/#/content/8933

UPDATES
	V1.1
	• Updated from Unity 3.5.7 to Unity 4.0
	• Added Highscore to each level
	• Added Model Preview Scene
	• Merged GUI with GameController GameObject

SCRIPT DESCRIPTION
	Read comments for each script for more information
	Asteroid.js			- Behaviour, hitpoints, score... etc of asteroids
	Bullet.js			- Bullets shot by player or enemies
	Button.js			- Controls all the buttons in the game
	Enemy.js			- Enemy behaviour (UFO)
	GameController.js	- Controls game events and keeps track of score
	GameOver.js			- Controls events in the Game Over scene
	Pickup.js			- Powerups, points and bombs dropped from Asteroids
	Player.js			- Behaviour, hitpoints, score... etc of Player. Handles Mouse/Touch events for player movement.
	SaveStats.js		- Saves score and carries it to next scene (GameOver)
	SoundController.js	- Handles all sounds and music in the game by simply calling the play function (Example: SoundController.instance.PlayMusic(_startMusic[1], 1.5, 1, true);)
	SpringScale.js		- Simple script to scale object from zero to desired size over a period of time. (Used on buttons)
	Stars.js			- Moves the stars negatively based on player position to simulate paralax scroll

GETTING STARTED
	CREATING A NEW LEVEL
		Duplicate an existing level
		Select the "_Game Controller" GameObject
		Change the variables to achieve the wanted difficulty
			Each variable is explained in the script comments, looking trough these are a good place to start
		The "Player" GameObject variables can also be altered to mix up the gameplay and difficulty
		
		Use this same procedure for creating new Asteroids and Enemies by duplicating the Prefab
	CREATING A NEW BULLET
		Duplicate a existing bullet prefab
		Drag it to the stage
		Child bullets inherit the firerate of the parent bullet
		Adding or decreasing "Mass" in the "Rigidbody" will determine the pushback effect of the bullet
		Make sure that the parent bullet has the longest lifetime
		To test a bullet add it to the "Bullet" variable of "Player" and it will be the first weapon of the player
		Lastly add it to the "Bullets" list of the "Player"


FOR INFORMATION AND SUPPORT, VISIT:
	Unluck Software
	http://www.chemicalbliss.com/

Thanks for purchasing this asset
Have fun with Unity!
