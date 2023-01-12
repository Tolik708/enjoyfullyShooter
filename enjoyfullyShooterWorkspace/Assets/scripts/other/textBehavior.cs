using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class textBehavior : MonoBehaviour
{
	public GameObject me;
    public float lifeTime;
	private float lifeTimer = -1;
	private float normalFontSize;
	public float dissapiarTime;
	public float offset;
	TextMeshPro tmp;
	
	void Awake()
	{
		tmp = me.GetComponent<TextMeshPro>();
		normalFontSize = tmp.fontSize;
	}
	
	void OnEnable()
	{
		tmp.fontSize = normalFontSize;
	}
	
	void Update()
	{
		if (lifeTimer == -1)
			lifeTimer = lifeTime;
		
		lifeTimer -= Time.deltaTime;
		
		if (lifeTimer < dissapiarTime)
		{
			//smooth change of alpha to 0
			tmp.color = new Color32((byte)(tmp.color.r*255), (byte)(tmp.color.g*255), (byte)(tmp.color.b*255), (byte)Mathf.Lerp(0, (tmp.color.a*255), lifeTimer/dissapiarTime));
			//smooth scale down
			tmp.fontSize = Mathf.Lerp(0, 6, lifeTimer/dissapiarTime);
		}
		
		//rotation and position
		me.transform.LookAt(GameObject.Find("/Player/player").transform);
		me.transform.position = new Vector3(me.transform.position.x, me.transform.position.y+(offset*Time.deltaTime), me.transform.position.z);
		
		if (lifeTimer < 0)
		{
			lifeTimer = -1;
			me.SetActive(false);
		}
	}
}
