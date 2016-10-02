﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionFollow: GoapAction 
{
	private float _distThreshold;

	public ActionFollow(string name, string description, float cost)
	{
		Name = name;
		Description = description;
		Cost = cost;
		_preconditions = new List<GoapWorldState>();
		_effects = new List<GoapWorldState>();
	}

	public override bool ExecuteAction()
	{
		if(ParentCharacter.MyAI.BlackBoard.FollowTarget == null)
		{
			return false;
		}

		CsDebug.Inst.CharLog(ParentCharacter, "Start executing Following " + ParentCharacter.name);
		_executionStopped = false;

		_distThreshold = UnityEngine.Random.Range(300, 500)/100f;

		UpdateAction();

		ParentCharacter.MyEventHandler.OnOneSecondTimer -= UpdateAction;
		ParentCharacter.MyEventHandler.OnOneSecondTimer += UpdateAction;
		ParentCharacter.MyEventHandler.OnPerFrameTimer -= PerFrameUpdate;
		ParentCharacter.MyEventHandler.OnPerFrameTimer += PerFrameUpdate;

		return true;
	}

	public override void StopAction()
	{
		CsDebug.Inst.CharLog(ParentCharacter, "Stop executing Following " + ParentCharacter.name);
		_executionStopped = true;

		ParentCharacter.MyEventHandler.OnOneSecondTimer -= UpdateAction;
		ParentCharacter.MyEventHandler.OnPerFrameTimer -= PerFrameUpdate;
	}

	public override bool AbortAction (float priority)
	{
		if(ParentCharacter.MyAI.BlackBoard.GuardLevel == 0)
		{
			return false;
		}

		if(priority > 0.5f)
		{
			CsDebug.Inst.CharLog(ParentCharacter, "Abort executing Following");
			ParentCharacter.MyAI.WorkingMemory.AddFact(FactType.FailedAction, this.Name, Vector3.zero, 1, 0.5f);
			StopAction();
			return true;
		}
		else
		{
			return false;
		}
	}

	public override bool CheckActionCompletion()
	{

		foreach(GoapWorldState state in Effects)
		{

			object result = ParentCharacter.MyAI.EvaluateWorldState(state);
			//CsDebug.Inst.CharLog(ParentCharacter, "Checking if state " + state.Name + " value is " + state.Value + " result: " + result);
			if(!result.Equals(state.Value))
			{
				//CsDebug.Inst.CharLog(ParentCharacter, "result is not equal to effect");
				return false;
			}
		}

		return true;
	}
		

	public void UpdateAction()
	{
		if(!CheckAvailability() || _executionStopped)
		{
			return;
		}

		float dist = Vector3.Distance(ParentCharacter.transform.position, ParentCharacter.MyAI.BlackBoard.FollowTarget.transform.position);

		//if distance between me and follow target is
		if(dist > _distThreshold)
		{
			//set destination to follow target


			ParentCharacter.CurrentStance = ParentCharacter.MyAI.BlackBoard.FollowTarget.CurrentStance;
			if(ParentCharacter.CurrentStance == HumanStances.Crouch || ParentCharacter.CurrentStance == HumanStances.CrouchRun)
			{
				ParentCharacter.SendCommand(CharacterCommands.Crouch);
			}
			else
			{
				ParentCharacter.SendCommand(CharacterCommands.StopCrouch);
			}
			ParentCharacter.SendCommand(CharacterCommands.GoToPosition);
		}
		else 
		{
			if(ParentCharacter.MyAI.BlackBoard.FollowTarget.MyNavAgent.velocity.magnitude <= 0)
			{
				ParentCharacter.SendCommand(CharacterCommands.Idle);
				_distThreshold = UnityEngine.Random.Range(300, 500)/100f;
			}
		}

		if(ParentCharacter.MyAI.BlackBoard.FollowTarget.MyReference.CurrentWeapon != null && ParentCharacter.MyReference.CurrentWeapon == null)
		{
			ParentCharacter.SendCommand(ParentCharacter.MyAI.WeaponSystem.GetBestWeaponChoice());
		}

		if(ParentCharacter.MyAI.BlackBoard.HighestPersonalThreat <= 0)
		{
			ParentCharacter.SendCommand(CharacterCommands.StopAim);
		}

		/*
		if(CheckActionCompletion())
		{
			StopAction();
			ParentCharacter.MyEventHandler.TriggerOnActionCompletion();
		}
		*/
	}

	public void PerFrameUpdate()
	{
		float dist = Vector3.Distance(ParentCharacter.transform.position, ParentCharacter.MyAI.BlackBoard.FollowTarget.transform.position);
		if(dist > _distThreshold)
		{
			ParentCharacter.Destination = ParentCharacter.MyAI.BlackBoard.FollowTarget.transform.position;
			ParentCharacter.MyAI.BlackBoard.NavTarget = ParentCharacter.Destination.Value;
		}
		else if( dist < _distThreshold / 2 && ParentCharacter.MyNavAgent.velocity.magnitude > 0)
		{
			ParentCharacter.SendCommand(CharacterCommands.Idle);
		}

	}



	private bool CheckAvailability()
	{
		if(ParentCharacter.IsBodyLocked)
		{
			return false;
		}
		else
		{
			return true;
		}
	}


}
