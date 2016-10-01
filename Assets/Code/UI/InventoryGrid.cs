using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour 
{
	public int Columns;
	public int Rows;
	public int GridWidth;

	public UISprite Grid;

	public UIPanel ParentPanel;

	public int BlockSize
	{
		get { return GridWidth / Columns; }
	}

	public List<GridItem> Items;


	public void Initialize()
	{
		Items = new List<GridItem>();
		Grid = GetComponent<UISprite>();


	}

	public void AddGridItem(Item item, int colPos, int rowPos, GridItemOrient orientation, int quantity)
	{
		//Debug.Log("orientation is " + orientation);
		GridItem item1 = LoadGridItem(item.SpriteName, orientation);
		item1.ColumnPos = colPos;
		item1.RowPos = rowPos;
		item1.Sprite.transform.localPosition = new Vector3(BlockSize * item1.ColumnPos, BlockSize * item1.RowPos, 0);
		item1.Boundary.transform.localPosition = item1.Sprite.transform.localPosition;
		item1.Quantity.transform.localPosition = item1.Sprite.transform.localPosition + new Vector3(4, 4, 0);
		item1.SetQuantity(quantity);
		item1.Item = item;
		Items.Add(item1);
	}

	public bool GetColumnRowFromPos(Vector3 pos, out int column, out int row)
	{
		column = 0;
		row = 0;

		Vector3 distance = pos - Grid.transform.localPosition;

		if(distance.x < -0.5f * BlockSize || distance.y < -0.5f * BlockSize)
		{
			return false;
		}
		else
		{
			float x = distance.x / BlockSize;
			float y = distance.y / BlockSize;

			column = Mathf.RoundToInt(x);
			row = Mathf.RoundToInt(y);

			return true;
		}
	}

	public bool GetColumnRowFromLocalPos(Vector3 pos, out int column, out int row)
	{
		column = 0;
		row = 0;

		Vector3 distance = pos;

		if(distance.x < 0 || distance.y < 0)
		{
			return false;
		}
		else
		{
			float x = distance.x / BlockSize;
			float y = distance.y / BlockSize;

			column = Mathf.RoundToInt(x);
			row = Mathf.RoundToInt(y);

			return true;
		}
	}

	public bool CanItemFitHere(int column, int row, int colSize, int rowSize, out GridItem replaceItem)
	{
		replaceItem = null;

		int overlapItems = 0;
		GridItem temp = null;
		GridItem overlapItem1 = null;

		for(int x=column; x<column + colSize; x++)
		{
			for(int y=row; y<row + rowSize; y++)
			{
				if(x >= Columns || y >= Rows)
				{
					return false;
				}

				if(IsBlockOccupied(x, y, out temp))
				{
					if(overlapItem1 == null)
					{
						//overlapitem1 hasn't been set yet, set it
						overlapItem1 = temp;
						overlapItems ++;
					}
					else if(overlapItem1 != temp)
					{
						//there's another overlap item
						overlapItems ++;
					}
				}
			}
		}

		if(overlapItems <= 1)
		{
			replaceItem = overlapItem1;
			return true;
		}
		else
		{
			return false;
		}
	}

	private bool IsBlockOccupied(int col, int row, out GridItem occupier)
	{
		occupier = null;
		foreach(GridItem item in Items)
		{
			if(item.State == GridItemState.Selected)
			{
				//ignore item that has been selected
				continue;
			}

			if(col >= item.ColumnPos && col < item.ColumnPos + item.ColumnSize &&
				row >= item.RowPos && row < item.RowPos + item.RowSize)
			{
				occupier = item;
				return true;
			}
		}

		return false;
	}

	private GridItem LoadGridItem(string itemID, GridItemOrient orientation)
	{
		GameObject o = GameObject.Instantiate(Resources.Load("ItemSprite_" + itemID)) as GameObject;
		UISprite sprite = o.GetComponent<UISprite>();
		GridItem item = o.GetComponent<GridItem>();
		o.transform.parent = transform;

		sprite.MakePixelPerfect();
		sprite.width = BlockSize * item.ColumnSize;
		sprite.height = BlockSize * item.RowSize;
		sprite.depth = (int)InventoryItemDepth.Normal;
		item.Sprite = sprite;

		//apply boundary
		o = GameObject.Instantiate(Resources.Load("ItemBoundary")) as GameObject;
		UISprite boundary = o.GetComponent<UISprite>();
		boundary.transform.parent = transform;
		boundary.MakePixelPerfect();
		boundary.width = sprite.width;
		boundary.height = sprite.height;
		item.Boundary = boundary;

		//quantity label
		o = GameObject.Instantiate(Resources.Load("ItemQuantity")) as GameObject;
		o.transform.parent = transform;
		o.transform.localScale = new Vector3(1, 1, 1);
		item.Quantity = o.GetComponent<UILabel>();
		item.Quantity.depth = item.Sprite.depth + 1;

		if(orientation == GridItemOrient.Portrait)
		{
			item.ToggleOrientation();
		}



		return item;

	}


}

