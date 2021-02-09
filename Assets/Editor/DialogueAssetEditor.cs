using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;

namespace CustomGraphEditors.DialogueSystem
{
	public class DialogueAssetEditor : GraphViewEditorWindow, ISearchWindowProvider
	{
		public DialogueGraphAsset graphAsset { get; set; }
		private DialogueGraphView graphView;


		[OnOpenAsset(1)]
		private static bool OpenWindow(int instanceId, int line)
		{
			var obj = EditorUtility.InstanceIDToObject(instanceId);

			if (obj is DialogueGraphAsset)
			{
				DialogueAssetEditor window = GetWindow<DialogueAssetEditor>();
				window.titleContent = new GUIContent("Dialogue Asset Editor");
				window.graphAsset = obj as DialogueGraphAsset;
				window.minSize = new Vector2(500f, 300f);

				return true;
			}

			return false;
		}

		// called when the object is loaded
		private void OnEnable()
		{
			// create graph view
			graphView = new DialogueGraphView { name = "DialogueGraph" };
			graphView.StretchToParentSize();
			rootVisualElement.Add(graphView);
			graphView.nodeCreationRequest += OnRequestNodeCreation;
		}

		// called when the object leaves scope
		private void OnDisable()
		{

		}

		// implement custom editor GUI code here
		private void OnGUI()
		{

		}

		protected void OnRequestNodeCreation(NodeCreationContext context)
		{
			SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), this);
		}

		public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
		{
			var tree = new List<SearchTreeEntry>();
			tree.Add(new SearchTreeGroupEntry(new GUIContent("Create Node"), 0));

			Texture2D icon = EditorGUIUtility.FindTexture("cs Script Icon");

			tree.Add(new SearchTreeGroupEntry(new GUIContent("Dialogue", icon)) { level = 1 });
			tree.Add(new SearchTreeEntry(new GUIContent("Basic Dialogue Node", icon)) { level = 2 });
			tree.Add(new SearchTreeEntry(new GUIContent("Advanced Dialogue Node", icon)) { level = 2 });
			tree.Add(new SearchTreeEntry(new GUIContent("Cinematic Dialogue Node", icon)) { level = 2 });

			tree.Add(new SearchTreeGroupEntry(new GUIContent("Boolean", icon)) { level = 1 });
			tree.Add(new SearchTreeEntry(new GUIContent("Boolean Node (Int)", icon)) { level = 2 });
			tree.Add(new SearchTreeEntry(new GUIContent("Boolean Node (Float)", icon)) { level = 2 });

			return tree;
		}

		public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
		{
			switch (entry.name)
			{
				case "Basic Dialogue Node":
				{
					var node = new DialogueGraphNode();
					graphView.AddElement(node);

					Vector2 pointInWindow = context.screenMousePosition - position.position;
					Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

					node.SetPosition(new Rect(pointInGraph, Vector2.zero));

					node.Select(graphView, false);

					return true;
				}

				case "Advanced Dialogue Node":
				{
					var node = new AdvDialogueNode();
					graphView.AddElement(node);

					Vector2 pointInWindow = context.screenMousePosition - position.position;
					Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

					node.SetPosition(new Rect(pointInGraph, Vector2.zero));

					node.Select(graphView, false);

					return true;
				}

				case "Cinematic Dialogue Node":
				{
					var node = new CinematicDialogueNode();
					graphView.AddElement(node);

					Vector2 pointInWindow = context.screenMousePosition - position.position;
					Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

					node.SetPosition(new Rect(pointInGraph, Vector2.zero));

					node.Select(graphView, false);

					return true;
				}

				case "Boolean Node (Int)":
				{
					var node = new IntBooleanNode();
					graphView.AddElement(node);

					Vector2 pointInWindow = context.screenMousePosition - position.position;
					Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

					node.SetPosition(new Rect(pointInGraph, Vector2.zero));

					node.Select(graphView, false);

					return true;
				}

				case "Boolean Node (Float)":
				{
					var node = new FloatBooleanNode();
					graphView.AddElement(node);

					Vector2 pointInWindow = context.screenMousePosition - position.position;
					Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

					node.SetPosition(new Rect(pointInGraph, Vector2.zero));

					node.Select(graphView, false);

					return true;
				}
			}

			return false;
		}
	}

	public class DialogueGraphView : GraphView
	{
		public DialogueGraphView()
		{
			styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphStyle"));

			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());
			this.AddManipulator(new FreehandSelector());

			var grid = new GridBackground { name = "GridBackground" };
			Insert(0, grid);
		}

		// overriden to allow port connections
		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			return ports.ToList().Where(port => port.direction != startPort.direction && port.node != startPort.node).ToList();
		}
	}
}
