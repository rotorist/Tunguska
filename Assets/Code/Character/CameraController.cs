using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{

	#region Public Fields
	public float RotateSpeed;
	public float PanSpeed;
	public float MaxPanDist;
	public float HighFov;
	public float LowFov;
	public Camera MainCamera;
	public AnimationCurve CameraAngleCurve;
	public AnimationCurve PanDistCurve;
	public AnimationCurve VignetteCurve;
	#endregion

	#region Private Fields
	private CameraModeEnum _cameraMode;
	private Vector3 _cameraPos;

	private bool _isRotatingLeft;
	private bool _isRotatingRight;
	private bool _isPanningLeft;
	private bool _isPanningRight;
	private bool _isPanningUp;
	private bool _isPanningDown;

	private bool _isLookingAhead;

	private float _delayTimer;

	private int _currentRotation; //1-8

	private float _rotation;

	private float _cameraAngle1;
	private float _cameraAngle2;
	private float _maxFov;
	#endregion

	void Update()
	{
		if(_rotation < 0)
		{
			transform.RotateAround(GameManager.Inst.PlayerControl.SelectedPC.transform.position, Vector3.up, _rotation);

		}
		else if(_rotation > 0)
		{
			transform.RotateAround(GameManager.Inst.PlayerControl.SelectedPC.transform.position, Vector3.up, _rotation);

		}
		_rotation = Mathf.Lerp(_rotation, 0, 5 * Time.unscaledDeltaTime);


		HumanCharacter pc = GameManager.Inst.PlayerControl.SelectedPC;
		Vector3 cameraFacing = Camera.main.transform.forward;

		float cameraHeight = 18;
		float cameraDistFromPlayer = 25;



		float cameraFov = _maxFov;
		if(_cameraMode == CameraModeEnum.Party)
		{
			cameraFov = _maxFov + 10;
		}

		_cameraPos = pc.transform.position - cameraFacing * cameraDistFromPlayer;
		_cameraPos = new Vector3(_cameraPos.x, cameraHeight, _cameraPos.z);

		/*
		Vector3 targetEuler = new Vector3(0, _currentRotation * 45, 0);
		Quaternion rotation = Quaternion.Euler(targetEuler);
		transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * 5);
		*/

		Vector3 mousePos = Input.mousePosition;
		mousePos.x -= Screen.width/2;
		mousePos.y -= Screen.height/2;
		float mouseAngle = Vector2.Angle(mousePos, new Vector2(0, 1));
		//float mouseAngle2 = Vector2.Angle(mousePos, new Vector2(1, 0));


		Vector3 aimDir = pc.AimPoint - pc.transform.position;
		Vector3 cameraPanDir = mousePos.normalized;//aimDir.normalized;

		cameraPanDir = new Vector3(cameraPanDir.x, 0, cameraPanDir.y);
		cameraPanDir = transform.TransformDirection(cameraPanDir).normalized * 0.75f;

		//panning distance is 0 when aimDir magnitude is less than 2
		//when greater than 2, slowly increase the distance up to say 7


		float maxMousePos = Screen.height * 0.5f * Mathf.Abs(mouseAngle - 90)/90 + Screen.width * 0.5f * (1 - Mathf.Abs(90 - mouseAngle)/90);

		float panDist = MaxPanDist * PanDistCurve.Evaluate((mousePos.magnitude) / (maxMousePos)); //Mathf.Clamp((mousePos.magnitude) / (maxMousePos) * (MaxPanDist), 0, MaxPanDist);
		float panDistX = MaxPanDist * PanDistCurve.Evaluate(Mathf.Abs(mousePos.x) / (maxMousePos));


		Vector3 lookAheadPos = Vector3.zero; 
		GameObject currentWeapon = pc.MyReference.CurrentWeapon;
		Weapon weapon = null;
		if(currentWeapon != null)
		{
			weapon = currentWeapon.GetComponent<Weapon>();
		}

		if(weapon != null)
		{
			if(weapon.IsScoped && pc.UpperBodyState == HumanUpperBodyStates.Aim)
			{
				lookAheadPos = _cameraPos + cameraPanDir * panDist * 2f;
			}
			else
			{
				if(mousePos.y < 0)
				{
					lookAheadPos = _cameraPos + cameraPanDir * panDist;
				}
				else
				{
					lookAheadPos = _cameraPos + cameraPanDir * panDistX;
				}
			}
		}
		else
		{
			lookAheadPos = _cameraPos + cameraPanDir * panDist;
		}



		float rotationLerpSpeed = 3;

		if(_cameraMode == CameraModeEnum.Leader)
		{
			/*
			if(pc.UpperBodyState != HumanUpperBodyStates.Aim && pc.UpperBodyState != HumanUpperBodyStates.HalfAim)
			{
				transform.position = Vector3.Lerp(transform.position, _cameraPos, 8 * Time.unscaledDeltaTime);

				_cameraAngle = 45;

			}
			else */
			if(InputEventHandler.Instance.State == UserInputState.Normal)
			{
				if( _maxFov >= HighFov)
				{
					if(weapon != null)
					{
						//if using sniper, then don't change camera angle
						if(!weapon.IsScoped || pc.UpperBodyState != HumanUpperBodyStates.Aim)
						{
							_cameraAngle1 = 45 - CameraAngleCurve.Evaluate(mouseAngle / 180) * 16 * PanDistCurve.Evaluate(Mathf.Abs(mousePos.y / (Screen.height/2)));//PanDistCurve.Evaluate(panDist / MaxPanDist);
						}
						else
						{
							_cameraAngle1 = 45;
						}

						cameraFov *= 0.85f;
					}
					else
					{
						_cameraAngle1 = 45;
					}

					transform.position = Vector3.Lerp(transform.position, lookAheadPos, 2 * Time.unscaledDeltaTime);
				}
				else
				{
					_cameraAngle1 = 45;
					transform.position = Vector3.Lerp(transform.position, lookAheadPos, 2 * Time.unscaledDeltaTime);
				}
			}


		}
		else
		{
			//_cameraAngle = 60;
			rotationLerpSpeed = 9;

			transform.position = Vector3.Lerp(transform.position, _cameraPos, 4 * Time.unscaledDeltaTime);

		}

		MainCamera.transform.localEulerAngles = Vector3.Lerp(MainCamera.transform.localEulerAngles, new Vector3(_cameraAngle1, 0, 0), rotationLerpSpeed * Time.unscaledDeltaTime);
		MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, cameraFov, rotationLerpSpeed * Time.unscaledDeltaTime);
	

		/*
		if(_cameraMode == CameraModeEnum.Party)
		{
			Transform pc = GameManager.Inst.PlayerControl.SelectedPC.transform;
			transform.position = new Vector3(transform.position.x, 50, transform.position.z);


			if(_isRotatingLeft)
			{
				transform.RotateAround(pc.position, Vector3.up, RotateSpeed * Time.unscaledDeltaTime);
			}

			if(_isRotatingRight)
			{
				transform.RotateAround(pc.position, Vector3.up, -1 * RotateSpeed * Time.unscaledDeltaTime);
			}

		}
		*/
	}


	#region Public Methods
	public void Initialize()
	{
		_cameraMode = CameraModeEnum.Leader;
		_currentRotation = 1;


		InputEventHandler.OnCameraRotateLeft += RotateLeft;
		InputEventHandler.OnCameraRotateRight += RotateRight;


		InputEventHandler.OnCameraPanLeft += StartPanLeft;
		InputEventHandler.OnCameraPanRight += StartPanRight;
		InputEventHandler.OnCameraPanUp += StartPanUp;
		InputEventHandler.OnCameraPanDown += StartPanDown;


		InputEventHandler.OnCameraLookAhead += StartLookAhead;
		InputEventHandler.OnCameraStopLookAhead += StopLookAhead;

		InputEventHandler.OnCameraZoomIn += ZoomIn;
		InputEventHandler.OnCameraZoomOut += ZoomOut;

		Transform pc = GameManager.Inst.PlayerControl.SelectedPC.transform;
		Vector3 cameraFacing = Camera.main.transform.forward;
		
		Vector3 cameraPos = pc.position - cameraFacing * 10;
		Vector3 targetPosition = cameraPos + pc.transform.forward * 10;
		transform.position = targetPosition;

		_maxFov = HighFov;
	}

	public void SetCameraMode(CameraModeEnum mode)
	{


		_cameraMode = mode;

	}
		



	public CameraModeEnum GetCameraMode()
	{
		return _cameraMode;
	}

	public void RotateLeft(float amount)
	{
		_rotation += amount * 15 * 1f;

	}

	public void RotateRight(float amount)
	{
		_rotation += amount * 15 * 1f;
	}

	public void StartRotateLeft()
	{
		_isRotatingLeft = true;
		_isRotatingRight = false;


		_currentRotation --;
		if(_currentRotation < 1)
		{
			_currentRotation = 8;
		}
	}

	public void StartRotateRight()
	{
		_isRotatingRight = true;
		_isRotatingLeft = false;


		_currentRotation ++;
		if(_currentRotation > 8)
		{
			_currentRotation = 1;
		}

	}
	
	public void StopRotating()
	{
		_isRotatingLeft = false;
		_isRotatingRight = false;
	}


	public void StartPanLeft()
	{
		if(_cameraMode == CameraModeEnum.Party)
		{
			float distRatio = 1 - Vector3.Distance(_cameraPos, transform.position) / (MaxPanDist * 2);
			transform.Translate(Vector3.left * Time.unscaledDeltaTime * PanSpeed * distRatio);
		}
	}

	public void StartPanRight()
	{
		if(_cameraMode == CameraModeEnum.Party)
		{
			float distRatio = 1 - Vector3.Distance(_cameraPos, transform.position) / (MaxPanDist * 2);
			transform.Translate(Vector3.right * Time.unscaledDeltaTime * PanSpeed * distRatio);
		}
	}

	public void StartPanUp()
	{
		if(_cameraMode == CameraModeEnum.Party)
		{
			float distRatio = 1 - Vector3.Distance(_cameraPos, transform.position) / (MaxPanDist * 2);
			transform.Translate(Vector3.forward * Time.unscaledDeltaTime * PanSpeed * distRatio);
		}
	}

	public void StartPanDown()
	{
		if(_cameraMode == CameraModeEnum.Party)
		{
			float distRatio = 1 - Vector3.Distance(_cameraPos, transform.position) / (MaxPanDist * 2);
			transform.Translate(Vector3.back * Time.unscaledDeltaTime * PanSpeed * distRatio);
		}
	}


	public void StartLookAhead()
	{
		_isLookingAhead = true;
	}

	public void StopLookAhead()
	{
		_isLookingAhead = false;
	}

	public void ZoomIn(float amount)
	{
		_maxFov -= (HighFov - LowFov)/3f;

		if(_maxFov < LowFov)
		{
			_maxFov = LowFov;
		}
	}

	public void ZoomOut(float amount)
	{
		_maxFov += (HighFov - LowFov)/3f;

		if(_maxFov > HighFov)
		{
			_maxFov = HighFov;
		}
	}


	#endregion

	#region Private Methods



	#endregion
}


public enum CameraModeEnum
{
	Leader,
	Party,
}