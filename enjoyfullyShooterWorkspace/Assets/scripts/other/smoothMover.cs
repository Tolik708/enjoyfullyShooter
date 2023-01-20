using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace smoothMover
{
	/*docs: to use smoothMover you need just write "using smoothMover" at the top and call function "MoveObj"
	parametrs for "MoveObj" you can find in region "MoveObjFunctions"
	if you want move or rotate in local space and/or do not remove moving after reaching destination,
	you need to save instance after calling MoveObj and set variable isGlobal and/or to false like this
	instance myInstance = MoveObj(myObjectToMove, myEndPosition, myEndRotation, myTime, moveType.stable);
	myInstance.isGlobal = false;
	myInstance.delteAfterCame = false;
	you can also use instance for destination changing or another variables you like
	*/
	public class smoothMover : MonoBehaviour
	{
		public enum moveType {stable, lerp};
		
		private float delta = 0.01f;
		
		public class instance
		{
			public Transform moveObj;
			public moveType type = moveType.stable;
			
			public Vector3 startPos;
			public Vector3 endPos;
			
			public Quaternion startRot;
			public Quaternion endRot;
			
			public float time;
			public float timer = 0;
			
			public bool movePos = false;
			public bool moveRot = false;
			
			public bool active = true;
			public bool isGlobal;
			public bool delteAfterCame = true;
			
			public void stopMe()
			{
				active = false;
			}
		}
		
		private List<instance> ins = new List<instance>();
		
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
				
				ins[i].timer += Time.deltaTime;
				
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
								ins[i].moveObj.rotation = Quaternion.Lerp(ins[i].startRot, ins[i].endRot, ins[i].timer/ins[i].time);
							
							break;
						}
						case moveType.lerp:
						{
							if (ins[i].movePos)
								ins[i].moveObj.position = Vector3.Lerp(ins[i].moveObj.position, ins[i].endPos, ins[i].timer/ins[i].time);
							
							if (ins[i].moveRot)
								ins[i].moveObj.rotation = Quaternion.Lerp(ins[i].moveObj.rotation, ins[i].endRot, ins[i].timer/ins[i].time);
							
							break;
						}
					}
					
					//check if ins get to destination
					if ((ins[i].moveObj.position-ins[i].endPos).sqrMagnitude < delta)
					{
						ins[i].movePos = false;
						ins[i].moveObj.position = ins[i].endPos;
					}
					if (Quaternion.Angle(ins[i].moveObj.rotation, ins[i].endRot) < delta)
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
								ins[i].moveObj.localRotation = Quaternion.Lerp(ins[i].startRot, ins[i].endRot, ins[i].timer/ins[i].time);
							
							break;
						}
						case moveType.lerp:
						{
							if (ins[i].movePos)
								ins[i].moveObj.localPosition = Vector3.Lerp(ins[i].moveObj.localPosition, ins[i].endPos, ins[i].timer/ins[i].time);
							
							if (ins[i].moveRot)
								ins[i].moveObj.localRotation = Quaternion.Lerp(ins[i].moveObj.localRotation, ins[i].endRot, ins[i].timer/ins[i].time);
							
							break;
						}
					}
					
					//check if ins get to destination
					if ((ins[i].moveObj.localPosition - ins[i].endPos).sqrMagnitude < delta)
					{
						ins[i].movePos = false;
						ins[i].moveObj.localPosition = ins[i].endPos;
					}
					if (Quaternion.Angle(ins[i].moveObj.localRotation, ins[i].endRot) < delta)
					{
						ins[i].moveRot = false;
						ins[i].moveObj.localRotation = ins[i].endRot;
					}
				}
				//remove object from list if needed && it came to destination
				if (ins[i].delteAfterCame && !ins[i].moveRot && !ins[i].movePos)
					ins.RemoveAt(i);
			}
		}
		
		#region MoveObjFunctions
		public instance MoveObj(Transform objectToMove, Vector3 endPos, Quaternion endRot, float time)
		{
			instance newIns = new instance();
			newIns.moveObj = objectToMove;
			newIns.startPos = objectToMove.position;
			newIns.startRot = objectToMove.rotation;
			newIns.endPos = endPos;
			newIns.endRot = endRot;
			newIns.time = time;
			newIns.movePos = true;
			newIns.moveRot = true;
			return newIns;
		}
		public instance MoveObj(Transform objectToMove, Quaternion endRot, float time)
		{
			instance newIns = new instance();
			newIns.moveObj = objectToMove;
			newIns.startRot = objectToMove.rotation;
			newIns.endRot = endRot;
			newIns.moveRot = true;
			newIns.time = time;
			return newIns;
		}
		public instance MoveObj(Transform objectToMove, Vector3 endPos, float time)
		{
			instance newIns = new instance();
			newIns.moveObj = objectToMove;
			newIns.startPos = objectToMove.position;
			newIns.endPos = endPos;
			newIns.movePos = true;
			newIns.time = time;
			return newIns;
		}
		
		public instance MoveObj(Transform objectToMove, Vector3 endPos, Quaternion endRot, float time, moveType type)
		{
			instance newIns = new instance();
			newIns.moveObj = objectToMove;
			newIns.startPos = objectToMove.position;
			newIns.startRot = objectToMove.rotation;
			newIns.endPos = endPos;
			newIns.endRot = endRot;
			newIns.time = time;
			newIns.type = type;
			newIns.movePos = true;
			newIns.moveRot = true;
			return newIns;
		}
		public instance MoveObj(Transform objectToMove, Quaternion endRot, float time, moveType type)
		{
			instance newIns = new instance();
			newIns.moveObj = objectToMove;
			newIns.startRot = objectToMove.rotation;
			newIns.endRot = endRot;
			newIns.moveRot = true;
			newIns.time = time;
			newIns.type = type;
			return newIns;
		}
		public instance MoveObj(Transform objectToMove, Vector3 endPos, float time, moveType type)
		{
			instance newIns = new instance();
			newIns.moveObj = objectToMove;
			newIns.startPos = objectToMove.position;
			newIns.endPos = endPos;
			newIns.movePos = true;
			newIns.time = time;
			newIns.type = type;
			return newIns;
		}
		#endregion
	}
}
