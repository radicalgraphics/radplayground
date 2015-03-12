#pragma strict
//Script saves score to game over scene
var _score:int;
var _previousLevel:String;
var _levelComplete:boolean;
var _saveSoundVol:float;
var _saveMusicVol:float;

static var instance : SaveStats;			//SaveStats is a singleton.	 SaveStats.instance.DoSomeThing();

function OnApplicationQuit() {				//Ensure that the instance is destroyed when the game is stopped in the editor.
    instance = null;
}

function Start () {
	if (instance){
        Destroy (gameObject);			//Destroy if there is a SaveStats loaded
    }else{
        instance = this;				
        DontDestroyOnLoad (gameObject); //Keep from deleting this gameObject when loading a new scene
    }
}