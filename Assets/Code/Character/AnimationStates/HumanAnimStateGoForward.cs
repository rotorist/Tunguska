using UnityEngine;
using System.Collections;

public class HumanAnimStateGoForward : HumanAnimStateBase
{
	private float _vSpeed;
	private bool _isWalkingBack;
	private bool _isStrafing;
	private float _aimFreelookAngle;
	private float _noAimFreelookAngle;
	private bool _isFirstUpdateDone;

	// This constructor will create new state taking values from old state
	public HumanAnimStateGoForward(HumanAnimStateBase state)     
		:this(state.ParentCharacter)
	{
		
	}
	
	// this constructor will be used by the other one
	public HumanAnimStateGoForward(HumanCharacter parentCharacter)
	{
		this.ParentCharacter = parentCharacter;
		
		Initialize();
	}
	
	
	public override void SendCommand(CharacterCommands command)
	{
		switch(command)
		{
		case CharacterCommands.Crouch:
			UpdateState(HumanBodyStates.CrouchWalk);
			break;
		case CharacterCommands.ThrowGrenade:
			UpdateState(HumanBodyStates.StandIdle);
			break;
		case CharacterCommands.Throw:
			UpdateState(HumanBodyStates.StandIdle);
			break;
		case CharacterCommands.LeftAttack:
			UpdateState(HumanBodyStates.StandIdle);
			break;
		case CharacterCommands.RightAttack:
			UpdateState(HumanBodyStates.StandIdle);
			break;
		case CharacterCommands.Block:
			UpdateState(HumanBodyStates.StandIdle);
			break;
		case CharacterCommands.Idle:
			UpdateState(HumanBodyStates.StandIdle);
			break;
		}
	}
	
	public override void Update()
	{
		float targetVSpeed = 0;
		float velocity = ParentCharacter.MyNavAgent.velocity.magnitude;

		if(/*(velocity > this.ParentCharacter.MyStatus.WalkSpeed && velocity <= this.ParentCharacter.MyStatus.RunSpeed) &&*/ this.ParentCharacter.CurrentStance == HumanStances.Run)
		{
			if(this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.Aim && !this.ParentCharacter.IsHipAiming)
			{
				targetVSpeed = 1f  * this.ParentCharacter.MyStatus.StrafeSpeedModifier;
				ParentCharacter.Stealth.SetNoiseLevel(8, 0.6f);
			}
			else if(this.ParentCharacter.ActionState == HumanActionStates.Melee)
			{
				targetVSpeed = 0.5f;
				ParentCharacter.Stealth.SetNoiseLevel(8, 0.6f);
			}
			else
			{
				targetVSpeed = 1.5f * this.ParentCharacter.MyStatus.RunSpeedModifier;
				ParentCharacter.Stealth.SetNoiseLevel(10, 0.6f);
			}

		}
		else if(/*(velocity > 0 && velocity <= this.ParentCharacter.MyStatus.WalkSpeed) &&*/ this.ParentCharacter.CurrentStance == HumanStances.Walk)
		{
			targetVSpeed = 1;
			ParentCharacter.Stealth.SetNoiseLevel(8, 0.6f);
		}
		else if(/*(velocity > this.ParentCharacter.MyStatus.RunSpeed) &&*/ this.ParentCharacter.CurrentStance == HumanStances.Sprint)
		{
			targetVSpeed = 2f * this.ParentCharacter.MyStatus.SprintSpeedModifier;
			ParentCharacter.Stealth.SetNoiseLevel(15, 0.6f);
		}

			
		_vSpeed = Mathf.Lerp(_vSpeed, targetVSpeed, 6 * Time.deltaTime);
		//Debug.Log("VSpeed " + _vSpeed + " target speed " + targetVSpeed);
		this.ParentCharacter.MyAnimator.SetFloat("VSpeed", _vSpeed);

		HandleNavAgentMovement();


	}
	
	public override bool IsRotatingBody ()
	{
		return false;
	}





	private void Initialize()
	{
		Debug.Log("initializing walk forward " + "Dest " + this.ParentCharacter.Destination);
		this.ParentCharacter.MyAnimator.SetFloat("VSpeed", 0);
		this.ParentCharacter.MyAnimator.SetBool("IsSneaking", false);
		this.ParentCharacter.MyHeadIK.Weight = 0.75f;
		//this.ParentCharacter.MyHeadIK.solver.SmoothDisable();
		_vSpeed = 0;

		_isFirstUpdateDone = false;
	}

	private void HandleNavAgentMovement()
	{
		NavMeshAgent agent = this.ParentCharacter.GetComponent<NavMeshAgent>();
		//set the speed and acceleration
		if(this.ParentCharacter.CurrentStance == HumanStances.Run || this.ParentCharacter.CurrentStance == HumanStances.Walk || _isWalkingBack)
		{
			if(_isWalkingBack && !_isStrafing)
			{
				if(this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.Aim)
				{
					//walking backward while aiming
					agent.speed = this.ParentCharacter.MyStatus.StrafeSpeed * this.ParentCharacter.MyStatus.StrafeSpeedModifier;
				}
				else
				{
					if(this.ParentCharacter.GetCurrentAnimWeapon() == WeaponAnimType.Melee)
					{
						agent.speed = this.ParentCharacter.MyStatus.StrafeSpeed * 1.25f;
					}
					else
					{
						agent.speed = this.ParentCharacter.MyStatus.WalkSpeed;
					}
				}

				agent.acceleration = 20;
			}
			else if(_isStrafing)
			{
				if(this.ParentCharacter.IsHipAiming)
				{
					agent.speed = this.ParentCharacter.MyStatus.RunSpeed * this.ParentCharacter.MyStatus.RunSpeedModifier * 0.9f;
				}
				else
				{
					agent.speed = this.ParentCharacter.MyStatus.StrafeSpeed * this.ParentCharacter.MyStatus.StrafeSpeedModifier;
				}

				agent.acceleration = 20;
			}
			else
			{
				if(this.ParentCharacter.CurrentStance == HumanStances.Run)
				{
					if(this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.Aim)
					{
						if(this.ParentCharacter.IsHipAiming)
						{
							agent.speed = this.ParentCharacter.MyStatus.RunSpeed * this.ParentCharacter.MyStatus.RunSpeedModifier * 0.9f;
						}
						else
						{
							agent.speed = this.ParentCharacter.MyStatus.StrafeSpeed;
						}
					}
					else
					{
						if(this.ParentCharacter.ActionState == HumanActionStates.Melee)
						{
							agent.speed = this.ParentCharacter.MyStatus.WalkSpeed;
						}
						else
						{
							agent.speed = this.ParentCharacter.MyStatus.RunSpeed * this.ParentCharacter.MyStatus.RunSpeedModifier;
						}
					}
					agent.acceleration = 20;
				}
				else
				{
					agent.speed = this.ParentCharacter.MyStatus.WalkSpeed;
					agent.acceleration = 6;
				}
			}


		}
		else if(this.ParentCharacter.CurrentStance == HumanStances.Sprint && !_isWalkingBack)
		{
			agent.speed = this.ParentCharacter.MyStatus.SprintSpeed * this.ParentCharacter.MyStatus.SprintSpeedModifier;
			agent.acceleration = 20;
		}

		if(agent.destination != this.ParentCharacter.Destination.Value)
		{
			agent.SetDestination(this.ParentCharacter.Destination.Value);


			//Debug.Log("Character go forward destination is " + agent.destination + " " + agent.velocity.magnitude);
		}

		if(this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.Idle || this.ParentCharacter.UpperBodyState == HumanUpperBodyStates.HalfAim || this.ParentCharacter.CurrentStance == HumanStances.Sprint)
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

			if(lookDestAngle > 90 && this.ParentCharacter.GetCurrentAnimWeapon() == WeaponAnimType.Melee && this.ParentCharacter.CurrentStance != HumanStances.Sprint)
			{
				this.ParentCharacter.MyAnimator.SetFloat("LookDestAngle", lookDestAngle);
				_isWalkingBack = true;
				_isStrafing = false;
				
				Vector3 direction = destDir * -1 + lookDir.normalized * 0.05f;
				Quaternion rotation = Quaternion.LookRotation(direction);
				this.ParentCharacter.transform.rotation = Quaternion.Lerp(this.ParentCharacter.transform.rotation, rotation, Time.deltaTime * 5);
			}
			else 
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
			Vector3 lookDir = this.ParentCharacter.AimTarget.position - this.ParentCharacter.transform.position;
			lookDir = new Vector3(lookDir.x, 0, lookDir.z);

			Vector3 destDir = this.ParentCharacter.MyNavAgent.velocity.normalized; //this.ParentCharacter.Destination.Value - this.ParentCharacter.transform.position;
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
				if(lookDestAngle > 45 && lookDestAngle <= 135 && this.ParentCharacter.CurrentStance != HumanStances.Sprint)
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

					if(direction == Vector3.zero)
					{
						direction = this.ParentCharacter.transform.forward;
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


		if(this.ParentCharacter.IsBodyLocked || this.ParentCharacter.IsMoveLocked)
		{
			UpdateState(HumanBodyStates.StandIdle);
		}
	

		//go to idle state if very close to destination
		//if(ParentCharacter.name == "HumanCharacter")
			//Debug.LogError("Remaining distance " + this.ParentCharacter.MyNavAgent.remainingDistance + " pending? " + this.ParentCharacter.MyNavAgent.pathPending + " has path? " + this.ParentCharacter.MyNavAgent.path.status);


		if(!this.ParentCharacter.MyNavAgent.pathPending && _isFirstUpdateDone)
		{
			
			if(this.ParentCharacter.MyAI.BlackBoard.PendingCommand == CharacterCommands.Talk)
			{
				if(this.ParentCharacter.MyNavAgent.remainingDistance <= 1.5f)
				{
					UpdateState(HumanBodyStates.StandIdle);
					this.ParentCharacter.MyAI.BlackBoard.PendingCommand = CharacterCommands.Idle;
					this.ParentCharacter.SendCommand(CharacterCommands.Talk);
				}
			}
			else if(this.ParentCharacter.MyAI.BlackBoard.PendingCommand == CharacterCommands.Loot)
			{
				if(this.ParentCharacter.MyNavAgent.remainingDistance <= this.ParentCharacter.MyNavAgent.stoppingDistance)
				{
					UpdateState(HumanBodyStates.StandIdle);
					this.ParentCharacter.MyAI.BlackBoard.PendingCommand = CharacterCommands.Idle;
					this.ParentCharacter.SendCommand(CharacterCommands.Loot);
				}
			}
			else if(this.ParentCharacter.MyAI.BlackBoard.PendingCommand == CharacterCommands.Pickup)
			{
				if(this.ParentCharacter.MyNavAgent.remainingDistance <= this.ParentCharacter.MyNavAgent.stoppingDistance)
				{
					UpdateState(HumanBodyStates.StandIdle);
					this.ParentCharacter.MyAI.BlackBoard.PendingCommand = CharacterCommands.Idle;
					this.ParentCharacter.SendCommand(CharacterCommands.Pickup);
				}
			}
			else if(this.ParentCharacter.MyNavAgent.remainingDistance <= this.ParentCharacter.MyNavAgent.stoppingDistance)
			{
				this.ParentCharacter.MyNavAgent.acceleration = 50;
				UpdateState(HumanBodyStates.StandIdle);

			}
		}


		if(!_isFirstUpdateDone)
		{
			_isFirstUpdateDone = true;
		}

	}

	private void UpdateState(HumanBodyStates state)
	{
		//Debug.Log("leaving go forward state, going to " + state);
		switch(state)
		{
		case HumanBodyStates.StandIdle:
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateIdle(this);
			break;
		case HumanBodyStates.CrouchWalk:
			this.ParentCharacter.CurrentStance = HumanStances.Crouch;
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateSneakForward(this);
			break;
		}
	}
	
	

}
