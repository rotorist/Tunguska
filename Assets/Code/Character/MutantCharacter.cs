﻿using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

public class MutantCharacter : Character
{
	public LeftHandIKControl MyLeftHandIK;
	public AimIK MyAimIK;
	public HeadIKControl MyHeadIK;

	public MutantAnimStateBase CurrentAnimState;


	public MutantUpperBodyStates UpperBodyState;
	public bool IsRangedCapable;

	public override bool IsAlive 
	{
		get 
		{
			return this.MyStatus.Health > 0;
		}
	}



	private Character _strangleTarget;
	private int _meleeStrikeStage;//0, 1, 2, 3
	private int _meleeSide; //0=left, 1=right

	void Update()
	{
		CurrentAnimState.Update();




		if(MyStatus.Health > 0)
		{
			UpdateLookDirection();

			UpdateDestBodyAngle();



			MyAI.AlwaysPerFrameUpdate();
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
		UpperBodyState = MutantUpperBodyStates.Idle;
		ActionState = HumanActionStates.None;


		this.MyStatus = new CharacterStatus();
		this.MyStatus.Initialize();

		this.MyStatus.RunSpeedModifier = 1f;

		this.Stealth = new CharacterStealth(this);

		this.Inventory = new CharacterInventory();


		//each time a human char is initialized it's added to NPC manager's list of human characters to keep track of
		GameManager.Inst.NPCManager.AddMutantCharacter(this);

		CurrentAnimState = new MutantAnimStateIdle(this);
		//SendCommand(CharacterCommands.Unarm);

		MyAI = GetComponent<AI>();
		MyAI.Initialize(this);

		this.ArmorSystem = new ArmorSystem(this);

		_meleeStrikeStage = 0;
	
	}

	public override void LoadCharacterModel (string prefabName)
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

		if(this.MyReference.FixedMeleeLeft != null)
		{
			this.MyReference.FixedMeleeLeft.Attacker = this;
			this.MyReference.FixedMeleeLeft.Rebuild(null, null);
		}

		if(this.MyReference.FixedMeleeRight != null)
		{
			this.MyReference.FixedMeleeRight.Attacker = this;
			this.MyReference.FixedMeleeRight.Rebuild(null, null);
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
		this.MyAnimator.SetInteger("AlertLevel", 0);

		this.Model = o;
		this.Model.name = prefabName;

		if(this.MyAI != null)
		{
			this.MyAI.BlackBoard.EquippedWeapon = null;
		}


		//subscribe events
		this.MyAnimEventHandler.OnMeleeStrikeLeftFinish -= OnMeleeStrikeFinish;
		this.MyAnimEventHandler.OnMeleeStrikeLeftFinish += OnMeleeStrikeFinish;

		this.MyAnimEventHandler.OnMeleeStrikeRightFinish -= OnMeleeStrikeFinish;
		this.MyAnimEventHandler.OnMeleeStrikeRightFinish += OnMeleeStrikeFinish;

		this.MyAnimEventHandler.OnMeleeBlocked -= OnMeleeBlocked;
		this.MyAnimEventHandler.OnMeleeBlocked += OnMeleeBlocked;

		this.MyAnimEventHandler.OnHitReover -= OnInjuryRecover;
		this.MyAnimEventHandler.OnHitReover += OnInjuryRecover;

		this.MyAnimEventHandler.OnAnimationActionEnd -= OnAnimationActionEnd;
		this.MyAnimEventHandler.OnAnimationActionEnd += OnAnimationActionEnd;

		this.MyAnimEventHandler.OnMeleeStrikeHalfWay -= OnMeleeStrikeHalfWay;
		this.MyAnimEventHandler.OnMeleeStrikeHalfWay += OnMeleeStrikeHalfWay;
	}

	public override void SendCommand (CharacterCommands command)
	{
		if(command == CharacterCommands.PlayAnimationAction)
		{
			CurrentAnimState = new MutantAnimStateAction(this);
			IsBodyLocked = true;
		}

		if(command == CharacterCommands.AnimationActionDone)
		{
			Debug.Log("Action Done");
			IsBodyLocked = false;
		}





		if(!IsBodyLocked && !IsMoveLocked)
		{
			CurrentAnimState.SendCommand(command);
		}

		if(IsBodyLocked)
		{
			return;
		}

		/*
		if(command == CharacterCommands.IdleAction)
		{
			if(UnityEngine.Random.value > 0.5f)
			{
				this.MyAnimator.SetTrigger("Agonize");
			}
			else
			{
				this.MyAnimator.SetTrigger("Convulse");
			}
		}
		*/

		if(command == CharacterCommands.RunningAttack)
		{
			this.MyAnimator.SetTrigger("RunningAttack");
			_meleeSide = 1;
			ActionState = HumanActionStates.Melee;
		}

		if(command == CharacterCommands.LeftAttack)
		{
			this.MyAnimator.SetTrigger("LeftAttack");
			_meleeSide = 0;
			ActionState = HumanActionStates.Melee;
		}

		if(command == CharacterCommands.RightAttack)
		{
			this.MyAnimator.SetTrigger("RightAttack");
			_meleeSide = 1;
			ActionState = HumanActionStates.Melee;
		}

		if(command == CharacterCommands.QuickAttack)
		{
			this.MyAnimator.SetTrigger("QuickAttack");
			_meleeSide = 1;
			ActionState = HumanActionStates.Melee;
		}


	}

	public override bool SendDamage(float damage, float penetration, Vector3 hitNormal, Character attacker, Weapon attackerWeapon)
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



		return false;
	}

	public override bool SendMeleeDamage (float sharpDamage, float bluntDamage, Vector3 hitNormal, Character attacker)
	{
		MyEventHandler.TriggerOnTakingHit();
		float attackerAngle = Vector3.Angle(transform.forward, transform.position - attacker.transform.position);
		if(ActionState == HumanActionStates.None && attackerAngle > 135 && UnityEngine.Random.value < 0.5f)
		{
			MyAnimator.SetTrigger("BlockSuccess");
			return true;
		}
		else
		{
			OnInjury(hitNormal, UnityEngine.Random.value > 0.5f);
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
		}

		return false;
	}

	public override bool SendCriticalDamage (float damage, float penetration, Vector3 hitNormal, Character attacker, Weapon attackerWeapon)
	{
		this.MyAnimator.SetTrigger("HitLeft");

		return true;
	}

	public override void SendDelayCallBack (float delay, DelayCallBack callback, object parameter)
	{
		//StartCoroutine(WaitAndCallback(delay, callback, parameter));
	}

	public override void OnSuccessfulHit (Character target)
	{
		SendCommand(CharacterCommands.AnimationActionDone);
	}


	public bool IsAlert()
	{
		if(MyAI.BlackBoard.GuardLevel > 1)
		{
			return true;
		}

		return false;
	}


	public void OnInjury(Vector3 normal, bool isHard)
	{
		if(!isHard)
		{
			if(normal != Vector3.zero)
			{
				float bodyAngle = Vector3.Angle(transform.right, normal * -1);

				if(bodyAngle <= 90)
				{
					this.MyAnimator.SetInteger("HitType", 0);
				}
				else
				{
					this.MyAnimator.SetInteger("HitType", 1);
				}
			}

			this.MyAnimator.SetTrigger("Hit");

			if(MyAnimator.GetBool("IsAiming"))
			{
				MyAimIK.solver.IKPositionWeight = 0;
				MyAimIK.solver.SmoothEnable(1);
			}


			MyHeadIK.Weight = 0;
			MyHeadIK.SmoothEnable(1);
		}
		else
		{
			MyAI.BlackBoard.AnimationAction = AnimationActions.KnockBack;
			MyAI.BlackBoard.ActionMovementDest = transform.position - transform.forward * 1;
			MyAI.BlackBoard.ActionMovementSpeed = 1.5f;

			SendCommand(CharacterCommands.PlayAnimationAction);
		}

		ActionState = HumanActionStates.Twitch;
	}

	public void OnInjuryRecover()
	{
		ActionState = HumanActionStates.None;
	}


	public void OnMeleeStrikeHalfWay()
	{
		ActionState = HumanActionStates.Melee;
		if(_meleeSide == 0)
		{
			MyReference.FixedMeleeLeft.GetComponent<MeleeWeapon>().SwingStart();
		}
		else if(_meleeSide == 1)
		{
			MyReference.FixedMeleeRight.GetComponent<MeleeWeapon>().SwingStart();
		}


	}

	public void OnMeleeStrikeFinish()
	{
		MyReference.FixedMeleeRight.GetComponent<MeleeWeapon>().SwingStop();
		MyReference.FixedMeleeLeft.GetComponent<MeleeWeapon>().SwingStop();
		ActionState = HumanActionStates.None;
	}

	public void OnMeleeBlocked()
	{
		Debug.Log("Blocked!");
		ActionState = HumanActionStates.None;
		MyAnimator.SetTrigger("Blocked");
	}

	public void OnAnimationActionEnd()
	{
		SendCommand(CharacterCommands.AnimationActionDone);
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
}