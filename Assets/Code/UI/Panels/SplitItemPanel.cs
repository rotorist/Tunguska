using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplitItemPanel : PanelBase
{
	public GridItem Target;
	public UISlider QuantitySlider;
	public UILabel QuantityLabel;
	public UIButton OK;
	public UIButton Cancel;

	private int _quanity;

	public override void Initialize ()
	{

		Hide();
	}

	public override void PerFrameUpdate ()
	{


	}

	public override void Show ()
	{
		NGUITools.SetActive(this.gameObject, true);
		this.IsActive = true;

		QuantitySlider.numberOfSteps = Target.GetQuantity();
		QuantitySlider.value = (Target.GetQuantity() * 1f) / Target.GetQuantity();

		InputEventHandler.Instance.State = UserInputState.PopupOpen;

		InputEventHandler.OnPopupMouseWheel -= OnMouseWheelInput;
		InputEventHandler.OnPopupMouseWheel += OnMouseWheelInput;
	}

	public override void Hide ()
	{
		NGUITools.SetActive(this.gameObject, false);
		this.IsActive = false;

		InputEventHandler.Instance.State = UserInputState.WindowsOpen;

		InputEventHandler.OnPopupMouseWheel -= OnMouseWheelInput;
	}

	public override bool HasBodySlots (out List<BodySlot> bodySlots)
	{
		bodySlots = null;
		return false;
	}

	public override bool HasTempSlots (out List<TempSlot> tempSlots)
	{
		tempSlots = null;

		return false;
	}

	public void OnSliderValueChange()
	{
		_quanity = Mathf.RoundToInt(QuantitySlider.value * Target.GetQuantity());
		QuantityLabel.text = _quanity.ToString();
	}

	public void OnCancelButtonPress()
	{
		Target = null;
		_quanity = 0;
		Hide();
	}

	public void OnOKButtonPress()
	{
		if(_quanity <= 0)
		{
			OnCancelButtonPress();
			return;
		}

		GameManager.Inst.UIManager.WindowPanel.InventoryPanel.OnCloseSplitMenu(Target, _quanity);
	}


	public void OnMouseWheelInput(float movement)
	{
		float normalizedMovement = 0;
		if(movement > 0)
		{
			normalizedMovement = 1;
		}
		else
		{
			normalizedMovement = -1;
		}

		QuantitySlider.value += normalizedMovement /  Target.GetQuantity();
	}
}
