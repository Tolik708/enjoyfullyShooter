using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyFall : MonoBehaviour
{
    player p;
	Rigidbody rb;
	enemyAI e;
	public GameObject me;
	public float powerOfFall;
	public float powerOfTorque;
	public float delayToT;
	
	
    void Start()
    {
		rb = me.GetComponent<Rigidbody>();
		e = me.GetComponent<enemyAI>();
        p = GameObject.Find("/Player/player").GetComponent<player>();
    }
	
	void OnTriggerEnter(Collider col)
	{
		if (col.gameObject.name == "player" && p.sliding == true)
		{
			rb.AddForce(Vector3.up * powerOfFall, ForceMode.Impulse);
			Invoke("torqu", delayToT);
			e.nv.enabled = false;
		}
	}
	
	void torqu()
	{
		rb.AddTorque(transform.up * powerOfTorque);
	}
}
