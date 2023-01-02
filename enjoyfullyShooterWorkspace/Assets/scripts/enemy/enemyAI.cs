using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour
{
    [Header("patrol")]
	public Transform[] patrolPoints;
	public float patrolSpeed;
	public float delayBPoints;
	int currPoint;
	bool once;
	
	[Header("seeing")]
	bool seen;
	bool see;
	
	[Header("general")]
	public GameObject me;
	public NavMeshAgent nv;
	
	void Update()
	{
		stateMachine();
	}
	
	void Start()
	{
		nv.speed = patrolSpeed;
	}
	
	void stateMachine()
	{
		if (seen == false && nv.enabled)
			patrol();
	}
	
	//////////////////////patrol//////////////////
	void patrol()
	{
		if (Vector3.Distance(me.transform.position, patrolPoints[currPoint].position) > 0.2f)
		{
			nv.destination = patrolPoints[currPoint].position;
		}
		else if (once == false)
		{
			once = true;
			nv.destination = me.transform.position;
			Invoke("findNextPoint", delayBPoints);
		}
	}
	void findNextPoint()
	{
		if (currPoint + 1 < patrolPoints.Length)
			currPoint++;
		else
			currPoint = 0;
		once = false;
	}
}
