using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorSystem : MonoBehaviour
{
	public LayerMask playerLayer;
	public level lv;
	
    [System.Serializable]
    public class door{
        public GameObject doorObj;
		public Transform checkPos;
		public Vector3 checkSize;
		public Vector3 openPos;
		public float openSpeed;
		public float closeSpeed;
		public bool closeAfterExit;
		public bool lerpOpen;
		public bool lockDoor;
		public bool genDoor;
		public bool canOpen;
		//hide
		public Vector3 normPos = Vector3.zero;
		public float delta = 0;
		public bool wasOpen = false;
		public bool nowOpen = false;
		public bool mustClose = false;
    }
 
    public List<door> doors = new List<door>();
	
	void Start()
	{
		for (int i = 0; i < doors.Count; i++)
			Check(i);
	}
	
	public void Check(int i)
	{
		doors[i].checkSize = new Vector3(doors[i].checkSize.x/2, doors[i].checkSize.y/2, doors[i].checkSize.z/2);
		doors[i].normPos = doors[i].doorObj.transform.localPosition;
	}
	
	void Update()
	{
		for (int i = 0; i < doors.Count; i++)
		{
			doors[i].nowOpen = false;
			///////////////checkForPlayer////////////////
			if (Physics.OverlapBox(doors[i].checkPos.position, doors[i].checkSize, Quaternion.identity, playerLayer).Length > 0 && !doors[i].mustClose)
			{
				doors[i].nowOpen = true;
				doors[i].wasOpen = true;
				if (doors[i].genDoor)
				{
					doors[i].genDoor = false;
					lv.genElevAndExitDoor();
				}
			}
			else if (doors[i].mustClose)
			{
				doors[i].nowOpen = false;
				doors[i].wasOpen = false;
			}
			
			////////////////openAndClose///////////////////
			if (doors[i].closeAfterExit)
			{
				if (doors[i].nowOpen && doors[i].canOpen)
				{
					if (doors[i].lerpOpen)
					{
						doors[i].doorObj.transform.localPosition = Vector3.Lerp(doors[i].doorObj.transform.localPosition, doors[i].openPos, Time.deltaTime * doors[i].openSpeed);
					}
					else
					{
						doors[i].delta++;
						if (doors[i].delta > doors[i].openSpeed)
							doors[i].delta = doors[i].openSpeed;
						doors[i].doorObj.transform.localPosition = Vector3.Lerp(doors[i].normPos, doors[i].openPos, doors[i].delta / doors[i].openSpeed);
					}
				}
				else
				{
					if (doors[i].lerpOpen)
					{
						if (doors[i].lockDoor)
							doors[i].canOpen = false;
						doors[i].doorObj.transform.localPosition = Vector3.Lerp(doors[i].doorObj.transform.localPosition, doors[i].normPos, doors[i].closeSpeed * Time.deltaTime);
					}
					else
					{
						if (doors[i].lockDoor)
							doors[i].canOpen = false;
						doors[i].delta--;
						if (doors[i].delta > doors[i].closeSpeed)
							doors[i].delta = ((doors[i].closeSpeed / 100) * (doors[i].delta / (doors[i].openSpeed / 100)));
						if (doors[i].delta < 0)
							doors[i].delta = 0;
						doors[i].doorObj.transform.localPosition = Vector3.Lerp(doors[i].normPos, doors[i].openPos, doors[i].delta / doors[i].closeSpeed);
					}
				}
			}
			else if (doors[i].wasOpen && doors[i].canOpen)
			{
				if (doors[i].lerpOpen && doors[i].canOpen)
				{
					doors[i].doorObj.transform.localPosition = Vector3.Lerp(doors[i].doorObj.transform.localPosition, doors[i].openPos, Time.deltaTime * doors[i].openSpeed);
				}
				else if (doors[i].canOpen)
				{
					doors[i].delta++;
					if (doors[i].delta > doors[i].openSpeed)
						doors[i].delta = doors[i].openSpeed;
					doors[i].doorObj.transform.localPosition = Vector3.Lerp(doors[i].normPos, doors[i].openPos, doors[i].delta / doors[i].openSpeed);
				}
			}
			else
			{
				if (doors[i].lerpOpen)
				{
					if (doors[i].lockDoor)
						doors[i].canOpen = false;
					doors[i].doorObj.transform.localPosition = Vector3.Lerp(doors[i].doorObj.transform.localPosition, doors[i].normPos, doors[i].closeSpeed * Time.deltaTime);
				}
				else
				{
					if (doors[i].lockDoor)
						doors[i].canOpen = false;
					doors[i].delta--;
					if (doors[i].delta > doors[i].closeSpeed)
						doors[i].delta = ((doors[i].closeSpeed / 100) * (doors[i].delta / (doors[i].openSpeed / 100)));
					if (doors[i].delta < 0)
						doors[i].delta = 0;
					doors[i].doorObj.transform.localPosition = Vector3.Lerp(doors[i].normPos, doors[i].openPos, doors[i].delta / doors[i].closeSpeed);
				}
			}
		}
	}
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1, 1, 0, 0.75F);
		for (int i = 0; i < doors.Count; i++)
		{
			Gizmos.DrawWireCube(doors[i].checkPos.position, doors[i].checkSize);
		}
	}
}
