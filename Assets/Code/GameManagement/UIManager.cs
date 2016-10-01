using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIManager
{
	public UIRoot Root;
	public Camera UICamera;
	public UIStateMachine UIStateMachine;

	public BarkPanel BarkPanel;
	public HUDPanel HUDPanel;
	public WindowPanel WindowPanel;
	public DialoguePanel DialoguePanel;

	private List<PanelBase> _panels;

	public void Initialize()
	{
		_panels = new List<PanelBase>();
		Root = GameObject.Find("UI Root").GetComponent<UIRoot>();

		Root.manualHeight = Screen.height;
		Root.manualWidth = Screen.width;

		UICamera = Root.transform.Find("UICamera").GetComponent<Camera>();


		BarkPanel = UICamera.transform.Find("BarkPanel").GetComponent<BarkPanel>();
		BarkPanel.Initialize();

		HUDPanel = UICamera.transform.Find("HUDPanel").GetComponent<HUDPanel>();
		HUDPanel.Initialize();

		WindowPanel = UICamera.transform.Find("WindowPanel").GetComponent<WindowPanel>();
		WindowPanel.Initialize();

		DialoguePanel = UICamera.transform.Find("DialoguePanel").GetComponent<DialoguePanel>();
		DialoguePanel.Initialize();


		_panels.Add(DialoguePanel);
		_panels.Add(WindowPanel);
		_panels.Add(HUDPanel);
		_panels.Add(BarkPanel);


		UIStateMachine = new UIStateMachine();
		UIStateMachine.Initialize();
	}

	public void PerFrameUpdate()
	{
		BarkPanel.PerFrameUpdate();
		HUDPanel.PerFrameUpdate();

		if(WindowPanel.IsActive)
		{
			WindowPanel.PerFrameUpdate();
		}
	}


	public bool IsCursorInHUDRegion()
	{
		Vector3 cursorLoc = GameManager.Inst.CursorManager.ActiveCursor.transform.localPosition;
		if(cursorLoc.x > -650 && cursorLoc.x < 650 && cursorLoc.y < -350)
		{
			return true;
		}

		return false;
	}

	public void HideAllPanels()
	{
		foreach(PanelBase panel in _panels)
		{
			if(panel.IsActive)
			{
				panel.Hide();
			}
		}
	}

	public void SetConsoleText(string text)
	{
		HUDPanel.SetConsoleText(text);
	}
}
