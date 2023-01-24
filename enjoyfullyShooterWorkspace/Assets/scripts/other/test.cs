using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
	public smoothMover sm;
	
	void Start()
	{
		smoothMover.instance ins = sm.MoveObj(transform, new Vector3(transform.position.x + 10, transform.position.y, transform.position.z), Quaternion.identity, 10, smoothMover.moveType.lerp);
	}
}
