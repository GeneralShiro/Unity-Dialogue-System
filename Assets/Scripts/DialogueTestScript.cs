using UnityEngine;

using CustomSystem.DialogueSystem;

public class DialogueTestScript : MonoBehaviour
{
    public DialogueGraphAsset _testDialogueGraph;

    // Start is called before the first frame update
    void Start()
    {
        DialogueManager manager = DialogueManager.GetCurrentManager();
        manager.LoadAsset(_testDialogueGraph);
    }

    // Update is called once per frame
    void Update()
    {
        if (!DialogueManager.IsRunningDialogue)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                DialogueManager.GetCurrentManager().StartDialogue();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                DialogueManager.GetCurrentManager().ContinueDialogue();
            }
        }
    }
}
