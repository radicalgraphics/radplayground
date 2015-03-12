#pragma strict
//Simple script to rotate a gameObject
var xRot:float;
var yRot:float;
var zRot:float;

function Update () {
	transform.Rotate(Vector3(xRot,yRot,zRot)*Time.deltaTime);
}