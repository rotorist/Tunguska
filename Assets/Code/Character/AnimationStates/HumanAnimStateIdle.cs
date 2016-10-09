using UnityEngine;
using System.Collections;

public class HumanAnimStateIdle : HumanAnimStateBase
{

	private float _vSpeed;
	private bool _isRotatingBody;
	private float _aimFreelookAngle;
	private float _noAimFreelookAngle;


	// This constructor will create new state taking values from old state
	public HumanAnimStateIdle(HumanAnimStateBase state)     
		:this(state.ParentCharacter)
	{
		
	}
	
	// this constructor will be used by the other one
	public HumanAnimStateIdle(HumanCharacter parentCharacter)
	{
		this.ParentCharacter = parentCharacter;
		
		Initialize();
	}

	
	public override void SendCommand (CharacterCommands command)
	{
		switch(command)
		{
		case CharacterCommands.GoToPosition:
			UpdateState(HumanBodyStates.WalkForward);
			break;
		case CharacterCommands.Crouch:
			UpdateState(HumanBodyStates.CrouchIdle);
			break;
		case CharacterCommands.ThrowGrenade:
			//_aimFreelookAngle = 0;
			//_noAimFreelookAngle = 0;
			break;
		}

	}

	public override void Update ()
	{

		if(_vSpeed > 0.3f)
		{
			_vSpeed -= 8 * Time.deltaTime;
		}
		else
		{
			_vSpeed = 0;
		}

		this.ParentCharacter.MyAnimator.SetFloat("VSpeed", _vSpeed);

		//update body rotation



		if(this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.Aim && !this.ParentCharacter.IsBodyLocked)
		{
			Vector3 lookDir = this.ParentCharacter.AimTarget.position - this.ParentCharacter.transform.position;

			lookDir = new Vector3(lookDir.x, 0, lookDir.z);
			float lookBodyAngle = Vector3.Angle(lookDir, this.ParentCharacter.transform.forward);

			if(_isRotatingBody)
			{

				Quaternion rotation = Quaternion.LookRotation(lookDir);
				this.ParentCharacter.transform.rotation = Quaternion.Lerp(this.ParentCharacter.transform.rotation, rotation, Time.deltaTime * 5);
				if(lookBodyAngle < 5)
				{
					_isRotatingBody = false;
				}
			}
			else
			{
				if(lookBodyAngle > _aimFreelookAngle)
				{
					_isRotatingBody = true;

				}
			}

			this.ParentCharacter.MyAnimator.SetBool("IsRotating", _isRotatingBody);
		}
		else if((this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.Idle  || this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.HalfAim) && !this.ParentCharacter.IsBodyLocked)
		{
			Vector3 lookDir = this.ParentCharacter.LookTarget.position - this.ParentCharacter.transform.position;

			if(this.ParentCharacter.IsBodyLocked)
			{
				lookDir = this.ParentCharacter.GetLockedAimTarget() - this.ParentCharacter.transform.position;
			}

			lookDir = new Vector3(lookDir.x, 0, lookDir.z);
			float lookBodyAngle = Vector3.Angle(lookDir, this.ParentCharacter.transform.forward);

			if(_isRotatingBody)
			{
				
				Quaternion rotation = Quaternion.LookRotation(lookDir);
				this.ParentCharacter.transform.rotation = Quaternion.Lerp(this.ParentCharacter.transform.rotation, rotation, Time.deltaTime * 5);
				if(lookBodyAngle < 5)
				{
					_isRotatingBody = false;
				}
			}
			else
			{
				float aimAngle = _noAimFreelookAngle;
				if(ParentCharacter.MyReference.CurrentWeapon != null && !ParentCharacter.MyReference.CurrentWeapon.GetComponent<Weapon>().IsRanged)
				{
					aimAngle = _aimFreelookAngle;
				}

				if(lookBodyAngle > aimAngle)
				{
					_isRotatingBody = true;
				}
			}
		}
	}

	public override bool IsRotatingBody ()
	{
		return _isRotatingBody;
	}



	private void Initialize()
	{
		Debug.Log("Initializing Stand Idle");
		_vSpeed = this.ParentCharacter.MyAnimator.GetFloat("VSpeed");
		this.ParentCharacter.MyAnimator.SetFloat("Blend", UnityEngine.Random.value);
		this.ParentCharacter.MyAnimator.SetBool("IsSneaking", false);
		this.ParentCharacter.Destination = this.ParentCharacter.transform.position;
		this.ParentCharacter.MyNavAgent.Stop();
		this.ParentCharacter.MyNavAgent.ResetPath();
		this.ParentCharacter.MyNavAgent.updateRotation = false;
		this.ParentCharacter.MyHeadIK.Weight = 1;
		this.ParentCharacter.CurrentAnimStateName = "Idle";

		_aimFreelookAngle = 60;
		_noAimFreelookAngle = 60;
	}

	private void UpdateState(HumanBodyStates state)
	{
		switch(state)
		{
		case HumanBodyStates.WalkForward:
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateGoForward(this);
			break;
		case HumanBodyStates.CrouchIdle:
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateSneakIdle(this);
			break;
		}
	}
}
