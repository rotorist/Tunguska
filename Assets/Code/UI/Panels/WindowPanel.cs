﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.ImageEffects;

public class WindowPanel : PanelBase
{
	public InventoryPanel InventoryPanel;
	public BodySlotPanel BodySlotPanel;
	public BodyLootPanel BodyLootPanel;
	public ChestLootPanel ChestLootPanel;
	public SplitItemPanel SplitItemPanel;

	public UIPanel SelectedItemPanel;

	public UISprite BackgroundNormal;
	public UISprite BackgroundTrifold;

	public GameObject WindowAnchor;


	private List<PanelBase> _panels;

	public override void Initialize ()
	{
		_panels = new List<PanelBase>();

		InventoryPanel.Initialize();
		BodySlotPanel.Initialize();
		BodyLootPanel.Initialize();
		ChestLootPanel.Initialize();
		SplitItemPanel.Initialize();



		_panels.Add(BodySlotPanel);
		_panels.Add(BodyLootPanel);
		_panels.Add(ChestLootPanel);
		_panels.Add(SplitItemPanel);
		_panels.Add(InventoryPanel);

		Hide();


	}

	public override void PerFrameUpdate ()
	{
		foreach(PanelBase panel in _panels)
		{
			if(panel.IsActive)
			{
				panel.PerFrameUpdate();
			}
		}
	}

	public override void Show ()
	{

		GameManager.Inst.UIManager.HUDPanel.OnWindowPanelOpen();
		Camera.main.GetComponent<BlurOptimized>().enabled = true;

		Time.timeScale = 0;

		NGUITools.SetActive(this.gameObject, true);
		this.IsActive = true;

		InputEventHandler.Instance.State = UserInputState.WindowsOpen;

		UIEventHandler.Instance.TriggerOpenWindow();
	}

	public override void Hide ()
	{

		GameManager.Inst.UIManager.HUDPanel.OnWindowPanelClose();
		Camera.main.GetComponent<BlurOptimized>().enabled = false;
		BackgroundNormal.enabled = false;
		BackgroundTrifold.enabled = false;

		foreach(PanelBase panel in _panels)
		{
			if(panel.IsActive)
			{
				panel.Hide();
			}
		}

		Time.timeScale = 1;



		NGUITools.SetActive(this.gameObject, false);
		this.IsActive = false;

		InputEventHandler.Instance.State = UserInputState.Normal;
	}

	public List<InventoryGrid> FindInventoryGrids()
	{
		List<InventoryGrid> grids = new List<InventoryGrid>();

		foreach(PanelBase panel in _panels)
		{
			if(panel.IsActive)
			{
				List<InventoryGrid> temp;
				if(panel.HasInventoryGrids(out temp))
				{
					grids.AddRange(temp);
				}
			}
		}

		return grids;
	}

	public List<BodySlot> FindBodySlots()
	{
		List<BodySlot> bodySlots = new List<BodySlot>();

		foreach(PanelBase panel in _panels)
		{
			if(panel.IsActive)
			{
				List<BodySlot> temp;
				if(panel.HasBodySlots(out temp))
				{
					bodySlots.AddRange(temp);
				}
			}
		}

		return bodySlots;
	}

	public List<TempSlot> FindTempSlots()
	{
		List<TempSlot> tempSlots = new List<TempSlot>();

		foreach(PanelBase panel in _panels)
		{
			if(panel.IsActive)
			{
				List<TempSlot> temp;
				if(panel.HasTempSlots(out temp))
				{
					tempSlots.AddRange(temp);
				}
			}
		}

		return tempSlots;
	}

	public void SetBackground(bool isTrifold)
	{
		if(isTrifold)
		{
			BackgroundNormal.enabled = false;
			BackgroundTrifold.enabled = true;
			WindowAnchor.transform.localPosition = new Vector3(-242f, 60, 0);
		}
		else
		{
			BackgroundNormal.enabled = true;
			BackgroundTrifold.enabled = false;
			WindowAnchor.transform.localPosition = new Vector3(0, 60, 0);
		}
	}

}
