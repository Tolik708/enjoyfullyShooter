using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class inverseKinematics : MonoBehaviour
{
	public Transform target;
	public Transform pole;
    public int chainLength;
	
	public float delta;
	public int iterations;
	
	//bones
	private Transform[] bones;
	private float[] bonesLength;
	private Vector3[] startPos;
	
	//rotations
	private Vector3[] startDir;
	private Quaternion[] startRot;
	private Quaternion startTargetRot;
	private Quaternion startRotRoot;
	
	private Vector3 lastTargetPos;
	private Vector3 lastPolePos;
	private float completeLength;
	private int boneAmm;
	
	void Awake()
	{
		if (target != null)
			init();
	}
	
	public void init()
	{
		//length of hole arm
		completeLength = 0;
		
		//init variables
		boneAmm = chainLength+1;
		bonesLength = new float[chainLength];
		bones = new Transform[boneAmm];
		startPos = new Vector3[boneAmm];
		
		startDir = new Vector3[boneAmm];
		startRot = new Quaternion[boneAmm];
		startTargetRot = target.localRotation;
		startRotRoot = transform.rotation;
		
		Transform curr = transform;
		for (int i = chainLength; i >= 0; i--)
		{
			bones[i] = curr;
			startRot[i] = curr.rotation;
			
			//if not last bone in arm
			if (i != chainLength)
			{
				startDir[i] = bones[i+1].position - curr.position;
				bonesLength[i] = (bones[i+1].position - curr.position).magnitude;
				completeLength += bonesLength[i];
			}
			else
			{
				startDir[i] = target.position - curr.position;
			}
			
			curr = curr.parent.transform;
		}
	}
	
	void LateUpdate()
	{
		if (target == null)
			return;
		
		if (pole != null && pole.position == lastPolePos && lastTargetPos == target.position)
			return;
		
		if (pole == null && lastTargetPos == target.position)
			return;
		
		//get postions
		for (int i = 0; i < boneAmm; i++)
			startPos[i] = bones[i].position;
		
		Quaternion rootRot = (bones[0].parent != null)? bones[0].parent.rotation : Quaternion.identity;
		Quaternion rootRotDiff = rootRot * Quaternion.Inverse(startRotRoot);
		
		//if you do not need to do any hard rotations
		if ((target.position - bones[0].position).sqrMagnitude > completeLength*completeLength)
		{
			Vector3 dir = (target.position - bones[0].position).normalized;
			for (int i = 1; i < boneAmm; i++)
				startPos[i] = bones[i-1].position + dir * bonesLength[i-1];
		}
		else
		{
			for (int i = 0; i < iterations; i++)
			{
				//back
				for (int u = chainLength; u > 0; u--)
				{
					if (u == chainLength)
						startPos[u] = target.position;
					else
						startPos[u] = startPos[u+1] + (startPos[u]-startPos[u+1]).normalized * bonesLength[u];
				}
				
				//forward
				for (int u = 1; u < boneAmm; u++)
					startPos[u] = startPos[u-1] + (startPos[u]-startPos[u-1]).normalized * bonesLength[u-1];
				
				
				//if so close, so no need for other iterations
				if ((target.position-startPos[chainLength]).sqrMagnitude < delta*delta)
					break;
			}
			
			if (pole != null)
			{
				for (int i = 1; i < chainLength; i++)
				{
					Plane plane = new Plane(startPos[i+1]-startPos[i-1], startPos[i-1]);
					Vector3 projectedPole = plane.ClosestPointOnPlane(pole.position);
					Vector3 projectedBone = plane.ClosestPointOnPlane(startPos[i]);
					float angle = Vector3.SignedAngle(projectedBone - startPos[i-1], projectedPole - startPos[i-1], plane.normal);
					startPos[i] = Quaternion.AngleAxis(angle, plane.normal) * (startPos[i]-startPos[i-1]) + startPos[i-1];
				}
			}
		}
		
		//set rotations
		for (int i = 0; i < boneAmm; i++)
		{
			if (i == chainLength)
				bones[i].rotation = target.localRotation * Quaternion.Inverse(startTargetRot) * startRot[i];
			else
				bones[i].rotation = Quaternion.FromToRotation(startDir[i], startPos[i+1] - startPos[i]) * startRot[i];
		}
		
		lastTargetPos = target.position;
		
		//set positions
		for (int i = 0; i < boneAmm; i++)
			bones[i].position = startPos[i];
	}
	
	void OnDrawGizmos()
	{
		Transform curr = transform;
		for (int i = 0; i < chainLength && curr.parent != null && curr != null; i++)
		{
			float scale = Vector3.Distance(curr.position, curr.parent.position) * 0.1f;
			Handles.matrix = Matrix4x4.TRS(curr.position, Quaternion.FromToRotation(Vector3.up, curr.parent.position - curr.position), new Vector3(scale, Vector3.Distance(curr.position, curr.parent.position), scale));
			Handles.color = Color.green;
			Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
			curr = curr.parent;
		}
	}
}
