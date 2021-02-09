﻿using System.Collections;
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


        protected Port AddPort(string portName, System.Type dataType, bool isInputPort = true, Port.Capacity capacity = Port.Capacity.Single, string elementId = "", int insertIndex = -1)
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

        protected Port AddPort(string portName, System.Type dataType, VisualElement parent, bool isInputPort = true, Port.Capacity capacity = Port.Capacity.Single, string elementId = "", int insertIndex = -1)
        {
            Port port = Port.Create<Edge>(
                  Orientation.Horizontal,
                  isInputPort ? Direction.Input : Direction.Output,
                  capacity,
                  dataType
                  );

            port.portName = portName;
            port.name = elementId;

            if(insertIndex >= 0)
            {
                parent.Insert(insertIndex, port);
            } else {
                parent.Add(port);              
            }
            
            return port;
        }
    }
}