using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalCameraShake : MonoBehaviour
{
	public Transform defaultCamerasToShake;
	public float defaultFadeOutDuration; // 1
	public float defaultMagnitude; // 1
	public float defaultRaughnes; // 1
	public float defaultFadeInDuration; // 1
	public Vector3 defaultPosInFluence; // 0.15, 0.15, 0.15 
	public Vector3 defaultRotInFluence; // 1, 1, 1
	
	float fadeOutDuration;
	float magnitude;
	float raughnes;
	float fadeInDuration;
	Vector3 posInFluence;
	Vector3 rotInFluence;
	bool shaking;
	bool fadingOut;
	bool fadingIn;
	float tick;
	float currentFadeTime;
	Vector3 amt;
	
	
	public void StartShake()
	{
		fadeOutDuration = defaultFadeOutDuration;
		magnitude = defaultMagnitude;
		raughnes = defaultRaughnes;
		fadeInDuration = defaultFadeInDuration;
		posInFluence = defaultPosInFluence;
		rotInFluence = defaultRotInFluence;
		
		fadingIn = true;
		fadingOut = false;
		shaking = true;
		
		currentFadeTime = 0;
		tick = Random.Range(-100, 100);
	}
	
	public void StartShake(float fadeOutDurationA, float magnitudeA, float raughnesA, float fadeInDurationA, Vector3 posInFluenceA, Vector3 rotInFluenceA)
	{
		fadeOutDuration = fadeOutDurationA;
		magnitude = magnitudeA;
		raughnes = raughnesA;
		fadeInDuration = fadeInDurationA;
		posInFluence = posInFluenceA;
		rotInFluence = rotInFluenceA;
		
		fadingIn = true;
		fadingOut = false;
		shaking = true;
		
		currentFadeTime = 0;
		tick = Random.Range(-100, 100);
	}
	
	public void ShakeOnce(float fadeOutDurationA, float magnitudeA, float raughnesA, float fadeInDurationA, float shakeDuration, Vector3 posInFluenceA, Vector3 rotInFluenceA)
	{
		fadeOutDuration = fadeOutDurationA;
		magnitude = magnitudeA;
		raughnes = raughnesA;
		fadeInDuration = fadeInDurationA;
		posInFluence = posInFluenceA;
		rotInFluence = rotInFluenceA;
		
		fadingIn = true;
		fadingOut = false;
		shaking = true;
		
		currentFadeTime = 0;
		tick = Random.Range(-100, 100);
		if (shakeDuration - fadeOutDurationA - fadeInDurationA > 0)
			Invoke("StopShake", shakeDuration - fadeOutDurationA - fadeInDurationA);
		else if (shakeDuration > 0)
			Invoke("StopShake", shakeDuration);
	}
	
	public void ShakeOnce(float shakeDuration)
	{
		fadeOutDuration = defaultFadeOutDuration;
		magnitude = defaultMagnitude;
		raughnes = defaultRaughnes;
		fadeInDuration = defaultFadeInDuration;
		posInFluence = defaultPosInFluence;
		rotInFluence = defaultRotInFluence;
		
		fadingIn = true;
		fadingOut = false;
		shaking = true;
		
		currentFadeTime = 0;
		tick = Random.Range(-100, 100);
		if (shakeDuration > 0)
			Invoke("StopShake", shakeDuration);
	}
	
	public void StopShake()
	{
		fadingIn = false;
		fadingOut = true;
	}
	
	void Update()
	{
		if (shaking)
		{
			defaultCamerasToShake.localPosition = MultiplyVectors(Shake(), posInFluence);
			defaultCamerasToShake.localEulerAngles = MultiplyVectors(Shake(), rotInFluence);
			/* for more than one camera
			for (int i = 0; i < defaultCamerasToShake.Length; i++)
			{
				defaultCamerasToShake[i].position = MultiplyVectors(Shake(), posInFluence);
				defaultCamerasToShake[i].position = MultiplyVectors(Shake(), rotInFluence);
			}
			*/
		}
		else
		{
			defaultCamerasToShake.localPosition = Vector3.zero;
			defaultCamerasToShake.localEulerAngles = Vector3.zero;
		}
	}
	
	Vector3 Shake()
	{
		amt.x = Mathf.PerlinNoise(tick, 0) - 0.5f;
        amt.y = Mathf.PerlinNoise(0, tick) - 0.5f;
        amt.z = Mathf.PerlinNoise(tick, tick) - 0.5f;
		
		if (fadingIn)
		{
			if (currentFadeTime < 1)
			{
				currentFadeTime += Time.deltaTime / fadeInDuration;
			}
			else
				fadingIn = false;
		}
		else if (fadingOut)
		{
			if (currentFadeTime > 0)
			{
				currentFadeTime -= Time.deltaTime / fadeOutDuration;
			}
			else
			{
				fadingOut = false;
				shaking = false;
			}
		}
		else
			currentFadeTime = 1;
		
		if (!fadingIn && !fadingOut)
            tick += Time.deltaTime * raughnes;
        else
            tick += Time.deltaTime * raughnes * currentFadeTime;
		
        return amt * magnitude * currentFadeTime;
	}
	
    ////////Utilities//////
	public static Vector3 MultiplyVectors(Vector3 v, Vector3 w)
    {
        v.x *= w.x;
        v.y *= w.y;
        v.z *= w.z;

        return v;
    }
}
