using UnityEngine;
using System.Collections;

public class HumanAnimStateSneakForward : HumanAnimStateBase
{

	private float _vSpeed;
	private bool _isWalkingBack;
	private bool _isStrafing;
	private float _aimFreelookAngle;
	private float _noAimFreelookAngle;
	private bool _isFirstUpdateDone;
	
	// This constructor will create new state taking values from old state
	public HumanAnimStateSneakForward(HumanAnimStateBase state)     
		:this(state.ParentCharacter)
	{
		
	}
	
	// this constructor will be used by the other one
	public HumanAnimStateSneakForward(HumanCharacter parentCharacter)
	{
		this.ParentCharacter = parentCharacter;
		
		Initialize();
	}
	
	
	public override void SendCommand(CharacterCommands command)
	{
		switch(command)
		{
		case CharacterCommands.StopCrouch:
			Debug.Log("Stop Crouch during sneak forward");
			UpdateState(HumanBodyStates.StandIdle);
			break;
		case CharacterCommands.Idle:
			UpdateState(HumanBodyStates.CrouchIdle);
			break;
		case CharacterCommands.ThrowGrenade:
			UpdateState(HumanBodyStates.CrouchIdle);
			break;
		case CharacterCommands.Throw:
			UpdateState(HumanBodyStates.CrouchIdle);
			break;
		}
	}
	
	public override void Update()
	{
		float targetVSpeed = 0;
		float velocity = ParentCharacter.MyNavAgent.velocity.magnitude;

		if(velocity > 0 && velocity <= 1.5f)//(this.ParentCharacter.CurrentStance == HumanStances.Crouch)
		{
			targetVSpeed = 1.0f;
		}
		else if(velocity > 1.5f)//(this.ParentCharacter.CurrentStance == HumanStances.CrouchRun)
		{
			targetVSpeed = 1.6f;
		}
		
		_vSpeed = Mathf.Lerp(_vSpeed, targetVSpeed, 6 * Time.deltaTime);
		//Debug.Log("VSpeed " + _vSpeed);
		this.ParentCharacter.MyAnimator.SetFloat("VSpeed", _vSpeed);
		
		HandleNavAgentMovement();
	}
	
	public override bool IsRotatingBody ()
	{
		return false;
	}
	
	
	
	
	
	private void Initialize()
	{
		Debug.Log("initializing sneak forward " + "Dest " + this.ParentCharacter.Destination);
		this.ParentCharacter.CurrentAnimStateName = "Sneak Forward";
		this.ParentCharacter.CurrentStance = HumanStances.Crouch;
		this.ParentCharacter.MyAnimator.SetFloat("VSpeed", 0);
		this.ParentCharacter.MyAnimator.SetBool("IsSneaking", true);
		this.ParentCharacter.MyHeadIK.Weight = 0.5f;
		_vSpeed = 0;
		_isFirstUpdateDone = false;
	}
	
	private void HandleNavAgentMovement()
	{
		NavMeshAgent agent = this.ParentCharacter.GetComponent<NavMeshAgent>();


		//set the speed and acceleration
		if(this.ParentCharacter.CurrentStance == HumanStances.Crouch)
		{
			if(_isWalkingBack)
			{
				agent.speed = 1f;
				agent.acceleration = 20;
			}
			else
			{
				agent.speed = 2f;
				agent.acceleration = 20;
			}
		}
		else if(this.ParentCharacter.CurrentStance == HumanStances.Sprint)//CrouchRun && !_isWalkingBack && !_isStrafing)
		{
			//agent.speed = 2.2f;
			//agent.acceleration = 20;
			UpdateState(HumanBodyStates.WalkForward);
		}

		
		agent.SetDestination(this.ParentCharacter.Destination.Value);

		if(this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.Idle || this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.HalfAim)
		{
			agent.updateRotation = false;
			
			_isStrafing = false;
			
			//check the destination and look angle
			Vector3 lookDir = this.ParentCharacter.LookTarget.position - this.ParentCharacter.transform.position;
			lookDir = new Vector3(lookDir.x, 0, lookDir.z);
			
			Vector3 destDir = this.ParentCharacter.MyNavAgent.velocity.normalized;
			destDir = new Vector3(destDir.x, 0, destDir.z);
			float lookDestAngle = Vector3.Angle(lookDir, destDir);
			float destRightBodyAngle = Vector3.Angle(destDir, this.ParentCharacter.transform.right);
			
			this.ParentCharacter.MyAnimator.SetFloat("LookDestAngle", 0);
			this.ParentCharacter.MyAnimator.SetFloat("DestRightBodyAngle", destRightBodyAngle);
			/*
			if(lookDestAngle > 90)
			{
				_isWalkingBack = true;
				_isStrafing = false;
				
				Vector3 direction = destDir * -1 + lookDir.normalized * 0.05f;
				Quaternion rotation = Quaternion.LookRotation(direction);
				this.ParentCharacter.transform.rotation = Quaternion.Lerp(this.ParentCharacter.transform.rotation, rotation, Time.deltaTime * 5);
			}
			else */
			{
				_isWalkingBack = false;
				_isStrafing = false;
				
				Vector3 direction = destDir + lookDir.normalized * 0.05f;
				Quaternion rotation = Quaternion.LookRotation(direction);
				this.ParentCharacter.transform.rotation = Quaternion.Lerp(this.ParentCharacter.transform.rotation, rotation, Time.deltaTime * 5);
			}
		}
		else if(this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.Aim)
		{
			agent.updateRotation = false;
			//check the destination and look angle
			Vector3 lookDir = this.ParentCharacter.LookTarget.position - this.ParentCharacter.transform.position;
			lookDir = new Vector3(lookDir.x, 0, lookDir.z);
			
			Vector3 destDir = this.ParentCharacter.Destination.Value - this.ParentCharacter.transform.position;
			destDir = new Vector3(destDir.x, 0, destDir.z);
			
			float lookDestAngle = Vector3.Angle(lookDir, destDir);
			float destRightBodyAngle = Vector3.Angle(destDir, this.ParentCharacter.transform.right);
			this.ParentCharacter.MyAnimator.SetFloat("LookDestAngle", lookDestAngle);
			this.ParentCharacter.MyAnimator.SetFloat("DestRightBodyAngle", destRightBodyAngle);
			//Debug.Log("look dest angle " + lookDestAngle);
			//if destination and look dir angle greater than 90 it means we are walking backwards. when
			//walking backwards disable agent update rotation and manually align rotation to opposite of destDir
			//when holding weapon and aiming, then it's 45 degrees so we will go into strafe mode
			WeaponAnimType weaponType = (WeaponAnimType)this.ParentCharacter.MyAnimator.GetInteger("WeaponType");
			
			if(weaponType == WeaponAnimType.Pistol || weaponType == WeaponAnimType.Longgun || weaponType == WeaponAnimType.Grenade)
			{
				if(lookDestAngle > 45 && lookDestAngle <= 135)
				{
					//strafe
					_isStrafing = true;
					_isWalkingBack = false;
					
					Vector3 direction = Vector3.zero;
					//check if body is turning left or right by checking the angle between lookdir and cross(up, destdir)
					Vector3 crossUpDestDir = Vector3.Cross(Vector3.up, destDir);
					float lookCrossDirAngle = Vector3.Angle(lookDir, crossUpDestDir);
					
					if(lookCrossDirAngle > 90)
					{
						direction = crossUpDestDir * -1;
					}
					else
					{
						direction = crossUpDestDir;
					}
					
					Quaternion rotation = Quaternion.LookRotation(direction);
					this.ParentCharacter.transform.rotation = Quaternion.Lerp(this.ParentCharacter.transform.rotation, rotation, Time.deltaTime * 5);
				}
				else if(lookDestAngle > 135)
				{
					//walk back
					_isWalkingBack = true;
					_isStrafing = false;
					
					Vector3 direction = destDir * -1 + lookDir.normalized * 0.05f;
					Quaternion rotation = Quaternion.LookRotation(direction);
					this.ParentCharacter.transform.rotation = Quaternion.Lerp(this.ParentCharacter.transform.rotation, rotation, Time.deltaTime * 5);
				}
				else
				{
					//walk forward
					_isWalkingBack = false;
					_isStrafing = false;
					
					Vector3 direction = destDir + lookDir.normalized * 0.05f;
					Quaternion rotation = Quaternion.LookRotation(direction);
					this.ParentCharacter.transform.rotation = Quaternion.Lerp(this.ParentCharacter.transform.rotation, rotation, Time.deltaTime * 5);
				}
			}
			
			
			
			
		}

		//go to idle state if very close to destination

		if(!this.ParentCharacter.MyNavAgent.pathPending && _isFirstUpdateDone)
		{

			if(this.ParentCharacter.MyAI.BlackBoard.PendingCommand == CharacterCommands.Talk)
			{
				if(this.ParentCharacter.MyNavAgent.remainingDistance <= 0.8f)
				{
					UpdateState(HumanBodyStates.CrouchIdle);
					this.ParentCharacter.MyAI.BlackBoard.PendingCommand = CharacterCommands.Idle;
					this.ParentCharacter.SendCommand(CharacterCommands.Talk);
				}
			}
			else if(this.ParentCharacter.MyAI.BlackBoard.PendingCommand == CharacterCommands.Loot)
			{
				if(this.ParentCharacter.MyNavAgent.remainingDistance <= this.ParentCharacter.MyNavAgent.stoppingDistance)
				{
					UpdateState(HumanBodyStates.CrouchIdle);
					this.ParentCharacter.MyAI.BlackBoard.PendingCommand = CharacterCommands.Idle;
					this.ParentCharacter.SendCommand(CharacterCommands.Loot);
				}
			}
			else if(this.ParentCharacter.MyAI.BlackBoard.PendingCommand == CharacterCommands.Pickup)
			{
				if(this.ParentCharacter.MyNavAgent.remainingDistance <= this.ParentCharacter.MyNavAgent.stoppingDistance)
				{
					UpdateState(HumanBodyStates.CrouchIdle);
					this.ParentCharacter.MyAI.BlackBoard.PendingCommand = CharacterCommands.Idle;
					this.ParentCharacter.SendCommand(CharacterCommands.Pickup);
				}
			}
			else if(this.ParentCharacter.MyNavAgent.remainingDistance <= this.ParentCharacter.MyNavAgent.stoppingDistance)
			{
				UpdateState(HumanBodyStates.CrouchIdle);
			}
		}


		if(!_isFirstUpdateDone)
		{
			_isFirstUpdateDone = true;
		}
	}
	
	private void UpdateState(HumanBodyStates state)
	{
		switch(state)
		{
		case HumanBodyStates.StandIdle:
			this.ParentCharacter.CurrentStance = HumanStances.Run;
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateIdle(this);
			break;
		case HumanBodyStates.CrouchIdle:
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateSneakIdle(this);
			break;
		case HumanBodyStates.WalkForward:
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateGoForward(this);
			break;
		}
	}
}
