using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Windows.Forms;

namespace ORC
{
    public partial class frmORC : Form
    {

        string strOpenedBook = "";
        public string strIDPrefix = "";
        string strOpenedIndex = "";
        string[] strContentLines;
        string strPrimaryIndex, strSecondaryIndex;
        long lnConvertedID, lnConvertedLabel, lnConvertedCondition, lnConvertedFigure, lnConvertedIndex;
        long lnIDTrailingNo=0;
        long lnFnsConverted = 0;
        public string[,] strEntityRepl;
        public string[,] strPUKFileNames;
        
        System.Collections.Stack stkIDs;


        public frmORC()
        {
            lnConvertedLabel = 0;
            lnConvertedCondition = 0;
            lnConvertedFigure = 0;
            lnConvertedIndex = 0;
            InitializeComponent();
            lnFnsConverted = 0;
        }


        


        private void btnBrowse_Click(object sender, EventArgs e)
        {

            ConvertFile();
        }


        private void ConvertFile()
        {
            Application.UseWaitCursor = true;

            ofdOpen.Title = "Select an xml file to create ID...";
            ofdOpen.Filter = "XML Files (*.xml)|*.xml";
            ofdOpen.FilterIndex = 0;
            ofdOpen.RestoreDirectory = true;
            string strContent = "";

            string[] strLines;

            if (ofdOpen.ShowDialog() == DialogResult.OK)
            {
                //txtOpen.Text = ofdOpen.FileName.ToString();


                StreamReader sr = new StreamReader(ofdOpen.FileName.ToString());


                strContent = sr.ReadToEnd();
                strLines = strContent.Split('\n');
                long i = strLines.Length;


                string strID = "";

                //Creating IDs
                for (int j = 0; j < i; j++)
                {

                    if (strLines[j].StartsWith("<title>"))
                    {
                        strID = CreateID(Regex.Replace(strLines[j], "<title>(.*)</title>", "$1"));

                        if (strLines[j - 1].IndexOf("id=\"*") >= 0)
                        {
                            strLines[j - 1] = strLines[j - 1].Insert(strLines[j - 1].IndexOf("id=") + 4, strID).Replace("*", "");

                        }
                        else
                        {
                            if (strLines[j - 2].IndexOf("id=\"*") >= 0)
                            {
                                strLines[j - 2] = strLines[j - 2].Insert(strLines[j - 2].IndexOf("id=") + 4, strID).Replace("*", "");
                            }
                        }



                    }

                }

                toolStripProgressBar1.Maximum = Convert.ToInt32(i);
                toolStripProgressBar1.Minimum = 1;
                toolStripProgressBar1.Value = 1;
                toolStripStatusLabel1.Text = "Converting... Please Wait";
                this.Refresh();

                long lnChapter = 0;
                long lnSect1 = 0;
                long lnSect2 = 0;

                long lnFigure = 0;
                long lnTable = 0;
                long lnExample = 0;

                Boolean blAfterAppendix = false;

                string strLabel = "";

                //Creating Labels
                for (int j = 0; j < i; j++)
                {

                    if (strLines[j].StartsWith("<chapter"))
                    {
                        lnChapter++;

                        strLabel = " label=\"" + lnChapter.ToString() + "\"";
                        strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                        lnSect1 = 0;
                        lnSect2 = 0;
                        lnFigure = 0;
                        lnTable = 0;
                        lnExample = 0;



                    }

                    if (strLines[j].StartsWith("<sect1"))
                    {
                        lnSect1++;
                        if (blAfterAppendix)
                        {
                            strLabel = " label=\"A." + lnSect1.ToString() + "\"";
                        }
                        else
                        {
                            strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "\"";
                        }
                        strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                        lnSect2 = 0;

                    }


                    if (strLines[j].StartsWith("<sect2"))
                    {
                        lnSect2++;
                        if (blAfterAppendix)
                        {
                            strLabel = " label=\"A." + lnSect1.ToString() + "." + lnSect2.ToString() + "\"";
                        }
                        else
                        {
                            strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "\"";
                        }
                        strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                    }


                    if (strLines[j].StartsWith("<figure"))
                    {
                        lnFigure++;
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnFigure.ToString() + "\"";
                        strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                    }

                    if (strLines[j].StartsWith("<table"))
                    {
                        lnTable++;
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnTable.ToString() + "\"";
                        strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                    }

                    if (strLines[j].StartsWith("<example"))
                    {
                        lnExample++;
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnExample.ToString() + "\"";
                        strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                    }



                    if (strLines[j].StartsWith("<appendix"))
                    {

                        strLabel = " label=\"A\"";
                        strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);
                        blAfterAppendix = true;
                        lnSect1 = 0;
                        lnSect2 = 0;

                    }



                }


                //Condition|Page No...

                string strCondition = "";
                //<?docpage num="25"?>
                for (long j = i - 1; j >= 0; j--)
                {
                    if (strLines[j].IndexOf("<?docpage") >= 0)
                    {
                        strCondition = Regex.Replace(strLines[j], "(.*)<?docpage([^>]+)\"([^>]+)\"(.*)", "$3");
                        //MessageBox.Show(strCondition);  
                    }



                    if (strLines[j].StartsWith("<chapter") || strLines[j].StartsWith("<sect1") || strLines[j].StartsWith("<sect2") || strLines[j].StartsWith("<figure") || strLines[j].StartsWith("<table") || strLines[j].StartsWith("<example") || strLines[j].StartsWith("<appendix"))
                    {

                        strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, " condition=\"" + strCondition + "\"");

                    }



                }





                string strOutput = "";
                //Showing...
                for (int j = 0; j < i; j++)
                {

                    strOutput = strOutput + strLines[j] + "\n";
                    toolStripProgressBar1.Increment(1);
                }

                rtbContent.Text = strOutput;
                toolStripStatusLabel1.Text = "Ready";
                Application.UseWaitCursor = false;
            }
        }



        private string CreateID(string strTitle)
        {
            //string strID = "";

            strTitle = Regex.Replace(strTitle, "<[^>]+>", "");
            strTitle = strTitle.ToLower();

            strTitle = Regex.Replace(strTitle, "^([0-9]+)(.*)", "$2");
            //strTitle = Regex.Replace(strTitle, "^(.*)([_.]+)", "$1");

            strTitle = Regex.Replace(strTitle, "&([a-z])[a-z]+;", "$1");
            strTitle = Regex.Replace(strTitle, "&#[^ ;]+;", "#");

            strTitle = ReplaceInvalidChar(strTitle).Replace("  ", " ").Trim();
            strTitle = strTitle.Replace(" ", "_");  

            //strTitle = strTitle.Remove(strTitle.Length - 1);

            if (strTitle.Length > 40)
            {
                strTitle = strTitle.Remove(40);
            }

            strTitle = Regex.Replace(strTitle, "^(.*)([_.]+)$", "$1");

            if (stkIDs.Contains(strTitle))
            {

                lnIDTrailingNo++;
                
                //MessageBox.Show("Before -- " + strTitle + " - "  + stkIDs.Count.ToString());

                if (strTitle.Length > 36)
                {
                    strTitle = strTitle.Remove(36);
                }

                strTitle = strTitle + "-" + lnIDTrailingNo.ToString("000");

                //MessageBox.Show("After -- " + strTitle + " - " + stkIDs.Count.ToString());
                
            }
            
            stkIDs.Push(strTitle);

            return strTitle;


        }


        private string ReplaceInvalidChar(string strTitle)
        {

            strTitle = strTitle.Replace("_", " underscore ");

            strTitle = strTitle.Replace("~", " tilde ");
            strTitle = strTitle.Replace("!", " exclamation ");
            strTitle = strTitle.Replace("@", " at_the_rate ");
            strTitle = strTitle.Replace("#", " number_symble ");
            strTitle = strTitle.Replace("$", " dollar ");
            strTitle = strTitle.Replace("%", " percent ");
            strTitle = strTitle.Replace("^", " carret ");
            strTitle = strTitle.Replace("&", " ampersand ");
            strTitle = strTitle.Replace("*", " asterisk ");
            strTitle = strTitle.Replace("(", " open_parenthesis ");
            strTitle = strTitle.Replace(")", " close_parenthesis ");
            
            strTitle = strTitle.Replace("+", " plus ");
            strTitle = strTitle.Replace("=", " equals ");
            strTitle = strTitle.Replace("\\", " reverse_solidus ");
            strTitle = strTitle.Replace("]", " close_square ");
            strTitle = strTitle.Replace("[", " open_square ");
            strTitle = strTitle.Replace("{", " open_curly ");
            strTitle = strTitle.Replace("}", " close_curly ");
            strTitle = strTitle.Replace("'", " apostrophy ");
            strTitle = strTitle.Replace(";", " semicolon ");
            strTitle = strTitle.Replace(":", " colon ");
            strTitle = strTitle.Replace("\"", " quotation_mark ");
            strTitle = strTitle.Replace("/", " solidus ");
            strTitle = strTitle.Replace("?", " question ");
            strTitle = strTitle.Replace(">", " greater_than ");
            strTitle = strTitle.Replace("<", " less_than ");
            strTitle = strTitle.Replace(",", " comma ");

            return strTitle;

        }

        private void frmORC_Load(object sender, EventArgs e)
        {

            //MessageBox.Show(Application.ProductVersion.ToString());   

            //MessageBox.Show("TestKannan".IndexOf("ka",StringComparison.InvariantCultureIgnoreCase  ).ToString());  
            this.Text = Application.ProductName.ToString(); 

            AppValidation();
            toolStripStatusLabel1.Text = "Ready";
            splitContainer1.Panel2.Hide();
            splitContainer1.Panel2Collapsed = true;

            splitContainer2.Panel2.Hide();

            splitContainer2.Panel2Collapsed = true; 
        }


        private void AppValidation()
        {

            try
            {

                StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.System).ToString() + "\\winadokx39.dll");
                string strMsg = "";

                if (!sr.EndOfStream)
                {
                    strMsg = sr.ReadToEnd();
                }

                sr.Close();
 
                if (strMsg != "No")
                {
                    //OK

                }
                else
                {

                    //Check webservice



                    if (strMsg == "No")
                    {

                        MessageBox.Show("Application License has Expired!\nPlease contact kannankr.in", "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        this.Close();
                        this.Dispose();
                   
                    }
                    else
                    {
                        MessageBox.Show(strMsg, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        this.Close();
                        this.Dispose();
                    }
                    
                }

            }
            catch
            {
                //Do nothing....

            }




        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Exit");  
            Application.Exit();  
        }

        private void oToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenBook();
        }

        private void OpenBook()
        {
            Application.UseWaitCursor = true;

            ofdOpen.Title = "Select a file to open...";
            ofdOpen.Filter = "XHTML Files (*.xhtml)|*.xhtml|XML Files (*.xml)|*.xml|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            ofdOpen.FilterIndex = 0;
            ofdOpen.RestoreDirectory = true;

            try
            {
                if (ofdOpen.ShowDialog() == DialogResult.OK)
                {
                    strOpenedBook = ofdOpen.FileName.ToString();
                    this.Text = Application.ProductName.ToString() + " | " + strOpenedBook;
                    WriteLog("Open Book\t" + strOpenedBook);
                    this.Refresh(); 
                    rtbContent.LoadFile(ofdOpen.FileName.ToString(), RichTextBoxStreamType.PlainText);

                }
            }
            catch
            {
                strOpenedBook = "";
                MessageBox.Show("Unable to open file!", "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);  
            }
            Application.UseWaitCursor = false;

        }

        private void createIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (strOpenedBook.Length > 3)
            {
                //ConvertFileNew();
                lnIDTrailingNo = 0;
                ConvertFileNewID();
                WriteLog("Convert IDs\t" + strOpenedBook);
                lnConvertedID++;
            }
            else
            {
                MessageBox.Show("Unable to convert file!", "Create ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);  
            }
        }


        private void ConvertFileNewID()
        {
            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Creating IDs... Please Wait";
            this.Refresh();  
            string strContent = "";

            string[] strLines;

            stkIDs = new System.Collections.Stack();
            stkIDs.Clear();
             

            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            toolStripProgressBar1.Maximum = Convert.ToInt32(i);
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            this.Refresh(); 


            string strID = "";

            #region First Loop


            //Creating IDs
            for (int j = 0; j < i; j++)
            {

                if (strLines[j].StartsWith("<preface") || strLines[j].StartsWith("<chapter") || strLines[j].StartsWith("<sect") || strLines[j].StartsWith("<figure") || strLines[j].StartsWith("<table") || strLines[j].StartsWith("<sidebar") || strLines[j].StartsWith("<example") || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                {
                    //MessageBox.Show(strLines[j]);
                    strLines[j] = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$1$3");
                    //MessageBox.Show(strLines[j]);
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), " id=\"****\"");
                    //MessageBox.Show(strLines[j]);

                }





                toolStripProgressBar1.Increment(1);
                if (strLines[j].StartsWith("<title>"))
                {
                    strID = CreateID(Regex.Replace(strLines[j], "<title>(.*)</title>", "$1"));

                    if (strLines[j - 1].IndexOf("id=\"*") >= 0)
                    {

                        /*if (strLines[j - 1].StartsWith("<preface"))
                        {
                            strLines[j - 1] = strLines[j - 1].Insert(strLines[j - 1].IndexOf("id=") + 4, "preface").Replace("*", "");
                        }
                        else
                        {
                        */
                            strLines[j - 1] = strLines[j - 1].Insert(strLines[j - 1].IndexOf("id=") + 4, strID).Replace("*", "");
                        //}
                    }
                    else
                    {
                        if (strLines[j - 2].IndexOf("id=\"*") >= 0)
                        {
                            /*
                            if (strLines[j - 2].StartsWith("<preface"))
                            {
                                strLines[j - 2] = strLines[j - 2].Insert(strLines[j - 2].IndexOf("id=") + 4, "preface").Replace("*", "");
                            }
                            else
                            {
                             */ 
                                strLines[j - 2] = strLines[j - 2].Insert(strLines[j - 2].IndexOf("id=") + 4, strID).Replace("*", "");
                            //}
                        }
                    }



                }

            }

            #endregion

            
            this.Refresh();

            rtbContent.Text = string.Join("\n", strLines);
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;




        }




        private void ConvertFileNewLabel()
        {
            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Creating Labels... Please Wait";
            this.Refresh();
            string strContent = "";

            string[] strLines;

            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            toolStripProgressBar1.Maximum = Convert.ToInt32(i);
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            this.Refresh();


            long lnPart = 0;

            long lnChapter = 0;
            long lnSect1 = 0;
            long lnSect2 = 0;
            long lnSect3 = 0;
            long lnSect4 = 0;
            long lnSect5 = 0;
            long lnSect6 = 0;
            long lnSect7 = 0;

            long lnFigure = 0;
            long lnTable = 0;
            long lnExample = 0;

            Boolean blAfterAppendix = false;
            string strAppLabel = "";
            long lnAppLabelCount = 0;

            string strLabel = "";

            //Creating Labels
            toolStripStatusLabel1.Text = "Creating Labels... Please Wait";
            this.Refresh();
            #region Label


            for (int j = 0; j < i; j++)
            {



                toolStripProgressBar1.Increment(1);

                if (strLines[j].StartsWith("<part") || strLines[j].StartsWith("<chapter") || strLines[j].StartsWith("<sect") || strLines[j].StartsWith("<figure") || strLines[j].StartsWith("<table") || strLines[j].StartsWith("<sidebar") || strLines[j].StartsWith("<example") || strLines[j].StartsWith("<appendix")) // || strLines[j].StartsWith("<preface")
                {
                    //MessageBox.Show(strLines[j]);
                    strLines[j] = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$1$3");
                    //MessageBox.Show(strLines[j]);
                    //strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), " id=\"****\"");
                    //MessageBox.Show(strLines[j]);

                }




                if (strLines[j].StartsWith("<part"))
                {
                    lnPart++;

                    strLabel = " label=\"" + lnPart.ToString() + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                    /*
                    lnChapter = 0;
                    lnSect1 = 0;
                    lnSect2 = 0;
                    lnSect3 = 0;
                    lnSect4 = 0;
                    lnSect5 = 0;
                    lnSect6 = 0;
                    lnSect7 = 0;
                    


                    lnFigure = 0;
                    lnTable = 0;
                    lnExample = 0;
                    */
                    

                }





                if (strLines[j].StartsWith("<chapter"))
                {
                    lnChapter++;

                    /*if (lnPart > 0)
                    {
                        strLabel = " label=\"" + lnPart.ToString() + "." + lnChapter.ToString() + "\"";
                    }
                    else
                    {*/

                        strLabel = " label=\"" + lnChapter.ToString() + "\"";
                    //}
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                    lnSect1 = 0;
                    lnSect2 = 0;
                    lnSect3 = 0;
                    lnSect4 = 0;
                    lnSect5 = 0;
                    lnSect6 = 0;
                    lnSect7 = 0;



                    lnFigure = 0;
                    lnTable = 0;
                    lnExample = 0;



                }

                if (strLines[j].StartsWith("<sect1"))
                {
                    lnSect1++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnSect1.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                    lnSect2 = 0;
                    lnSect3 = 0;
                    lnSect4 = 0;
                    lnSect5 = 0;
                    lnSect6 = 0;
                    lnSect7 = 0;


                }


                if (strLines[j].StartsWith("<sect2"))
                {
                    lnSect2++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                    lnSect3 = 0;
                    lnSect4 = 0;
                    lnSect5 = 0;
                    lnSect6 = 0;
                    lnSect7 = 0;



                }



                if (strLines[j].StartsWith("<sect3"))
                {
                    lnSect3++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString()  +"\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                    
                    lnSect4 = 0;
                    lnSect5 = 0;
                    lnSect6 = 0;
                    lnSect7 = 0;



                }


                if (strLines[j].StartsWith("<sect4"))
                {
                    lnSect4++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "." + lnSect4.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "." + lnSect4.ToString() + "\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);


                    lnSect5 = 0;
                    lnSect6 = 0;
                    lnSect7 = 0;



                }

                if (strLines[j].StartsWith("<sect5"))
                {
                    lnSect5++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "." + lnSect4.ToString() + "." + lnSect5.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "." + lnSect4.ToString() + "." + lnSect5.ToString() + "\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);


                    lnSect6 = 0;
                    lnSect7 = 0;



                }


                if (strLines[j].StartsWith("<sect6"))
                {
                    lnSect6++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "." + lnSect4.ToString() + "." + lnSect5.ToString() + "." + lnSect6.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "." + lnSect4.ToString() + "." + lnSect5.ToString() + "." + lnSect6.ToString() + "\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);


                    lnSect7 = 0;



                }


                if (strLines[j].StartsWith("<sect7"))
                {
                    lnSect7++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "." + lnSect4.ToString() + "." + lnSect5.ToString() + "." + lnSect6.ToString() + "." + lnSect7.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "." + lnSect3.ToString() + "." + lnSect4.ToString() + "." + lnSect5.ToString() + "." + lnSect6.ToString() + "." + lnSect7.ToString() + "\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                    



                }



                if (strLines[j].StartsWith("<figure"))
                {
                    lnFigure++;

                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnFigure.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnFigure.ToString() + "\"";
                    }


                    //strLabel = " label=\"" + lnChapter.ToString() + "." + lnFigure.ToString() + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                }

                if (strLines[j].StartsWith("<table"))
                {
                    lnTable++;


                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"" + strAppLabel + "." + lnTable.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnTable.ToString() + "\"";
                    }


                    //strLabel = " label=\"" + lnChapter.ToString() + "." + lnTable.ToString() + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                }

                if (strLines[j].StartsWith("<example"))
                {
                    lnExample++;
                    strLabel = " label=\"" + lnChapter.ToString() + "." + lnExample.ToString() + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);

                }



                if (strLines[j].StartsWith("<appendix"))
                {

                    lnAppLabelCount++;
                    /*
                    if (strLines[j + 1].StartsWith("<title"))
                    {
                        strAppLabel = Regex.Replace(strLines[j + 1], "<title>(Appendix|APPENDIX) ([^ ])</title>", "$2");
                    }
                    else
                    {
                        if (strLines[j + 2].StartsWith("<title"))
                        {
                            strAppLabel = Regex.Replace(strLines[j + 2], "<title>(Appendix|APPENDIX) ([^ ])</title>", "$2");
                        }
                        else
                        {
                    

                        }

                    }
                    */

                    string[] strAppLabelAlfa = "A B C D E F G H I J K L M N O P Q R S T U V W X Y Z".Split(' ');

                    strAppLabel = strAppLabelAlfa[lnAppLabelCount - 1];
                    

                    strLabel = " label=\"" + strAppLabel + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), strLabel);
                    blAfterAppendix = true;
                    lnSect1 = 0;
                    lnSect2 = 0;
                    lnSect3 = 0;
                    lnSect4 = 0;
                    lnSect5 = 0;
                    lnSect6 = 0;
                    lnSect7 = 0;

                    lnFigure = 0;
                    lnTable = 0;
                    lnExample = 0;

                    //strAppLabel = strAppLabel; //+".1";

                }



            }

            #endregion

            this.Refresh();

            rtbContent.Text = string.Join("\n", strLines);
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;

        }

        private void ConvertFileNewCondition()
        {


            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Creating Conditions... Please Wait";
            this.Refresh();
            string strContent = "";

            string[] strLines;

            
            
            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            toolStripProgressBar1.Maximum = Convert.ToInt32(i*2);
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            this.Refresh();




            //Condition|Page No...
            string strCondition = "";
            //<?docpage num="25"?>

            #region Third Loop




            for (long j = i - 1; j >= 0; j--)
            {
                toolStripProgressBar1.Increment(1);

                //Removing Conditions
                if (strLines[j].StartsWith("<chapter") || strLines[j].StartsWith("<sect") || strLines[j].StartsWith("<figure") || strLines[j].StartsWith("<table") || strLines[j].StartsWith("<sidebar") || strLines[j].StartsWith("<example") || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<preface") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                {
                    //MessageBox.Show(strLines[j]);
                    strLines[j] = Regex.Replace(strLines[j], "^(.*) condition=\"([^\"]*)\"(.*)$", "$1$3");
                    //MessageBox.Show(strLines[j]);
                    //strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), " id=\"****\"");
                    //MessageBox.Show(strLines[j]);

                }


                //Capturing Conditions
                if (strLines[j].IndexOf("<?docpage") >= 0)
                {
                    strCondition = Regex.Replace(strLines[j], "(.*)<?docpage([^>]+)\"([^>]+)\"(.*)", "$3");
                    //MessageBox.Show(strCondition);  
                }


                //Inserting Conditions on Chapter
                if (strLines[j].StartsWith("<chapter"))
                {

                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), " condition=\"" + strCondition + "\"");

                }



            }
            #endregion


            #region Fourth Loop



            for (long j = 0; j < i; j++)
            {
                //Capturing Conditions
                toolStripProgressBar1.Increment(1);
                if (strLines[j].IndexOf("<?docpage") >= 0)
                {
                    strCondition = Regex.Replace(strLines[j], "(.*)<?docpage([^>]+)\"([^>]+)\"(.*)", "$3");
                    //MessageBox.Show(strCondition);  
                }



                if ((strLines[j].StartsWith("<sect") || strLines[j].StartsWith("<preface") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary") || strLines[j].StartsWith("<appendix")) && strLines[j + 1].StartsWith("<?docpage num"))
                {
                    strCondition = Regex.Replace(strLines[j + 1], "(.*)<?docpage([^>]+)\"([^>]+)\"(.*)", "$3");

                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), " condition=\"" + strCondition + "\"");

                }
                else
                {


                    if (strLines[j].StartsWith("<sect") || strLines[j].StartsWith("<preface") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary") || strLines[j].StartsWith("<appendix"))
                    {
                        if (strLines[j + 1].StartsWith("<?docpage cont") == false)
                        {
                            strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), " condition=\"" + strCondition + "\"") + "\n<?docpage cont page=\"" + strCondition + "\"?>";
                        }
                        else
                        {
                            strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), " condition=\"" + strCondition + "\"");
                        }
                    }
                    else
                    {
                        /*
                        if (strLines[j].StartsWith("<appendix"))
                        {
                            strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf(">"), " condition=\"" + strCondition + "\"");
                        }
                        */
                    }


                }


            }


            #endregion


            this.Refresh();

            rtbContent.Text = string.Join("\n", strLines);
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;



        }


        private void ConvertFileNew()
        {
            Application.UseWaitCursor = true;

            string strContent = "";

            string[] strLines;



            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            toolStripProgressBar1.Maximum = Convert.ToInt32(i*4);
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;

               

            string strID = "";

            #region First Loop

            
            //Creating IDs
            for (int j = 0; j < i; j++)
            {
                toolStripProgressBar1.Increment(1); 
                if (strLines[j].StartsWith("<title>"))
                {
                    strID = CreateID(Regex.Replace(strLines[j], "<title>(.*)</title>", "$1"));

                    if (strLines[j - 1].IndexOf("id=\"*") >= 0)
                    {
                        strLines[j - 1] = strLines[j - 1].Insert(strLines[j - 1].IndexOf("id=") + 4, strID).Replace("*", "");

                    }
                    else
                    {
                        if (strLines[j - 2].IndexOf("id=\"*") >= 0)
                        {
                            strLines[j - 2] = strLines[j - 2].Insert(strLines[j - 2].IndexOf("id=") + 4, strID).Replace("*", "");
                        }
                    }



                }

            }

            #endregion

            toolStripStatusLabel1.Text = "Creating IDs... Please Wait";
            this.Refresh();

            long lnChapter = 0;
            long lnSect1 = 0;
            long lnSect2 = 0;

            long lnFigure = 0;
            long lnTable = 0;
            long lnExample = 0;

            Boolean blAfterAppendix = false;

            string strLabel = "";

            //Creating Labels
            toolStripStatusLabel1.Text = "Creating Labels... Please Wait";
            this.Refresh();
            #region Second Loop

            
            for (int j = 0; j < i; j++)
            {
                toolStripProgressBar1.Increment(1);
                if (strLines[j].StartsWith("<chapter"))
                {
                    lnChapter++;

                    strLabel = " label=\"" + lnChapter.ToString() + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                    lnSect1 = 0;
                    lnSect2 = 0;
                    lnFigure = 0;
                    lnTable = 0;
                    lnExample = 0;



                }

                if (strLines[j].StartsWith("<sect1"))
                {
                    lnSect1++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"A." + lnSect1.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                    lnSect2 = 0;

                }


                if (strLines[j].StartsWith("<sect2"))
                {
                    lnSect2++;
                    if (blAfterAppendix)
                    {
                        strLabel = " label=\"A." + lnSect1.ToString() + "." + lnSect2.ToString() + "\"";
                    }
                    else
                    {
                        strLabel = " label=\"" + lnChapter.ToString() + "." + lnSect1.ToString() + "." + lnSect2.ToString() + "\"";
                    }
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                }


                if (strLines[j].StartsWith("<figure"))
                {
                    lnFigure++;
                    strLabel = " label=\"" + lnChapter.ToString() + "." + lnFigure.ToString() + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                }

                if (strLines[j].StartsWith("<table"))
                {
                    lnTable++;
                    strLabel = " label=\"" + lnChapter.ToString() + "." + lnTable.ToString() + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                }

                if (strLines[j].StartsWith("<example"))
                {
                    lnExample++;
                    strLabel = " label=\"" + lnChapter.ToString() + "." + lnExample.ToString() + "\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);

                }



                if (strLines[j].StartsWith("<appendix"))
                {

                    strLabel = " label=\"A\"";
                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, strLabel);
                    blAfterAppendix = true;
                    lnSect1 = 0;
                    lnSect2 = 0;

                }



            }

            #endregion

            //Condition|Page No...
            toolStripStatusLabel1.Text = "Creating Conditions... Please Wait";
            this.Refresh();
            string strCondition = "";
            //<?docpage num="25"?>
            
            #region Third Loop

            
                

            for (long j = i - 1; j >= 0; j--)
            {
                toolStripProgressBar1.Increment(1);
                if (strLines[j].IndexOf("<?docpage") >= 0)
                {
                    strCondition = Regex.Replace(strLines[j], "(.*)<?docpage([^>]+)\"([^>]+)\"(.*)", "$3");
                    //MessageBox.Show(strCondition);  
                }



                if (strLines[j].StartsWith("<chapter"))
                {

                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, " condition=\"" + strCondition + "\"");

                }



            }
            #endregion
            

            #region Fourth Loop

            

            for (long j = 0 ; j < i; j++)
            {
                toolStripProgressBar1.Increment(1);
                if (strLines[j].IndexOf("<?docpage") >= 0)
                {
                    strCondition = Regex.Replace(strLines[j], "(.*)<?docpage([^>]+)\"([^>]+)\"(.*)", "$3");
                    //MessageBox.Show(strCondition);  
                }



                if (strLines[j].StartsWith("<sect1") || strLines[j].StartsWith("<sect2") || strLines[j].StartsWith("<figure") || strLines[j].StartsWith("<table") || strLines[j].StartsWith("<example") || strLines[j].StartsWith("<appendix"))
                {

                    strLines[j] = strLines[j].Insert(strLines[j].LastIndexOf("\"") + 1, " condition=\"" + strCondition + "\"");

                }



            }


            #endregion





            /*
            //Showing...
            for (int j = 0; j < i; j++)
            {

                strOutput = strOutput + strLines[j] + "\n";
                toolStripProgressBar1.Increment(1);
            }
            */
            rtbContent.Text = string.Join("\n", strLines);   
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;

        }

        private void openIndexToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Application.UseWaitCursor = true;

            ofdOpen.Title = "Select an index file";
            ofdOpen.Filter = "XML Files (*.xml)|*.xml";
            ofdOpen.FilterIndex = 0;
            ofdOpen.RestoreDirectory = true;

            try
            {
                if (ofdOpen.ShowDialog() == DialogResult.OK)
                {
                    strOpenedIndex = ofdOpen.FileName.ToString();
                    lblIndex.Text = strOpenedIndex;
                    
                    WriteLog("Open Index\t" + strOpenedIndex);

                    this.Refresh();
                    rtbIndex.LoadFile(ofdOpen.FileName.ToString(), RichTextBoxStreamType.PlainText);

                    splitContainer1.Panel2.Show();
                    splitContainer1.Panel2Collapsed = false;
                    indexToolStripMenuItem.Checked = true;

                }
            }
            catch
            {
                strOpenedIndex = "";
                MessageBox.Show("Unable to open file!", "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            Application.UseWaitCursor = false;


        }

        private void saveBookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveBook();
        }

        private void SaveBook()
        {
            try
            {
                if (strOpenedBook.Length > 3)
                {
                    rtbContent.SaveFile(strOpenedBook, RichTextBoxStreamType.PlainText);
                    WriteLog("Save Book\t" + strOpenedBook);

                }
                else
                {
                    MessageBox.Show("No Opened file found!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            {
                MessageBox.Show("Unable to save file!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void saveBookAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveBookAs();
        }

        private void SaveBookAs()
        {
             
            sfdSave.Title = "Save Book";
            sfdSave.Filter = "XML Files (*.xml)|*.xml|XHTML Files (*.xhtml)|*.xhtml|HTML Files (*.html)|*.html";
            sfdSave.FilterIndex = 0;
            sfdSave.RestoreDirectory = true;

            try
            {
                if (sfdSave.ShowDialog() == DialogResult.OK)
                {


                    try
                    {
                        if (strOpenedBook.Length > 3)
                        {

                            rtbContent.SaveFile(sfdSave.FileName.ToString(), RichTextBoxStreamType.PlainText);
                            strOpenedBook = sfdSave.FileName.ToString();
                            this.Text = "ORC " + strOpenedBook;
                            WriteLog("Save As Book\t" + strOpenedBook);
                            this.Refresh(); 

                        }
                        else
                        {
                            MessageBox.Show("No Opened file found!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Unable to save file!", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }


                }

            }
            catch
            {

            }
        }

        private string EntityReplace(string strEntityLine, string strEntity, string strReplStr)
        {
            //string strRetString = strEntityLine.Replace(strEntity, strReplStr);
            string strRetString = strEntityLine; // Regex.Replace(strEntityLine, ">([^<]*)" + strEntity + "([^<]*)<", ">$1" + strReplStr + "$2<");
            int intStartTag = 0;
            int intEndTag = 0;
            int intNextStartTag = 0;
            int intEntityIndex = 0;
            int intCount = 0;
            string strLeft = "";
            string strMid = "";
            string strRight = "";

            

            if (strRetString.IndexOf("<", intEndTag) == -1 && strRetString.IndexOf(">", intEndTag) == -1)
            {
                strRetString = strRetString.Replace(strEntity, strReplStr);
                //MessageBox.Show(strRetString);  
            }



            intStartTag = strRetString.IndexOf("<", intEndTag);

            if (intStartTag > 0)
            {
                strLeft = strRetString.Substring(0, intStartTag);
                strMid = strRetString.Substring(intStartTag);
                strLeft = strLeft.Replace(strEntity, strReplStr);
                strRetString = strLeft + strMid;
                //MessageBox.Show(strLeft);
                //MessageBox.Show(strMid);
            }
            

            do
            {

                intStartTag = strRetString.IndexOf("<", intEndTag);
               
                if (intStartTag >= 0)
                {
                    intEndTag = strRetString.IndexOf(">", intStartTag);
                    if (intEndTag >= 0)
                    {
                        intNextStartTag = strRetString.IndexOf("<", intEndTag);
                        intCount = intNextStartTag - intEndTag;
                        if (intNextStartTag >= 0)
                        {
                            intEntityIndex = strRetString.IndexOf(strEntity, intEndTag, intCount);
                            strLeft = strRetString.Substring(0, intEndTag + 1);
                            strMid = strRetString.Substring(intEndTag + 1, intCount - 1);
                            strRight = strRetString.Substring(intNextStartTag);
                            strMid = strMid.Replace(strEntity, strReplStr);
                            strRetString = strLeft + strMid + strRight;
                        }
                        else
                        {
                            if (strRetString.Length > intEndTag + 1)
                            {
                                strLeft = strRetString.Substring(0, intEndTag + 1);
                                strMid = strRetString.Substring(intEndTag + 1);
                                strMid = strMid.Replace(strEntity, strReplStr);
                                strRetString = strLeft + strMid;
                                /*MessageBox.Show(strLeft);
                                MessageBox.Show(strMid);*/
                            }
                        }
                    }
                    else
                    {
                        intNextStartTag = -1;
                    }
                }
                else
                {
                    intNextStartTag = -1;
                }
            } while (intNextStartTag>=0);



            return strRetString;
        }


        private void createIndexToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //MessageBox.Show(Regex.Replace("<?docpage num=\"iv\"?>mmmmmmmmmmm<?docpage num=\"iv\"?>", "<?docpage([^>]+)\"([^>\"]+)\"", "<a id=\"page_$2\"></a>", RegexOptions.RightToLeft));

            //MessageBox.Show(GeneralReplace("99999999<?docpage cont page=\"4\"?>9999"));  
            MessageBox.Show(EntityReplace("hhjkh\"khk<?docpage num=\"iv\"?>mm<b>m'm</b> m\"mmjjj'jj\"mmmm<?docpage num=\"iv\"?>", "\"", "&quot;"));  

            /*
            rtbErrorLog.Text = "";
            CreateIndex();
            //RemoveIndex();
            WriteLog("Convert Indexs\t" + strOpenedBook + "\t" + strOpenedIndex);
            lnConvertedIndex++;
            */
        }

        private void RemoveIndex()
        {
            strContentLines = rtbContent.Text.Split('\n');

            for (int j = 0; j < strContentLines.Length; j++)
            {

                //MessageBox.Show(strContentLines[j].IndexOf("indexterm").ToString());
   
                if (strContentLines[j].IndexOf("indexterm") >= 0)
                {
                    //MessageBox.Show(strContentLines[j]);
                    strContentLines[j] = Regex.Replace(strContentLines[j], "^(.*)<indexterm(.*)</indexterm></para>$", "$1</para>", RegexOptions.RightToLeft );
                    strContentLines[j] = Regex.Replace(strContentLines[j], "^(.*)<indexterm(.*)</indexterm></para></(entry|listitem)>$", "$1</para></$3>", RegexOptions.RightToLeft);
                    //MessageBox.Show(strContentLines[j]);
                }
                else
                {

                   // MessageBox.Show(strContentLines[j]);
                }


            }

            rtbContent.Text = string.Join("\n", strContentLines);

        }


        private void CreateIndex()
        {

            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Removing Existing Indexes... Please Wait";
            this.Refresh();  
            
            
            
            string[] strIndexLines;
            string strText2Find = "";
            string strPage2Find = "";

            strContentLines = rtbContent.Text.Split('\n');
            strIndexLines = rtbIndex.Text.Split('\n');

            for (int j = 0; j < strContentLines.Length; j++)
            {

                if (strContentLines[j].IndexOf("<indexterm") > 0)
                {
                    //MessageBox.Show(strContentLines[j]);
                    //strContentLines[j] = Regex.Replace(strContentLines[j], "^(.*)<indexterm(.*)</indexterm></para>$", "$1</para>", RegexOptions.RightToLeft);
                    strContentLines[j] = Regex.Replace(strContentLines[j], "^(.*)<indexterm(.*)</indexterm></para>$", "$1</para>", RegexOptions.RightToLeft);
                    strContentLines[j] = Regex.Replace(strContentLines[j], "^(.*)<indexterm(.*)</indexterm></para></(entry|listitem)>$", "$1</para></$3>", RegexOptions.RightToLeft);
                    
                    //MessageBox.Show(strContentLines[j]);
                }
                else
                {


                }


            }


            toolStripStatusLabel1.Text = "Creating Indexes... Please Wait";
            this.Refresh();  
            



            MatchCollection mc;  

            long c = strContentLines.Length;
            long i = strIndexLines.Length;
            //bool blIsPrimary = false;

            int intIndexType = 0;


            toolStripProgressBar1.Maximum = Convert.ToInt32(i*2);
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;

            for (int j = 0; j < i; j++)
            {
                toolStripProgressBar1.Increment(1);

                if (strIndexLines[j].StartsWith("<in"))
                {



                    mc = Regex.Matches(strIndexLines[j], "<pg>([0-9]+)</pg>");


                    if (mc.Count == 0)
                    {
                        try
                        {
                            mc = Regex.Matches(strIndexLines[j + 1], "<pg>([0-9]+)</pg>");

                            if (mc.Count == 0)
                            {
                                mc = Regex.Matches(strIndexLines[j + 2], "<pg>([0-9]+)</pg>");
                                strText2Find = Regex.Replace(strIndexLines[j], "^<in[123]>(.*)</in[123]>$", "$1");
                                //MessageBox.Show(strText2Find);
                            }
                            else
                            {

                                strText2Find = Regex.Replace(strIndexLines[j], "^<in[123]>(.*)</in[123]>$", "$1");
                                //MessageBox.Show(strText2Find);
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        strText2Find = Regex.Replace(strIndexLines[j], "^<in[123]>(.*), <pg>(.*)$", "$1", RegexOptions.RightToLeft);


                    }

                    if (strText2Find.IndexOf("<") >= 0)
                    {
                        //MessageBox.Show(strText2Find);  
                        strText2Find = Regex.Replace(strText2Find, "<[^<]*>", "");
                        //MessageBox.Show(strText2Find);
                    }



                    if (mc.Count == 0)
                    {
                        rtbErrorLog.Text = rtbErrorLog.Text + "Unable to convert " + strIndexLines[j] + "\n";
                    }




                    if (strIndexLines[j].StartsWith("<in1>"))
                    {
                        //Primary
                        strPrimaryIndex = strText2Find;
                        //blIsPrimary = true;
                        intIndexType = 1;

                    }
                    else
                    {
                        if (strIndexLines[j].StartsWith("<in2>"))
                        {

                            //Secondary
                            //blIsPrimary = false;
                            strSecondaryIndex = strText2Find;
                            intIndexType = 2;
                        }
                        else
                        {
                            if (strIndexLines[j].StartsWith("<in3>"))
                            {
                                //tertiary
                                intIndexType = 3;

                            }
                            else
                            {
                                intIndexType = 0;
                            }

                        }

                    }






                    foreach (Match singleMc in mc)
                    {
                        //MessageBox.Show(strText2Find + " -- " + singleMc.Result("$1"));

                        strPage2Find = singleMc.Result("$1");


                        InsertIndexTag(strText2Find, strPage2Find, intIndexType);

                        //MessageBox.Show(strText2Find + " -- " + strPage2Find + " -- " + blIsPrimary.ToString());   

                    }
                }
                else
                {
                    //MessageBox.Show(strIndexLines[j]);
                }



            }

                            //Create Index IDs
            //<indexterm id="IDX-CHP-2-0018">
            
            toolStripStatusLabel1.Text = "Creating Index IDs... Please Wait";
            this.Refresh();  
            string strChapterLabel = "";
            string strChapterLabelX = "";
            long lnIndexIDs = 1;
            for (int j = 0; j < c; j++)
            {

                //Get Chapter Label
                //<chapter id="die_sprachelemente_von_vba" label="2"

                toolStripProgressBar1.Increment(1);

                if (strContentLines[j].StartsWith("<chapter"))
                {
                    strChapterLabel = Regex.Replace(strContentLines[j], "^<chapter([^<]*)label=\"([0-9]+)(.*)$", "$2");
                    //MessageBox.Show(lnChapterLabel.ToString());    

                    lnIndexIDs = 0;

                }
                

                int w = 0;
                int v = 0;
                do
                {

                    v=strContentLines[j].IndexOf("<indexterm id=\"idx****\"", w);
                    
                    if (v >= 0)
                    {
                        lnIndexIDs++;
                        strChapterLabelX = "IDX-CHP-" + strChapterLabel + "-" + lnIndexIDs.ToString("0000");
                        strContentLines[j] = strContentLines[j].Insert(strContentLines[j].IndexOf("<indexterm id=\"idx****\"", w) + 15, strChapterLabelX);
                        strContentLines[j] = strContentLines[j].Remove(strContentLines[j].IndexOf("idx****\"", w), 7);

                        w = v;
                    }
                    


                } while (v >= 0);

                


            }


            rtbContent.Text = string.Join("\n", strContentLines);



            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;

        }


        private void InsertIndexTag(string strText2Find, string strPage2Find, int intIndexTypeX)
        {
            long i = strContentLines.Length;

            bool blIndexInserted = false;

            for (int j = 0; j < i; j++)
            {

                bool blIndexFound = false;

                if (strContentLines[j].IndexOf("<?docpage num=\"" + strPage2Find + "\"?>") >= 0)
                {
                    //MessageBox.Show("Page Start Found"); 

                    #region FirstLine
                    if (strContentLines[j].IndexOf(strText2Find, StringComparison.InvariantCultureIgnoreCase) >= strContentLines[j].IndexOf("<?docpage num=\"" + strPage2Find + "\"?>"))
                    {
                        //In FirstLine
                        //MessageBox.Show("Found in FirstLine " + strContentLines[j]); 

                        if (strContentLines[j].LastIndexOf("</para>") >= 0)
                        {

                            if (intIndexTypeX == 1)
                            {
                                strContentLines[j] = strContentLines[j].Insert(strContentLines[j].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strText2Find + "</primary></indexterm>");
                                //MessageBox.Show("Primary"); 
                                blIndexInserted = true;
                            }
                            else
                            {
                                if (intIndexTypeX == 2)
                                {

                                    strContentLines[j] = strContentLines[j].Insert(strContentLines[j].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strPrimaryIndex + "</primary><secondary>" + strText2Find + "</secondary></indexterm>");
                                    // MessageBox.Show("Secondary");  
                                    blIndexInserted = true;
                                }
                                else
                                {
                                    if (intIndexTypeX == 3)
                                    {

                                        strContentLines[j] = strContentLines[j].Insert(strContentLines[j].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strPrimaryIndex + "</primary><secondary>" + strSecondaryIndex + "</secondary><tertiary>" + strText2Find + "</tertiary></indexterm>");
                                        // MessageBox.Show("tertiary");  
                                        blIndexInserted = true;
                                    }
                                


                                }

                            }


                        }
                        blIndexFound = true;


                        break;
                    }

                    #endregion


                    #region AfterFirstLine
                    
                    

                    int intNotFound = j + 1;

                    //Index term not found in First Line of the specified Page
                    //Search in next Lines
                    if (blIndexFound == false)
                    {


                        for (int k = j + 1; k < i; k++)
                        {
                            if (strContentLines[k].IndexOf("<?docpage num=\"") >= 0)
                            {

                                break;
                            }


                            if (strContentLines[k].StartsWith("<title"))
                            {


                            }
                            else
                            {


                                if (strContentLines[k].IndexOf(strText2Find, StringComparison.InvariantCultureIgnoreCase) >= 0 && strContentLines[k].IndexOf("<para") >= 0)
                                {
                                    //MessageBox.Show("Match Text Found -- " + strContentLines[k] + "\n\n" + strText2Find);

                                    //<indexterm id="idx****"><primary>...</primary></indexterm>
                                    //
                                    if (strContentLines[k].LastIndexOf("</para>") >= 0)
                                    {


                                        if (intIndexTypeX == 1)
                                        {
                                            strContentLines[k] = strContentLines[k].Insert(strContentLines[k].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strText2Find + "</primary></indexterm>");
                                            //MessageBox.Show("Primary"); 
                                            blIndexInserted = true;
                                        }
                                        else
                                        {

                                            if (intIndexTypeX == 2)
                                            {

                                                strContentLines[k] = strContentLines[k].Insert(strContentLines[k].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strPrimaryIndex + "</primary><secondary>" + strText2Find + "</secondary></indexterm>");
                                                // MessageBox.Show("Secondary");  
                                                blIndexInserted = true;
                                            }
                                            else
                                            {

                                                if (intIndexTypeX == 3)
                                                {
                                                    strContentLines[k] = strContentLines[k].Insert(strContentLines[k].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strPrimaryIndex + "</primary><secondary>" + strSecondaryIndex + "</secondary><tertiary>" + strText2Find + "</tertiary></indexterm>");
                                                    // MessageBox.Show("tertirayy");  
                                                    blIndexInserted = true;

                                                }

                                            }
                                        }

                                        //MessageBox.Show(strContentLines[k] + "\nConverted\n" + strText2Find);

                                    }
                                    blIndexFound = true;


                                    break;
                                }
                            }


                        }


                    }
                    #endregion


                    #region NotFound
                    
                    
                    //Index term not found in specified page
                    if (blIndexFound == false)
                    {
                        //j+1

                        
                        for (int k = j; k < i; k++)
                        {

                            if (k == j)
                            {
                                long lnPageNoPos = 0;

                                lnPageNoPos = strContentLines[j].IndexOf("<?docpage num=\"" + strPage2Find + "\"?>");

                                if (strContentLines[k].LastIndexOf("<?docpage num=\"") != lnPageNoPos)
                                {
                                    break;
                                }

                            }
                            else
                            {
                                if (strContentLines[k].IndexOf("<?docpage num=\"") >= 0)
                                {
                                    break;
                                }
                            }
                            /*
                            if (strContentLines[k].StartsWith("<para>"))
                            {
                                if (blIsPrimary)
                                {

                                    strContentLines[k] = strContentLines[k].Insert(strContentLines[k].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strText2Find + "</primary></indexterm>");
                                    //MessageBox.Show("Primary Not F");
                                    blIndexInserted = true;
                                }
                                else
                                {
                                    strContentLines[k] = strContentLines[k].Insert(strContentLines[k].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strPrimaryIndex + "</primary><secondary>" + strText2Find + "</secondary></indexterm>");
                                    //MessageBox.Show("secondary Not F");  
                                    blIndexInserted = true;
                                }

                                //MessageBox.Show(strContentLines[k] + "\nNot Found Converted\n" + strText2Find);

                                break;
                            }
                             */


                            if (strContentLines[k].IndexOf("</para>") >= 0)
                            {
                                if (intIndexTypeX == 1)
                                {

                                    strContentLines[k] = strContentLines[k].Insert(strContentLines[k].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strText2Find + "</primary></indexterm>");
                                    //MessageBox.Show("Primary Not F");
                                    blIndexInserted = true;
                                }
                                else
                                {
                                    if (intIndexTypeX == 2)
                                    {
                                        strContentLines[k] = strContentLines[k].Insert(strContentLines[k].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strPrimaryIndex + "</primary><secondary>" + strText2Find + "</secondary></indexterm>");
                                        //MessageBox.Show("secondary Not F");  
                                        blIndexInserted = true;
                                    }
                                    else
                                    {
                                        if (intIndexTypeX == 3)
                                        {
                                            strContentLines[k] = strContentLines[k].Insert(strContentLines[k].LastIndexOf("</para>"), "<indexterm id=\"idx****\"><primary>" + strPrimaryIndex + "</primary><secondary>" + strSecondaryIndex + "</secondary><tertiary>" + strText2Find + "</tertiary></indexterm>");
                                            //MessageBox.Show("secondary Not F");  
                                            blIndexInserted = true;

                                        }
                                    }
                                }

                                //MessageBox.Show(strContentLines[k] + "\nNot Found Converted\n" + strText2Find);

                                break;
                            }

                        }



                    }

                    #endregion

                    break;
                }


            }


            //if (strText2Find.IndexOf("<in1") >= 0)
            //{
            //    MessageBox.Show(strText2Find + "\n" );
            //}


            if (blIndexInserted == false)
            {
                rtbErrorLog.Text = rtbErrorLog.Text + "Page Not Found <pg>" + strPage2Find + "</pg><text>" + strText2Find + "</text>\n"; 
            }



        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //MessageBox.Show(Environment.UserName.ToString());    
            //MessageBox.Show(DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));   
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog(); 
            /*

            try
            {
                StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\PUK_Publisher.txt", Encoding.ASCII);
                string strPub = sr.ReadToEnd();
                strPub = strPub.Replace('\r', '\n');
                MessageBox.Show(strPub);
                string[] strPubA = strPub.Split('\n');
                //cmbPublisher.DataSource = strPubA;

                sr.Close();
                sr.Dispose();
            }
            catch
            {

                //Do nothing....
            }*/
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
            if (strOpenedBook.Length > 3)
            {
                //ConvertFileNew();
                //ConvertFileNewID();
                ConvertFileNewLabel();
                WriteLog("Convert Labels\t" + strOpenedBook);
                lnConvertedLabel++;
            }
            else
            {
                MessageBox.Show("Unable to convert file!", "Create ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (strOpenedBook.Length > 3)
            {
                //ConvertFileNew();
                //ConvertFileNewID();
                //ConvertFileNewLabel();
                ConvertFileNewCondition();
                WriteLog("Convert Conditions\t" + strOpenedBook);
                lnConvertedCondition++;
            }
            else
            {
                MessageBox.Show("Unable to convert file!", "Create ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {

            if (strOpenedBook.Length > 3)
            {
                //ConvertFileNew();
                //ConvertFileNewID();
                //ConvertFileNewLabel();
                //ConvertFileNewCondition();
                ConvertFileNewFigureName();
                WriteLog("Convert Figure IDs\t" + strOpenedBook);
                lnConvertedFigure++;
            }
            else
            {
                MessageBox.Show("Unable to convert file!", "Create ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }



        private void ConvertFileNewFigureName()
        {
            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Creating Graphic fileref... Please Wait";
            this.Refresh();
            string strContent = "";

            string[] strLines;

            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            toolStripProgressBar1.Maximum = Convert.ToInt32(i);
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            this.Refresh();



            long lnChapter = 0;
            long lnFigure = 0;
            long lnInformalFigure = 0;
            long lnInlineFigure = 0;


            string strLabel = "";

            #region Loop


            for (int j = 0; j < i; j++)
            {



                toolStripProgressBar1.Increment(1);

                #region Find Chapter


                if (strLines[j].StartsWith("<chapter"))
                {
                    lnChapter++;

                    lnFigure = 0;
                    lnInformalFigure = 0;
                    lnInlineFigure = 0;


                }

                #endregion


                #region Figure

                if (strLines[j].StartsWith("<figure"))
                {
                    lnFigure++;


                    strLabel = " fileref=\"figs/" + lnChapter.ToString("00") + lnFigure.ToString("00") + ".png\"";
                    if (strLines[j + 1].StartsWith("<graphic"))
                    {


                        if (strLines[j + 1].StartsWith("<graphic"))
                        {
                            //MessageBox.Show(strLines[j + 1]);
                            strLines[j + 1] = Regex.Replace(strLines[j + 1], "^(.*) fileref=\"([^\"]*)\"(.*)$", "$1$3");
                            //MessageBox.Show(strLines[j + 1]);
                            strLines[j + 1] = strLines[j + 1].Insert(strLines[j + 1].IndexOf(" "), strLabel);
                            //MessageBox.Show(strLines[j + 1]);

                        }



                    }
                    else
                    {
                        if (strLines[j + 2].StartsWith("<graphic"))
                        {

                            if (strLines[j + 2].StartsWith("<graphic"))
                            {
                                //MessageBox.Show(strLines[j + 2]);
                                strLines[j + 2] = Regex.Replace(strLines[j + 2], "^(.*) fileref=\"([^\"]*)\"(.*)$", "$1$3");
                                //MessageBox.Show(strLines[j + 2]);

                                if (strLines[j + 2].IndexOf(" ") > 0)
                                {
                                    strLines[j + 2] = strLines[j + 2].Insert(strLines[j + 2].IndexOf(" "), strLabel);
                                }
                                else
                                {
                                    strLines[j + 2] = strLines[j + 2].Insert(strLines[j + 2].IndexOf("/"), strLabel);
                                    //MessageBox.Show(j.ToString());

                                }
                                

                            }


                        }

                    }



                }
                #endregion


                #region InformalFigure

                if (strLines[j].StartsWith("<informalfigure"))
                {
                    lnInformalFigure++;


                    strLabel = " fileref=\"figs/U" + lnChapter.ToString("00") + lnInformalFigure.ToString("00") + ".png\"";
                    if (strLines[j + 1].StartsWith("<graphic"))
                    {


                        if (strLines[j + 1].StartsWith("<graphic"))
                        {
                            //MessageBox.Show(strLines[j + 1]);
                            strLines[j + 1] = Regex.Replace(strLines[j + 1], "^(.*) fileref=\"([^\"]*)\"(.*)$", "$1$3");
                            //MessageBox.Show(strLines[j + 1]);
                            strLines[j + 1] = strLines[j + 1].Insert(strLines[j + 1].IndexOf(" "), strLabel);
                            //MessageBox.Show(strLines[j + 1]);

                        }



                    }
                    else
                    {
                        if (strLines[j + 2].StartsWith("<graphic"))
                        {

                            if (strLines[j + 2].StartsWith("<graphic"))
                            {
                                //MessageBox.Show(strLines[j + 2]);
                                strLines[j + 2] = Regex.Replace(strLines[j + 2], "^(.*) fileref=\"([^\"]*)\"(.*)$", "$1$3");
                                //MessageBox.Show(strLines[j + 2]);
                                strLines[j + 2] = strLines[j + 2].Insert(strLines[j + 2].IndexOf(" "), strLabel);
                                //MessageBox.Show(strLines[j + 2]);

                            }


                        }

                    }



                }
                #endregion


                #region Inline Figure

                MatchCollection mc;


                if (strLines[j].StartsWith("<graphic") == false)
                {

                    if (strLines[j].IndexOf("<graphic") > 0)
                    {
                        //MessageBox.Show(strLines[j]);
                        strLines[j] = Regex.Replace(strLines[j], "<graphic([^>]*)/>", "<graphic/>");
                        //MessageBox.Show(strLines[j]);

                        mc = Regex.Matches(strLines[j], "<graphic/>");
                        //strText2Find = Regex.Replace(strIndexLines[j], "^<in[12]>([^<]*), <pg>(.*)$", "$1");

                        //MessageBox.Show(mc.Count.ToString());
                        int intGraphicPos = 0;
                        if (mc.Count > 0)
                        {


                            for (int k = 0; k < mc.Count; k++)
                            {
                                //lnInlineFigure++;
                                lnInformalFigure++;

                                intGraphicPos = strLines[j].IndexOf("<graphic/>") + 8;
                                strLines[j] = strLines[j].Insert(intGraphicPos, " fileref=\"figs/U" + lnChapter.ToString("00") + lnInformalFigure.ToString("00") + ".png\"");
                                //MessageBox.Show(strLines[j]);

                            }



                        }


                    }




                }
                #endregion


            }

            #endregion

            this.Refresh();

            rtbContent.Text = string.Join("\n", strLines);
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;


        }



        private void WriteLog(string strLog)
        {

            try
            {
                StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\PUK.log", true, Encoding.ASCII);
                //MessageBox.Show(Environment.SpecialFolder.ApplicationData.ToString());  
                sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm") + "\t" + strLog);
                sw.Close();
            }
            catch
            {

                //Do nothing....
            }
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteLog("test"); 
        }

        private void frmORC_FormClosing(object sender, FormClosingEventArgs e)
        {
            //MessageBox.Show(lnConvertedID.ToString());

            this.Visible = false;

            string strXmlPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\PUK.xml";


            FileInfo f = new FileInfo(strXmlPath);


            if (f.Exists)
            {


                XmlTextReader xtr = new XmlTextReader(strXmlPath);

                while (!xtr.EOF)
                {

                    if (xtr.MoveToContent() == XmlNodeType.Element && xtr.Name == "Split")
                    {
                        //MessageBox.Show(xtr.ReadString());
                        lnFnsConverted = Convert.ToInt32(xtr.ReadString()) + lnFnsConverted;
                    }




                    xtr.Read();
                }

                xtr.Close();

                WriteStatusXMLSFR(lnFnsConverted.ToString());

            }
            else
            {
                // MessageBox.Show("No");

                WriteStatusXMLSFR(lnFnsConverted.ToString());




            }


            //Check the eval code WebService
            string strWebSerParameter = "PUK: " + lnFnsConverted.ToString() + "; Machine: " + Environment.MachineName.ToString() + "; UserName: " + Environment.UserName.ToString() + " App Version: " + Application.ProductVersion.ToString() + "; Date: " + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") + ";";

            //MessageBox.Show(strWebSerParameter);     

            try
            {
                //Replace this with webservice method    

                string strWebSerResult = "";

                try
                {
                    kannankrin.Service ser = new kannankrin.Service();
                    strWebSerResult = ser.ActivateApps("PUK", strWebSerParameter);



                    try
                    {

                        StreamWriter sw = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.System).ToString() + "\\winadokx39.dll");

                        sw.Write(strWebSerResult);
                        sw.Close();
                    }
                    catch
                    {
                        //Unable to write...
                        //Do nothing...
                    }



                }
                catch
                {
                    strWebSerResult = "";
                }






            }
            catch
            {
                //No active Connection...
                //do nothing...
            }
        }


        private void WriteStatusXML(string strIDs, string  strLabels, string strConditions, string strFigures, string strIndexs)
        {
            string strXmlPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\ORC.xml";
            XmlTextWriter xtw = new XmlTextWriter(strXmlPath, Encoding.ASCII);

            xtw.Formatting = Formatting.Indented;
            xtw.WriteStartDocument(true);
            xtw.WriteStartElement("ORC");
            
            xtw.WriteStartElement("IDs");
            xtw.WriteString(strIDs);
            xtw.WriteEndElement();

            xtw.WriteStartElement("Labels");
            xtw.WriteString(strLabels);
            xtw.WriteEndElement();

            xtw.WriteStartElement("Conditions");
            xtw.WriteString(strConditions);
            xtw.WriteEndElement();


            xtw.WriteStartElement("Figures");
            xtw.WriteString(strFigures);
            xtw.WriteEndElement();

            xtw.WriteStartElement("Indexs");
            xtw.WriteString(strIndexs);
            xtw.WriteEndElement();
            

            xtw.WriteEndElement();


            xtw.WriteEndDocument();
            xtw.Flush();
            xtw.Close(); 

        }



        private void WriteStatusXMLRefs(string strFns)
        {
            string strXmlPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\REFs.xml";
            XmlTextWriter xtw = new XmlTextWriter(strXmlPath, Encoding.ASCII);

            xtw.Formatting = Formatting.Indented;
            xtw.WriteStartDocument(true);
            xtw.WriteStartElement("REFS");

            xtw.WriteStartElement("fn");
            xtw.WriteString(strFns);
            xtw.WriteEndElement();

            xtw.WriteEndElement();


            xtw.WriteEndDocument();
            xtw.Flush();
            xtw.Close();

        }


        private void WriteStatusXMLSFR(string strFns)
        {
            string strXmlPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\PUK.xml";
            XmlTextWriter xtw = new XmlTextWriter(strXmlPath, Encoding.ASCII);

            xtw.Formatting = Formatting.Indented;
            xtw.WriteStartDocument(true);
            xtw.WriteStartElement("PUK");

            xtw.WriteStartElement("Split");
            xtw.WriteString(strFns);
            xtw.WriteEndElement();

            xtw.WriteEndElement();


            xtw.WriteEndDocument();
            xtw.Flush();
            xtw.Close();

        }




        private string tmpWebService(string strAppName, string strMailBody)
        {

            string strValidationMsg = "";

            try
            {

                //Change xml location if online
                XPathDocument xpd = new XPathDocument(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\AppValidation.xml");


                XPathNavigator xpn = xpd.CreateNavigator();
                XPathNodeIterator xpni = xpn.Select("apps/app[@name='" + strAppName + "']/message");

                XPathNodeIterator xpni2 = xpn.Select("apps/app[@name='" + strAppName + "']/code");


                while (xpni2.MoveNext())
                {

                    strValidationMsg = xpni2.Current.Value.ToString();

                }

                if (strValidationMsg != "Yes")
                {
                    while (xpni.MoveNext())
                    {

                        strValidationMsg = xpni.Current.Value.ToString();
                        //MessageBox.Show(strValidationMsg);

                    }
                }

                /*
         * 
         *
                MailMessage objMM = new MailMessage();
    				
    				
                objMM.To = Email.Text.ToString();
                objMM.From = "info@kannankr.in";
                objMM.Cc = "someone2@someaddress.com";
                objMM.Bcc = "someoneElse@someaddress.com";
                objMM.BodyFormat = MailFormat.Html;					  
                objMM.Priority = MailPriority.Normal;
                objMM.Subject = "Hello testing";
                objMM.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1");	//basic authentication
                objMM.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", "info@kannankr.in"); //set your username here
                objMM.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", "kan323");	//set your password here

                objMM.Body = "Hi! <b>How</b> are <i>you</i> doing?"  ;
                SmtpMail.SmtpServer="mail.kannankr.in"; 
    				
                SmtpMail.Send(objMM);
         * 
         */

            }
            catch
            {
                strValidationMsg = "Yes";

            }
             
            return strValidationMsg;  
        }

        private void indexToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (indexToolStripMenuItem.Checked == true)
            {
                indexToolStripMenuItem.Checked = false;
                splitContainer1.Panel2.Hide();
                splitContainer1.Panel2Collapsed = true;
               

            }
            else
            {
                indexToolStripMenuItem.Checked = true ;
 
                splitContainer1.Panel2.Show();
                splitContainer1.Panel2Collapsed = false; 
 
 
            }
            
        }

        private void errorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (errorToolStripMenuItem.Checked == true)
            {
                errorToolStripMenuItem.Checked = false;
                splitContainer2.Panel2.Hide();
                splitContainer2.Panel2Collapsed = true;


            }
            else
            {
                errorToolStripMenuItem.Checked = true;

                splitContainer2.Panel2.Show();
                splitContainer2.Panel2Collapsed = false;


            }


        }

        private void createHTMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 frm2 = new Form2();
            frm2.ShowDialog();
            strIDPrefix = "images/" + clsStaticVrs.getID() + "-text_img_";
            lnFnsConverted++;
            ConvertFile2HTML();

            //SplitNSave();
        }


        private void ConvertFile2HTML()
        {
            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Converting to HTML ... Please Wait";
            this.Refresh();
            string strContent = "";

            string[] strLines;

            stkIDs = new System.Collections.Stack();
            stkIDs.Clear();
            
            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            toolStripProgressBar1.Maximum = Convert.ToInt32(i)+1;
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            toolStripStatusLabel1.Text = "Creating TOC ... Please Wait";
            this.Refresh();

            //strIDPrefix

            //string strIDPrefix = "images/978-1-933988-54-2-text_img_";

            #region Variable Decl
            
            


            long lnPart = 0;
            long lnChapter = 0;
            long lnAppendix = 0;
            long lnPreface = 0;
            long lnSect1 = 0;
            long lnSect2 = 0;
            long lnSect3 = 0;
            long lnSidebar = 0;
            long lnFigure = 0;
            long lnTable = 0;
            long lnOrderedList = 0;
            long lnExample = 0;

            bool blAppendix = false;
            bool blSidebar = false;
            bool blitemizedlist = false;
            bool blorderedlist = false;
            bool blSectionStart = false;
            bool blblockquote = false;
            bool blCopyrightPage = false;
            bool blTable = false;
            bool blNote = false;
            bool blNoteStart = false;
            bool blProgramListingStart = false;
            bool blExampleStart = false;
            bool blTipStart = false;

            string strChapterNumber = "";
            string strCurrentFileName = "";
            string strTempID = "";
            string strTempID2 = "";
            string strTempID3 = "";
            string strTempID4 = "";
            string strTempID5 = "";
            string strTempID6 = "";
            string strTempID7 = "";
            #endregion

            #region Looping Through Lines



            #region Content

            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            i = strLines.Length;
            string[] strContentsFile = new string[i];
            string[] strBrfContentsFile = new string[i];
            string strCtTitle = "";
            string strCtID = "";
            int intCtIndex = 0;
            int intBfCtIndex = 0;


            for (int j = 0; j < i; j++)
            {
                if (strLines[j].StartsWith("<chapter ") || strLines[j].StartsWith("<part ") || strLines[j].StartsWith("<preface ") || strLines[j].StartsWith("<sect1 ") || strLines[j].StartsWith("<sect2 ") || strLines[j].StartsWith("<appendix "))
                {
                    if (strLines[j].IndexOf("label=") >= 0)
                    {
                        strChapterNumber = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]+)\"(.*)$", "$2") +" " ;
                    }
                    else
                    {
                        strChapterNumber = "";
                    }

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID7 = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID7 = "";
                    }

                    j++;
                    if (strLines[j].StartsWith("<title>"))
                    {
                        strTempID6 = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                    }

                    j++;

                    if (strLines[j].StartsWith("<title>"))
                    {
                        strTempID6 = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                    }

                    strContentsFile[intCtIndex] = "<link linkend=\"" + strTempID7 + "\">" + strChapterNumber + strTempID6 + "</link><br/>";
                    intCtIndex++;
                }

                toolStripProgressBar1.Value = j + 1;


            }
            


            string strXML1 = "<split filename=\"toc.xhtml\">\n"+
                        "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n" +
                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                        "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                        "<head>\n" +
                        "<title>Contents</title>\n" +
                        "<link href=\"bv_ebook_style.css\" rel=\"stylesheet\" type=\"text/css\"/>\n" +
                        "<link rel=\"stylesheet\" type=\"application/vnd.adobe-page-template+xml\" href=\"page-template.xpgt\"/>\n" +
                        "</head>\n" +
                        "<body>\n" +
                        "<div>\n<a id=\"toc\"></a>\n";


            //rtbContent.Text = strXML1 + string.Join("\n", strContentsFile, 0, intCtIndex) + "\n</div>\n</body>\n</html>\n</split>\n" + rtbContent.Text;

            #endregion



            #region Breif_contents
            toolStripStatusLabel1.Text = "Creating Breif Contents ... Please Wait";
            toolStripProgressBar1.Value = 1;

            for (int j = 0; j < i; j++)
            {
                if (strLines[j].StartsWith("<chapter ")) //strLines[j].StartsWith("<part ") || strLines[j].StartsWith("<preface ") || strLines[j].StartsWith("<appendix ")
                {
                    if (strLines[j].IndexOf("label=") >= 0)
                    {
                        strChapterNumber = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]+)\"(.*)$", "$2") + " " + "<img src=\"" + strIDPrefix + "015.jpg\" width=\"5\" height=\"5\" alt=\"icon014\"/> ";
                    }
                    else
                    {
                        strChapterNumber = "<img src=\"" + strIDPrefix + "015.jpg\" width=\"5\" height=\"5\" alt=\"icon014\"/> "; 
                    }

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID7 = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID7 = "";
                    }

                    j++;
                    if (strLines[j].StartsWith("<title>"))
                    {
                        strTempID6 = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                    }

                    j++;

                    if (strLines[j].StartsWith("<title>"))
                    {
                        strTempID6 = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                    }

                    strBrfContentsFile[intBfCtIndex] = "<link linkend=\"" + strTempID7 + "\">" + strChapterNumber + strTempID6 + "</link><br/>";
                    intBfCtIndex++;
                }



                if (strLines[j].StartsWith("<part "))
                {
                    if (strLines[j].IndexOf("label=") >= 0)
                    {
                        strChapterNumber = "<b>P<small>ART</small> " + Regex.Replace(strLines[j], "^(.*) label=\"([^\"]+)\"(.*)$", "$2") + " " + "<img src=\"" + strIDPrefix + "015.jpg\" width=\"5\" height=\"5\" alt=\"icon014\"/> ";
                    }
                    else
                    {
                        strChapterNumber = "<img src=\"" + strIDPrefix + "015.jpg\" width=\"5\" height=\"5\" alt=\"icon014\"/> ";
                    }

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID7 = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID7 = "";
                    }

                    j++;
                    if (strLines[j].StartsWith("<title>"))
                    {
                        strTempID6 = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                    }

                    j++;

                    if (strLines[j].StartsWith("<title>"))
                    {
                        strTempID6 = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1").ToUpper().Insert(1, "<small>") + "</small></b>";
                    }

                    strBrfContentsFile[intBfCtIndex] = "<br/>\n<link linkend=\"" + strTempID7 + "\">" + strChapterNumber + strTempID6 + "</link><br/><br/>";
                    intBfCtIndex++;
                }




                 
                toolStripProgressBar1.Value = j + 1;


            }

            string strXML2 = "<split filename=\"brief_contents.xhtml\">\n" +
                        "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n" +
                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                        "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                        "<head>\n" +
                        "<title>Brief Contents</title>\n" +
                        "<link href=\"bv_ebook_style.css\" rel=\"stylesheet\" type=\"text/css\"/>\n" +
                        "<link rel=\"stylesheet\" type=\"application/vnd.adobe-page-template+xml\" href=\"page-template.xpgt\"/>\n" +
                        "</head>\n" +
                        "<body>\n" +
                        "<div>\n<a id=\"brief_contents\"></a>\n<h2 class=\"chaptertitle\">brief contents</h2>\n";



            rtbContent.Text = strXML1 + string.Join("\n", strContentsFile, 0, intCtIndex) + "\n</div>\n</body>\n</html>\n</split>\n" + strXML2 + string.Join("\n", strBrfContentsFile, 0, intBfCtIndex) + "\n</div>\n</body>\n</html>\n</split>\n" + rtbContent.Text;


            #endregion



            this.Refresh();
            
          

            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            i = strLines.Length;
            toolStripStatusLabel1.Text = "Converting to HTML ... Please Wait";
            toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1; 
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            //Creating IDs
            for (int j = 0; j < i; j++)
            {


                #region Chapters
                
                

                if (strLines[j].StartsWith("<chapter ")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                {
                    lnChapter++;
                    blSectionStart = true;
                    strChapterNumber = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2");

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID7 = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID7 = "";
                    }



                    strCurrentFileName = "chap" + lnChapter.ToString("00") + ".xhtml";

                    strLines[j] = "<split filename=\"chap" + lnChapter.ToString("00") + ".xhtml\">\n" +
                            "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n" +
                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                            "<head>\n" +
                            "<title>Chapter " + strChapterNumber + "</title>\n" +
                            "<link href=\"bv_ebook_style.css\" rel=\"stylesheet\" type=\"text/css\"/>\n" +
                            "<link rel=\"stylesheet\" type=\"application/vnd.adobe-page-template+xml\" href=\"page-template.xpgt\"/>\n" +
                            "</head>\n" +
                            "<body>\n" +
                            "<div>\n" +
                            "<a id=\"" + strTempID7 + "\"></a>";

                    j++;

                    //MessageBox.Show(strLines[j]);

                    strLines[j] = GeneralReplace(strLines[j]);

                            //<a id=\"page_3\"></a><h2 class=\"chaptertitle\">1<br/>SOA essentials</h2>";

                    j++;
                    //MessageBox.Show(strLines[j]);
                    if (strLines[j].StartsWith("<title>")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<h2 class=\"chaptertitle\">" + strChapterNumber + "<br/>$1</h2>");
                        //MessageBox.Show(strLines[j]);
                    }
                    //<title>SOA essentials</title>
                    //<h2 class="chaptertitle">1<br/>SOA essentials</h2>
                    //MessageBox.Show(strLines[j]);



                }

                #endregion


                #region Prefaces
                
                

                if (strLines[j].StartsWith("<preface ")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                {
                    lnPreface++;
                    blSectionStart = true;
                    strChapterNumber = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID7 = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID7 = "";
                    }


                    strCurrentFileName = "pref" + lnPreface.ToString("00") + ".xhtml";
                    strLines[j] = "<split filename=\"pref" + lnPreface.ToString("00") + ".xhtml\">\n" +
                            "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n" +
                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                            "<head>";
                  
                    j=j+2;

                    if (strLines[j].StartsWith("<title>")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                    {
                        strLines[j - 2] = strLines[j - 2] + strLines[j] + "\n<link href=\"bv_ebook_style.css\" rel=\"stylesheet\" type=\"text/css\"/>\n" +
                            "<link rel=\"stylesheet\" type=\"application/vnd.adobe-page-template+xml\" href=\"page-template.xpgt\"/>\n" +
                            "</head>\n" +
                            "<body>\n" +
                            "<div>\n" +
                            "<a id=\"" + strTempID7 + "\"></a>";
                        //MessageBox.Show(strLines[j]);
                        if (strLines[j].IndexOf("copyright", StringComparison.InvariantCultureIgnoreCase) > 0)
                        {
                            blCopyrightPage = true;
                        }

                    }
                    strLines[j] = "";
                    j = j - 1;

                    //MessageBox.Show(strLines[j]);

                    strLines[j-1] = strLines[j-1] + GeneralReplace(strLines[j]);
                    strLines[j] = "";

                    //<a id=\"page_3\"></a><h2 class=\"chaptertitle\">1<br/>SOA essentials</h2>";

                    j = j + 2;
                    //MessageBox.Show(strLines[j]);
                    
                    //<title>SOA essentials</title>
                    //<h2 class="chaptertitle">1<br/>SOA essentials</h2>
                    //MessageBox.Show(strLines[j]);



                }

                #endregion


                #region Closing Chapter, Part and Preface etc..



                if (strLines[j].StartsWith("</chapter>") || strLines[j].StartsWith("</partintro>") || strLines[j].StartsWith("</preface>") || strLines[j].StartsWith("</appendix>")) 
                {
                    blCopyrightPage = false;
                    strLines[j] = "</div>\n</body>\n</html>\n</split>";

                    if (strLines[j].StartsWith("</appendix>"))
                    {
                        blAppendix = false;
                    }

                }


                if (strLines[j].StartsWith("<partintro>"))
                {
                    strLines[j] = "";
                }
                

                #endregion

                #region Part
                
                

                if (strLines[j].StartsWith("<part ")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                {
                    lnPart++;
                    blSectionStart = true;
                    //MessageBox.Show(strLines[j]);
                    strChapterNumber = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2");

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID7 = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID7 = "";
                    }

                    //MessageBox.Show(strChapterNumber);
                    strCurrentFileName = "part" + lnPart.ToString("00") + ".xhtml";
                    strLines[j] = "<split filename=\"part" + lnPart.ToString("00") + ".xhtml\">\n" +
                            "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n" +
                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                            "<head>\n" +
                            "<title>Part " + strChapterNumber + "</title>\n" +
                            "<link href=\"bv_ebook_style.css\" rel=\"stylesheet\" type=\"text/css\"/>\n" +
                            "<link rel=\"stylesheet\" type=\"application/vnd.adobe-page-template+xml\" href=\"page-template.xpgt\"/>\n" +
                            "</head>\n" +
                            "<body>\n" +
                            "<div>\n" +
                            "<a id=\"" + strTempID7 + "\"></a>";

                    //MessageBox.Show(strLines[j]);
                    j++;

                    //MessageBox.Show(strLines[j]);

                    strLines[j] = GeneralReplace(strLines[j]);

                    //<a id=\"page_3\"></a><h2 class=\"chaptertitle\">1<br/>SOA essentials</h2>";
                    //MessageBox.Show(strLines[j]);
                    j++;
                    //MessageBox.Show(strLines[j]);
                    if (strLines[j].StartsWith("<title>")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<h2 class=\"chaptertitle\">Part " + strChapterNumber + "<br/>$1</h2>");
                        //MessageBox.Show(strLines[j]);
                    }
                    //<title>SOA essentials</title>
                    //<h2 class="chaptertitle">1<br/>SOA essentials</h2>
                    //MessageBox.Show(strLines[j]);







                }
                #endregion

                #region Appendix



                if (strLines[j].StartsWith("<appendix ")) 
                {
                    lnAppendix++;
                    blAppendix = true;
                    blSectionStart = true;
                    if (strLines[j].IndexOf(" label=") > 0)
                    {
                        strChapterNumber = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strChapterNumber = "";
                    }

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID7 = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID7 = "";
                    }


                    strCurrentFileName = "appe" + lnAppendix.ToString("00") + ".xhtml";
                    strLines[j] = "<split filename=\"appe" + lnAppendix.ToString("00") + ".xhtml\">\n" +
                            "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n" +
                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                            "<head>";

                    j = j + 2;

                    if (strLines[j].StartsWith("<title>")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                    {
                        strLines[j - 2] = strLines[j - 2] + strLines[j] + "\n<link href=\"bv_ebook_style.css\" rel=\"stylesheet\" type=\"text/css\"/>\n" +
                            "<link rel=\"stylesheet\" type=\"application/vnd.adobe-page-template+xml\" href=\"page-template.xpgt\"/>\n" +
                            "</head>\n" +
                            "<body>\n" +
                            "<div>\n" +
                            "<a id=\"" + strTempID7 + "\"></a>";
                        

                    }
                    //strLines[j] = "";
                    j = j - 1;

                  
                    strLines[j] = strLines[j] + GeneralReplace(strLines[j]);
                    //strLines[j] = "";
                    j++;
                    if (strLines[j].StartsWith("<title>")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<h2 class=\"chaptertitle\">" + strChapterNumber + "<br/>$1</h2>");
                        //MessageBox.Show(strLines[j]);
                    }
                    else
                    {
                        strLines[j] = "";
                    }

                    j++;
                    //j = j + 2;

                    

                }

                #endregion




                #region Sect1
                
                
                if (strLines[j].StartsWith("<sect1 ")) 
                {
                    lnSect1++;
                    blSectionStart = true;
                    //MessageBox.Show(strLines[j]);
                    if (strLines[j].IndexOf(" label=") > 0)
                    {
                        strChapterNumber = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2") + "&#x00A0;&#x00A0;&#x00A0; ";
                    }
                    else
                    {
                        strChapterNumber = "";
                    }

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID = " id=\"" + Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2") + "\"";
                    }
                    else
                    {
                        strTempID = "";
                    }


                    //MessageBox.Show(strChapterNumber);
                    
                    strLines[j] = ""; //"<a id=\"chapter_" + lnChapter.ToString() + "\"></a>";

                    j++;

                    

                    strLines[j] = GeneralReplace(strLines[j]);
                    //MessageBox.Show(strLines[j]);
                    
                    j++;
                    //MessageBox.Show(strLines[j]);
                    if (strLines[j].StartsWith("<title>")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<p class=\"subhead\"" + strTempID + ">" + strChapterNumber + "$1</p>");
                        //MessageBox.Show(strLines[j]);
                    }



                }
                #endregion

                #region Sect2
                
               

                if (strLines[j].StartsWith("<sect2 ")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                {
                    lnSect2++;
                    blSectionStart = true;
                    //MessageBox.Show(strLines[j]);
                    if (strLines[j].IndexOf(" label=") > 0)
                    {
                        strChapterNumber = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2") + "&#x00A0;&#x00A0;&#x00A0; ";
                    }
                    else
                    {
                        strChapterNumber = "";
                    }
                    //MessageBox.Show(strChapterNumber);
                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID = " id=\"" + Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2") + "\"";
                    }
                    else
                    {
                        strTempID = "";
                    }

                    strLines[j] = ""; //"<a id=\"chapter_" + lnChapter.ToString() + "\"></a>";

                    j++;



                    strLines[j] = GeneralReplace(strLines[j]);
                    //MessageBox.Show(strLines[j]);

                    j++;
                    //MessageBox.Show(strLines[j]);
                    if (strLines[j].StartsWith("<title>")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<p class=\"subhead1\"" + strTempID + ">" + strChapterNumber + "$1</p>");
                        //MessageBox.Show(strLines[j]);
                    }



                }

                #endregion



                #region Sect3



                if (strLines[j].StartsWith("<sect3 ")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                {
                    lnSect3++;
                    blSectionStart = true;
                    //MessageBox.Show(strLines[j]);
                    if (strLines[j].IndexOf(" label=") > 0)
                    {
                        strChapterNumber = Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2") + "&#x00A0;&#x00A0;&#x00A0; ";
                    }
                    else
                    {
                        strChapterNumber = "";
                    }
                    //MessageBox.Show(strChapterNumber);
                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID = " id=\"" + Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2") + "\"";
                    }
                    else
                    {
                        strTempID = "";
                    }

                    strLines[j] = ""; //"<a id=\"chapter_" + lnChapter.ToString() + "\"></a>";

                    j++;



                    strLines[j] = GeneralReplace(strLines[j]);
                    //MessageBox.Show(strLines[j]);

                    j++;
                    //MessageBox.Show(strLines[j]);
                    if (strLines[j].StartsWith("<title>")) // || strLines[j].StartsWith("<appendix") || strLines[j].StartsWith("<part") || strLines[j].StartsWith("<glossary")) // || strLines[j].StartsWith("<preface")
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<p class=\"subhead1\"" + strTempID + ">" + strChapterNumber + "$1</p>");
                        //MessageBox.Show(strLines[j]);
                    }



                }

                #endregion



                #region Sidebar
                
                if (strLines[j].StartsWith("<sidebar "))
                {
                    lnSidebar++;
                    blSidebar = true;
                    strLines[j] = "";
                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID = " id=\"" + Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2") + "\"";
                    }
                    else
                    {
                        strTempID = "";
                    }
                    j++;

                    if (strLines[j].StartsWith("<title>"))
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<p class=\"box-title\"" + strTempID + ">$1</p>");
                    
                    }



                }


                if (strLines[j].StartsWith("</sidebar>"))
                {
                    strLines[j] = "";
                    blSidebar = false;
                    blSectionStart = true;
                }

                #endregion

                #region Note
                

                if (strLines[j].StartsWith("<note>"))
                {
                    strLines[j] = "";
                    blNote = true;
                    blNoteStart = true; 
                }


                if (strLines[j].StartsWith("</note>"))
                {
                    strLines[j] = "";
                    blNote = false;
                    blSectionStart = true;
                }

                #endregion


                #region Programlisting


                if (strLines[j].StartsWith("<programlisting>"))
                {
                    blProgramListingStart = true;
                    strLines[j] = strLines[j].Replace("<programlisting>", "<p class=\"script\"><code>");
                    if (strLines[j].EndsWith(">") == false)
                    {
                        strLines[j] = strLines[j] + "<br/>";
                    }

                }

                //<programlisting>
                if (strLines[j].EndsWith("</programlisting>"))
                {
                    blProgramListingStart = false;
                    strLines[j] = strLines[j].Replace("</programlisting>", "</code></p>");
                    blSectionStart = true; 
                }

                if (strLines[j].StartsWith("<")==false)
                {
                    if (blProgramListingStart == true)
                    {
                        strLines[j] = strLines[j] + "<br/>";
                        

                        if (strLines[j].StartsWith(" ") == true)
                        {
                           // MessageBox.Show("[" + strLines[j].ToString() + "]");
                            strLines[j] = Regex.Replace(strLines[j], "([ ][ ])", "&#x00A0;&#x00A0;");
                            strLines[j] = Regex.Replace(strLines[j], "(&#x00A0;[ ])", "&#x00A0;&#x00A0;");
                            strLines[j] = Regex.Replace(strLines[j], "^([ ])(.*)$", "&#x00A0;$2");
                           // MessageBox.Show("[" + strLines[j].ToString() + "]");
                        }
                    }
                }

                #endregion


                #region Para
                
                
                if (strLines[j].StartsWith("<para>"))
                {
                    //Table
                    strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)</para></entry>$", "<p class=\"body-text\">$1</p></td>");

                    if (strLines[j].IndexOf("<emphasis role=\"strong\">") > 0)
                    {

                        strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)<emphasis role=\"strong\">(.*)</emphasis></para>$", "<p class=\"subhead2\">$1$2</p>");
                    }


                    if (blSidebar == true)
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"box-para\">$1");
                    }
                    else
                    {
                        if (blblockquote == true)
                        {
                            strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"blockquote\">$1");
                        }
                        else
                        {

                            if (blNote == true)
                            {
                                if (blNoteStart == true)
                                {
                                    strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"hanging-note\"><small><b>NOTE</b></small> $1");
                                    blNoteStart = false;
                                }
                                else
                                {
                                    strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"hanging-note\">$1");
                                }

                            }
                            else
                            {


                                if (blCopyrightPage == true)
                                {

                                    strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"copyright\">$1");

                                }
                                else
                                {
                                    if (blTipStart == true)
                                    {

                                        strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"hanging-tip\"><small><b>TIP</b></small>&#x00A0;&#x00A0;&#x00A0;&#x00A0;&#x00A0;&#x00A0;$1");

                                    }
                                    else
                                    {

                                        if (blAppendix == true)
                                        {
                                            strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"hanging-indent\">$1");
                                        }
                                        else
                                        {
                                            if (blSectionStart == true)
                                            {

                                                strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"body-text\">$1");
                                            }
                                            else
                                            {

                                                strLines[j] = Regex.Replace(strLines[j], "^<para>(.*)$", "<p class=\"indent\">$1");

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }



                    blSectionStart = false;
                }

                #endregion


                #region Itemizedlist
                
                if (strLines[j].StartsWith("<itemizedlist mark=\"squf\">"))
                {
                    //strLines[j] = "";
                    strLines[j] = "<ul>";
                    blitemizedlist = true;
                }

                if (strLines[j].StartsWith("</itemizedlist>"))
                {
                    //strLines[j] = "";
                    strLines[j] = "</ul>";
                    blitemizedlist = false;
                    blSectionStart = true;
                }
                #endregion


                #region Orderedlist
                
                
                if (strLines[j].StartsWith("<orderedlist numeration=\"arabic\">"))
                {
                    //strLines[j] = "";
                    strLines[j] = "<ol>";
                    lnOrderedList = 0;
                    blorderedlist = true;
                }

                if (strLines[j].StartsWith("</orderedlist>"))
                {
                    //strLines[j] = "";
                    strLines[j] = "</ol>";
                    lnOrderedList = 0;
                    blorderedlist = false;
                    blSectionStart = true;
                }
                #endregion

                #region Blockquote
                
                

                if (strLines[j].StartsWith("<blockquote>"))
                {
                    strLines[j] = "";
                    blblockquote = true;
                }

                if (strLines[j].IndexOf("</blockquote>") > 0)
                {
                    strLines[j] = strLines[j].Replace("</blockquote>","");
                    blblockquote = false;
                    blSectionStart = true;

                }


                if (strLines[j].StartsWith("</blockquote>"))
                {
                    strLines[j] = "";
                    blblockquote = false;
                    blSectionStart = true;
                }

                #endregion


                #region Closing Sect, Figure
                //</example>

                if (strLines[j].StartsWith("</sect") || strLines[j].StartsWith("</figure>"))
                {
                    strLines[j] = "";
                    blSectionStart = true;
                }

                #endregion


                #region Closing example
                //</example>

                if (strLines[j].StartsWith("</example>"))
                {
                    strLines[j] = "";
                    blExampleStart  = false;
                    blSectionStart = true;
                }

                #endregion


                #region ListItem
                
                

                if (strLines[j].StartsWith("<listitem><para>"))
                {

                    if (blitemizedlist == true)
                    {
                        //Old
                        //strLines[j] = Regex.Replace(strLines[j], "^<listitem><para>(.*)</para></listitem>$", "<p class=\"hanging-list\"><img src=\"" + strIDPrefix + "015.jpg\" width=\"5\" height=\"5\" alt=\"icon014\"/> $1</p>");
                        strLines[j] = Regex.Replace(strLines[j], "^<listitem><para>(.*)</para></listitem>$", "<li><p>$1</p></li>");
                        
                    }
                    else
                    {
                        if (blorderedlist == true)
                        {
                            lnOrderedList++; 
                            //Old
                            //strLines[j] = Regex.Replace(strLines[j], "^<listitem><para>(.*)</para></listitem>$", "<p class=\"hanging-numberlist\">" + lnOrderedList.ToString() + " $1</p>");
                            strLines[j] = Regex.Replace(strLines[j], "^<listitem><para>(.*)</para></listitem>$", "<li><p>$1</p></li>");

                        }
                    }


                }

                #endregion


                #region Figure
                
                

                if (strLines[j].StartsWith("<figure "))
                {
                    lnFigure++;
                    if (strLines[j].IndexOf(" label=") > 0)
                    {
                        strChapterNumber = "Figure " + Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2") + "&#x00A0;&#x00A0;&#x00A0; ";
                    }
                    else
                    {
                        strChapterNumber = "";
                    }

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID = "";
                    }

                    strLines[j] = "";

                    j++;
                    strTempID2 = "";
                    if (strLines[j].StartsWith("<title>"))
                    {
                        strTempID2 = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<p class=\"figure-caption\"><b>" + strChapterNumber + "$1</b></p>");
                        strLines[j] = "";
                    }

                    j++;

                    if (strLines[j].StartsWith("<graphic"))
                    {
                        //File Name
                        strTempID3 = Regex.Replace(strLines[j], "^(.*) fileref=\"([^\"]*)\"(.*)$", "$2");
                        //Width
                        strTempID4 = Regex.Replace(strLines[j], "^(.*) width=\"([^\"]*)\"(.*)$", "$2");
                        //height
                        strTempID5 = Regex.Replace(strLines[j], "^(.*) depth=\"([^\"]*)\"(.*)$", "$2");



                        //strLines[j] = "<p class=\"figure-image\" id=\"" + strTempID + "\"><img src=\"" + strIDPrefix + strTempID3 + ".jpg\" width=\"" + strTempID4 + "\" height=\"" + strTempID5 + "\" alt=\"fig" + lnFigure.ToString("000") + "\"/></p>\n" + strTempID2; 
                        strLines[j] = "<p class=\"figure-image\" id=\"" + strTempID + "\"><img src=\"" + strTempID3 + "\" width=\"" + strTempID4 + "\" height=\"" + strTempID5 + "\" alt=\"" + strTempID3 + "\"/></p>\n" + strTempID2; 

                    }


                }
                #endregion



                #region Example



                if (strLines[j].StartsWith("<example "))
                {
                    lnExample++;
                    blExampleStart = true;
                    if (strLines[j].IndexOf(" label=") > 0 && strLines[j].IndexOf(" role=") > 0)
                    {
                        strTempID6 = Regex.Replace(strLines[j], "^(.*) role=\"([^\"]*)\"(.*)$", "$2");
                        strChapterNumber = strTempID6 + " " + Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2") + "&#x00A0;&#x00A0;&#x00A0; ";
                    }
                    else
                    {
                        strChapterNumber = "";
                    }

                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID = " id=\"" + Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2") + "\"";
                    }
                    else
                    {
                        strTempID = "";
                    }

                    strLines[j] = "";

                    j++;
                    strTempID2 = "";
                    if (strLines[j].StartsWith("<title>"))
                    {
                        strLines[j] = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "<p class=\"listing-script\"" + strTempID + "><b>" + strChapterNumber + "$1</b></p>");
                        //strLines[j] = "";
                    }

                    j++;

                    


                }

                if (blExampleStart == true)
                {
                    if (strLines[j].StartsWith("<programlisting") == false && strLines[j].StartsWith("<") == true)
                    {
                        //strLines[j] = "<p class=\"script\">" + strLines[j] + "</p>";

                    }
                    else
                    {
                        strLines[j] = strLines[j].Replace("<programlisting>", "<p class=\"script\"><code>");
                        blProgramListingStart = true;
                    }
                }


                #endregion



                
                #region Tip

                if (strLines[j].StartsWith("<tip>"))
                {
                    blTipStart = true;
                    strLines[j] = "";


                }

                if (strLines[j].StartsWith("</tip>"))
                {
                    blTipStart = false ;
                    strLines[j] = "";


                }



                #endregion

                #region Table



                if (strLines[j].StartsWith("<table "))
                {
                    lnTable++;
                    strChapterNumber = "";
                    if (strLines[j].IndexOf(" label=") > 0)
                    {
                        strChapterNumber = "Table " + Regex.Replace(strLines[j], "^(.*) label=\"([^\"]*)\"(.*)$", "$2") + "&#x00A0;&#x00A0;&#x00A0; ";
                    }
                    else
                    {
                        strChapterNumber = "";
                    }



                    if (strLines[j].IndexOf(" id=") > 0)
                    {
                        strTempID = Regex.Replace(strLines[j], "^(.*) id=\"([^\"]*)\"(.*)$", "$2");
                    }
                    else
                    {
                        strTempID = "";
                    }

                    strLines[j] = "";

                    j++;
                    strTempID2 = "";
                    if (strLines[j].StartsWith("<title>"))
                    {
                        strTempID2 = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                        strLines[j] = "<table id=\"" + strTempID + "\" cellpadding=\"2\" cellspacing=\"0\">\n<p class=\"table-caption\"><b>" + strChapterNumber + strTempID2 + "</b></p>";
                    }
                    else
                    {
                        j--;
                        strLines[j] = "<table id=\"" + strTempID + "\" cellpadding=\"2\" cellspacing=\"0\">";
                        

                    }



                }


                if (strLines[j].StartsWith("<row>"))
                {
                    strLines[j] = "<tr>";
                }
                if (strLines[j].StartsWith("</row>"))
                {
                    strLines[j] = "</tr>";
                }

                if (strLines[j].StartsWith("<entry"))
                {
                    strLines[j] = Regex.Replace(strLines[j], "^<entry(.*)><para>(.*)</para></entry>$", "<td$1>$2</td>");
                    strLines[j] = Regex.Replace(strLines[j], "^<entry(.*)><para>(.*)</para>$", "<td$1><p>$2</p>");
                    strLines[j] = Regex.Replace(strLines[j], "^<entry(.*)><para>(.*)$", "<td$1>$2");
                    
                    //MessageBox.Show(strLines[j]);  
                }
                

                if (strLines[j].StartsWith("<tgroup") || strLines[j].StartsWith("<colspec") || strLines[j].StartsWith("<thead>") || strLines[j].StartsWith("</thead>") || strLines[j].StartsWith("<tbody>") || strLines[j].StartsWith("</tbody>") || strLines[j].StartsWith("</tgroup>"))
                {
                    strLines[j] = "";
                    blSectionStart = true;
                }

                #endregion

                #region Remove Index

                if (strLines[j].IndexOf("<indexterm ") > 0 && strLines[j].LastIndexOf("</indexterm>") > 0)
                {
                    //MessageBox.Show(strLines[j] + strLines[j].IndexOf("<indexterm ").ToString());
                        strLines[j] = strLines[j].Remove(strLines[j].IndexOf("<indexterm "), (strLines[j].LastIndexOf("</indexterm>") - strLines[j].IndexOf("<indexterm ") + 12));
                    //MessageBox.Show(strLines[j]);
                }

                #endregion



                #region Some General Replacings
                //</para></entry>

                strLines[j] = strLines[j].Replace("</para></entry>", "</td>");
                strLines[j] = strLines[j].Replace("</programlisting>", "</code></p>");
                strLines[j] = strLines[j].Replace("<programlisting>", "<p class=\"script\"><code>");
                strLines[j] = strLines[j].Replace("</entry>", "</td>");
                strLines[j] = strLines[j].Replace("<p class=\"script\"><br/></p>", "<br/>");
                //Replace all general things
                strLines[j] = strLines[j].Replace("</para>", "</p>");
                //strLines[j] = strLines[j].Replace("<p class=\"script\"><p class=\"script\"><code>",
                strLines[j] = strLines[j].Replace("<listitem><para>", "<li><p>");
                strLines[j] = strLines[j].Replace("</para></listitem>", "</p></li>");
                strLines[j] = strLines[j].Replace("<listitem>", "<li>");
                strLines[j] = strLines[j].Replace("</listitem>", "</li>");
                strLines[j] = strLines[j].Replace("<entry", "<td");
                strLines[j] = strLines[j].Replace("<td align=\"center\" valign=\"bottom\">", "<td>");
                //
                if (strLines[j].IndexOf("<literal>") > 0)
                {
                    //MessageBox.Show(strLines[j]);    
                    strLines[j] = Regex.Replace(strLines[j], "<literal>([^<]+)</literal>", "<code>$1</code>", RegexOptions.RightToLeft );
                    //MessageBox.Show(strLines[j]);
                }

                if (strLines[j].IndexOf("<emphasis>") > 0)
                {
                    //MessageBox.Show(strLines[j]);
                    strLines[j] = Regex.Replace(strLines[j], "<emphasis>([^<]+)</emphasis>", "<i>$1</i>", RegexOptions.RightToLeft);
                    //MessageBox.Show(strLines[j]);
                }

                if (strLines[j].IndexOf("<informalfigure>") >= 0)
                {
                    //MessageBox.Show(strLines[j]);
                    //strLines[j] = Regex.Replace(strLines[j], "<informalfigure><graphic fileref=\"figs/([^<> ]+).png\"/></informalfigure>", "<img src=\"images/$1.png\" alt=\"$1\"/>", RegexOptions.RightToLeft);
                    strLines[j] = Regex.Replace(strLines[j], "<informalfigure><graphic fileref=\"([^<> ]+)\"/></informalfigure>", "<img src=\"$1\" alt=\"$1\"/>", RegexOptions.RightToLeft);
                    //MessageBox.Show(strLines[j]);
                }

                if (strLines[j].IndexOf("<informalfigure>") < 0 && strLines[j].IndexOf("<graphic") >=0)
                {
                    //MessageBox.Show(strLines[j]);
                    //strLines[j] = Regex.Replace(strLines[j], "<graphic fileref=\"figs/([^<> ]+).png\"/>", "<img src=\"" + strIDPrefix + "/$1.png\" alt=\"$1\"/>", RegexOptions.RightToLeft);
                    strLines[j] = Regex.Replace(strLines[j], "<graphic fileref=\"([^<> ]+)\"/>", "<img src=\"$1\" alt=\"$1\"/>", RegexOptions.RightToLeft);
                    //MessageBox.Show(strLines[j]);
                }


                if (strLines[j].IndexOf("<systemitem role=\"url\">") >= 0)
                {
                    //MessageBox.Show(strLines[j]);
                    strLines[j] = Regex.Replace(strLines[j], "<systemitem role=\"url\">([^<>]+)</systemitem>", "<a href=\"$1\">$1</a>", RegexOptions.RightToLeft);
                    //MessageBox.Show(strLines[j]);
                }

                if (strLines[j].IndexOf("<systemitem role=\"httpurl\">") >= 0)
                {
                    //MessageBox.Show(strLines[j]);
                    strLines[j] = Regex.Replace(strLines[j], "<systemitem role=\"httpurl\">([^<>]+)</systemitem>", "<a href=\"http://$1\">$1</a>", RegexOptions.RightToLeft);
                    //MessageBox.Show(strLines[j]);
                }




                #endregion

                toolStripProgressBar1.Value = j+1;

            }

            #endregion


            this.Refresh();

            rtbContent.Text = string.Join("\n", strLines);
            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum; 
            //toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;

            //IDLinking();
            IDLinkingVer2();

        }



        private string GeneralReplace(string strLine)
        {
            string strRpl = "";
            
            strRpl = Regex.Replace(strLine, "^(.*)<?docpage cont page=\"([^>]+)\"?>(.*)$", "$1****$3").Replace("<?****","");
            //MessageBox.Show(strRpl); 
            strRpl = Regex.Replace(strRpl, "<?docpage([^>]+)\"([^>\"]+)\"", "<a id=\"page_$2\"></a>", RegexOptions.RightToLeft).Replace("<?<a", "<a").Replace("a>?>", "a>");
            //strRpl = Regex.Replace(strRpl, "^(.*)<?docpage([^>]+)\"([^>\"]+)\">(.*)", "$1$3", RegexOptions.RightToLeft).Replace("<?<a", "<a").Replace("a>?>", "a>");
            //<?docpage num="iv"?>

            return strRpl; 
        }


        private void ConvertFootNote()
        {
            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Converting Footnotes ... Please Wait";
            this.Refresh();
            string strContent = "";

              

            string[] strLines;

            stkIDs = new System.Collections.Stack();
            stkIDs.Clear();

            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            toolStripProgressBar1.Maximum = Convert.ToInt32(i)+1;
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            this.Refresh();

            
            

            bool blNoteStart = false;
            string strFn = "";
            long lnBibNo = 0;
            
            MatchCollection mc;
      
            #region First Loop


            //Creating IDs
            for (int j = 0; j < i; j++)
            {


                if (strLines[j].StartsWith("<note")) 
                {
                    
                    blNoteStart = true;
                    strLines[j] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"+
                        "<!DOCTYPE noteGroup PUBLIC \"-//OXFORD//DTD OXCHAPML//EN\" \"OxChapML.dtd\">\n"+
                        "<!-- [DTD] OxChapML, v2.5 -->\n"+
                        "<!-- [TCI] Oxford Scholarship Online Text Capture Instructions, v1.2 -->\n" +
                        "<!-- [TCI] OUP Bibliographic reference capture, v1.15 -->\n"+
                        "<noteGroup>";

                }



                if (strLines[j].StartsWith("</note>"))
                {
                    blNoteStart = false;

                    strLines[j] = "</noteGroup>";
                }

                if (strLines[j].StartsWith("<fn"))
                {


                    strFn = Regex.Replace(strLines[j], "^<fn([0-9]+)>(.*)", "$1");


                    mc = Regex.Matches(strLines[j], "</bibn>", RegexOptions.RightToLeft);
                    int intFirstBibStart = 0;
                    int intFirstBibEnd = 0;
                    string strBIB = "";

                    foreach (Match singleMc in mc)
                    {
                        lnBibNo++;
                        intFirstBibStart = strLines[j].IndexOf("<bibn>");
                        intFirstBibEnd = strLines[j].IndexOf("</bibn>");
                        //MessageBox.Show(strLines[j]);
                        strBIB = strLines[j].Substring(intFirstBibStart, (intFirstBibEnd - intFirstBibStart) + 7);
                        //MessageBox.Show(strBIB);
                        strLines[j] = strLines[j].Remove(intFirstBibStart, (intFirstBibEnd - intFirstBibStart) + 7);
                        //MessageBox.Show(strLines[j]);
                        strLines[j] = strLines[j].Insert(intFirstBibStart, ConvertSingleBibn(strBIB, lnBibNo.ToString()));
                        //MessageBox.Show(strLines[j]);


                    }

                    strLines[j] = Regex.Replace(strLines[j], "^<fn([0-9]+)>(.*)</fn>$", "<note id=\"" + strIDPrefix + "-note-$1\" type=\"footnote\"><p><enumerator><sup>$1</sup></enumerator> $2</p></note>");


                }





                toolStripProgressBar1.Value = toolStripProgressBar1.Value + 1;

            }

            #endregion


            this.Refresh();

            rtbContent.Text = string.Join("\n", strLines);
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;




        }

        private string ConvertAU(string strAU)
        {
            string strAuthor = strAU;
            strAuthor = Regex.Replace(strAuthor, "([^<]+) ([^< ]+)", "$1XXXX$2");  
            return strAuthor; 
        }


        private void convertRefToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 frm2 = new Form2();
            frm2.ShowDialog();
            strIDPrefix = clsStaticVrs.getID();
            //MessageBox.Show(strIDPrefix);  
            ConvertFootNote();
            lnFnsConverted++;

        }


        private string ConvertSingleBibn(string strBibn, string strFn)
        {
            strBibn = strBibn.Replace("<bibn>", "").Replace("</bibn>", "");

            string strpublisher1 = "";
            string strAu1 = "";
            string strTitle1 = "";
            string strYear = "";
            string strPage = "";
            string strVol = "";

            string strBib1 = "";
            

            MatchCollection mAu;

            mAu = Regex.Matches(strBibn, "<an>([^<>]+)</an>");
            strAu1 = "";
            int k = 0;


            foreach (Match singleMc in mAu)
                {
                    if (k == 0)
                    {
                        strAu1 = Regex.Replace(singleMc.Value.ToString()  , "^<an>([^<>]+) ([^ ]+)</an>$", "$1 $2");
                    }
                    else
                    {

                        strAu1 = strAu1 + " | " + Regex.Replace(singleMc.Value.ToString(), "^<an>([^<>]+) ([^ ]+)</an>$", "$1 $2");
                    }
                    k++;    
                }

                if (strAu1.Length > 1)
                {
                    strAu1 = " author=\"" + strAu1 + "\"";
                }
            



            strBibn = Regex.Replace(strBibn, "<an>([^<>]+) ([^ ]+)</an>", "<nameGrp mainName=\"$2\" foreNames=\"$1\">$1 $2</nameGrp>", RegexOptions.RightToLeft);

            strBibn = Regex.Replace(strBibn, "ed. <nameGrp([^<>]+)>", "ed. <nameGrp$1 role=\"editor\">",RegexOptions.RightToLeft);
            strBibn = Regex.Replace(strBibn, "trans. <nameGrp([^<>]+)>", "trans. <nameGrp$1 role=\"translator\">", RegexOptions.RightToLeft);
            strBibn = Regex.Replace(strBibn, "rev. <nameGrp([^<>]+)>", "rev. <nameGrp$1 role=\"reviser\">", RegexOptions.RightToLeft);
            //ed. <nameGrp mainName="Launay" foreNames="Michel" role="editor">
            //Title

            if (Regex.IsMatch(strBibn, "^(.*)<i>([^<>]+)</i>(.*)$"))
            {

                strTitle1 = " title=\"" + Regex.Replace(strBibn, "^(.*)<i>([^<>]+)</i>(.*)$", "$2") + "\"";

            }
            else
            {
                strTitle1 = "";
            }


            //Date

            if (Regex.IsMatch(strBibn, "^(.*)<yn>([^<>]+)</yn>(.*)$"))
            {

                strYear = " date=\"" + Regex.Replace(strBibn, "^(.*)<yn>([^<>]+)</yn>(.*)$", "$2") + "\"";

            }
            else
            {
                strYear = "";
            }


            //Page
            if (Regex.IsMatch(strBibn, "^(.*), ([0-9]+)(.*)$"))
            {

                strPage = " page=\"" + Regex.Replace(strBibn, "^(.*), ([0-9]+)(.*)$", "$2") + "\"";

            }
            else
            {
                if (Regex.IsMatch(strBibn, "^(.*) p. ([0-9]+)(.*)$"))
                {

                    strPage = " page=\"" + Regex.Replace(strBibn, "^(.*) p. ([0-9]+)(.*)$", "$2") + "\"";

                }
                else
                {

                    if (Regex.IsMatch(strBibn, "^(.*) pp. ([0-9]+)(.*)$"))
                    {

                        strPage = " page=\"" + Regex.Replace(strBibn, "^(.*) pp. ([0-9]+)(.*)$", "$2") + "\"";

                    }
                    else
                    {
                        strPage = "";
                    }

                }
            }


            //Vol

            if (Regex.IsMatch(strBibn, "^(.*) vol. ([A-z0-9]+)(.*)$"))
            {

                strVol = " vol=\"" + Regex.Replace(strBibn, "^(.*) vol. ([A-z0-9]+)(.*)$", "$2") + "\"";

            }
            else
            {
                strVol = "";
            }



            //Publ
            //strBibn = ConvertAU(strBibn);
            if (Regex.IsMatch(strBibn, "^(.*)<locn>([^<>]+): ([^<>]*)</locn>(.*)$"))
            {

                strpublisher1 = Regex.Replace(strBibn, "^(.*)<locn>([^<>]+): ([^<>]*)</locn>(.*)$", " place=\"$2\" publisher=\"$3\"");


            }
            else
            {
                if (Regex.IsMatch(strBibn, "^(.*)<locn>([^<>]+)</locn>(.*)$"))
                {

                    strpublisher1 = Regex.Replace(strBibn, "^(.*)<locn>([^<>]+)</locn>(.*)$", " place=\"$2\" publisher=\"***\"");

                }
                else
                {
                    strpublisher1 = "";
                }
            }


            strBib1 = "<bibItem id=\"" + strIDPrefix + "-bibItem-" + strFn + "\" class=\"book\"" + strAu1 + strVol + strYear + strPage + strpublisher1 + strTitle1 + ">" + strBibn.Replace("<locn>", "").Replace("</locn>", "").Replace("<yn>", "").Replace("</yn>", "").Replace("<an>", "").Replace("</an>", "") + "</bibItem>";

            /*
            if (Regex.IsMatch(strLines[j], "^(.*)<bibn>(.*)</bibn>(.*)$"))
            {
                strBib2 = strBib + Regex.Replace(strLines[j], "^(.*)<bibn>(.*)<an>(.*)</an>(.*)</bibn>(.*)$", "$2" + strAu2 + "$4").Replace("<locn>", "").Replace("</locn>", "").Replace("<yn>", "").Replace("</yn>", "") + "</bibItem>";
                //MessageBox.Show(strBib2);

                strLines[j] = Regex.Replace(strLines[j], "^<fn([0-9]+)>(.*)<bibn>(.*)</bibn>(.*)</fn>$", "<note id=\"acprof-9780199208098-note-$1\" type=\"footnote\"><p><enumerator><sup>$1</sup></enumerator> $2" + strBib2 + "$4</p></note>");

            }
            */


            return strBib1;
        }

        private void testToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            

            DataTable dtIds = new DataTable();
            dtIds.Columns.Add(new DataColumn("IDX", typeof(string)));
            dtIds.Columns.Add(new DataColumn("FileName", typeof(string)));


            string[] strLines;
            string strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;
            long x = 0;
            string strFileNames = "";
            bool blSplitStart = false;
            MatchCollection mc;

            string[] strIDs = new string[100000];

            
            for (long k = 0; k < i; k++)
            {

                if (strLines[k].StartsWith("<split"))
                {
                    strFileNames = Regex.Replace(strLines[k], "^<split filename=\"([^<>]+)\">$", "$1");
                    blSplitStart = true;

                }

                if (strLines[k].StartsWith("</split"))
                {
                    strFileNames = "";
                    blSplitStart = false;

                }



                if (blSplitStart == true)
                {


                    if (strLines[k].IndexOf(" id=") >= 0)
                    {

                        mc = Regex.Matches(strLines[k], "id=\"([^<>]+)\"");

                        foreach (Match singleMc in mc)
                        {
                            //strIDs[x] = singleMc.Result("$1") + " | " + strFileNames;
                            //x++;

                            //strIDs.
                            DataRow r = dtIds.NewRow();
                            r["IDX"] = singleMc.Result("$1");
                            r["FileName"] = strFileNames;
                            dtIds.Rows.Add(r);


                        }
                    }

                }
            }

            
            //MessageBox.Show(GetLinkRef(dtIds, "about_this_book"));


            for (long k = 0; k < i; k++)
            {
                if (strLines[k].IndexOf("<link") >= 0)
                {
                    mc = Regex.Matches(strLines[k], "<link linkend=\"([^<>]+)\">([^<>]+)</link>");
                    int intFirstBibStart = 0;
                    int intFirstBibEnd = 0;
                    string strBIB = "";

                    foreach (Match singleMc in mc)
                    {
                        
                        intFirstBibStart = strLines[k].IndexOf("<link");
                        intFirstBibEnd = strLines[k].IndexOf("</link>");
                        //MessageBox.Show(intFirstBibStart.ToString()+"--" + intFirstBibEnd.ToString()+"--"+   strLines[k] );

                        if ((intFirstBibStart < intFirstBibEnd) && (intFirstBibStart > 0) && (intFirstBibEnd > 0))
                        {
                            strBIB = strLines[k].Substring(intFirstBibStart, (intFirstBibEnd - intFirstBibStart) + 7);
                            strLines[k] = strLines[k].Remove(intFirstBibStart, (intFirstBibEnd - intFirstBibStart) + 7);
                            strLines[k] = strLines[k].Insert(intFirstBibStart, "<a href=\""+ GetLinkRef(dtIds, singleMc.Result("$1")) + "\">" + singleMc.Result("$2") + "</a>");
                            
                        }

                    }



                }

            }


           
            rtbContent.Text = string.Join("\n", strLines);
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;


        }



        private void IDLinking()
        {



            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Creating ID Table ... Please Wait";
            this.Refresh();
            
            

            DataTable dtIds = new DataTable();
            dtIds.Columns.Add(new DataColumn("IDX", typeof(string)));
            dtIds.Columns.Add(new DataColumn("FileName", typeof(string)));


            string[] strLines;
            string strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;
       
            string strFileNames = "";
            bool blSplitStart = false;
            MatchCollection mc;



            toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            this.Refresh();




            for (int k = 0; k < i; k++)
            {

                if (strLines[k].StartsWith("<split"))
                {
                    strFileNames = Regex.Replace(strLines[k], "^<split filename=\"([^<>]+)\">$", "$1");
                    blSplitStart = true;

                }

                if (strLines[k].StartsWith("</split"))
                {
                    strFileNames = "";
                    blSplitStart = false;

                }



                if (blSplitStart == true)
                {


                    if (strLines[k].IndexOf("id=") >= 0)
                    {

                        mc = Regex.Matches(strLines[k], "id=\"([^<>\" ]+)\"");
                        //MessageBox.Show(strLines[k]);  
                        foreach (Match singleMc in mc)
                        {
                            //strIDs[x] = singleMc.Result("$1") + " | " + strFileNames;
                            //x++;

                            //strIDs.
                            DataRow r = dtIds.NewRow();
                            r["IDX"] = singleMc.Result("$1");
                            //MessageBox.Show(singleMc.Result("$1"));
                            r["FileName"] = strFileNames;
                            dtIds.Rows.Add(r);


                        }
                    }

                }
                toolStripProgressBar1.Value = k+1;
            }

            toolStripProgressBar1.Value = 1;
            toolStripStatusLabel1.Text = "Linking IDs ... Please Wait";

            this.Refresh();

           // MessageBox.Show(GetLinkRef(dtIds, "core_esb_features_and_capabilities"));
           // MessageBox.Show("TEst");

            for (int k = 0; k < i; k++)
            {
                if (strLines[k].IndexOf("<link") >= 0)
                {
                    mc = Regex.Matches(strLines[k], "<link linkend=\"([^<>]+)\">([^<>]+)</link>");
                    //mc = Regex.Matches(strLines[k], "<link linkend=\"([^<>]+)\">");
                    int intFirstBibStart = 0;
                    int intFirstBibEnd = 0;
                    string strBIB = "";

                    foreach (Match singleMc in mc)
                    {

                        intFirstBibStart = strLines[k].IndexOf("<link");
                        intFirstBibEnd = strLines[k].IndexOf("</link>");
                        //MessageBox.Show(intFirstBibStart.ToString()+"--" + intFirstBibEnd.ToString()+"--"+   strLines[k] );

                        if ((intFirstBibStart < intFirstBibEnd) && (intFirstBibStart >= 0) && (intFirstBibEnd >= 0))
                        {
                            strBIB = strLines[k].Substring(intFirstBibStart, (intFirstBibEnd - intFirstBibStart) + 7);
                            strLines[k] = strLines[k].Remove(intFirstBibStart, (intFirstBibEnd - intFirstBibStart) + 7);
                            strLines[k] = strLines[k].Insert(intFirstBibStart, "<a href=\"" + GetLinkRef(dtIds, singleMc.Result("$1")) + "\">" + singleMc.Result("$2") + "</a>");

                        }

                    }



                }
                toolStripProgressBar1.Value = k + 1;
            }



            rtbContent.Text = string.Join("\n", strLines);
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;



        }






        private void IDLinkingVer2()
        {



            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Creating ID Database ... Please Wait";
            this.Refresh();



            DataTable dtIds = new DataTable();
            dtIds.Columns.Add(new DataColumn("IDX", typeof(string)));
            dtIds.Columns.Add(new DataColumn("FileName", typeof(string)));


            string[] strLines;
            string strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            string strFileNames = "";
            bool blSplitStart = false;
            bool blBfCtStart = false;
            MatchCollection mc;



            toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            this.Refresh();




            for (int k = 0; k < i; k++)
            {

                if (strLines[k].StartsWith("<split"))
                {
                    strFileNames = Regex.Replace(strLines[k], "^<split filename=\"([^<>]+)\">$", "$1");
                    blSplitStart = true;

                }

                if (strLines[k].StartsWith("</split"))
                {
                    strFileNames = "";
                    blSplitStart = false;

                }



                if (blSplitStart == true)
                {


                    if (strLines[k].IndexOf("id=") >= 0)
                    {

                        mc = Regex.Matches(strLines[k], "id=\"([^<>\" ]+)\"");
                        //MessageBox.Show(strLines[k]);  
                        foreach (Match singleMc in mc)
                        {
                            //strIDs[x] = singleMc.Result("$1") + " | " + strFileNames;
                            //x++;

                            //strIDs.
                            DataRow r = dtIds.NewRow();
                            r["IDX"] = singleMc.Result("$1");
                            //MessageBox.Show(singleMc.Result("$1"));
                            r["FileName"] = strFileNames;
                            dtIds.Rows.Add(r);


                        }
                    }

                }
                toolStripProgressBar1.Value = k + 1;
            }

            toolStripProgressBar1.Value = 1;
            toolStripStatusLabel1.Text = "Linking IDs ... Please Wait";

            this.Refresh();

            // MessageBox.Show(GetLinkRef(dtIds, "core_esb_features_and_capabilities"));
            // MessageBox.Show("TEst");

            for (int k = 0; k < i; k++)
            {

                if (strLines[k].StartsWith("<a id=\"brief_contents\">"))
                {
                    blBfCtStart = true;
                }

                if (strLines[k].StartsWith("</split"))
                {
                    blBfCtStart = false;
                }

                
                if (strLines[k].IndexOf("<link") >= 0)
                {

                    if (blBfCtStart == true)
                    {
                        mc = Regex.Matches(strLines[k], "<link linkend=\"([^<>]+)\">(.*)</link>");
                        
                    }
                    else
                    {
                        mc = Regex.Matches(strLines[k], "<link linkend=\"([^<>]+)\">([^<>]+)</link>");
                    }
                    //mc = Regex.Matches(strLines[k], "<link linkend=\"([^<>]+)\">");
                    int intFirstBibStart = 0;
                    int intFirstBibEnd = 0;
                    string strBIB = "";

                    foreach (Match singleMc in mc)
                    {

                        intFirstBibStart = strLines[k].IndexOf("<link");
                        intFirstBibEnd = strLines[k].IndexOf("</link>");
                        //MessageBox.Show(intFirstBibStart.ToString()+"--" + intFirstBibEnd.ToString()+"--"+   strLines[k] );

                        if ((intFirstBibStart < intFirstBibEnd) && (intFirstBibStart >= 0) && (intFirstBibEnd >= 0))
                        {
                            strBIB = strLines[k].Substring(intFirstBibStart, (intFirstBibEnd - intFirstBibStart) + 7);
                            strLines[k] = strLines[k].Remove(intFirstBibStart, (intFirstBibEnd - intFirstBibStart) + 7);
                            strLines[k] = strLines[k].Insert(intFirstBibStart, "<a href=\"" + GetLinkRef(dtIds, singleMc.Result("$1")) + "\">" + singleMc.Result("$2") + "</a>");

                        }

                    }



                }
                toolStripProgressBar1.Value = k + 1;
            }



            rtbContent.Text = string.Join("\n", strLines);
            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;



        }







        private string GetLinkRef(DataTable dtX, string strID)
        {
            string strLinkRef = "";
            DataRow[] drs;

            drs = dtX.Select("IDX = '" + strID + "'");
            /*
            if (drs.Length == 0)
            {
                //MessageBox.Show(strID);  
            }
            */
            foreach (DataRow drx in drs)
            {
                strLinkRef = drx[1].ToString() + "#" +strID;

            }

            return strLinkRef;
        }



        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            string strOEBPS = "";
            if (strOpenedBook.Length > 3)
            {

                WriteLog("SplitStart\t" + strOpenedBook);
                fbdSplit.Description = "Select the output folder, all files will be created in this folder(Blank Folder preferred!)"; 

                if (fbdSplit.ShowDialog() == DialogResult.OK)
                {
                    string strPath = fbdSplit.SelectedPath.ToString();
                    //SplitNSave(strPath);
                    //createOPFAuto(strPath);
                    //createNCXAuto(strPath);
                    DirectoryInfo di = new DirectoryInfo(strPath);
                    if (di.Exists)
                    {
                        Form2 frm2 = new Form2();
                        frm2.ShowDialog();
                        strOEBPS = clsStaticVrs.getID();
                        //MessageBox.Show(strOEBPS);  
                        DirectoryInfo di12 = new DirectoryInfo(strPath + "\\" + strOEBPS);
                        if (di12.Exists)
                        {
                            if (MessageBox.Show("Folder '" + strOEBPS + "' already exist in " + strPath + "\nDo you want to delete existing files?", "Delete Files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                di12.Delete(true);
                                di.CreateSubdirectory(strOEBPS);
                            }
                        }
                        else
                        {
                            di.CreateSubdirectory(strOEBPS);
                        }

                            DirectoryInfo di1 = new DirectoryInfo(strPath + "\\" + strOEBPS + "\\xhtml");
                            if (di1.Exists)
                            {
                                if (MessageBox.Show("Folder 'xhtml' already exist in '" + strPath + "\\" + strOEBPS + "'\nDo you want to delete existing files?", "Delete Files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    di1.Delete(true);
                                    di12.CreateSubdirectory("xhtml");
                                }
                            }
                            else
                            {
                                di12.CreateSubdirectory("xhtml");
                            }

                    }
                    
                    lnFnsConverted++;
                    SplitPUK(strPath + "\\" + strOEBPS + "\\xhtml");
                }
            }
            else
            {
                MessageBox.Show("No Opened file found!", "Split", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }


        private void SplitNSave(string strPath)
        {

            
            
            string strSavePath = strPath; 


            if (strSavePath.Length > 2)
            {

                try
                {

                    Application.UseWaitCursor = true;
                    toolStripStatusLabel1.Text = "Spliting Files... Please Wait";
                    this.Refresh();
                    string strContent = "";

                    string[] strLines;

                    strContent = rtbContent.Text;
                    strLines = strContent.Split('\n');
                    long i = strLines.Length;

                    toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
                    toolStripProgressBar1.Minimum = 1;
                    toolStripProgressBar1.Value = 1;
                    this.Refresh();



                    StreamWriter swFiles;
                    string strFileNames = "";
                    bool blSplitStart = false;

                    swFiles = new StreamWriter(strSavePath + "\\tmpK01x.del");

                    for (int j = 0; j < i; j++)
                    {



                        //<split filename=\"chap" + lnChapter.ToString("00") + ".xhtml\">

                        if (strLines[j].StartsWith("<split"))
                        {
                            strFileNames = Regex.Replace(strLines[j], "^<split filename=\"([^<>]+)\">$", "$1");
                            swFiles.Flush();
                            swFiles.Close();
                            swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                            blSplitStart = true;

                        }

                        if (strLines[j].StartsWith("<split") == false && strLines[j].StartsWith("</split") == false && blSplitStart == true)
                        {
                            if (strLines[j] != "")
                            {
                                swFiles.WriteLine(strLines[j]);
                            }
                        }



                        if (strLines[j].StartsWith("</split"))
                        {

                            blSplitStart = false;
                        }



                        toolStripProgressBar1.Value = j + 1;

                    }

                    swFiles.Flush();
                    swFiles.Close();


                    this.Refresh();

                    rtbContent.Text = string.Join("\n", strLines);
                    toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                    toolStripStatusLabel1.Text = "Deleting Temp Files... Please Wait";

                    FileInfo fl = new FileInfo(strSavePath + "\\tmpK01x.del");
                    fl.Delete();
                    toolStripStatusLabel1.Text = "Ready";
                    Application.UseWaitCursor = false;
                }
                catch
                {
                    MessageBox.Show("Unexpected Error", "ERR", MessageBoxButtons.OK);   

                }
            }
        }


        private void SplitPUK(string strPath)
        {



            string strSavePath = strPath;
            long lnChapter = 0;
            long lnPart = 0;
            long lnSvg = 0;
            long lnFm = 0;
            long lnBm = 0;
            long lnAppe = 0;
            long lnSections = 0;
            string strTitle = "";
            string strTitleXX = "";
            string strCurrentFile = "";

            strPUKFileNames = new string[1000, 4];
            int intFiles = 0;
            if (strSavePath.Length > 2)
            {

                try
                {

                    Application.UseWaitCursor = true;
                    toolStripStatusLabel1.Text = "Spliting Files... Please Wait";
                    this.Refresh();
                    string strContent = "";

                    string[] strLines;

                    strContent = rtbContent.Text;
                    strLines = strContent.Split('\n');
                    long i = strLines.Length;

                    toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
                    toolStripProgressBar1.Minimum = 1;
                    toolStripProgressBar1.Value = 1;
                    this.Refresh();

                    bool isChapter = false;
                    bool isPart = false;
                    bool isSection = false;
                    for (int j = 0; j < i; j++)
                    {
                        if (strLines[j].StartsWith("<div class=\"pn\"") == true)
                        {
                            isPart = true;
                        }

                        if (strLines[j].StartsWith("<div class=\"cn\"") == true)
                        {
                            isChapter = true;
                        }

                        
                        if (strLines[j].StartsWith("<div class=\"sn\"") == true)
                        {
                            isSection = true;
                        }


                    }
                    /*
                    MessageBox.Show(isPart.ToString());
                    MessageBox.Show(isChapter.ToString());
                     */

                    string strChapterLevel = "";
                    string strPartLevel = "";
                    string strsectionLevel = "";

                    if (isSection && isChapter && isPart)
                    {
                        strPartLevel = "0";
                        strChapterLevel = "1";
                        strsectionLevel = "2";


                    }
                    else
                    {
                        if (isChapter && isPart)
                        {
                            strPartLevel = "0";
                            strChapterLevel = "1";
                            strsectionLevel = "0";

                        }
                        else
                        {
                            if (isChapter)
                            {
                                strPartLevel = "0";
                                strChapterLevel = "0";
                                strsectionLevel = "0";

                            }
                            else
                            {
                                strPartLevel = "0";
                                strChapterLevel = "0";
                                strsectionLevel = "0";


                            }

                        }
                    }
                    
                    StreamWriter swFiles;
                    string strFileNames = "";
                    
                    bool blSplitStart = false;

                    swFiles = new StreamWriter(strSavePath + "\\tmpK01x.del");

                    for (int j = 0; j < i; j++)
                    {
                        if (strTitle == "")
                        {
                            if (Regex.IsMatch(strLines[j], "^<title>(.*)</title>$"))
                            {
                                strTitle = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                            }
                        }

                        //<split filename=\"chap" + lnChapter.ToString("000") + ".xhtml\">

                        if (strLines[j].StartsWith("<div class=\"cn\"") == true)
                        {
                            lnChapter++;
                            
                            strFileNames = "chapter" + lnChapter.ToString("000") + ".xhtml";
                            strCurrentFile = strFileNames;
                            if (Regex.IsMatch(strLines[j], "^<div class=\"cn\">(.*)</div>$"))
                            {
                                strTitleXX = Regex.Replace(strLines[j], "^<div class=\"cn\">(.*)</div>$", "$1");
                                if (strTitleXX.Trim() == "")
                                {
                                    strTitleXX = lnChapter.ToString(); 
                                }
                            }
                            else
                            {
                                strTitleXX = lnChapter.ToString(); 
                            }
                            intFiles++;
                            swFiles.WriteLine("</body>\n</html>");
                            swFiles.Flush();
                            swFiles.Close();
                            swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                            swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                "<head>\n" +
                                "<title>" + strTitle + "</title>\n" +
                                "</head>\n" +
                                "<body class=\"book\">");

                            swFiles.WriteLine(strLines[j]);
                            blSplitStart = true;

                            if (strLines[j+1].StartsWith("<div class=\"ct\"") == true)
                            {
                                strPUKFileNames[intFiles, 0] = strFileNames;
                                strPUKFileNames[intFiles, 1] = CreateTitle(strTitleXX, strLines[j + 1], "cn");
                                strPUKFileNames[intFiles, 2] = strChapterLevel;
                                strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting(strTitleXX, strLines[j + 1], "cn");
                                j++;
                                swFiles.WriteLine(strLines[j]);
                            }
                            else
                            {
                                strPUKFileNames[intFiles, 0] = strFileNames;
                                strPUKFileNames[intFiles, 1] = CreateTitle(strTitleXX, "", "cn");
                                strPUKFileNames[intFiles, 2] = strChapterLevel;
                                strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting(strTitleXX, "", "cn");
                            }

                            
                                   
                            

                        }
                        else
                        {

                            if (strLines[j].StartsWith("<div class=\"pn\">") == true)
                            {
                                lnPart++;

                                strFileNames = "part" + lnPart.ToString("000") + ".xhtml";
                                strCurrentFile = strFileNames;
                                if (Regex.IsMatch(strLines[j], "^<div class=\"pn\">(.*)</div>$"))
                                {
                                    strTitleXX = Regex.Replace(strLines[j], "^<div class=\"pn\">(.*)</div>$", "$1");
                                    if (strTitleXX.Trim() == "")
                                    {
                                        strTitleXX = lnPart.ToString();
                                    }
                                }
                                else
                                {
                                    strTitleXX = lnPart.ToString();
                                }
                                intFiles++;
                                swFiles.WriteLine("</body>\n</html>");
                                swFiles.Flush();
                                swFiles.Close();
                                swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                    "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                    "<head>\n" +
                                    "<title>" + strTitle + "</title>\n" +
                                    "</head>\n" +
                                    "<body class=\"book\">");

                                swFiles.WriteLine(strLines[j]);
                                blSplitStart = true;

                                if (strLines[j + 1].StartsWith("<div class=\"pt\"") == true)
                                {
                                    strPUKFileNames[intFiles, 0] = strFileNames;
                                    strPUKFileNames[intFiles, 1] = CreateTitle(strTitleXX, strLines[j + 1], "pn");
                                    strPUKFileNames[intFiles, 2] = strPartLevel;
                                    strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting(strTitleXX, strLines[j + 1], "pn");
                                    j++;
                                    swFiles.WriteLine(strLines[j]);
                                }
                                else
                                {
                                    strPUKFileNames[intFiles, 0] = strFileNames;
                                    strPUKFileNames[intFiles, 1] = CreateTitle(strTitleXX, "", "pn");
                                    strPUKFileNames[intFiles, 2] = strPartLevel;
                                    strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting(strTitleXX, "", "pn");
                                }

                            
                            



                            }
                            else
                            {
                                if (strLines[j].StartsWith("<div class=\"contents\"") == true)
                                {

                                    strFileNames = "contents.xhtml";
                                    strCurrentFile = strFileNames;
                                    
                                    intFiles++;
                                    swFiles.WriteLine("</body>\n</html>");
                                    swFiles.Flush();
                                    swFiles.Close();
                                    swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                    swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                        "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                        "<head>\n" +
                                        "<title>" + strTitle + "</title>\n" +
                                        "</head>\n" +
                                        "<body class=\"book\">");

                                    swFiles.WriteLine(strLines[j]);
                                    blSplitStart = true;

                                        strPUKFileNames[intFiles, 0] = strFileNames;
                                        strPUKFileNames[intFiles, 1] = "Contents";
                                        strPUKFileNames[intFiles, 2] = "0";
                                        strPUKFileNames[intFiles, 3] = "Contents";





                                }
                                else
                                {
                                    if (strLines[j].StartsWith("<div class=\"fm\"") == true)
                                    {
                                        lnFm++;
                                        strFileNames = RemoveTag(strLines[j]).Replace(" ", "").Trim().ToLower();
                                        
                                        if (strFileNames == "")
                                        {
                                            strFileNames = "fm" + lnFm.ToString("000") + ".xhtml";
                                        }
                                        else
                                        {
                                            strFileNames = strFileNames + ".xhtml";
                                        }
                                        strCurrentFile = strFileNames;
                                        intFiles++;
                                        swFiles.WriteLine("</body>\n</html>");
                                        swFiles.Flush();
                                        swFiles.Close();
                                        swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                        swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                            "<head>\n" +
                                            "<title>" + strTitle + "</title>\n" +
                                            "</head>\n" +
                                            "<body class=\"book\">");

                                        swFiles.WriteLine(strLines[j]);
                                        blSplitStart = true;

                                        strPUKFileNames[intFiles, 0] = strFileNames;
                                        strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j]).Trim();
                                        strPUKFileNames[intFiles, 2] = "0";
                                        strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j], "fm");





                                    }
                                    else
                                    {
                                        if (strLines[j].StartsWith("<div class=\"bm\"") == true)
                                        {
                                            lnBm++;
                                            strFileNames = RemoveTag(strLines[j]).Replace(" ", "").Trim().ToLower();
                                            if (strFileNames == "")
                                            {
                                                strFileNames = "bm" + lnBm.ToString("000") + ".xhtml";
                                            }
                                            else
                                            {
                                                strFileNames = strFileNames + ".xhtml";
                                            }
                                            strCurrentFile = strFileNames;

                                            intFiles++;
                                            swFiles.WriteLine("</body>\n</html>");
                                            swFiles.Flush();
                                            swFiles.Close();
                                            swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                            swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                "<head>\n" +
                                                "<title>" + strTitle + "</title>\n" +
                                                "</head>\n" +
                                                "<body class=\"book\">");

                                            swFiles.WriteLine(strLines[j]);
                                            blSplitStart = true;

                                            strPUKFileNames[intFiles, 0] = strFileNames;
                                            strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j]).Trim();
                                            strPUKFileNames[intFiles, 2] = "0";
                                            strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j], "fm");


                                        }
                                        else
                                        {
                                            if (strLines[j].StartsWith("<div class=\"cover\"") == true)
                                            {
                                                strFileNames = "cover.xhtml";
                                                strCurrentFile = strFileNames;

                                                intFiles++;
                                                swFiles.WriteLine("</body>\n</html>");
                                                swFiles.Flush();
                                                swFiles.Close();
                                                swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                    "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                    "<head>\n" +
                                                    "<title>" + strTitle + "</title>\n" +
                                                    "</head>\n" +
                                                    "<body class=\"book\">");

                                                swFiles.WriteLine(strLines[j]);
                                                blSplitStart = true;

                                                strPUKFileNames[intFiles, 0] = strFileNames;
                                                strPUKFileNames[intFiles, 1] = "Cover";
                                                strPUKFileNames[intFiles, 2] = "0";
                                                strPUKFileNames[intFiles, 3] = "Cover";





                                            }
                                            else
                                            {
                                                if (strLines[j].StartsWith("<div class=\"copyright\"") == true)
                                                {
                                                    strFileNames = "copyright.xhtml";
                                                    strCurrentFile = strFileNames;

                                                    intFiles++;
                                                    swFiles.WriteLine("</body>\n</html>");
                                                    swFiles.Flush();
                                                    swFiles.Close();
                                                    swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                    swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                        "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                        "<head>\n" +
                                                        "<title>" + strTitle + "</title>\n" +
                                                        "</head>\n" +
                                                        "<body class=\"book\">");

                                                    swFiles.WriteLine(strLines[j]);
                                                    blSplitStart = true;

                                                    strPUKFileNames[intFiles, 0] = strFileNames;
                                                    strPUKFileNames[intFiles, 1] = "Copyright";
                                                    strPUKFileNames[intFiles, 2] = "0";
                                                    strPUKFileNames[intFiles, 3] = "Copyright";





                                                }
                                                else
                                                {

                                                    if (strLines[j].StartsWith("<div class=\"dedi\"") == true)
                                                    {
                                                        strFileNames = "dedication.xhtml";
                                                        strCurrentFile = strFileNames;

                                                        intFiles++;
                                                        swFiles.WriteLine("</body>\n</html>");
                                                        swFiles.Flush();
                                                        swFiles.Close();
                                                        swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                        swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                            "<head>\n" +
                                                            "<title>" + strTitle + "</title>\n" +
                                                            "</head>\n" +
                                                            "<body class=\"book\">");

                                                        swFiles.WriteLine(strLines[j]);
                                                        blSplitStart = true;

                                                        strPUKFileNames[intFiles, 0] = strFileNames;
                                                        strPUKFileNames[intFiles, 1] = "Dedication";
                                                        strPUKFileNames[intFiles, 2] = "0";
                                                        strPUKFileNames[intFiles, 3] = "Dedication";





                                                    }
                                                    else
                                                    {
                                                        if (strLines[j].StartsWith("<div class=\"epigraph\"") == true)
                                                        {
                                                            strFileNames = "epigraph.xhtml";
                                                            strCurrentFile = strFileNames;

                                                            intFiles++;
                                                            swFiles.WriteLine("</body>\n</html>");
                                                            swFiles.Flush();
                                                            swFiles.Close();
                                                            swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                            swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                "<head>\n" +
                                                                "<title>" + strTitle + "</title>\n" +
                                                                "</head>\n" +
                                                                "<body class=\"book\">");

                                                            swFiles.WriteLine(strLines[j]);
                                                            blSplitStart = true;

                                                            strPUKFileNames[intFiles, 0] = strFileNames;
                                                            strPUKFileNames[intFiles, 1] = "Quote";
                                                            strPUKFileNames[intFiles, 2] = "0";
                                                            strPUKFileNames[intFiles, 3] = "Quote";





                                                        }
                                                        else
                                                        {
                                                            if (strLines[j].StartsWith("<div class=\"index\"") == true)
                                                            {
                                                                strFileNames = "index.xhtml";
                                                                strCurrentFile = strFileNames;

                                                                intFiles++;
                                                                swFiles.WriteLine("</body>\n</html>");
                                                                swFiles.Flush();
                                                                swFiles.Close();
                                                                swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                    "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                    "<head>\n" +
                                                                    "<title>" + strTitle + "</title>\n" +
                                                                    "</head>\n" +
                                                                    "<body class=\"book\">");

                                                                swFiles.WriteLine(strLines[j]);
                                                                blSplitStart = true;

                                                                strPUKFileNames[intFiles, 0] = strFileNames;
                                                                strPUKFileNames[intFiles, 1] = "Index";
                                                                strPUKFileNames[intFiles, 2] = "0";
                                                                strPUKFileNames[intFiles, 3] = "Index";





                                                            }
                                                            else
                                                            {
                                                                if (strLines[j].StartsWith("<div class=\"footnotes\"") == true)
                                                                {
                                                                    strFileNames = "footnotes.xhtml";
                                                                    strCurrentFile = strFileNames;

                                                                    intFiles++;
                                                                    swFiles.WriteLine("</body>\n</html>");
                                                                    swFiles.Flush();
                                                                    swFiles.Close();
                                                                    swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                    swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                        "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                        "<head>\n" +
                                                                        "<title>" + strTitle + "</title>\n" +
                                                                        "</head>\n" +
                                                                        "<body class=\"book\">");

                                                                    swFiles.WriteLine(strLines[j]);
                                                                    blSplitStart = true;

                                                                    strPUKFileNames[intFiles, 0] = strFileNames;
                                                                    strPUKFileNames[intFiles, 1] = "Footnotes";
                                                                    strPUKFileNames[intFiles, 2] = "0";
                                                                    strPUKFileNames[intFiles, 3] = "Footnotes";





                                                                }
                                                                else
                                                                {

                                                                    if (strLines[j].StartsWith("<div class=\"praise\"") == true)
                                                                    {

                                                                        strFileNames = "praise.xhtml";
                                                                        strCurrentFile = strFileNames;

                                                                        intFiles++;
                                                                        swFiles.WriteLine("</body>\n</html>");
                                                                        swFiles.Flush();
                                                                        swFiles.Close();
                                                                        swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                        swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                            "<head>\n" +
                                                                            "<title>" + strTitle + "</title>\n" +
                                                                            "</head>\n" +
                                                                            "<body class=\"book\">");

                                                                        swFiles.WriteLine(strLines[j]);
                                                                        blSplitStart = true;

                                                                        strPUKFileNames[intFiles, 0] = strFileNames;
                                                                        strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j]).Trim();
                                                                        strPUKFileNames[intFiles, 2] = "0";
                                                                        strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j], "fm");




                                                                    }
                                                                    else
                                                                    {
                                                                        if (strLines[j].StartsWith("<div class=\"alsoby\"") == true)
                                                                        {

                                                                            strFileNames = "alsoby.xhtml";

                                                                            strCurrentFile = strFileNames;
                                                                            intFiles++;
                                                                            swFiles.WriteLine("</body>\n</html>");
                                                                            swFiles.Flush();
                                                                            swFiles.Close();
                                                                            swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                            swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                                "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                                "<head>\n" +
                                                                                "<title>" + strTitle + "</title>\n" +
                                                                                "</head>\n" +
                                                                                "<body class=\"book\">");

                                                                            swFiles.WriteLine(strLines[j]);
                                                                            blSplitStart = true;

                                                                            strPUKFileNames[intFiles, 0] = strFileNames;
                                                                            strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j]).Trim();
                                                                            strPUKFileNames[intFiles, 2] = "0";
                                                                            strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j], "fm");




                                                                        }
                                                                        else
                                                                        {
                                                                            if (strLines[j].StartsWith("<div class=\"aboutauthor\"") == true)
                                                                            {

                                                                                strFileNames = "aboutauthor.xhtml";

                                                                                strCurrentFile = strFileNames;
                                                                                intFiles++;
                                                                                swFiles.WriteLine("</body>\n</html>");
                                                                                swFiles.Flush();
                                                                                swFiles.Close();
                                                                                swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                                swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                                    "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                                    "<head>\n" +
                                                                                    "<title>" + strTitle + "</title>\n" +
                                                                                    "</head>\n" +
                                                                                    "<body class=\"book\">");

                                                                                swFiles.WriteLine(strLines[j]);
                                                                                blSplitStart = true;

                                                                                strPUKFileNames[intFiles, 0] = strFileNames;
                                                                                strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j]).Trim();
                                                                                strPUKFileNames[intFiles, 2] = "0";
                                                                                strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j], "fm");




                                                                            }
                                                                            else
                                                                            {
                                                                                if (strLines[j].StartsWith("<div class=\"appendix\"") == true)
                                                                                {
                                                                                    lnAppe++;
                                                                                    strFileNames = "appendix" + lnAppe.ToString("000") + ".xhtml";
                                                                                    strCurrentFile = strFileNames;

                                                                                    intFiles++;
                                                                                    swFiles.WriteLine("</body>\n</html>");
                                                                                    swFiles.Flush();
                                                                                    swFiles.Close();
                                                                                    swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                                    swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                                        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                                        "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                                        "<head>\n" +
                                                                                        "<title>" + strTitle + "</title>\n" +
                                                                                        "</head>\n" +
                                                                                        "<body class=\"book\">");

                                                                                    swFiles.WriteLine(strLines[j]);
                                                                                    blSplitStart = true;

                                                                                    strPUKFileNames[intFiles, 0] = strFileNames;
                                                                                    strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j]).Trim();
                                                                                    strPUKFileNames[intFiles, 2] = "0";
                                                                                    strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j], "fm");




                                                                                }
                                                                                else
                                                                                {
                                                                                    if (strLines[j].StartsWith("<div class=\"title\"") == true)
                                                                                    {
                                                                                        
                                                                                        strFileNames = "title.xhtml";
                                                                                        strCurrentFile = strFileNames;

                                                                                        intFiles++;
                                                                                        swFiles.WriteLine("</body>\n</html>");
                                                                                        swFiles.Flush();
                                                                                        swFiles.Close();
                                                                                        swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                                        swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                                            "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                                            "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                                            "<head>\n" +
                                                                                            "<title>" + strTitle + "</title>\n" +
                                                                                            "</head>\n" +
                                                                                            "<body class=\"book\">");

                                                                                        swFiles.WriteLine(strLines[j]);
                                                                                        blSplitStart = true;

                                                                                        strPUKFileNames[intFiles, 0] = strFileNames;
                                                                                        strPUKFileNames[intFiles, 1] = strTitle;
                                                                                        strPUKFileNames[intFiles, 2] = "0";
                                                                                        strPUKFileNames[intFiles, 3] = strTitle; 




                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        if (strLines[j].StartsWith("<div class=\"pt\"") == true)
                                                                                        {

                                                                                            lnPart++;

                                                                                            strFileNames = "part" + lnPart.ToString("000") + ".xhtml";
                                                                                            strCurrentFile = strFileNames;
                                                                                            strTitleXX = RemoveTag(strLines[j]).Trim();
                                                                                            if (strTitleXX == "")
                                                                                            {
                                                                                                strTitleXX = "PART " + lnPart.ToString();
                                                                                            }
                                                                                           
                                                                                            intFiles++;
                                                                                            swFiles.WriteLine("</body>\n</html>");
                                                                                            swFiles.Flush();
                                                                                            swFiles.Close();
                                                                                            swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                                            swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                                                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                                                "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                                                "<head>\n" +
                                                                                                "<title>" + strTitle + "</title>\n" +
                                                                                                "</head>\n" +
                                                                                                "<body class=\"book\">");

                                                                                            swFiles.WriteLine(strLines[j]);
                                                                                            blSplitStart = true;

                                                                                            
                                                                                                strPUKFileNames[intFiles, 0] = strFileNames;
                                                                                                strPUKFileNames[intFiles, 1] = strTitleXX;
                                                                                                strPUKFileNames[intFiles, 2] = strPartLevel;
                                                                                                strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j], "fm");
                                                                                            
                            



                                                                                        }
                                                                                        else
                                                                                        {

                                                                                            if (strLines[j].StartsWith("<div class=\"ct\"") == true)
                                                                                            {

                                                                                                lnChapter++;

                                                                                                strFileNames = "chapter" + lnChapter.ToString("000") + ".xhtml";
                                                                                                strCurrentFile = strFileNames;
                                                                                                strTitleXX = RemoveTag(strLines[j]).Trim();
                                                                                                if (strTitleXX == "")
                                                                                                {
                                                                                                    strTitleXX = "Chapter " + lnChapter.ToString();
                                                                                                }

                                                                                                intFiles++;
                                                                                                swFiles.WriteLine("</body>\n</html>");
                                                                                                swFiles.Flush();
                                                                                                swFiles.Close();
                                                                                                swFiles = new StreamWriter(strSavePath + "\\" + strFileNames);
                                                                                                swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                                                                                    "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                                                                                    "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                                                                                    "<head>\n" +
                                                                                                    "<title>" + strTitle + "</title>\n" +
                                                                                                    "</head>\n" +
                                                                                                    "<body class=\"book\">");

                                                                                                swFiles.WriteLine(strLines[j]);
                                                                                                blSplitStart = true;


                                                                                                strPUKFileNames[intFiles, 0] = strFileNames;
                                                                                                strPUKFileNames[intFiles, 1] = strTitleXX;
                                                                                                strPUKFileNames[intFiles, 2] = strChapterLevel;
                                                                                                strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j], "fm");





                                                                                            }
                                                                                            else
                                                                                            {




                                                                                                if (strLines[j].StartsWith("<div class=\"sn\"") == true)
                                                                                                {
                                                                                                    intFiles++;
                                                                                                    lnSections++;

                                                                                                    if (strLines[j] != "")
                                                                                                    {
                                                                                                        //Insert <link>
                                                                                                        swFiles.WriteLine("<a name=\"sect" + lnSections.ToString("0000") +"\"/>" + strLines[j]);
                                                                                                    }


                                                                                                    j++;

                                                                                                    if (strLines[j].StartsWith("<div class=\"st\"") == true)
                                                                                                    {
                                                                                                        swFiles.WriteLine(strLines[j]);
                                                                                                        strPUKFileNames[intFiles, 0] = strCurrentFile + "#sect" + lnSections.ToString("0000");
                                                                                                        strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j - 1]).Trim() + ": " + RemoveTag(strLines[j]).Trim();
                                                                                                        strPUKFileNames[intFiles, 2] = strsectionLevel;
                                                                                                        strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting(strLines[j - 1].Trim(), strLines[j], "fm");
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        strPUKFileNames[intFiles, 0] = strCurrentFile + "#sect" + lnSections.ToString("0000");
                                                                                                        strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j - 1]);
                                                                                                        strPUKFileNames[intFiles, 2] = strsectionLevel;
                                                                                                        strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j - 1].Trim(), "fm");
                                                                                                        j--;
                                                                                                    }

                                                                                                }
                                                                                                else
                                                                                                {

                                                                                                    if (strLines[j].StartsWith("<div class=\"st\"") == true)
                                                                                                    {
                                                                                                        intFiles++;
                                                                                                        lnSections++;
                                                                                                        if (strLines[j] != "")
                                                                                                        {
                                                                                                            swFiles.WriteLine("<a name=\"sect" + lnSections.ToString("0000") + "\"/>" + strLines[j]);
                                                                                                        }

                                                                                                        strPUKFileNames[intFiles, 0] = strCurrentFile + "#sect" + lnSections.ToString("0000");
                                                                                                        strPUKFileNames[intFiles, 1] = RemoveTag(strLines[j]).Trim();
                                                                                                        strPUKFileNames[intFiles, 2] = strsectionLevel;
                                                                                                        strPUKFileNames[intFiles, 3] = CreateTitleWithOutFormatting("", strLines[j].Trim(), "fm");

                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        if (strLines[j] != "")
                                                                                                        {
                                                                                                            swFiles.WriteLine(strLines[j]);
                                                                                                        }


                                                                                                    }

                                                                                                }

                                                                                            }

                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }

                     


                        


                        toolStripProgressBar1.Value = j + 1;

                    }

                    swFiles.Flush();
                    swFiles.Close();


                    this.Refresh();

                    rtbContent.Text = string.Join("\n", strLines);
                    toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                    toolStripStatusLabel1.Text = "Deleting Temp Files... Please Wait";

                    FileInfo fl = new FileInfo(strSavePath + "\\tmpK01x.del");
                    fl.Delete();
                    toolStripStatusLabel1.Text = "Ready";
                    Application.UseWaitCursor = false;
                }
                catch
                {
                    MessageBox.Show("Unexpected Error", "ERR", MessageBoxButtons.OK);

                }
            }

            clsStaticVrs.setBookTitle(strTitle);  
            clsStaticVrs.setFileNames(strPUKFileNames);
            clsStaticVrs.setFolderNameSaved(strSavePath);  
            frmSplitter frmsp = new frmSplitter();
            frmsp.ShowDialog();
        }

        private string CreateTitleWithOutFormatting(string strLn1, string strLn2, string strType)
        {
            string strTitle = "";
            strLn1 = strLn1.Trim();
            strLn2 = strLn2.Trim();
            switch (strType)
            {
                case "cn":
                    {
                        strLn2 = Regex.Replace(strLn2, "^<div class=\"([^ ]+)\">(.*)</div>$", "$2");
                        if (strLn2 == "")
                        {
                            strTitle = strLn1;
                        }
                        else
                        {
                            strTitle = strLn1 + ". " + strLn2;
                        }
                    }
                    break;
                case "pn":
                    {
                        strLn2 = Regex.Replace(strLn2, "^<div class=\"([^ ]+)\">(.*)</div>$", "$2");
                        if (strLn2 == "")
                        {
                            strTitle = strLn1;
                        }
                        else
                        {
                            strTitle = strLn1 + ": " + strLn2;
                        }
                        //strTitle = strTitle.ToUpper(); 
                    }
                    break;

                default:
                    {

                        strLn2 = Regex.Replace(strLn2, "^<div class=\"([^ ]+)\">(.*)</div>$", "$2");
                        strTitle = strLn2;
                       
                    
                    }

                    break;
            }
            return strTitle;
        }




        private string CreateTitle(string strLn1, string strLn2, string strType)
        {
            string strTitle = "";
            strLn1 = RemoveTag(strLn1).Trim();
            strLn2 = RemoveTag(strLn2).Trim();
            switch (strType)
            {
                case "cn":
                    {
                        strLn2 = RemoveTag(strLn2).Trim();
                        if (strLn2 == "")
                        {
                            strTitle = strLn1;
                        }
                        else
                        {
                            strTitle = strLn1 + ". " + strLn2;
                        }
                    }
                    break;
                case "pn":
                    {
                        strLn2 = RemoveTag(strLn2).Trim();
                        if (strLn2 == "")
                        {
                            strTitle = strLn1;
                        }
                        else
                        {
                            strTitle = strLn1 + ": " + strLn2;
                        }
                        //strTitle = strTitle.ToUpper(); 
                    }
                    break;
               


                default:
                    break;
            }
            return strTitle;
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            createNCX();
        }

        private string RemoveTag(string strLn)
        {
            strLn = Regex.Replace(strLn, "<([^>]*)>", " ");
            strLn = Regex.Replace(strLn, "([ ]+)", " ");
            return strLn;
        }


        private void createNCX()
        {

            if (fbdSplit.ShowDialog() == DialogResult.OK)
            {

                string strSavePath = ""; fbdSplit.SelectedPath.ToString();
                strSavePath = fbdSplit.SelectedPath.ToString();

                if (strSavePath.Length > 2)
                {

                    try
                    {

                        Application.UseWaitCursor = true;
                        toolStripStatusLabel1.Text = "Creating .NCX File... Please Wait";
                        this.Refresh();
                        string strContent = "";

                        string[] strLines;

                        strContent = rtbContent.Text;
                        strLines = strContent.Split('\n');
                        long i = strLines.Length;

                        toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
                        toolStripProgressBar1.Minimum = 1;
                        toolStripProgressBar1.Value = 1;
                        this.Refresh();



                        StreamWriter swFiles;
                        string strFileNames = "";
                        string strChapterTitle = "";
                        bool blSplitStart = false;
                        bool blIdFound = false;
                        bool blSrcFound = false;
                        bool blTitleFound = false;
                        bool blATitleFound = false;
                        string strWrite = "";

                        string strIdFound = "";
                        string strSrcFound = "";
                        string strTitleFound = "";
                        string strATitleFound = "";
                        long lnChapterNumber = 1;


                        swFiles = new StreamWriter(strSavePath + "\\toc.ncx");

                        swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                "<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">\n" +
                                "  <head>\n" +
                                "    <meta name=\"dtb:uid\" content=\"isbn:*****\"/>\n" +
                                "    <meta name=\"dtb:depth\" content=\"1\"/>\n" +
                                "    <meta name=\"dtb:totalPageCount\" content=\"0\"/>\n" +
                                "    <meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n" +
                                "  </head>\n" +
                                "  <docTitle>\n" +
                                "    <text>***Book Name***</text>\n" +
                                "  </docTitle>\n" +
                                "<docAuthor>\n" +
                                "     <text>***Author Name***</text>\n" +
                                "</docAuthor>\n" +
                                "  <navMap>");

                        for (int j = 0; j < i; j++)
                        {




                            if (strLines[j].StartsWith("<split"))
                            {
                                strFileNames = Regex.Replace(strLines[j], "^<split filename=\"([^<>]+)\">$", "$1");
                                blSplitStart = true;
                                //swFiles.WriteLine("      <content src=\"" + strFileNames + "\"/>");
                                blSrcFound = true;
                                strSrcFound = "      <content src=\"" + strFileNames + "\"/>";

                            }

                            if (strLines[j].StartsWith("<head>") == true)
                            {
                                j++;
                                if (strLines[j].StartsWith("<title>") == true)
                                {
                                    strChapterTitle = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                                    //swFiles.WriteLine("        <text>" + strChapterTitle + "</text>");
                                    blTitleFound = true;
                                    strTitleFound = "      <navLabel>\n        <text>" + strChapterTitle + "</text>\n      </navLabel>";
                                }

                            }

                            if (strLines[j].StartsWith("<h2 class=\"chaptertitle\">") == true)
                            {

                                strChapterTitle = Regex.Replace(strLines[j], "^<h2 class=\"chaptertitle\">(.*)</h2>$", "$1");
                                strChapterTitle = RemoveTag(strChapterTitle);
                                blATitleFound = true;
                                strATitleFound = "      <navLabel>\n        <text>" + strChapterTitle + "</text>\n      </navLabel>";


                            }



                            if (strLines[j].StartsWith("<div>") == true)
                            {
                                j++;
                                if (strLines[j].StartsWith("<a id=") == true)
                                {
                                    strChapterTitle = Regex.Replace(strLines[j], "^<a id=\"([^<]*)\"></a>(.*)$", "$1");
                                    //swFiles.WriteLine("    <navPoint class=\"chapter\" id=\"" + strChapterTitle + "\" playOrder=\"1\">");
                                    blIdFound = true;
                                    strIdFound = "    <navPoint class=\"chapter\" id=\"" + strChapterTitle + "\" playOrder=\"" + lnChapterNumber.ToString() + "\">";
                                    lnChapterNumber++;
                                }

                            }





                            if (strLines[j].StartsWith("</split"))
                            {
                                //MessageBox.Show("Yes");  
                                strWrite = "";
                                if (blIdFound == true)
                                {
                                    strWrite = strIdFound;
                                    if (blATitleFound == true)
                                    {
                                        strWrite = strWrite + "\n" + strATitleFound;
                                    }
                                    else
                                    {
                                        if (blTitleFound == true)
                                        {
                                            strWrite = strWrite + "\n" + strTitleFound;
                                        }


                                    }
                                    if (blSrcFound == true)
                                    {
                                        strWrite = strWrite + "\n" + strSrcFound;
                                    }

                                    strWrite = strWrite + "\n    </navPoint>";

                                    swFiles.WriteLine(strWrite);
                                    // MessageBox.Show("In");
                                }



                                blIdFound = false;
                                blSrcFound = false;
                                blTitleFound = false;
                                blATitleFound = false;

                                strIdFound = "";
                                strSrcFound = "";
                                strTitleFound = "";
                                strATitleFound = "";

                                blSplitStart = false;
                            }



                            toolStripProgressBar1.Value = j + 1;

                        }

                        swFiles.WriteLine("  </navMap>\n</ncx>");

                        swFiles.Flush();
                        swFiles.Close();


                        this.Refresh();

                        rtbContent.Text = string.Join("\n", strLines);
                        toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                        toolStripStatusLabel1.Text = "Ready";
                        Application.UseWaitCursor = false;
                    }
                    catch
                    {
                        MessageBox.Show("Unexpected Error", "ERR", MessageBoxButtons.OK);

                    }
                }
            }

        }

        private void createNCXAuto(string strPath)
        {

            //fbdSplit.ShowDialog();
            string strSavePath = strPath; //fbdSplit.SelectedPath.ToString();


            if (strSavePath.Length > 2)
            {

                try
                {

                    Application.UseWaitCursor = true;
                    toolStripStatusLabel1.Text = "Creating .NCX File... Please Wait";
                    this.Refresh();
                    string strContent = "";

                    string[] strLines;

                    strContent = rtbContent.Text;
                    strLines = strContent.Split('\n');
                    long i = strLines.Length;

                    toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
                    toolStripProgressBar1.Minimum = 1;
                    toolStripProgressBar1.Value = 1;
                    this.Refresh();



                    StreamWriter swFiles;
                    string strFileNames = "";
                    string strChapterTitle = "";
                    bool blSplitStart = false;
                    bool blIdFound = false;
                    bool blSrcFound = false;
                    bool blTitleFound = false;
                    bool blATitleFound = false;
                    string strWrite = "";

                    string strIdFound = "";
                    string strSrcFound = "";
                    string strTitleFound = "";
                    string strATitleFound = "";
                    long lnChapterNumber = 1;


                    swFiles = new StreamWriter(strSavePath + "\\toc.ncx");

                    swFiles.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\">\n" +
                            "  <head>\n" +
                            "    <meta name=\"dtb:uid\" content=\"isbn:*****\"/>\n" +
                            "    <meta name=\"dtb:depth\" content=\"1\"/>\n" +
                            "    <meta name=\"dtb:totalPageCount\" content=\"0\"/>\n" +
                            "    <meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n" +
                            "  </head>\n" +
                            "  <docTitle>\n" +
                            "    <text>***Book Name***</text>\n" +
                            "  </docTitle>\n" +
                            "<docAuthor>\n" +
                            "     <text>***Author Name***</text>\n" +
                            "</docAuthor>\n" +
                            "  <navMap>");

                    for (int j = 0; j < i; j++)
                    {




                        if (strLines[j].StartsWith("<split"))
                        {
                            strFileNames = Regex.Replace(strLines[j], "^<split filename=\"([^<>]+)\">$", "$1");
                            blSplitStart = true;
                            //swFiles.WriteLine("      <content src=\"" + strFileNames + "\"/>");
                            blSrcFound = true;
                            strSrcFound = "      <content src=\"" + strFileNames + "\"/>";

                        }

                        if (strLines[j].StartsWith("<head>") == true)
                        {
                            j++;
                            if (strLines[j].StartsWith("<title>") == true)
                            {
                                strChapterTitle = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                                //swFiles.WriteLine("        <text>" + strChapterTitle + "</text>");
                                blTitleFound = true;
                                strTitleFound = "      <navLabel>\n        <text>" + strChapterTitle + "</text>\n      </navLabel>";
                            }

                        }

                        if (strLines[j].StartsWith("<h2 class=\"chaptertitle\">") == true)
                        {

                            strChapterTitle = Regex.Replace(strLines[j], "^<h2 class=\"chaptertitle\">(.*)</h2>$", "$1");
                            strChapterTitle = RemoveTag(strChapterTitle);
                            blATitleFound = true;
                            strATitleFound = "      <navLabel>\n        <text>" + strChapterTitle + "</text>\n      </navLabel>";


                        }



                        if (strLines[j].StartsWith("<div>") == true)
                        {
                            j++;
                            if (strLines[j].StartsWith("<a id=") == true)
                            {
                                strChapterTitle = Regex.Replace(strLines[j], "^<a id=\"([^<]*)\"></a>(.*)$", "$1");
                                //swFiles.WriteLine("    <navPoint class=\"chapter\" id=\"" + strChapterTitle + "\" playOrder=\"1\">");
                                blIdFound = true;
                                strIdFound = "    <navPoint class=\"chapter\" id=\"" + strChapterTitle + "\" playOrder=\"" + lnChapterNumber.ToString() + "\">";
                                lnChapterNumber++;
                            }

                        }





                        if (strLines[j].StartsWith("</split"))
                        {
                            //MessageBox.Show("Yes");  
                            strWrite = "";
                            if (blIdFound == true)
                            {
                                strWrite = strIdFound;
                                if (blATitleFound == true)
                                {
                                    strWrite = strWrite + "\n" + strATitleFound;
                                }
                                else
                                {
                                    if (blTitleFound == true)
                                    {
                                        strWrite = strWrite + "\n" + strTitleFound;
                                    }


                                }
                                if (blSrcFound == true)
                                {
                                    strWrite = strWrite + "\n" + strSrcFound;
                                }

                                strWrite = strWrite + "\n    </navPoint>";

                                swFiles.WriteLine(strWrite);
                                // MessageBox.Show("In");
                            }



                            blIdFound = false;
                            blSrcFound = false;
                            blTitleFound = false;
                            blATitleFound = false;

                            strIdFound = "";
                            strSrcFound = "";
                            strTitleFound = "";
                            strATitleFound = "";

                            blSplitStart = false;
                        }



                        toolStripProgressBar1.Value = j + 1;

                    }

                    swFiles.WriteLine("  </navMap>\n</ncx>");

                    swFiles.Flush();
                    swFiles.Close();


                    this.Refresh();

                    rtbContent.Text = string.Join("\n", strLines);
                    toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                    toolStripStatusLabel1.Text = "Ready";
                    Application.UseWaitCursor = false;
                }
                catch
                {
                    MessageBox.Show("Unexpected Error", "ERR", MessageBoxButtons.OK);

                }
            }


        }

        private void createOPF()
        {

            if (fbdSplit.ShowDialog() == DialogResult.OK)
            {
                string strSavePath = fbdSplit.SelectedPath.ToString();
                System.Collections.Stack stkImgs;
                stkImgs = new System.Collections.Stack();
                stkImgs.Clear();
                MatchCollection mc;


                if (strSavePath.Length > 2)
                {

                    try
                    {

                        Application.UseWaitCursor = true;
                        toolStripStatusLabel1.Text = "Creating .OPF File... Please Wait";
                        this.Refresh();
                        string strContent = "";

                        string[] strLines;

                        strContent = rtbContent.Text;
                        strLines = strContent.Split('\n');
                        long i = strLines.Length;

                        toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
                        toolStripProgressBar1.Minimum = 1;
                        toolStripProgressBar1.Value = 1;
                        this.Refresh();



                        StreamWriter swFiles;
                        string strFileNames = "";
                        string strChapterTitle = "";
                        bool blSplitStart = false;
                        bool blIdFound = false;
                        bool blSrcFound = false;
                        bool blTitleFound = false;
                        bool blATitleFound = false;
                        string strWrite = "";

                        string strIdFound = "";
                        string strSrcFound = "";
                        string strTitleFound = "";
                        string strATitleFound = "";
                        long lnImgIDCount = 1;


                        swFiles = new StreamWriter(strSavePath + "\\content.opf");

                        swFiles.WriteLine("<?xml version=\"1.0\"?>\n" +
                            "<package version=\"2.0\" xmlns=\"http://www.idpf.org/2007/opf\"\n" +
                            "         unique-identifier=\"isbn\">\n" +
                            " <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\"\n" +
                            "           xmlns:opf=\"http://www.idpf.org/2007/opf\">\n" +
                            "   <dc:title>***Book Name***</dc:title> \n" +
                            "   <dc:creator>***Author Name***</dc:creator>\n" +
                            "   <dc:language>en-US</dc:language> \n" +
                            "   <dc:rights>***Copyright***</dc:rights>\n" +
                            "   <dc:publisher>***Publisher***</dc:publisher>\n" +
                            "   <dc:identifier id=\"isbn\">****</dc:identifier>\n" +
                            "   <meta name=\"cover\" content=\"cover-image\"/>  \n" +
                            " </metadata>\n" +
                            " <manifest>\n" +
                            "\n" +
                            "<!-- Images -->\n");

                        for (int j = 0; j < i; j++)
                        {

                            mc = Regex.Matches(strLines[j], "<img src=\"([^\"]+)\"");

                            foreach (Match singleMc in mc)
                            {

                                if (stkImgs.Contains(singleMc.Result("$1")) == false)
                                {
                                    stkImgs.Push(singleMc.Result("$1"));
                                    swFiles.WriteLine("  <item href=\"" + singleMc.Result("$1") + "\" id=\"img_" + lnImgIDCount.ToString() + "\" media-type=\"image/jpeg\"/>");
                                    lnImgIDCount++;
                                }


                            }


                            toolStripProgressBar1.Value = j + 1;

                        }

                        swFiles.WriteLine("<!-- NCX -->\n" +
                            "\n" +
                            "<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>\n" +
                            "\n" +
                            " <!-- CSS Style Sheets -->\n" +
                            "\n" +
                            "<item id=\"style_bv\" href=\"bv_ebook_style.css\" media-type=\"text/css\"/>\n" +
                            "<item id=\"style_basic\" href=\"stylesheet.css\" media-type=\"text/css\"/>\n" +
                            "<item id=\"pagetemplate\" href=\"page-template.xpgt\" media-type=\"application/vnd.adobe-page-template+xml\"/>\n" +
                            "<!-- Content Documents -->\n" +
                            "\n");







                        string strIDRef = " <spine toc=\"ncx\">";



                        for (int j = 0; j < i; j++)
                        {




                            if (strLines[j].StartsWith("<split"))
                            {
                                strFileNames = Regex.Replace(strLines[j], "^<split filename=\"([^<>]+)\">$", "$1");
                                blSplitStart = true;
                                //swFiles.WriteLine("      <content src=\"" + strFileNames + "\"/>");
                                blSrcFound = true;
                                strSrcFound = strFileNames;

                            }



                            if (strLines[j].StartsWith("<div>") == true)
                            {
                                j++;
                                if (strLines[j].StartsWith("<a id=") == true)
                                {
                                    strChapterTitle = Regex.Replace(strLines[j], "^<a id=\"([^<]*)\"></a>(.*)$", "$1");
                                    //swFiles.WriteLine("    <navPoint class=\"chapter\" id=\"" + strChapterTitle + "\" playOrder=\"1\">");
                                    blIdFound = true;
                                    strIdFound = strChapterTitle;

                                }

                            }





                            if (strLines[j].StartsWith("</split"))
                            {
                                strWrite = "";
                                if (blIdFound == true)
                                {

                                    if (blSrcFound == true)
                                    {
                                        strWrite = "  <item id=\"" + strIdFound + "\" href=\"" + strSrcFound + "\" media-type=\"application/xhtml+xml\"/>";
                                        swFiles.WriteLine(strWrite);

                                        strIDRef = strIDRef + "\n" + "  <itemref idref=\"" + strIdFound + "\" linear=\"yes\" />";

                                    }


                                }



                                blIdFound = false;
                                blSrcFound = false;
                                blTitleFound = false;
                                blATitleFound = false;

                                strIdFound = "";
                                strSrcFound = "";
                                strTitleFound = "";
                                strATitleFound = "";

                                blSplitStart = false;
                            }



                            toolStripProgressBar1.Value = j + 1;

                        }

                        swFiles.WriteLine("  </manifest>\n");

                        swFiles.WriteLine(strIDRef);

                        swFiles.WriteLine("<guide>");

                        for (int j = 0; j < i; j++)
                        {




                            if (strLines[j].StartsWith("<split"))
                            {
                                strFileNames = Regex.Replace(strLines[j], "^<split filename=\"([^<>]+)\">$", "$1");
                                blSplitStart = true;
                                //swFiles.WriteLine("      <content src=\"" + strFileNames + "\"/>");
                                blSrcFound = true;
                                strSrcFound = strFileNames;

                            }

                            if (strLines[j].StartsWith("<head>") == true)
                            {
                                j++;
                                if (strLines[j].StartsWith("<title>") == true)
                                {
                                    strChapterTitle = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                                    //swFiles.WriteLine("        <text>" + strChapterTitle + "</text>");
                                    blTitleFound = true;
                                    strTitleFound = strChapterTitle;
                                }

                            }

                            if (strLines[j].StartsWith("<h2 class=\"chaptertitle\">") == true)
                            {

                                strChapterTitle = Regex.Replace(strLines[j], "^<h2 class=\"chaptertitle\">(.*)</h2>$", "$1");
                                strChapterTitle = RemoveTag(strChapterTitle);
                                blATitleFound = true;
                                strATitleFound = strChapterTitle;


                            }



                            if (strLines[j].StartsWith("<div>") == true)
                            {
                                j++;
                                if (strLines[j].StartsWith("<a id=") == true)
                                {
                                    strChapterTitle = Regex.Replace(strLines[j], "^<a id=\"([^<]*)\"></a>(.*)$", "$1");
                                    //swFiles.WriteLine("    <navPoint class=\"chapter\" id=\"" + strChapterTitle + "\" playOrder=\"1\">");
                                    blIdFound = true;
                                    strIdFound = strChapterTitle;

                                }

                            }





                            if (strLines[j].StartsWith("</split"))
                            {
                                strWrite = "";
                                if (blIdFound == true)
                                {
                                    strWrite = strIdFound;
                                    if (blATitleFound == true)
                                    {
                                        //strATitleFound
                                    }
                                    else
                                    {
                                        if (blTitleFound == true)
                                        {
                                            strATitleFound = strTitleFound;
                                        }


                                    }

                                    strWrite = "<reference type=\"text\"\n" +
                                    "		   title=\"" + strATitleFound + "\"\n" +
                                    "          href=\"" + strSrcFound + "\"/>";

                                    swFiles.WriteLine(strWrite);
                                }



                                blIdFound = false;
                                blSrcFound = false;
                                blTitleFound = false;
                                blATitleFound = false;

                                strIdFound = "";
                                strSrcFound = "";
                                strTitleFound = "";
                                strATitleFound = "";

                                blSplitStart = false;
                            }



                            toolStripProgressBar1.Value = j + 1;

                        }


















                        swFiles.WriteLine("</guide>\n</package>");

                        swFiles.Flush();
                        swFiles.Close();


                        this.Refresh();

                        rtbContent.Text = string.Join("\n", strLines);
                        toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                        toolStripStatusLabel1.Text = "Ready";
                        Application.UseWaitCursor = false;
                    }
                    catch
                    {
                        MessageBox.Show("Unexpected Error", "ERR", MessageBoxButtons.OK);

                    }
                }

            }
        }

        private void createOPFAuto(string strPath)
        {

            //fbdSplit.ShowDialog();
            string strSavePath = strPath;
            System.Collections.Stack stkImgs;
            stkImgs = new System.Collections.Stack();
            stkImgs.Clear();
            MatchCollection mc;


            if (strSavePath.Length > 2)
            {

                try
                {

                    Application.UseWaitCursor = true;
                    toolStripStatusLabel1.Text = "Creating .OPF File... Please Wait";
                    this.Refresh();
                    string strContent = "";

                    string[] strLines;

                    strContent = rtbContent.Text;
                    strLines = strContent.Split('\n');
                    long i = strLines.Length;

                    toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
                    toolStripProgressBar1.Minimum = 1;
                    toolStripProgressBar1.Value = 1;
                    this.Refresh();



                    StreamWriter swFiles;
                    string strFileNames = "";
                    string strChapterTitle = "";
                    bool blSplitStart = false;
                    bool blIdFound = false;
                    bool blSrcFound = false;
                    bool blTitleFound = false;
                    bool blATitleFound = false;
                    string strWrite = "";

                    string strIdFound = "";
                    string strSrcFound = "";
                    string strTitleFound = "";
                    string strATitleFound = "";
                    long lnImgIDCount = 1;


                    swFiles = new StreamWriter(strSavePath + "\\content.opf");

                    swFiles.WriteLine("<?xml version=\"1.0\"?>\n" +
                        "<package version=\"2.0\" xmlns=\"http://www.idpf.org/2007/opf\"\n" +
                        "         unique-identifier=\"isbn\">\n" +
                        " <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\"\n" +
                        "           xmlns:opf=\"http://www.idpf.org/2007/opf\">\n" +
                        "   <dc:title>***Book Name***</dc:title> \n" +
                        "   <dc:creator>***Author Name***</dc:creator>\n" +
                        "   <dc:language>en-US</dc:language> \n" +
                        "   <dc:rights>***Copyright***</dc:rights>\n" +
                        "   <dc:publisher>***Publisher***</dc:publisher>\n" +
                        "   <dc:identifier id=\"isbn\">****</dc:identifier>\n" +
                        "   <meta name=\"cover\" content=\"cover-image\"/>  \n" +
                        " </metadata>\n" +
                        " <manifest>\n" +
                        "\n" +
                        "<!-- Images -->\n");

                    for (int j = 0; j < i; j++)
                    {

                        mc = Regex.Matches(strLines[j], "<img src=\"([^\"]+)\"");

                        foreach (Match singleMc in mc)
                        {

                            if (stkImgs.Contains(singleMc.Result("$1")) == false)
                            {
                                stkImgs.Push(singleMc.Result("$1"));
                                swFiles.WriteLine("  <item href=\"" + singleMc.Result("$1") + "\" id=\"img_" + lnImgIDCount.ToString() + "\" media-type=\"image/jpeg\"/>");
                                lnImgIDCount++;
                            }


                        }


                        toolStripProgressBar1.Value = j + 1;

                    }

                    swFiles.WriteLine("<!-- NCX -->\n" +
                        "\n" +
                        "<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>\n" +
                        "\n" +
                        " <!-- CSS Style Sheets -->\n" +
                        "\n" +
                        "<item id=\"style_bv\" href=\"bv_ebook_style.css\" media-type=\"text/css\"/>\n" +
                        "<item id=\"style_basic\" href=\"stylesheet.css\" media-type=\"text/css\"/>\n" +
                        "<item id=\"pagetemplate\" href=\"page-template.xpgt\" media-type=\"application/vnd.adobe-page-template+xml\"/>\n" +
                        "<!-- Content Documents -->\n" +
                        "\n");







                    string strIDRef = " <spine toc=\"ncx\">";



                    for (int j = 0; j < i; j++)
                    {




                        if (strLines[j].StartsWith("<split"))
                        {
                            strFileNames = Regex.Replace(strLines[j], "^<split filename=\"([^<>]+)\">$", "$1");
                            blSplitStart = true;
                            //swFiles.WriteLine("      <content src=\"" + strFileNames + "\"/>");
                            blSrcFound = true;
                            strSrcFound = strFileNames;

                        }



                        if (strLines[j].StartsWith("<div>") == true)
                        {
                            j++;
                            if (strLines[j].StartsWith("<a id=") == true)
                            {
                                strChapterTitle = Regex.Replace(strLines[j], "^<a id=\"([^<]*)\"></a>(.*)$", "$1");
                                //swFiles.WriteLine("    <navPoint class=\"chapter\" id=\"" + strChapterTitle + "\" playOrder=\"1\">");
                                blIdFound = true;
                                strIdFound = strChapterTitle;

                            }

                        }





                        if (strLines[j].StartsWith("</split"))
                        {
                            strWrite = "";
                            if (blIdFound == true)
                            {

                                if (blSrcFound == true)
                                {
                                    strWrite = "  <item id=\"" + strIdFound + "\" href=\"" + strSrcFound + "\" media-type=\"application/xhtml+xml\"/>";
                                    swFiles.WriteLine(strWrite);

                                    strIDRef = strIDRef + "\n" + "  <itemref idref=\"" + strIdFound + "\" linear=\"yes\" />";

                                }


                            }



                            blIdFound = false;
                            blSrcFound = false;
                            blTitleFound = false;
                            blATitleFound = false;

                            strIdFound = "";
                            strSrcFound = "";
                            strTitleFound = "";
                            strATitleFound = "";

                            blSplitStart = false;
                        }



                        toolStripProgressBar1.Value = j + 1;

                    }

                    swFiles.WriteLine("  </manifest>\n");

                    swFiles.WriteLine(strIDRef);

                    swFiles.WriteLine("<guide>");

                    for (int j = 0; j < i; j++)
                    {




                        if (strLines[j].StartsWith("<split"))
                        {
                            strFileNames = Regex.Replace(strLines[j], "^<split filename=\"([^<>]+)\">$", "$1");
                            blSplitStart = true;
                            //swFiles.WriteLine("      <content src=\"" + strFileNames + "\"/>");
                            blSrcFound = true;
                            strSrcFound = strFileNames;

                        }

                        if (strLines[j].StartsWith("<head>") == true)
                        {
                            j++;
                            if (strLines[j].StartsWith("<title>") == true)
                            {
                                strChapterTitle = Regex.Replace(strLines[j], "^<title>(.*)</title>$", "$1");
                                //swFiles.WriteLine("        <text>" + strChapterTitle + "</text>");
                                blTitleFound = true;
                                strTitleFound = strChapterTitle;
                            }

                        }

                        if (strLines[j].StartsWith("<h2 class=\"chaptertitle\">") == true)
                        {

                            strChapterTitle = Regex.Replace(strLines[j], "^<h2 class=\"chaptertitle\">(.*)</h2>$", "$1");
                            strChapterTitle = RemoveTag(strChapterTitle);
                            blATitleFound = true;
                            strATitleFound = strChapterTitle;


                        }



                        if (strLines[j].StartsWith("<div>") == true)
                        {
                            j++;
                            if (strLines[j].StartsWith("<a id=") == true)
                            {
                                strChapterTitle = Regex.Replace(strLines[j], "^<a id=\"([^<]*)\"></a>(.*)$", "$1");
                                //swFiles.WriteLine("    <navPoint class=\"chapter\" id=\"" + strChapterTitle + "\" playOrder=\"1\">");
                                blIdFound = true;
                                strIdFound = strChapterTitle;

                            }

                        }





                        if (strLines[j].StartsWith("</split"))
                        {
                            strWrite = "";
                            if (blIdFound == true)
                            {
                                strWrite = strIdFound;
                                if (blATitleFound == true)
                                {
                                    //strATitleFound
                                }
                                else
                                {
                                    if (blTitleFound == true)
                                    {
                                        strATitleFound = strTitleFound;
                                    }


                                }

                                strWrite = "<reference type=\"text\"\n" +
                                "		   title=\"" + strATitleFound + "\"\n" +
                                "          href=\"" + strSrcFound + "\"/>";

                                swFiles.WriteLine(strWrite);
                            }



                            blIdFound = false;
                            blSrcFound = false;
                            blTitleFound = false;
                            blATitleFound = false;

                            strIdFound = "";
                            strSrcFound = "";
                            strTitleFound = "";
                            strATitleFound = "";

                            blSplitStart = false;
                        }



                        toolStripProgressBar1.Value = j + 1;

                    }


















                    swFiles.WriteLine("</guide>\n</package>");

                    swFiles.Flush();
                    swFiles.Close();


                    this.Refresh();

                    rtbContent.Text = string.Join("\n", strLines);
                    toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

                    toolStripStatusLabel1.Text = "Ready";
                    Application.UseWaitCursor = false;
                }
                catch
                {
                    MessageBox.Show("Unexpected Error", "ERR", MessageBoxButtons.OK);

                }
            }


        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            createOPF(); 
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {

            Application.UseWaitCursor = true;

            ofdOpen.Title = "Select an entity file";
            ofdOpen.Filter = "XML Files (*.xml)|*.xml|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            ofdOpen.FilterIndex = 0;
            ofdOpen.RestoreDirectory = true;

            try
            {
                if (ofdOpen.ShowDialog() == DialogResult.OK)
                {
                    strOpenedIndex = ofdOpen.FileName.ToString();
                    lblIndex.Text = strOpenedIndex;

                    WriteLog("Open entity\t" + strOpenedIndex);

                    this.Refresh();
                    rtbIndex.LoadFile(ofdOpen.FileName.ToString(), RichTextBoxStreamType.PlainText);

                    splitContainer1.Panel2.Show();
                    splitContainer1.Panel2Collapsed = false;
                    indexToolStripMenuItem.Checked = true;

                }
            }
            catch
            {
                strOpenedIndex = "";
                MessageBox.Show("Unable to open file!", "Open", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            Application.UseWaitCursor = false;


        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            CreateEntityM();
            ReplaceEntityFromEntityFile();

        }

        private void ReplaceEntityFromEntityFile()
        {


            Application.UseWaitCursor = true;
            toolStripStatusLabel1.Text = "Replacing Entities... Please Wait";
            this.Refresh();
            string strContent = "";

            string[] strLines;

            strContent = rtbContent.Text;
            strLines = strContent.Split('\n');
            long i = strLines.Length;

            toolStripProgressBar1.Maximum = Convert.ToInt32(i) + 1;
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Value = 1;
            this.Refresh();

            long k = strEntityRepl.Length / 2;

            for (int j = 0; j < i; j++)
            {
                

                for (int m = 0; m < k; m++)
                {

                    if (strEntityRepl[m, 0] == null || strEntityRepl[m, 0] == "")
                    {

                    }
                    else
                    {
                        strLines[j] = EntityReplace(strLines[j], strEntityRepl[m, 0], strEntityRepl[m, 1]);
                    }
                }

                toolStripProgressBar1.Value++;
            }

            this.Refresh();

            rtbContent.Text = string.Join("\n", strLines);
            toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;

            toolStripStatusLabel1.Text = "Ready";
            Application.UseWaitCursor = false;

        }


        private void CreateEntityM()
        {

            string strEntityContent = "";

            string[] strEntityLines;

            try
            {
                strEntityContent = rtbIndex.Text;
                strEntityLines = strEntityContent.Split('\n');
                long i = strEntityLines.Length;

                strEntityRepl = new string[i, 2];

                for (int j = 0; j < i; j++)
                {
                    if (strEntityLines[j].IndexOf("\t") >= 0)
                    {
                        strEntityRepl[j, 0] = strEntityLines[j].Substring(0, strEntityLines[j].IndexOf("\t"));
                        strEntityRepl[j, 1] = strEntityLines[j].Substring(strEntityLines[j].IndexOf("\t") + 1);
                        //MessageBox.Show(strEntityRepl[j, 0].ToString());
                        //MessageBox.Show(strEntityRepl[j, 1].ToString());

                    }
                }
            }
            catch
            {
                MessageBox.Show("There are some problem in 'Entity file' please make suitable changes and load again");
            }

            //MessageBox.Show(strEntityRepl.Length.ToString());     

        }


    }
}