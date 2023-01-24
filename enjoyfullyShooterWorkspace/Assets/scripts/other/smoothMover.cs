using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/*docs: to use smoothMover you need just make reference to a script and call function "MoveObj" from it
parametrs for "MoveObj" you can find in region "MoveObjFunctions"
if you want move or rotate in local space,
you need to save instance after calling MoveObj and set variable isGlobal to false like this:
smoothMover.instance myInstance = MoveObj(myObjectToMove, myEndPosition, myEndRotation, myTime, smoothMover.moveType.stable);
myInstance.isGlobal = false;
you can also use instance for destination changing or another variables you like. See in instance class.
*/

/* another great example (in coroutine)
//moving to dynamic target in global space (works even if it is child)
currWea.transform.parent = weaPos;
smoothMover.instance ins = sm.MoveObj(currWea.transform, weaPos.transform.position, 2, smoothMover.moveType.stableLerp);
ins.dynamicTarget = weaPos.transform;

//rotating in local space
smoothMover.instance ins1 = sm.RotObj(currWea.transform, Quaternion.identity, 2, smoothMover.moveType.lerp);
ins1.global(false);
//wait while all two instances finish
while (ins.active && ins1.active)
	yield return 0;
*/
public class smoothMover : MonoBehaviour
{
	public enum moveType {stable, lerp, stableLerp};
	/*move type discription:
	stable - always sertain amount of time to get to the target even if target is dynamic
	lerp - time means percent of path object makes at a frame and each frame start postion sets to its current position
	stableLerp - is same as lerp but need always same amount of time to get to the target */
	
	private float delta = 0.01f;
	
	public class instance
	{
		//object that need to move
		public Transform moveObj;
		//move method
		public moveType type = moveType.stable;
		
		//start position of moving 
		public Vector3 startPos; //sets aautomaticaly
		//destination of object's position
		public Vector3 endPos;
		
		//start rotation of object
		public Quaternion startRot; //sets automaticaly
		//destination of object's rotation
		public Quaternion endRot;
		
		//if != null object will go to the dynamicTarget's rotation or/and position
		public Transform dynamicTransform;
		public bool dynamicPos;
		public bool dynamicRot;
		
		//adds additional position or/and rotation after all calculations (not recomended for use with lerp- move types)
		public Vector3 additionalPos;
		public Quaternion additionalRot;
		
		////time that object need to trevel from start to end. If move type == lerp time means percent of path object makes at a frame
		public float time;
		//timer uses to keep track of how far this object get already
		public float timer = 0; //sets automaticaly
		
		//for knowing if we still need to move in position this object
		public bool movePos = false; //sets automaticaly
		//for knowing if we still need to move in rotation this object
		public bool moveRot = false; //sets automaticaly
		
		//set to false if you do not want to stop movement after reaching destination
		public bool stopPos = true;
		//set to false if you do not want to stop rotation after reaching destination
		public bool stopRot = true;
		
		//for checking if this object need to exist in list
		public bool active = true;
		//system coordinates to use
		public bool isGlobal = true;
		
		public void stopMe()
		{
			active = false;
		}
		public void global(bool choice)
		{
			if (choice == true)
			{
				startPos = moveObj.position;
				startRot = moveObj.rotation;
			}
			else
			{
				startPos = moveObj.localPosition;
				startRot = moveObj.localRotation;
			}
			
			isGlobal = choice;
		}
	}
	
	public List<instance> ins = new List<instance>();
	
	void Update()
	{
		for (int i = 0; i < ins.Count; i++)
		{
			//check if it wasn't removed
			if (!ins[i].active)
			{
				ins.RemoveAt(i);
				continue;
			}
			
			ins[i].timer = Mathf.Clamp(ins[i].timer + Time.deltaTime, 0, ins[i].time);
			
			if (ins[i].movePos && ins[i].dynamicPos)
				ins[i].endPos = ins[i].dynamicTransform.position;
				
			if (ins[i].moveRot && ins[i].dynamicRot)
				ins[i].endRot = ins[i].dynamicTransform.rotation;
			
			//global
			if (ins[i].isGlobal)
			{
				switch (ins[i].type)
				{
					case moveType.stable:
					{
						if (ins[i].movePos)
							ins[i].moveObj.position = Vector3.Lerp(ins[i].startPos, ins[i].endPos, ins[i].timer/ins[i].time);
						
						if (ins[i].moveRot)
							ins[i].moveObj.rotation = Quaternion.Slerp(ins[i].startRot, ins[i].endRot, ins[i].timer/ins[i].time);
						
						break;
					}
					case moveType.stableLerp:
					{
						if (ins[i].movePos)
							ins[i].moveObj.position = Vector3.Lerp(ins[i].moveObj.position, ins[i].endPos, ins[i].timer/ins[i].time);
						
						if (ins[i].moveRot)
							ins[i].moveObj.rotation = Quaternion.Slerp(ins[i].moveObj.rotation, ins[i].endRot, ins[i].timer/ins[i].time);
						
						break;
					}
					case moveType.lerp:
					{
						if (ins[i].movePos)
							ins[i].moveObj.position = Vector3.Lerp(ins[i].moveObj.position, ins[i].endPos, ins[i].time/100);
						
						if (ins[i].moveRot)
							ins[i].moveObj.rotation = Quaternion.Slerp(ins[i].moveObj.rotation, ins[i].endRot, ins[i].time/100);
						
						break;
					}
				}
				
				//check if ins get to destination
				if (ins[i].stopPos && (ins[i].moveObj.position-ins[i].endPos).sqrMagnitude < delta)
				{
					ins[i].movePos = false;
					ins[i].moveObj.position = ins[i].endPos;
				}
				
				if (ins[i].stopRot && Quaternion.Angle(ins[i].moveObj.rotation, ins[i].endRot) < delta)
				{
					ins[i].moveRot = false;
					ins[i].moveObj.rotation = ins[i].endRot;
				}
			}
			//local
			else
			{
				switch (ins[i].type)
				{
					case moveType.stable:
					{
						if (ins[i].movePos)
							ins[i].moveObj.localPosition = Vector3.Lerp(ins[i].startPos, ins[i].endPos, ins[i].timer/ins[i].time);
						
						if (ins[i].moveRot)
							ins[i].moveObj.localRotation = Quaternion.Slerp(ins[i].startRot, ins[i].endRot, ins[i].timer/ins[i].time);
						
						break;
					}
					case moveType.stableLerp:
					{
						if (ins[i].movePos)
							ins[i].moveObj.localPosition = Vector3.Lerp(ins[i].moveObj.localPosition, ins[i].endPos, ins[i].timer/ins[i].time);
						
						if (ins[i].moveRot)
							ins[i].moveObj.localRotation = Quaternion.Slerp(ins[i].moveObj.localRotation, ins[i].endRot, ins[i].timer/ins[i].time);
						
						break;
					}
					case moveType.lerp:
					{
						if (ins[i].movePos)
							ins[i].moveObj.localPosition = Vector3.Lerp(ins[i].moveObj.localPosition, ins[i].endPos, Time.deltaTime*ins[i].time);
						
						if (ins[i].moveRot)
							ins[i].moveObj.localRotation = Quaternion.Slerp(ins[i].moveObj.localRotation, ins[i].endRot, Time.deltaTime*ins[i].time);
						
						break;
					}
				}
				
				//check if ins get to destination
				if (ins[i].stopPos && (ins[i].moveObj.localPosition - ins[i].endPos).sqrMagnitude < delta*delta)
				{
					ins[i].movePos = false;
					ins[i].moveObj.localPosition = ins[i].endPos;
				}
				
				if (ins[i].stopRot && Quaternion.Angle(ins[i].moveObj.localRotation, ins[i].endRot) < delta*delta)
				{
					ins[i].moveRot = false;
					ins[i].moveObj.localRotation = ins[i].endRot;
				}
			}
			//add additional
			if (ins[i].additionalPos != null)
				ins[i].moveObj.position += ins[i].additionalPos;
			if (ins[i].additionalRot != null)
				ins[i].moveObj.rotation *= ins[i].additionalRot;
			
			//remove object from list if needed && it came to destination
			if (!ins[i].moveRot && !ins[i].movePos)
			{
				ins[i].active = false;
				ins.RemoveAt(i);
			}
		}
	}
	
	#region MoveObjFunctions
	private void initInstance(instance newIns, Transform objectToMove, Vector3 endPos, Quaternion endRot, float time, moveType type, bool usePos, bool useRot)
	{
		newIns.moveObj = objectToMove;
		
		newIns.startPos = objectToMove.position;
		newIns.endPos = endPos;
		newIns.movePos = usePos;
		
		newIns.startRot = objectToMove.rotation;
		newIns.endRot = endRot;
		newIns.moveRot = useRot;
		
		newIns.type = type;
		newIns.time = time;
		ins.Add(newIns);
	}
	public instance MoveObj(Transform objectToMove, Vector3 endPos, Quaternion endRot, float time) //rot and pos
	{
		instance newIns = new instance();
		initInstance(newIns, objectToMove, endPos, endRot, time, moveType.stable, true, true);
		return ins[ins.Count-1];
	}
	public instance RotObj(Transform objectToMove, Quaternion endRot, float time) //only rot
	{
		instance newIns = new instance();
		initInstance(newIns, objectToMove, Vector3.zero, endRot, time, moveType.stable, false, true);
		return ins[ins.Count-1];
	}
	public instance MoveObj(Transform objectToMove, Vector3 endPos, float time) //only pos
	{
		instance newIns = new instance();
		initInstance(newIns, objectToMove, endPos, Quaternion.identity, time, moveType.stable, true, false);
		return ins[ins.Count-1];
	}
	
	//with move type
	public instance MoveObj(Transform objectToMove, Vector3 endPos, Quaternion endRot, float time, moveType type) //pos and rot
	{
		instance newIns = new instance();
		initInstance(newIns, objectToMove, endPos, endRot, time, type, true, true);
		return ins[ins.Count-1];
	}
	public instance RotObj(Transform objectToMove, Quaternion endRot, float time, moveType type) //only rot
	{
		instance newIns = new instance();
		initInstance(newIns, objectToMove, Vector3.zero, endRot, time, type, false, true);
		return ins[ins.Count-1];
	}
	public instance MoveObj(Transform objectToMove, Vector3 endPos, float time, moveType type) //only pos
	{
		instance newIns = new instance();
		initInstance(newIns, objectToMove, endPos, Quaternion.identity, time, type, true, false);
		return ins[ins.Count-1];
	}
	#endregion
}