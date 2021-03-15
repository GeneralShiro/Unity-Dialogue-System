using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace CustomEditors
{
    public class BooleanLogicNode : GraphNode
    {
        public enum LogicOperator
        {
            AND,
            OR
        }
        public EnumField operationEnumField { get; set; }
        public List<uint> inputPortIds { get; protected set; }
        protected uint nextInputPortId;


        public BooleanLogicNode() : this(LogicOperator.AND) { }

        public BooleanLogicNode(LogicOperator logicOp)
        {
            title = "Logic Node";
            AddToClassList("boolLogicNode");    // USS style

            operationEnumField = new EnumField(logicOp);
            mainContainer.Insert(1, operationEnumField);

            // create output port
            AddPort("Output", typeof(bool), false);

            // add button to input container to allow the addition of ports
            var addInputPortsButton = new Button(OnAddInputPortButtonClick);
            addInputPortsButton.text = "Add Input";
            inputContainer.Add(addInputPortsButton);

            nextInputPortId = 2;
            inputPortIds = new List<uint>();

            // create default input ports
            Port inputPort1 = AddPort("Boolean", typeof(bool));
            inputPort1.name = "boolean-input-port-0";
            Port inputPort2 = AddPort("Boolean", typeof(bool));
            inputPort2.name = "boolean-input-port-1";
            inputPort2.tooltip = inputPort1.tooltip = "Connect a boolean value";
        }

        protected void OnAddInputPortButtonClick()
        {
            AddInputPort(nextInputPortId);
            nextInputPortId++;
        }

        protected void AddInputPort(uint id)
        {
            // 1. create panel to parent the port and delete button; add to input container
            var boolInputPortPanel = new VisualElement()
            {
                name = "boolean-input-panel"
            };
            inputContainer.Add(boolInputPortPanel);


            // 2. add input port
            Port port = AddPort(
                "Boolean",
                typeof(bool),
                boolInputPortPanel,
                true,
                Port.Capacity.Single,
                "boolean-input-port-" + id.ToString()
                );
            port.tooltip = "Connect a boolean value";
            port.AddToClassList("dialogueConditionInputPort");

            // 3. add delete button
            var deleteButton = new Button(() =>
            {
                port.DisconnectAll();
                inputPortIds.Remove(id);
                inputContainer.Remove(boolInputPortPanel);
            });
            deleteButton.name = "boolean-input-port-delete-button";
            deleteButton.text = "X";
            deleteButton.tooltip = "Delete";
            boolInputPortPanel.Add(deleteButton);

            inputPortIds.Add(id);
        }
    }

    public class BooleanNOTNode : GraphNode
    {
        public BooleanNOTNode()
        {
            title = "Logic NOT Node";
            tooltip = "Inverses the value of the input boolean, sent as the output boolean.";

            // create output port
            AddPort("", typeof(bool), false);

            // create input port
            AddPort("", typeof(bool));
        }
    }
}
