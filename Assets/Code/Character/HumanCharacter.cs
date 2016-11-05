using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

public class HumanCharacter : Character
{
	public LeftHandIKControl MyLeftHandIK;
	public AimIK MyAimIK;
	public HeadIKControl MyHeadIK;

	public CharacterMarkerSet Markers;



	public HumanAnimStateBase CurrentAnimState;

	public HumanUpperBodyStates UpperBodyState;


	public override bool IsAlive 
	{
		get 
		{
			return this.MyStatus.Health > 0;
		}
	}

	public bool IsHipAiming
	{
		get
		{
			return _isHipAiming;
		}
	}


	private bool _isHipAiming;
	private bool _isThrowing;
	private bool _isSwitchingWeapon;
	private Vector3 _throwTarget;
	private Vector3 _throwDir;
	private ThrownObject _thrownObjectInHand;
	private Character _strangleTarget;
	private Item _weaponToSwitch;
	private bool _isLowThrow;
	private int _meleeStrikeStage;//0, 1, 2
	private bool _isComboAttack;
	private bool _layerDState;//false = decreasing; true = increasing
	private float _blockTimer;


	void Update()
	{
		CurrentAnimState.Update();




		if(MyStatus.Health > 0)
		{
			UpdateLookDirection();

			UpdateDestBodyAngle();

			UpdateFatigue();

			MyAI.AlwaysPerFrameUpdate();

		}

		if(ActionState == HumanActionStates.Block)
		{
			_blockTimer += Time.deltaTime;
		}
	}

	

	
	void LateUpdate() 
	{
		//adjust aim direction for recoil
		if(this.MyReference.CurrentWeapon != null && UpperBodyState == HumanUpperBodyStates.Aim)
		{

			Vector3 originalDir = AimTransform.transform.forward;
			Vector3 dir = new Vector3(originalDir.x, 0, originalDir.z);
			MyAimIK.solver.axis = Vector3.Lerp(MyAimIK.solver.transform.InverseTransformDirection(originalDir), MyAimIK.solver.transform.InverseTransformDirection(dir), Time.deltaTime * 0.1f);

			//recover from recoil
			float recoverRate = 8 * (1 - (this.MyStatus.ArmFatigue / this.MyStatus.MaxArmFatigue));
			AimTarget.localPosition = Vector3.Lerp(AimTarget.localPosition, new Vector3(0, 0, 2), Time.deltaTime * recoverRate);

		}



	}


	public void Initialize()
	{
		//load aim target and look target
		GameObject aimTarget = (GameObject)GameObject.Instantiate(Resources.Load("IKAimTargetRoot"));
		GameObject lookTarget = (GameObject)GameObject.Instantiate(Resources.Load("IKLookTarget"));
		AimTargetRoot = aimTarget.transform;
		AimTarget = AimTargetRoot.Find("IKAimTarget").transform;
		LookTarget = lookTarget.transform;


		LoadCharacterModel(this.CharacterID);




		this.MyEventHandler = GetComponent<CharacterEventHandler>();

		this.Destination = transform.position;
		MyNavAgent = GetComponent<NavMeshAgent>();
		UpperBodyState = HumanUpperBodyStates.Idle;
		ActionState = HumanActionStates.None;



		Markers = new CharacterMarkerSet();



		this.MyStatus = GetComponent<CharacterStatus>();
		this.MyStatus.Initialize();

		this.Stealth = new CharacterStealth(this);

		this.Inventory = new CharacterInventory();


		//each time a human char is initialized it's added to NPC manager's list of human characters to keep track of
		GameManager.Inst.NPCManager.AddHumanCharacter(this);

		CurrentAnimState = new HumanAnimStateIdle(this);
		//SendCommand(CharacterCommands.Unarm);

		MyAI = GetComponent<AI>();
		MyAI.Initialize(this);

		this.ArmorSystem = new ArmorSystem(this);

		_meleeStrikeStage = 0;
		_isLowThrow = false;
	}

	public override void LoadCharacterModel(string prefabName)
	{
		if(this.Model != null)
		{
			GameObject.Destroy(this.Model);
		}

		GameObject o = GameObject.Instantiate(Resources.Load(prefabName)) as GameObject;
		o.transform.parent = this.transform;
		o.transform.localPosition = Vector3.zero;
		o.transform.localEulerAngles = Vector3.zero;

		this.MyAnimator = o.transform.GetComponent<Animator>();
		this.MyReference = o.transform.GetComponent<CharacterReference>();
		this.MyAnimEventHandler = o.transform.GetComponent<AnimationEventHandler>();

		this.MyReference.ParentCharacter = this;
		this.MyReference.LiveCollider = transform.GetComponent<CapsuleCollider>();
		if(this.MyReference.DeathCollider != null)
		{
			this.MyReference.DeathCollider.GetComponent<DeathCollider>().ParentCharacter = this;
			this.MyReference.DeathCollider.enabled = false;
		}

		if(this.MyReference.RightFoot != null)
		{
			this.MyReference.RightFoot.Attacker = this;
		}

		this.MyAimIK = o.transform.GetComponent<AimIK>();
		this.MyAimIK.solver.target = AimTarget;
		this.MyAimIK.solver.IKPositionWeight = 0;
		this.MyAimIK.solver.SmoothDisable();

		this.MyLeftHandIK = o.transform.GetComponent<LeftHandIKControl>();
		this.MyLeftHandIK.Initialize();

		this.MyHeadIK = o.transform.GetComponent<HeadIKControl>();
		this.MyHeadIK.Initialize();
		this.MyHeadIK.LookTarget = LookTarget;


		//setup animator parameters initial values
		this.MyAnimator.SetBool("IsAiming", false);
		this.MyAnimator.SetBool("IsSneaking", false);

		this.Model = o;
		this.Model.name = prefabName;

		if(this.MyAI != null)
		{
			this.MyAI.BlackBoard.EquippedWeapon = null;
		}


		//subscribe events
		this.MyAnimEventHandler.OnLongGunPullOutFinish -= OnLongGunPullOutFinish;
		this.MyAnimEventHandler.OnLongGunPullOutFinish += OnLongGunPullOutFinish;

		this.MyAnimEventHandler.OnLongGunPutAwayFinish -= OnLongGunPutAwayFinish;
		this.MyAnimEventHandler.OnLongGunPutAwayFinish += OnLongGunPutAwayFinish;

		this.MyAnimEventHandler.OnPistolPullOutFinish -= OnPistolPullOutFinish;
		this.MyAnimEventHandler.OnPistolPullOutFinish += OnPistolPullOutFinish;

		this.MyAnimEventHandler.OnPistolPutAwayFinish -= OnPistolPutAwayFinish;
		this.MyAnimEventHandler.OnPistolPutAwayFinish += OnPistolPutAwayFinish;

		this.MyAnimEventHandler.OnGrenadePullOutFinish -= OnGrenadePullOutFinish;
		this.MyAnimEventHandler.OnGrenadePullOutFinish += OnGrenadePullOutFinish;

		this.MyAnimEventHandler.OnMeleePullOutFinish -= OnMeleePullOutFinish;
		this.MyAnimEventHandler.OnMeleePullOutFinish += OnMeleePullOutFinish;
		this.MyAnimEventHandler.OnMeleePutAwayFinish -= OnMeleePutAwayFinish;
		this.MyAnimEventHandler.OnMeleePutAwayFinish += OnMeleePutAwayFinish;

		this.MyAnimEventHandler.OnReloadFinish -= OnReloadFinish;
		this.MyAnimEventHandler.OnReloadFinish += OnReloadFinish;

		this.MyAnimEventHandler.OnThrowFinish -= OnThrowFinish;
		this.MyAnimEventHandler.OnThrowFinish += OnThrowFinish;

		this.MyAnimEventHandler.OnThrowLeaveHand -= OnThrowLeaveHand;
		this.MyAnimEventHandler.OnThrowLeaveHand += OnThrowLeaveHand;

		this.MyAnimEventHandler.OnStartStrangle -= OnStartStrangle;
		this.MyAnimEventHandler.OnStartStrangle += OnStartStrangle;

		this.MyAnimEventHandler.OnEndStrangle -= OnEndStrangle;
		this.MyAnimEventHandler.OnEndStrangle += OnEndStrangle;

		this.MyAnimEventHandler.OnDeath -= OnDeath;
		this.MyAnimEventHandler.OnDeath += OnDeath;

		this.MyAnimEventHandler.OnStrangledDeath -= OnStrangledDeath;
		this.MyAnimEventHandler.OnStrangledDeath += OnStrangledDeath;

		this.MyAnimEventHandler.OnHitReover -= OnInjuryRecover;
		this.MyAnimEventHandler.OnHitReover += OnInjuryRecover;

		this.MyAnimEventHandler.OnSwitchWeapon -= OnSwitchWeapon;
		this.MyAnimEventHandler.OnSwitchWeapon += OnSwitchWeapon;

		this.MyAnimEventHandler.OnFinishTakeObject -= OnTakeObjectFinish;
		this.MyAnimEventHandler.OnFinishTakeObject += OnTakeObjectFinish;

		this.MyAnimEventHandler.OnMeleeStrikeHalfWay -= OnMeleeStrikeHalfWay;
		this.MyAnimEventHandler.OnMeleeStrikeHalfWay += OnMeleeStrikeHalfWay;

		this.MyAnimEventHandler.OnMeleeComboStageTwo -= OnMeleeComboStageTwo;
		this.MyAnimEventHandler.OnMeleeComboStageTwo += OnMeleeComboStageTwo;

		this.MyAnimEventHandler.OnMeleeStrikeLeftFinish -= OnMeleeStrikeLeftFinish;
		this.MyAnimEventHandler.OnMeleeStrikeLeftFinish += OnMeleeStrikeLeftFinish;

		this.MyAnimEventHandler.OnMeleeStrikeRightFinish -= OnMeleeStrikeRightFinish;
		this.MyAnimEventHandler.OnMeleeStrikeRightFinish += OnMeleeStrikeRightFinish;

		this.MyAnimEventHandler.OnMeleeBlockFinish -= OnMeleeBlockFinish;
		this.MyAnimEventHandler.OnMeleeBlockFinish += OnMeleeBlockFinish;

		this.MyAnimEventHandler.OnAnimationActionEnd -= OnAnimationActionEnd;
		this.MyAnimEventHandler.OnAnimationActionEnd += OnAnimationActionEnd;
	}
		

	public override void SendCommand(CharacterCommands command)
	{

		if(command == CharacterCommands.PlayAnimationAction)
		{
			CurrentAnimState = new HumanAnimStateAction(this);
			IsBodyLocked = true;
		}

		if(command == CharacterCommands.AnimationActionDone)
		{
			Debug.Log("Action Done");
			IsBodyLocked = false;
			IsMoveLocked = false;

		}




		if(!IsBodyLocked && !IsMoveLocked)
		{
			CurrentAnimState.SendCommand(command);
		}


		//following commands are not given by AI or user. All commands that will unlock the body go here
		if(command == CharacterCommands.StopAim)
		{
			Debug.Log("Stop AIM " + this.name);
			if(ActionState == HumanActionStates.None)
			{
				MyAI.WeaponSystem.StopFiringRangedWeapon();
				UpperBodyState = HumanUpperBodyStates.Idle;
				MyAimIK.solver.SmoothDisable(6);
				MyHeadIK.SmoothEnable();
				MyAnimator.SetBool("IsAiming", false);
				MyReference.Flashlight.transform.localEulerAngles = new Vector3(27, 0, 0);

				if(GetCurrentAnimWeapon() == WeaponAnimType.Pistol || GetCurrentAnimWeapon() == WeaponAnimType.Grenade)
				{
					MyLeftHandIK.InstantDisable();
				}
			}
			else
			{
				UpperBodyState = HumanUpperBodyStates.Idle;
			}

			_isHipAiming = false;
			//Debug.LogError("stopping aim " + ActionState + " " + this.name);
		}

		if(command == CharacterCommands.Cancel)
		{
			if(ActionState == HumanActionStates.Strangle)
			{
				ActionState = HumanActionStates.None;
				MyAnimator.SetTrigger("Cancel");

				Vector3 lineOfSight = _strangleTarget.transform.position - transform.position;
				transform.position = _strangleTarget.transform.position - lineOfSight.normalized;

				if(_strangleTarget.IsAlive)
				{
					_strangleTarget.IsBodyLocked = false;
					_strangleTarget.MyAnimator.SetTrigger("Cancel");
					_strangleTarget = null;
				}

				IsBodyLocked = false;
				MyNavAgent.enabled = true;

			}
		}

		if(command == CharacterCommands.RightAttack)
		{
			ActionState = HumanActionStates.Melee;
			_meleeStrikeStage = 0;

			MyAnimator.SetTrigger("RightAttack");
			IsMoveLocked = true;
			MyHeadIK.SmoothDisable(9);

			Debug.Log("starting right attack");
		}

		if(command == CharacterCommands.LeftAttack)
		{
			ActionState = HumanActionStates.Melee;
			_meleeStrikeStage = 0;

			Vector3 lookDir = LookTarget.position - transform.position;
			lookDir = new Vector3(lookDir.x, 0, lookDir.z);

			Vector3 destDir = MyNavAgent.velocity.normalized; 
			destDir = new Vector3(destDir.x, 0, destDir.z);
			float lookDestAngle = Vector3.Angle(lookDir, destDir);

			if(MyNavAgent.velocity.magnitude > 0f && lookDestAngle < 70)
			{
				//MyAnimator.SetTrigger("ComboAttack");
				MyAI.BlackBoard.AnimationAction = AnimationActions.ComboAttack;
				MyAI.BlackBoard.ActionMovementDest = transform.position + (this.LookTarget.position - transform.position).normalized * 2f;
				MyAI.BlackBoard.ActionMovementSpeed = 1.5f;

				SendCommand(CharacterCommands.PlayAnimationAction);
				_isComboAttack = true;
			}
			else
			{
				MyAnimator.SetTrigger("LeftAttack");
				IsMoveLocked = true;
			}
			MyHeadIK.SmoothDisable(9);

			Debug.Log("starting left attack ");
		}

		if(command == CharacterCommands.Block)
		{
			ActionState = HumanActionStates.Block;
			this.MyLeftHandIK.Target = MyReference.CurrentWeapon.GetComponent<Weapon>().ForeGrip;
			this.MyLeftHandIK.SmoothEnable(20);
			_meleeStrikeStage = 0;
			MyAnimator.SetTrigger("Block");
			IsMoveLocked = true;
			_blockTimer = 0;
		}

		if(command == CharacterCommands.Kick)
		{
			if(ActionState == HumanActionStates.None)
			{
				//kick direction
				Vector3 lookDir = LookTarget.position - transform.position;
				lookDir = new Vector3(lookDir.x, 0, lookDir.z);
				transform.rotation = Quaternion.LookRotation(lookDir);

				if(MyReference.RightFoot != null)
				{
					MyReference.RightFoot.SetActive(true);
				}
				ActionState = HumanActionStates.Melee;
				MyAnimator.SetTrigger("Kick");
				IsBodyLocked = true;
			}
		}





		if(IsBodyLocked)
		{
			return;
		}

		//following commands are given by AI or user, and can be locked


		if(command == CharacterCommands.Crouch)
		{
			CapsuleCollider collider = GetComponent<CapsuleCollider>();
			collider.height = 1.5f;
			collider.center = new Vector3(0, 0.5f, 0);

		}

		if(command == CharacterCommands.StopCrouch)
		{
			CapsuleCollider collider = GetComponent<CapsuleCollider>();
			collider.height = 1.7f;
			collider.center = new Vector3(0, 1, 0);
		}

	
		if((command == CharacterCommands.Aim || command == CharacterCommands.HipAim) && CurrentStance != HumanStances.Sprint)
		{
			if(MyAI.ControlType != AIControlType.Player)
				Debug.Log("action state " + ActionState + " weapon type " + GetCurrentAnimWeapon() + " upper body state " + UpperBodyState);

			if((ActionState == HumanActionStates.None || ActionState == HumanActionStates.Twitch) && GetCurrentAnimWeapon() != WeaponAnimType.Unarmed && !MyAnimator.GetBool("IsAiming"))
			{
				if(MyAI.ControlType != AIControlType.Player)
					Debug.Log(command);

				if(GetCurrentAnimWeapon() == WeaponAnimType.Grenade || GetCurrentAnimWeapon() == WeaponAnimType.Tool)
				{
					MyLeftHandIK.SmoothDisable(6);
					UpperBodyState = HumanUpperBodyStates.HalfAim;
				}
				else
				{
					
					UpperBodyState = HumanUpperBodyStates.Aim;
					if(GetCurrentAnimWeapon() == WeaponAnimType.Pistol)
					{
						MyLeftHandIK.InstantDisable();
					}
					MyLeftHandIK.SmoothEnable(6);
					MyAimIK.solver.InstantDisable();
					MyAimIK.solver.CurvedEnable(6f);
					//if(GetCurrentAnimWeapon() != WeaponAnimType.Melee)
					{
						MyHeadIK.InstantDisable();
					}
					MyAnimator.SetBool("IsAiming", true);
					if(MyAI.ControlType != AIControlType.Player)
						Debug.LogError("Animation parameter IsAiming has been set");
					MyReference.Flashlight.transform.localEulerAngles = new Vector3(0, 0, 0);
				}
					
				/*
				//draw a new grenade if there isn't one
				if(GetCurrentAnimWeapon() == WeaponAnimType.Grenade)
				{
					if(_thrownObjectInHand == null)
					{
						DrawNextGrenade();
					}
				}
				*/

			}
			else if(ActionState == HumanActionStates.SwitchWeapon)
			{
				
				if(UpperBodyState == HumanUpperBodyStates.Aim && !MyAnimator.GetBool("IsAiming"))
				{
					
					MyLeftHandIK.InstantDisable();
					MyLeftHandIK.SmoothEnable(6);
					MyAimIK.solver.InstantDisable();
					MyAimIK.solver.SmoothEnable(6f);
					MyHeadIK.InstantDisable();
					MyAnimator.SetBool("IsAiming", true);
					//if(MyAI.ControlType != AIControlType.Player)
					//	Debug.LogError("Animation parameter IsAiming has been set");
					MyReference.Flashlight.transform.localEulerAngles = new Vector3(0, 0, 0);
				}
				else
				{
					UpperBodyState = HumanUpperBodyStates.Aim;
				}

			}
			else if(GetCurrentAnimWeapon() == WeaponAnimType.Unarmed)
			{
				SendCommand(MyAI.WeaponSystem.GetBestWeaponChoice());

			}


			if(command == CharacterCommands.HipAim)
			{
				_isHipAiming = true;
			}
			else if(command == CharacterCommands.Aim)
			{
				_isHipAiming = false;
			}
		}



		if(command == CharacterCommands.Sprint)
		{

			/*
			if(CurrentStance == HumanStances.Crouch || CurrentStance == HumanStances.CrouchRun)
			{
				CurrentStance = HumanStances.CrouchRun;
			}
			else*/
			{
				CurrentStance = HumanStances.Sprint;
				MyAimIK.solver.SmoothDisable();
				MyHeadIK.InstantDisable();
			}
		}

		if(command == CharacterCommands.StopSprint)
		{
			if(UpperBodyState == HumanUpperBodyStates.Aim)
			{
				MyAimIK.solver.SmoothEnable();
			}

			if(CurrentStance == HumanStances.CrouchRun || CurrentStance == HumanStances.Crouch)
			{
				CurrentStance = HumanStances.Crouch;
			}
			else
			{
				CurrentStance = HumanStances.Run;
			}
			MyHeadIK.SmoothEnable();
		}

		if(command == CharacterCommands.SwitchWeapon2)
		{
			if((ActionState == HumanActionStates.None || ActionState == HumanActionStates.Twitch) && MyAI.WeaponSystem.PrimaryWeapon != null)
			{
				CsDebug.Inst.CharLog(this, "Start switching weapon2");
				MyLeftHandIK.SmoothDisable(15);
				MyAimIK.solver.SmoothDisable(9);

				//SwitchWeapon(Inventory.RifleSlot);
				_weaponToSwitch = Inventory.RifleSlot;
				if(MyAI.WeaponSystem.PrimaryWeapon.IsRanged)
				{
					MyAnimator.SetInteger("WeaponType", 2);
				}
				else
				{
					MyAnimator.SetInteger("WeaponType", 3);
				}

				ActionState = HumanActionStates.SwitchWeapon;
			}
		}

		if(command == CharacterCommands.SwitchWeapon1)
		{
			if((ActionState == HumanActionStates.None || ActionState == HumanActionStates.Twitch) && MyAI.WeaponSystem.SideArm != null)
			{
				if(UpperBodyState == HumanUpperBodyStates.Aim)
				{
					//MyLeftHandIK.SmoothEnable();
				}
				else
				{
					
				}
				MyLeftHandIK.SmoothDisable(15);
				MyAimIK.solver.SmoothDisable(9);
				MyAnimator.SetInteger("WeaponType", 1);
				//SwitchWeapon(Inventory.SideArmSlot);
				_weaponToSwitch = Inventory.SideArmSlot;

				ActionState = HumanActionStates.SwitchWeapon;
			}
		}

		if(command == CharacterCommands.SwitchThrown)
		{
			if(ActionState == HumanActionStates.None)
			{

				MyLeftHandIK.SmoothDisable(6);
				MyAimIK.solver.SmoothDisable(9);
				MyAnimator.SetInteger("WeaponType", -1);
				//SwitchWeapon(Inventory.ThrowSlot);
				_weaponToSwitch = Inventory.ThrowSlot;

				ActionState = HumanActionStates.SwitchWeapon;
			}
		}

		if(command == CharacterCommands.SwitchTool)
		{
			if(ActionState == HumanActionStates.None)
			{
				GameObject.Destroy(this.MyReference.CurrentWeapon);
				if(_thrownObjectInHand != null)
				{
					GameObject.Destroy(_thrownObjectInHand.gameObject);
				}

				MyLeftHandIK.SmoothDisable(6);
				MyAimIK.solver.SmoothDisable(9);
				MyAnimator.SetInteger("WeaponType", -2);
				//SwitchWeapon("ThrowingRock");

				ActionState = HumanActionStates.SwitchWeapon;
			}
		}

		if(command == CharacterCommands.Unarm)
		{
			if(ActionState == HumanActionStates.None)
			{
				Debug.Log("Unarm " + this.name);
				MyLeftHandIK.SmoothDisable();
				UpperBodyState = HumanUpperBodyStates.Idle;
				MyAimIK.solver.SmoothDisable();
				MyHeadIK.SmoothEnable();
				MyAnimator.SetBool("IsAiming", false);
				MyAnimator.SetInteger("WeaponType", 0);
				//SwitchWeapon(null);
				_weaponToSwitch = null;

				ActionState = HumanActionStates.SwitchWeapon;
			}
		}

		if(command == CharacterCommands.PullTrigger)
		{
			if(ActionState != HumanActionStates.None || UpperBodyState != HumanUpperBodyStates.Aim)
			{
				return;
			}

			if(GetCurrentAnimWeapon() == WeaponAnimType.Longgun || GetCurrentAnimWeapon() == WeaponAnimType.Pistol)
			{
				//
				this.MyReference.CurrentWeapon.GetComponent<Gun>().TriggerPull();


			}
		}

		if(command == CharacterCommands.ReleaseTrigger)
		{
			if(ActionState != HumanActionStates.None || UpperBodyState != HumanUpperBodyStates.Aim)
			{
				return;
			}

			if(GetCurrentAnimWeapon() == WeaponAnimType.Longgun || GetCurrentAnimWeapon() == WeaponAnimType.Pistol)
			{
				//
				this.MyReference.CurrentWeapon.GetComponent<Gun>().TriggerRelease();


			}
		}


		if(command == CharacterCommands.Reload)
		{
			if(ActionState == HumanActionStates.None && this.MyReference.CurrentWeapon != null)
			{
				GunMagazine magazine = this.MyReference.CurrentWeapon.GetComponent<GunMagazine>();
				if(magazine != null && magazine.AmmoLeft < magazine.MaxCapacity)
				{
					GridItemData ammo = this.Inventory.FindItemInBackpack(magazine.LoadedAmmoID);
					if(ammo != null)
					{
						if(GetCurrentAnimWeapon() == WeaponAnimType.Longgun || GetCurrentAnimWeapon() == WeaponAnimType.Pistol)
						{
							MyAimIK.solver.SmoothDisable(12);
							MyAnimator.SetInteger("ReloadType", (int)magazine.ReloadType);
							MyAnimator.SetTrigger("Reload");
							
							MyLeftHandIK.SmoothDisable();

						}

						MyHeadIK.SmoothDisable();
							
						ActionState = HumanActionStates.Reload;
					}
				}

			}
		}



		if(command == CharacterCommands.CancelReload)
		{
			if(ActionState == HumanActionStates.Reload && this.MyReference.CurrentWeapon != null)
			{
				
				Debug.Log("cancel reload");
				ActionState = HumanActionStates.None;

				if(UpperBodyState == HumanUpperBodyStates.Aim)
				{
					MyAimIK.solver.SmoothEnable();
					MyAnimator.SetTrigger("CancelReload");
				}
				else
				{
					MyAnimator.SetTrigger("CancelReload");
				}

				if(MyAnimator.GetInteger("WeaponType") == (int)WeaponAnimType.Longgun)
				{
					MyLeftHandIK.SmoothEnable();
				}
				else
				{
					Debug.Log("done reloading pistol " + UpperBodyState);
					if(UpperBodyState == HumanUpperBodyStates.Aim)
					{
						MyLeftHandIK.SmoothEnable();
					}
					else
					{
						//MyLeftHandIK.SmoothDisable();
						SendCommand(CharacterCommands.StopAim);
					}
				}

				MyHeadIK.SmoothEnable();

			}
		}



		if(command == CharacterCommands.Throw || command == CharacterCommands.LowThrow)
		{
			if(ActionState != HumanActionStates.None)
			{
				return;
			}

			if(_thrownObjectInHand == null)
			{
				DrawNextGrenade();
			}


			MyAimIK.solver.SmoothDisable(15);
			if(command == CharacterCommands.LowThrow)
			{
				MyAnimator.SetTrigger("LowThrow");
				_isLowThrow = true;
			}
			else
			{
				MyAnimator.SetTrigger("Throw");
				_isLowThrow = false;
			}

			_throwTarget = this.AimPoint;
			Quaternion rotation = Quaternion.LookRotation(this.AimPoint - transform.position);
			transform.rotation = rotation;
			IsBodyLocked = true;
			Debug.Log("Throw triggered");
		}

		if(command == CharacterCommands.ThrowGrenade)
		{
			if(ActionState != HumanActionStates.None)
			{
				return;
			}

			if(UpperBodyState != HumanUpperBodyStates.Aim)
			{
				//MyAimIK.solver.transform = this.MyReference.TorsoWeaponMount.transform;
			}

			SendCommand(CharacterCommands.CancelReload);

			if(this.MyReference.CurrentWeapon != null && MyAnimator.GetInteger("WeaponType") == (int)WeaponAnimType.Longgun)
			{
				MyLeftHandIK.SmoothEnable();
			}

			//MyHeadIK.SmoothDisable(1);

			//move weapon to torso mount so that right hand is free
			if(this.MyReference.CurrentWeapon != null)
			{
				this.MyReference.CurrentWeapon.transform.parent = this.MyReference.TorsoWeaponMount.transform;
			}
			MyAnimator.SetTrigger("ThrowGrenade");

			_throwTarget = this.AimPoint;

			_throwDir = this.AimPoint - transform.position;
			IsBodyLocked = true;
			Quaternion rotation = Quaternion.LookRotation(new Vector3(_throwDir.x, 0, _throwDir.z));
			transform.rotation = rotation;

			ActionState = HumanActionStates.Throw;

			_thrownObjectInHand = ((GameObject)GameObject.Instantiate(Resources.Load("PipeGrenade"))).GetComponent<ThrownObject>();
			Explosive explosive = _thrownObjectInHand.GetComponent<Explosive>();
			if(explosive != null)
			{
				explosive.Attacker = this;
			}

			_thrownObjectInHand.GetComponent<Rigidbody>().isKinematic = true;

			_thrownObjectInHand.transform.parent = this.MyReference.RightHandWeaponMount.transform;
			_thrownObjectInHand.transform.localPosition = _thrownObjectInHand.InHandPosition;
			_thrownObjectInHand.transform.localEulerAngles = _thrownObjectInHand.InHandRotation;
		}

		if(command == CharacterCommands.UseTool)
		{
			if(ActionState != HumanActionStates.None || MyAI.BlackBoard.TargetEnemy == null)
			{
				return;
			}

			//check if the target enemy is close enough and angle between character and enemy is less than 45
			if(Vector3.Distance(MyAI.BlackBoard.TargetEnemy.transform.position, transform.position) > 2 || 
				Vector3.Angle(MyAI.BlackBoard.TargetEnemy.transform.forward, transform.forward) > 45)
			{
				return;
			}

			Vector3 lineOfSight = MyAI.BlackBoard.TargetEnemy.transform.position - transform.position;
			//check if angle between character facing and target line of sight is less than 45
			if(Vector3.Angle(lineOfSight, transform.forward) > 45)
			{
				return;
			}

			//stop movement
			SendCommand(CharacterCommands.Idle);
			IsBodyLocked = true;
			MyNavAgent.enabled = false;

			//place player right behind target
			transform.position = MyAI.BlackBoard.TargetEnemy.transform.position - lineOfSight.normalized * 0.25f;

			//align player facing direction to enemy's
			lineOfSight = new Vector3(lineOfSight.x, 0, lineOfSight.z);
			Quaternion rotation = Quaternion.LookRotation(lineOfSight);
			transform.rotation = rotation;

			MyAnimator.SetTrigger("Strangle");
			_strangleTarget = MyAI.BlackBoard.TargetEnemy;

			SendCommand(CharacterCommands.StopAim);
			ActionState = HumanActionStates.Strangle;
		}

		if(command == CharacterCommands.Pickup)
		{
			if(ActionState != HumanActionStates.None || MyAI.BlackBoard.PickupTarget == null)
			{
				return;
			}

			//move weapon to torso mount so that right hand is free
			if(this.MyReference.CurrentWeapon != null && MyAnimator.GetInteger("WeaponType") == (int)WeaponAnimType.Longgun)
			{
				this.MyReference.CurrentWeapon.transform.parent = this.MyReference.TorsoWeaponMount.transform;
			}
			MyAnimator.SetTrigger("TakeObject");


		}

		if(command == CharacterCommands.Loot)
		{
			if(ActionState != HumanActionStates.None)
			{
				return;
			}
			GameObject useTarget = MyAI.BlackBoard.UseTarget;
			Character target = MyAI.BlackBoard.InteractTarget;

			if(target != null && target.MyStatus.Health <= 0)
			{
				if(this.MyAI.ControlType == AIControlType.Player)
				{
					UIEventHandler.Instance.TriggerLootBody();

				}
			}
			else if(useTarget != null)
			{
				//open chest
				Chest chest = useTarget.GetComponent<Chest>();
				if(chest != null)
				{
					
					//open UI 
					UIEventHandler.Instance.TriggerLootChest();
				}
			}

		}

		if(command == CharacterCommands.Talk)
		{
			if(ActionState != HumanActionStates.None)
			{
				return;
			}
			Debug.Log("opening dialog");
			Character target = MyAI.BlackBoard.InteractTarget;

			if(target != null && target.MyStatus.Health > 0)
			{
				if(this.MyAI.ControlType == AIControlType.Player)
				{
					UIEventHandler.Instance.TriggerDialogue();

				}
			}

		}

		if(command == CharacterCommands.SetAlert)
		{
			float ambient = RenderSettings.ambientIntensity;

			if(MyAI.BlackBoard.GuardLevel == 1)
			{
				//weapon holstered; only turn on flash light (if has one) when completely dark
				if(MyReference.CurrentWeapon != null)
				{
					SendCommand(CharacterCommands.Unarm);
				}
				if(MyReference.Flashlight != null)
				{
					if(ambient <= 0.3f)
					{
						MyReference.Flashlight.Toggle(true);
					}
					else
					{
						MyReference.Flashlight.Toggle(false);
					}
				}
			}
			else if(MyAI.BlackBoard.GuardLevel >= 2)
			{
				if(MyReference.CurrentWeapon == null)
				{
					SendCommand(MyAI.WeaponSystem.GetBestWeaponChoice());
				}

				if(MyReference.Flashlight != null)
				{
					if(ambient <= 0.5f)
					{
						MyReference.Flashlight.Toggle(true);
					}
					else
					{
						MyReference.Flashlight.Toggle(false);
					}
				}
			}

		}



	}



	public override bool SendDamage(float damage, float penetration, Vector3 hitNormal, Character attacker, Weapon attackerWeapon)
	{
		
		
		//if(MyAI.ControlType != AIControlType.Player)
		{

			OnInjury(hitNormal, false);
			MyAI.Sensor.OnTakingDamage(attacker);

			float finalDamage = damage;
			if(this.Inventory.ArmorSlot != null)
			{
				float armorRating = (float)this.Inventory.ArmorSlot.GetAttributeByName("Armor").Value;
				float coverage = (float)this.Inventory.ArmorSlot.GetAttributeByName("Coverage").Value;
				float chance = UnityEngine.Random.value;
				if(chance < coverage)
				{
					//covered, calculate armor rating vs penetration
					if(penetration >= armorRating)
					{
						//penetrated the armor
						finalDamage = damage * Mathf.Clamp01((penetration - armorRating) / armorRating);
					}
					else
					{
						//not penetrated
						return true;
					}
				}
				else
				{
					//uncovered area are less vulnerable. this does NOT apply to critical (head) hits.
					finalDamage *= 0.5f;
				}

			}

			MyStatus.Health -= finalDamage;

			if(MyAI.ControlType == AIControlType.Player)
			{
				GameManager.Inst.CameraShaker.TriggerScreenShake(0.1f, 0.06f);
			}
			//GameManager.Inst.UIManager.BarkPanel.AddFloater(this, Mathf.FloorToInt(damage).ToString(), true);
		}


		if(MyStatus.Health <= 0)
		{
			MyStatus.Health = 0;
			OnDeath();
		}

		return false;
	}

	public override bool SendMeleeDamage (float sharpDamage, float bluntDamage, Vector3 hitNormal, Character attacker, float knockBackChance)
	{
		float attackerAngle = Vector3.Angle(transform.forward, transform.position - attacker.transform.position);
		if(ActionState == HumanActionStates.Block && attackerAngle > 135)
		{
			this.MyLeftHandIK.InstantDisable();
			MyAnimator.SetTrigger("BlockSuccess");
			ActionState = HumanActionStates.Twitch;

			if(MyAI.ControlType == AIControlType.Player)
			{
				GameManager.Inst.CameraShaker.TriggerScreenShake(0.07f, 0.09f);
			}

			Debug.Log("block timer " + _blockTimer);
			if(_blockTimer < 0.25f)
			{
				//face attacker
				Vector3 lookDir = attacker.transform.position - transform.position;
				lookDir = new Vector3(lookDir.x, 0, lookDir.z);
				transform.rotation = Quaternion.LookRotation(lookDir);

				ActionState = HumanActionStates.Melee;
				if(MyReference.RightFoot != null)
				{
					MyReference.RightFoot.SetActive(true);
				}
				MyAnimator.SetTrigger("Kick");
				IsBodyLocked = true;
			}

			return true;
		}
		else
		{
			OnInjury(hitNormal, UnityEngine.Random.value <= knockBackChance);
			MyAI.Sensor.OnTakingDamage(attacker);
			float finalDamage = 0;
			if(this.Inventory.ArmorSlot != null)
			{
				float padding = (float)this.Inventory.ArmorSlot.GetAttributeByName("Padding").Value;
				float armorRating = (float)this.Inventory.ArmorSlot.GetAttributeByName("Armor").Value;
				float coverage = (float)this.Inventory.ArmorSlot.GetAttributeByName("Coverage").Value;
				float chance = UnityEngine.Random.value;
				if(chance < coverage)
				{
					//when hitting armor, only blunt damage applies
					finalDamage = bluntDamage - padding + sharpDamage - armorRating;
					if(finalDamage < 0)
					{
						finalDamage = 0;
					}
				}
				else
				{
					finalDamage = sharpDamage + bluntDamage;
				}

			}
			else
			{
				finalDamage = sharpDamage + bluntDamage;
			}

			MyStatus.Health -= finalDamage;

			if(MyAI.ControlType == AIControlType.Player)
			{
				GameManager.Inst.CameraShaker.TriggerScreenShake(0.1f, 0.1f);
			}
		}

		if(MyStatus.Health <= 0)
		{
			MyStatus.Health = 0;
			OnDeath();
		}

		return false;
	}

	public override bool SendCriticalDamage (float damage, float penetration, Vector3 hitNormal, Character attacker, Weapon attackerWeapon)
	{
		//if(MyAI.ControlType != AIControlType.Player)
		{
			OnInjury(hitNormal, true);
			MyAI.Sensor.OnTakingDamage(attacker);

			float finalDamage = damage * 1.5f;
			if(this.Inventory.HeadSlot != null)
			{
				float armorRating = (float)this.Inventory.HeadSlot.GetAttributeByName("Armor").Value;
				float coverage = (float)this.Inventory.HeadSlot.GetAttributeByName("Coverage").Value;
				float chance = UnityEngine.Random.value;
				if(chance < coverage)
				{
					//covered, calculate armor rating vs penetration
					if(penetration >= armorRating)
					{
						//penetrated the armor
						finalDamage = damage * Mathf.Clamp01((penetration - armorRating) / armorRating);
					}
					else
					{
						//not penetrated
						return true;
					}
				}


			}

			MyStatus.Health -= finalDamage;

			if(MyAI.ControlType == AIControlType.Player)
			{
				GameManager.Inst.CameraShaker.TriggerScreenShake(0.1f, 0.2f);
			}
		}

		if(MyStatus.Health <= 0)
		{
			MyStatus.Health = 0;
			OnDeath();
		}

		return false;
	}



	public override void SendDelayCallBack (float delay, DelayCallBack callback, object parameter)
	{
		StartCoroutine(WaitAndCallback(delay, callback, parameter));
	}

	public WeaponAnimType GetCurrentAnimWeapon()
	{
		return (WeaponAnimType)MyAnimator.GetInteger("WeaponType");
	}








	public bool IsThrowing()
	{
		return _isThrowing;
	}

	public bool IsComboAttack()
	{
		return _isComboAttack;
	}

	public Vector3 GetLockedAimTarget()
	{
		return _throwTarget;
	}

	public int GetMeleeStrikeStage()
	{
		return _meleeStrikeStage;
	}






	public void OnSuccessfulShot()
	{
		MyAnimator.SetTrigger("Shoot");
		StartCoroutine(WaitAndMuzzleClimb(0.05f));
		this.MyStatus.ArmFatigue += 1f;
		if(this.MyStatus.ArmFatigue > this.MyStatus.MaxArmFatigue)
		{
			this.MyStatus.ArmFatigue = this.MyStatus.MaxArmFatigue;
		}


	}

	public override void OnSuccessfulHit (Character target)
	{
		if(GameManager.Inst.PlayerControl.SelectedPC == this)
		{
			GameManager.Inst.CursorManager.OnHitMarkerShow();
		}
		else
		{
			if(target.MyStatus.Health <= 0)
			{
				MyAI.Bark("Target is down!");
			}
		}
	}


	public void OnThrowLeaveHand()
	{
		_thrownObjectInHand.transform.parent = null;
		Vector3 distance = _throwTarget - transform.position;

		float magnitude = Mathf.Clamp(distance.magnitude, 10, 15);
		Vector3 direction = distance.normalized;

		/*
		if(UpperBodyState == HumanUpperBodyStates.HalfAim)
		{
			//direction = MyAimIK.solver.transform.forward;
			direction = distance.normalized;
		}
		*/




		_thrownObjectInHand.transform.position = this.MyReference.RightHandWeaponMount.transform.position + direction * 1f;

		Vector3 throwForce = (direction * 2 + Vector3.up).normalized * (magnitude * 0.8f);
		if(_isLowThrow)
		{
			throwForce = direction.normalized * (magnitude * 0.8f);
		}

		_thrownObjectInHand.GetComponent<Rigidbody>().isKinematic = false;
		_thrownObjectInHand.GetComponent<Rigidbody>().AddForce(throwForce, ForceMode.Impulse);
		_thrownObjectInHand.GetComponent<Rigidbody>().AddTorque((transform.right + transform.up) * 6, ForceMode.Impulse);
		_thrownObjectInHand.IsThrown = true;
		Explosive explosive = _thrownObjectInHand.GetComponent<Explosive>();
		if(explosive != null)
		{
			explosive.IsEnabled = true;
		}


		_thrownObjectInHand = null;

		if(this.Inventory.ThrowSlot != null)
		{
			//remove either one of the items in backpack or remove the one in bodyslot
			GridItemData itemInBackpack = this.Inventory.FindItemInBackpack(this.Inventory.ThrowSlot.ID);
			if(itemInBackpack != null)
			{
				Item itemToRemove = itemInBackpack.Item;
				this.Inventory.RemoveItemFromBackpack(itemToRemove);
			}
			else
			{
				this.Inventory.ThrowSlot = null;
			}
		}
	}

	public void OnThrowFinish()
	{
		IsBodyLocked = false;

		ActionState = HumanActionStates.None;

		if(UpperBodyState == HumanUpperBodyStates.HalfAim)
		{
			SendCommand(CharacterCommands.Aim);
		}
		else
		{
			SendCommand(CharacterCommands.StopAim);
		}

		MyHeadIK.SmoothEnable();

		//move weapon back to right hand mount
		if(this.MyReference.CurrentWeapon != null)
		{
			Weapon myWeapon = this.MyReference.CurrentWeapon.GetComponent<Weapon>();
			myWeapon.transform.parent = MyReference.RightHandWeaponMount.transform;
			myWeapon.transform.localPosition = myWeapon.InHandPosition;
			myWeapon.transform.localEulerAngles = myWeapon.InHandAngles;
		}



	}

	public void OnReloadFinish()
	{
		if(ActionState == HumanActionStates.Reload && this.MyReference.CurrentWeapon != null)
		{
			Debug.Log("On Reload Finish");
			ActionState = HumanActionStates.None;

			if(UpperBodyState == HumanUpperBodyStates.Aim)
			{
				MyAimIK.solver.SmoothEnable(1f);
			}
			else
			{
				SendCommand(CharacterCommands.StopAim);
				MyHeadIK.SmoothEnable();
			}

			if(MyAnimator.GetInteger("WeaponType") == (int)WeaponAnimType.Longgun)
			{
				MyLeftHandIK.SmoothEnable(6);
			}
			else
			{
				if(UpperBodyState == HumanUpperBodyStates.Aim)
				{
					MyLeftHandIK.SmoothEnable(6);
				}
				else
				{
					MyLeftHandIK.SmoothDisable();
				}
			}



			Gun gun = this.MyReference.CurrentWeapon.GetComponent<Gun>();
			if(gun != null)
			{
				int quantity = 0;

				//if it's a not shotgun then load entire mag
				if(gun.Magazine.ReloadType != ReloadAnimType.Shotgun)
				{

					if(GameManager.Inst.PlayerControl.Party.Members.Contains(this))
					{
						quantity = this.Inventory.RemoveItemsFromBackpack(gun.Magazine.LoadedAmmoID, gun.Magazine.MaxCapacity - gun.Magazine.AmmoLeft);
					}
					else
					{
						quantity = gun.Magazine.MaxCapacity;
					}


					gun.WeaponItem.SetAttribute("_LoadedAmmos", gun.Magazine.AmmoLeft + quantity);
					gun.Refresh();
				}
				else
				{
					//if it's a shotgun then only load one
					if(GameManager.Inst.PlayerControl.Party.Members.Contains(this))
					{
						quantity = this.Inventory.RemoveItemsFromBackpack(gun.Magazine.LoadedAmmoID, 1);
					}
					else
					{
						quantity = 1;
					}

					gun.WeaponItem.SetAttribute("_LoadedAmmos", gun.Magazine.AmmoLeft + quantity);
					gun.Refresh();

					//if not fully loaded then reload again
					if(gun.Magazine.AmmoLeft < gun.Magazine.MaxCapacity)
					{
						SendCommand(CharacterCommands.Reload);
					}

				}


				

			}

		}

	}

	public void OnTakeObjectFinish()
	{
		//if there's pickup item highlighted by the hand cursor, pick it up
		PickupItem pickup = MyAI.BlackBoard.PickupTarget;
		if(pickup != null)
		{
			int colPos;
			int rowPos;
			GridItemOrient orientation;
			/*
				if(this.Inventory.FitItemInBodySlot(pickup.Item))
				{
					GameObject.Destroy(MyAI.BlackBoard.PickupTarget.GetSparkleObject());
					GameObject.Destroy(MyAI.BlackBoard.PickupTarget.gameObject);
				}
				*/
			if(this.Inventory.FitItemInBackpack(pickup.Item, out colPos, out rowPos, out orientation))
			{
				Debug.Log("Found backpack fit " + colPos + ", " + rowPos + " orientation " + orientation);

				GridItemData itemData = new GridItemData(pickup.Item, colPos, rowPos, orientation, pickup.Quantity);
				this.Inventory.Backpack.Add(itemData);
				GameObject.Destroy(MyAI.BlackBoard.PickupTarget.GetSparkleObject());
				GameObject.Destroy(MyAI.BlackBoard.PickupTarget.gameObject);

			}
			else
			{
				Debug.Log("Wont fit");
				if(this.MyAI.ControlType == AIControlType.Player)
				{
					UIEventHandler.Instance.TriggerToggleInventory();
					GameManager.Inst.UIManager.WindowPanel.InventoryPanel.AddUnfitItem(pickup);
					GameObject.Destroy(MyAI.BlackBoard.PickupTarget.GetSparkleObject());
					GameObject.Destroy(MyAI.BlackBoard.PickupTarget.gameObject);
				}
			}

		}

		MyAI.BlackBoard.PickupTarget = null;

		//move weapon back to right hand mount
		if(this.MyReference.CurrentWeapon != null && MyAnimator.GetInteger("WeaponType") == (int)WeaponAnimType.Longgun)
		{
			Weapon myWeapon = this.MyReference.CurrentWeapon.GetComponent<Weapon>();
			myWeapon.transform.parent = MyReference.RightHandWeaponMount.transform;
			myWeapon.transform.localPosition = myWeapon.InHandPosition;
			myWeapon.transform.localEulerAngles = myWeapon.InHandAngles;
		}
	}

	public void OnLongGunPullOutFinish()
	{
		

		if(UpperBodyState == HumanUpperBodyStates.Aim)
		{
			SendCommand(CharacterCommands.Aim);
		}

		ActionState = HumanActionStates.None;
		this.MyLeftHandIK.SmoothEnable(6);
	}

	public void OnLongGunPutAwayFinish()
	{
		ActionState = HumanActionStates.None;
	}
		
	public void OnMeleePullOutFinish()
	{
		
		ActionState = HumanActionStates.None;
		//SendCommand(CharacterCommands.Aim);
	}

	public void OnMeleePutAwayFinish()
	{
		ActionState = HumanActionStates.None;
	}

	public void OnPistolPullOutFinish()
	{
		
		if(UpperBodyState == HumanUpperBodyStates.Aim)
		{
			SendCommand(CharacterCommands.Aim);
		}
		else
		{
			MyLeftHandIK.SmoothDisable(6);
		}

		ActionState = HumanActionStates.None;
	}

	public void OnPistolPutAwayFinish()
	{
		ActionState = HumanActionStates.None;
	}

	public void OnGrenadePullOutFinish()
	{
		ActionState = HumanActionStates.None;
		if(UpperBodyState == HumanUpperBodyStates.Aim)
		{
			SendCommand(CharacterCommands.Aim);
		}

		MyLeftHandIK.SmoothDisable(6);

	}

	public void OnStartStrangle()
	{
		if(MyAI.BlackBoard.TargetEnemy == null || Vector3.Distance(MyAI.BlackBoard.TargetEnemy.transform.position, transform.position) > 0.5f)
		{
			//cancel the strangle
			MyAnimator.SetTrigger("Cancel");
		}

		Character target = MyAI.BlackBoard.TargetEnemy;
		target.SendCommand(CharacterCommands.Idle);
		target.SendCommand(CharacterCommands.StopAim);
		target.IsBodyLocked = true;
		target.SendDamage(1, 0, Vector3.zero, this, null);


		//align enemy facing direction to player's
		Vector3 lineOfSight = target.transform.position - transform.position;
		lineOfSight = new Vector3(lineOfSight.x, 0, lineOfSight.z);
		Quaternion rotation = Quaternion.LookRotation(lineOfSight);
		target.transform.rotation = rotation;
		target.MyAnimator.SetTrigger("GetStrangled");


		Stealth.SetNoiseLevel(10, 0.8f);
	}

	public void OnEndStrangle()
	{
		if(ActionState == HumanActionStates.Strangle)
		{
			ActionState = HumanActionStates.None;

			Vector3 lineOfSight = _strangleTarget.transform.position - transform.position;
			transform.position = _strangleTarget.transform.position - lineOfSight.normalized;

			_strangleTarget.IsBodyLocked = true;

			IsBodyLocked = false;
			MyNavAgent.enabled = true;
			_strangleTarget = null;
		}
	}

	public void OnInjury(Vector3 normal, bool isKnockBack)
	{
		if(ActionState == HumanActionStates.None)
		{
			if(!isKnockBack || normal == Vector3.zero)
			{
				this.MyAnimator.SetTrigger("Injure");
				if(MyAnimator.GetBool("IsAiming"))
				{
					MyAimIK.solver.IKPositionWeight = 0;
					MyAimIK.solver.SmoothEnable(1);
				}

				if(MyLeftHandIK.IsEnabled() && MyAnimator.GetInteger("WeaponType") == (int)WeaponAnimType.Longgun)
				{
					MyLeftHandIK.InstantDisable();
					MyLeftHandIK.SmoothEnable(1);
				}

				MyHeadIK.Weight = 0;
				MyHeadIK.SmoothEnable(1);
			}
			else
			{
				//if normal is in front of character then knock back, other knock forward

				normal = new Vector3(normal.x, 0, normal.z);
				float bodyAngle = Vector3.Angle(transform.forward, normal * -1);

				Vector3 lookDir = normal;


				if(bodyAngle <= 90)
				{
					lookDir = normal * -1;
					MyAI.BlackBoard.AnimationAction = AnimationActions.KnockBack;
					MyAI.BlackBoard.ActionMovementDest = transform.position + normal.normalized * 1; //transform.position - transform.forward * 1;
					MyAI.BlackBoard.ActionMovementSpeed = 1.5f;
				}
				else
				{
					MyAI.BlackBoard.AnimationAction = AnimationActions.KnockForward;
					MyAI.BlackBoard.ActionMovementDest = transform.position + normal.normalized * 1.2f; //transform.position + transform.forward * 1.5f;
					MyAI.BlackBoard.ActionMovementSpeed = 2f;
				}

				lookDir = new Vector3(lookDir.x, 0, lookDir.z);
				transform.rotation = Quaternion.LookRotation(lookDir);

				SendCommand(CharacterCommands.PlayAnimationAction);
			}
		}

		ActionState = HumanActionStates.Twitch;
	}

	public void OnInjuryRecover()
	{
		ActionState = HumanActionStates.None;
	}

	public void OnDeath()
	{
		MyAI.OnDeath();
		Stealth.OnDeath();
		float posture = UnityEngine.Random.Range(0.1f, 200)/200f;

		this.MyAnimator.SetFloat("DeathPosture", posture);
		this.MyAnimator.SetBool("IsDead", true);

		CurrentAnimState = new HumanAnimStateDeath(this);
		IsBodyLocked = true;
		MyAimIK.solver.SmoothDisable(9);
		MyLeftHandIK.SmoothDisable(12);
		MyHeadIK.SmoothDisable(9);
		MyNavAgent.enabled = false;
		MyReference.Flashlight.Toggle(false);
		MyReference.LiveCollider.enabled = false;
		MyReference.DeathCollider.enabled = true;

		/*
		CapsuleCollider collider = GetComponent<CapsuleCollider>();
		collider.height = 0.5f;
		collider.radius = 0.6f;
		collider.center = new Vector3(0, 0, 0);
		collider.isTrigger = true;
		*/

	}

	public void OnStrangledDeath()
	{
		MyStatus.Health = 0;
		MyAI.OnDeath();
		Stealth.OnDeath();

		CurrentAnimState = new HumanAnimStateDeath(this);
		IsBodyLocked = true;
		MyAimIK.solver.SmoothDisable(9);
		MyLeftHandIK.SmoothDisable(12);
		MyHeadIK.SmoothDisable(9);
		MyNavAgent.enabled = false;
		MyReference.Flashlight.Toggle(false);

		CapsuleCollider collider = GetComponent<CapsuleCollider>();
		collider.height = 0.5f;
		collider.radius = 0.6f;
		collider.center = new Vector3(0, 0, 0);
		collider.isTrigger = true;


	}

	public void OnSwitchWeapon()
	{
		
		//first destroy the current weapon prefab
		GameObject.Destroy(this.MyReference.CurrentWeapon);
		if(_thrownObjectInHand != null)
		{
			GameObject.Destroy(_thrownObjectInHand.gameObject);
		}

		this.MyStatus.ResetSpeedModifier();

		if(_weaponToSwitch != null && _weaponToSwitch.Type == ItemType.Thrown)
		{

			DrawNextGrenade();


		}
		else if(_weaponToSwitch != null)
		{
			
			//clear weapon in body
			if(_weaponToSwitch.Type == ItemType.PrimaryWeapon)
			{
				MyAI.WeaponSystem.ClearPrimaryWeapon();
			}
			else if(_weaponToSwitch.Type == ItemType.SideArm)
			{
				MyAI.WeaponSystem.ClearSideArm();
			}

			//load a new weapon and set position/rotation/parent
			GameObject obj = GameObject.Instantiate(Resources.Load(_weaponToSwitch.PrefabName) as GameObject);
			Weapon newWeapon = obj.GetComponent<Weapon>();
			newWeapon.transform.parent = MyReference.RightHandWeaponMount.transform;
			newWeapon.transform.localPosition = newWeapon.InHandPosition;
			newWeapon.transform.localEulerAngles = newWeapon.InHandAngles;

			/*
			GunMagazine magazine = newWeapon.GetComponent<GunMagazine>();
			if(magazine != null)
			{
				magazine.AmmoLeft = (int)_weaponToSwitch.GetAttributeByName("_LoadedAmmos").Value;
				magazine.LoadedAmmoID = (string)_weaponToSwitch.GetAttributeByName("_LoadedAmmoID").Value;
			}
			*/

			newWeapon.Attacker = this;
			newWeapon.Rebuild(OnSuccessfulShot, _weaponToSwitch);

			bool isRanged = (bool)_weaponToSwitch.GetAttributeByName("_IsRanged").Value;
			if(isRanged)
			{
				this.MyAimIK.solver.transform = newWeapon.AimTransform;
				AimTransform = newWeapon.AimTransform;
				this.MyLeftHandIK.Target = newWeapon.ForeGrip;
			}
			else
			{
				this.MyAimIK.solver.transform = MyReference.TorsoWeaponMount.transform;
				AimTransform = MyReference.TorsoWeaponMount.transform;
			}

			this.MyReference.CurrentWeapon = obj;

			float lessSpeed = newWeapon.GetTotalLessMoveSpeed();

			this.MyStatus.RunSpeedModifier = Mathf.Clamp(this.MyStatus.RunSpeedModifier - lessSpeed, 0.9f, 1.2f);
			this.MyStatus.SprintSpeedModifier = Mathf.Clamp(this.MyStatus.SprintSpeedModifier, 0.9f, 1.1f);
			this.MyStatus.StrafeSpeedModifier = Mathf.Clamp(this.MyStatus.StrafeSpeedModifier - lessSpeed, 0.9f, 1.2f);


		}
		else
		{
			this.MyReference.CurrentWeapon = null;
		}

		this.MyAI.BlackBoard.EquippedWeapon = _weaponToSwitch;

		this.MyAI.WeaponSystem.LoadWeaponsFromInventory();

	}

	public void OnMeleeStrikeHalfWay()
	{
		ActionState = HumanActionStates.Melee;
		_meleeStrikeStage = 1;
		if(MyReference.CurrentWeapon != null)
		{
			MyReference.CurrentWeapon.GetComponent<MeleeWeapon>().SwingStart();
		}
			

		Debug.Log("on strike half way ");
	}

	public void OnMeleeComboStageTwo()
	{
		ActionState = HumanActionStates.Melee;
		_meleeStrikeStage = 1;
		MyReference.CurrentWeapon.GetComponent<MeleeWeapon>().SwingStop();
		OnAnimationActionEnd();
		//IsMoveLocked = true;
		_isComboAttack = false;
	}



	public void OnMeleeStrikeLeftFinish()
	{
		ActionState = HumanActionStates.None;
		_meleeStrikeStage = 0;
		IsMoveLocked = false;
		_isComboAttack = false;
		MyHeadIK.SmoothEnable(9);
		MyReference.CurrentWeapon.GetComponent<MeleeWeapon>().SwingStop();
		Debug.Log("strike left finished");
	}

	public void OnMeleeStrikeRightFinish()
	{
		ActionState = HumanActionStates.None;
		_meleeStrikeStage = 0;
		IsMoveLocked = false;
		IsBodyLocked = false;
		_isComboAttack = false;

		MyHeadIK.SmoothEnable(9);
		if(MyReference.CurrentWeapon != null)
		{
			MyReference.CurrentWeapon.GetComponent<MeleeWeapon>().SwingStop();
		}

		if(MyReference.RightFoot != null)
		{
			MyReference.RightFoot.SetActive(false);
		}

		Debug.Log("strike right finished");
	}

	public void OnMeleeBlockFinish()
	{
		this.MyLeftHandIK.InstantDisable();
		ActionState = HumanActionStates.None;
		_meleeStrikeStage = 0;
		IsMoveLocked = false;
	}

	public void OnAnimationActionEnd()
	{
		SendCommand(CharacterCommands.AnimationActionDone);
	}





	private void DrawNextGrenade()
	{
		//first remove anything in hand already
		if(_thrownObjectInHand != null)
		{
			GameObject.Destroy(_thrownObjectInHand.gameObject);
		}

		//check what kind of item is in thrown slot
		Item throwItem = this.Inventory.ThrowSlot;
		if(throwItem == null)
		{
			//we are going to throw a rock
			_thrownObjectInHand = ((GameObject)GameObject.Instantiate(Resources.Load("ThrowingRock"))).GetComponent<ThrownObject>();
		}
		else
		{
			
			_thrownObjectInHand = ((GameObject)GameObject.Instantiate(Resources.Load(throwItem.PrefabName))).GetComponent<ThrownObject>();
		}


		_thrownObjectInHand.Thrower = this;
		Explosive explosive = _thrownObjectInHand.GetComponent<Explosive>();
		if(explosive != null)
		{
			explosive.Attacker = this;
		}

		_thrownObjectInHand.GetComponent<Rigidbody>().isKinematic = true;

		_thrownObjectInHand.transform.parent = this.MyReference.RightHandWeaponMount.transform;
		_thrownObjectInHand.transform.localPosition = _thrownObjectInHand.InHandPosition;
		_thrownObjectInHand.transform.localEulerAngles = _thrownObjectInHand.InHandRotation;

		//this.MyAimIK.solver.transform = _thrownObjectInHand.transform.Find("AimTransform");
		//AimTransform = this.MyAimIK.solver.transform;

		this.MyAimIK.solver.transform = this.MyReference.TorsoWeaponMount.transform;
		AimTransform = this.MyAimIK.solver.transform;
	}



	private void UpdateLayerWeights()
	{
		if(_layerDState)
		{
			this.MyAnimator.SetLayerWeight(this.MyAnimator.GetLayerIndex("FullBodyOverride-D"), 
				Mathf.Lerp(this.MyAnimator.GetLayerWeight(this.MyAnimator.GetLayerIndex("FullBodyOverride-D")), 1, Time.deltaTime * 1));
		}
		else
		{
			this.MyAnimator.SetLayerWeight(this.MyAnimator.GetLayerIndex("FullBodyOverride-D"), 
				Mathf.Lerp(this.MyAnimator.GetLayerWeight(this.MyAnimator.GetLayerIndex("FullBodyOverride-D")), 0, Time.deltaTime * 1));
		}
	}

	private void UpdateLookDirection()
	{
		//get the direction of look on the xz plane
		Vector3 lookDir = LookTarget.position - transform.position;
		lookDir = new Vector3(lookDir.x, 0, lookDir.z);
		float lookBodyAngle = Vector3.Angle(lookDir, transform.right);

		//manipulate lookBodyAngle so it's not linear
		//float controlValue = lookBodyAngle * lookBodyAngle / 100;
		this.MyAnimator.SetFloat("LookBodyAngle", lookBodyAngle);



	}

	private void UpdateDestBodyAngle()
	{
		//get the direction of destination on the xz plane
		Vector3 destDir = this.MyNavAgent.velocity; //this.Destination.Value - transform.position;
		destDir = new Vector3(destDir.x, 0, destDir.z);
		float destBodyAngle = Vector3.Angle(destDir, transform.right);
		
		//manipulate destBodyAngle so it's not linear
		float controlValue = destBodyAngle;
		this.MyAnimator.SetFloat("DestBodyAngle", controlValue);
		//Debug.Log(destBodyAngle.ToString() + " " + controlValue);
	}

	private void UpdateFatigue()
	{
		this.MyStatus.ArmFatigue -= Time.deltaTime * 3;
		if(this.MyStatus.ArmFatigue < 0)
		{
			this.MyStatus.ArmFatigue = 0;
		}
	}




	IEnumerator WaitAndCallback(float waitTime, Character.DelayCallBack callback, object parameter)
	{
		yield return new WaitForSeconds(waitTime);
		callback(parameter);
	}

	IEnumerator WaitAndMuzzleClimb(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);

		if(this.MyReference.CurrentWeapon != null)
		{
			if(AimTarget.localPosition.y < 0.1f)
			{
				float climb = Mathf.Clamp(this.MyReference.CurrentWeapon.GetComponent<Gun>().GetRecoil() 
					* (this.MyStatus.ArmFatigue / this.MyStatus.MaxArmFatigue), 0, 1);
				AimTarget.localPosition += new Vector3(0, climb, 0);
			}
			else
			{
				float maxSpread = Mathf.Clamp(this.MyReference.CurrentWeapon.GetComponent<Gun>().GetRecoil()
					* (this.MyStatus.ArmFatigue / this.MyStatus.MaxArmFatigue), 0, 0.5f) / 2;
				AimTarget.localPosition = new Vector3(0, AimTarget.localPosition.y, 2);
				//AimTarget.localPosition += new Vector3(UnityEngine.Random.Range(-1 * maxSpread, maxSpread), 0, 0);
			}
		}

	}

	IEnumerator WaitAndCreateThrowObject(float waitTime, string objectName)
	{
		yield return new WaitForSeconds(waitTime);



	}
}
