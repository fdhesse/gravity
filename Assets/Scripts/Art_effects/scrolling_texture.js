#pragma strict


var scrollSpeed = 1.00;
var scrollSpeed2 = 2.00;

function FixedUpdate() {

	var offset = Time.time * scrollSpeed;
	var offset2 = Time.time * scrollSpeed2;
	GetComponent.<Renderer>().material.mainTextureOffset = Vector3 (0,offset2,0);
}