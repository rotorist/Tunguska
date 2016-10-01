﻿using UnityEngine;
using System.Collections;

public class CursorManager  
{
	public CursorState CurrentState;
	public GameObject ActiveCursor;

	public GameObject CursorDefault;
	public GameObject CursorAim;
	public GameObject CursorHand;
	public GameObject CursorTalk;

	public UILabel ToolTip;
	public float ToolTipDelay;


	private float _toolTipTimer;
	private bool _isShowingToolTip;

	public void Initialize()
	{
		UnityEngine.Cursor.visible = false;

		CursorDefault = GameObject.Find("CursorDefault");
		CursorAim = GameObject.Find("CursorAim");
		CursorHand = GameObject.Find("CursorHand");
		CursorTalk = GameObject.Find("CursorTalk");

		CurrentState = CursorState.Default;
		RefreshCursor();

		ToolTip = GameObject.Find("ToolTip").GetComponent<UILabel>();
		ToolTipDelay = 0.5f;
		NGUITools.SetActive(ToolTip.gameObject, false);

		UIEventHandler.OnCloseWindow += OnResetCursor;
		UIEventHandler.OnLootBody += OnResetCursor;
		UIEventHandler.OnLootChest += OnResetCursor;
		UIEventHandler.OnToggleInventory += OnResetCursor;

	}

	public void SetCursorState(CursorState state)
	{
		if(CurrentState != state)
		{
			CurrentState = state;
		}

		RefreshCursor();
	}

	public void OnHitMarkerShow()
	{
		CursorAim.GetComponent<AimCursor>().SetCenterCursorAlpha(1);
	}

	public void ShowToolTip(string text)
	{
		ToolTip.text = text;
		UISprite background = ToolTip.transform.Find("ToolTipBackground").GetComponent<UISprite>();
		background.MakePixelPerfect();
		background.width = ToolTip.width + 6;
		background.height = ToolTip.height + 6;
		background.transform.localPosition = new Vector3(-3, -3, 0);
		_isShowingToolTip = true;
	}

	public void HideToolTip()
	{
		NGUITools.SetActive(ToolTip.gameObject, false);
		_isShowingToolTip = false;
		_toolTipTimer = 0;
	}

	public void OnResetCursor()
	{
		HideToolTip();
		SetCursorState(CursorState.Default);
	}

	public void PerFrameUpdate()
	{
		//run delay timer for tooltip
		if(_isShowingToolTip)
		{
			if(_toolTipTimer < ToolTipDelay)
			{
				_toolTipTimer += Time.unscaledDeltaTime;
			}
			else
			{
				if(!NGUITools.GetActive(ToolTip))
				{
					NGUITools.SetActive(ToolTip.gameObject, true);
				}
				ToolTip.transform.localPosition = ActiveCursor.transform.localPosition;
				_toolTipTimer = 0;
			}
		}


		//Debug.Log(Input.mousePosition);
		if(GameManager.Inst.PlayerControl.IsGamePaused || InputEventHandler.Instance.State == UserInputState.WindowsOpen)
		{
			return;
		}

		UnityEngine.Cursor.visible = false;
		if(CurrentState == CursorState.Aim)
		{
			AimCursor aimCursor = ActiveCursor.GetComponent<AimCursor>();
			if(aimCursor != null)
			{
				float climb = GameManager.Inst.PlayerControl.SelectedPC.AimTarget.localPosition.y;
				float amount = Mathf.Clamp(climb, 0, 1);
				float baseAmount = 5;
				if(GameManager.Inst.PlayerControl.SelectedPC.IsHipAiming || GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState != HumanUpperBodyStates.Aim)
				{
					baseAmount = 15;
				}
				amount = amount * 100 + baseAmount + 20 * GameManager.Inst.PlayerControl.SelectedPC.MyAI.WeaponSystem.GetTurnMoveScatter();
				aimCursor.SetExpansion(amount);
			}
		}


		float centerAlpha = CursorAim.GetComponent<AimCursor>().AimCursorCenter.alpha;
		CursorAim.GetComponent<AimCursor>().SetCenterCursorAlpha(Mathf.Lerp(centerAlpha, 0, Time.deltaTime * 3));
		//update cursor state based on what player is doing

		GameObject aimedObject = GameManager.Inst.PlayerControl.GetAimedObject();

		if(GameManager.Inst.PlayerControl.SelectedPC.MyAI.ControlType == AIControlType.Player)
		{
			
			if(GameManager.Inst.PlayerControl.SelectedPC.GetCurrentAnimWeapon() != WeaponAnimType.Unarmed)//GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState == HumanUpperBodyStates.Aim || GameManager.Inst.PlayerControl.SelectedPC.UpperBodyState == HumanUpperBodyStates.HalfAim)
			{
				
				SetCursorState(CursorState.Aim);


				/*
				//now check if aimed object is enemy
				Character aimedChar = GameManager.Inst.PlayerControl.GetAimedObject().GetComponent<Character>();
				if(aimedChar != null && aimedChar.Faction != GameManager.Inst.PlayerControl.SelectedPC.Faction)
				{
					//mark cursor as red
					CursorAim.color = new Color(1, 0, 0);
				}
				*/
			}
			else if(aimedObject != null && aimedObject.GetComponent<PickupItem>() != null)
			{
				ShowToolTip(aimedObject.GetComponent<PickupItem>().Item.Name);
				SetCursorState(CursorState.Hand);
			}
			else if(aimedObject != null)
			{
				Character aimedCharacter = aimedObject.GetComponent<Character>();
				if(aimedCharacter != null && aimedCharacter.MyStatus.Health <= 0)
				{
					SetCursorState(CursorState.Hand);
				}
				else if(aimedCharacter != null && aimedCharacter.MyStatus.Health > 0 && aimedCharacter != GameManager.Inst.PlayerControl.SelectedPC)
				{
					SetCursorState(CursorState.Talk);
				}
				else if(aimedObject.tag == "Chest" && aimedObject.GetComponent<Chest>() != null)
				{
					SetCursorState(CursorState.Hand);
				}
				else
				{
					SetCursorState(CursorState.Default);
				}

				HideToolTip();

			}
			else
			{
				SetCursorState(CursorState.Default);
				HideToolTip();
			}
		}
		else
		{
			SetCursorState(CursorState.Default);
			HideToolTip();
		}


	}

	private void RefreshCursor()
	{
		switch(CurrentState)
		{
		case CursorState.Default:
			CursorAim.transform.position = new Vector3(0, 1000, 0);
			CursorHand.transform.position = new Vector3(0, 1000, 0);;
			NGUITools.SetActive(CursorAim.gameObject, false);
			NGUITools.SetActive(CursorHand.gameObject, false);
			NGUITools.SetActive(CursorTalk.gameObject, false);
			NGUITools.SetActive(CursorDefault.gameObject, true);

			ActiveCursor = CursorDefault;
			break;
		case CursorState.Aim:
			CursorDefault.transform.position = new Vector3(0, 1000, 0);;
			CursorHand.transform.position = new Vector3(0, 1000, 0);;
			NGUITools.SetActive(CursorDefault.gameObject, false);
			NGUITools.SetActive(CursorHand.gameObject, false);
			NGUITools.SetActive(CursorTalk.gameObject, false);
			NGUITools.SetActive(CursorAim.gameObject, true);

			ActiveCursor = CursorAim;
			break;
		case CursorState.Hand:
			CursorAim.transform.position = new Vector3(0, 1000, 0);;
			CursorDefault.transform.position = new Vector3(0, 1000, 0);;
			NGUITools.SetActive(CursorDefault.gameObject, false);
			NGUITools.SetActive(CursorAim.gameObject, false);
			NGUITools.SetActive(CursorTalk.gameObject, false);
			NGUITools.SetActive(CursorHand.gameObject, true);

			ActiveCursor = CursorHand;
			break;
		case CursorState.Talk:
			CursorAim.transform.position = new Vector3(0, 1000, 0);;
			CursorDefault.transform.position = new Vector3(0, 1000, 0);;
			NGUITools.SetActive(CursorDefault.gameObject, false);
			NGUITools.SetActive(CursorAim.gameObject, false);
			NGUITools.SetActive(CursorHand.gameObject, false);
			NGUITools.SetActive(CursorTalk.gameObject, true);


			ActiveCursor = CursorTalk;
			break;
		}

	}
}


public enum CursorState
{
	Default,
	Aim,
	Observe,
	Hand,
	Talk,
}