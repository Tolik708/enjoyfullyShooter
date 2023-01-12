using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu]
public class weaponAsset : ScriptableObject
{
    [Header("General")]
	public float shootDelay;
	public bool multiShoot;
	
	[Header("bullet")]
	public LayerMask enemyLayer;
	public LayerMask collidingLayers;
	public List<int> collidingLayers1;
	public float bulletLifeTime;
	public GameObject bullPrefab;
	
	[Header("damage")]
	public float bulletDamage;
	public float headDamagaMultiplayer;
	
	[Header("ModelAndAnimation")]
	public GameObject weaModel;
	public Quaternion normalRotation;
	public Texture2D myTexture;
	public float unequipTime;
	public float equipTime;
	public float shootTime;
	[HideInInspector]
	public Sprite mySprite;
	
	[Header("kickback")]
	public Vector2 kickPowerFly;
	public Vector2 kickPowerGround;
	public bool useKick;
	
	[Header("explosion")]
	public LayerMask explosionLayers;
	public float powerOfExplFlyY;
	public float powerOfExplGroundY;
	public float powerOfExplFlyX;
	public float powerOfExplGroundX;
	public float explosionRadius;
	public bool useExplosions;
	
	[Header("gravity")]
	public bool useGravity;
	public float gravityPower;
	
	[Header("speed")]
	public bool randSpeed;
	public float bullSpeed;
	public float minRandSpeed;
	public float maxRandSpeed;
	
	[Header("Multiple Bullets")]
	public float bullCount;
	public float randXRot;
	public float randYRot;
	public bool multiBull;
	
	[Header("Pattern")]
	public float bullAmmount;
	public bool pattern;
	
	[Header("laser")]
	public bool laser;
	
	[Header("mele")]
	public bool mele;
	public Vector3 meleRadius;
	public float handReturnSpeed;
	
	[Header("scater")]
	public float yScater;
	public float xScater;
	public bool scater;
	
	[Header("rebound")]
	public int reboundCount;
	public bool rebound;
	
	[Header("ammo")]
	public float reloadTime;
	public float reloadWantingTime;
	public int maxAmmo;
	public bool useAmmo;
	public bool smoothReload;
	
	[Header("Input")]
	public KeyCode shootKey;
	public KeyCode reloadKey;
	
	public void start()
	{
		//Init texture of sprite
		if (myTexture != null)
			mySprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f), 100);
		
		//variables that can't be 0
		if (equipTime == 0)
			equipTime = 0.1f;
		if (unequipTime == 0)
			unequipTime = 0.1f;
		if (shootTime == 0)
			shootTime = 0.1f;
		
		if (normalRotation.x == 0 && normalRotation.y == 0 && normalRotation.z == 0)
			normalRotation = new Quaternion(0, 0, 0, 1);
	}
}