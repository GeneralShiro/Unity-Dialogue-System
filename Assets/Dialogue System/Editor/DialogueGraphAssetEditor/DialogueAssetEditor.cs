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
                DialogueAssetEditor window = GetWindow<DialogueAssetEditor>("Dialogue Asset Editor", true, typeof(SceneView));
                window.graphAsset = obj as DialogueGraphAsset;
                window.minSize = new Vector2(500f, 300f);
                window.LoadGraphAsset();

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
                LoadGraphAsset();
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

            // get rid of edges with missing nodes before saving
            graphView.ClearDanglingEdges();

            DialogueGraphAsset assetData = ScriptableObject.CreateInstance<DialogueGraphAsset>();

            List<Node> nodes = graphView.nodes.ToList();

            // copy node data into a serializable class
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                // make sure the node has a GUID if it's a GraphNode; if it's '0', the guid still needs to be set 
                if (node is GraphNode)
                {
                    GraphNode castNode = node as GraphNode;

                    if (castNode.NodeGuid == 0)
                    {
                        castNode.NodeGuid = graphAsset.GetNewGUID();
                    }
                }

                // create data objects based on graph nodes
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
                        nodeData._lerpTime = castNode.lerpTimeField.value;

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
                else if (node is BooleanComparisonNode)
                {
                    if (node is IntComparisonNode)
                    {
                        IntComparisonNode castNode = node as IntComparisonNode;
                        BooleanComparisonNodeData nodeData = new BooleanComparisonNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "IntComparisonNode";

                        // boolean node data
                        nodeData._comparisonEnumVal = Convert.ToInt32(castNode.operationEnumField.value);

                        assetData.booleanComparisonNodeData.Add(nodeData);
                    }
                    else if (node is FloatComparisonNode)
                    {
                        FloatComparisonNode castNode = node as FloatComparisonNode;
                        BooleanComparisonNodeData nodeData = new BooleanComparisonNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "FloatComparisonNode";

                        // boolean node data
                        nodeData._comparisonEnumVal = Convert.ToInt32(castNode.operationEnumField.value);

                        assetData.booleanComparisonNodeData.Add(nodeData);
                    }
                }
                else if (node is BooleanLogicNode)
                {
                    BooleanLogicNode castNode = node as BooleanLogicNode;
                    BooleanLogicNodeData nodeData = new BooleanLogicNodeData();

                    // graph node data
                    nodeData._nodeGuid = castNode.NodeGuid;
                    nodeData._nodePosition = castNode.GetPosition().position;
                    nodeData._nodeType = "BooleanLogicNode";

                    // boolean logic node data
                    nodeData._logicEnumVal = Convert.ToInt32(castNode.operationEnumField.value);

                    for (int j = 0; j < castNode.additionalInputPortIds.Count; j++)
                    {
                        nodeData._additionalInputPortIds.Add(castNode.additionalInputPortIds[j]);
                    }

                    assetData.booleanLogicNodeData.Add(nodeData);
                }
                else if (node is BooleanNOTNode)
                {
                    BooleanNOTNode castNode = node as BooleanNOTNode;
                    NodeData nodeData = new NodeData();

                    // graph node data
                    nodeData._nodeGuid = castNode.NodeGuid;
                    nodeData._nodePosition = castNode.GetPosition().position;
                    nodeData._nodeType = "BooleanNOTNode";

                    assetData.graphNodeData.Add(nodeData);
                }
                else if (node is AccessorNode)
                {
                    if (node is EnumComparisonNode)
                    {
                        EnumComparisonNode castNode = node as EnumComparisonNode;
                        EnumComparisonNodeData nodeData = new EnumComparisonNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "EnumComparisonNode";

                        // accessor node data
                        nodeData._scriptableObj = castNode._objectField.value as ScriptableObject;
                        nodeData._typeEnumVal = (int)castNode._targetPropertyType;
                        nodeData._chosenPropertyString = castNode._popupField.value;

                        // enum comparison node data
                        nodeData._chosenEnumValue = castNode._enumValField.value;

                        assetData.enumComparisonNodeData.Add(nodeData);
                    }
                    else
                    {
                        AccessorNode castNode = node as AccessorNode;
                        AccessorNodeData nodeData = new AccessorNodeData();

                        // graph node data
                        nodeData._nodeGuid = castNode.NodeGuid;
                        nodeData._nodePosition = castNode.GetPosition().position;
                        nodeData._nodeType = "AccessorNode";

                        // accessor node data
                        nodeData._scriptableObj = castNode._objectField.value as ScriptableObject;
                        nodeData._typeEnumVal = (int)castNode._targetPropertyType;
                        nodeData._chosenPropertyString = castNode._popupField.value;

                        assetData.accessorNodeData.Add(nodeData);
                    }
                }
                else if (node is IntValueNode)
                {
                    IntValueNode castNode = node as IntValueNode;
                    IntValNodeData nodeData = new IntValNodeData();

                    // graph node data
                    nodeData._nodeGuid = castNode.NodeGuid;
                    nodeData._nodePosition = castNode.GetPosition().position;
                    nodeData._nodeType = "IntValueNode";

                    // int value node data
                    nodeData._intVal = castNode._intField.value;

                    assetData.intValNodeData.Add(nodeData);
                }
                else if (node is FloatValueNode)
                {
                    FloatValueNode castNode = node as FloatValueNode;
                    FloatValNodeData nodeData = new FloatValNodeData();

                    // graph node data
                    nodeData._nodeGuid = castNode.NodeGuid;
                    nodeData._nodePosition = castNode.GetPosition().position;
                    nodeData._nodeType = "FloatValueNode";

                    // float value node data
                    nodeData._floatVal = castNode._floatField.value;

                    assetData.floatValNodeData.Add(nodeData);
                }
                else if (node is EdgeRedirector)
                {
                    EdgeRedirector castNode = node as EdgeRedirector;
                    EdgeRedirectorData nodeData = new EdgeRedirectorData();

                    // graph node data
                    nodeData._nodeGuid = castNode.NodeGuid;
                    nodeData._nodePosition = castNode.GetPosition().position;
                    nodeData._nodeType = "EdgeRedirector";

                    // edge redirector data
                    nodeData._leftPortType = castNode._leftPort.portType;
                    nodeData._rightPortType = castNode._rightPort.portType;
                    nodeData._rightPortCapacityVal = Convert.ToInt32(castNode._rightPort.capacity);

                    assetData.edgeRedirectorData.Add(nodeData);
                }
                else if (node is GraphNode)
                {
                    GraphNode castNode = node as GraphNode;
                    NodeData nodeData = new NodeData();

                    // graph node data
                    nodeData._nodeGuid = castNode.NodeGuid;
                    nodeData._nodePosition = castNode.GetPosition().position;

                    if (node.name == "StartNode")
                    {
                        nodeData._nodeType = "StartNode";
                    }

                    assetData.graphNodeData.Add(nodeData);
                }
            }

            List<Edge> edges = graphView.edges.ToList();

            // copy edge data into a serializable class
            for (int i = 0; i < edges.Count; i++)
            {
                Edge edge = edges[i];

                if (edge.output.node != null && edge.input.node != null)
                {
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
            }

            EditorUtility.CopySerialized(assetData, graphAsset);
            AssetDatabase.SaveAssets();
        }

        public GraphViewChange OnGraphViewChanged(GraphViewChange gvc)
        {
            EditorApplication.update += SaveDelayedGraphAsset;

            if (gvc.edgesToCreate != null)
            {
                foreach (Edge e in gvc.edgesToCreate)
                {
                    e.AddManipulator(new EdgeRedirectManipulator());
                }
            }

            return gvc;
        }

        private void SaveDelayedGraphAsset()
        {
            EditorApplication.update -= SaveDelayedGraphAsset;
            SaveGraphAsset();
        }

        public bool LoadGraphAsset()
        {
            ClearGraph();

            if (graphAsset == null)
            {
                return false;
            }

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

                    case "BooleanNOTNode":
                        {
                            BooleanNOTNode node = new BooleanNOTNode();

                            // transfer standard GraphNode data, add to graph
                            node.NodeGuid = data._nodeGuid;
                            node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                            graphView.AddElement(node);

                            nodes.Add(node);

                            break;
                        }
                }
            }

            // create edge redirector nodes
            foreach (EdgeRedirectorData data in graphAsset.edgeRedirectorData)
            {
                EdgeRedirector node = new EdgeRedirector(
                    data._leftPortType,
                    data._rightPortType,
                    (Port.Capacity)data._rightPortCapacityVal
                    );

                // transfer standard GraphNode data, add to graph
                node.NodeGuid = data._nodeGuid;
                node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                graphView.AddElement(node);

                nodes.Add(node);
            }

            // create accessor nodes
            foreach (AccessorNodeData data in graphAsset.accessorNodeData)
            {
                SerializedPropertyType type = (SerializedPropertyType)data._typeEnumVal;

                switch (type)
                {
                    case SerializedPropertyType.Integer:
                        {
                            IntGetterNode node = new IntGetterNode();
                            node.InitializeFromData(data);

                            graphView.AddElement(node);
                            nodes.Add(node);

                            break;
                        }
                    case SerializedPropertyType.Float:
                        {
                            FloatGetterNode node = new FloatGetterNode();
                            node.InitializeFromData(data);

                            graphView.AddElement(node);
                            nodes.Add(node);

                            break;
                        }
                    case SerializedPropertyType.Boolean:
                        {
                            BoolGetterNode node = new BoolGetterNode();
                            node.InitializeFromData(data);

                            graphView.AddElement(node);
                            nodes.Add(node);

                            break;
                        }
                }
            }

            // create enum comparison nodes
            foreach (EnumComparisonNodeData data in graphAsset.enumComparisonNodeData)
            {
                EnumComparisonNode node = new EnumComparisonNode();
                node.InitializeFromData(data);

                graphView.AddElement(node);
                nodes.Add(node);
            }

            // create int value nodes
            foreach (IntValNodeData data in graphAsset.intValNodeData)
            {
                IntValueNode node = new IntValueNode();
                node._intField.value = data._intVal;

                // transfer standard GraphNode data, add to graph
                node.NodeGuid = data._nodeGuid;
                node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                graphView.AddElement(node);

                nodes.Add(node);
            }

            // create float value nodes
            foreach (FloatValNodeData data in graphAsset.floatValNodeData)
            {
                FloatValueNode node = new FloatValueNode();
                node._floatField.value = data._floatVal;

                // transfer standard GraphNode data, add to graph
                node.NodeGuid = data._nodeGuid;
                node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                graphView.AddElement(node);

                nodes.Add(node);
            }

            // create boolean comparison nodes
            foreach (BooleanComparisonNodeData data in graphAsset.booleanComparisonNodeData)
            {
                switch (data._nodeType)
                {
                    case "FloatComparisonNode":
                        {
                            FloatComparisonNode node = new FloatComparisonNode();

                            // transfer boolean node data over to new node
                            node.operationEnumField.value = (BooleanComparisonNode.ComparisonOperator)data._comparisonEnumVal;

                            // transfer standard GraphNode data, add to graph
                            node.NodeGuid = data._nodeGuid;
                            node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                            graphView.AddElement(node);

                            nodes.Add(node);

                            break;
                        }

                    case "IntComparisonNode":
                        {
                            IntComparisonNode node = new IntComparisonNode();

                            // transfer boolean node data over to new node
                            node.operationEnumField.value = (BooleanComparisonNode.ComparisonOperator)data._comparisonEnumVal;

                            // transfer standard GraphNode data, add to graph
                            node.NodeGuid = data._nodeGuid;
                            node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                            graphView.AddElement(node);

                            nodes.Add(node);

                            break;
                        }
                }
            }

            // create boolean logic nodes
            foreach (BooleanLogicNodeData data in graphAsset.booleanLogicNodeData)
            {
                BooleanLogicNode node = new BooleanLogicNode();

                // transfer boolean logic node data to new node
                node.operationEnumField.value = (BooleanLogicNode.LogicOperator)data._logicEnumVal;

                // add additional input ports
                for (int i = 0; i < data._additionalInputPortIds.Count; i++)
                {
                    node.AddInputPort(data._additionalInputPortIds[i]);
                }

                // transfer standard GraphNode data, add to graph
                node.NodeGuid = data._nodeGuid;
                node.SetPosition(new Rect(data._nodePosition, Vector2.zero));
                graphView.AddElement(node);

                nodes.Add(node);
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
                        // we can have ports in the title container, output container, input container and top container, so get all of their child elements to search through
                        List<VisualElement> allElements = node.titleContainer.Children().ToList();

                        // find input container ports
                        List<VisualElement> inputContainerChildren = node.inputContainer.Children().ToList();
                        foreach (VisualElement ve in inputContainerChildren)
                        {
                            List<VisualElement> children = ve.Children().ToList();
                            foreach (VisualElement childVe in children)
                            {
                                if (childVe is Port)
                                {
                                    allElements.Add(childVe);
                                }
                            }
                        }

                        // find output container ports
                        List<VisualElement> outputContainerChildren = node.outputContainer.Children().ToList();
                        foreach (VisualElement ve in outputContainerChildren)
                        {
                            List<VisualElement> children = ve.Children().ToList();
                            foreach (VisualElement childVe in children)
                            {
                                if (childVe is Port)
                                {
                                    allElements.Add(childVe);
                                }
                            }
                        }

                        // find top container ports
                        List<VisualElement> topContainerChildren = node.topContainer.Children().ToList();
                        foreach (VisualElement ve in topContainerChildren)
                        {
                            List<VisualElement> children = ve.Children().ToList();
                            foreach (VisualElement childVe in children)
                            {
                                if (childVe is Port)
                                {
                                    allElements.Add(childVe);
                                }
                            }
                        }

                        // check all ports in the collection of elements
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
                        Edge newEdge = outputPort.ConnectTo(inputPort);
                        newEdge.AddManipulator(new EdgeRedirectManipulator());

                        graphView.AddElement(newEdge);

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
            tree.Add(new SearchTreeEntry(new GUIContent("Logic NOT Node", icon)) { level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Logic AND Node", icon)) { level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Logic OR Node", icon)) { level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Compare (Int)", icon)) { level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Compare (Float)", icon)) { level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Compare (Enum)", icon)) { level = 2 });

            tree.Add(new SearchTreeGroupEntry(new GUIContent("Getters", icon)) { level = 1 });
            tree.Add(new SearchTreeEntry(new GUIContent("Get (Int)", icon)) { level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Get (Float)", icon)) { level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("Get (Bool)", icon)) { level = 2 });

            tree.Add(new SearchTreeGroupEntry(new GUIContent("Raw Values", icon)) { level = 1 });
            tree.Add(new SearchTreeEntry(new GUIContent("New Int", icon)) { level = 2 });
            tree.Add(new SearchTreeEntry(new GUIContent("New Float", icon)) { level = 2 });

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
                case "Logic NOT Node":
                    {
                        var node = new BooleanNOTNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        return true;
                    }

                case "Logic AND Node":
                    {
                        var node = new BooleanLogicNode(BooleanLogicNode.LogicOperator.AND);
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        return true;
                    }

                case "Logic OR Node":
                    {
                        var node = new BooleanLogicNode(BooleanLogicNode.LogicOperator.OR);
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        return true;
                    }

                case "Compare (Int)":
                    {
                        var node = new IntComparisonNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        node.operationEnumField.RegisterValueChangedCallback(val => SaveGraphAsset());

                        return true;
                    }

                case "Compare (Float)":
                    {
                        var node = new FloatComparisonNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        node.operationEnumField.RegisterValueChangedCallback(val => SaveGraphAsset());

                        return true;
                    }
                case "Compare (Enum)":
                    {
                        var node = new EnumComparisonNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        return true;
                    }
                case "Get (Int)":
                    {
                        var node = new IntGetterNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        return true;
                    }

                case "Get (Float)":
                    {
                        var node = new FloatGetterNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        return true;
                    }
                case "Get (Bool)":
                    {
                        var node = new BoolGetterNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        return true;
                    }
                case "New Int":
                    {
                        var node = new IntValueNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

                        return true;
                    }
                case "New Float":
                    {
                        var node = new FloatValueNode();
                        graphView.AddElement(node);
                        node.NodeGuid = graphAsset.GetNewGUID();

                        PositionNewNodeElementAtClick(node, context);

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
            startNode.Query("contents").First().RemoveFromHierarchy();

            var nextDialogueNodePort = startNode.AddPort("", typeof(DialogueGraphNode), startNode.titleContainer, false, Port.Capacity.Multi, "next-dialogue-node-output");
            startNode.styleSheets.Add(Resources.Load<StyleSheet>("DialogueNodeStyle"));
            startNode.AddToClassList("startNode");

            nextDialogueNodePort.AddToClassList("dialogueProgressPort");
            nextDialogueNodePort.tooltip = "Connect to the first dialogue node";

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

        public void ClearDanglingEdges()
        {
            List<Edge> edgesList = this.edges.ToList();

            for (int i = edgesList.Count - 1; i >= 0; i--)
            {
                Edge e = edgesList[i];
                if (e.output.node == null || e.input.node == null)
                {
                    e.output.Disconnect(e);
                    e.input.Disconnect(e);

                    RemoveElement(e);
                }
            }
        }
    }
}
