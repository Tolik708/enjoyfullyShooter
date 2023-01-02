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
	public LayerMask collidingLayers;
	public List<int> collidingLayers1;
	public float bulletLifeTime;
	public GameObject bullPrefab;
	
	[Header("ModelAndAnimation")]
	public GameObject weaModel;
	public Quaternion normalRotation;
	public Texture2D myTexture;
	public float unequipTime;
	public float equipTime;
	[HideInInspector]
	public Sprite mySprite;
	
	[Header("kickback")]
	public float powerOfKickFlyY;
	public float powerOfKickGroundY;
	public float powerOfKickFlyX;
	public float powerOfKickGroundX;
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
	
	[Header("scater")]
	public float yScater;
	public float xScater;
	public bool scater;
	
	[Header("mele")]
	public bool mele;
	public Vector3 meleRadius;
	
	[Header("ammo")]
	public float reloadTime;
	public float reloadWantingTime;
	public int maxAmmo;
	public bool useAmmo;
	public bool smoothReload;
	
	[Header("Input")]
	public KeyCode shootKey;
	public KeyCode reloadKey;
}
