using System;

namespace DialogueTree
{
    public class DialogueOption
    {
        public string Text;
        public int DestinationNodeID;
        public bool DisplayOnce;
        private bool HasBeenDisplay = false;
        public int MinCharisma;

        public DialogueOption() { }

        public DialogueOption(string text, int dest)
        {
            Text = text;
            DestinationNodeID = dest;
        }

        public override string ToString()
        {
            return "Text : " + Text + " destinationNodeID : " + DestinationNodeID;
        }
    }
}