using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class graplingGun : MonoBehaviour
{
	[Header("general")]
	public float checkRadius;
	public float checkDist;
	public float ropeTime;
	public Rigidbody rb;
	public Transform cam;
	public Transform gunTip;
	public Transform me;
	public Transform orientation;
	public LineRenderer lr;
	private player pm;
	
	[Header("swining")]
	public float swingStartDelay;
	public float jointSpring;
	public float jointDamper;
	public float jointMassScale;
	public float swingVelocity;
	public float swingSpeedExtend;
	public float swingSideThrust;
	public float swingForwardThrust;
	public LayerMask swingLayer;
	public LayerMask swingLayer1;
	public KeyCode swingKey;
	public SpringJoint joint;
	[HideInInspector]
	public bool swing;
	private float swingTimer;
	private float startVelocity;
	
	[Header("G")]
	public KeyCode GKey;
	public float GStartDelay;
	public float overshoot;
	public LayerMask GLayer;
	public LayerMask GLayer2;
	private float GTimer;
	private float h;
	private float gravity;
	private bool G;
	[HideInInspector]
	public bool activeGrapple;
	private Vector3 GPoint;
	private Vector3 target;
	
	[Header("Rope")]
	public float damper;
	public float waveHeight;
	public float waveCount;
	public float strangth;
	public float velocity;
	public float flyRopeSpeed;
	public int quality;
	public Spring spr;
	public AnimationCurve[] affectCurve;
	private int randCurve = 0;
	private Vector3 currGrapplePos;
	
	
    void Update()
	{
		if (Input.GetKeyDown(GKey) && !(G || swing))
			StartG();
		
		if (Input.GetKeyDown(swingKey) && !G)
			StartSwing();
		if (Input.GetKeyUp(swingKey) && swing)
			StopSwing();
		
		if (GTimer >= 0)
			GTimer -= Time.deltaTime;
		
		if (swingTimer >= 0)
			swingTimer -= Time.deltaTime;
		
		if (activeGrapple)
		{
			if (Vector3.Distance(me.position, target) < 8)
			{
				activeGrapple = false;
				StopG();
			}
		}
		
		if (joint)
		{
			swingMovement();
		}
	}
	
	void LateUpdate()
	{
		DrawRope();
	}
	
	void Awake()
	{
		startVelocity = velocity;
		pm = GameObject.Find("/Player/player").GetComponent<player>();
		spr = GameObject.Find("/Player/player").GetComponent<Spring>();
		spr.SetTarget(0);
	}
	
	////////////swing//////////
	void StartSwing()
	{
		RaycastHit hat;
		lr.enabled = true;
		lr.positionCount = 0;
		swing = true;
		if (Physics.Raycast(cam.position, cam.forward, out hat, checkDist, swingLayer1) && swingTimer < 0)
		{
			randCurve = Random.Range(0, affectCurve.Length);
			GPoint = hat.point;
			Invoke("StopSwing", ropeTime);
		}
		else if (Physics.SphereCast(cam.position, checkRadius, cam.forward, out hat, checkDist, swingLayer) && swingTimer < 0)
		{
			randCurve = Random.Range(0, affectCurve.Length);
			GPoint = hat.point;
			realStart();
		}
		else if (Physics.SphereCast(cam.position, checkRadius, cam.forward, out hat, checkDist, swingLayer) && swingTimer < 0)
		{
			randCurve = Random.Range(0, affectCurve.Length);
			GPoint = hat.point;
			realStart();
		}
		else if (swingTimer < 0)
		{
			randCurve = Random.Range(0, affectCurve.Length);
			GPoint = cam.position + cam.forward * checkDist;
			Invoke("StopSwing", ropeTime);
		}
		else
		{
			lr.enabled = false;
			swing = false;
		}
	}
	
	void realStart()
	{
		pm.swingSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
		velocity = swingVelocity;
		joint = me.gameObject.AddComponent<SpringJoint>();
		joint.autoConfigureConnectedAnchor = false;
		joint.connectedAnchor = GPoint;
		float dist = Vector3.Distance(me.position, GPoint);
		
		joint.maxDistance = dist * 0.8f;
		joint.minDistance = dist * 0.25f;
		
		joint.spring = jointSpring;
		joint.damper = jointDamper;
		joint.massScale = jointMassScale;
	}
	
	void swingMovement()
	{
		if (Input.GetKey(KeyCode.LeftShift))
		{
			float distBMeGPoint = Vector3.Distance(GPoint, me.position) + swingSpeedExtend;
			joint.maxDistance = distBMeGPoint * 0.8f;
			joint.minDistance = distBMeGPoint * 0.25f;
		}
		else if (Input.GetKey(KeyCode.Space))
		{
			Vector3 dirToGPoint = GPoint - me.position;
			rb.AddForce(dirToGPoint * swingForwardThrust * Time.deltaTime);
			
			float distBMeGPoint = Vector3.Distance(GPoint, me.position);
			joint.maxDistance = distBMeGPoint * 0.8f;
			joint.minDistance = distBMeGPoint * 0.25f;
		}
	}
	
	void StopSwing()
	{
		velocity = startVelocity;
		pm.jumpAmm = pm.ammountOfJumps;
		swing = false;
		swingTimer = swingStartDelay;
		pm.speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
		Destroy(joint);
	}
	
	////////////G//////////////
	void StartG()
	{
		G = true;
		RaycastHit hit;
		lr.positionCount = 0;
		lr.enabled = true;
		if (Physics.Raycast(cam.position, cam.forward, out hit, checkDist, GLayer2) && GTimer < 0)
		{
			randCurve = Random.Range(0, affectCurve.Length);
			GPoint = hit.point;
			Invoke("StopG", ropeTime);
		}
		else if (Physics.Raycast(cam.position, cam.forward, out hit, checkDist, GLayer) && GTimer < 0)
		{
			randCurve = Random.Range(0, affectCurve.Length);
			pm.freeze = true;
			target = GPoint = hit.point;
			Invoke("ExecuteG", ropeTime);
		}
		else if (Physics.SphereCast(cam.position, checkRadius, cam.forward, out hit, checkDist, GLayer) && GTimer < 0)
		{
			randCurve = Random.Range(0, affectCurve.Length);
			pm.freeze = true;
			target = GPoint = hit.point;
			Invoke("ExecuteG", ropeTime);
		}
		else if (GTimer < 0)
		{
			randCurve = Random.Range(0, affectCurve.Length);
			GPoint = cam.position + cam.forward * checkDist;
			Invoke("StopG", ropeTime);
		}
		else
		{
			lr.positionCount = 2;
			G = false;
		}
			
	}
	
	void ExecuteG()
	{
		pm.freeze = false;
		activeGrapple = true;
		
		Vector3 lowestPoint = new Vector3(me.position.x, me.position.y - 1, me.position.z);
		h = target.y - lowestPoint.y;
		if ((target.y - lowestPoint.y) < 0) h = overshoot;
		
		rb.velocity = calculateVelocity();
	}
	
	public void StopG()
	{
		pm.jumpAmm = pm.ammountOfJumps;
		pm.freeze = false;
		G = false;
		GTimer = GStartDelay;
		pm.speed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;
	}
	
	
	Vector3 calculateVelocity()
	{
		gravity = -9.81f;
		float displacementY = target.y - me.position.y;
		Vector3 displacementXZ = new Vector3(target.x - me.position.x, 0, target.z - me.position.z);
		
		Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * h);
		Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * h / gravity) + Mathf.Sqrt(2 * (displacementY - h) / gravity));
		return velocityXZ + velocityY;
	}
	
	
	////////////////////////ROPE/////////////
	void DrawRope()
	{
		if (!G && !swing) 
		{
			currGrapplePos = GPoint = Vector3.Lerp(GPoint, gunTip.position, Time.deltaTime * 10);
			if (Vector3.Distance(gunTip.position, GPoint) < 1)
			{
				lr.enabled = false;
				lr.positionCount = 0;
			}
			else
			{
				lr.positionCount = 2;
				lr.SetPosition(0, gunTip.position);
				lr.SetPosition(1, GPoint);
			}
			spr.Reset();
			return;
		}

		if (lr.positionCount == 0)
		{
			spr.SetVelocity(velocity);
			lr.positionCount = quality + 1;
		}
		
		spr.SetDamper(damper);
		spr.SetStrength(strangth);
		spr.update(Time.deltaTime);
		
		Vector3 up = Quaternion.LookRotation(GPoint - gunTip.position).normalized * Vector3.up;
		
		currGrapplePos = Vector3.Lerp(currGrapplePos, GPoint, Time.deltaTime * flyRopeSpeed);
		
		for (int i = 0; i < quality + 1; i++)
		{
			float delta = i / (float)quality;
			Vector3 offset = up * waveHeight * Mathf.Sin(delta * waveCount * Mathf.PI) * spr.Value * affectCurve[randCurve].Evaluate(delta);
			lr.SetPosition(i, Vector3.Lerp(gunTip.position, currGrapplePos, delta) + offset);
		}
	}
}
