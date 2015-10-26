var speed = Vector3(20.0,15.0,5.0);

function Update () {
	transform.localEulerAngles = speed * Time.time;
}
