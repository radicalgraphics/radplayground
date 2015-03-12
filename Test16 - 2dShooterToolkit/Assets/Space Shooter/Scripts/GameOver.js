#pragma strict
var _gameOver:GameObject;				//Gameobject visible when player has died
var _levelComplete:GameObject;			//Gameobject visible when level complete
var _scoreTextMesh:TextMesh;			//Displays score from previous level
var _retryButton:Button;				//Insert retry button to go back to previously loaded level
private var _previousLevel:String;

function Start () {
	_gameOver.SetActive(false);
	_levelComplete.SetActive(false);
	_scoreTextMesh.text = SaveStats.instance._score.ToString("000000000");	//Change score text
	_previousLevel = SaveStats.instance._previousLevel;
	if(SaveStats.instance._score > PlayerPrefs.GetInt(Application.loadedLevelName+"highscore", 0)){
		PlayerPrefs.SetInt(_previousLevel+"highscore", SaveStats.instance._score);
	
	}	
	if(SaveStats.instance._levelComplete){				//If player completed level
		_levelComplete.SetActive(true);
	}else if(!SaveStats.instance._levelComplete){
		_gameOver.SetActive(true);			//If player died in last level
	}
	if(_retryButton){
		_retryButton._sceneToLoad = _previousLevel;		//Replay last level
	}
}