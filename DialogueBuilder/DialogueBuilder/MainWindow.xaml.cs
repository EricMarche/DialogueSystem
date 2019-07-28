using DialogueTree;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace DialogueBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dialogue dialogue;
        MenuItem racine;
        SelectedItem selectedItem;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Dialogue Builder";
        }

        private void Save_DialogueNode_Click(object sender, RoutedEventArgs e)
        {
            string dialogueText = "";
            DialogueOption[] options = new DialogueOption[3];
            string path = FilePath.Text;
            int nodeID = selectedItem.selectedDialogueNode.NodeID;

            if (CheckBox_Option1.IsChecked.Value)
            {
                options[0].DisplayOnce = DisplayOnce1.IsChecked.Value;
                int destNode = returnParentNode1.IsChecked.Value ? nodeID : -1;
                options[0].DestinationNodeID = destNode;
                int minCharisma = MinCharisma1.Text != null ? 0 : int.Parse(MinCharisma1.Text);
                options[0].Text = Option1_Text.Text;
            }
            if (CheckBox_Option2.IsChecked.Value)
            {
                options[1].DisplayOnce = DisplayOnce2.IsChecked.Value;
                int destNode = returnParentNode2.IsChecked.Value ? nodeID : -1;
                options[1].DestinationNodeID = destNode;
                int minCharisma = MinCharisma2.Text != null ? 0 : int.Parse(MinCharisma2.Text);
                options[1].Text = Option2_Text.Text;
            }
            if (CheckBox_Option3.IsChecked.Value)
            {
                options[2].DisplayOnce = DisplayOnce3.IsChecked.Value;
                int destNode = returnParentNode3.IsChecked.Value ? nodeID : -1;
                options[2].DestinationNodeID = destNode;
                int minCharisma = MinCharisma3.Text != null ? 0 : int.Parse(MinCharisma3.Text);
                options[2].Text = Option3_Text.Text;
            }

            // I set string to empty if no value set to be able to handle then later
            //if (CheckBox_Option1.IsChecked.Value)
            //    options.Add(Option1_Text.Text);
            //else
            //    options.Add("");
            //if (CheckBox_Option2.IsChecked.Value)
            //    options.Add(Option2_Text.Text);
            //else
            //    options.Add("");
            //if (CheckBox_Option3.IsChecked.Value)
            //    options.Add(Option3_Text.Text);
            //else
            //    options.Add("");

            if (DialogueText.Text != null) 
                dialogueText = DialogueText.Text;

            dialogue = Dialogue.UpdateDialogueNode(dialogue, nodeID, dialogueText, options, path);
            UpdateNodeTree();
        }

        private void Load_DialogueFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "xml";
            openFileDialog.Filter = "XML Files|*.xml"; ;
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != null)
            {
                FilePath.Text = openFileDialog.FileName;
                FileName.Text = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                dialogue = Dialogue.LoadDialogue(openFileDialog.FileName);
                ClearDialogueNodeForm();
                UpdateNodeTree();
            }
        }

        private void UpdateNodeTree()
        {
            NodeTree.Items.Clear();
            if (dialogue.Nodes.Count > 0)
            {
                racine = new MenuItem() { Title = dialogue.Nodes[0].Text };
                AddMenuItem(dialogue.Nodes[0], racine);
                NodeTree.Items.Add(racine);
            }
        }

        private void AddMenuItem(DialogueNode node, MenuItem menuItem)
        {
            foreach (DialogueOption option in node.Options)
            {
                if (option.DestinationNodeID != -1)
                {
                    DialogueNode nextNode = dialogue.Nodes.Find(x => x.NodeID == option.DestinationNodeID);
                    //Debug.Print("menuItem : " + menuItem.Title + "/ option.Text : " + option.Text 
                    //    + "/ nextNode.ToString() : " + nextNode.ToString());
                    MenuItem optionMenuItem = new MenuItem() { Title = option.Text, ParentNodeID = node.NodeID };
                    //-2 because they dont have ParentID and -1 already mean no destinationNodeID
                    MenuItem destinationMenuItem = new MenuItem() { Title = nextNode.Text, ParentNodeID = -2 };

                    optionMenuItem.Items.Add(destinationMenuItem);
                    menuItem.Items.Add(optionMenuItem);
                    AddMenuItem(nextNode, destinationMenuItem);
                } else
                {
                    MenuItem optionMenuItem = new MenuItem() { Title = option.Text, ParentNodeID = node.NodeID};
                    menuItem.Items.Add(optionMenuItem);
                }
            }
        }

        private void Add_DialogueNode_Click(object sender, RoutedEventArgs e)
        {
            int newNodeID = dialogue.Nodes.Count;
            DialogueNode newNode = new DialogueNode("Default text");
            newNode.NodeID = newNodeID;
            dialogue.Nodes.Add(newNode);

            DialogueNode replaceNode = selectedItem.selectedDialogueNode;
            DialogueOption replaceOption = selectedItem.selectedDialogueOption;
            int indexOfOption = replaceNode.Options.IndexOf(replaceOption);
            replaceOption.DestinationNodeID = newNodeID;
            replaceNode.Options[indexOfOption] = replaceOption;

            // because the ID is the position in the list
            dialogue.Nodes[selectedItem.selectedDialogueNode.NodeID] = replaceNode;

            string filename = "default";
            if (FileName.Text != null)
                filename = FileName.Text;

            string path = null;
            if (FilePath.Text != null)
                path = FilePath.Text;

            Dialogue.SaveDialogue(path, filename, dialogue);
            UpdateNodeTree();
        }

        private void NodeTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ClearDialogueNodeForm();
            Add_DialogueNode.IsEnabled = false;
            selectedItem = default(SelectedItem);
            MenuItem selectedMenuItem = (MenuItem)e.NewValue;
            //Debug.Print("Title : " + selectedMenuItem.Title + " ParentNodeID : " + selectedMenuItem.ParentNodeID);

            //0 in MenuItem mean we didnt select a DialogueOption
            //So we need to get the DialogueOption from the DialogueNode
            if (selectedMenuItem != null && selectedMenuItem.ParentNodeID != -2)
            {
                DialogueNode node = dialogue.Nodes[selectedMenuItem.ParentNodeID];
                DialogueOption option = node.Options.Find(x => x.Text.Equals(selectedMenuItem.Title));

                if (option != null && option.DestinationNodeID == -1)
                {
                    Add_DialogueNode.IsEnabled = true;
                }

                selectedItem.selectedMenuItem = selectedMenuItem;
                selectedItem.selectedDialogueOption = option;
                selectedItem.selectedDialogueNode = node;
            }
            else if (selectedMenuItem != null && selectedMenuItem.ParentNodeID == -2)
            {
                // If we click on a node we want to be able to modify it and its option
                // Can we have 2 time same dialogue ? Hope not
                DialogueNode node = dialogue.Nodes.Find(x => x.Text.Equals(selectedMenuItem.Title));
                selectedItem.selectedMenuItem = selectedMenuItem;
                selectedItem.selectedDialogueNode = node;
                selectedItem.selectedDialogueOption = null;
            }
            
            UpdateDialogueNodeForm();
        }

        private void Create_New_Tree_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.Filter = "XML Files|*.xml"; ;
            saveFileDialog.FileName = "default";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "" && saveFileDialog.FileName != null)
            {
                FilePath.Text = saveFileDialog.FileName;
                FileName.Text = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                dialogue = Dialogue.CreateDialogue(saveFileDialog.FileName);
                UpdateNodeTree();
            }
        }

        private void UpdateDialogueNodeForm()
        {
            if (selectedItem.selectedDialogueNode != null)
            {
                DialogueText.Text = selectedItem.selectedDialogueNode.Text;
                List<DialogueOption> options = selectedItem.selectedDialogueNode.Options;
                for (int i = 0; i < selectedItem.selectedDialogueNode.Options.Count && i < 3; ++i)
                {
                    switch (i)
                    {
                        case 0:
                            Option1_Text.Text = options[i].Text;
                            if (Option1_Text.Text != "")
                                CheckBox_Option1.IsChecked = true;
                            break;
                        case 1:
                            Option2_Text.Text = options[i].Text;
                            if (Option2_Text.Text != "")
                                CheckBox_Option2.IsChecked = true;
                            break;
                        case 2:
                            Option3_Text.Text = options[i].Text;
                            if (Option3_Text.Text != "")
                                CheckBox_Option3.IsChecked = true;
                            break;
                    }
                }
            }
        }

        private void ClearDialogueNodeForm()
        {
            DialogueText.Clear();

            Option1_Text.Clear();
            Option2_Text.Clear();
            Option3_Text.Clear();

            CheckBox_Option1.IsChecked = false;
            CheckBox_Option2.IsChecked = false;
            CheckBox_Option3.IsChecked = false;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }
    }

    public struct SelectedItem
    {
        public MenuItem selectedMenuItem;
        public DialogueOption selectedDialogueOption;
        public DialogueNode selectedDialogueNode;
    }
}
