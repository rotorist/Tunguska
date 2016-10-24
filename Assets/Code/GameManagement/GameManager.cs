using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GameManager : MonoBehaviour 
{

	#region Singleton
	
	public static GameManager Inst;


	#endregion

	#region Public Fields

	public EventManager EventManager;
	public FXManager FXManager;
	public NPCManager NPCManager;
	public DBManager DBManager;
	public UIManager UIManager;
	public CursorManager CursorManager;
	public ItemManager ItemManager;

	public CameraController CameraController;
	public CameraShaker CameraShaker;
	public PlayerControl PlayerControl;

	public AIScheduler AIScheduler;

	public string AppDataPath;

	#endregion

	void Start()
	{
		UnityEngine.Debug.Log("Game Manager Started");
		AppDataPath = Application.dataPath;
		Initialize();
	}

	void Update()
	{
		EventManager.ManagerPerFrameUpdate();
		PlayerControl.PerFrameUpdate();
		AIScheduler.UpdatePerFrame();
		CursorManager.PerFrameUpdate();
		UIManager.PerFrameUpdate();
	}

	void LateUpdate()
	{
		
	}


	#region Private Methods

	private void Initialize()
	{
		

		Inst = this;

		//Initializing CsDebug
		CsDebug debug = GetComponent<CsDebug>();
		debug.Initialize();

		//Initializing DBManager
		DBManager = new DBManager();
		DBManager.Initialize();

		//Initializing Event Manager
		EventManager = new EventManager();
		EventManager.Initialize();

		ItemManager = new ItemManager();
		ItemManager.Initialize();

		//Initializing NPC Manager
		NPCManager = new NPCManager();
		NPCManager.Initialize();



		PlayerControl = new PlayerControl();
		PlayerControl.Initialize();



		UIManager = new UIManager();
		UIManager.Initialize();


		MutantCharacter mutant1 = GameObject.Find("MutantCharacter").GetComponent<MutantCharacter>();
		mutant1.Initialize();
		mutant1.MyStatus.MaxHealth = 200;
		mutant1.MyStatus.Health = 200;
		mutant1.MyAI.BlackBoard.PatrolLoc = new Vector3(60, 0, -11);
		mutant1.MyAI.BlackBoard.PatrolRange = new Vector3(30, 10, 10);
		mutant1.MyAI.BlackBoard.CombatRange = new Vector3(40, 20, 20);
		mutant1.MyAI.BlackBoard.HasPatrolInfo = true;


		MutantCharacter mutant2 = GameObject.Find("MutantCharacter2").GetComponent<MutantCharacter>();
		mutant2.Initialize();
		mutant2.MyStatus.MaxHealth = 100;
		mutant2.MyStatus.Health = 100;
		mutant2.MyAI.BlackBoard.PatrolLoc = new Vector3(60, 0, -11);
		mutant2.MyAI.BlackBoard.PatrolRange = new Vector3(30, 10, 10);
		mutant2.MyAI.BlackBoard.CombatRange = new Vector3(40, 20, 20);
		mutant2.MyAI.BlackBoard.HasPatrolInfo = true;



		//HumanCharacter enemy1 = GameObject.Find("HumanCharacter2").GetComponent<HumanCharacter>();
		HumanCharacter enemy2 = GameObject.Find("HumanCharacter4").GetComponent<HumanCharacter>();
		//HumanCharacter enemy3 = GameObject.Find("HumanCharacter5").GetComponent<HumanCharacter>();
		//HumanCharacter enemy4 = GameObject.Find("HumanCharacter6").GetComponent<HumanCharacter>();

		AISquad enemySquad = new AISquad();
		//enemySquad.Members.Add(enemy1);
		enemySquad.Members.Add(enemy2);
		//enemySquad.Members.Add(enemy3);
		//enemySquad.Members.Add(enemy4);

		/*
		enemy1.Initialize();
		enemy1.MyAI.Squad = enemySquad;
		ItemManager.LoadNPCInventory(enemy1.Inventory);
		enemy1.MyAI.WeaponSystem.LoadWeaponsFromInventory();
		*/
		enemy2.Initialize();
		enemy2.MyAI.Squad = enemySquad;
		ItemManager.LoadNPCInventory(enemy2.Inventory);
		enemy2.MyAI.WeaponSystem.LoadWeaponsFromInventory();

		/*
		enemy3.Initialize();
		enemy3.MyAI.Squad = enemySquad;
		ItemManager.LoadNPCInventory(enemy3.Inventory);
		enemy3.MyAI.WeaponSystem.LoadWeaponsFromInventory();


		enemy4.Initialize();
		enemy4.MyAI.Squad = enemySquad;
		ItemManager.LoadNPCInventory(enemy4.Inventory);
		enemy4.MyAI.WeaponSystem.LoadWeaponsFromInventory();
		*/

		//enemy1.MyStatus.MaxHealth = 160;
		//enemy1.MyStatus.Health = 160;
		enemy2.MyStatus.MaxHealth = 100;
		enemy2.MyStatus.Health = 100;

		/*
		enemy3.MyStatus.MaxHealth = 80;
		enemy3.MyStatus.Health = 80;

		enemy4.MyStatus.MaxHealth = 100;
		enemy4.MyStatus.Health = 100;
		*/
		enemySquad.IssueSquadCommand();


		CameraController = GameObject.Find("CameraController").GetComponent<CameraController>();
		CameraController.Initialize();

		CameraShaker = CameraController.GetComponent<CameraShaker>();
		CameraShaker.Initialize();

		FXManager = new FXManager();
		FXManager.Initialize(50);

		AIScheduler = new AIScheduler();
		AIScheduler.Initialize();



		CursorManager = new CursorManager();
		CursorManager.Initialize();

		StartCoroutine(DoPerSecond());
		StartCoroutine(DoPerHalfSecond());
	}



	private void PerSecondUpdate()
	{
		TimerEventHandler.Instance.TriggerOneSecondTimer();
	}

	private void PerHalfSecondUpdate()
	{
		TimerEventHandler.Instance.TriggerHalfSecondTimer();
	}





	#endregion

	#region Coroutines
	IEnumerator DoPerSecond()
	{
		for(;;)
		{
			PerSecondUpdate();
			yield return new WaitForSeconds(1);
		}

	}

	IEnumerator DoPerHalfSecond()
	{
		for(;;)
		{
			PerHalfSecondUpdate();
			yield return new WaitForSeconds(0.5f);
		}

	}


	#endregion
}

