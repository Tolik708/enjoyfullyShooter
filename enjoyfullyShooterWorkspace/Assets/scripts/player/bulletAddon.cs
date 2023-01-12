using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletAddon : MonoBehaviour
{
	public GameObject me;
	[HideInInspector]
	public weaponAsset wa;
	private float lifeTime = -1;
	private int detectCount = -1;
	private Vector3 lastVelocity;
	private Rigidbody myRb;
	void Update()
	{
		if (lifeTime < 0 && lifeTime > -1)
			StartCoroutine(destroyer());
		else if (lifeTime == -1)
		{
			lifeTime = wa.bulletLifeTime;
			lifeTime -= Time.deltaTime;
		}
		else
			lifeTime -= Time.deltaTime;
		
		lastVelocity = myRb.velocity;
	}
	
	void FixedUpdate()
	{
		if (wa.useGravity)
			myRb.AddForce(wa.gravityPower * Vector3.down, ForceMode.Acceleration);
	}
	
	void Awake()
	{
		if (myRb == null)
			myRb = me.GetComponent<Rigidbody>();
	}

    void OnCollisionEnter(Collision col)
	{
		//check for rebound
		if (wa.rebound)
		{
			if (detectCount == -1)
			{
				detectCount = wa.reboundCount-1;
				rebound(col.contacts[0].normal);
			}
			else if (detectCount > 0)
			{
				detectCount--;
				rebound(col.contacts[0].normal);
				
			}
			else
				StartCoroutine(destroyer());
		}
		//destroy if collide with certain layers
		else if (wa.collidingLayers1.Contains(col.gameObject.layer))
		{
			//deal damage
			if (col.gameObject.CompareTag("enemyBody"))
				col.gameObject.transform.parent.GetComponent<hpTest>().takeDamage(wa.bulletDamage, wa.bulletDamage, wa.bulletDamage*wa.headDamagaMultiplayer);
			else if (col.gameObject.CompareTag("enemyHead"))
				col.gameObject.transform.parent.GetComponent<hpTest>().takeDamage(wa.bulletDamage*wa.headDamagaMultiplayer, wa.bulletDamage, wa.bulletDamage*wa.headDamagaMultiplayer);
			
			me.GetComponent<SphereCollider>().enabled = false;
			me.GetComponent<MeshRenderer>().enabled = false;
			
			StartCoroutine(destroyer());
		}
	}
	
	IEnumerator destroyer()
	{
		myRb.velocity = Vector3.zero;
		lifeTime = -1;
		
		me.GetComponent<SphereCollider>().enabled = true;
		me.GetComponent<MeshRenderer>().enabled = false;
		
		me.SetActive(false);
		yield return null;
	}
	
	void rebound(Vector3 normalDir)
	{
		myRb.velocity = (lastVelocity-((2*Vector3.Dot(lastVelocity, normalDir.normalized))*normalDir.normalized));
	}
}
