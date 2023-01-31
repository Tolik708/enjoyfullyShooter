using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu]
public class weaponAsset : ScriptableObject
{
	public enum weaType {simple, fist, sword, pattern, multiBullet_likeShotgun, laser, };
	
    [Header("General")]
	public weaType weaponType;
	public float shootDelay;
	public float helpStrength;
	public bool multiShoot;
	
	[Header("ModelAndAnimation")]
	public bool haveAttackAnim = true;
	public GameObject weaModel;
	public Quaternion normalRotation;
	public Texture2D myTexture;
	public float unequipTime;
	public float equipTime;
	public float shootTime;
	public bool useArm;
	[HideInInspector]
	public Sprite mySprite;
	
	[Header("Input")]
	public KeyCode shootKey;
	public KeyCode reloadKey;
	
	[Header("bullet")]
	public LayerMask enemyLayer;
	public LayerMask collidingLayers;
	public List<int> collidingLayers1;
	public float bulletLifeTime;
	public GameObject bullPrefab;
	public bool randSpeed;
	public float bullSpeed;
	public float minRandSpeed;
	public float maxRandSpeed;
	
	[Header("damage")]
	public bool randomDamage;
	public float bulletDamage;
	public float headDamageMultiplayer;
	public Vector2 minMaxDamage;
	
	
	[Header("raycast")]
	public bool rayCast;
	public GameObject trail;
	public float bulletDelay;
	public float trailSpeed;
	
	[Header("Multiple Bullets")]
	public float bullCount;
	public float randXRot;
	public float randYRot;
	
	[Header("Pattern")]
	public bool pattern;
	public float bullAmmount;
	
	//[Header("laser")]
	
	[Header("fist")]
	public bool distanceInfluence;
	public Vector2 distanceInfluencClamp;
	public Vector3 meleRadius;
	public float maxMeleDist;
	
	[Header("kickback")]
	public bool useKick;
	public Vector2 kickPowerFly;
	public Vector2 kickPowerGround;
	
	[Header("gravity")]
	public bool useGravity;
	public float gravityPower;
	
	[Header("explosion")]
	public bool useExplosions;
	public LayerMask explosionLayers;
	public float powerOfExplFlyY;
	public float powerOfExplGroundY;
	public float powerOfExplFlyX;
	public float powerOfExplGroundX;
	public float explosionRadius;
	
	[Header("scater")]
	public bool scater;
	public float yScater;
	public float xScater;
	
	[Header("rebound")]
	public bool rebound;
	public int reboundCount;
	
	[Header("ammo")]
	public bool useAmmo;
	public bool smoothReload;
	public float reloadTime;
	public float reloadWantingTime;
	public int maxAmmo;
	
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
		if (helpStrength == 0)
			helpStrength = 0.01f;
		
		if (normalRotation.x == 0 && normalRotation.y == 0 && normalRotation.z == 0)
			normalRotation = new Quaternion(0, 0, 0, 1);
	}
}