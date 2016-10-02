﻿using UnityEngine;
using System.Collections;

public class AIWeapon
{
	public AIWeaponStates AIWeaponState;

	public Weapon PrimaryWeapon;//points only to the weapon on sling or holster
	public Weapon SideArm;



	private Character _parentCharacter;
	private AIWeaponTriggerState _triggerState;
	private float _turnMoveScatter; //a percentage value between 0 and 1


	public void Initialize(Character c)
	{
		_parentCharacter = c;



		//TODO: when we have weapon on the shoulder/holster, update this to point to that
		_parentCharacter.MyAI.BlackBoard.FocusedWeapon = new Weapon();
	}

	public void LoadWeaponsFromInventory()
	{
		//if rifle slot is empty but primary weapon is already equipped, remove the equipped weapon
		if(_parentCharacter.MyAI.BlackBoard.EquippedWeapon != null && _parentCharacter.MyAI.BlackBoard.EquippedWeapon.Type == ItemType.PrimaryWeapon 
			&& (_parentCharacter.Inventory.RifleSlot == null || _parentCharacter.Inventory.RifleSlot != _parentCharacter.MyAI.BlackBoard.EquippedWeapon))
		{
			_parentCharacter.SendCommand(CharacterCommands.Unarm);
		}

		//if side arm slot is empty but secondary weapon is already equipped, remove the equipped weapon
		if(_parentCharacter.MyAI.BlackBoard.EquippedWeapon != null && _parentCharacter.MyAI.BlackBoard.EquippedWeapon.Type == ItemType.SideArm 
			&& (_parentCharacter.Inventory.SideArmSlot == null || _parentCharacter.Inventory.SideArmSlot != _parentCharacter.MyAI.BlackBoard.EquippedWeapon))
		{
			_parentCharacter.SendCommand(CharacterCommands.Unarm);
		}

		if(_parentCharacter.MyAI.BlackBoard.EquippedWeapon != null && _parentCharacter.MyAI.BlackBoard.EquippedWeapon.Type == ItemType.Thrown)
		{
			_parentCharacter.SendCommand(CharacterCommands.Unarm);
		}

		if(PrimaryWeapon != null)
		{
			GameObject.Destroy(PrimaryWeapon.gameObject);
		}


		if(SideArm != null)
		{
			GameObject.Destroy(SideArm.gameObject);
		}



		
		//load weapons based on character inventory
		if(_parentCharacter.Inventory.RifleSlot != null && _parentCharacter.MyAI.BlackBoard.EquippedWeapon != _parentCharacter.Inventory.RifleSlot)
		{

			GameObject obj = GameObject.Instantiate(Resources.Load(_parentCharacter.Inventory.RifleSlot.PrefabName) as GameObject);
			Weapon newWeapon = obj.GetComponent<Weapon>();
			newWeapon.transform.parent = _parentCharacter.MyReference.SlingWeaponMount.transform;
			newWeapon.transform.localPosition = newWeapon.InHolsterPosition;
			newWeapon.transform.localEulerAngles = newWeapon.InHolsterAngles;
			PrimaryWeapon = newWeapon;
			/*
			GunMagazine magazine = newWeapon.GetComponent<GunMagazine>();
			if(magazine != null)
			{
				magazine.AmmoLeft = (int)_parentCharacter.Inventory.RifleSlot.GetAttributeByName("_LoadedAmmos").Value;
				magazine.LoadedAmmoID = (string)_parentCharacter.Inventory.RifleSlot.GetAttributeByName("_LoadedAmmoID").Value;
			}
			*/
		}


		if(_parentCharacter.Inventory.SideArmSlot != null && _parentCharacter.MyAI.BlackBoard.EquippedWeapon != _parentCharacter.Inventory.SideArmSlot)
		{

			GameObject obj = GameObject.Instantiate(Resources.Load(_parentCharacter.Inventory.SideArmSlot.PrefabName) as GameObject);
			Weapon newWeapon = obj.GetComponent<Weapon>();
			newWeapon.transform.parent = _parentCharacter.MyReference.HolsterWeaponMount.transform;
			newWeapon.transform.localPosition = newWeapon.InHolsterPosition;
			newWeapon.transform.localEulerAngles = newWeapon.InHolsterAngles;
			SideArm = newWeapon;
			/*
			GunMagazine magazine = newWeapon.GetComponent<GunMagazine>();
			if(magazine != null)
			{
				magazine.AmmoLeft = (int)_parentCharacter.Inventory.SideArmSlot.GetAttributeByName("_LoadedAmmos").Value;
				magazine.LoadedAmmoID = (string)_parentCharacter.Inventory.SideArmSlot.GetAttributeByName("_LoadedAmmoID").Value;
			}
			*/
		}

		if(_parentCharacter.MyReference.CurrentWeapon != null)
		{
			//refresh attributes
			_parentCharacter.MyReference.CurrentWeapon.GetComponent<Weapon>().Refresh();
		}

	}

	public void ClearPrimaryWeapon()
	{
		GameManager.Destroy(PrimaryWeapon.gameObject);
		PrimaryWeapon = null;
	}

	public void ClearSideArm()
	{
		GameManager.Destroy(SideArm.gameObject);
		SideArm = null;
	}

	public float GetTurnMoveScatter()
	{
		return _turnMoveScatter;
	}

	public void UpdatePerFrame()
	{
		if(_parentCharacter.MyReference.CurrentWeapon == null)
		{
			return;
		}

		if(!_parentCharacter.MyReference.CurrentWeapon.GetComponent<Weapon>().IsRanged)
		{
			return;
		}

		//check if there's ammo in the magazine. if low then reload. When there are no enemies in sight,
		//reload even when not empty; don't do this on player controlled character 
		GunMagazine magazine = _parentCharacter.MyReference.CurrentWeapon.GetComponent<GunMagazine>();
		if(magazine.AmmoLeft <= 0)
		{
			StopFiringRangedWeapon();
			_parentCharacter.SendCommand(CharacterCommands.Reload);
		}
		else if(_parentCharacter.MyAI.BlackBoard.TargetEnemy == null && magazine.AmmoLeft <= magazine.MaxCapacity * 0.6f && _parentCharacter.MyAI.ControlType != AIControlType.Player)
		{
			StopFiringRangedWeapon();
			_parentCharacter.SendCommand(CharacterCommands.Reload);

		}
		else if(AIWeaponState == AIWeaponStates.FiringRangedWeapon && _parentCharacter.ActionState == HumanActionStates.None)
		{
			HandleFiringRangedWeapon();
		}


		//calculate turn/moving scatter
		float scatterRestore = 3; //TODO: later the restore speed will be based on player skill
		if(((HumanCharacter)_parentCharacter).CurrentAnimState.IsRotatingBody())
		{
			//increase scatter
			_turnMoveScatter = Mathf.Lerp(_turnMoveScatter, 1, Time.deltaTime * 6);
		}
		else if(_parentCharacter.MyNavAgent.velocity.magnitude > 0.1f)
		{
			//increase scatter based on velocity
			_turnMoveScatter = Mathf.Lerp(_turnMoveScatter, Mathf.Clamp01(_parentCharacter.MyNavAgent.velocity.magnitude / 6), Time.deltaTime * 6);
		}
		else
		{
			//decrease scatter
			_turnMoveScatter = Mathf.Lerp(_turnMoveScatter, 0, Time.deltaTime * scatterRestore);
		}


	}

	public void StartFiringRangedWeapon()
	{
		AIWeaponState = AIWeaponStates.FiringRangedWeapon;
		UpdatePerFrame();
	}

	public void StopFiringRangedWeapon()
	{
		if(_parentCharacter.MyReference.CurrentWeapon != null && _parentCharacter.MyReference.CurrentWeapon.GetComponent<Weapon>().IsRanged)
		{
			_parentCharacter.MyReference.CurrentWeapon.GetComponent<Gun>().TriggerRelease();
		}
		AIWeaponState = AIWeaponStates.None;
		_triggerState = AIWeaponTriggerState.Released;
	}

	public CharacterCommands GetBestWeaponChoice()
	{	
		if(PrimaryWeapon != null)
		{
			return CharacterCommands.SwitchWeapon2;
		}

		if(SideArm != null)
		{
			return CharacterCommands.SwitchWeapon1;
		}

		return CharacterCommands.StopAim;

	}

	public bool IsCurrentWeaponScoped()
	{
		if(_parentCharacter.MyReference.CurrentWeapon != null)
		{
			Weapon currentWeapon = _parentCharacter.MyReference.CurrentWeapon.GetComponent<Weapon>();
			return currentWeapon.IsScoped;
		}
		else
		{
			return false;
		}
	}

	public Weapon GetCurrentWeapon()
	{
		if(_parentCharacter.MyReference.CurrentWeapon != null)
		{
			Weapon currentWeapon = _parentCharacter.MyReference.CurrentWeapon.GetComponent<Weapon>();
			return currentWeapon;
		}
		else
		{
			return null;
		}
	}

	private void HandleFiringRangedWeapon()
	{
		/*
		if(_parentCharacter.MyAI.BlackBoard.TargetEnemy == null)
		{
			StopFiringRangedWeapon();
			return;
		}
		*/

		float aimAngleThreshold = 10;
		Vector3 aimPoint = _parentCharacter.MyAI.BlackBoard.AimPoint;
		float aimAngle = Vector3.Angle(aimPoint - _parentCharacter.MyReference.CurrentWeapon.transform.position, _parentCharacter.MyReference.CurrentWeapon.transform.forward);
		float climb = _parentCharacter.AimTarget.localPosition.y;
		bool aimReady = _parentCharacter.MyAI.ControlType == AIControlType.Player ? true : (aimAngle < aimAngleThreshold);
		//Debug.Log("Trigger pull aim ready? " + aimReady + " " + _parentCharacter.name);
		if(climb >= 0.05f && _triggerState == AIWeaponTriggerState.WaitForRecoil)
		{

		}
		else if(climb < 0.05f && aimReady)
		{
			
			Gun gun = _parentCharacter.MyReference.CurrentWeapon.GetComponent<Gun>();
			if(gun.CurrentFireMode == GunFireModes.Full && _triggerState != AIWeaponTriggerState.Pulled)
			{
				
				gun.TriggerPull();
				_triggerState = AIWeaponTriggerState.Pulled;
			}
			else if(gun.CurrentFireMode == GunFireModes.Semi || gun.CurrentFireMode == GunFireModes.Burst || gun.CurrentFireMode == GunFireModes.Pump || gun.CurrentFireMode == GunFireModes.Bolt)
			{
				gun.TriggerPull();
				gun.TriggerRelease();
				_triggerState = AIWeaponTriggerState.Released;
			}
		}
		else if(climb >= 0.2f && _triggerState == AIWeaponTriggerState.Pulled)
		{
			_parentCharacter.MyReference.CurrentWeapon.GetComponent<Gun>().TriggerRelease();
			_triggerState = AIWeaponTriggerState.WaitForRecoil;
		}
	}
}

public enum AIWeaponStates
{
	None,
	FiringRangedWeapon,

}

public enum AIWeaponTriggerState
{
	WaitForRecoil,
	Pulled,
	Released,
}