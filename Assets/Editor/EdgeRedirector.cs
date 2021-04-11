using System.Collections;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

namespace CustomEditors
{
    public class EdgeRedirector : GraphNode
    {
        public Port _leftPort;
        public Port _rightPort;

        public EdgeRedirector(
            System.Type leftPortType,
            Port.Capacity leftPortCapacity,
            System.Type rightPortType,
            Port.Capacity rightPortCapacity)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("EdgeRedirectorStyle"));
            AddToClassList("edgeRedirector");

            VisualElement inputElement = new VisualElement() { name = "customInput" };
            VisualElement outputElement = new VisualElement() { name = "customOutput" };

            topContainer.Add(inputElement);
            topContainer.Add(outputElement);

            _leftPort = AddPort("", leftPortType, inputElement, true, leftPortCapacity, "leftPort");
            _rightPort = AddPort("", rightPortType, outputElement, false, rightPortCapacity, "rightPort");

            titleContainer.RemoveFromHierarchy();
            inputContainer.RemoveFromHierarchy();
            outputContainer.RemoveFromHierarchy();
        }

        public EdgeRedirector(Port sourcePort, Port targetPort)
        : this(
            sourcePort.portType,
            sourcePort.capacity,
            targetPort.portType,
            targetPort.capacity)
        { }
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
