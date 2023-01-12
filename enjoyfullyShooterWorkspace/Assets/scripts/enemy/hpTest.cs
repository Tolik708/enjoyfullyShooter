using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class hpTest : MonoBehaviour
{
	[Header("general")]
	public float hp;
	
	[Header("damage text")]
	public GameObject textPrefab;
	public Transform textPos;
	public Color critColor;
	public Color normalColor;
	
	public void takeDamage(float damage, float minDamage, float maxDamage)
	{
		hp -= damage;
		damagePopup(damage, minDamage, minDamage);
	}
	
	void damagePopup(float damage, float minDamage, float maxDamage)
	{
		TextMeshPro floatingText = ObjectPools.instance.getFromPool(textPrefab, textPos.position, Quaternion.identity).GetComponent<TextMeshPro>();
		floatingText.text = damage.ToString();
		floatingText.color =  Color.Lerp(normalColor, critColor, -((damage-minDamage)/(minDamage-maxDamage)));
	}
}
