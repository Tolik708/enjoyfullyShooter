using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Random=UnityEngine.Random;

public class weaponSystem : MonoBehaviour
{
	[Header("General")]
	public player pm;
	public weaponAsset[] assets;
	public weaponAsset a;
	
	[Header("shooting")]
	public Rigidbody playerRb;
	public Transform cam;
	private Transform gunTip;
	private List<bulletAddon> shootedBulls = new List<bulletAddon>();
	
	[Header("mele")]
	public Transform meleAtackPos;
	
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
	public Transform weaPos;
	public float returnRotationSpeed;
	private Animator weaAnim;
	private bool animEnabled = true;
	
	//sway
	public float swaySpeed;
	public float swayIntensity;
	public Vector2 minAndMaxXSway;
	public Vector2 minAndMaxYSway;
	
	//curves
	public AnimationCurve idleAnimCurve;
	public float idleAnimCurveSpeed;
	private Vector3 addResult = Vector3.zero;
	private int delta1 = 1;
	
	void Start()
	{	
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
			if(myWeapon > 0)
			{
				Throw();
			}
		}
		
		//shooting
		if (canShoot)
		{
			if (a.useAmmo)
			{
				if (a.multiShoot && shootDelayTimer < 0 && Input.GetKey(a.shootKey) && ammoWi.ammo > 0)
				{
					if (reloading)
					{
						StopCoroutine(reload());
						shootDelayTimer = a.shootDelay;
						shoot();
						ammoWi.ammo--;
						myWeapons[myWeapon] = new Vector2(myWeapons[myWeapon].x, ammoWi.ammo);
					}
					else
					{
						shootDelayTimer = a.shootDelay;
						shoot();
						ammoWi.ammo--;
						myWeapons[myWeapon] = new Vector2(myWeapons[myWeapon].x, ammoWi.ammo);
					}
				}
				else if (!a.multiShoot && shootDelayTimer < 0 && Input.GetKey(a.shootKey) && ammoWi.ammo > 0)
				{
					if (reloading)
					{
						StopCoroutine(reload());
						shootDelayTimer = a.shootDelay;
						shoot();
						ammoWi.ammo--;
						myWeapons[myWeapon] = new Vector2(myWeapons[myWeapon].x, ammoWi.ammo);
					}
					else
					{
						shootDelayTimer = a.shootDelay;
						shoot();
						ammoWi.ammo--;
						myWeapons[myWeapon] = new Vector2(myWeapons[myWeapon].x, ammoWi.ammo);
					}
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
			weaAnim.SetFloat("speed", i > a.reloadTime/2 ? 1/i : 1/a.reloadTime/2);
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
		yield return null;
	}
	
	////////////////////////SHOOT////////////////////////
	void shoot()
	{
		//animation
		weaAnim.SetFloat("speed", 1 / a.shootTime);
		weaAnim.Play("shoot");
		
		if (a.multiBull)
		{
			for (int i = 0; i < a.bullCount; i++)
			{
				GameObject bull = Instantiate(a.bullPrefab, gunTip.position, gunTip.rotation);
				bull.transform.Rotate(Random.Range(a.randXRot, -a.randXRot), Random.Range(a.randYRot, -a.randYRot), 0);
				Rigidbody bullRb = bull.GetComponent<Rigidbody>();
				float playerSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
				bullRb.AddForce(((a.randSpeed == false ? a.bullSpeed : Random.Range(a.minRandSpeed, a.minRandSpeed)) + playerSpeed) * bull.transform.forward, ForceMode.Impulse);
				bulletAddon bullBAddon = bull.GetComponent<bulletAddon>();
				bullBAddon.weaAsset = a;
				shootedBulls.Add(bullBAddon);
			}
		}
		else if (a.pattern)
		{
			GameObject bullet = Instantiate(a.bullPrefab, gunTip.position, gunTip.rotation);
			
			RaycastHit bellHit;
			if (Physics.Raycast(cam.position, cam.forward, out bellHit, 200, a.collidingLayers))
				bullet.transform.LookAt(bellHit.point);
			else
				bullet.transform.LookAt(cam.position + cam.forward * 200); 
			for (int i = 0; i < a.bullAmmount; i++)
			{
				GameObject bull = bullet.transform.GetChild(i).gameObject;
				Rigidbody bullRb = bull.GetComponent<Rigidbody>();
				float playerSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
				bullRb.AddForce(((a.randSpeed == false ? a.bullSpeed : Random.Range(a.minRandSpeed, a.minRandSpeed)) + playerSpeed) * bull.transform.forward, ForceMode.Impulse);
				bulletAddon bullBAddon = bull.GetComponent<bulletAddon>();
				bullBAddon.weaAsset = a;
				shootedBulls.Add(bullBAddon);
			}
		}
		else if (a.mele)
		{
			
		}
		else
		{
			Vector3 forceDir;
			RaycastHit bellHit;
			if (Physics.Raycast(cam.position, cam.forward, out bellHit, 200, a.collidingLayers))
				forceDir = (bellHit.point - gunTip.position).normalized;
			else
				forceDir = cam.forward;
			GameObject bull = Instantiate(a.bullPrefab, gunTip.position, gunTip.rotation);
			Rigidbody bullRb = bull.GetComponent<Rigidbody>();
			float playerSpeed = new Vector3(playerRb.velocity.x, 0, playerRb.velocity.z).magnitude;
			bullRb.AddForce(((a.randSpeed == false ? a.bullSpeed : Random.Range(a.minRandSpeed, a.minRandSpeed)) + playerSpeed) * forceDir, ForceMode.Impulse);
			bulletAddon bullBAddon = bull.GetComponent<bulletAddon>();
			bullBAddon.weaAsset = a;
			shootedBulls.Add(bullBAddon);
			if (a.scater)
			{
				bullRb.AddForce(bull.transform.up * (Random.Range(1, 3) == 1 ? a.yScater : -a.yScater));
				bullRb.AddForce(bull.transform.right * (Random.Range(1, 3) == 1 ? a.xScater : -a.xScater));
			}
		}
		
		//kick
		if (a.useKick)
		{
			playerRb.velocity = new Vector3(playerRb.velocity.x, playerRb.velocity.y > 0.1f ? playerRb.velocity.y : 0, playerRb.velocity.z);
			if (pm.realGrounded)
				playerRb.AddForce(-new Vector3(cam.forward.x * a.powerOfKickGroundX, cam.forward.y * a.powerOfKickGroundY, cam.forward.z * a.powerOfKickGroundX), ForceMode.Impulse);
			else
				playerRb.AddForce(-new Vector3(cam.forward.x * a.powerOfKickFlyX, cam.forward.y * a.powerOfKickFlyY, cam.forward.z * a.powerOfKickFlyX), ForceMode.Impulse);
			pm.eddForce(2);
		}
	}
	
	///////////////////////EQUIP/////////////////////////
	IEnumerator Equip(int i, int u)
	{
		//prepeaing
		canShoot = false;
		canChange = false;
		animEnabled = false;
		weaPos.transform.localRotation = Quaternion.identity;
		
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
		
		//init
		myWeapon = u;
		a = assets[i];
		
		currWea = Instantiate(a.weaModel, weaPos.position, a.normalRotation);
		currWea.transform.parent = weaPos;
		weaPos.transform.localRotation = a.normalRotation;
		ammoWi = currWea.GetComponent<weaponIndex>();
		ammoWi.ammo = (int)myWeapons[myWeapon].y;
		if (!a.mele)
			gunTip = currWea.transform.GetChild(0);
		
		//animation
		weaAnim = currWea.GetComponent<Animator>();
		weaAnim.SetFloat("speed", 1 / a.equipTime);
		weaAnim.Play("equip");
		
		yield return new WaitForSeconds(a.unequipTime-0.1f);
		
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
		weaPos.transform.localRotation = Quaternion.identity;
		Destroy(newWeapon.GetComponent<Rigidbody>());
		
		//unequiping
		if (currWea != null)
		{
			weaAnim.SetFloat("speed", 1 / a.unequipTime);
			weaAnim.Play("unequip");
			yield return new WaitForSeconds(a.unequipTime);
			Destroy(currWea);
		}
		
		//slots
		recalculateInventory();
		allSlots[u].color = new Color32(255, 255, 255, 255);
		
		//init
		a = assets[i];
		myWeapon = u;
		
		currWea = newWeapon;
		currWea.layer = 10;
		ammoWi = currWea.GetComponent<weaponIndex>();
		myWeapons.Add(new Vector2(i, ammoWi.ammo));
		if (!a.mele)
			gunTip = currWea.transform.GetChild(0);
		
		//moving
		int delta = 0;
		while ((newWeapon.transform.position - weaPos.transform.position).sqrMagnitude > 0.1f)
		{
			delta++;
			newWeapon.transform.position = Vector3.Lerp(newWeapon.transform.position, weaPos.transform.position, delta/newWeaSpeed);
			yield return 0;
		}
		newWeapon.transform.position = weaPos.transform.position;
		currWea.transform.parent = weaPos;
		
		//rotation
		delta = 0;
		while (Quaternion.Angle(currWea.transform.localRotation, a.normalRotation) > 0.1f)
		{
			delta++;
			currWea.transform.localRotation = Quaternion.Lerp(currWea.transform.localRotation, a.normalRotation, delta/newWeaSpeed);
			yield return 0;
		}
		weaPos.transform.localRotation = a.normalRotation;
		
		//animation
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
			ammoText.text = ammoWi.ammo.ToString() + "/" + a.maxAmmo.ToString();
		else
			ammoText.text = "Infinite";
	}
	
	////////////////////////////////ANIMATION///////////////////////
	void weaAnimation()
	{
		if (animEnabled)
		{
			//sway
			float xChange = Mathf.Clamp((Input.GetAxis("Mouse X") + (Input.GetAxisRaw("Horizontal") * new Vector3(playerRb.velocity.x, 0, playerRb.velocity.y).magnitude)), minAndMaxXSway.x, minAndMaxXSway.y);
			float yChange = Mathf.Clamp((Input.GetAxis("Mouse Y") + (playerRb.velocity.y / 2)), minAndMaxYSway.x, minAndMaxYSway.y);
			
			//calculate target dir
			Quaternion targetDir;
			if (Mathf.Approximately(a.normalRotation.x, Quaternion.identity.x) && Mathf.Approximately(a.normalRotation.y, Quaternion.identity.y) && Mathf.Approximately(a.normalRotation.z, Quaternion.identity.z))
				targetDir = Quaternion.AngleAxis(xChange*swayIntensity, -Vector3.up) * Quaternion.AngleAxis(yChange*swayIntensity, Vector3.right);
			else
				targetDir = Quaternion.AngleAxis(xChange*swayIntensity, -Vector3.up) * Quaternion.AngleAxis(yChange*swayIntensity, Vector3.right) * a.normalRotation;
			//go smooth to dir
			weaPos.localRotation = Quaternion.Slerp(weaPos.localRotation, targetDir, swaySpeed*Time.deltaTime);
			
			//deltas
			if (delta1 > idleAnimCurveSpeed)
				delta1 = 1;
			
			//curv movement
			weaPos.Translate(-addResult * Time.deltaTime);	
			addResult = new Vector3(0, idleAnimCurve.Evaluate(delta1/idleAnimCurveSpeed), 0);
			weaPos.Translate(addResult * Time.deltaTime);
			
			delta1++;
		}
		else
		{
			weaPos.localRotation = Quaternion.Slerp(weaPos.localRotation, a.normalRotation, returnRotationSpeed*Time.deltaTime);
		}
	}
}