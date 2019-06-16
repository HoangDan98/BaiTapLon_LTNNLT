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

        }


        private void OpenGrammar()
        {
            /*    settings.LastGrammar = openGrammarDialog.FileName;
                SaveSettings();
                WriteLn("Reading file...");
                long t1 = DateTime.Now.Ticks;
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
                    long t2 = DateTime.Now.Ticks;
                MessageBox.Show("Load file compiled !");
                }
                catch (Exception e)
                {
                MessageBox.Show("Error: " + e.ToString());
                }
                */
        }

        private void mainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
