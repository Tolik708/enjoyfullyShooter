using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletAddon : MonoBehaviour
{
	public GameObject me;
	public weaponAsset weaAsset;
	public float lifeTime;
	void Start()
	{
		lifeTime = weaAsset.bulletLifeTime;
	}
    void OnCollisionEnter(Collision col)
	{
		if (weaAsset.collidingLayers1.Contains(col.gameObject.layer))
			destroyer();
	}
	public void destroyer()
	{
		Destroy(me);
	}
}
