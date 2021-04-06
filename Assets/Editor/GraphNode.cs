using System.Collections;
using System.Linq;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

namespace CustomEditors
{
    public class GraphNode : Node
    {
        public uint NodeGuid { get; set; }

        public GraphNode()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("GraphNodeStyle"));
            AddToClassList("graphNode");

            // get rid of the default collapse button
            titleButtonContainer.RemoveFromHierarchy();
        }


        public Port AddPort(string portName, System.Type dataType, bool isInputPort = true, Port.Capacity capacity = Port.Capacity.Single, string elementId = "", int insertIndex = -1)
        {
            return AddPort(
                portName,
                dataType,
                isInputPort ? inputContainer : outputContainer,
                isInputPort,
                capacity,
                elementId,
                insertIndex
                );
        }

        public Port AddPort(string portName, System.Type dataType, VisualElement parent, bool isInputPort = true, Port.Capacity capacity = Port.Capacity.Single, string elementId = "", int insertIndex = -1)
        {
            Port port = Port.Create<Edge>(
                  Orientation.Horizontal,
                  isInputPort ? Direction.Input : Direction.Output,
                  capacity,
                  dataType
                  );

            port.portName = portName;
            port.name = elementId;

            if (insertIndex >= 0)
            {
                parent.Insert(insertIndex, port);
            }
            else
            {
                parent.Add(port);
            }

            return port;
        }
    }
}
