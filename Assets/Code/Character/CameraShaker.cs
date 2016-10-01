using UnityEngine;
using System.Collections;

public class CameraShaker : MonoBehaviour 
{
	public Camera MainCamera;


	private float _tempSlowDuration;
	private float _tempSlowTimer;
	private float _shakeIntensity;
	private float _shakeDuration;
	private float _shakeTimer;
	private float _zoomIntensity;
	private float _zoomDuration;
	private float _zoomTimer;

	// Update is called once per frame
	void Update () 
	{
		if(_tempSlowDuration != 0 && !GameManager.Inst.PlayerControl.IsGamePaused)
		{
			HandleTempSlow();
		}

		if(_shakeDuration != 0 && !GameManager.Inst.PlayerControl.IsGamePaused)
		{
			HandleScreenShake();
		}

		if(_zoomDuration != 0 && !GameManager.Inst.PlayerControl.IsGamePaused)
		{
			HandleZoomShake();
		}
	}


	public void Initialize()
	{
		
	}

	public void TriggerTempSlow(float duration)
	{
		_tempSlowTimer = 0;
		_tempSlowDuration = duration;
	}

	public void TriggerScreenShake(float duration, float intensity)
	{
		_shakeIntensity = intensity;
		_shakeDuration = duration;
		_shakeTimer = 0;
	}

	public void TriggerZoomShake(float duration, float intensity)
	{
		_zoomIntensity = intensity;
		_zoomDuration = duration;
		_zoomTimer = 0;
	}




	private void HandleTempSlow()
	{
		if(_tempSlowTimer < _tempSlowDuration / 2)
		{
			_tempSlowTimer += Time.deltaTime;
			Time.timeScale = Mathf.Lerp(Time.timeScale, 0, 200 * Time.deltaTime);
		}
		else if(_tempSlowTimer >= _tempSlowDuration / 2 && _tempSlowTimer <= _tempSlowDuration)
		{
			_tempSlowTimer += Time.deltaTime;
			Time.timeScale = Mathf.Lerp(Time.timeScale, 1, 200 * Time.deltaTime);
		}
		else
		{
			Time.timeScale = 1;
			_tempSlowTimer = 0;
			_tempSlowDuration = 0;
		}
	}

	private void HandleScreenShake()
	{
		if(_shakeTimer < _shakeDuration)
		{
			MainCamera.transform.localPosition = new Vector3(UnityEngine.Random.Range(-1f, 1f) * _shakeIntensity, UnityEngine.Random.Range(-1f, 1f) * _shakeIntensity, UnityEngine.Random.Range(-1f, 1f) * _shakeIntensity);
		}

		_shakeTimer += Time.deltaTime;
	}

	private void HandleZoomShake()
	{
		if(_zoomTimer < _zoomDuration)
		{
			MainCamera.fieldOfView = MainCamera.fieldOfView - UnityEngine.Random.Range(-1f, 1f) * _zoomIntensity;
		}

		_zoomTimer += Time.deltaTime;
	}
}
