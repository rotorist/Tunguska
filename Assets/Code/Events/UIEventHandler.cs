using UnityEngine;
using System.Collections;

public class UIEventHandler 
{
	#region Singleton 
	private static UIEventHandler _instance;
	public static UIEventHandler Instance	
	{
		get 
		{
			if (_instance == null)
				_instance = new UIEventHandler();

			return _instance;
		}
	}
	#endregion

	#region Constructor
	public UIEventHandler()
	{

	}

	#endregion


	public delegate void GeneralUIEventDelegate();
	public static event GeneralUIEventDelegate OnOpenWindow;
	public static event GeneralUIEventDelegate OnCloseWindow;
	public static event GeneralUIEventDelegate OnToggleInventory;
	public static event GeneralUIEventDelegate OnLootBody;
	public static event GeneralUIEventDelegate OnLootChest;
	public static event GeneralUIEventDelegate OnStartDialogue;



	public void TriggerOpenWindow()
	{
		if(OnOpenWindow != null)
		{
			OnOpenWindow();
		}
	}

	public void TriggerCloseWindow()
	{
		if(OnCloseWindow != null)
		{
			OnCloseWindow();
		}
	}

	public void TriggerToggleInventory()
	{
		if(OnToggleInventory != null)
		{
			OnToggleInventory();
		}
	}

	public void TriggerLootBody()
	{
		if(OnLootBody != null)
		{
			OnLootBody();
		}
	}

	public void TriggerLootChest()
	{
		if(OnLootChest != null)
		{
			OnLootChest();
		}
	}

	public void TriggerDialogue()
	{
		if(OnStartDialogue != null)
		{
			OnStartDialogue();
		}
	}
}
