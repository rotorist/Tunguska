using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueNode
{
	public List<DialogueResponse> Responses;
	public List<Topic> Options;

	public DialogueNode()
	{
		Responses = new List<DialogueResponse>();
		Options = new List<Topic>();
	}
}

public class DialogueCondition
{
	public string ID;
	public bool IsAND; //true = AND


}

public class DialogueResponse
{
	public List<DialogueCondition> Conditions;
	public List<string> Events;
	public string Text;

	public DialogueResponse()
	{
		Conditions = new List<DialogueCondition>();
		Events = new List<string>();
	}
}



public class DialogueHandle
{
	public string IntroText;
	public string NextNode;
}