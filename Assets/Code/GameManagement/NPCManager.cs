using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCManager
{
	public GoapGoal DynamicGoalGuard;
	public GoapGoal DynamicGoalFollow;
	public GoapGoal DynamicGoalPatrol;

	public List<HumanCharacter> HumansInScene
	{
		get { return _humansInScene; }
	}

	public List<MutantCharacter> MutantsInScene
	{
		get { return _mutantsInScene; }
	}

	public List<Character> AllCharacters
	{
		get { return _allCharacters; }
	}

	private List<HumanCharacter> _humansInScene;
	private List<MutantCharacter> _mutantsInScene;
	private List<Character> _allCharacters;

	public void Initialize()
	{
		_allCharacters = new List<Character>();
		_humansInScene = new List<HumanCharacter>();
		_mutantsInScene = new List<MutantCharacter>();

		DynamicGoalGuard = GameManager.Inst.DBManager.DBHandlerAI.GetGoalByID(6);
		DynamicGoalGuard.Priority = 5;
		DynamicGoalFollow = GameManager.Inst.DBManager.DBHandlerAI.GetGoalByID(8);
		DynamicGoalFollow.Priority = 5;
		DynamicGoalPatrol = GameManager.Inst.DBManager.DBHandlerAI.GetGoalByID(2);
		DynamicGoalPatrol.Priority = 5;
	}

	public void AddHumanCharacter(HumanCharacter character)
	{
		_humansInScene.Add(character);
		_allCharacters.Add(character);
	}

	public void AddMutantCharacter(MutantCharacter character)
	{
		_mutantsInScene.Add(character);
		_allCharacters.Add(character);
	}
}
