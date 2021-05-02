using System.Collections;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

using CustomSystem;

namespace CustomEditors
{
    public class EdgeRedirector : GraphNode
    {
        public Port _leftPort;
        public Port _rightPort;

        public EdgeRedirector(
            System.Type leftPortType,
            System.Type rightPortType,
            Port.Capacity rightPortCapacity)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("EdgeRedirectorStyle"));
            AddToClassList("edgeRedirector");

            VisualElement inputElement = new VisualElement() { name = "edgeRedirectInput" };
            VisualElement outputElement = new VisualElement() { name = "edgeRedirectOutput" };

            topContainer.Add(inputElement);
            topContainer.Add(outputElement);

            _leftPort = AddPort("", leftPortType, inputElement, true, Port.Capacity.Single, "edge-redirect-input");
            _rightPort = AddPort("", rightPortType, outputElement, false, rightPortCapacity, "edge-redirect-output");

            titleContainer.RemoveFromHierarchy();
            inputContainer.RemoveFromHierarchy();
            outputContainer.RemoveFromHierarchy();
        }

        public EdgeRedirector(Port sourcePort, Port targetPort)
        : this(
            sourcePort.portType,
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

            // create the redirector node at the specified position
            EdgeRedirector newRedirect = new EdgeRedirector(inputPort, outputPort);
            newRedirect.SetPosition(new Rect(pos, Vector2.zero));

            GraphView graph = edge.GetFirstAncestorOfType<GraphView>();

            // get rid of edge; it'll be replaced two new ones
            inputPort.Disconnect(edge);
            outputPort.Disconnect(edge);
            graph.RemoveElement(edge);
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
