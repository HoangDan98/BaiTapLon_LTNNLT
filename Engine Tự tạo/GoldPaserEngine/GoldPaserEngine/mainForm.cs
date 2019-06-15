using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using GoldPaserEngine.GoldParser;
using GoldPaserEngine.GoldParser.Commons;

namespace GoldPaserEngine
{
    public partial class mainForm : Form
    {

       // UserSettings settings;
        LALRParser parser;
        int maxerrors = 0;
        int errors = 0;
        private byte[] BOM = new byte[] { 0xFF, 0xFE };

        public mainForm()
        {
            InitializeComponent();
        }

        private void openGrammarFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenGrammar();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ParseSource();
        }


        private void OpenGrammar()
        {
            if (openGrammarDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    CGTReader reader = new CGTReader(openGrammarDialog.FileName);
                    parser = reader.CreateNewParser();
                    parser.OnTokenRead += new LALRParser.TokenReadHandler(TokenReadEvent);
                    parser.OnShift += new LALRParser.ShiftHandler(ShiftEvent);
                    parser.OnReduce += new LALRParser.ReduceHandler(ReduceEvent);
                    parser.OnGoto += new LALRParser.GotoHandler(GotoEvent);
                    parser.OnAccept += new LALRParser.AcceptHandler(AcceptEvent);
                    parser.OnTokenError += new LALRParser.TokenErrorHandler(TokenErrorEvent);
                    parser.OnParseError += new LALRParser.ParseErrorHandler(ParseErrorEvent);
                    parser.OnCommentRead += new LALRParser.CommentReadHandler(CommentReadEvent);
                    MessageBox.Show("Reading file compiled!");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error: " + e.ToString());
                } 
            }
        }

        private void TokenReadEvent(LALRParser parser, TokenReadEventArgs args)
        {
            AddViewItem("Token Read",
                args.Token.Location,
                args.Token.Symbol.ToString(),
                StringUtil.ShowEscapeChars(args.Token.Text), "", 0);
        }

        /// <summary>
        /// Event handler for the shift action.
        /// </summary>
        /// <param name="parser">parser that is the source of this event</param>
        /// <param name="args">event arguments</param>
        private void ShiftEvent(LALRParser parser, ShiftEventArgs args)
        {
            AddViewItem("Shift",
                args.Token.Location,
                args.Token.Symbol.ToString(),
                StringUtil.ShowEscapeChars(args.Token.Text),
                args.NewState.Id.ToString(),
                1);
        }

        /// <summary>
        /// Event handler for the reduce action.
        /// </summary>
        /// <param name="parser">parser that is the source of this event</param>
        /// <param name="args">event arguments</param>
        private void ReduceEvent(LALRParser parser, ReduceEventArgs args)
        {
            //WriteLn(args.Rule.Id.ToString());
            //WriteLn(args.Rule.Lhs.Id.ToString());
            AddViewItem("Reduce",
                null,
                args.Token.Rule.ToString(),
                "",
                args.NewState.Id.ToString(),
                2);
        }

        /// <summary>
        /// Event handlers for the goto action.
        /// </summary>
        /// <param name="parser">parser that is the source of this event</param>
        /// <param name="args">parser that is the source of this event</param>
        public void GotoEvent(LALRParser parser, GotoEventArgs args)
        {
            AddViewItem("Goto",
                null,
                args.Symbol.ToString(),
                "",
                args.NewState.Id.ToString(),
                3);
        }

        private void FillTreeViewRecursive(TreeView tree, TreeNode parentNode, Token token)
        {
            if (token is TerminalToken)
            {
                TerminalToken t = (TerminalToken)token;
                TreeNode node = new TreeNode(StringUtil.ShowEscapeChars(t.Text), 0, 0);
                parentNode.Nodes.Add(node);
            }
            else if (token is NonterminalToken)
            {
                NonterminalToken t = (NonterminalToken)token;
                TreeNode node = new TreeNode(t.Rule.ToString(), 1, 1);
                parentNode.Nodes.Add(node);
                foreach (Token childToken in t.Tokens)
                {
                    FillTreeViewRecursive(tree, node, childToken);
                }
            }
        }

        private void FillTreeView(TreeView tree, NonterminalToken token)
        {
            TreeNode node = new TreeNode(token.Rule.ToString(), 1, 1);
            tree.Nodes.Add(node);
            foreach (Token childToken in token.Tokens)
            {
                FillTreeViewRecursive(tree, node, childToken);
            }
        }

        private void AcceptEvent(LALRParser parser, AcceptEventArgs args)
        {
            AddViewItem("Accept", null, "", "", "", 4);
            FillTreeView(parseTreeView, args.Token);
            tabControl.SelectedIndex = 2;
        }

        private void TokenErrorEvent(LALRParser parser, TokenErrorEventArgs args)
        {
            AddViewItem("Token error", args.Token.Location, "Cannot recognize token",
                args.Token.Text, "", 5);
            errors++;
            if (errors <= maxerrors)
                args.Continue = true;
        }

        private void ParseErrorEvent(LALRParser parser, ParseErrorEventArgs args)
        {
            AddViewItem("Parse error", null, "Expecting the following tokens:",
                args.ExpectedTokens.ToString(), "", 5);
            errors++;

            if (errors <= maxerrors)
            {
                args.Continue = ContinueMode.Skip;
                // example for inserting a new token
                /*
				args.Continue = ContinueMode.Insert;
				TerminalToken token = new TerminalToken((SymbolTerminal)parser.Symbols.Get(4),
				                                        "555",
				                                        new Location(0, 0, 0));
				args.NextToken = token;
				*/
            }
        }

        /// <summary>
        /// Event handler for the reading a comment.
        /// </summary>
        /// <param name="parser">parser that is the source of this event</param>
        /// <param name="args">event arguments</param>
        private void CommentReadEvent(LALRParser parser, CommentReadEventArgs args)
        {
            string desc;
            if (args.LineComment)
                desc = "Line Comment";
            else
                desc = "Block Comment";
            AddViewItem("Comment Read", null, desc, args.Content, "", 6);
        }

        private void AddViewItem(String action, Location location, String description,
            String value, String state, int type)
        {
            string[] strItems = new string[7];
            strItems[0] = action;
            if (location != null)
            {
                strItems[1] = location.Position.ToString();
                strItems[2] = (location.LineNr + 1).ToString();
                strItems[3] = (location.ColumnNr + 1).ToString();
            }
            strItems[4] = description;
            strItems[5] = StringUtil.ShowEscapeChars(value);
            strItems[6] = state;
            ListViewItem item = new ListViewItem(strItems, type);
            switch (type)
            {
                case 0: item.ForeColor = Color.Gray; break;
                case 1: break;
                case 2: break;
                case 3: break;
                case 4: item.ForeColor = Color.Green; break;
                case 5: item.ForeColor = Color.Red; break;
                case 6: item.ForeColor = Color.Gray; break;
            }
            parseActionsView.Items.Add(item);
        }



        private void OpenSource()
        {
            if (openSourceDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    RichTextBoxStreamType type;
                    if (FileUtil.IsUTF16LE(openSourceDialog.FileName))
                        type = RichTextBoxStreamType.UnicodePlainText;
                    else
                        type = RichTextBoxStreamType.PlainText;
                    sourceTextBox.LoadFile(openSourceDialog.FileName, type);
                    if (type == RichTextBoxStreamType.UnicodePlainText)
                        sourceTextBox.Text = sourceTextBox.Text.Remove(0, 1);
                }
                catch (IOException e)
                {
                    MessageBox.Show(this, e.Message, "Error opening file");
                }
            }
        }

        private void ParseSource()
        {
            if (parser != null)
            {
                errors = 0;
                parseActionsView.Items.Clear();
                parseTreeView.Nodes.Clear();
                tabControl.SelectedIndex = 1;
                try
                {
                    string str = sourceTextBox.Text;
                    parser.Parse(str);
                }
                catch (System.ApplicationException e)
                {
                    MessageBox.Show("Error: "+e.ToString());
                    AddViewItem("Internal error", null, "View log for details", "", "", 5);
                }
            }
        }
        /*
        private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            if (e.Button == openGrammarButton)
            {
                OpenGrammar();
            }
            else if (e.Button == logButton)
            {
                ActivateLog();
            }
            else if (e.Button == testGrammarButton)
            {
                ActivateTest();
            }
            else if (e.Button == exitButton)
            {
                Application.Exit();
            }
        }

        private void openGrammarClick(object sender, System.EventArgs e)
        {
            OpenGrammar();
        }

        private void exitClick(object sender, System.EventArgs e)
        {
            Application.Exit();
        }

        private void aboutClick(object sender, System.EventArgs e)
        {
            Form aboutForm = new AboutForm();
            aboutForm.ShowDialog();

            /*
			String message =
				"Calitha GOLD Parser Engine Test 1.9\n\n"+
				"Author: Robert van Loenhout\n\n"+
				"Website: http://www.calitha.com\n\n"+
				"For more information about the GOLD parser see http://www.goldparser.com\n\n";

			MessageBox.Show(message, "About Gold Parser Engine Test", 
				MessageBoxButtons.OK, MessageBoxIcon.Information);*/
          /*
        }

        private void testGrammarClick(object sender, System.EventArgs e)
        {
            ActivateTest();
        }

        private void viewLogClick(object sender, System.EventArgs e)
        {
            ActivateLog();
        }

        private void parseButton_Click(object sender, System.EventArgs e)
        {
            ParseSource();
        }

        private void trimReductionsBox_CheckedChanged(object sender, System.EventArgs e)
        {
            settings.TrimReductions = trimReductionsBox.Checked;
            SaveSettings();
        }

        private void maxErrorsBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            try
            {
                string maxerrorsstr = maxErrorsBox.Items[maxErrorsBox.SelectedIndex].ToString();
                maxerrors = Int32.Parse(maxerrorsstr);
                settings.MaxErrorsIndex = maxErrorsBox.SelectedIndex;
                SaveSettings();
            }
            catch
            {
                maxerrors = 0;
            }
        }

        private void setFontMenuItem_Click(object sender, System.EventArgs e)
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                settings.Font = fontDialog.Font;
                SetFontInViews();
                SaveSettings();
            }
        }

        private void SetFontInViews()
        {
            logBox.Font = settings.Font;
            sourceTextBox.Font = settings.Font;
            parseActionsView.Font = settings.Font;
            parseTreeView.Font = settings.Font;
        }

        private void openSourceButton_Click(object sender, System.EventArgs e)
        {
            OpenSource();
        } */
    }
}
