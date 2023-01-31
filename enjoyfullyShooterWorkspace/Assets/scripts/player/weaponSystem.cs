using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random=UnityEngine.Random;

public class weaponSystem : MonoBehaviour
{
	public float debug;
	
	[Header("General")]
	public player pm;
	public weaponAsset[] assets;
	public weaponAsset a;
	public smoothMover sm;
	
	[Header("shooting")]
	public Rigidbody playerRb;
	public Transform cam;
	private Transform gunTip;
	private List<bulletAddon> shootedBulls = new List<bulletAddon>();
	
	[Header("mele")]
	public Transform meleAtackPos;
	[HideInInspector]
	public bool attacking;
	private List<GameObject> attackedObjects = new List<GameObject>();
	
	[Header("UI")]
	public TextMeshProUGUI ammoText;
	
	[Header("equiping")]
	//UI
	public Image[] allSlots;
	
	//throwing
	public KeyCode throwKey;
	public float throwForce;
	public float minYTorque;
	public float maxYTorque;
	public float minZTorque;
	public float maxZTorque;
	
	//timers
	public float changeWantingTime;
	private float changeWantingTimer;
	private float shootDelayTimer;
	
	//equiping
	public float equipDist;
	public float equipRadius;
	public float newWeaSpeed;
	public KeyCode equipKey;
	
	//general
	public List<Vector2> myWeapons = new List<Vector2>();
	public LayerMask weaponLayer;
	private int myWeapon = -1;
	private Vector3 wantingWeapon;
	private GameObject wantingWeaObj;
	private GameObject currWea;
	private weaponIndex ammoWi;
	private bool canShoot;
	private bool canChange = true;
	private bool reloading;
	private KeyCode[] keyCodes = new KeyCode []{ KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9 };
	
	[Header("animation")]
	public inverseKinematics arm;
	public GameObject myArm;
	public Transform weaPos;
	public Vector3 stableLocalWeaPos;
	public float returnSpeed;
	private Animator weaAnim;
	private bool animEnabled = true;
	
	//sway
	public float swaySpeed;
	public Vector2 swayIntensity;
	public Vector2 swayPosIntensity;
	public Vector2 minAndMaxXSway;
	public Vector2 minAndMaxYSway;
	
	//curves
	public AnimationCurve idleAnimCurve;
	public float idleAnimCurveSpeed;
	private float ticks;
	
	void Start()
	{
		stableLocalWeaPos = weaPos.localPosition;
		
		for (int i = 0; i < assets.Length; i++)
			assets[i].start();
		
		recalculateInventory();
		StartCoroutine(Equip((int)myWeapons[0].x, 0));
	}
	
	void Update()
	{
		myInput();
		MyUI();
		weaAnimation();
		meleWeapon();
		
		//timers
		if (shootDelayTimer >= 0)
			shootDelayTimer -= Time.deltaTime;
		
		if (changeWantingTimer >= 0)
			changeWantingTimer -= Time.deltaTime;
	}
	
	////////////////////////INPUT////////////////////////
	void myInput()
	{
		//equiping
		if (Input.anyKeyDown)
		{
			for (int i = 1; i < myWeapons.Count+1; i++)
			{
				if (Input.GetKeyDown(keyCodes[i]) && i-1 != myWeapon && canChange)
				{
					StartCoroutine(Equip((int)myWeapons[i-1].x, i-1));
				}
				else if (Input.GetKeyDown(keyCodes[i]) && i-1 != myWeapon && !canChange)
				{
					wantingWeapon = new Vector3(myWeapons[i-1].x, i-1, 0);
					changeWantingTimer = changeWantingTime;
				}
			}
		}
		if (changeWantingTimer > 0 && canChange)
		{
			if (wantingWeapon.z == 0)
			{
				changeWantingTimer = -1;
				StartCoroutine(Equip((int)wantingWeapon.x, (int)wantingWeapon.y));
			}
			else
			{
				changeWantingTimer = -1;
				StartCoroutine(EquipNew((int)wantingWeapon.x, (int)wantingWeapon.y, wantingWeaObj));
			}
		}
		
		//equiping new
		RaycastHit hit;
		if (Physics.Raycast(cam.position, cam.forward, out hit, equipDist + 1, weaponLayer))
		{
			weaponIndex wi = hit.collider.gameObject.GetComponent<weaponIndex>();
			wi.watching = true;
			if (Input.GetKeyDown(equipKey))
			{
				if (myWeapons.Count < allSlots.Length && canChange)
				{
					recalculateInventory();
					StartCoroutine(EquipNew(wi.index, myWeapons.Count, hit.collider.gameObject));
				}
				else if (myWeapons.Count < allSlots.Length && !canChange)
				{
					wantingWeapon = new Vector3(wi.index, myWeapons.Count, 1);
					changeWantingTimer = changeWantingTime;
					wantingWeaObj = hit.collider.gameObject;
				}
			}
		}
		else if (Physics.SphereCast(cam.position, equipRadius, cam.forward, out hit, equipDist, weaponLayer))
		{
			weaponIndex wi = hit.collider.gameObject.GetComponent<weaponIndex>();
			wi.watching = true;
			if (Input.GetKeyDown(equipKey))
			{
				for (int i = 0; i < myWeapons.Count; i++)
				{
					if (myWeapons[i].x != wi.index && myWeapons.Count < allSlots.Length && canChange)
					{
						recalculateInventory();
						StartCoroutine(EquipNew(wi.index, myWeapons.Count, hit.collider.gameObject));
					}
					else if (myWeapons[i].x != wi.index && myWeapons.Count < allSlots.Length && !canChange)
					{
						wantingWeapon = new Vector3(wi.index, myWeapons.Count, 1);
						changeWantingTimer = changeWantingTime;
						wantingWeaObj = hit.collider.gameObject;
					}
				}
			}
		}
		
		//throwing
		if (Input.GetKeyDown(throwKey))
		{
			if(myWeapon > 0 && canChange)
			{
				Throw();
			}
		}
		
		//shooting
		if (canShoot)
		{
			if (a.useAmmo)
			{
				if (a.multiShoot && shootDelayTimer < 0 && Input.GetKey(a.shootKey) && ammoWi.ammo > 0 && !reloading)
				{
					shootDelayTimer = a.shootDelay;
					shoot();
					ammoWi.ammo--;
					myWeapons[myWeapon] = new Vector2(myWeapons[myWeapon].x, ammoWi.ammo);
				}
				else if (!a.multiShoot && shootDelayTimer < 0 && Input.GetKey(a.shootKey) && ammoWi.ammo > 0 && !reloading)
				{
					shootDelayTimer = a.shootDelay;
					shoot();
					ammoWi.ammo--;
					myWeapons[myWeapon] = new Vector2(myWeapons[myWeapon].x, ammoWi.ammo);
				}
				
				//reloading
				if (shootDelayTimer < 0 && ammoWi.ammo <= 0 && !reloading)
				{
					StartCoroutine(reload());
				}
				if (Input.GetKeyDown(a.reloadKey) && ammoWi.ammo < a.maxAmmo && !reloading)
				{
					StartCoroutine(reload());
				}
			}
			else
			{
				if (a.multiShoot && shootDelayTimer < 0 && Input.GetKey(a.shootKey))
				{
					shootDelayTimer = a.shootDelay;
					shoot();
				}
				else if (!a.multiShoot && shootDelayTimer < 0 && Input.GetKey(a.shootKey))
				{
					shootDelayTimer = a.shootDelay;
					shoot();
				}
			}
		}
	}
	
	
	////////////////////////RELOAD///////////////////////
	IEnumerator reload()
	{
		reloading = true;
		animEnabled = false;
		if (a.smoothReload)
		{
			float oneBullTime = a.reloadTime/a.maxAmmo;
			float i = (a.maxAmmo-ammoWi.ammo)*oneBullTime;
			weaAnim.SetFloat("speed", i > a.reloadTime/2 ? 1/i : 1/(a.reloadTime/2));
			weaAnim.Play("reload");
			while (ammoWi.ammo < a.maxAmmo)
			{
				yield return new WaitForSeconds(oneBullTime);
				ammoWi.ammo++;
				myWeapons[myWeapon] = new Vector2(myWeapons[myWeapon].x, ammoWi.ammo);
			}
		}
		else
		{
			float oneBullTime = a.reloadTime/a.maxAmmo;
			float i = (a.maxAmmo-ammoWi.ammo)*oneBullTime > a.reloadTime/2 ? (a.maxAmmo-ammoWi.ammo)*oneBullTime : a.reloadTime/2;
			weaAnim.SetFloat("speed", 1/i);
			weaAnim.Play("reload");
			yield return new WaitForSeconds(i/2);
			ammoWi.ammo = 0;
			yield return new WaitForSeconds(i/2);
			ammoWi.ammo = a.maxAmmo;
			myWeapons[myWeapon] = new Vector2(myWeapons[myWeapon].x, ammoWi.ammo);
		}
		reloading = false;
		animEnabled = true;
		shootDelayTimer = 0.5f;
	}
	
	////////////////////////SHOOT////////////////////////
	void shoot()
	{
		StartCoroutine(shootTimer());
		//animation
		if (a.haveAttackAnim)
		{
			weaAnim.SetFloat("speed", 1 / a.shootTime);
			weaAnim.Play("shoot");
		}
		
		if (a.rayCast)
		{
			switch (a.weaponType)
			{
				case weaponAsset.weaType.multiBullet_likeShotgun:
				{
					
					break;
				}
				case weaponAsset.weaType.pattern:
				{
					
					break;
				}
				case weaponAsset.weaType.simple:
				{
					GameObject trail = ObjectPools.instance.getFromPool(a.trail, gunTip.position, gunTip.rotation);
					break;
				}
				default:
					Debug.Log("raycasting with this weapontype imposible");
					break;
			}
		}
		else
		{
			switch (a.weaponType)
			{
				case weaponAsset.weaType.multiBullet_likeShotgun:
				{
					for (int i = 0; i < a.bullCount; i++)
					{
						//get object from its pool
						GameObject bull = ObjectPools.instance.getFromPool(a.bullPrefab, gunTip.position, gunTip.rotation);
						//add random rotation
						bull.transform.Rotate(Random.Range(a.randXRot, -a.randXRot), Random.Range(a.randYRot, -a.randYRot), 0);
						//add velocity
						Rigidbody bullRb = bull.GetComponent<Rigidbody>();
						float playerSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
						bullRb.AddForce(((a.randSpeed == false ? a.bullSpeed : Random.Range(a.minRandSpeed, a.minRandSpeed)) + playerSpeed) * bull.transform.forward, ForceMode.Impulse);
						//bullet addon
						bull.GetComponent<bulletAddon>().wa = a;
					}
					break;
				}
				case weaponAsset.weaType.fist:
				{
					StartCoroutine(fistAtack());
					break;
				}
				case weaponAsset.weaType.sword:
				{
					StartCoroutine(swordAttack());
					break;
				}
				case weaponAsset.weaType.pattern:
				{
					//get object from its pool
					GameObject bullet = ObjectPools.instance.getFromPool(a.bullPrefab, gunTip.position, gunTip.rotation);
					//assign right rotation to bullet
					RaycastHit bellHit;
					if (Physics.Raycast(cam.position, cam.forward, out bellHit, 200, a.collidingLayers))
						bullet.transform.LookAt(bellHit.point);
					else
						bullet.transform.LookAt(cam.position + cam.forward * 200); 
					
					//go trough each bullet
					for (int i = 0; i < a.bullAmmount; i++)
					{
						GameObject bull = bullet.transform.GetChild(i).gameObject;
						//add velocity
						Rigidbody bullRb = bull.GetComponent<Rigidbody>();
						float playerSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
						bullRb.AddForce(((a.randSpeed == false ? a.bullSpeed : Random.Range(a.minRandSpeed, a.minRandSpeed)) + playerSpeed) * bull.transform.forward, ForceMode.Impulse);
						//bulletAddon
						bull.GetComponent<bulletAddon>().wa = a;
					}
					break;
				}
				case weaponAsset.weaType.laser:
				{
					break;
				}
				case weaponAsset.weaType.simple:
				{
					//get object from its pool
					GameObject bull = ObjectPools.instance.getFromPool(a.bullPrefab, gunTip.position, gunTip.rotation);
					//set rotation
					RaycastHit bellHit;
					if (Physics.SphereCast(cam.position, a.helpStrength, cam.forward, out bellHit, 200, a.collidingLayers))
						bull.transform.LookAt(bellHit.point);
					else
						bull.transform.LookAt(cam.position + cam.forward * 200); 
					//set velocity
					Rigidbody bullRb = bull.GetComponent<Rigidbody>();
					float playerSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
					bullRb.AddForce(((a.randSpeed == false ? a.bullSpeed : Random.Range(a.minRandSpeed, a.minRandSpeed)) + playerSpeed) * bull.transform.forward, ForceMode.Impulse);
					//init bulletAddon
					bull.GetComponent<bulletAddon>().wa = a;
					//scater
					if (a.scater)
					{
						bullRb.AddForce(bull.transform.up * (Random.Range(1, 3) == 1 ? a.yScater : -a.yScater));
						bullRb.AddForce(bull.transform.right * (Random.Range(1, 3) == 1 ? a.xScater : -a.xScater));
					}
					break;
				}
			}
		}
		
		//kickback
		if (a.useKick)
		{
			playerRb.velocity = new Vector3(playerRb.velocity.x, playerRb.velocity.y > 0.1f ? playerRb.velocity.y : 0, playerRb.velocity.z);
			if (pm.realGrounded)
				playerRb.AddForce(-new Vector3(cam.forward.x * a.kickPowerGround.x, cam.forward.y * a.kickPowerGround.y, cam.forward.z * a.kickPowerGround.x), ForceMode.Impulse);
			else
				playerRb.AddForce(-new Vector3(cam.forward.x * a.kickPowerFly.x, cam.forward.y * a.kickPowerFly.y, cam.forward.z * a.kickPowerFly.x), ForceMode.Impulse);
		}
	}
	void meleWeapon()
	{
		if ((a.weaponType == weaponAsset.weaType.fist || a.weaponType == weaponAsset.weaType.sword) && attacking)
		{
			Collider[] contacts = Physics.OverlapBox(currWea.transform.position, a.meleRadius, currWea.transform.rotation, a.enemyLayer);
			for (int i = 0; i < contacts.Length; i++)
			{
				if (attackedObjects.Contains(contacts[i].gameObject.transform.parent.gameObject))
					continue;

				if (a.enemyLayer == (a.enemyLayer | (1 << contacts[i].gameObject.layer)))
				{
					attackedObjects.Add(contacts[i].gameObject.transform.parent.gameObject);
					//deal damage
					if (contacts[i].gameObject.CompareTag("enemyHead"))
						contacts[i].gameObject.transform.parent.gameObject.GetComponent<hpTest>().takeDamage(true, a);
					else if (contacts[i].gameObject.CompareTag("enemyBody"))
						contacts[i].gameObject.transform.parent.gameObject.GetComponent<hpTest>().takeDamage(false, a);
				}
			}
		}
	}
	
	IEnumerator shootTimer()
	{
		animEnabled = false;
		canChange = false;
		yield return new WaitForSeconds(a.shootDelay);
		animEnabled = true;
		canChange = true;
		yield return null;
	}
	
	IEnumerator fistAtack()
	{
		attacking = true;
		
		//calculating attack time
		float toPointTime = a.shootDelay/3; //25%
		float recoveryTime = a.shootDelay/1.5f; //75%
		
		RaycastHit hat;
		//point to what we are going
		Vector3 toPoint;
		
		//if not hited anything
		if (!Physics.Raycast(cam.position, cam.forward, out hat, a.maxMeleDist, a.collidingLayers))
			toPoint = cam.InverseTransformPoint(cam.position + cam.forward * a.maxMeleDist);
		
		//if hit in something
		else
		{
			//to local position of camera
			toPoint = cam.InverseTransformPoint(hat.point);
			if (a.distanceInfluence)
			{
				//calculate time percent (0-1) of distance we need
				float percent = Mathf.Clamp(1-((((cam.position + cam.forward * a.maxMeleDist)-cam.position).sqrMagnitude - (hat.point-cam.position).sqrMagnitude)/((cam.position + cam.forward * a.maxMeleDist)-cam.position).sqrMagnitude), a.distanceInfluencClamp.x, a.distanceInfluencClamp.y);
				//if distance to target small we want to attack upper
				if (percent < 0.5f)
				{
					toPoint += Vector3.up * 0.5f;
					percent = Mathf.Clamp(1-((((cam.position + cam.forward * a.maxMeleDist)-cam.position).sqrMagnitude - (hat.point-cam.position).sqrMagnitude)/((cam.position + cam.forward * a.maxMeleDist)-cam.position).sqrMagnitude), a.distanceInfluencClamp.x, a.distanceInfluencClamp.y);
				}
				//apply time percentage
				toPointTime *= percent;
				recoveryTime *= percent;
			}
		}
		
		smoothMover.instance ins = sm.MoveObj(weaPos, toPoint, toPointTime, smoothMover.moveType.stable);
		ins.global(false);
		while (ins.active)
			yield return 0;
			
		attacking = false;
		
		smoothMover.instance ins1 = sm.MoveObj(weaPos, stableLocalWeaPos, recoveryTime, smoothMover.moveType.stable);
		ins1.global(false);
		while(ins1.active)
			yield return 0;
		
		
		attackedObjects = new List<GameObject>();
		yield return null;
	}
	
	IEnumerator swordAttack()
	{
		attacking = true;
		
		float lastShootTimer = a.shootTime;
		while (lastShootTimer > 0)
		{
			lastShootTimer -= Time.deltaTime;
			yield return null;
		}
		
		attacking = false;
		attackedObjects = new List<GameObject>();
		yield return null;
	}
	
	///////////////////////EQUIP/////////////////////////
	IEnumerator Equip(int i, int u)
	{
		//prepeaing
		canShoot = false;
		canChange = false;
		animEnabled = false;
		
		//slots
		if (myWeapon != -1)
			allSlots[myWeapon].color = new Color32(255, 255, 255, 128);
		allSlots[u].color = new Color32(255, 255, 255, 255);
		
		//unequiping
		if (currWea != null)
		{
			weaAnim.SetFloat("speed", 1 / a.unequipTime);
			weaAnim.Play("unequip");
			yield return new WaitForSeconds(a.unequipTime);
			Destroy(currWea);
		}
		
		//set current slot and asset for use
		myWeapon = u;
		a = assets[i];
		
		//init current weapon
		currWea = Instantiate(a.weaModel, weaPos.position, Quaternion.identity);
		currWea.transform.parent = weaPos;
		currWea.transform.localRotation = Quaternion.identity;
		//set rotation to normal rotation of weapon
		weaPos.transform.localRotation = a.normalRotation;
		//get ammo of weapon and save it for ability to change weapons
		ammoWi = currWea.GetComponent<weaponIndex>();
		ammoWi.ammo = (int)myWeapons[myWeapon].y;
		//get guntip if it needed
		if (a.weaponType != weaponAsset.weaType.fist && a.weaponType != weaponAsset.weaType.sword)
			gunTip = currWea.transform.GetChild(0);
		
		//init arm if needed
		if (a.useArm)
		{
			myArm.SetActive(true);
			arm.target = currWea.transform.Find("armTarget");;
			arm.init();
		}
		else
			myArm.SetActive(false);
		
		//animation
		weaAnim = currWea.GetComponent<Animator>();
		weaAnim.SetFloat("speed", 1 / a.equipTime);
		weaAnim.Play("equip");
		
		yield return new WaitForSeconds(a.equipTime-0.1f);
		
		canShoot = true;
		canChange = true;
		animEnabled = true;
	}
	
	IEnumerator EquipNew(int i, int u, GameObject newWeapon)
	{
		//prepeaing
		canShoot = false;
		canChange = false;
		animEnabled = false;
		
		//unequiping
		if (currWea != null)
		{
			weaAnim.SetFloat("speed", 1 / a.unequipTime);
			weaAnim.Play("unequip");
			yield return new WaitForSeconds(a.unequipTime);
			Destroy(currWea);
		}
		
		//init
		a = assets[i];
		myWeapon = u;
		
		Destroy(newWeapon.GetComponent<Rigidbody>());
		currWea = newWeapon;
		currWea.layer = 10;
		ammoWi = currWea.GetComponent<weaponIndex>();
		myWeapons.Add(new Vector2(i, ammoWi.ammo));
		if (a.weaponType != weaponAsset.weaType.fist && a.weaponType != weaponAsset.weaType.sword)
			gunTip = currWea.transform.GetChild(0);
		
		//slots
		recalculateInventory();
		allSlots[u].color = new Color32(255, 255, 255, 255);
		
		//moving
		currWea.transform.parent = weaPos;
		smoothMover.instance ins = sm.MoveObj(currWea.transform, weaPos.transform.position, 2, smoothMover.moveType.stableLerp);
		ins.dynamicPos = true;
		ins.dynamicTransform = weaPos.transform;
		while (ins.active)
			yield return 0;
		
		//rotating
		smoothMover.instance ins1 = sm.RotObj(currWea.transform, Quaternion.identity, 2, smoothMover.moveType.stableLerp);
		ins1.global(false);
		while (ins1.active)
			yield return 0;
		
		//animation
		currWea.GetComponent<Animator>().enabled = true;
		weaAnim = currWea.GetComponent<Animator>();
		
		canChange = true;
		canShoot = true;
		animEnabled = true;
	}
	
	void Throw()
	{
		//prepeaing
		currWea.transform.parent = null;
		currWea.layer = 17;
		ammoWi.ammo = (int)myWeapons[myWeapon].y;
		weaAnim.enabled = false;
		
		//addingForce
		Rigidbody rb = currWea.AddComponent<Rigidbody>();
		rb.AddForce(-currWea.transform.right * throwForce, ForceMode.Impulse);
		rb.AddTorque(Vector3.up * Random.Range(minYTorque, maxYTorque), ForceMode.Impulse);
		rb.AddTorque(Vector3.forward * Random.Range(minZTorque, maxZTorque), ForceMode.Impulse);
		
		//cleaning
		currWea = null;
		myWeapons.RemoveAt(myWeapon);
		myWeapon = -1;
		recalculateInventory();
		
		//equiping new
		StartCoroutine(Equip(0, 0));
	}
	
	void recalculateInventory()
	{
		for (int i = 0; i < allSlots.Length; i++)
		{
			allSlots[i].enabled = false;
		}
		
		for (int i = 0; i < myWeapons.Count; i++)
		{
			allSlots[i].enabled = true;
			if (i != myWeapon)
				allSlots[i].color = new Color32(255, 255, 255, 128);
			else
				allSlots[i].color = new Color32(255, 255, 255, 255);
			allSlots[i].sprite = assets[(int)myWeapons[i].x].mySprite;
		}
	}
	
	////////////////////////////////UI/////////////////////////////
	void MyUI()
	{
		if (a.useAmmo)
		{
			if (reloading)
			{
				float i = Mathf.Round(((float)3/(float)a.maxAmmo)*ammoWi.ammo);
				switch(i)
				{
					case 0:
						ammoText.text = "Reloading" + "/" + a.maxAmmo.ToString();
						break;
					case 1:
						ammoText.text = ". Reloading" + "/" + a.maxAmmo.ToString();
						break;
					case 2:
						ammoText.text = ".. Reloading" + "/" + a.maxAmmo.ToString();
						break;
					case 3:
						ammoText.text = "... Reloading" + "/" + a.maxAmmo.ToString();
						break;
					default:
						ammoText.text = ammoWi.ammo.ToString() + "/" + a.maxAmmo.ToString();
						break;
				}
			}
			else
				ammoText.text = ammoWi.ammo.ToString() + "/" + a.maxAmmo.ToString();
		}
		else
			ammoText.text = "Infinite";
	}
	
	////////////////////////////////ANIMATION///////////////////////
	void weaAnimation()
	{
		Vector3 addResult = new Vector3();
		if (animEnabled)
		{
			addResult = Vector3.zero;
			ticks += 1*Time.deltaTime;
			//deltas
			if (ticks > idleAnimCurveSpeed)
				ticks = 1;
			//curv movement
			addResult += new Vector3(0, idleAnimCurve.Evaluate(ticks/idleAnimCurveSpeed)/16, 0);
			
			//sway
			float xChange = Mathf.Clamp((Input.GetAxis("Mouse X") + (Input.GetAxisRaw("Horizontal") * (new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude/6))), minAndMaxXSway.x, minAndMaxXSway.y);
			float yChange = Mathf.Clamp((Input.GetAxis("Mouse Y") + (playerRb.velocity.y/8)), minAndMaxYSway.x, minAndMaxYSway.y);
			
			//calculate target dir
			Quaternion targetDir = Quaternion.AngleAxis(xChange*swayIntensity.x, -Vector3.up) * Quaternion.AngleAxis(yChange*swayIntensity.y, Vector3.right);
			//go smooth to dir
			weaPos.localRotation = Quaternion.Slerp(weaPos.localRotation, targetDir*a.normalRotation, swaySpeed*Time.deltaTime);
			//add position sway to add result
			addResult -= new Vector3(xChange*swayPosIntensity.x, yChange*swayPosIntensity.y, 0);
			
			//apply addResult to position
			weaPos.localPosition = Vector3.Lerp(weaPos.localPosition, addResult+stableLocalWeaPos, Time.deltaTime*swaySpeed);
			
		}
		else
		{
			if (!attacking)
			{
				weaPos.localRotation = Quaternion.Slerp(weaPos.localRotation, a.normalRotation, returnSpeed*Time.deltaTime);
				weaPos.localPosition = Vector3.Lerp(weaPos.localPosition, stableLocalWeaPos, returnSpeed*Time.deltaTime);
			}
		}
	}
	
	void OnDrawGizmosSelected()
	{
		if ((a.weaponType != weaponAsset.weaType.fist || a.weaponType != weaponAsset.weaType.sword) && currWea != null)
		{
			Gizmos.color = new Color(1, 0, 0, 0.75F);
			Gizmos.matrix = Matrix4x4.TRS(currWea.transform.position, currWea.transform.rotation, new Vector3(1, 1, 1));
			Gizmos.DrawWireCube(Vector3.zero, a.meleRadius);
		}
	}
}