using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;

public class DialogueAssetEditor : GraphViewEditorWindow
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

		// create toolbar
		var toolbar = new Toolbar();
		toolbar.Add(new ToolbarButton(() => { }) { text = "Save" });
		rootVisualElement.Add(toolbar);
	}

	// called when the object leaves scope
	private void OnDisable()
	{
		if(graphView != null)
		{
			rootVisualElement.Remove(graphView);
		}
	}

	// implement custom editor GUI code here
	private void OnGUI()
	{

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
}

public class DialogueNode : Node
{
	public DialogueNode()
	{

	}
}
