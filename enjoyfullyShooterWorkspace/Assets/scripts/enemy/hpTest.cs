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
	
	public void takeDamage(bool head, weaponAsset wa)
	{
		float damage = wa.bulletDamage;
		if (wa.randomDamage)
			damage = Random.Range(wa.minMaxDamage.x, wa.minMaxDamage.y);
		
		if (head)
			damage *= wa.headDamageMultiplayer;
		
		hp -= damage;
		if (wa.randomDamage)
			damagePopup(damage, wa.minMaxDamage.x, wa.minMaxDamage.y);
		else
			damagePopup(damage, wa.bulletDamage, wa.bulletDamage*wa.headDamageMultiplayer);
	}
	
	void damagePopup(float damage, float minDamage, float maxDamage)
	{
		Debug.Log(damage);
		TextMeshPro floatingText = ObjectPools.instance.getFromPool(textPrefab, textPos.position, Quaternion.identity).GetComponent<TextMeshPro>();
		floatingText.text = damage.ToString();
		
		float t = 0;
		if ((minDamage-maxDamage) != 0)
			t = -((damage-minDamage)/(minDamage-maxDamage));
		
		floatingText.color = new Color(Mathf.Lerp(normalColor.r, critColor.r, t), Mathf.Lerp(normalColor.g, critColor.g, t), Mathf.Lerp(normalColor.b, critColor.b, t), 255);
	}
}
