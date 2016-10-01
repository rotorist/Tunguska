using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AISquad
{
	public List<HumanCharacter> Members;
	public Household Household;

	public AISquad()
	{
		Members = new List<HumanCharacter>();

		Household = GameObject.Find("Household1").GetComponent<Household>();
	}

	public void IssueSquadCommand()
	{
		


		Members[0].MyAI.BlackBoard.PatrolLoc = Household.transform.position;
		Members[0].MyAI.BlackBoard.PatrolRange = Household.PatrolRange;
		Members[0].MyAI.BlackBoard.CombatRange = Household.CombatRange;
		Members[0].MyAI.BlackBoard.HasPatrolInfo = true;
		Members[0].MyAI.BlackBoard.PatrolNodeIndex = 0;
		Members[0].MyAI.SetDynamicyGoal(GameManager.Inst.NPCManager.DynamicGoalPatrol, 5);
		/*
		Members[1].MyAI.BlackBoard.PatrolLoc = Household.GuardLocs[0];
		Members[1].MyAI.BlackBoard.GuardDirection = Household.GuardDirs[0];
		Members[1].MyAI.BlackBoard.CombatRange = new Vector3(10, 10, 10);
		Members[1].MyAI.BlackBoard.HasPatrolInfo = true;
		Members[1].MyAI.BlackBoard.PatrolNodeIndex = -1;
		Members[1].MyAI.SetDynamicyGoal(GameManager.Inst.NPCManager.DynamicGoalGuard, 5);

		Members[2].MyAI.BlackBoard.PatrolLoc = Household.GuardLocs[1];
		Members[2].MyAI.BlackBoard.GuardDirection = Household.GuardDirs[1];
		Members[2].MyAI.BlackBoard.CombatRange = new Vector3(10, 10, 10);
		Members[2].MyAI.BlackBoard.HasPatrolInfo = true;
		Members[2].MyAI.BlackBoard.PatrolNodeIndex = -1;
		Members[2].MyAI.SetDynamicyGoal(GameManager.Inst.NPCManager.DynamicGoalGuard, 5);


		Members[3].MyAI.BlackBoard.PatrolLoc = Household.transform.position;
		Members[3].MyAI.BlackBoard.PatrolRange = Household.PatrolRange;
		Members[3].MyAI.BlackBoard.CombatRange = Household.CombatRange;
		Members[3].MyAI.BlackBoard.HasPatrolInfo = true;
		Members[3].MyAI.BlackBoard.PatrolNodeIndex = -1;
		Members[3].MyAI.SetDynamicyGoal(GameManager.Inst.NPCManager.DynamicGoalPatrol, 5);
		*/
		/*
		Members[0].MyAI.BlackBoard.PatrolLoc = Household.GuardLocs[0];
		Members[0].MyAI.BlackBoard.GuardDirection = Household.GuardDirs[0];
		Members[0].MyAI.BlackBoard.CombatRange = new Vector3(10, 10, 10);
		Members[0].MyAI.BlackBoard.HasPatrolInfo = true;
		Members[0].MyAI.BlackBoard.PatrolNodeIndex = -1;
		Members[0].MyAI.SetDynamicyGoal(GameManager.Inst.NPCManager.DynamicGoalGuard, 5);
		*/
		/*
		Members[0].MyAI.BlackBoard.PatrolLoc = Household.GuardLocs[0];
		Members[0].MyAI.BlackBoard.GuardDirection = Household.GuardDirs[0];
		Members[0].MyAI.BlackBoard.CombatRange = new Vector3(10, 10, 10);
		Members[0].MyAI.BlackBoard.HasPatrolInfo = true;
		Members[0].MyAI.BlackBoard.PatrolNodeIndex = -1;
		Members[0].MyAI.SetDynamicyGoal(GameManager.Inst.NPCManager.DynamicGoalGuard, 5);

		Members[1].MyAI.BlackBoard.PatrolLoc = Household.GuardLocs[1];
		Members[1].MyAI.BlackBoard.GuardDirection = Household.GuardDirs[1];
		Members[1].MyAI.BlackBoard.CombatRange = new Vector3(10, 10, 10);
		Members[1].MyAI.BlackBoard.HasPatrolInfo = true;
		Members[1].MyAI.BlackBoard.PatrolNodeIndex = -1;
		Members[1].MyAI.SetDynamicyGoal(GameManager.Inst.NPCManager.DynamicGoalGuard, 5);
		*/
	}

	public bool IsAnyOneInvestigating(Vector3 location)
	{
		foreach(HumanCharacter member in Members)
		{
			GoapAction currentAction = member.MyAI.GetCurrentAction();
			if(currentAction != null && currentAction.Name == "ActionInvestigate")
			{
				Debug.Log("member  " + member + " is doing investigation");
				if(Vector3.Distance(member.MyAI.BlackBoard.HighestDisturbanceLoc, location) < 5)
				{
					Debug.Log("member " + member.name + " is investigating loc " + location);
					return true;
				}
			}
			else if(currentAction != null && currentAction.Name == "ActionCheckCorpse")
			{
				if(Vector3.Distance(member.MyAI.BlackBoard.TargetCorpse.LastKnownPos, location) < 5)
				{
					Debug.Log("member " + member.name + " is checking corpse at loc " + location);
					return true;
				}
			}
		}
		return false;
	}

	public bool IsAnyOneIntimidating(Character target)
	{

		return false;
	}

	public bool IsPatrolNodeTaken(int node)
	{
		foreach(HumanCharacter member in Members)
		{
			if(member.MyAI.BlackBoard.PatrolNodeIndex == node)
			{
				return true;
			}
		}

		return false;
	}

	public void SetSquadAlertLevel(object level)
	{
		foreach(HumanCharacter member in Members)
		{
			member.MyAI.BlackBoard.GuardLevel = (int)level;
		}
	}


	public void BroadcastMemoryFact(WorkingMemoryFact fact)
	{
		foreach(HumanCharacter member in Members)
		{
			member.MyAI.WorkingMemory.AddFact(fact);
		}
	}
}
