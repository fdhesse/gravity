#pragma strict


var scrollSpeed = 1.00;
var scrollSpeed2 = 2.00;

function FixedUpdate() {

    var offset = Time.time * scrollSpeed * Random.Range(1, 2);
    var offset2 = Time.time * scrollSpeed2 * Random.Range(1, 2);
	GetComponent.<Renderer>().material.mainTextureOffset = Vector3 (0,offset2,0);
}