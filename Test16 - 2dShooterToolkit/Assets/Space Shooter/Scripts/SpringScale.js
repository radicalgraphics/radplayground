#pragma strict
//A simple script that scales up gameobject from zero to the original scale
//Consider using a tween plugin like iTween to improve the behaviours
//This script is just a simple example to add some pizzazz to the menu items and buttons
var _pulseSpeed:float = 10;
var _pulseAmplitude:float = .01; 
var _scaleUpAnim:boolean;
var _scaleUpSpeed:float = .45;
var _scaleUpDelay:float = 0;

private var _scaleUp:boolean;    
private var _scaleSave:Vector3;
private var _pulse:boolean;
private var _velocity:Vector3= Vector3.zero;
	
function Start () {
	if(_scaleUpAnim){
	_scaleSave = transform.localScale;
	transform.localScale = Vector3.zero;
	yield(WaitForSeconds(_scaleUpDelay));
	_scaleUp = true;
	}
}

function LateUpdate () {
	if(_scaleUp && !_pulse && transform.localScale.x+.01 < _scaleSave.x){		
		transform.localScale = Vector3.SmoothDamp(transform.localScale, _scaleSave ,_velocity, _scaleUpSpeed);
	}else if(_scaleUp && _pulseSpeed > 0 && _pulseAmplitude > 0){	
		_pulse = true;
  	 	transform.localScale.x = transform.localScale.z = transform.localScale.y += _pulseAmplitude * Mathf.Sin(Time.time * _pulseSpeed);
   	}
}