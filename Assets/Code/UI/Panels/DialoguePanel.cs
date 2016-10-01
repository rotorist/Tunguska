﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DialoguePanel : PanelBase
{
	public UIScrollView DialogueScroll;
	public UIScrollView TopicScroll;
	public UIScrollView DialogueOptionScroll;
	public UISprite TopicDivider;


	public struct DialogueEntry
	{
		public UILabel SpeakerName;
		public UILabel Text;
	}

	public struct TopicEntry
	{
		public UILabel Text;
	}


	public struct DialogueOptionEntry
	{
		public UILabel Text;
		//public Topic Option;
	}


	private string _rootNode;
	private string _currentNodeID;
	private UILabel _intro;

	private Stack<DialogueEntry> _entries;
	private List<TopicEntry> _topics;
	private List<DialogueOptionEntry> _options;



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

		if(_entries == null)
		{
			_entries = new Stack<DialogueEntry>();
		}

		if(_options == null)
		{
			_options = new List<DialogueOptionEntry>();
		}

		if(_topics == null)
		{
			_topics = new List<TopicEntry>();
		}

		InputEventHandler.Instance.State = UserInputState.Dialogue;

		DialogueHandle handle = GameManager.Inst.DBManager.DBHandlerDialogue.LoadNPCDialogue(null);
		ClearDialogue();
		ClearTopics();
		LoadIntro(handle.IntroText);
		_currentNodeID = handle.NextNode;
		_rootNode = handle.NextNode;


		RefreshDialogue(_currentNodeID, true);


	}

	public override void Hide ()
	{
		NGUITools.SetActive(this.gameObject, false);
		this.IsActive = false;

		UIEventHandler.Instance.TriggerCloseWindow();

		InputEventHandler.Instance.State = UserInputState.Normal;
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

	public void OnSelectTopic()
	{
		UIButton selectedButton = UIButton.current;
		Debug.Log("clicked " + selectedButton.GetComponent<TopicReference>().Topic.Title);

		Topic selectedTopic = selectedButton.GetComponent<TopicReference>().Topic;

		if(selectedTopic.Type == TopicType.Info && selectedTopic.Response != null && selectedTopic.Response != "")
		{
			//we have an immediate response. 
			DialogueEntry request = CreateDialogueEntry("Gabriel Chang", selectedTopic.Title, true);
			_entries.Push(request);

			string parsedResponse = ParseDialogueText(selectedTopic.Response);
			DialogueEntry entry = CreateDialogueEntry("Jonathan Perpy", parsedResponse, false);
			_entries.Push(entry);
			ClearTopics();
			RefreshDialogue(_currentNodeID, false);
		}
		else if(selectedTopic.Type == TopicType.Info && selectedTopic.NextNode != null && selectedTopic.NextNode != "")
		{
			//we are going to a new node
			//check if there's request text
			if(selectedTopic.Request != null && selectedTopic.Request != "")
			{
				DialogueEntry entry = CreateDialogueEntry("Gabriel Chang", selectedTopic.Request, true);
				_entries.Push(entry);
			}
			else
			{
				DialogueEntry entry = CreateDialogueEntry("Gabriel Chang", selectedTopic.Title, true);
				_entries.Push(entry);
			}

			ClearTopics();
			_currentNodeID = selectedTopic.NextNode;
			RefreshDialogue(selectedTopic.NextNode, true);
		}
		else if(selectedTopic.Type == TopicType.Return)
		{
			//back to root node

			DialogueEntry entry = CreateDialogueEntry("Gabriel Chang", "Let's talk about something else.", true);
			_entries.Push(entry);

			ClearTopics();
			_currentNodeID = _rootNode;
			RefreshDialogue(_rootNode, true);
		}
		else if(selectedTopic.Type == TopicType.Exit)
		{
			ClearDialogue();
			ClearTopics();
			Hide();
		}
	}

	public void ClearDialogue()
	{
		if(_entries.Count > 0)
		{
			foreach(DialogueEntry entry in _entries)
			{
				if(entry.SpeakerName != null)
				{
					GameObject.Destroy(entry.SpeakerName.gameObject);
				}
				if(entry.Text != null)
				{
					GameObject.Destroy(entry.Text.gameObject);
				}
			}
		}

		if(_intro != null)
		{
			GameObject.Destroy(_intro.gameObject);
		}

		_entries = new Stack<DialogueEntry>();
		_intro = null;
	}

	public void ClearTopics()
	{




		if(_topics.Count > 0)
		{
			foreach(TopicEntry entry in _topics)
			{
				if(entry.Text != null)
				{
					GameObject.Destroy(entry.Text.gameObject);
				}
			}
		}


		if(_options.Count > 0)
		{
			foreach(DialogueOptionEntry entry in _options)
			{
				if(entry.Text != null)
				{
					GameObject.Destroy(entry.Text.gameObject);
				}
			}
		}


		//
		_topics = new List<TopicEntry>();
		_options = new List<DialogueOptionEntry>();

	}

	public void LoadIntro(string text)
	{
		GameObject o = GameObject.Instantiate(Resources.Load("IntroBox")) as GameObject;
		_intro = o.GetComponent<UILabel>();
		_intro.text = text;
	}

	public void RefreshDialogue(string newNodeID, bool showResponse)
	{
		//if loading a new node, first create a dialogue entry for it and push into 
		//the stack. 
		if(newNodeID != "")
		{
			//create dialogue entry

			DialogueNode node = GameManager.Inst.DBManager.DBHandlerDialogue.GetDialogueNode(newNodeID);

			if(showResponse)
			{
				DialogueResponse response = EvaluateResponse(node.Responses);
				if(response != null)
				{
					DialogueEntry entry = CreateDialogueEntry("Jonathan Perpy", ParseDialogueText(response.Text), false);

					_entries.Push(entry);
				}
			}

			//create dialogue options relevant to this node
			foreach(Topic option in node.Options)
			{
				if(EvaluateTopicConditions(option.Conditions))
				{
					DialogueOptionEntry optionEntry = new DialogueOptionEntry();

					GameObject o = GameObject.Instantiate(Resources.Load("TopicEntry")) as GameObject;
					optionEntry.Text = o.GetComponent<UILabel>();
					optionEntry.Text.text = option.Title;
					TopicReference reference = o.GetComponent<TopicReference>();
					reference.Topic = option;
					UIButton button = o.GetComponent<UIButton>();
					button.onClick.Add(new EventDelegate(this, "OnSelectTopic"));
					_options.Add(optionEntry);
				}
			}

		}

		//create topics entry
		List<Topic> topics = GameManager.Inst.DBManager.DBHandlerDialogue.GetNPCTopics(null, null);
		foreach(Topic topic in topics)
		{
			//if we are not at root node then don't show info topics
			if(topic.Type == TopicType.Info && _currentNodeID != _rootNode)
			{
				continue;
			}

			TopicEntry entry = new TopicEntry();

			GameObject o = GameObject.Instantiate(Resources.Load("TopicEntry")) as GameObject;
			entry.Text = o.GetComponent<UILabel>();
			entry.Text.text = topic.Title;
			TopicReference reference = o.GetComponent<TopicReference>();
			reference.Topic = topic;
			UIButton button = o.GetComponent<UIButton>();
			button.onClick.Add(new EventDelegate(this, "OnSelectTopic"));

			_topics.Add(entry);
		}

		//now make a copy of the stack, and start popping entries out of it, and arrange them under the dialog panel
		Stack<DialogueEntry> copy = new Stack<DialogueEntry>(_entries.Reverse());

		float currentY = -260;
		while(copy.Count > 0)
		{
			DialogueEntry entry = copy.Pop();
			entry.SpeakerName.transform.parent = DialogueScroll.transform;
			entry.SpeakerName.MakePixelPerfect();
			entry.Text.transform.parent = DialogueScroll.transform;
			entry.Text.MakePixelPerfect();

			float height = Mathf.Max(entry.SpeakerName.height * 1f, entry.Text.height * 1f);

			entry.SpeakerName.transform.localPosition = new Vector3(-150, currentY + height, 0);
			entry.Text.transform.localPosition = new Vector3(22, currentY + height, 0);

			currentY = currentY + height + 15;
		}
		//now re-add collider to fix collider size
		NGUITools.AddWidgetCollider(DialogueScroll.gameObject);


		if(_intro != null)
		{
			_intro.transform.parent = DialogueScroll.transform;
			_intro.MakePixelPerfect();
			_intro.transform.localPosition = new Vector3(-150, currentY + _intro.height, 0);
		}

		//arrange topic entries
		currentY = 85;
		foreach(TopicEntry entry in _topics)
		{
			entry.Text.transform.parent = TopicScroll.transform;
			entry.Text.MakePixelPerfect();
			entry.Text.transform.localPosition = new Vector3(-122, currentY, 0);

			currentY = currentY - entry.Text.height;

			if(entry.Text.text == "Goodbye")
			{
				currentY = currentY - 4;
				TopicDivider.transform.localPosition = new Vector3(8, currentY, 0);
				currentY = currentY - 8;
			}


		}

		float currentX = -128;
		foreach(DialogueOptionEntry entry in _options)
		{
			entry.Text.transform.parent = DialogueOptionScroll.transform;
			entry.Text.MakePixelPerfect();
			entry.Text.width = entry.Text.text.Length * 15;
			entry.Text.transform.localPosition = new Vector3(currentX, 78, 0);

			currentX = currentX + entry.Text.width;
		}

		//now re-add collider to fix collider size
		NGUITools.AddWidgetCollider(TopicScroll.gameObject);

	}




	private DialogueEntry CreateDialogueEntry(string speaker, string text, bool isRequest)
	{
		DialogueEntry entry = new DialogueEntry();
		GameObject o = GameObject.Instantiate(Resources.Load("NameBox")) as GameObject;
		entry.SpeakerName = o.GetComponent<UILabel>();
		entry.SpeakerName.text = speaker;

		o = GameObject.Instantiate(Resources.Load("DialogueBox")) as GameObject;
		entry.Text = o.GetComponent<UILabel>();
		entry.Text.text = text;

		if(isRequest)
		{
			entry.SpeakerName.color = new Color(0.3f, 0.8f, 0.3f);
		}

		return entry;
	}

	private DialogueResponse EvaluateResponse(List<DialogueResponse> responses)
	{
		foreach(DialogueResponse response in responses)
		{
			bool result = true;

			if(response.Conditions.Count > 0)
			{
				Debug.Log("condition count " + response.Conditions.Count);
				//evaluate each condition of each response
				foreach(DialogueCondition condition in response.Conditions)
				{
					bool tempResult = EvaluateCondition(condition);
					if(condition.IsAND)
					{
						result = result && tempResult;
					}
					else
					{
						result = result || tempResult;
					}
				}
			}

			if(result)
			{
				return response;
			}
		}

		return null;
	}

	private bool EvaluateTopicConditions(List<DialogueCondition> conditions)
	{
		bool result = true;

		if(conditions.Count <= 0)
		{
			return true;
		}

		foreach(DialogueCondition condition in conditions)
		{
			bool tempResult = EvaluateCondition(condition);
			if(condition.IsAND)
			{
				result = result && tempResult;
			}
			else
			{
				result = result || tempResult;
			}
		}


		return result;
	}


	private bool EvaluateCondition(DialogueCondition condition)
	{
		if(condition.ID == "duringgreeting")
		{
			if(_entries.Count <= 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else if(condition.ID == "isawake")
		{
			return true;
		}
		else if(condition.ID == "hastomatoseeds")
		{
			return true;
		}
		else if(condition.ID == "hasnotomatoseeds")
		{
			return false;
		}

		return true;
	}


	private string ParseDialogueText(string input)
	{
		char [] chars = input.ToCharArray();
		string output = "";
		string temp = "";

		ParseStates state = ParseStates.Normal;

		for(int i=0; i<chars.Length; i++)
		{
			if(chars[i] == '{' && state == ParseStates.Normal)
			{
				state = ParseStates.Response;
				temp = "";
			}
			else if(chars[i] == '}' && state == ParseStates.Response)
			{
				state = ParseStates.Normal;
				string response = GameManager.Inst.DBManager.DBHandlerDialogue.GetGlobalResponse(temp);
				output = output + response;
			}
			else if(state == ParseStates.Response)
			{
				temp = temp + chars[i];
			}
			else if(state == ParseStates.Normal)
			{
				output = output + chars[i];
			}
		}

		return output;
	}
}
