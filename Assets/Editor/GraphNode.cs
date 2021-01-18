using System.Collections;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

public class GraphNode : Node
{
	public uint NodeGuid { get; set; }

	public GraphNode()
	{
		styleSheets.Add(Resources.Load<StyleSheet>("GraphNodeStyle"));
		AddToClassList("graphNode");
	}

	protected void AddOutputPort(string name, System.Type dataType)
	{
		var outputPort = GetPortInstance(Direction.Output, dataType);
		outputPort.portName = name;
		outputContainer.Add(outputPort);
	}

	protected void AddInputPort(string name, System.Type dataType)
	{
		var inputPort = GetPortInstance(Direction.Input, dataType);
		inputPort.portName = name;
		inputContainer.Add(inputPort);
	}

	private Port GetPortInstance(Direction nodeDirection, System.Type dataType, Port.Capacity capacity = Port.Capacity.Single)
	{
		return InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, dataType);
	}
}
