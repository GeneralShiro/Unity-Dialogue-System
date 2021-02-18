using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Callbacks;

using CustomSystem;
using CustomSystem.DialogueSystem;

namespace CustomEditors.DialogueSystem
{
    public class DialogueAssetEditor : GraphViewEditorWindow, ISearchWindowProvider
    {
        public DialogueGraphAsset graphAsset { get; set; }
        private DialogueGraphView graphView;
        private Label noAssetSelectedLabel;
        private bool isLoadingAsset;


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
                window.LoadGraphAsset("OpenWindow");

                return true;
            }

            return false;
        }

        [MenuItem("Window/Custom Editors/Dialogue Editor")]
        private static void OpenWindow()
        {
            GetWindow<DialogueAssetEditor>("Dialogue Asset Editor", true, typeof(SceneView));
        }

        // called when the object is loaded
        private void OnEnable()
        {
            rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraphStyle"));

            // create graph view
            graphView = new DialogueGraphView { name = "DialogueGraph" };
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
            graphView.nodeCreationRequest += OnRequestNodeCreation;
            graphView.graphViewChanged += OnGraphViewChanged;

            // create warning message for user if they haven't selected a dialogue asset first
            noAssetSelectedLabel = new Label("Select a Dialogue Asset to see its graph!");
            noAssetSelectedLabel.name = "NoAssetSelectLabel";
            noAssetSelectedLabel.StretchToParentSize();
            rootVisualElement.Add(noAssetSelectedLabel);
            noAssetSelectedLabel.visible = false;

            isLoadingAsset = false;

            OnSelectionChange();
            Selection.selectionChanged += OnSelectionChange;
        }

        // called when the object leaves scope
        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChange;
        }

        private void OnLostFocus()
        {
            SaveGraphAsset();
        }

        private void OnSelectionChange()
        {
            DialogueGraphAsset[] selectedAssets = Selection.GetFiltered<DialogueGraphAsset>(SelectionMode.Assets);

            if (selectedAssets.Length != 1)
            {
                ClearGraph();
                graphAsset = null;
                return;
            }

            graphAsset = selectedAssets[0];

            if (!isLoadingAsset)
            {
                LoadGraphAsset("OnSelectionChange");
            }
        }

        private void ClearGraph()
        {
            // hide the graph view so it can't be interacted with
            graphView.visible = false;
            noAssetSelectedLabel.visible = true;

            graphView.ClearGraphNodes();
        }

        public void SaveGraphAsset()
        {
            if (graphAsset == null || isLoadingAsset)
                return;

            DialogueGraphAsset assetData = ScriptableObject.CreateInstance<DialogueGraphAsset>();

            List<Node> nodes = graphView.nodes.ToList();

            // copy node data into a serializable class
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                if (node is DialogueGraphNode)
                {
                    if (node is AdvDialogueNode)
                    {
                        AdvDialogueNode castNode = node as AdvDialogueNode;
                        AdvDialogueNodeData nodeData = new AdvDialogueNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "AdvDialogueNode";

                        // basic dialogue node data
                        nodeData._speakerName = castNode.speakerTextField.value;
                        nodeData._dialogueText = castNode.dialogueTextField.value;

                        // store IDs for condition input ports; needed to reconnect ports on load
                        for (int j = 0; j < castNode.conditionIds.Count; j++)
                        {
                            nodeData._conditionPortIds.Add(castNode.conditionIds[j]);
                        }

                        // store IDs for choice output ports; needed to reconnect ports on load
                        for (int j = 0; j < castNode.choicePorts.Count; j++)
                        {
                            DialogueNodeData.ChoicePortData choiceData = new DialogueNodeData.ChoicePortData();
                            choiceData._portId = castNode.choicePorts[j].id;
                            choiceData._choiceText = castNode.choicePorts[j].choiceText.value;
                            nodeData._choicePorts.Add(choiceData);
                        }

                        // advanced dialogue node data
                        nodeData._cameraPos = castNode.cameraPosField.value;
                        nodeData._cameraRot = castNode.cameraRotField.value;

                        assetData.advDialogueNodeData.Add(nodeData);
                    }
                    else if (node is CinematicDialogueNode)
                    {
                        CinematicDialogueNode castNode = node as CinematicDialogueNode;
                        CinematicDialogueNodeData nodeData = new CinematicDialogueNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "CinematicDialogueNode";

                        // basic dialogue node data
                        nodeData._speakerName = castNode.speakerTextField.value;
                        nodeData._dialogueText = castNode.dialogueTextField.value;

                        // store IDs for condition input ports; needed to reconnect ports on load
                        for (int j = 0; j < castNode.conditionIds.Count; j++)
                        {
                            nodeData._conditionPortIds.Add(castNode.conditionIds[j]);
                        }

                        // store IDs for choice output ports; needed to reconnect ports on load
                        for (int j = 0; j < castNode.choicePorts.Count; j++)
                        {
                            DialogueNodeData.ChoicePortData choiceData = new DialogueNodeData.ChoicePortData();
                            choiceData._portId = castNode.choicePorts[j].id;
                            choiceData._choiceText = castNode.choicePorts[j].choiceText.value;
                            nodeData._choicePorts.Add(choiceData);
                        }

                        // cinematic node data
                        nodeData._timelineAsset = castNode.timelineField.value as TimelineAsset;

                        assetData.cinematicDialogueNodeData.Add(nodeData);
                    }
                    else
                    {
                        DialogueGraphNode castNode = node as DialogueGraphNode;
                        DialogueNodeData nodeData = new DialogueNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "DialogueGraphNode";

                        // basic dialogue node data
                        nodeData._speakerName = castNode.speakerTextField.value;
                        nodeData._dialogueText = castNode.dialogueTextField.value;

                        // store IDs for condition input ports; needed to reconnect ports on load
                        for (int j = 0; j < castNode.conditionIds.Count; j++)
                        {
                            nodeData._conditionPortIds.Add(castNode.conditionIds[j]);
                        }

                        // store IDs for choice output ports; needed to reconnect ports on load
                        for (int j = 0; j < castNode.choicePorts.Count; j++)
                        {
                            DialogueNodeData.ChoicePortData choiceData = new DialogueNodeData.ChoicePortData();
                            choiceData._portId = castNode.choicePorts[j].id;
                            choiceData._choiceText = castNode.choicePorts[j].choiceText.value;
                            nodeData._choicePorts.Add(choiceData);
                        }

                        assetData.dialogueNodeData.Add(nodeData);
                    }
                }
                else if (node is BooleanNode)
                {
                    if (node is IntBooleanNode)
                    {
                        IntBooleanNode castNode = node as IntBooleanNode;
                        BooleanNodeData nodeData = new BooleanNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "IntBooleanNode";

                        // boolean node data
                        nodeData._booleanOpEnumVal = Convert.ToInt32(castNode.operationEnumField.value);

                        assetData.booleanNodeData.Add(nodeData);
                    }
                    else if (node is FloatBooleanNode)
                    {
                        FloatBooleanNode castNode = node as FloatBooleanNode;
                        BooleanNodeData nodeData = new BooleanNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "FloatBooleanNode";

                        // boolean node data
                        nodeData._booleanOpEnumVal = Convert.ToInt32(castNode.operationEnumField.value);

                        assetData.booleanNodeData.Add(nodeData);
                    }
                }
                else if (node is GraphNode)
                {
                    if (node.name == "StartNode")
                    {
                        GraphNode castNode = node as GraphNode;
                        NodeData nodeData = new NodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "StartNode";

                        assetData.graphNodeData.Add(nodeData);
                    }
                }
            }

            List<Edge> edges = graphView.edges.ToList();

            // copy edge data into a serializable class
            for (int i = 0; i < edges.Count; i++)
            {
                Edge edge = edges[i];
                NodeLinkData edgeData = new NodeLinkData();

                GraphNode castNode = edge.output.node as GraphNode;
                edgeData._outputNodeGuid = castNode.NodeGuid;
                edgeData._outputPortName = edge.output.portName;
                edgeData._outputElementName = edge.output.name;

                castNode = edge.input.node as GraphNode;
                edgeData._inputNodeGuid = castNode.NodeGuid;
                edgeData._inputPortName = edge.input.portName;
                edgeData._inputElementName = edge.input.name;

                assetData.nodeLinkData.Add(edgeData);
            }

            EditorUtility.CopySerialized(assetData, graphAsset);
            AssetDatabase.SaveAssets();
        }

        public GraphViewChange OnGraphViewChanged(GraphViewChange gvc)
        {
            EditorApplication.update += SaveDelayedGraphAsset;
            return gvc;
        }

        private void SaveDelayedGraphAsset()
        {
            EditorApplication.update -= SaveDelayedGraphAsset;
            SaveGraphAsset();
        }

        public bool LoadGraphAsset(string loadFunc)
        {
            ClearGraph();

            if (graphAsset == null)
            {
                return false;
            }

            //Debug.Log("Loading asset from " + loadFunc);

            isLoadingAsset = true;

            graphView.visible = true;
            noAssetSelectedLabel.visible = false;

            List<GraphNode> nodes = new List<GraphNode>();

            foreach (NodeData data in graphAsset.graphNodeData)
            {
                switch (data._nodeType)
                {
                    case "StartNode":
                        {
                            // transfer standard GraphNode data, add to graph
                            GraphNode node = graphView.CreateStartNode(data._nodeGuid);
                            node.SetPosition(new Rect(data._nodePosition, Vector2.zero));

                            nodes.Add(node);

                            break;
                        }
                }
            }

            // create boolean nodes
            foreach (BooleanNodeData data in graphAsset.booleanNodeData)
            {
                switch (data._nodeType)
                {
                    case "FloatBooleanNode":
                        {
                            FloatBooleanNode node = new FloatBooleanNode();

                            // transfer boolean node data over to new node
                            node.operationEnumField.value = (BooleanNode.BooleanOperation)data._booleanOpEnumVal;

                            // transfer standard GraphNode data, add to graph
                            node.NodeGuid = data._nodeGuid;
                            node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                            graphView.AddElement(node);

                            nodes.Add(node);

                            break;
                        }

                    case "IntBooleanNode":
                        {
                            IntBooleanNode node = new IntBooleanNode();

                            // transfer boolean node data over to new node
                            node.operationEnumField.value = (BooleanNode.BooleanOperation)data._booleanOpEnumVal;

                            // transfer standard GraphNode data, add to graph
                            node.NodeGuid = data._nodeGuid;
                            node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                            graphView.AddElement(node);

                            nodes.Add(node);

                            break;
                        }
                }
            }

            // create basic dialogue nodes
            foreach (DialogueNodeData data in graphAsset.dialogueNodeData)
            {
                DialogueGraphNode node = new DialogueGraphNode();
                node.InitializeFromData(data);
                graphView.AddElement(node);
                nodes.Add(node);
            }

            // create advanced dialogue nodes
            foreach (AdvDialogueNodeData data in graphAsset.advDialogueNodeData)
            {
                AdvDialogueNode node = new AdvDialogueNode();
                node.InitializeFromData(data);
                graphView.AddElement(node);
                nodes.Add(node);
            }

            // create cinematic dialogue nodes
            foreach (CinematicDialogueNodeData data in graphAsset.cinematicDialogueNodeData)
            {
                CinematicDialogueNode node = new CinematicDialogueNode();
                node.InitializeFromData(data);
                graphView.AddElement(node);
                nodes.Add(node);
            }

            // create edges to link nodes together
            foreach (NodeLinkData data in graphAsset.nodeLinkData)
            {
                Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(int));
                Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(int));

                bool foundInput = false;
                bool foundOutput = false;

                for (int i = 0; i < nodes.Count; i++)
                {
                    GraphNode node = nodes[i];

                    if (data._inputNodeGuid == node.NodeGuid || data._outputNodeGuid == node.NodeGuid)
                    {
                        // we can have ports in the title container, output container or input container, so get all of their child elements to search through
                        List<VisualElement> allElements = node.titleContainer.Children()
                                                            .Concat(node.outputContainer.Children())
                                                            .Concat(node.inputContainer.Children())
                                                            .ToList();

                        foreach (VisualElement element in allElements)
                        {
                            if (element is Port)
                            {
                                Port portElement = element as Port;

                                if (data._inputNodeGuid == node.NodeGuid && !foundInput)
                                {
                                    if (portElement.portName == data._inputPortName && portElement.name == data._inputElementName)
                                    {
                                        inputPort = portElement;
                                        foundInput = true;
                                        break;
                                    }
                                }
                                else if (data._outputNodeGuid == node.NodeGuid && !foundOutput)
                                {
                                    if (portElement.portName == data._outputPortName && portElement.name == data._outputElementName)
                                    {
                                        outputPort = portElement;
                                        foundOutput = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (foundInput && foundOutput)
                    {
                        graphView.AddElement(outputPort.ConnectTo(inputPort));
                        break;
                    }
                }
            }

            if (graphView.startNode == null)
            {
                graphView.startNode = graphView.CreateStartNode(graphAsset.GetNewGUID());
            }

            isLoadingAsset = false;

            return true;
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
                        node.NodeGuid = graphAsset.GetNewGUID();
                        node.OnNodeChange += new DialogueGraphNode.NodeChangeEventHandler(SaveGraphAsset);

                        PositionNewNodeElementAtClick(node, context);
                        RegisterDialogueNodesValueChangedCallbacks(node);

                        return true;
                    }

                case "Advanced Dialogue Node":
                    {
                        var node = new AdvDialogueNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();
                        node.OnNodeChange += new DialogueGraphNode.NodeChangeEventHandler(SaveGraphAsset);

                        PositionNewNodeElementAtClick(node, context);
                        RegisterDialogueNodesValueChangedCallbacks(node);

                        return true;
                    }

                case "Cinematic Dialogue Node":
                    {
                        var node = new CinematicDialogueNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();
                        node.OnNodeChange += new DialogueGraphNode.NodeChangeEventHandler(SaveGraphAsset);

                        PositionNewNodeElementAtClick(node, context);
                        RegisterDialogueNodesValueChangedCallbacks(node);

                        return true;
                    }

                case "Boolean Node (Int)":
                    {
                        var node = new IntBooleanNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        node.operationEnumField.RegisterValueChangedCallback(val => SaveGraphAsset());

                        return true;
                    }

                case "Boolean Node (Float)":
                    {
                        var node = new FloatBooleanNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        node.operationEnumField.RegisterValueChangedCallback(val => SaveGraphAsset());

                        return true;
                    }
            }

            return false;
        }

        protected void PositionNewNodeElementAtClick(Node node, SearchWindowContext context)
        {
            Vector2 pointInWindow = context.screenMousePosition - position.position;
            Vector2 pointInGraph = node.parent.WorldToLocal(pointInWindow);

            node.SetPosition(new Rect(pointInGraph, Vector2.zero));
            node.Select(graphView, false);

            EditorApplication.update += SaveDelayedGraphAsset;
        }

        private void RegisterDialogueNodesValueChangedCallbacks(DialogueGraphNode node)
        {
            if (node is DialogueGraphNode)
            {
                node.speakerTextField.RegisterValueChangedCallback(val => SaveGraphAsset());
                node.dialogueTextField.RegisterValueChangedCallback(val => SaveGraphAsset());
            }

            if (node is AdvDialogueNode)
            {
                AdvDialogueNode advNode = node as AdvDialogueNode;

                advNode.cameraPosField.RegisterValueChangedCallback(val => SaveGraphAsset());
                advNode.cameraRotField.RegisterValueChangedCallback(val => SaveGraphAsset());
            }

            if (node is CinematicDialogueNode)
            {
                CinematicDialogueNode advNode = node as CinematicDialogueNode;

                advNode.timelineField.RegisterValueChangedCallback(val => SaveGraphAsset());
            }
        }
    }

    public class DialogueGraphView : GraphView
    {
        public GraphNode startNode;

        public DialogueGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            var grid = new GridBackground { name = "GridBackground" };
            Insert(0, grid);
        }

        public GraphNode CreateStartNode(uint guid)
        {
            startNode = new GraphNode() { name = "StartNode" };
            startNode.title = "START";
            startNode.capabilities &= ~(Capabilities.Deletable | Capabilities.Copiable);
            startNode.SetPosition(new Rect(new Vector2(15f, 200f), new Vector2(100f, 100f)));
            startNode.NodeGuid = guid;

            startNode.titleButtonContainer.RemoveFromHierarchy();
            startNode.inputContainer.RemoveFromHierarchy();
            startNode.outputContainer.RemoveFromHierarchy();

            var nextDialogueNodePort = startNode.AddPort("", typeof(DialogueGraphNode), startNode.titleContainer, false, Port.Capacity.Multi, "next-dialogue-node-input");
            nextDialogueNodePort.AddToClassList("dialogueProgressPort");

            AddElement(startNode);

            return startNode;
        }

        // overriden to allow port connections
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(port => port.direction != startPort.direction && port.node != startPort.node).ToList();
        }

        public void ClearGraphNodes()
        {
            //	! don't use `Clear()`, as this will remove the GridBackground unnecessarily
            //	remove all nodes
            List<Node> allNodes = nodes.ToList();
            foreach (Node n in allNodes)
            {
                RemoveElement(n);
            }
            startNode = null;

            //	remove all edges
            List<Edge> allEdges = edges.ToList();
            foreach (Edge e in allEdges)
            {
                RemoveElement(e);
            }
        }
    }
}
