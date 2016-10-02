﻿using UnityEngine;
using System.Collections;

public class InputEventHandler
{

	#region Singleton 
	private static InputEventHandler _instance;
	public static InputEventHandler Instance	
	{
		get 
		{
			if (_instance == null)
				_instance = new InputEventHandler();
			
			return _instance;
		}
	}
	#endregion
	
	#region Constructor
	public InputEventHandler()
	{
		Initialize();
	}
	
	#endregion

	#region Public Events
	public delegate void GenericDelegate();
	public delegate void SelectMemberDelegate(HumanCharacter prev);
	public delegate void KeyEventDelegate();
	public delegate void MouseEventDelegate(float movement);
	
	
	public static event KeyEventDelegate OnPlayerMove;
	public static event KeyEventDelegate OnPlayerStopMove;

	public static event KeyEventDelegate OnWeaponPullTrigger;
	public static event KeyEventDelegate OnWeaponReleaseTrigger;
	public static event KeyEventDelegate OnRMBDown;
	public static event KeyEventDelegate OnRMBUp;
	public static event KeyEventDelegate OnKick;
	public static event KeyEventDelegate OnLRMBDown;

	public static event KeyEventDelegate OnCameraSwitchMode;

	public static event MouseEventDelegate OnCameraRotateLeft;
	public static event MouseEventDelegate OnCameraRotateRight;
	public static event KeyEventDelegate OnCameraStopRotate;

	public static event MouseEventDelegate OnCameraZoomIn;
	public static event MouseEventDelegate OnCameraZoomOut;

	public static event KeyEventDelegate OnCameraPanLeft;
	public static event KeyEventDelegate OnCameraPanRight;
	public static event KeyEventDelegate OnCameraPanUp;
	public static event KeyEventDelegate OnCameraPanDown;

	public static event KeyEventDelegate OnCameraLookAhead;
	public static event KeyEventDelegate OnCameraStopLookAhead;

	public static event KeyEventDelegate OnPlayerMoveLeft;
	public static event KeyEventDelegate OnPlayerMoveRight;
	public static event KeyEventDelegate OnPlayerMoveUp;
	public static event KeyEventDelegate OnPlayerMoveDown;
	public static event KeyEventDelegate OnPlayerStopMoveLeft;
	public static event KeyEventDelegate OnPlayerStopMoveRight;
	public static event KeyEventDelegate OnPlayerStopMoveUp;
	public static event KeyEventDelegate OnPlayerStopMoveDown;

	public static event KeyEventDelegate OnPlayerToggleSneak;

	public static event KeyEventDelegate OnPlayerStartSprint;
	public static event KeyEventDelegate OnPlayerStopSprint;


	public static event KeyEventDelegate OnPlayerSwitchWeapon2;
	public static event KeyEventDelegate OnPlayerSwitchWeapon1;
	public static event KeyEventDelegate OnPlayerSwitchThrown;
	public static event KeyEventDelegate OnPlayerSwitchTool;

	public static event KeyEventDelegate OnPlayerReload;
	public static event KeyEventDelegate OnToggleFlashlight;
	public static event KeyEventDelegate OnPlayerThrow;

	public static event KeyEventDelegate OnGameTogglePause;
	public static event KeyEventDelegate OnSelectMember1;
	public static event KeyEventDelegate OnSelectMember2;
	public static event KeyEventDelegate OnSelectMember3;
	public static event KeyEventDelegate OnSelectMember4;
	public static event KeyEventDelegate OnIssueTaskLMB;
	public static event KeyEventDelegate OnIssueTaskRMB;
	public static event KeyEventDelegate OnMouseSelect;
	public static event KeyEventDelegate OnClearTask;



	public static event GenericDelegate OnIssueTaskComplete;
	public static event GenericDelegate OnGamePause;
	public static event GenericDelegate OnGameUnpause;
	public static event SelectMemberDelegate OnSelectActiveMember;

	public static event MouseEventDelegate OnPopupMouseWheel;


	#endregion

	#region Public fields
	public UserInputState State;

	#endregion

	#region Public Methods

	public void PerFrameUpdate()
	{
		if(State == UserInputState.Normal)
		{
			HandleNormalModeInput();
		}
		else if(State == UserInputState.WindowsOpen)
		{
			HandleWindowModeInput();
		}
		else if(State == UserInputState.PopupOpen)
		{
			HandlePopupModeInput();
		}
		else if(State == UserInputState.Dialogue)
		{

		}
	}


	public void TriggerIssueTaskComplete()
	{
		if(OnIssueTaskComplete != null)
		{
			OnIssueTaskComplete();
		}
	}

	public void TriggerOnGamePause()
	{
		if(OnGamePause != null)
		{
			OnGamePause();
		}
	}

	public void TriggerOnGameUnpause()
	{
		if(OnGameUnpause != null)
		{
			OnGameUnpause();
		}
	}

	public void TriggerOnSelectActiveMember(HumanCharacter prev)
	{
		if(OnSelectActiveMember != null)
		{
			OnSelectActiveMember(prev);
		}
	}



	public void HandleWindowModeInput()
	{
		if(Input.GetKeyDown(KeyCode.I))
		{
			UIEventHandler.Instance.TriggerToggleInventory();
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			UIEventHandler.Instance.TriggerCloseWindow();
		}
	}

	public void HandlePopupModeInput()
	{
		float wheelInput = Input.GetAxis("Mouse ScrollWheel");
		if(wheelInput != 0)
		{
			if(OnPopupMouseWheel != null)
			{
				OnPopupMouseWheel(wheelInput);
			}
		}
	}

	public void HandleNormalModeInput()
	{
		#region Testing

			

		#endregion


		#region Camera Controls

		float wheelInput = Input.GetAxis("Mouse ScrollWheel");
		if(wheelInput > 0)
		{
			if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				if(OnCameraZoomIn != null)
				{
					OnCameraZoomIn(wheelInput);
				}
			}
			else
			{
				if(OnCameraRotateLeft != null)
				{
					OnCameraRotateLeft(wheelInput);
				}
			}
		}
		else if(wheelInput < 0)
		{
			if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				if(OnCameraZoomOut != null)
				{
					OnCameraZoomOut(wheelInput);
				}
			}
			else
			{
				if(OnCameraRotateRight != null)
				{
					OnCameraRotateRight(wheelInput);
				}
			}
		}


		if(Input.GetKeyDown(KeyCode.Q))
		{
			if(OnCameraRotateLeft != null)
			{
				//OnCameraRotateLeft();
			}
		}

		if(Input.GetKeyDown(KeyCode.E))
		{
			if(OnCameraRotateRight != null)
			{
				//OnCameraRotateRight();
			}
		}

		if((Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q)) &&
		   !(Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Q)))
		{
			if(OnCameraStopRotate != null)
			{
				OnCameraStopRotate();
			}
		}




		if(Input.GetKey(KeyCode.A))
		{
			if(OnCameraPanLeft != null)
			{
				OnCameraPanLeft();
			}
		}

		if(Input.GetKey(KeyCode.D))
		{
			if(OnCameraPanRight != null)
			{
				OnCameraPanRight();
			}
		}

		if(Input.GetKey(KeyCode.W))
		{
			if(OnCameraPanUp != null)
			{
				OnCameraPanUp();
			}
		}

		if(Input.GetKey(KeyCode.S))
		{
			if(OnCameraPanDown != null)
			{
				OnCameraPanDown();
			}
		}


		if(Input.GetKeyDown(KeyCode.LeftAlt))
		{
			if(OnCameraLookAhead != null)
			{
				OnCameraLookAhead();
			}
		}

		if(Input.GetKeyUp(KeyCode.LeftAlt))
		{
			if(OnCameraStopLookAhead != null)
			{
				OnCameraStopLookAhead();
			}
		}

		#endregion

		#region Character Control

		if(Input.GetKeyDown(KeyCode.Space))
		{
			if(OnGameTogglePause != null)
			{
				OnGameTogglePause();
			}
		}

		if(Input.GetKeyDown(KeyCode.Backspace))
		{
			if(OnClearTask != null)
			{
				OnClearTask();
			}
		}

		if(Input.GetKeyDown(KeyCode.F1))
		{
			if(OnSelectMember1 != null)
			{
				OnSelectMember1();
			}
		}

		if(Input.GetKeyDown(KeyCode.F2))
		{
			if(OnSelectMember2 != null)
			{
				OnSelectMember2();
			}
		}

		if(Input.GetKeyDown(KeyCode.F3))
		{
			if(OnSelectMember3 != null)
			{
				OnSelectMember3();
			}
		}

		if(Input.GetKeyDown(KeyCode.F4))
		{
			if(OnSelectMember4 != null)
			{
				OnSelectMember4();
			}
		}



		#region Mouse Buttons
		if(!GameManager.Inst.PlayerControl.IsGamePaused)
		{
			if(Input.GetKeyDown(KeyCode.Mouse2))
			{
				if(OnLRMBDown != null)
				{
					OnLRMBDown();
					return;
				}
			}

			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				/*
				if(!GameManager.Inst.UIManager.IsCursorInHUDRegion())
				{
					if(OnPlayerMove != null)
					{
						OnPlayerMove();
					}
				}
				*/
				if(GameManager.Inst.PlayerControl.SelectedPC.GetCurrentAnimWeapon() == WeaponAnimType.Unarmed)
				{
					if(OnMouseSelect != null)
					{
						OnMouseSelect();
					}

					if(OnPlayerStopMove != null)
					{
						OnPlayerStopMove();
					}
				}
				else
				{	
					if(OnWeaponPullTrigger != null)
					{
						OnWeaponPullTrigger();
					}
				}
			}

			/*
			if(Input.GetKeyDown(KeyCode.E))
			{
				if(OnMouseSelect != null)
				{
					OnMouseSelect();
				}

				if(OnPlayerStopMove != null)
				{
					OnPlayerStopMove();
				}
			}
			*/

			if(Input.GetKeyUp(KeyCode.Mouse0))
			{



				if(OnWeaponReleaseTrigger != null)
				{
					OnWeaponReleaseTrigger();
				}
			}

			if(Input.GetKeyDown(KeyCode.Mouse1))
			{
				if(OnRMBDown != null)
				{
					OnRMBDown();
				}
			}

			if(Input.GetKeyUp(KeyCode.Mouse1))
			{
				if(OnRMBUp != null)
				{
					OnRMBUp();
				}
			}
		}
		else
		{

			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				if(!GameManager.Inst.UIManager.IsCursorInHUDRegion())
				{
					if(OnMouseSelect != null)
					{
						OnMouseSelect();
					}

					if(OnIssueTaskLMB != null)
					{
						OnIssueTaskLMB();
					}


				}


			}

			if(Input.GetKeyDown(KeyCode.Mouse1))
			{
				if(OnIssueTaskRMB != null)
				{
					OnIssueTaskRMB();
				}


			}
		}



		#endregion



		if(GameManager.Inst.PlayerControl.SelectedPC.MyAI.ControlType == AIControlType.Player)
		{
			
			if(Input.GetKeyDown(KeyCode.A))
			{
				if(OnPlayerMoveLeft != null)
				{
					OnPlayerMoveLeft();
				}
			}
			
			if(Input.GetKeyDown(KeyCode.D))
			{
				if(OnPlayerMoveRight != null)
				{
					OnPlayerMoveRight();
				}
			}
			
			if(Input.GetKeyDown(KeyCode.W))
			{
				if(OnPlayerMoveUp != null)
				{
					OnPlayerMoveUp();
				}
			}
			
			if(Input.GetKeyDown(KeyCode.S))
			{
				if(OnPlayerMoveDown != null)
				{
					OnPlayerMoveDown();
				}
			}


			//


			if(!Input.GetKey(KeyCode.A))
			{
				if(OnPlayerStopMoveLeft != null)
				{
					OnPlayerStopMoveLeft();
				}
			}
			
			if(!Input.GetKey(KeyCode.D))
			{
				if(OnPlayerStopMoveRight != null)
				{
					OnPlayerStopMoveRight();
				}
			}
			
			if(!Input.GetKey(KeyCode.W))
			{
				if(OnPlayerStopMoveUp != null)
				{
					OnPlayerStopMoveUp();
				}
			}
			
			if(!Input.GetKey(KeyCode.S))
			{
				if(OnPlayerStopMoveDown != null)
				{
					OnPlayerStopMoveDown();
				}
			}

			if(Input.GetKeyDown(KeyCode.LeftShift))
			{
				if(OnPlayerStartSprint != null)
				{
					OnPlayerStartSprint();
				}
			}

			if(Input.GetKeyUp(KeyCode.LeftShift))
			{
				if(OnPlayerStopSprint != null)
				{
					OnPlayerStopSprint();
				}
			}






			
		

			if(Input.GetKeyDown(KeyCode.F))
			{
				if(OnKick != null)
				{
					OnKick();
				}
			}



			if(Input.GetKeyDown(KeyCode.C))
			{
				if(OnPlayerToggleSneak != null)
				{
					OnPlayerToggleSneak();
				}
			}


			#endregion

			#region Use Prop Events

			if(Input.GetKeyDown(KeyCode.Alpha2))
			{
				if(OnPlayerSwitchWeapon2 != null)
				{
					OnPlayerSwitchWeapon2();
				}
			}

			if(Input.GetKeyDown(KeyCode.Alpha1))
			{
				if(OnPlayerSwitchWeapon1 != null)
				{
					OnPlayerSwitchWeapon1();
				}
			}

			if(Input.GetKeyDown(KeyCode.Alpha3))
			{
				if(OnPlayerSwitchThrown != null)
				{
					OnPlayerSwitchThrown();
				}
			}

			if(Input.GetKeyDown(KeyCode.Alpha4))
			{
				if(OnPlayerSwitchTool != null)
				{
					OnPlayerSwitchTool();
				}
			}

			if(Input.GetKeyDown(KeyCode.R))
			{
				if(OnPlayerReload != null)
				{
					OnPlayerReload();
				}
			}

			if(Input.GetKeyDown(KeyCode.G))
			{
				if(OnPlayerThrow != null)
				{
					OnPlayerThrow();
				}
			}

			if(Input.GetKeyDown(KeyCode.L))
			{
				if(OnToggleFlashlight != null)
				{
					OnToggleFlashlight();
				}
			}

			if(Input.GetKeyDown(KeyCode.I))
			{
				UIEventHandler.Instance.TriggerToggleInventory();
			}

			if(Input.GetKeyDown(KeyCode.X))
			{
				UIEventHandler.Instance.TriggerDialogue();
			}


			#endregion
		}
	}

	#endregion

	#region Private Methods

	private void Initialize()
	{
		State = UserInputState.Normal;
	}

	#endregion

}

public enum UserInputState
{
	Normal,
	WindowsOpen,
	PopupOpen,
	Dialogue,
}