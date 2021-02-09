using System.Collections;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

namespace CustomGraphEditors
{
	public class GraphNode : Node
	{
		public uint NodeGuid { get; set; }

		public GraphNode()
		{
			styleSheets.Add(Resources.Load<StyleSheet>("GraphNodeStyle"));
			AddToClassList("graphNode");
		}

		protected void AddOutputPort(string name, System.Type dataType, Port.Capacity capacity = Port.Capacity.Single)
		{
			var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, capacity, dataType);
			outputPort.portName = name;
			outputContainer.Add(outputPort);
		}

		protected void AddInputPort(string name, System.Type dataType, Port.Capacity capacity = Port.Capacity.Single)
		{
			var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, capacity, dataType);
			inputPort.portName = name;
			inputContainer.Add(inputPort);
		}
	}
}
