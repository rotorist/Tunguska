using UnityEngine;
using System.Collections;

public class HumanAnimStateDeath : HumanAnimStateBase
{

	// This constructor will create new state taking values from old state
	public HumanAnimStateDeath(HumanAnimStateBase state)     
		:this(state.ParentCharacter)
	{

	}

	// this constructor will be used by the other one
	public HumanAnimStateDeath(HumanCharacter parentCharacter)
	{
		this.ParentCharacter = parentCharacter;

		Initialize();
	}

	public override void SendCommand (CharacterCommands command)
	{


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
		Debug.Log("Initializing Stand Idle");
		this.ParentCharacter.Destination = this.ParentCharacter.transform.position;

		this.ParentCharacter.MyNavAgent.acceleration = 3;
		this.ParentCharacter.MyNavAgent.Stop();
		this.ParentCharacter.MyNavAgent.ResetPath();
		this.ParentCharacter.MyNavAgent.updateRotation = false;
	}

	private void UpdateState(HumanBodyStates state)
	{


	}
}
