using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class inverseKinematics : MonoBehaviour
{
	public Transform target;
	public Transform pole;
    public int chainLength;
	
	public float delta;
	public int maxIterations;
	[Range(0, 1)]
	public float snapBackStrength;
	
	//bones
	private Transform[] bones;
	private float[] bonesLength;
	private Vector3[] startPos;
	
	//rotations
	private Vector3[] startDir;
	private Quaternion[] startRot;
	private Quaternion startTargetRot;
	
	private Transform rootParent;
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
		if (target == null)
			return;
		
		//length of hole arm
		completeLength = 0;
		
		//init variables
		boneAmm = chainLength+1;
		bonesLength = new float[chainLength];
		bones = new Transform[boneAmm];
		startPos = new Vector3[boneAmm];
		
		startDir = new Vector3[boneAmm];
		startRot = new Quaternion[boneAmm];
		startTargetRot = GetRotationRootSpace(target);
		
		
		//init root parent
		Transform curr = transform;
		for (int i = chainLength; i >= 0; i--)
		{
			rootParent = curr;
			curr = curr.parent;
		}
		
		curr = transform;
		//init other parts for bones
		for (int i = chainLength; i >= 0; i--)
		{
			bones[i] = curr;
			startRot[i] = GetRotationRootSpace(curr);
			
			//if not last bone in arm
			if (i != chainLength)
			{
				startDir[i] = GetPositionRootSpace(bones[i+1]) - GetPositionRootSpace(curr);
				bonesLength[i] = startDir[i].magnitude;
				completeLength += bonesLength[i];
			}
			else
				startDir[i] = GetPositionRootSpace(target) - GetPositionRootSpace(curr);
			
			curr = curr.parent;
		}
	}
	
	void LateUpdate()
	{
		if (target == null)
			return;
		
		//check if target or/and pole changed
		if (pole != null && pole.position == lastPolePos && lastTargetPos == target.position)
			return;
		
		if (pole == null && lastTargetPos == target.position)
			return;
		
		//get postions
		for (int i = 0; i < boneAmm; i++)
			startPos[i] = GetPositionRootSpace(bones[i]);

		for (int twice = 0; twice < 2; twice++)
		{
			//variable for readability
			Vector3 targetPosition = GetPositionRootSpace(target);
				
			//if you do not need to do any hard rotations because target is far
			if ((targetPosition - GetPositionRootSpace(bones[0])).sqrMagnitude >= completeLength*completeLength)
			{
				Vector3 dir = (targetPosition - startPos[0]).normalized;
				for (int i = 1; i < boneAmm; i++)
					startPos[i] = startPos[i-1] + (dir * bonesLength[i-1]);
			}
			else
			{
				for (int i = 0; i < chainLength; i++)
					startPos[i+1] = Vector3.Lerp(startPos[i+1], startPos[i] - startDir[i], snapBackStrength);
			
				for (int iteration = 0; iteration < maxIterations; iteration++)
				{
					//back
					for (int i = chainLength; i > 0; i--)
					{
						if (i == chainLength)
							startPos[i] = targetPosition;
						else
							startPos[i] = startPos[i+1] + (startPos[i]-startPos[i+1]).normalized * bonesLength[i];
					}
					
					//forward
					for (int i = 1; i < boneAmm; i++)
						startPos[i] = startPos[i-1] + (startPos[i]-startPos[i-1]).normalized * bonesLength[i-1];
					
					//if so close, so no need for other iterations
					if ((startPos[chainLength] - targetPosition).sqrMagnitude < delta*delta)
					{
						//Debug.Log("iterations needed: " + iteration.ToString());
						break;
					}
				}
			}
			
			if (pole != null)
			{
				for (int i = 1; i < chainLength; i++)
				{
					Plane plane = new Plane(startPos[i+1]-startPos[i-1], startPos[i-1]);
					Vector3 projectedPole = plane.ClosestPointOnPlane(GetPositionRootSpace(pole));
					Vector3 projectedBone = plane.ClosestPointOnPlane(startPos[i]);
					float angle = Vector3.SignedAngle(projectedBone - startPos[i-1], projectedPole - startPos[i-1], plane.normal);
					startPos[i] = Quaternion.AngleAxis(angle, plane.normal) * (startPos[i]-startPos[i-1]) + startPos[i-1];
				}
			}
				
			//set rotations & positions
			for (int i = 0; i < boneAmm; i++)
			{
				if (i == chainLength)
					SetRotationRootSpace(bones[i], Quaternion.Inverse(GetRotationRootSpace(target)) * startTargetRot * Quaternion.Inverse(startRot[i]));
				else
					SetRotationRootSpace(bones[i], Quaternion.FromToRotation(startDir[i], startPos[i+1] - startPos[i]) * Quaternion.Inverse(startRot[i]));
				
				SetPositionRootSpace(bones[i], startPos[i]);
			}
		}
		
		lastTargetPos = target.position;
	}
	
	private Vector3 GetPositionRootSpace(Transform current)
    {
        if (rootParent == null)
            return current.position;
        else
            return Quaternion.Inverse(rootParent.rotation) * (current.position - rootParent.position);
    }

    private void SetPositionRootSpace(Transform current, Vector3 position)
    {
        if (rootParent == null)
            current.position = position;
        else
            current.position = rootParent.rotation * position + rootParent.position;
    }

    private Quaternion GetRotationRootSpace(Transform current)
    {
        //inverse(after) * before => rot: before -> after
        if (rootParent == null)
            return current.rotation;
        else
            return Quaternion.Inverse(current.rotation) * rootParent.rotation;
    }

    private void SetRotationRootSpace(Transform current, Quaternion rotation)
    {
        if (rootParent == null)
            current.rotation = rotation;
        else
            current.rotation = rootParent.rotation * rotation;
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
