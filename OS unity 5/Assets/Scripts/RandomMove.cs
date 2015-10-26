using UnityEngine;
using System.Collections;

public class RandomMove : MonoBehaviour {
	
	public float speed = 1.0f;
//  public float angularSpeed = 1.0f;
	
//	protected bool invertedRotation = false;
	
    //protected Vector3 eulerAngleVelocity = new Vector3(0, 100, 0);

	// Use this for initialization
	
	protected int layerMask = 1;
	public int duckMask = 21;
	private bool isMooving = false;
	
	void Start () {
		layerMask  = 1 << duckMask;					
	}
	
	void Awake ()
	{
		float x = Random.value*2.0f-1.0f;
		float z = Random.value*2.0f-1.0f;
		GetComponent<Rigidbody>().velocity = new Vector3 (x*speed, 0, z*speed);
		//if(!invertedRotation)
		//	rigidbody.AddTorque(Vector3.up * 10 * speed);
		//	rigidbody.AddForce
	} 
	
	// Update is called once per frame
	void FixedUpdate () {	
	//	GetDuck();
	}
	
	private void GetDuck()
	{	
		if(Input.GetMouseButton(0)&& !isMooving)
		{	
			isMooving = true;		
			RaycastHit hitm = new RaycastHit ();
			Physics.Raycast(
				Camera.main.ScreenPointToRay (Input.mousePosition), 
				out hitm, 
				Mathf.Infinity, layerMask);
			
				//	Debug.Log("raycast");
			//if(hitm.collider.attachedRigidbody.layer==21)
			{
				
				if(hitm.collider!= null)
				{	
					if(hitm.collider.attachedRigidbody == this.GetComponent<Rigidbody>())
					{
				        hitDuck();
					}	
			}
			}
		}
		if(isMooving && !Input.GetMouseButton(0)){
			isMooving = false;	
		//	Debug.Log("reset");
		}
	}
	
	private void hitDuck()
	{
		GetComponent<AudioSource>().Play();   
		float radius = 5.0F;
		float power = 100.0F;		
        Vector3 explosionPos = transform.position;
		
		float x = Random.value*2.0f-1.0f;
		float y = 0.0f;
		float z = Random.value*2.0f-1.0f;
		explosionPos = new Vector3 (x, y, z);
		explosionPos.Normalize();
		
		//rigidbody.AddExplosionForce(power, explosionPos, radius, 3.0F);
		
        GetComponent<Rigidbody>().AddForce(explosionPos*power);
		//Debug.Log(explosionPos);
	}
	
	
  /*  void FixedUpdate() {
		
		if(!invertedRotation)
		{
			Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime * angularSpeed);
	        rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);
		}
		else
		{
			Quaternion deltaRotation = Quaternion.Euler(-eulerAngleVelocity * Time.deltaTime * angularSpeed);
	        rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);
		}
    }*/

	/*void OnTriggerEnter(Collider  collision)
	{
		//collision.
		Debug.Log("OnTriggerEnter");
		Vector3 vectIni  = -rigidbody.velocity;
		Quaternion quat = Quaternion.AngleAxis(90.0f, Vector3.up);
		
		//rigidbody.velocity = quat * vectIni;	
		invertedRotation = !invertedRotation;
     
	}*/
	
	
	  void OnCollisionEnter(Collision collision) 
	{
		//Debug.Log(collision.contacts.Length);
		//int i=0;
        foreach (ContactPoint contact in collision.contacts) {	
		//Debug.Log(i++);	
			Vector3 vectIni  = -GetComponent<Rigidbody>().velocity;
			float angle = Vector3.Angle(vectIni,contact.normal);
			Vector3 vectNew = Quaternion.AngleAxis(angle,Vector3.up) * contact.normal;
			vectNew  = - contact.normal;
			vectNew.Normalize();
			vectNew = vectNew * speed;
			GetComponent<Rigidbody>().velocity = vectNew;
			//Debug.Log(vectNew);	
			break;
        }
		GetComponent<AudioSource>().Play();        
    }

}
