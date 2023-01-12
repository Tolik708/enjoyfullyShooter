using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPools : MonoBehaviour
{
	public int standartPoolSize;
    public static ObjectPools instance;
	public List<myObject> pooled = new List<myObject>();
	
	[System.Serializable]
	public class myObject
	{
		public GameObject prefabToPool;
		public int ammountToPull;
		//hide
		[HideInInspector]
		public string prefabName;
		[HideInInspector]
		public List<GameObject> pooledPrefabs = new List<GameObject>();
	}
	
	void Awake()
	{
		if (instance == null)
			instance = this;
	}
	
	void Start()
	{
		for (int i = 0; i < pooled.Count; i++)
		{
			for (int u = 0; u < pooled[i].ammountToPull; u++)
			{
				GameObject newObj = Instantiate(pooled[i].prefabToPool);
				newObj.SetActive(false);
				pooled[i].pooledPrefabs.Add(newObj);
			}
			pooled[i].prefabName = pooled[i].prefabToPool.name;
		}
	}
	
	public GameObject getFromPool(GameObject wantedObj, Vector3 pos, Quaternion rot)
	{
		for (int i = 0; i < pooled.Count; i++)
		{
			if(wantedObj.name == pooled[i].prefabName)
			{
				for (int u = 0; u < pooled[i].ammountToPull; u++)
				{
					//if found free object
					if (!pooled[i].pooledPrefabs[u].activeSelf)
						return prepare(pooled[i].pooledPrefabs[u], pos, rot);
				}
				//if all pooled objects are busy add new to pool
				GameObject newObje = Instantiate(wantedObj, pos, rot);
				pooled[i].ammountToPull++;
				pooled[i].pooledPrefabs.Add(newObje);
				return prepare(newObje, pos, rot);
			}
		}
		//if this object isn't pooled
		myObject objToAdd = new myObject();
		objToAdd.ammountToPull = standartPoolSize;
		objToAdd.prefabName = wantedObj.name;
		//init newObj
		GameObject newObj = Instantiate(wantedObj, pos, rot);
		newObj.SetActive(false);
		//add to pooled
		objToAdd.pooledPrefabs.Add(newObj);
		pooled.Add(objToAdd);
		for (int u = 1; u < standartPoolSize; u++)
		{
			newObj = Instantiate(wantedObj, Vector3.zero, Quaternion.identity);
			newObj.SetActive(false);
			objToAdd.pooledPrefabs.Add(newObj);
		}
		
		return prepare(newObj, pos, rot);
	}
	
	GameObject prepare(GameObject obj, Vector3 pos, Quaternion rot)
	{
		obj.SetActive(true);
		obj.transform.position = pos;
		obj.transform.rotation = rot;
		if (obj.GetComponent<TrailRenderer>() != null)
			obj.GetComponent<TrailRenderer>().Clear();
		return obj;
	}
}
