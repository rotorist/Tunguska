﻿using UnityEngine;
using System.Collections;

public class GridItem : MonoBehaviour
{

	public int ColumnPos;
	public int RowPos;
	public int ColumnSize;
	public int RowSize;
	public UISprite Sprite;
	public UISprite Boundary;
	public UILabel Quantity;
	public GridItemOrient Orientation;
	public GridItemState State;

	public Item Item;


	private int _quantity;


	public bool IsRotatable
	{
		get { return ColumnSize != RowSize; }
	}

	private InventoryGrid _grid;

	void OnHover(bool isOver)
	{
		if(isOver)
		{
			GameManager.Inst.CursorManager.SetCursorState(CursorState.Hand);
		}
		else
		{
			GameManager.Inst.CursorManager.SetCursorState(CursorState.Default);
		}

		GameManager.Inst.UIManager.WindowPanel.InventoryPanel.OnHoverItem(this);
	}

	void OnClick()
	{
		if(UICamera.currentTouchID == -1)
		{
			if(State == GridItemState.None)
			{
				GameManager.Inst.UIManager.WindowPanel.InventoryPanel.OnSelectItem(this);
			}
			else if(State == GridItemState.Selected)
			{
				//place item
				GameManager.Inst.UIManager.WindowPanel.InventoryPanel.OnPlaceItem(this);
			}
		}
		else if(UICamera.currentTouchID == -2)
		{
			if(State == GridItemState.None)
			{
				if(Item.Type == ItemType.PrimaryWeapon || Item.Type == ItemType.SideArm)
				{
					//unload ammo
					GameManager.Inst.UIManager.WindowPanel.InventoryPanel.OnUnloadAmmo(this);
				}
				else
				{
					//open split panel
					GameManager.Inst.UIManager.WindowPanel.InventoryPanel.OnOpenSplitMenu(this);
				}
			}
			else if(State == GridItemState.Selected)
			{
				//rotate item
				ToggleOrientation();
			}
		}
	}



	public void Initialize(InventoryGrid grid)
	{
		_grid = grid;
		State = GridItemState.None;
	}

	public void ToggleOrientation()
	{
		if(Orientation == GridItemOrient.Landscape)
		{
			Sprite.transform.localEulerAngles = new Vector3(0, 0, 90);
			if(State == GridItemState.None)
			{
				Sprite.pivot = UIWidget.Pivot.TopLeft;
			}
			else
			{
				Sprite.pivot = UIWidget.Pivot.Center;
			}

			Boundary.transform.localEulerAngles = new Vector3(0, 0, 90);
			Boundary.pivot = UIWidget.Pivot.TopLeft;

			int temp = ColumnSize;
			ColumnSize = RowSize;
			RowSize = temp;

			Orientation = GridItemOrient.Portrait;
		}
		else
		{
			Sprite.transform.localEulerAngles = new Vector3(0, 0, 0);
			if(State == GridItemState.None)
			{
				Sprite.pivot = UIWidget.Pivot.BottomLeft;
			}
			else
			{
				Sprite.pivot = UIWidget.Pivot.Center;
			}

			Boundary.transform.localEulerAngles = new Vector3(0, 0, 0);
			Boundary.pivot = UIWidget.Pivot.BottomLeft;

			int temp = ColumnSize;
			ColumnSize = RowSize;
			RowSize = temp;

			Orientation = GridItemOrient.Landscape;
		}

		NGUITools.AddWidgetCollider(gameObject);
	}

	public void SetQuantity(int quantity)
	{
		_quantity = quantity;

		if(quantity <= 1)
		{
			Quantity.text = "";
		}
		else
		{
			Quantity.text = quantity.ToString();

		}
	}

	public int GetQuantity()
	{
		return _quantity;
	}
}

public enum GridItemOrient
{
	Landscape,
	Portrait,
}

public enum GridItemState
{
	None,
	Selected,
	MenuOpen,
}