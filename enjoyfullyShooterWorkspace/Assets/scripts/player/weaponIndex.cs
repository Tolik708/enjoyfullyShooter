using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponIndex : MonoBehaviour
{
	[Header("Outline")]
    public Outline myOutline;
	public float maxOutline;
	public float outlineSpeed;
	[HideInInspector]
	public bool watching;
	private float currOutline;
	
	public int index;
	public int ammo;
	
	void Update()
	{
		if(watching)
			currOutline = myOutline.OutlineWidth = Mathf.Lerp(currOutline, maxOutline, Time.deltaTime * outlineSpeed);
		else
			currOutline = myOutline.OutlineWidth = Mathf.Lerp(currOutline, 0, Time.deltaTime * outlineSpeed);
		watching = false;
	}
}
