using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level : MonoBehaviour
{
	public doorSystem ds;
	public elevatorSystem es;
	public GameObject barierObj;
	public GameObject[] rooms;
	public GameObject[] prefabs; // 0 - enterDoor, 1 - lift, 2 - exitDoor ...
	public Transform firstLastPos;
	private Transform lastPos;
    private List<GameObject> insObj = new List<GameObject>();
	private List<GameObject> insRooms = new List<GameObject>();
	private bool firstTime;
	private bool firstTime1;
	
	void Start()
	{
		//enter door
		lastPos = firstLastPos;
		GameObject enterDoor = Instantiate(prefabs[0], lastPos.position, lastPos.rotation);
		doorSystem.door doorClass = new doorSystem.door();
		doorClass.doorObj = enterDoor.transform.GetChild(0).gameObject;
		doorClass.checkPos = enterDoor.transform.GetChild(1).transform;
		doorClass.checkSize = new Vector3(4, 4, 4);
		doorClass.openPos = new Vector3(0, 5, 0);
		doorClass.openSpeed = 500;
		doorClass.closeSpeed = 100;
		doorClass.closeAfterExit = false;
		doorClass.lerpOpen = false;
		doorClass.lockDoor = false;
		doorClass.genDoor = true;
		doorClass.canOpen = true;
		ds.doors.Add(doorClass);
		ds.Check(ds.doors.Count - 1);
		lastPos = enterDoor.transform.GetChild(2);
		insObj.Add(enterDoor);
	}
	
	public void genElevAndExitDoor() //door uses
	{
		//elevator
		GameObject elev = Instantiate(prefabs[1], lastPos.position, lastPos.rotation);
		elevatorSystem.elevator elevClass = new elevatorSystem.elevator();
		elevClass.checkPos = elev.transform.GetChild(1).transform;
		elevClass.elevatorObj.Add(elev.transform.GetChild(4).gameObject);
		elevClass.elevatorObj.Add(insObj[insObj.Count - 1]);
		elevClass.checkSize = new Vector3(2.5f, 4, 4);
		elevClass.finPosition = new Vector3(0, -20, 0);
		elevClass.timeToCloseDoor = 3;
		elevClass.elevatorSpeed = 10000;
		elevClass.lerpElevator = false;
		elevClass.barierObj = barierObj;
		elevClass.barierPos = elev.transform.GetChild(2);
		elevClass.doorToClose = ds.doors.Count - 1;
		elevClass.needToCloseDoor = true;
		es.elevs.Add(elevClass);
		lastPos = elev.transform.GetChild(3);
		insObj.Add(elev);

		
		//exit door
		GameObject exitDoor = Instantiate(prefabs[2], lastPos.position, lastPos.rotation);
		doorSystem.door doorClass = new doorSystem.door();
		doorClass.doorObj = exitDoor.transform.GetChild(0).gameObject;
		doorClass.checkPos = exitDoor.transform.GetChild(1).transform;
		doorClass.checkSize = new Vector3(6, 4, 4);
		doorClass.openPos = new Vector3(0, 5, 0);
		doorClass.openSpeed = 500;
		doorClass.closeSpeed = 50;
		doorClass.closeAfterExit = true;
		doorClass.lerpOpen = false;
		doorClass.lockDoor = true;
		doorClass.genDoor = false;
		doorClass.canOpen = false;
		ds.doors.Add(doorClass);
		ds.Check(ds.doors.Count - 1);
		lastPos = exitDoor.transform.GetChild(2);
		insObj.Add(exitDoor);
		
		elevClass.elevatorObj.Add(exitDoor);
		elevClass.doorToOpen = 0;
		es.Check(es.elevs.Count - 1);
		
		//destroying
		if (!firstTime1)
		{
			firstTime1 = true;
		}
		else
		{
			Destroy(insObj[0]);
			insObj.RemoveAt(0);
			es.elevs.RemoveAt(0);
		}
	}
	public void genRoomAndEnterDoor() //elevator uses
	{
		GameObject room = Instantiate(rooms[Random.Range(0, rooms.Length)], lastPos.position, lastPos.rotation);
		lastPos = room.transform.GetChild(0);
		insRooms.Add(room);
		
		GameObject enterDoor = Instantiate(prefabs[0], lastPos.position, lastPos.rotation);
		doorSystem.door doorClass = new doorSystem.door();
		doorClass.doorObj = enterDoor.transform.GetChild(0).gameObject;
		doorClass.checkPos = enterDoor.transform.GetChild(1).transform;
		doorClass.checkSize = new Vector3(4, 4, 4);
		doorClass.openPos = new Vector3(0, 5, 0);
		doorClass.openSpeed = 500;
		doorClass.closeSpeed = 100;
		doorClass.closeAfterExit = false;
		doorClass.lerpOpen = false;
		doorClass.lockDoor = false;
		doorClass.genDoor = true;
		doorClass.canOpen = true;
		ds.doors.Add(doorClass);
		ds.Check(ds.doors.Count - 1);
		lastPos = enterDoor.transform.GetChild(2);
		insObj.Add(enterDoor);
		
		// destroying
		if (!firstTime)
		{
			Destroy(insObj[0]);
			Destroy(GameObject.Find("barier(Clone)"));
			insObj.RemoveAt(0);
			ds.doors.RemoveAt(0);
			firstTime = true;
		}
		else
		{
			Destroy(insObj[0]);
			Destroy(insObj[1]);
			Destroy(insRooms[0]);
			insRooms.RemoveAt(0);
			insObj.RemoveAt(1);
			insObj.RemoveAt(0);
			ds.doors.RemoveAt(1);
			ds.doors.RemoveAt(0);
			Destroy(GameObject.Find("barier(Clone)"));
		}
	}
}