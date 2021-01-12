using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeBasedEditor : EditorWindow
{
	protected List<Node> nodes;
	protected List<Connection> connections;

	protected GUIStyle nodeStyle;
	protected GUIStyle selectedNodeStyle;
	protected GUIStyle inPointStyle;
	protected GUIStyle outPointStyle;

	protected ConnectionPoint selectedInPoint;
	protected ConnectionPoint selectedOutPoint;

	private Node selectedNode;
	protected Connection selectedConnection;

	protected Vector2 offset;
	protected Vector2 drag;


	protected virtual void OnEnable()
	{
		string editorSkin = EditorGUIUtility.isProSkin ? "darkskin" : "lightskin";

		nodeStyle = new GUIStyle();
		nodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/" + editorSkin + "/images/node1.png") as Texture2D;
		nodeStyle.border = new RectOffset(12, 12, 12, 12);

		selectedNodeStyle = new GUIStyle();
		selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/" + editorSkin + "/images/node1 on.png") as Texture2D;
		selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

		inPointStyle = new GUIStyle();
		inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/" + editorSkin + "/images/btn left.png") as Texture2D;
		inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/" + editorSkin + "/images/btn left on.png") as Texture2D;
		inPointStyle.border = new RectOffset(4, 4, 12, 12);

		outPointStyle = new GUIStyle();
		outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/" + editorSkin + "/images/btn right.png") as Texture2D;
		outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/" + editorSkin + "/images/btn right on.png") as Texture2D;
		outPointStyle.border = new RectOffset(4, 4, 12, 12);
	}

	protected virtual void OnGUI()
	{
		DrawGrid(20, 0.2f, Color.gray);
		DrawGrid(100, 0.4f, Color.gray);

		DrawNodes();
		DrawConnections();

		ProcessNodeEvents(Event.current);
		ProcessConnectionEvents(Event.current);
		ProcessEvents(Event.current);

		DrawConnectionLineDragging(Event.current);

		if (GUI.changed) Repaint();
	}

	protected void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
	{
		int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
		int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

		Handles.BeginGUI();
		Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

		offset += drag * 0.5f;
		Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

		for (int i = 0; i < widthDivs; i++)
		{
			Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
		}

		for (int j = 0; j < heightDivs; j++)
		{
			Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
		}

		Handles.color = Color.white;
		Handles.EndGUI();
	}

	protected virtual void DrawNodes()
	{
		if (nodes != null)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].Draw();
			}
		}
	}

	protected void DrawConnections()
	{
		if (connections != null)
		{
			for (int i = 0; i < connections.Count; i++)
			{
				connections[i].Draw();
			}
		}
	}

	protected void ProcessEvents(Event e)
	{
		drag = Vector2.zero;

		switch (e.type)
		{
			case EventType.MouseDown:
			{
				if (e.button == 0)
				{
					ClearConnectionSelection();
				}

				if (e.button == 1)
				{
					ProcessContextMenu(e.mousePosition);
				}

				break;
			}

			case EventType.MouseDrag:
			{
				if (e.button == 0)
				{
					OnDrag(e.delta);
				}

				break;
			}
		}
	}

	protected virtual void ProcessNodeEvents(Event e)
	{
		if (nodes != null)
		{
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				if (nodes[i].ProcessEvents(e))
				{
					GUI.changed = true;
				}
			}
		}
	}

	protected void ProcessConnectionEvents(Event e)
	{
		if (connections != null)
		{
			for (int i = connections.Count - 1; i >= 0; i--)
			{
				if (connections[i].ProcessEvents(e))
				{
					GUI.changed = true;
				}
			}
		}
	}

	protected virtual void ProcessContextMenu(Vector2 mousePosition)
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
		genericMenu.ShowAsContext();
	}

	protected virtual void OnClickAddNode(Vector2 mousePosition)
	{
		if (nodes == null)
		{
			nodes = new List<Node>();
		}

		nodes.Add(new Node(mousePosition, 200, 50, nodeStyle, selectedNodeStyle, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickSelectNode));
	}

	protected void OnClickSelectNode(Node node)
	{
		if (selectedNode != null)
		{
			selectedNode.isSelected = false;
		}

		if (selectedConnection != null)
		{
			selectedConnection.isSelected = false;
			selectedConnection = null;
		}

		selectedNode = node;
		selectedNode.isSelected = true;

		GUI.changed = true;
		Repaint();
	}

	protected void OnClickInPoint(ConnectionPoint inPoint)
	{
		selectedInPoint = inPoint;

		if (selectedOutPoint != null)
		{
			if (selectedOutPoint.node != selectedInPoint.node)
			{
				CreateConnection();
				ClearConnectionSelection();
			}
			else
			{
				ClearConnectionSelection();
			}
		}
	}

	protected void OnClickOutPoint(ConnectionPoint outPoint)
	{
		selectedOutPoint = outPoint;

		if (selectedInPoint != null)
		{
			if (selectedOutPoint.node != selectedInPoint.node)
			{
				CreateConnection();
				ClearConnectionSelection();
			}
			else
			{
				ClearConnectionSelection();
			}
		}
	}

	protected void OnClickRemoveConnection(Connection connection)
	{
		connections.Remove(connection);
	}

	protected void OnClickSelectConnection(Connection connection)
	{
		if (selectedConnection != null)
		{
			selectedConnection.isSelected = false;
		}

		if (selectedNode != null)
		{
			selectedNode.isSelected = false;
			selectedNode = null;
		}

		selectedConnection = connection;
		selectedConnection.isSelected = true;

		GUI.changed = true;
		Repaint();
	}

	protected void OnClickRemoveNode(Node node)
	{
		if (connections != null)
		{
			List<Connection> connectionsToRemove = new List<Connection>();

			for (int i = 0; i < connections.Count; i++)
			{
				if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
				{
					connectionsToRemove.Add(connections[i]);
				}
			}

			for (int i = 0; i < connectionsToRemove.Count; i++)
			{
				connections.Remove(connectionsToRemove[i]);
			}

			connectionsToRemove = null;
		}

		nodes.Remove(node);
	}

	protected void CreateConnection()
	{
		if (connections == null)
		{
			connections = new List<Connection>();
		}

		connections.Add(new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection, OnClickSelectConnection));
	}

	protected void ClearConnectionSelection()
	{
		selectedInPoint = null;
		selectedOutPoint = null;
	}

	protected void DrawConnectionLineDragging(Event e)
	{
		if (selectedInPoint != null && selectedOutPoint == null)
		{
			Handles.DrawBezier(
				selectedInPoint.rect.center,
				e.mousePosition,
				selectedInPoint.rect.center + Vector2.left * 50f,
				e.mousePosition - Vector2.left * 50f,
				Color.yellow,
				null,
				2f
			);

			GUI.changed = true;
		}
		else if (selectedOutPoint != null && selectedInPoint == null)
		{
			Handles.DrawBezier(
				selectedOutPoint.rect.center,
				e.mousePosition,
				selectedOutPoint.rect.center - Vector2.left * 50f,
				e.mousePosition + Vector2.left * 50f,
				Color.yellow,
				null,
				2f
			);

			GUI.changed = true;
		}
	}

	protected void OnDrag(Vector2 delta)
	{
		drag = delta;

		if (nodes != null)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				nodes[i].Drag(delta);
			}
		}

		GUI.changed = true;
	}
}

public class Node
{
	public Rect rect;
	public string title;
	public bool isDragged;
	public bool isSelected;

	public ConnectionPoint inPoint;
	public ConnectionPoint outPoint;

	public GUIStyle defaultNodeStyle;
	public GUIStyle selectedNodeStyle;

	public Action<Node> OnRemoveNode;
	public Action<Node> OnSelectNode;


	public Node(
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
	{
		rect = new Rect(position.x, position.y, width, height);

		inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
		outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);

		isSelected = isDragged = false;

		defaultNodeStyle = nodeStyle;
		selectedNodeStyle = selectedStyle;

		OnRemoveNode = OnClickRemoveNode;
		OnSelectNode = OnClickSelectNode;
	}

	public void Drag(Vector2 delta)
	{
		rect.position += delta;
	}

	public virtual void Draw()
	{
		inPoint.Draw();
		outPoint.Draw();
		GUI.Box(rect, title, isSelected ? selectedNodeStyle : defaultNodeStyle);
	}

	public bool ProcessEvents(Event e)
	{
		bool eventProcessed = false;

		switch (e.type)
		{
			case EventType.MouseDown:
			{
				if (e.button == 0)
				{
					bool clickedInsideNode = rect.Contains(e.mousePosition);

					isSelected = clickedInsideNode;

					if (clickedInsideNode)
					{
						isDragged = true;

						OnClickSelectNode();
						e.Use();
					}

					eventProcessed = true;
				}
				else if (e.button == 1 && rect.Contains(e.mousePosition))
				{
					ProcessContextMenu();
					e.Use();
					eventProcessed = true;
				}

				break;
			}

			case EventType.MouseUp:
			{
				isDragged = false;

				eventProcessed = true;

				break;
			}

			case EventType.MouseDrag:
			{
				if (e.button == 0 && isDragged)
				{
					Drag(e.delta);
					e.Use();
					eventProcessed = true;
				}

				break;
			}

			case EventType.KeyDown:
			{
				if (isSelected && !isDragged && e.keyCode == KeyCode.Delete)
				{
					isSelected = false;
					OnClickRemoveNode();
					eventProcessed = true;
				}

				break;
			}
		}

		return eventProcessed;
	}

	protected virtual void ProcessContextMenu()
	{
		GenericMenu genericMenu = new GenericMenu();
		genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
		genericMenu.ShowAsContext();
	}

	protected void OnClickRemoveNode()
	{
		OnRemoveNode?.Invoke(this);
	}

	protected void OnClickSelectNode()
	{
		OnSelectNode?.Invoke(this);
	}
}

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
	public Rect rect;
	public ConnectionPointType type;
	public Node node;
	public GUIStyle style;
	public Action<ConnectionPoint> OnClickConnectionPoint;

	public ConnectionPoint(
		Node node, 
		ConnectionPointType type, 
		GUIStyle style, 
		Action<ConnectionPoint> OnClickConnectionPoint)
	{
		this.node = node;
		this.type = type;
		this.style = style;
		this.OnClickConnectionPoint = OnClickConnectionPoint;
		rect = new Rect(0, 0, 10f, 20f);
	}

	public void Draw()
	{
		rect.y = node.rect.y + (node.rect.height * 0.5f) - rect.height * 0.5f;

		switch (type)
		{
			case ConnectionPointType.In:
				rect.x = node.rect.x - rect.width + 8f;
				break;

			case ConnectionPointType.Out:
				rect.x = node.rect.x + node.rect.width - 8f;
				break;
		}

		if (GUI.Button(rect, "", style))
		{
			OnClickConnectionPoint?.Invoke(this);
		}
	}
}

public class Connection
{
	public ConnectionPoint inPoint;
	public ConnectionPoint outPoint;
	public Action<Connection> OnClickRemoveConnection;
	public Action<Connection> OnClickSelectConnection;
	public bool isSelected;

	public Connection(
		ConnectionPoint inPoint, 
		ConnectionPoint outPoint, 
		Action<Connection> OnClickRemoveConnection, 
		Action<Connection> OnClickSelectConnection)
	{
		this.inPoint = inPoint;
		this.outPoint = outPoint;
		this.OnClickRemoveConnection = OnClickRemoveConnection;
		this.OnClickSelectConnection = OnClickSelectConnection;
		isSelected = false;
	}

	public void Draw()
	{
		Color color = EditorGUIUtility.isProSkin ? Color.white : Color.blue;

		Handles.DrawBezier(
			inPoint.rect.center,
			outPoint.rect.center,
			inPoint.rect.center + Vector2.left * 50f,
			outPoint.rect.center - Vector2.left * 50f,
			color,
			null,
			isSelected ? 5f : 2f
		);
	}

	public bool ProcessEvents(Event e)
	{
		bool eventProcessed = false;

		switch (e.type)
		{
			case EventType.MouseDown:
			{
				isSelected = IsMouseOverConnection(e.mousePosition);

				if (isSelected)
				{
					OnClickSelectConnection?.Invoke(this);
				}

				eventProcessed = true;

				break;
			}

			case EventType.KeyDown:
			{
				if (e.keyCode == KeyCode.Delete && isSelected)
				{
					OnClickRemoveConnection?.Invoke(this);
				}

				eventProcessed = true;

				break;
			}
		}

		return eventProcessed;
	}

	private bool IsMouseOverConnection(Vector2 mousePos)
	{
		Vector2 p1 = inPoint.rect.center;
		Vector2 p2 = outPoint.rect.center;
		Vector2 closestPoint = mousePos;

		float slope = (p2.y - p1.y) / (p2.x - p1.x);
		closestPoint.y = (slope * mousePos.x) - (slope * p1.x) + p1.y;

		float distanceFromClosest = (closestPoint - mousePos).sqrMagnitude;

		return (distanceFromClosest <= 100f);
	}
}