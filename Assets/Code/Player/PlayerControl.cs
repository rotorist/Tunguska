﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class PlayerControl
{

	#region Public Fields


	public PlayerParty Party;
	public bool IsGamePaused;
	public PlayerTimedAction TimedAction;

	public bool IsHoldToAim;

	public HumanCharacter SelectedPC
	{
		get { return Party.SelectedMember; }
	}
	#endregion
	
	#region Private Fields
	private bool _isMoveKeyDown;
	private PlayerMoveDirEnum _moveDirection;
	private PlayerMoveDirEnum _moveDirection2;
	private int _numberOfRocks;
	private GameObject _aimedObject;
	private Vector3 _tempGuardDest;

	private VignetteAndChromaticAberration _vignette;
	private int _vignetteFlashDir;

	private Light _playerLight;
	#endregion
	
	
	#region Public Methods

	public void Initialize()
	{
		Party = new PlayerParty(this);
		Party.Initialize();
		TimedAction = new PlayerTimedAction(this);

		_vignette = Camera.main.GetComponent<VignetteAndChromaticAberration>();

		IsHoldToAim = true;

		_numberOfRocks = 10;

		InputEventHandler.OnCameraSwitchMode += SwitchCameraMode;

		InputEventHandler.OnPlayerMove += this.OnPlayerMove;
		InputEventHandler.OnPlayerStopMove += this.OnPlayerStopMove;
		InputEventHandler.OnWeaponPullTrigger += this.OnWeaponPullTrigger;
		InputEventHandler.OnWeaponReleaseTrigger += this.OnWeaponReleaseTrigger;
		InputEventHandler.OnRMBDown += this.OnRMBDown;
		InputEventHandler.OnRMBUp += this.OnRMBUp;
		InputEventHandler.OnLRMBDown += this.OnLRMBDown;
		InputEventHandler.OnKick += this.OnKick;

		InputEventHandler.OnPlayerMoveLeft += this.OnPlayerMoveLeft;
		InputEventHandler.OnPlayerMoveRight += this.OnPlayerMoveRight;
		InputEventHandler.OnPlayerMoveUp += this.OnPlayerMoveUp;
		InputEventHandler.OnPlayerMoveDown += this.OnPlayerMoveDown;
		InputEventHandler.OnPlayerStopMoveLeft += this.OnPlayerStopMoveLeft;
		InputEventHandler.OnPlayerStopMoveRight += this.OnPlayerStopMoveRight;
		InputEventHandler.OnPlayerStopMoveUp += this.OnPlayerStopMoveUp;
		InputEventHandler.OnPlayerStopMoveDown += this.OnPlayerStopMoveDown;

		InputEventHandler.OnPlayerStartSprint += this.OnPlayerStartSprint;
		InputEventHandler.OnPlayerStopSprint += this.OnPlayerStopSprint;

		InputEventHandler.OnPlayerToggleSneak += OnToggleSneaking;
		InputEventHandler.OnPlayerThrow += OnThrow;

		InputEventHandler.OnPlayerSwitchWeapon2 += OnToggleWeapon2;
		InputEventHandler.OnPlayerSwitchWeapon1 += OnToggleWeapon1;
		InputEventHandler.OnPlayerSwitchThrown += OnToggleThrown;
		InputEventHandler.OnPlayerSwitchTool += OnToggleTool;

		InputEventHandler.OnPlayerReload += OnWeaponReload;
		InputEventHandler.OnToggleFlashlight += OnToggleFlashlight;

		InputEventHandler.OnGameTogglePause += ToggleGamePause;
		InputEventHandler.OnSelectMember1 += OnSelectMember1;
		InputEventHandler.OnSelectMember2 += OnSelectMember2;
		InputEventHandler.OnSelectMember3 += OnSelectMember3;
		InputEventHandler.OnSelectMember4 += OnSelectMember4;
		InputEventHandler.OnMouseSelect += OnPlayerMouseSelect;

		InputEventHandler.OnClearTask += OnClearTask;

		_moveDirection = PlayerMoveDirEnum.Stop;
		_moveDirection2 = PlayerMoveDirEnum.Stop;

		SelectedPC.CurrentStance = HumanStances.Run;

		_playerLight = GameObject.Find("PlayerLight").GetComponent<Light>();
		_playerLight.intensity = 0.1f;
	}
	


	public void PerFrameUpdate()
	{
		

		//set time scale for game pausing
		if(IsGamePaused)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, 0, Time.unscaledDeltaTime * 8);
		}
		else if(InputEventHandler.Instance.State == UserInputState.Normal)
		{
			Time.timeScale = Mathf.Lerp(Time.timeScale, 1, Time.unscaledDeltaTime * 8);
		}

		//upward raycast to detect ceiling and notify building to hide building components
		RaycastHit upHit;
		if(Physics.Raycast(SelectedPC.transform.position, Vector3.up, out upHit, 200, (1 << 9 | 1 << 8 | 1 << 10)))
		{
			BuildingComponent component = upHit.collider.GetComponent<BuildingComponent>();
			if(component != null)
			{
				component.Building.NotifyHidingComponent(component, SelectedPC.transform.position.y);
			}
		}

		//calculate aimpoint
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		int mask = ~(1 << 8 | 1 << 10);
		if(Physics.Raycast(ray, out hit, 100, mask))
		{
			_aimedObject = hit.collider.gameObject;
			if(SelectedPC.MyAI.ControlType == AIControlType.Player)
			{
				SelectedPC.AimPoint = hit.point;



			}
		}

		//snap auto-aim to closest enemy
		if(SelectedPC.MyAI.ControlType == AIControlType.Player && !IsGamePaused)// && SelectedPC.UpperBodyState == HumanUpperBodyStates.Aim)
		{
			Character hoverTarget = null;
			float minDist = 9999;

			foreach(Character c in GameManager.Inst.NPCManager.AllCharacters)
			{
				if(c.Faction != SelectedPC.Faction && c != SelectedPC && c.IsAlive)
				{
					//check if the middle point of the character's screen space coord is near mouse cursor
					Vector3 midPoint = c.transform.position + new Vector3(0, c.GetComponent<CapsuleCollider>().height/2, 0);
					Vector3 screenLoc = Camera.main.WorldToScreenPoint(midPoint);
					screenLoc = new Vector3(screenLoc.x, screenLoc.y, 0);
					float dist = Vector3.Distance(screenLoc, Input.mousePosition);
					if(dist < Screen.width/15 && dist < minDist)
					{
						minDist = dist;
						hoverTarget = c;
					}
				}
			}

			if(hoverTarget != null)
			{
				SelectedPC.MyAI.BlackBoard.TargetEnemy = hoverTarget;
			}
			else
			{
				SelectedPC.MyAI.BlackBoard.TargetEnemy = null;
			}
		}

		if(!IsGamePaused && SelectedPC.MyAI.ControlType == AIControlType.Player)
		{
			UpdatePlayerMovement();
			SelectedPC.Stealth.UpdatePerSchedulerFrame();
			SelectedPC.Stealth.UpdatePerFrame();
		}

		//if any member is selected, update vignette
		if(SelectedPC.MyAI.ControlType == AIControlType.Player)
		{
			//float intensity = GameManager.Inst.CameraController.VignetteCurve.Evaluate(SelectedPC.Stealth.Visibility / 50f); //Mathf.Clamp((1 - SelectedPC.Stealth.Visibility / 50) * 0.5f, 0, 0.4f);
			float intensity = GameManager.Inst.CameraController.VignetteCurve.Evaluate((RenderSettings.ambientIntensity - 0.25f) / 0.66f);
			//Debug.Log("Player visib " + SelectedPC.Stealth.Visibility + " intensity " + intensity);

			if(SelectedPC.Stealth.AlmostDetected > 0)
			{
				float flashIntensity = intensity * 0.3f;
				if(_vignette.intensity <= flashIntensity + 0.05f)
				{
					_vignetteFlashDir = 1;
				}
				else if(_vignette.intensity >= intensity - 0.05f)
				{
					_vignetteFlashDir = 0;
				}

				if(_vignetteFlashDir == 1)
				{
					_vignette.intensity = Mathf.Lerp(_vignette.intensity, intensity, Time.deltaTime * 3);
				}
				else
				{
					_vignette.intensity = Mathf.Lerp(_vignette.intensity, flashIntensity, Time.deltaTime * 3);
				}
			}
			else
			{
				if(NGUITools.GetActive(GameManager.Inst.UIManager.HUDPanel.Aperture.gameObject))
				{
					_vignette.intensity = 0;
				}
				else
				{
					_vignette.intensity = Mathf.Lerp(_vignette.intensity, intensity, Time.deltaTime * 3);
				}
			}
		}


		Party.PerFrameUpdate();
		TimedAction.PerFrameUpdate();
	}




	public void SwitchCameraMode()
	{
		CameraModeEnum cameraMode = GameManager.Inst.CameraController.GetCameraMode();

		if(cameraMode == CameraModeEnum.Leader)
		{
			GameManager.Inst.CameraController.SetCameraMode(CameraModeEnum.Party);
		}
		else
		{
			GameManager.Inst.CameraController.SetCameraMode(CameraModeEnum.Leader);
		}


	}

	public void ToggleGamePause()
	{
		IsGamePaused = !IsGamePaused;


		if(IsGamePaused)
		{
			//Party.Pause();

		}
		else
		{
			//Party.Unpause();

		}

		//Party.RefreshMarkerForAll();

	}

	public Vector3 GetTempGuardDest()
	{
		return _tempGuardDest;
	}

	public GameObject GetAimedObject()
	{
		return _aimedObject;
	}

	public void OnSelectMember1()
	{
		Party.SetActiveMember(0);
	}

	public void OnSelectMember2()
	{
		Party.SetActiveMember(1);
	}

	public void OnSelectMember3()
	{
		Party.SetActiveMember(2);
	}

	public void OnSelectMember4()
	{
		Party.SetActiveMember(3);
	}



	public void OnPlayerMove()
	{
		if(!IsGamePaused)
		{
			if(SelectedPC.UpperBodyState == HumanUpperBodyStates.Aim)
			{
				return;
			}

			Party.ClearTaskForSelectedMember();
			SelectedPC.GetComponent<HumanCharacter>().SendCommand(CharacterCommands.GoToPosition);
			_isMoveKeyDown = true;

		}

	}

	public void OnPlayerStopMove()
	{
		if(!IsGamePaused)
		{
			if(SelectedPC.UpperBodyState == HumanUpperBodyStates.Aim)
			{
				return;
			}

			HumanCharacter c = SelectedPC.GetComponent<HumanCharacter>();

			/*
			//get world position from mouse cursor
			RaycastHit hit;
			int mask = ~(1 << 8);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit, 100, mask))
			{
				c.Destination = hit.point;
			}
			*/
			if(_aimedObject.GetComponent<Character>() != null)
			{
				Character aimedCharacter = _aimedObject.GetComponent<Character>();
				SelectedPC.MyAI.BlackBoard.InteractTarget = aimedCharacter;
				if(aimedCharacter.MyStatus.Health > 0)
				{
					SelectedPC.MyAI.BlackBoard.PendingCommand = CharacterCommands.Talk;
				}
				else
				{
					SelectedPC.MyAI.BlackBoard.PendingCommand = CharacterCommands.Loot;
				}

				c.Destination = _aimedObject.transform.position;
				Debug.Log("destination " + c.Destination.Value);


			}
			else if(_aimedObject.GetComponent<PickupItem>() != null)
			{
				SelectedPC.MyAI.BlackBoard.PickupTarget = _aimedObject.GetComponent<PickupItem>();
				SelectedPC.MyAI.BlackBoard.PendingCommand = CharacterCommands.Pickup;
				c.Destination = c.AimPoint;
			}
			else if(_aimedObject.GetComponent<Chest>() != null)
			{
				SelectedPC.MyAI.BlackBoard.UseTarget = _aimedObject;
				SelectedPC.MyAI.BlackBoard.PendingCommand = CharacterCommands.Loot;
				c.Destination = c.AimPoint;
			}
			else
			{
				
				c.Destination = c.AimPoint;
				SelectedPC.MyAI.BlackBoard.PendingCommand = CharacterCommands.Idle;
				SelectedPC.MyAI.BlackBoard.PickupTarget = null;
				SelectedPC.MyAI.BlackBoard.UseTarget = null;
				SelectedPC.MyAI.BlackBoard.InteractTarget = null;

			}

			SelectedPC.GetComponent<HumanCharacter>().SendCommand(CharacterCommands.GoToPosition);
			_isMoveKeyDown = false;
		}

	}

	public void OnIssueTaskMouseDown()
	{
		if(IsGamePaused && SelectedPC.MyAI.ControlType == AIControlType.Player)
		{
			HumanCharacter aimedHuman = _aimedObject.GetComponent<HumanCharacter>();
			Character aimedCharacter = _aimedObject.GetComponent<Character>();

			if(Party.SelectedMemberTask == PartyTasks.Grenade)
			{
				Party.SelectedMember.MyAI.BlackBoard.IsGrenadePending = true;
				Party.SelectedMember.MyAI.BlackBoard.PendingGrenadeTarget = Party.SelectedMember.AimPoint;
				InputEventHandler.Instance.TriggerIssueTaskComplete();

				return;
			}



			if(aimedCharacter == null)
			{
				if(Party.SelectedMemberTask == PartyTasks.Default)
				{
					Party.SelectedMemberTask = PartyTasks.GoToGuard;
				}

				if(Party.SelectedMemberTask == PartyTasks.GoToGuard || Party.SelectedMemberTask == PartyTasks.SprintToGuard)
				{
					if(SelectedPC.MyAI.BlackBoard.GuardConfigStage != 1)
					{
						//during paused game, record the aimpoint as temp guard pos

						_tempGuardDest = SelectedPC.AimPoint;
						SelectedPC.MyAI.BlackBoard.GuardConfigStage = 1;
						Party.SetGuardTaskForSelectedMember(0, Vector3.zero, _tempGuardDest, 0);
						Party.RefreshMarkerForMember(SelectedPC);
					}
					else
					{

						Vector3 dir = SelectedPC.AimPoint - _tempGuardDest;
						Party.SetGuardTaskForSelectedMember(0, dir.normalized, _tempGuardDest, dir.magnitude);
						SelectedPC.MyAI.BlackBoard.GuardConfigStage = 2;
						if(Party.SelectedMemberTask == PartyTasks.SprintToGuard)
						{
							SelectedPC.CurrentStance = HumanStances.Sprint;
						}
					

						InputEventHandler.Instance.TriggerIssueTaskComplete();
					}
				}

			}
			else if(aimedHuman != null)
			{
				if(Party.SelectedMemberTask == PartyTasks.Default || Party.SelectedMemberTask == PartyTasks.AttackTarget || Party.SelectedMemberTask == PartyTasks.Follow)
				{
					if(Party.Members.Contains(aimedHuman))
					{
						Party.SetFollowTaskForSelectedMember(aimedHuman);
						SelectedPC.MyAI.BlackBoard.GuardConfigStage = 0;
						InputEventHandler.Instance.TriggerIssueTaskComplete();
					}
					else
					{
						Party.SetAttackTaskForSelectedMember(aimedCharacter);
						InputEventHandler.Instance.TriggerIssueTaskComplete();
					}
				}
			}
			else
			{
				Party.SetAttackTaskForSelectedMember(aimedCharacter);
				InputEventHandler.Instance.TriggerIssueTaskComplete();
			}


		}
	}

	/*
	public void OnIssueTaskMouseUp()
	{
		if(IsGamePaused && SelectedPC.MyAI.ControlType == AIControlType.Player)
		{
			//during paused game, issue a guard task
			//but if aimed object is a party member then issue follow command
			HumanCharacter aimedHuman = _aimedObject.GetComponent<HumanCharacter>();
			if(aimedHuman != null && Party.Members.Contains(aimedHuman))
			{
				Party.SetFollowTaskForSelectedMember(aimedHuman);
				SelectedPC.MyAI.BlackBoard.GuardConfigStage = 0;
			}
			else if(aimedHuman != null)
			{
				SelectedPC.MyAI.BlackBoard.TargetEnemy = aimedHuman;
			}
			else
			{
				Vector3 dir = SelectedPC.AimPoint - _tempGuardDest;
				Party.SetGuardTaskForSelectedMember(0, dir.normalized, _tempGuardDest, dir.magnitude);
				SelectedPC.MyAI.BlackBoard.GuardConfigStage = 2;
			}

			Party.RefreshMarkerForMember(SelectedPC);
		}
	}
	*/

	public void OnClearTask()
	{
		if(IsGamePaused && SelectedPC.MyAI.ControlType == AIControlType.Player)
		{
			Party.ClearTaskForSelectedMember();
		}
	}

	public void OnPlayerMouseSelect()
	{
		if((!IsGamePaused && SelectedPC.MyAI.ControlType == AIControlType.PlayerTeam) || (IsGamePaused && Party.SelectedMemberTask == PartyTasks.Default))
		{
			HumanCharacter aimedHuman = _aimedObject.GetComponent<HumanCharacter>();
			if(aimedHuman != null && Party.Members.Contains(aimedHuman))
			{
				if(aimedHuman == SelectedPC && aimedHuman.MyAI.ControlType == AIControlType.Player)
				{
					Party.ClearActiveMember();
				}
				else
				{
					Party.SetActiveMember(aimedHuman);
				}
			}
		}

	}

	public void OnPlayerMoveLeft()
	{
		if(GameManager.Inst.CameraController.GetCameraMode() == CameraModeEnum.Leader)
		{
			if(_moveDirection == PlayerMoveDirEnum.Stop)
			{
				_moveDirection = PlayerMoveDirEnum.Left;
			}
			else if(_moveDirection2 == PlayerMoveDirEnum.Stop)
			{
				_moveDirection2 = PlayerMoveDirEnum.Left;
			}
		}
	}

	public void OnPlayerMoveRight()
	{
		if(GameManager.Inst.CameraController.GetCameraMode() == CameraModeEnum.Leader)
		{

			if(_moveDirection == PlayerMoveDirEnum.Stop)
			{
				_moveDirection = PlayerMoveDirEnum.Right;
			}
			else if(_moveDirection2 == PlayerMoveDirEnum.Stop)
			{
				_moveDirection2 = PlayerMoveDirEnum.Right;
			}
		}
	}

	public void OnPlayerMoveUp()
	{
		if(GameManager.Inst.CameraController.GetCameraMode() == CameraModeEnum.Leader)
		{

			if(_moveDirection == PlayerMoveDirEnum.Stop)
			{
				_moveDirection = PlayerMoveDirEnum.Up;
			}
			else if(_moveDirection2 == PlayerMoveDirEnum.Stop)
			{
				_moveDirection2 = PlayerMoveDirEnum.Up;
			}
		}
	}

	public void OnPlayerMoveDown()
	{
		if(GameManager.Inst.CameraController.GetCameraMode() == CameraModeEnum.Leader)
		{

			if(_moveDirection == PlayerMoveDirEnum.Stop)
			{
				_moveDirection = PlayerMoveDirEnum.Down;
			}
			else if(_moveDirection2 == PlayerMoveDirEnum.Stop)
			{
				_moveDirection2 = PlayerMoveDirEnum.Down;
			}
		}
	}

	public void OnPlayerStopMoveLeft()
	{
		if(GameManager.Inst.CameraController.GetCameraMode() == CameraModeEnum.Leader)
		{
			if(_moveDirection == PlayerMoveDirEnum.Left)
			{
				_moveDirection = PlayerMoveDirEnum.Stop;
			}
			else if(_moveDirection2 == PlayerMoveDirEnum.Left)
			{
				_moveDirection2 = PlayerMoveDirEnum.Stop;
			}
		}
	}
	
	public void OnPlayerStopMoveRight()
	{
		if(GameManager.Inst.CameraController.GetCameraMode() == CameraModeEnum.Leader)
		{
			if(_moveDirection == PlayerMoveDirEnum.Right)
			{
				_moveDirection = PlayerMoveDirEnum.Stop;
			}
			else if(_moveDirection2 == PlayerMoveDirEnum.Right)
			{
				_moveDirection2 = PlayerMoveDirEnum.Stop;
			}

		}
	}
	
	public void OnPlayerStopMoveUp()
	{
		if(GameManager.Inst.CameraController.GetCameraMode() == CameraModeEnum.Leader)
		{
			if(_moveDirection == PlayerMoveDirEnum.Up)
			{
				_moveDirection = PlayerMoveDirEnum.Stop;
			}
			else if(_moveDirection2 == PlayerMoveDirEnum.Up)
			{
				_moveDirection2 = PlayerMoveDirEnum.Stop;
			}

		}
	}
	
	public void OnPlayerStopMoveDown()
	{
		if(GameManager.Inst.CameraController.GetCameraMode() == CameraModeEnum.Leader)
		{
			if(_moveDirection == PlayerMoveDirEnum.Down)
			{
				_moveDirection = PlayerMoveDirEnum.Stop;
			}
			else if(_moveDirection2 == PlayerMoveDirEnum.Down)
			{
				_moveDirection2 = PlayerMoveDirEnum.Stop;
			}
			
		}
	}

	public void OnPlayerStartSprint()
	{
		SelectedPC.SendCommand(CharacterCommands.Sprint);


	}

	public void OnPlayerStopSprint()
	{
		SelectedPC.SendCommand(CharacterCommands.StopSprint);


	}




	public void OnRMBDown()
	{
		if(!IsGamePaused)
		{
			WeaponAnimType currentWeapon = SelectedPC.GetCurrentAnimWeapon();

			if(_aimedObject != null && Vector3.Distance(SelectedPC.transform.position, _aimedObject.transform.position) < 2 && currentWeapon == WeaponAnimType.Unarmed)
			{
				//we have a nearby aimed object
				Character character = _aimedObject.GetComponent<Character>();
				if(character != null && !character.IsAlive && SelectedPC.UpperBodyState != HumanUpperBodyStates.Aim && SelectedPC.UpperBodyState != HumanUpperBodyStates.HalfAim)
				{
					//now we have a nearby dead body. start disguising
					Debug.Log("Start disguising");
					TimedAction.StartTimedAction(TimedAction.StartDisguiseBody, TimedAction.EndDisguiseBody, TimedAction.CancelDisguiseBody, 3);
				}

			}
			else
			{

				if(currentWeapon == WeaponAnimType.Longgun || currentWeapon == WeaponAnimType.Pistol || currentWeapon == WeaponAnimType.Unarmed)
				{
					if(IsHoldToAim)
					{
						SelectedPC.GetComponent<HumanCharacter>().SendCommand(CharacterCommands.Aim);
					}
					else
					{
						if(SelectedPC.UpperBodyState == HumanUpperBodyStates.Aim || SelectedPC.UpperBodyState == HumanUpperBodyStates.HalfAim)
						{
							TimedAction.CancelTimedAction();
							SelectedPC.GetComponent<HumanCharacter>().SendCommand(CharacterCommands.StopAim);
						}
						else
						{
							SelectedPC.GetComponent<HumanCharacter>().SendCommand(CharacterCommands.Aim);
						}
					}
				}
				else if(currentWeapon == WeaponAnimType.Melee)
				{
					Debug.Log("attempting to strike melee");
					if(SelectedPC.GetMeleeStrikeStage() == 0 && SelectedPC.ActionState != HumanActionStates.Twitch)
					{
						SelectedPC.SendCommand(CharacterCommands.RightAttack);
					}
				}
				else if(currentWeapon == WeaponAnimType.Grenade)
				{
					SelectedPC.SendCommand(CharacterCommands.Throw);
				}
			}
		}


	}

	public void OnRMBUp()
	{
		if(!IsGamePaused)
		{
			WeaponAnimType currentWeapon = SelectedPC.GetCurrentAnimWeapon();
			if(currentWeapon == WeaponAnimType.Tool)
			{
				SelectedPC.SendCommand(CharacterCommands.Cancel);
			}

			if(IsHoldToAim)
			{
				TimedAction.CancelTimedAction();
				SelectedPC.GetComponent<HumanCharacter>().SendCommand(CharacterCommands.StopAim);
			}
		}
	}


	public void OnLRMBDown()
	{
		WeaponAnimType currentWeapon = SelectedPC.GetCurrentAnimWeapon();

		if(currentWeapon == WeaponAnimType.Melee)
		{

			SelectedPC.SendCommand(CharacterCommands.Block);



		}
	}

	public void OnWeaponPullTrigger()
	{
		if(!IsGamePaused)
		{
			if(SelectedPC.MyAI.ControlType != AIControlType.Player)// || (SelectedPC.UpperBodyState != HumanUpperBodyStates.Aim && SelectedPC.UpperBodyState != HumanUpperBodyStates.HalfAim))
			{
				return;
			}

			WeaponAnimType currentWeapon = SelectedPC.GetCurrentAnimWeapon();

			if(currentWeapon == WeaponAnimType.Tool)
			{
				SelectedPC.SendCommand(CharacterCommands.UseTool);
			}
			else if(currentWeapon == WeaponAnimType.Grenade)
			{
				SelectedPC.SendCommand(CharacterCommands.LowThrow);
			}
			else if(currentWeapon == WeaponAnimType.Melee)
			{
				Debug.Log("attempting to strike melee");
				if(SelectedPC.GetMeleeStrikeStage() == 0 && SelectedPC.ActionState != HumanActionStates.Twitch)
				{
					SelectedPC.SendCommand(CharacterCommands.LeftAttack);
				}


			}
			else
			{

				if(SelectedPC.UpperBodyState != HumanUpperBodyStates.Aim)
				{
					TimedAction.StartTimedAction(TimedAction.StartAimThenShoot, TimedAction.EndAimThenShoot, TimedAction.CancelAimThenShoot, 0.5f);
				}
				else
				{
					if(currentWeapon == WeaponAnimType.Pistol || currentWeapon == WeaponAnimType.Longgun)
					{
						SelectedPC.MyAI.WeaponSystem.StartFiringRangedWeapon();
						//SelectedPC.GetComponent<HumanCharacter>().SendCommand(CharacterCommands.PullTrigger);
					}

				}
			}

		}
	}

	public void OnWeaponReleaseTrigger()
	{
		
		WeaponAnimType currentWeapon = SelectedPC.GetCurrentAnimWeapon();
			
		if(currentWeapon == WeaponAnimType.Pistol || currentWeapon == WeaponAnimType.Longgun)
		{
			SelectedPC.MyAI.WeaponSystem.StopFiringRangedWeapon();
			TimedAction.CancelTimedAction();
			//HumanCharacter character = SelectedPC.GetComponent<HumanCharacter>();
			//character.SendCommand(CharacterCommands.ReleaseTrigger);
		}

	}



	public void OnWeaponReload()
	{
		HumanCharacter character = SelectedPC.GetComponent<HumanCharacter>();
		if(character.ActionState != HumanActionStates.Reload)
		{
			character.SendCommand(CharacterCommands.Reload);
		}
		else
		{
			character.SendCommand(CharacterCommands.CancelReload);
		}
	}


	public void OnKick()
	{
		SelectedPC.GetComponent<HumanCharacter>().SendCommand(CharacterCommands.Knock);
	}

	public void OnToggleSneaking()
	{
		if(SelectedPC.CurrentStance == HumanStances.Crouch || SelectedPC.CurrentStance == HumanStances.CrouchRun)
		{
			SelectedPC.SendCommand(CharacterCommands.StopCrouch);
		}
		else
		{
			SelectedPC.SendCommand(CharacterCommands.Crouch);
		}
	}

	public void OnThrow()
	{
		SelectedPC.SendCommand(CharacterCommands.ThrowGrenade);
	}



	public void OnToggleWeapon2()
	{
		WeaponAnimType weaponType = SelectedPC.GetCurrentAnimWeapon();
		if(weaponType != WeaponAnimType.Longgun)
		{
			SelectedPC.SendCommand(CharacterCommands.SwitchWeapon2);
		}
		else
		{
			SelectedPC.SendCommand(CharacterCommands.Unarm);
		}
	}

	public void OnToggleWeapon1()
	{
		WeaponAnimType weaponType = SelectedPC.GetCurrentAnimWeapon();
		if(weaponType != WeaponAnimType.Pistol)
		{
			SelectedPC.SendCommand(CharacterCommands.SwitchWeapon1);
		}
		else
		{
			SelectedPC.SendCommand(CharacterCommands.Unarm);
		}
	}

	public void OnToggleThrown()
	{
		WeaponAnimType weaponType = SelectedPC.GetCurrentAnimWeapon();
		if(weaponType != WeaponAnimType.Grenade)
		{
			SelectedPC.SendCommand(CharacterCommands.SwitchThrown);
		}
		else
		{
			SelectedPC.SendCommand(CharacterCommands.Unarm);
		}
	}

	public void OnToggleTool()
	{
		WeaponAnimType weaponType = SelectedPC.GetCurrentAnimWeapon();
		if(weaponType != WeaponAnimType.Tool)
		{
			SelectedPC.SendCommand(CharacterCommands.SwitchTool);
		}
		else
		{
			SelectedPC.SendCommand(CharacterCommands.Unarm);
		}
	}

	public void OnToggleFlashlight()
	{
		if(SelectedPC.MyReference.Flashlight != null)
		{
			SelectedPC.MyReference.Flashlight.Toggle(!SelectedPC.MyReference.Flashlight.IsOn);
			if(SelectedPC.MyReference.Flashlight.IsOn)
			{
				_playerLight.intensity = 0.33f;
			}
			else
			{
				_playerLight.intensity = 0.1f;
			}
		}
	}

	#endregion
	
	#region Private Methods
	
	private void UpdatePlayerMovement()
	{
		HumanCharacter c = SelectedPC.GetComponent<HumanCharacter>();

		if(_moveDirection != PlayerMoveDirEnum.Stop || _moveDirection2 != PlayerMoveDirEnum.Stop)
		{
			float straightDist = Vector3.Distance(SelectedPC.Destination.Value, SelectedPC.transform.position);
			//Debug.Log("straight dist " + straightDist + " remaining dest " + SelectedPC.MyNavAgent.remainingDistance);
			if(straightDist < 2 && SelectedPC.MyNavAgent.remainingDistance > straightDist + 1)
			{

				_moveDirection = PlayerMoveDirEnum.Stop;
				_moveDirection2 = PlayerMoveDirEnum.Stop;
				SelectedPC.SendCommand(CharacterCommands.Idle);
			}

		}



		if((_moveDirection == PlayerMoveDirEnum.Left && _moveDirection2 == PlayerMoveDirEnum.Stop) ||
			(_moveDirection == PlayerMoveDirEnum.Stop && _moveDirection2 == PlayerMoveDirEnum.Left))
		{
			Vector3 roughDest = SelectedPC.transform.position + Camera.main.transform.right * -1 * 1f;
			MakeStep(roughDest);
		}
		else if((_moveDirection == PlayerMoveDirEnum.Left && _moveDirection2 == PlayerMoveDirEnum.Up) ||
			(_moveDirection == PlayerMoveDirEnum.Up && _moveDirection2 == PlayerMoveDirEnum.Left))
		{
			Vector3 cameraForwardFlat = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
			Vector3 roughDest = SelectedPC.transform.position + (Camera.main.transform.right * -1 + cameraForwardFlat.normalized).normalized * 1f;
			MakeStep(roughDest);
		}
		else if((_moveDirection == PlayerMoveDirEnum.Left && _moveDirection2 == PlayerMoveDirEnum.Down) ||
			(_moveDirection == PlayerMoveDirEnum.Down && _moveDirection2 == PlayerMoveDirEnum.Left))
		{
			Vector3 cameraForwardFlat = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
			Vector3 roughDest = SelectedPC.transform.position + (Camera.main.transform.right * -1 + cameraForwardFlat.normalized * -1).normalized * 1f;
			MakeStep(roughDest);
		}
		else if((_moveDirection == PlayerMoveDirEnum.Right && _moveDirection2 == PlayerMoveDirEnum.Stop) ||
			(_moveDirection == PlayerMoveDirEnum.Stop && _moveDirection2 == PlayerMoveDirEnum.Right))
		{
			Vector3 roughDest = SelectedPC.transform.position + Camera.main.transform.right * 1f;
			//SelectedPC.Destination = SelectedPC.transform.position + SelectedPC.transform.right * 0.1f;
			MakeStep(roughDest);
		}
		else if((_moveDirection == PlayerMoveDirEnum.Right && _moveDirection2 == PlayerMoveDirEnum.Up) ||
			(_moveDirection == PlayerMoveDirEnum.Up && _moveDirection2 == PlayerMoveDirEnum.Right))
		{
			Vector3 cameraForwardFlat = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
			Vector3 roughDest = SelectedPC.transform.position + (Camera.main.transform.right + cameraForwardFlat.normalized).normalized * 1f;
			MakeStep(roughDest);
		}
		else if((_moveDirection == PlayerMoveDirEnum.Right && _moveDirection2 == PlayerMoveDirEnum.Down) ||
			(_moveDirection == PlayerMoveDirEnum.Down && _moveDirection2 == PlayerMoveDirEnum.Right))
		{
			Vector3 cameraForwardFlat = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
			Vector3 roughDest = SelectedPC.transform.position + (Camera.main.transform.right + cameraForwardFlat.normalized * -1).normalized * 1f;
			MakeStep(roughDest);
		}
		else if((_moveDirection == PlayerMoveDirEnum.Up && _moveDirection2 == PlayerMoveDirEnum.Stop) ||
			(_moveDirection == PlayerMoveDirEnum.Stop && _moveDirection2 == PlayerMoveDirEnum.Up))
		{
			Vector3 cameraForwardFlat = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
			Vector3 roughDest = SelectedPC.transform.position + cameraForwardFlat * 2f;
			//SelectedPC.Destination = SelectedPC.transform.position + SelectedPC.transform.forward * 0.1f;
			MakeStep(roughDest);
		}
		else if((_moveDirection == PlayerMoveDirEnum.Down && _moveDirection2 == PlayerMoveDirEnum.Stop) ||
			(_moveDirection == PlayerMoveDirEnum.Stop && _moveDirection2 == PlayerMoveDirEnum.Down))
		{
			Vector3 cameraForwardFlat = new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
			Vector3 roughDest = SelectedPC.transform.position + cameraForwardFlat * -1 * 2f;
			//SelectedPC.Destination = SelectedPC.transform.position + SelectedPC.transform.forward * -1 * 0.1f;
			MakeStep(roughDest);
		}




		if(_moveDirection == PlayerMoveDirEnum.Stop && _moveDirection2 == PlayerMoveDirEnum.Stop)
		{
			if(_isMoveKeyDown)
			{
				//keep updating move direction as the vector from character to mouse cursor's projection point with magnitude of 1
				Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, 
					Input.mousePosition.y, 
					Vector3.Distance(c.transform.position, Camera.main.transform.position)));

				c.Destination = c.transform.position + (mouseWorldPos - c.transform.position).normalized * 2;

				/*
				RaycastHit hitMove;
				Ray rayMove = Camera.main.ScreenPointToRay(Input.mousePosition);
				if(Physics.Raycast(ray, out hit))
				{
					c.Destination = hit.point;// + new Vector3(0, 1, 0);
					//Debug.Log(c.Destination);
				}
				*/

			}

			/*
			Vector3 displacement = c.Destination.Value - c.transform.position;
			if(displacement.magnitude < 0.1f && !_isMoveKeyDown)
			{
				c.SendCommand(CharacterCommands.Idle);
				//c.GetComponent<CharacterController>().Move(Vector3.zero);
			}
			*/
		}



		float aimHeight = 0f;

		if(_aimedObject != null && _aimedObject.tag == "GroundOrFloor")
		{
			if(c.CurrentStance == HumanStances.Crouch || c.CurrentStance == HumanStances.CrouchRun)
			{
				aimHeight = 1f;
			}
			else
			{
				aimHeight = 1.5f;
			}
		}
		else
		{
			aimHeight = 0;
		}



		if(Mathf.Abs(c.AimPoint.y - c.transform.position.y) > 0.5f)
		{
			aimHeight = 0;
		}

		if(c.UpperBodyState == HumanUpperBodyStates.Aim || c.UpperBodyState == HumanUpperBodyStates.HalfAim || c.UpperBodyState == HumanUpperBodyStates.Idle)
		{
			if(c.MyReference.CurrentWeapon != null)
			{
				//c.AimTarget.position = Vector3.Lerp(c.AimTarget.position, c.AimPoint + new Vector3(0, aimHeight, 0), 5 * Time.deltaTime);
				c.AimTargetRoot.position = c.MyReference.CurrentWeapon.transform.position;
				Vector3 cameraDir = (GameManager.Inst.CameraController.transform.position - c.AimPoint).normalized;
				Vector3 aimPoint = c.AimPoint + cameraDir * aimHeight;

				Weapon weapon = c.MyReference.CurrentWeapon.GetComponent<Weapon>();
				if(c.MyAI.BlackBoard.TargetEnemy != null && !weapon.IsScoped)
				{
					aimPoint = c.MyAI.TargetingSystem.GetAimPointOnTarget(c.MyAI.BlackBoard.TargetEnemy);
				}
				else if(weapon.IsScoped)
				{
					aimPoint = c.AimPoint;
				}

				c.MyAI.BlackBoard.AimPoint = aimPoint;
				Vector3 aimDir = aimPoint - c.MyReference.CurrentWeapon.transform.position;

				Quaternion rotation = Quaternion.LookRotation(aimDir);
				c.AimTargetRoot.transform.rotation = Quaternion.Lerp(c.AimTargetRoot.transform.rotation, rotation, Time.deltaTime * 6);

			}
			else
			{
				c.AimTargetRoot.position = c.transform.position + Vector3.up * 1.5f;
				Vector3 cameraDir = (GameManager.Inst.CameraController.transform.position - c.AimPoint).normalized;
				Vector3 aimPoint = c.AimPoint + cameraDir;
				Vector3 aimDir = aimPoint - c.AimTargetRoot.position;//c.transform.position;
				Quaternion rotation = Quaternion.LookRotation(aimDir);
				c.AimTargetRoot.transform.rotation = Quaternion.Lerp(c.AimTargetRoot.transform.rotation, rotation, Time.deltaTime * 6);
			}

			Vector3 lookPosition;
			if(SelectedPC.MyReference.Flashlight.IsOn)
			{
				lookPosition = c.AimTarget.position;
			}
			else
			{
				lookPosition = new Vector3(c.AimTarget.position.x, c.transform.position.y + 1.5f, c.AimTarget.position.z);
			}

			c.LookTarget.position = Vector3.Lerp(c.LookTarget.position, lookPosition, 10 * Time.deltaTime);



		}

	}

	private void MakeStep(Vector3 roughDest)
	{
		Vector3 origin = SelectedPC.transform.position + new Vector3(0, 1, 0);
		RaycastHit hit;
		if(Physics.Raycast(origin, roughDest - origin, out hit))
		{
			if(Vector3.Distance(hit.point, origin) > 2)
			{
				SelectedPC.Destination = roughDest;
			}
			else
			{
				SelectedPC.Destination = hit.point;
			}
			SelectedPC.SendCommand(CharacterCommands.GoToPosition);
		}


	}
	
	#endregion
}
