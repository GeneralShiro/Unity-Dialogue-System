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

    public class EdgeRedirector : GraphNode
    {
        public Port _leftPort;
        public Port _rightPort;

        public EdgeRedirector(Port sourcePort, Port targetPort)
        {
            _leftPort = AddPort("", sourcePort.portType, true, sourcePort.capacity);
            _rightPort = AddPort("", targetPort.portType, false, targetPort.capacity);

            titleContainer.RemoveFromHierarchy();
        }
    }

    public class EdgeRedirectManipulator : MouseManipulator
    {
        private Edge _edge;

        public EdgeRedirectManipulator()
        {
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse,
                clickCount = 2
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            _edge = target as Edge;

            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);

            _edge = null;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.target is Edge)
            {
                Edge targetEdge = evt.target as Edge;

                if (targetEdge == _edge)
                {
                    if (CanStartManipulation(evt))
                    {
                        CreateEdgeRedirect(_edge, evt.localMousePosition);
                    }

                    evt.StopPropagation();
                }
            }
        }

        private void CreateEdgeRedirect(Edge edge, Vector2 pos)
        {
            // store refs to the ports for the edge
            Port inputPort = edge.input;
            Port outputPort = edge.output;

            GraphView graph = edge.GetFirstAncestorOfType<GraphView>();

            // get rid of edge; it'll be replaced two new ones
            inputPort.Disconnect(edge);
            outputPort.Disconnect(edge);
            graph.RemoveElement(edge);

            // create the redirector node at the specified position
            EdgeRedirector newRedirect = new EdgeRedirector(inputPort, outputPort);
            newRedirect.SetPosition(new Rect(pos, Vector2.zero));
            graph.AddElement(newRedirect);

            // create new edges using port references
            Edge leftEdge = newRedirect._leftPort.ConnectTo(outputPort);
            leftEdge.AddManipulator(new EdgeRedirectManipulator());
            graph.AddElement(leftEdge);

            Edge rightEdge = newRedirect._rightPort.ConnectTo(inputPort);
            rightEdge.AddManipulator(new EdgeRedirectManipulator());
            graph.AddElement(rightEdge);
        }
    }
}
