using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DialogueAssetEditor : NodeBasedEditor
{
	
	[MenuItem("Window/Dialogue Asset Editor")]
	private static void OpenWindow()
	{
		DialogueAssetEditor window = GetWindow<DialogueAssetEditor>();
		window.titleContent = new GUIContent("Dialogue Asset Editor");
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnGUI()
	{
		base.OnGUI();
	}

	protected override void ProcessContextMenu(Vector2 mousePosition)
	{
		GenericMenu genericMenu = new GenericMenu();

		genericMenu.AddItem(new GUIContent("Add dialogue node"), false, () => OnClickAddNode(mousePosition));

		genericMenu.ShowAsContext();
	}

	protected override void OnClickAddNode(Vector2 mousePosition)
	{
		if (nodes == null)
		{
			nodes = new List<Node>();
		}

		nodes.Add(new DialogueNode(
				mousePosition,
				200,
				50,
				nodeStyle,
				selectedNodeStyle,
				inPointStyle,
				outPointStyle,
				OnClickInPoint,
				OnClickOutPoint,
				OnClickRemoveNode,
				OnClickSelectNode));
	}
}

public class DialogueNode : Node
{


	public DialogueNode(
		Vector2 position,
		float width,
		float height,
		GUIStyle nodeStyle,
		GUIStyle selectedStyle,
		GUIStyle inPointStyle,
		GUIStyle outPointStyle,
		Action<ConnectionPoint> OnClickInPoint,
		Action<ConnectionPoint> OnClickOutPoint,
		Action<Node> OnClickRemoveNode,
		Action<Node> OnClickSelectNode)
		: base(position, width, height, nodeStyle, selectedStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode)
	{

	}

	protected override void ProcessContextMenu()
	{
		GenericMenu genericMenu = new GenericMenu();

		genericMenu.AddItem(new GUIContent("Remove dialogue node"), false, OnClickRemoveNode);

		genericMenu.ShowAsContext();
	}
}
