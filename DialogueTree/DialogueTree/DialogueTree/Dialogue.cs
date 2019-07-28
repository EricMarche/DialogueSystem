using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DialogueTree
{
    public class Dialogue
    {
        public List<DialogueNode> Nodes;
        private static Dialogue dialogue;

        public static Dialogue Instance()
        {
            if (dialogue == null)
            {
                dialogue = new Dialogue();
                return dialogue;
            }

            return dialogue;
        }

        public Dialogue()
        {
            Nodes = new List<DialogueNode>();
        }

        public void AddNode(DialogueNode node)
        {
            if (node == null) return;

            Nodes.Add(node);

            node.NodeID = Nodes.IndexOf(node);
        }

        public void AddOption(string text, DialogueNode node, DialogueNode dest)
        {
            // add destination node
            if (!Nodes.Contains(dest))
                AddNode(dest);

            // Add parent node
            if (!Nodes.Contains(node))
                AddNode(node);

            DialogueOption option;

            // -1 mean there no more option following that
            if (dest == null)
                option = new DialogueOption(text, -1);
            else
                option = new DialogueOption(text, dest.NodeID);

            node.Options.Add(option);
        }

        public static void CreateDialogue()
        {
            Dialogue dialogue = new Dialogue();
            DialogueNode node1 = new DialogueNode("Bonjour, Eric comment-va tu ?");
            DialogueNode node2 = new DialogueNode("Bien merci");
            DialogueNode node3 = new DialogueNode("Ayoye c'est méchant sa");

            dialogue.AddNode(node1);
            dialogue.AddNode(node2);
            dialogue.AddNode(node3);

            dialogue.AddOption("Très bien et toi ?", node1, node2);
            dialogue.AddOption("Mange d'la crote", node1, node3);

            DialogueNode node4 = new DialogueNode("As-tu finalement fini Kingmaker ? Serait temps");
            dialogue.AddOption(null, node2, node4);
            dialogue.AddOption("Oui, c'était quand bien comme jeux", node4, null);
            dialogue.AddOption("Non, c'est je n'ai plus de motivation", node4, null);
            dialogue.AddOption("Presque, on dirait le jeux veut pas finir", node4, null);

            dialogue.AddNode(node4);

            XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));
            StreamWriter writer = new StreamWriter("Assets/TheGrid/Text/test_dialogue.xml");

            serializer.Serialize(writer, dialogue);
            writer.Close();
        }

        public static Dialogue CreateDialogue(string path)
        {
            // We will need to add different destination node somehow
            DialogueNode node = new DialogueNode("Default text");
            Dialogue dialogue = Instance();
            dialogue.AddNode(node);
            
            XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));
            StreamWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, dialogue);
            writer.Close();

            return dialogue;
        }

        public static void SaveDialogue(string path, string filename, Dialogue dialogue)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));

            StreamWriter  writer = new StreamWriter(path);

            serializer.Serialize(writer, dialogue);
            writer.Close();
        }

        public static Dialogue UpdateDialogueNode(Dialogue dialogue, int nodeID, string dialogueText, List<string> options, string path)
        {
            // We will need to add different destination node somehow
            DialogueNode node = dialogue.Nodes[nodeID];
            node.Text = dialogueText;
            //node.Options.Clear();

            for(int i = 0; i < options.Count; i++)
            {
                // Si le text est vide alors qu'on a une option signale qu'on veut la retirer
                if (i < node.Options.Count && options[i] == "")
                {
                    node.Options.RemoveAt(i);
                }
                else if (i < node.Options.Count && !node.Options[i].Text.Equals(options[i]))
                {
                    // Si le text a pas changé remet la même node
                    DialogueNode destinationNode = dialogue.Nodes[node.Options[i].DestinationNodeID];
                    dialogue.AddOption(options[i], node, destinationNode);
                }
                else if (i >= node.Options.Count && options[i] != "")
                {
                    //On ajoute une nouvelle option sans destination
                    dialogue.AddOption(options[i], node, null);
                }
            }

            dialogue.Nodes[nodeID] = node;

            XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));
            StreamWriter writer = new StreamWriter(path);

            serializer.Serialize(writer, dialogue);
            writer.Close();

            return dialogue;
        }

        public static Dialogue LoadDialogue(string path)
        {
            if (path != null)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Dialogue));
                StreamReader reader = new StreamReader(path);

                dialogue = (Dialogue)serializer.Deserialize(reader);
                reader.Close();
            }

            return dialogue;
        }
    }
}
