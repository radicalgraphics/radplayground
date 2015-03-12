#pragma strict
//Button script used by all buttons in the game
var _buttonSound:AudioClip;			//Place sound clip for when button is pressed down
var _buttonSoundUp:AudioClip;		//Place sound clip for when button is released (this can be based on type of button)
var _pauseButton:boolean;			//Enable for pause button
var _playMesh:Mesh;					//Mesh for play button after pause has been pressed
var _pauseMenu:GameObject;			//GameObject containing the buttons to be visible during pause
var _quitButton:boolean;			//Enable for quit button (only level mode)
var _quitMenu:GameObject;			
var _musicButton:boolean;			//Enable for music button
var _soundButton:boolean;			//Enable for sound button
var _nextTrackButton:boolean;		//Enable for track shuffle button
var _offIcon:GameObject;			//Place icon for on/off buttons
var _guiCamera:Camera;				//The gui camera(Ortographic) provides more accurate raycasting for small buttons

var _scaleDown:float = 0.9;
var _sceneToLoad:String;			//Enter the name of scene to load when button is pressed
private var _saveScale:Vector3;		//Saves button scale
private var _savePauseMesh:Mesh;	//Saves pause button mesh


function Start () {
	_saveScale = transform.localScale;				//Save the scale for reset
	if(SaveStats.instance._saveMusicVol==0){
	SaveStats.instance._saveMusicVol = SoundController.instance._musicVol;
	SaveStats.instance._saveSoundVol = SoundController.instance._soundVol;
	}
	//Button types
	if(_musicButton && SoundController.instance._musicVol>0){
		_offIcon.renderer.enabled = false;
	} else if(_soundButton && SoundController.instance._soundVol>0){
		_offIcon.renderer.enabled = false;
	} else if(_pauseButton){
		_pauseMenu.SetActive(false);
		_savePauseMesh = transform.GetComponent(MeshFilter).sharedMesh;
	} else if(_quitButton){
		_quitMenu.SetActive(false);
	}
	//Find best camera
	if(!_guiCamera){
		var camGO:GameObject = GameObject.Find("_Gui Camera");
		if(camGO){
			_guiCamera = camGO.camera;
		}else{
			_guiCamera = Camera.main;
		}	
	}	
}

function Update () {
	for (var touch : Touch in Input.touches) {
		var hit:RaycastHit = new RaycastHit();
		var t:boolean;
		var ray = _guiCamera.ScreenPointToRay (touch.position);
		if (touch.phase == TouchPhase.Began && Physics.Raycast(ray,hit,500000)) {	
			if (hit.collider.gameObject == this.gameObject) {						//User touches a button
				OnButtonDown();
				t= true;
			}
		}
		if(t && touch.phase == TouchPhase.Ended){									//When user stops touching screen but not pressing button (button just resets, nothing happens)
			ButtonUp ();
		}
	}
}
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_STANDALONE || UNITY_WEBPLAYER
function OnMouseUp () {
	transform.localScale = _saveScale;
}
#endif
function ButtonUp () {
	if(_sceneToLoad){
		Application.LoadLevel(_sceneToLoad);
		Time.timeScale = 1;
	}else if(_pauseButton){
		if(Time.timeScale > 0){
			Time.timeScale = 0;
			_pauseMenu.SetActive(true);
			transform.GetComponent(MeshFilter).sharedMesh = _playMesh;
			if(this._buttonSoundUp)
			SoundController.instance.Play(_buttonSoundUp, 2, 1);
		}else{
			_pauseMenu.SetActive(false);
			_quitMenu.SetActive(false);
			transform.GetComponent(MeshFilter).sharedMesh = _savePauseMesh;
			Time.timeScale = .99;
			yield(WaitForSeconds(.1));
			Time.timeScale = 1;
		}
	}else if(_musicButton){
		if(SoundController.instance._musicVol > 0){
			SoundController.instance._musicVol = 0;
			_offIcon.renderer.enabled = true;
		}else{
			SoundController.instance._musicVol = SaveStats.instance._saveMusicVol;
			_offIcon.renderer.enabled = false;
		}
		SoundController.instance.UpdateMusicVolume();
	}else if(_soundButton){
		if(SoundController.instance._soundVol > 0){
			SoundController.instance._soundVol = 0;
			_offIcon.renderer.enabled = true;
		}else{
			SoundController.instance._soundVol = SaveStats.instance._saveSoundVol;
			_offIcon.renderer.enabled = false;
		}
	}else if(_nextTrackButton){
		GameController.instance.NextTrack();
		
	}else if(_quitButton){
		_quitMenu.SetActive(!_quitMenu.activeInHierarchy);
		
	}
}

function OnMouseUpAsButton () {
	ButtonUp ();
}

function OnButtonDown () {
	if(this._buttonSound)
	SoundController.instance.Play(_buttonSound, 2, Random.Range(1.5,1.25));	//Play a sound when pressing button
	transform.localScale = _saveScale * _scaleDown;							//Scale the button down to indicate button is being pressed
}

function OnMouseDown () {
	OnButtonDown();
}