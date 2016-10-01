using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using System.Data;
using System.Text;
using System.Xml;
using System.IO;

public class DBHandlerDialogue 
{
	public XmlDocument CurrentDialogXML;

	public List<Topic> GetPlayerTopics()
	{
		List<Topic> topics = new List<Topic>();

		topics.Add(new Topic("1", "This place", TopicType.Info));
		topics.Add(new Topic("2", "Life in the Zone", TopicType.Info));

		return topics;
	}

	public List<Topic> GetNPCTopics(HumanCharacter npc, HumanCharacter initiator)
	{
		List<Topic> topics = new List<Topic>();

		topics.Add(new Topic("3", "Trade", TopicType.Trade));
		topics.Add(new Topic("5", "Change Subject", TopicType.Return));
		topics.Add(new Topic("4", "Goodbye", TopicType.Exit));

		//topics.Add(new Topic("1", "This place", TopicType.Info));
		//topics.Add(new Topic("2", "Life in the Zone", TopicType.Info));

		XmlNodeList topicList = CurrentDialogXML.GetElementsByTagName("topic");

		int tempIndex = 0;

		if(topicList.Count > 0)
		{
			foreach(XmlNode topic in topicList)
			{
				string id = "";
				string response = "";
				string nextNode = "";
				string title = "";

				XmlNodeList nodeContent = topic.ChildNodes;
				foreach(XmlNode nodeItem in nodeContent)
				{
					if(nodeItem.Name == "id")
					{
						id = nodeItem.InnerText;
					}
						
					if(nodeItem.Name == "response")
					{
						response = nodeItem.InnerText;
					}

					if(nodeItem.Name == "next_node")
					{
						nextNode = nodeItem.InnerText;
					}

					if(nodeItem.Name == "title")
					{
						title = nodeItem.InnerText;
					}
						
				}

				if(id == "")
				{
					id = "temp" + tempIndex;
				}
				if(title == "")
				{
					title = GetTopicTitle(id);
				}

				Topic newTopic = new Topic(id, title, TopicType.Info);

				newTopic.Response = response;
				newTopic.NextNode = nextNode;


				topics.Add(newTopic);


				tempIndex ++;
			}
		}

		return topics;
	}

	public string GetTopicTitle(string id)
	{

		if(id == "1")
		{
			return "This place";
		}
		else if(id == "2")
		{
			return "How is life";
		}
		else 
		{
			return "Placeholder";
		}
	}


	public DialogueHandle LoadNPCDialogue(HumanCharacter npc)
	{
		XmlDocument xmlDoc = new XmlDocument();
		string path = Application.dataPath + "/GameData/Dialogue/";
		string file = File.ReadAllText(path + "BaldMan.xml");
		xmlDoc.LoadXml(file);

		CurrentDialogXML = xmlDoc;

		string introText = "";
		string nextNode = "";

		DialogueHandle handle = new DialogueHandle();

		if(GetDialogueIntro(out introText, out nextNode))
		{
			handle.NextNode = nextNode;
			handle.IntroText = introText;

			return handle;
		}

		return null;
	}

	public DialogueNode GetDialogueNode(string id)
	{
		XmlElement node = CurrentDialogXML.GetElementById(id);
		if(node == null)
		{
			return null;
		}

		XmlNodeList nodeContent = node.ChildNodes;
		DialogueNode dialogueNode = new DialogueNode();

		foreach(XmlNode nodeItem in nodeContent)
		{
			if(nodeItem.Name == "response")
			{
				DialogueResponse response = GetDialogueResponse(nodeItem);
				dialogueNode.Responses.Add(response);
			}
			else if(nodeItem.Name == "option")
			{
				Topic option = GetDialogueOption(nodeItem);
				dialogueNode.Options.Add(option);
			}
		}

		return dialogueNode;
	}

	public Topic GetDialogueOption(XmlNode node)
	{
		XmlNodeList nodeContent = node.ChildNodes;
		Topic topic = new Topic();

		XmlAttributeCollection nodeAttributes = node.Attributes;
		if(nodeAttributes["id"] != null)
		{
			topic.ID = nodeAttributes["id"].Value;
		}

		foreach(XmlNode nodeItem in nodeContent)
		{
			if(nodeItem.Name == "condition")
			{
				DialogueCondition condition = new DialogueCondition();
				condition.ID = nodeItem.InnerText;
				if(condition.ID.Length > 0)
				{
					XmlAttributeCollection attributes = nodeItem.Attributes;
					if(attributes["type"] != null)
					{
						if(attributes["type"].Value == "and")
						{
							condition.IsAND = true;
						}
						else
						{
							condition.IsAND = false;
						}
					}
					else
					{
						condition.IsAND = true;
					}
					topic.Conditions.Add(condition);
				}
			}
			else if(nodeItem.Name == "title")
			{
				topic.Title = nodeItem.InnerText;
			}
			else if(nodeItem.Name == "text")
			{
				topic.Request = nodeItem.InnerText;
			}
			else if(nodeItem.Name == "next_node")
			{
				topic.NextNode = nodeItem.InnerText;
			}

		}

		return topic;
	}

	public DialogueResponse GetDialogueResponse(XmlNode node)
	{
		XmlNodeList nodeContent = node.ChildNodes;

		DialogueResponse response = new DialogueResponse();

		foreach(XmlNode nodeItem in nodeContent)
		{
			if(nodeItem.Name == "condition")
			{
				DialogueCondition condition = new DialogueCondition();
				condition.ID = nodeItem.InnerText;
				if(condition.ID.Length > 0)
				{
					XmlAttributeCollection attributes = nodeItem.Attributes;
					if(attributes["type"] != null)
					{
						if(attributes["type"].Value == "and")
						{
							condition.IsAND = true;
						}
						else
						{
							condition.IsAND = false;
						}
					}
					else
					{
						condition.IsAND = true;
					}
					response.Conditions.Add(condition);
				}
			}
			else if(nodeItem.Name == "text")
			{
				response.Text = nodeItem.InnerText;
			}
			else if(nodeItem.Name == "event")
			{
				response.Events.Add(nodeItem.InnerText);
			}
		}

		return response;
	}

	public string GetGlobalResponse(string id)
	{
		IDataReader reader = GameManager.Inst.DBManager.RunQuery(
			"SELECT response FROM global_dialogue_response WHERE id = '" + id + "'");

		string output = "";

		while(reader.Read())
		{
			output = reader.GetString(0);
		}

		return output;
	}


	public bool GetDialogueIntro(out string text, out string nextNode)
	{
		text = "";
		nextNode = "";

		XmlNodeList intros = CurrentDialogXML.GetElementsByTagName("intro");

		if(intros.Count <= 0)
		{
			
			return false;
		}

		foreach(XmlNode intro in intros)
		{
			XmlNodeList nodeContent = intro.ChildNodes;
			foreach(XmlNode nodeItem in nodeContent)
			{
				if(nodeItem.Name == "text")
				{
					Debug.Log(nodeItem.InnerText);
					text = nodeItem.InnerText;
				}

				if(nodeItem.Name == "next_node")
				{
					nextNode = nodeItem.InnerText;
				}
			}
		}

		return true;
	}
}
