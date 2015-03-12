#pragma strict
var _music:AudioClip;

function Start () {
	yield(WaitForSeconds(.1));
		if(_music) SoundController.instance.PlayMusic(_music, 1, 1, true);
	}