using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class elevatorSystem : MonoBehaviour
{
    public LayerMask playerLayer;
	public doorSystem ds;
	public level lv;
    [System.Serializable]
    public class elevator{
		[Header("general")]
		public List<GameObject> elevatorObj = new List<GameObject>();
		public Transform checkPos;
		public Vector3 checkSize;
		public Vector3 finPosition;
		public float timeToCloseDoor;
		public float elevatorSpeed;
		public bool lerpElevator;
		public bool needToCloseDoor;
		public bool movingPlatform;
		
		[Header("barier")]
		public GameObject barierObj;
		public Transform barierPos;
		public bool barier;
		//hide
		public Vector3[] startPos;
		public Vector3[] finPos;
		public int doorToClose;
		public int doorToOpen;
		public float delta;
		public bool onPlace;
		public bool doorClosed;
		public bool playerIn;
		public bool once;
	}
	
	public List<elevator> elevs = new List<elevator>();
	
	void Start()
	{
		if (elevs != null)
			StartCoroutine(upudate());
		for (int i = 0; i < elevs.Count; i++)
			Check(i);
	}
	
	public void Check(int i)
	{
		elevs[i].checkSize = new Vector3(elevs[i].checkSize.x/2, elevs[i].checkSize.y/2, elevs[i].checkSize.z/2);
		if (elevs[i].movingPlatform)
		{
			elevs[i].startPos[0] = elevs[i].elevatorObj[0].transform.localPosition;
			elevs[i].finPos[0] = elevs[i].finPosition + elevs[i].startPos[0];
		}
		else
		{
			for (int u = 0; u < elevs[i].elevatorObj.Count; u++)
			{
				Array.Resize(ref elevs[i].startPos, elevs[i].elevatorObj.Count);
				Array.Resize(ref elevs[i].finPos, elevs[i].elevatorObj.Count);
				elevs[i].startPos[u] = elevs[i].elevatorObj[u].transform.localPosition;
				elevs[i].finPos[u] = elevs[i].finPosition + elevs[i].startPos[u];
			}
		}
	}
	
	IEnumerator upudate()
	{
		while (true)
		{
			for (int i = 0; i < elevs.Count; i++)
			{
				if (Physics.OverlapBox(elevs[i].checkPos.position, elevs[i].checkSize, Quaternion.identity, playerLayer).Length > 0)
					elevs[i].playerIn = true;
				
				if (elevs[i].playerIn && !elevs[i].onPlace)
				{
					if (!elevs[i].barier)
					{
						elevs[i].barier = true;
						Instantiate(elevs[i].barierObj, elevs[i].barierPos.position, elevs[i].barierPos.rotation);
					}
					if (elevs[i].needToCloseDoor)
						ds.doors[elevs[i].doorToClose].mustClose = true;
					if (!elevs[i].doorClosed)
					{
						yield return new WaitForSeconds(elevs[i].timeToCloseDoor);
						elevs[i].doorClosed = true;
					}
					if (elevs[i].movingPlatform)
					{
						if (elevs[i].lerpElevator)
						{
							elevs[i].elevatorObj[0].transform.localPosition = Vector3.Lerp(elevs[i].elevatorObj[0].transform.localPosition, elevs[i].finPos[0], Time.deltaTime * elevs[i].elevatorSpeed);
						}
						else
						{
							elevs[i].delta++;
							if (elevs[i].delta > elevs[i].elevatorSpeed)
								elevs[i].delta = elevs[i].elevatorSpeed;
							elevs[i].elevatorObj[0].transform.localPosition = Vector3.Lerp(elevs[i].elevatorObj[0].transform.localPosition, elevs[i].finPos[0], elevs[i].delta / elevs[i].elevatorSpeed);
						}
						
						if (Vector3.Distance(elevs[i].elevatorObj[0].transform.localPosition, elevs[i].finPos[0]) < 0.1f && !elevs[i].once)
						{
							elevs[i].once = true;
							lv.genRoomAndEnterDoor();
							ds.doors[elevs[i].doorToOpen].canOpen = true;
							elevs[i].onPlace = true;
						}
					}
					else
					{
						for (int u = 0; u < elevs[i].elevatorObj.Count; u++)
						{
							if (elevs[i].lerpElevator)
							{
								elevs[i].elevatorObj[u].transform.localPosition = Vector3.Lerp(elevs[i].elevatorObj[u].transform.localPosition, elevs[i].finPos[u], Time.deltaTime * elevs[i].elevatorSpeed);
							}
							else
							{
								elevs[i].delta++;
								if (elevs[i].delta > elevs[i].elevatorSpeed)
									elevs[i].delta = elevs[i].elevatorSpeed;
								elevs[i].elevatorObj[u].transform.localPosition = Vector3.Lerp(elevs[i].elevatorObj[u].transform.localPosition, elevs[i].finPos[u], elevs[i].delta / elevs[i].elevatorSpeed);
							}
							
							if (Vector3.Distance(elevs[i].elevatorObj[0].transform.localPosition, elevs[i].finPos[0]) < 0.1f && !elevs[i].once)
							{
								elevs[i].once = true;
								lv.genRoomAndEnterDoor();
								ds.doors[elevs[i].doorToOpen].canOpen = true;
								elevs[i].onPlace = true;
							}
						}
					}
				}
			}
			yield return null;
		}
	}
	
	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1, 1, 0, 0.75F);
		for (int i = 0; i < elevs.Count; i++)
		{
			Gizmos.DrawWireCube(elevs[i].checkPos.position, elevs[i].checkSize);
		}
	}
}
