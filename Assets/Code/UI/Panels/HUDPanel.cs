﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HUDPanel : PanelBase 
{
	//public List<HUDPartyMember> MembersBrief;
	//public List<GameObject> MemberSlots;
	//public List<UISprite> CommandButtons;
	public UILabel Console;
	public UISprite Aperture;

	public struct HUDPartyMember
	{
		public HumanCharacter Member;
		public UISprite Picture;
		public UISprite HealthBar;
		public GameObject Slot;
	}

	public override void Initialize ()
	{
		/*
		RebuildPartySlots();

		InputEventHandler.OnIssueTaskComplete -= OnCommandComplete;
		InputEventHandler.OnIssueTaskComplete += OnCommandComplete;
		InputEventHandler.OnGamePause -= UpdateButtonState;
		InputEventHandler.OnGamePause += UpdateButtonState;
		InputEventHandler.OnGameUnpause -= UpdateButtonState;
		InputEventHandler.OnGameUnpause += UpdateButtonState;
		InputEventHandler.OnSelectActiveMember -= OnSelectActiveMember;
		InputEventHandler.OnSelectActiveMember += OnSelectActiveMember;


		UIEventHandler.OnOpenWindow -= UpdateButtonState;
		UIEventHandler.OnOpenWindow += UpdateButtonState;
		UIEventHandler.OnCloseWindow -= UpdateButtonState;
		UIEventHandler.OnCloseWindow += UpdateButtonState;

		UpdateButtonState();
		*/

	}

	public override void PerFrameUpdate ()
	{
		UpdateHealthBars();
		//RefreshCommandButtons();
		UpdateAperture();
	}

	public void SetConsoleText(string text)
	{
		Console.text = text;
	}

	/*
	public void UpdateButtonState()
	{
		if(GameManager.Inst.PlayerControl.IsGamePaused && InputEventHandler.Instance.State == UserInputState.Normal && 
			GameManager.Inst.PlayerControl.SelectedPC.MyAI.ControlType == AIControlType.Player)
		{

			SetButtonState(-1);
		}
		else
		{
			SetButtonState(-1);
		}
	}



	public void OnSelectActiveMember(HumanCharacter prev)
	{
		UpdateButtonState();
	}


	public void OnMemberSlotClick()
	{
		//if player is aiming then don't do anything
		if(GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState == HumanUpperBodyStates.Aim 
			|| GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState == HumanUpperBodyStates.HalfAim)
		{
			return;
		}


		foreach(HUDPartyMember m in MembersBrief)
		{
			if(m.Slot == UIButton.current.gameObject)
			{
				GameManager.Inst.PlayerControl.Party.SetActiveMember(m.Member);
			}
		}
	}

	public void OnWindowPanelOpen()
	{
		SetButtonState(-1);
	}

	public void OnWindowPanelClose()
	{
		if(GameManager.Inst.PlayerControl.SelectedPC.MyAI.ControlType == AIControlType.Player && GameManager.Inst.PlayerControl.IsGamePaused)
		{
			SetButtonState(-1);
		}
		else
		{
			SetButtonState(-1);
		}
	}

	
	public void OnCommandSelectGoto()
	{
		GameManager.Inst.PlayerControl.Party.ClearTaskForSelectedMember();
		GameManager.Inst.CursorManager.SetCursorState(CursorState.Aim);
		InputEventHandler.OnIssueTaskLMB -= GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		InputEventHandler.OnIssueTaskLMB += GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		GameManager.Inst.PlayerControl.Party.SelectedMemberTask = PartyTasks.GoToGuard;
		//disable all other buttons except cancel
		SetButtonState(0);
	}

	public void OnCommandSelectSprintTo()
	{
		GameManager.Inst.PlayerControl.Party.ClearTaskForSelectedMember();
		GameManager.Inst.CursorManager.SetCursorState(CursorState.Aim);
		InputEventHandler.OnIssueTaskLMB -= GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		InputEventHandler.OnIssueTaskLMB += GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		GameManager.Inst.PlayerControl.Party.SelectedMemberTask = PartyTasks.SprintToGuard;
		SetButtonState(1);
	}

	public void OnCommandSelectGrenade()
	{
		GameManager.Inst.PlayerControl.Party.ClearTaskForSelectedMember();
		GameManager.Inst.CursorManager.SetCursorState(CursorState.Aim);
		InputEventHandler.OnIssueTaskLMB -= GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		InputEventHandler.OnIssueTaskLMB += GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		GameManager.Inst.PlayerControl.Party.SelectedMemberTask = PartyTasks.Grenade;
		SetButtonState(4);
	}

	public void OnCommandSelectAttack()
	{
		GameManager.Inst.PlayerControl.Party.ClearTaskForSelectedMember();
		GameManager.Inst.CursorManager.SetCursorState(CursorState.Aim);
		InputEventHandler.OnIssueTaskLMB -= GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		InputEventHandler.OnIssueTaskLMB += GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		GameManager.Inst.PlayerControl.Party.SelectedMemberTask = PartyTasks.AttackTarget;
		SetButtonState(3);
	}

	public void OnCommandSelectFollow()
	{
		GameManager.Inst.PlayerControl.Party.ClearTaskForSelectedMember();
		GameManager.Inst.CursorManager.SetCursorState(CursorState.Aim);
		InputEventHandler.OnIssueTaskLMB -= GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		InputEventHandler.OnIssueTaskLMB += GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		GameManager.Inst.PlayerControl.Party.SelectedMemberTask = PartyTasks.Follow;
		SetButtonState(2);
	}

	public void OnCommandSelectCancel()
	{
		GameManager.Inst.CursorManager.SetCursorState(CursorState.Default);
		GameManager.Inst.PlayerControl.Party.SelectedMemberTask = PartyTasks.Default;
		GameManager.Inst.PlayerControl.Party.ClearTaskForSelectedMember();
		SetButtonState(8);
	}

	public void OnCommandSelectToggleCrouch()
	{
		if(GameManager.Inst.PlayerControl.SelectedPC.CurrentStance == HumanStances.Crouch)
		{
			GameManager.Inst.PlayerControl.SelectedPC.SendCommand(CharacterCommands.StopCrouch);
			CommandButtons[6].spriteName = "CommandStand";
			CommandButtons[6].GetComponent<UIButton>().normalSprite = "CommandStand";
		}
		else
		{
			GameManager.Inst.PlayerControl.SelectedPC.SendCommand(CharacterCommands.Crouch);
			CommandButtons[6].spriteName = "CommandCrouch";
			CommandButtons[6].GetComponent<UIButton>().normalSprite = "CommandCrouch";
		}
	}

	public void OnCommandSelectToggleHoldFire()
	{
		if(GameManager.Inst.PlayerControl.SelectedPC.MyAI.BlackBoard.GuardLevel > 0)
		{
			GameManager.Inst.PlayerControl.SelectedPC.MyAI.BlackBoard.GuardLevel = 0;
			CommandButtons[7].spriteName = "CommandHoldFire";
			CommandButtons[7].GetComponent<UIButton>().normalSprite = "CommandHoldFire";
		}
		else
		{
			GameManager.Inst.PlayerControl.SelectedPC.MyAI.BlackBoard.GuardLevel = 2;
			CommandButtons[7].spriteName = "CommandFireAtWill";
			CommandButtons[7].GetComponent<UIButton>().normalSprite = "CommandFireAtWill";
		}
	}

	public void OnCommandComplete()
	{
		GameManager.Inst.CursorManager.SetCursorState(CursorState.Default);
		InputEventHandler.OnIssueTaskRMB -= GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		InputEventHandler.OnIssueTaskRMB += GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		InputEventHandler.OnIssueTaskLMB -= GameManager.Inst.PlayerControl.OnIssueTaskMouseDown;
		GameManager.Inst.PlayerControl.Party.SelectedMemberTask = PartyTasks.Default;

		SetButtonState(8);
	}
	*/


	private void UpdateAperture()
	{
		GameObject o = GameManager.Inst.PlayerControl.SelectedPC.MyReference.CurrentWeapon;
		if(o == null || InputEventHandler.Instance.State != UserInputState.Normal)
		{
			NGUITools.SetActive(Aperture.gameObject, false);
			return;
		}

		Weapon weapon = o.GetComponent<Weapon>();

		if(weapon != null && weapon.IsScoped && GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState == HumanUpperBodyStates.Aim)
		{

			NGUITools.SetActive(Aperture.gameObject, true);


			Aperture.transform.position = GameManager.Inst.CursorManager.ActiveCursor.transform.position;
		}
		else
		{
			NGUITools.SetActive(Aperture.gameObject, false);
		}
	}



	private void UpdateHealthBars()
	{
		/*
		foreach(HUDPartyMember m in MembersBrief)
		{
			float widthFloat = 100f * (m.Member.MyStatus.Health / m.Member.MyStatus.MaxHealth);
			m.HealthBar.width = Mathf.CeilToInt(widthFloat);
			if(widthFloat <= 0)
			{
				m.HealthBar.alpha = 0;
			}

			if(GameManager.Inst.PlayerControl.SelectedPC == m.Member)
			{
				m.Picture.color = new Color(1, 1, 1);
			}
			else
			{
				m.Picture.color = new Color(0.4f, 0.4f, 0.4f);
			}
		}
		*/
	}

	/*
	private void RebuildPartySlots()
	{
		//load all the members from player party
		int count = GameManager.Inst.PlayerControl.Party.Members.Count;
		MembersBrief = new List<HUDPartyMember>();
		int i = 0;
		foreach(HumanCharacter member in GameManager.Inst.PlayerControl.Party.Members)
		{
			GameObject slot = MemberSlots[i];
			GameObject o = GameObject.Instantiate(Resources.Load("CharPic" + member.CharacterID)) as GameObject;
			o.transform.parent = slot.transform;
			o.transform.localPosition = Vector3.zero;
			UISprite pic = o.GetComponent<UISprite>();
			pic.MakePixelPerfect();
			pic.width = 100;
			pic.height = 100;


			o = GameObject.Instantiate(Resources.Load("HealthBar")) as GameObject;
			o.transform.parent = slot.transform;
			o.transform.localPosition = new Vector3(-50, -50, 0);
			UISprite bar = o.GetComponent<UISprite>();
			bar.MakePixelPerfect();
			bar.width = 100;
			bar.height = 15;

			HUDPartyMember m = new HUDPartyMember();
			m.Member = member;
			m.Slot = slot;
			m.Picture = pic;
			m.HealthBar = bar;

			MembersBrief.Add(m);

			i++;
		}
	}

	private void RefreshCommandButtons()
	{
		if(GameManager.Inst.PlayerControl.SelectedPC.CurrentStance == HumanStances.Crouch)
		{
			CommandButtons[6].spriteName = "CommandCrouch";
			CommandButtons[6].GetComponent<UIButton>().normalSprite = "CommandCrouch";
		}
		else
		{
			CommandButtons[6].spriteName = "CommandStand";
			CommandButtons[6].GetComponent<UIButton>().normalSprite = "CommandStand";
		}

		if(GameManager.Inst.PlayerControl.SelectedPC.MyAI.BlackBoard.GuardLevel > 0)
		{
			CommandButtons[7].spriteName = "CommandFireAtWill";
			CommandButtons[7].GetComponent<UIButton>().normalSprite = "CommandFireAtWill";

		}
		else
		{
			CommandButtons[7].spriteName = "CommandHoldFire";
			CommandButtons[7].GetComponent<UIButton>().normalSprite = "CommandHoldFire";
		}
	}

	private void SetButtonState(int buttonEnabled)
	{
		if(buttonEnabled < 0)
		{
			//disable and hide all buttons
			foreach(UISprite button in CommandButtons)
			{
				
				button.color = new Color(255, 255, 255);
				button.alpha = 1;
				button.GetComponent<UIButton>().enabled = false;
				NGUITools.SetActive(button.gameObject, false);
			}

			NGUITools.SetActive(Console.gameObject, true);
		}
		else if(buttonEnabled > 7)
		{
			//enable and show all buttons
			foreach(UISprite button in CommandButtons)
			{
				button.alpha = 1;
				button.color = new Color(255, 255, 255);
				button.GetComponent<UIButton>().enabled = true;
				NGUITools.SetActive(button.gameObject, true);
			}

			NGUITools.SetActive(Console.gameObject, false);
		}
		else
		{
			foreach(UISprite button in CommandButtons)
			{
				if(button != CommandButtons[buttonEnabled] && button != CommandButtons[5]) //let 5 which is cancel button show
				{
					button.alpha = 1;
					button.color = new Color(0, 0, 0);
					button.GetComponent<UIButton>().enabled = false;
				}
				else
				{
					button.alpha = 1;
					button.color = new Color(255, 255, 255);
					button.GetComponent<UIButton>().enabled = true;
				}
			}
		}
	}

	*/
}

