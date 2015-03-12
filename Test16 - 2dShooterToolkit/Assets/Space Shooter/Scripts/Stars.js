#pragma strict
var _speed:float = 1;

function Update () {
	transform.position = Vector3.Lerp(transform.position, Player.instance.transform.position*-1, Time.deltaTime*_speed);	//Move the stars negatively based on player position to simulate paralax scroll
}