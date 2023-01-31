using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class player : MonoBehaviour
{
	#region variables
    [Header("general")]
	public GameObject mainstenMe;
	public GameObject me;
	public Transform cameraHolder;
	public Transform orientation;
	public graplingGun pg;
	private Rigidbody rb;
	
	[Header("camera")]
	public float sens;
	public float speedOfTiltWallRun;
	public float powerOfTiltWallRun;
	public Vector3 cameraOffset;
	public Transform ghostTilt;
	private float xRotation;
	private float yRotation;
	private float zRotation;
	
	[Header("movement")]
	public float walkSpeed;
	public float sprintSpeed;
	public float speedDecresseSpeed;
	public float fallGravity;
	public float walkDelayTime;
	[HideInInspector]
	public float speed;
	private float realSpeed;
	private float noMoveTimer;
	[HideInInspector]
	public bool freeze;
	private Vector3 moveDirection;
	
	[Header("crouching")]
	public float crouchScale;
	public float crouchSpeed;
	public float speedCrouchScale;
	private float normalScale;
	private bool crouching;
	
	[Header("sliding")]
	public float slideSpeedScale;
	public float slideSlopeSpeed;
	public float slideSpeed;
	public float slideForce;
	public float slideTilt;
	public float slideTiltSpeed;
	public float slideSpeedDecreese;
	public float maxSlideSpeed;
	public bool sliding;
	private bool exitingSlope;
	private bool rightSlideLean;
	private Vector3 slideDir;
	
	[Header("wallRun")]
	public float wallRunForce;
	public float wallCheckDist;
	public float minJumpHeight;
	public float wallClimbSpeed;
	public float wallJumpUpForce;
	public float wallJumpSideForce;
	public float wallExitTime;
	public float wallClimbSpeedDecreese;
	public LayerMask wallLayer;
	private RaycastHit wallLeftHit;
	private RaycastHit wallRightHit;
	private float wallExitTimer;
	private bool exitWall;
	private bool wallRight;
	private bool wallLeft;
	private bool aboveGround;
	private bool wallRunning;
	private bool wallUp;
	private bool wallDown;
	
	[Header("wallClimbing")]
	public Vector3 wallClimbCheckRadius;
	public float wallClimbDetectionLength;
	public float maxWallLookAngle;
	public float wallClimbingSpeed;
	public float climbSpeedDecreese;
	public float climbJumpBackForce;
	public float climbJumpUpForce;
	public LayerMask wallClimbLayer;
	private RaycastHit wallFrontHit;
	private bool wallFront;
	private bool climbing;
	private bool wallBack;
	private bool exitClimb;
	private float wallLookAngle;
	private float climbungTimer;
	
	[Header("jump")]
	public float jumpForce;
	public float dobleJumpForce;
	public float jumpDelay;
	public float ammountOfJumps;
	public float bunnyHopTo;
	public float jumpRemTime;
	public float jumpIncreaser;
	private float jumpRemTimer;
	private float jumpTimer;
	[HideInInspector]
	public float jumpAmm;
	
	[Header("groundcheck")]
	public LayerMask groundLayer;
	public LayerMask movingLayer;
	public float chekSize;
	public float cyotekTime;
	public float groundDrag;
	private float cyotekTimer;
	[HideInInspector]
	public bool realGrounded;
	private bool bigGrounded;
	private RaycastHit hit;
	
	[Header("input")]
	public KeyCode jumpKey;
	public KeyCode sprintKey;
	public KeyCode crouchKey;
	public KeyCode slideKey;
	public KeyCode upClimbWall;
	public KeyCode downClimbWall;
	private float vInput;
	private float hInput;
	private float lastVInput;
	private float lastHInput;
	private float mVInput;
	private float mHInput;
	[HideInInspector]
	public float noInputTimer;
	
	[Header("slopes")]
	public float maxAngle;
	public RaycastHit slopeHit;
	
	[Header("TextStuf")]
	public TextMeshProUGUI TextSpeed;
	public TextMeshProUGUI TextRealSpeed;
	public TextMeshProUGUI TextWishSpeed;
	
	[Header("swing")]
	public float swingDownEncr;
	public float swingUpDecr;
	public float minSwingSpeed;
	public float maxSwingSpeed;
	[HideInInspector]
	public float swingSpeed;
	
	[Header("Animation")]
	public float speedForParticles;
	public GameObject speedParticles;
	
	[Header("cameraShake")]
	public NormalCameraShake csh;
	public float fadeOutDuration;
	public float magnitude;
	public float raughnes;
	public float fadeInDuration;
	public Vector3 posInFluence;
	public Vector3 rotInFluence;
	
	private bool once;
	
	
	[Header("Other")]
	public float debug;
	
	#endregion variables
	
	void Start()
	{
		rb = me.GetComponent<Rigidbody>();
		
		normalScale = me.transform.localScale.y;
		
		Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
	}
	
	void Update()
	{
		groundcheck();
		gravity();
		
		if (jumpTimer >= 0)
			jumpTimer -= Time.deltaTime;
		
		if (noInputTimer < 0)
			realSpeed = speed = walkSpeed;
		
		if (climbungTimer >= 0)
			climbungTimer -= Time.deltaTime;
		
		if (noMoveTimer >= 0)
			noMoveTimer -= Time.deltaTime;
		
		if (pg.activeGrapple)
		{
			noInputTimer = 1f;
			return;
		}
		
		if (freeze)
		{
			rb.velocity = Vector3.zero;
			return;
		}
		
		smoothSpeed();
		myInput();
		SpeedLimit();
		TextAndUI();
		crouchingScale();
		slidingScale();
		wallCheck();
		wallClimbCheck();
		Animation();
	}
	
	void LateUpdate()
	{
		cameraRot();
	}
	
	void FixedUpdate()
	{
		if (freeze || pg.activeGrapple)
			return;
		
		movement();
		
		if (sliding)
			slidingMovement();
		
		if (wallRunning)
			wallRunningMovement();
		
		if (climbing)
			ClimbingMovement();
	}
	
	
	/////////////////input///////////////
	void myInput()
	{
		vInput = Input.GetAxisRaw("Vertical");
		hInput = Input.GetAxisRaw("Horizontal");
		
		if (!pg.swing)
		{
			//jump
			if (Input.GetKeyDown(jumpKey) && !wallRunning && !climbing)
				jump(true);
			if (Input.GetKeyDown(jumpKey) && cyotekTimer < 0 && jumpAmm == 0 && !wallRunning && !climbing)
				jumpRemTimer = jumpRemTime;
			if (jumpRemTimer > 0 && cyotekTimer > 0)
				jump(false);
			else if (jumpRemTimer >= 0)
				jumpRemTimer -= Time.deltaTime;
			
			//crouching
			if (Input.GetKeyDown(crouchKey) && !sliding && cyotekTimer > 0 && !wallRunning)
				startCrouch();
			else if ((Input.GetKeyUp(crouchKey) || cyotekTimer > 0) && crouching)
				stopCrouch();
			
			//sliding
			if (Input.GetKeyDown(slideKey) && (hInput != 0 || vInput != 0) && !crouching && !wallRunning)
				startSlide();
			else if (Input.GetKeyUp(slideKey) && sliding)
				stopSlide();
			
			//wallRunning
			if ((wallLeft || wallRight) && !crouching && !sliding && aboveGround && vInput != 0 && !exitWall && realSpeed > 0.5f)
			{
				if (!wallRunning)
					startWallRun();
				
				if (Input.GetKeyDown(jumpKey))
					wallJump();
			}
			else if (exitWall)
			{
				if (wallRunning)
					StopWallRun();
				if (wallExitTimer > 0)
					wallExitTimer -= Time.deltaTime;
				if (wallExitTimer <= 0)
					exitWall = false;
			}
			else 
				if (wallRunning)
					StopWallRun();
				
			wallUp = Input.GetKey(upClimbWall);
			wallDown = Input.GetKey(downClimbWall);
			
			//sprinting/walking/air
			if (Input.GetKey(sprintKey) && !crouching && !sliding && !wallRunning && realGrounded)
				Invoke("walkDelay", walkDelayTime);
			else if (!crouching && !sliding && !wallRunning && realGrounded)
				Invoke("walkDelay", walkDelayTime);
			
			//wallClimbing
			if (!exitClimb && wallFront && (wallBack ? Input.GetKey(KeyCode.S) : Input.GetKey(KeyCode.W)) && wallLookAngle < maxWallLookAngle)
			{
				StartClimbing();
				if (Input.GetKeyDown(jumpKey))
					climbJump();
			}
			else if (climbing)
				StopClimbing();
		}
		else
		{
			if (rb.velocity.y > 0.2f && speed > minSwingSpeed)
				swingSpeed -= swingUpDecr;
			if (rb.velocity.y < -0.2f && speed < maxSwingSpeed)
				swingSpeed += swingDownEncr;
			speed = swingSpeed;
		}
	}
	
	void walkDelay()
	{
		if (Input.GetKey(sprintKey) && !crouching && !sliding && !wallRunning && realGrounded)
			speed = sprintSpeed;
		else if (!crouching && !sliding && !wallRunning && realGrounded)
			speed = walkSpeed;
	}
	
	
	/////////////////groundcheck/////////
	void groundcheck()
	{
		if (cyotekTimer >= 0)
			cyotekTimer -= Time.deltaTime;
		
		bigGrounded = Physics.Raycast(orientation.position, Vector3.down, chekSize * 2.5f, groundLayer);
		RaycastHit het;
		if (Physics.Raycast(orientation.position, Vector3.down, out het, chekSize, groundLayer))
		{
			
			realGrounded = true;
			cyotekTimer = cyotekTime;
			jumpAmm = ammountOfJumps;
			if (!pg.activeGrapple)
				rb.drag = groundDrag;
			else
				rb.drag = 0;
		}
		else
		{
			realGrounded = false;
			rb.drag = 0;
		}
		
		if (Physics.Raycast(orientation.position, Vector3.down, out het, chekSize, movingLayer))
		{
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			mainstenMe.transform.SetParent(het.transform);
		}
		else if (mainstenMe.transform.parent != null)
		{
			rb.interpolation = RigidbodyInterpolation.Interpolate;
			mainstenMe.transform.parent = null;
		}
		
		//groundcheckForWall
		aboveGround = !Physics.Raycast(orientation.position, Vector3.down, minJumpHeight, groundLayer);
	}
	
	
	/////////////////jump////////////////
	void jump(bool dubble)
	{
		if (jumpTimer < 0)
		{
			if (cyotekTimer > 0)
			{
				rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
				exitingSlope = true;
				Invoke("jumpReset", jumpDelay);
				cyotekTimer = -1;
				jumpTimer = jumpDelay;
				if (speed + 0.5f <= bunnyHopTo && moveDirection != Vector3.zero)
					speed = realSpeed > bunnyHopTo ? bunnyHopTo : (realSpeed + jumpIncreaser);
				rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
			}
			else if (jumpAmm > 0 && dubble)
			{
				rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
				jumpAmm--;
				jumpTimer = jumpDelay;
				rb.AddForce(Vector3.up * dobleJumpForce, ForceMode.Impulse);
			}
		}
	}
	
	void jumpReset()
	{
		exitingSlope = false;
	}
	
	void gravity()
	{
		if (rb.velocity.y < -0.1f)
			Physics.gravity = new Vector3(0, -fallGravity, 0);
		else
			Physics.gravity = new Vector3(0, -9.81f, 0);
		
		if (pg.activeGrapple)
			Physics.gravity = new Vector3(0, -9.81f, 0);
	}
	
	
	/////////////////movement////////////
	void movement()
	{
		moveDirection = orientation.forward * vInput + orientation.right * hInput;
		
		if (OnSlope() && !exitingSlope)
		{
			rb.AddForce(SlopeDir(moveDirection) * realSpeed * 20);
			if (rb.velocity.y > 0)
				rb.AddForce(Vector3.down * 40);
			rb.useGravity = false;
		}
		else if (!exitClimb)
		{
			rb.useGravity = true;
			rb.AddForce(moveDirection * realSpeed * 10);
		}
		if (vInput == 0 && hInput == 0 && !pg.swing)
			noInputTimer -= Time.deltaTime;
		else
			noInputTimer = 0.5f;
		
	}
	
	
	/////////////////crouching///////////
	void startCrouch()
	{
		rb.AddForce(Vector3.down * 5, ForceMode.Impulse);
		speed = crouchSpeed;
		crouching = true;
	}
	
	void crouchingScale()
	{
		if (crouching)
		{
			me.transform.localScale = Vector3.Lerp(me.transform.localScale, new Vector3(me.transform.localScale.x, crouchScale, me.transform.localScale.z), speedCrouchScale * Time.deltaTime);
			cameraOffset = Vector3.Lerp(cameraOffset, new Vector3(cameraOffset.x, 0.3f, cameraOffset.z), speedCrouchScale * Time.deltaTime);
		}
		else if (!sliding && !crouching)
		{
			me.transform.localScale = Vector3.Lerp(me.transform.localScale, new Vector3(me.transform.localScale.x, normalScale, me.transform.localScale.z), speedCrouchScale * Time.deltaTime);
			cameraOffset = Vector3.Lerp(cameraOffset, new Vector3(cameraOffset.x, 0.6f, cameraOffset.z), speedCrouchScale * Time.deltaTime);
		}
	}
	
	void stopCrouch()
	{
		rb.AddForce(Vector3.down * 5, ForceMode.Impulse);
		crouching = false;
	}
	
	
	/////////////////sliding/////////////
	void startSlide()
	{
		sliding = true;
		if (realGrounded)
		{
			rb.AddForce(Vector3.down * 5, ForceMode.Impulse);
			if (Random.Range(-2, 2) > 0)
				rightSlideLean = true;
			else
				rightSlideLean = false;
		}
		if ((speed + slideSpeed) < maxSlideSpeed && bigGrounded && !OnSlope())
			speed += slideSpeed;
	}
	
	void slidingMovement()
	{
		slideDir = orientation.forward * vInput + orientation.right * hInput;
		
		if (OnSlope() && rb.velocity.y < 0.1f)
		{
			rb.AddForce(SlopeDir(slideDir) * slideForce);
			if (speed < 20)
				speed += slideSpeedDecreese;
		}
		else if (OnSlope())
		{
			rb.AddForce(SlopeDir(slideDir) * slideForce);
			if (speed > -0.1)
				speed -= slideSpeedDecreese * 2;
		}
		else
		{
			rb.AddForce(slideDir.normalized * slideForce);
			
			if (realGrounded && speed > -0.1f)
				speed -= slideSpeedDecreese;
		}
		if (realGrounded)
			zRotation = Mathf.Lerp(cameraHolder.eulerAngles.z > 180 ? WrapAngle(cameraHolder.eulerAngles.z) : cameraHolder.eulerAngles.z, rightSlideLean ? slideTilt : -slideTilt, Time.deltaTime * slideTiltSpeed);
	}
	
	void stopSlide()
	{
		sliding = false;
	}
	
	void slidingScale()
	{
		if (sliding)
		{
			me.transform.localScale = Vector3.Lerp(me.transform.localScale, new Vector3(me.transform.localScale.x, crouchScale, me.transform.localScale.z), slideSpeedScale * Time.deltaTime);
			cameraOffset = Vector3.Lerp(cameraOffset, new Vector3(cameraOffset.x, 0.35f, cameraOffset.z), slideSpeedScale * Time.deltaTime);
			if (realSpeed > -0.3f && realSpeed < 0.3f)
				stopSlide();
		}
	}
	
	
	/////////////////wallRunning/////////
	void wallCheck()
	{
		wallRight = Physics.Raycast(orientation.position, orientation.right, out wallRightHit, wallCheckDist, wallLayer);
		wallLeft = Physics.Raycast(orientation.position, -orientation.right, out wallLeftHit, wallCheckDist, wallLayer);
	}
	
	void startWallRun()
	{
		wallRunning = true;
	}
	
	void wallRunningMovement()
	{
		Vector3 wallNormal = wallRight ? wallRightHit.normal : wallLeftHit.normal;
		Vector3 wallForward = Vector3.Cross(wallNormal, orientation.transform.up);
		
		if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
			wallForward = -wallForward;
		
		rb.useGravity = false;
		rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
		rb.AddForce(wallForward * wallRunForce);
		
		if (wallUp)
		{
			realSpeed -= wallClimbSpeedDecreese;
			speed = realSpeed;
			rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
		}
		else if (wallDown)
		{
			realSpeed -= wallClimbSpeedDecreese;
			speed = realSpeed;
			rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);
		}
		
		if (!(wallLeft && hInput > 0) && !(wallRight && hInput < 0))
			rb.AddForce(-wallNormal * 100);
		
		//camera tilt
		zRotation = Mathf.Lerp(cameraHolder.eulerAngles.z > 180 ? WrapAngle(cameraHolder.eulerAngles.z) : cameraHolder.eulerAngles.z, wallRight ? powerOfTiltWallRun : -powerOfTiltWallRun, Time.deltaTime * speedOfTiltWallRun);
	}
	
	void StopWallRun()
	{
		wallRunning = false;
	}
	
	void wallJump()
	{
		exitWall = true;
		wallExitTimer = wallExitTime;
		jumpAmm = ammountOfJumps;
		StopWallRun();
		if (speed <= bunnyHopTo && moveDirection != Vector3.zero)
			speed += 0.5f;
		Vector3 wallNormal = wallRight ? wallRightHit.normal : wallLeftHit.normal;
		rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
		rb.AddForce(wallNormal * wallJumpSideForce, ForceMode.Impulse);
		Invoke("secondWallJump", 0.05f);
	}
	
	void secondWallJump()
	{
		rb.AddForce(Vector3.up * wallJumpUpForce, ForceMode.Impulse);
	}
	
	
	/////////////////wallClimbing////////
	void wallClimbCheck()
	{
		wallFront = Physics.BoxCast(orientation.position, wallClimbCheckRadius, orientation.forward, out wallFrontHit, orientation.rotation, wallClimbDetectionLength, wallClimbLayer);
		if (wallFront)
		{
			wallBack = false;
			wallLookAngle = Vector3.Angle(orientation.forward, -wallFrontHit.normal);
		}
		else
		{
			wallBack = true;
			wallFront = Physics.BoxCast(orientation.position, wallClimbCheckRadius, -orientation.forward, out wallFrontHit, orientation.rotation, wallClimbDetectionLength, wallClimbLayer);
			wallLookAngle = Vector3.Angle(-orientation.forward, -wallFrontHit.normal);
		}
	}
	
	void StartClimbing()
	{
		climbing = true;
	}
	
	void ClimbingMovement()
	{
		if (realSpeed > 2)
			realSpeed -= climbSpeedDecreese;
		else
		{
			rb.AddForce(-Vector3.up * 0.5f, ForceMode.Impulse);
			StopClimbing();
			return;
		}
		climbungTimer = 0.3f;
		rb.velocity = new Vector3(0, wallClimbingSpeed * realSpeed, 0);
		speed = realSpeed;
	}
	
	void StopClimbing()
	{
		climbing = false;
	}
	
	void climbJump()
	{
		rb.AddForce((wallBack ? orientation.forward : -orientation.forward) * climbJumpBackForce * (realSpeed / realSpeed > 6 ? 2 : 1));
		StopClimbing();
		jumpAmm = ammountOfJumps;
		exitClimb = true;
		Invoke("secondClimbJump", 0.05f);
		Invoke("exitClimbDelay", 0.3f);
	}
	
	void secondClimbJump()
	{
		rb.AddForce(Vector3.up * climbJumpUpForce * (realSpeed / 2));
	}
	
	void exitClimbDelay()
	{
		exitClimb = false;
	}
	
	
	/////////////////speedLimitation/////
	void SpeedLimit()
	{
		if (OnSlope() && !exitingSlope)
		{
			if (rb.velocity.magnitude > realSpeed)
				rb.velocity = rb.velocity.normalized * realSpeed;
		}
		else
		{
			Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);
			if (flatVel.magnitude > realSpeed)
			{
				Vector3 limitedVel = flatVel.normalized * realSpeed;
				rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
			}
		}
	}
	
	//rules of speed
	void smoothSpeed()
	{
		if ((rb.drag != 0 && speed < realSpeed) || pg.swing)
			realSpeed -= speedDecresseSpeed * Time.deltaTime * (sliding ? 5 : 1);
		
		if (speed > realSpeed && !climbing)
			realSpeed = speed;
		
		if (rb.velocity.magnitude < 0.01f && climbungTimer < 0)
			noMoveTimer = 0.3f;
		
		if (noMoveTimer > 0 && rb.velocity.magnitude < 0.01f)
			speed = realSpeed = walkSpeed;
		
		if (speed > 15 || realSpeed > 15)
			speed = realSpeed = 15;
	}
	
	/////////////////slopes//////////////
	bool OnSlope()
	{
		if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, chekSize))
		{
			float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
			return angle < maxAngle && angle != 0;
		}
		
		return false;
	}
	
	Vector3 SlopeDir(Vector3 dir)
	{
		return Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;
	}
	
	
	/////////////////animation/particles/
	void Animation()
	{
		if (speedForParticles < rb.velocity.magnitude)
		{
			if (!once)
			{
				once = true;
				csh.StartShake(fadeOutDuration, magnitude, raughnes, fadeInDuration, posInFluence, rotInFluence);
			}
			speedParticles.SetActive(true);
		}
		else
		{
			once = false;
			csh.StopShake();
			speedParticles.SetActive(false);
		}
	}
	
	
	/////////////////camera//////////////
	void cameraRot()
	{
		mHInput = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;
		mVInput = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sens;
		
		yRotation += mHInput;
        xRotation -= mVInput;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
		
		cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);
		orientation.rotation = Quaternion.Euler(0, yRotation, 0);
		
		cameraHolder.position = me.transform.position + cameraOffset;
		
		if (!wallRunning && !sliding && (cameraHolder.eulerAngles.z > 0.1f || cameraHolder.eulerAngles.z < -0.1f))
			zRotation = Mathf.Lerp(cameraHolder.eulerAngles.z > 180 ? WrapAngle(cameraHolder.eulerAngles.z) : cameraHolder.eulerAngles.z, 0, speedOfTiltWallRun * Time.deltaTime);
	}
	
	
	
	
	/////////////////OnCollisionEnter()//
	
	void OnCollisionEnter(Collision col)
	{
		if (pg.activeGrapple)
		{
			pg.StopG();
			pg.activeGrapple = false;
		}
	}
	
	
	/////////////////TextAndUI///////////
	void TextAndUI()
	{
		TextSpeed.text = (Mathf.Round(new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude * 100) / 100).ToString();
		TextRealSpeed.text = (Mathf.Round(realSpeed * 100) / 100).ToString();
		TextWishSpeed.text = (Mathf.Round(speed * 100) / 100).ToString();
	}
	
	
	/////////////////gizmos//////////////
	void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(1, 1, 0, 0.75F);
        Gizmos.DrawWireCube(orientation.position + orientation.forward * wallClimbDetectionLength, wallClimbCheckRadius);
	}
}
