using UnityEngine;
using System.Collections;

public class HumanAnimStateAction : HumanAnimStateBase
{
	private float _vSpeed;

	// This constructor will create new state taking values from old state
	public HumanAnimStateAction(HumanAnimStateBase state)     
		:this(state.ParentCharacter)
	{

	}

	// this constructor will be used by the other one
	public HumanAnimStateAction(HumanCharacter parentCharacter)
	{
		this.ParentCharacter = parentCharacter;

		Initialize();
	}


	public override void SendCommand (CharacterCommands command)
	{
		if(command == CharacterCommands.AnimationActionDone)
		{
			UpdateState(HumanBodyStates.StandIdle);

		}

	}

	public override void Update ()
	{

	}

	public override bool IsRotatingBody ()
	{
		return false;
	}



	private void Initialize()
	{
		Debug.Log("Initializing Action");
		_vSpeed = this.ParentCharacter.MyAnimator.GetFloat("VSpeed");
		this.ParentCharacter.Destination = this.ParentCharacter.transform.position;
		this.ParentCharacter.MyNavAgent.Stop();
		this.ParentCharacter.MyNavAgent.ResetPath();
		this.ParentCharacter.MyNavAgent.updateRotation = false;
		this.ParentCharacter.MyHeadIK.Weight = 0;
		this.ParentCharacter.CurrentAnimStateName = "Action";

		if(this.ParentCharacter.MyAI.BlackBoard.AnimationAction == AnimationActions.KnockBack)
		{
			this.ParentCharacter.MyAnimator.SetTrigger("KnockBack");
			this.ParentCharacter.MyNavAgent.destination = this.ParentCharacter.MyAI.BlackBoard.ActionMovementDest;
			this.ParentCharacter.MyNavAgent.speed = this.ParentCharacter.MyAI.BlackBoard.ActionMovementSpeed;
			this.ParentCharacter.MyNavAgent.acceleration = 60;
		}
		else if(this.ParentCharacter.MyAI.BlackBoard.AnimationAction == AnimationActions.KnockForward)
		{
			this.ParentCharacter.MyAnimator.SetTrigger("KnockForward");
			this.ParentCharacter.MyNavAgent.destination = this.ParentCharacter.MyAI.BlackBoard.ActionMovementDest;
			this.ParentCharacter.MyNavAgent.speed = this.ParentCharacter.MyAI.BlackBoard.ActionMovementSpeed;
			this.ParentCharacter.MyNavAgent.acceleration = 60;
		}
		else if(this.ParentCharacter.MyAI.BlackBoard.AnimationAction == AnimationActions.ComboAttack)
		{
			//make character face look target
			Vector3 lookDir = this.ParentCharacter.LookTarget.position - this.ParentCharacter.transform.position;
			lookDir = new Vector3(lookDir.x, 0, lookDir.z);
			Quaternion rotation = Quaternion.LookRotation(lookDir);
			this.ParentCharacter.transform.rotation = rotation;

			this.ParentCharacter.MyAnimator.SetTrigger("ComboAttack");

			this.ParentCharacter.MyNavAgent.destination = this.ParentCharacter.MyAI.BlackBoard.ActionMovementDest;
			this.ParentCharacter.MyNavAgent.speed = this.ParentCharacter.MyAI.BlackBoard.ActionMovementSpeed;
			this.ParentCharacter.MyNavAgent.acceleration = 30;
		}
	}

	private void UpdateState(HumanBodyStates state)
	{
		switch(state)
		{
		case HumanBodyStates.WalkForward:
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateGoForward(this);
			break;
		case HumanBodyStates.StandIdle:
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateIdle(this);
			break;
		case HumanBodyStates.CrouchIdle:
			this.ParentCharacter.CurrentAnimState = new HumanAnimStateSneakIdle(this);
			break;
		}

	}
}
