using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using UnityEditor.Experimental.GraphView;


namespace CustomEditors.DialogueSystem
{
    public class DialogueBranchNode : GraphNode
    {
        public DialogueBranchNode()
        {
            title = "Dialogue Branch";
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueBranchStyle"));
            AddToClassList("dialogueBranch");    // USS style

            // add input bool port and port for previous dialogue node(s)
            AddPort("Prev Dialogue", typeof(DialogueGraphNode), true, Port.Capacity.Multi);
            AddPort("Boolean", typeof(bool));

            // add output ports for Dialogue Nodes
            AddPort("If True", typeof(DialogueGraphNode), false, Port.Capacity.Multi);
            AddPort("If False", typeof(DialogueGraphNode), false, Port.Capacity.Multi);
        }
    }
}
