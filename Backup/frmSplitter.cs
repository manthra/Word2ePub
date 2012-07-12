using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ionic.Zip;
//using ICSharpCode.SharpZipLib.Zip;
 
namespace ORC
{
    public partial class frmSplitter : Form
    {
        string strFldName = "";
        public frmSplitter()
        {
            InitializeComponent();
        }

        private void LoadCombo()
        {

            try
            {
                StreamReader sr = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + "\\PUK_Publisher.txt", Encoding.ASCII);
                string strPub = sr.ReadToEnd();
                strPub = strPub.Replace('\r', '\n').Replace("\n\n","\n");
                //MessageBox.Show(strPub);  
                string[] strPubA = strPub.Split('\n');
                cmbPublisher.DataSource = strPubA;
                 
                sr.Close();
                sr.Dispose(); 
            }
            catch
            {

                //Do nothing....
            }


        }


        private void frmSplitter_Load(object sender, EventArgs e)
        {
            //lbOriginal.Items.Add(clsStaticVrs.getFileNames());
            //btnMultiLevels.Enabled = false; 
            LoadCombo();
            string[,] strA;
            strA = clsStaticVrs.getFileNames();
            strFldName = clsStaticVrs.getFolderNameSaved();
            lblFolder.Text = strFldName;

            
            for (int i = 0; i < strA.Length-1; i++)
            {
                //MessageBox.Show(strA[i, 0]);
                try
                {

                    if (strA[i, 0].Length > 1)
                    {
                     /*   lbOriginal.Items.Add(strA[i, 0]);
                        lbTbContent.Items.Add(strA[i, 1]);
                        lbLevel.Items.Add(strA[i, 2]);*/
                        
                        ListViewItem lvi = new ListViewItem(strA[i, 0]);
                        lvi.SubItems.Add(strA[i, 1]);
                        lvi.SubItems.Add(strA[i, 2]);
                        lvi.SubItems.Add(strA[i, 3]);

                        listView1.Items.Add(lvi);   
                        
                           

                    }
                }
                catch
                {

                }
            }
            //lbOriginal.DataSource = clsStaticVrs.getFileNames();
             
        }

        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {

        }

       
        /*
        private void btnChange_Click(object sender, EventArgs e)
        {
            bool blErr = false;
            for (int i = 0; i < lbOriginal.Items.Count-1  ; i++)
            {
                if (i == lbOriginal.SelectedIndex)
                {

                }
                else
                {
                    if (lbOriginal.Items[i].ToString()  == txtFileName.Text)
                    {
                        blErr = true;
                    }

                }
                
            }

            if (blErr == false)
            {
                try
                {
                    FileInfo fi = new FileInfo(strFldName + "\\" + lbOriginal.Items[lbOriginal.SelectedIndex].ToString());
                    fi.MoveTo(strFldName + "\\" + txtFileName.Text);

                    lbOriginal.Items[lbOriginal.SelectedIndex] = txtFileName.Text;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Error");   
                }
            }
            else
            {
                MessageBox.Show("Filename already exist!", "Dupicate Filename", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            

        }
        */
        
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                
                int selectedIndex = listView1.SelectedItems[0].Index; // clone the item that is moving 
                ListViewItem cloneItem = (ListViewItem)listView1.Items[listView1.SelectedItems[0].Index].Clone(); // Save the position below the current selected node 
                int nextIndex = listView1.SelectedItems[0].Index + 1; 
                ListViewItem nextItem = listView1.Items[nextIndex]; // Insert at new position 
                listView1.Items.Insert(nextIndex + 1, cloneItem); // Remove the item that was at old position 
                listView1.Items.RemoveAt(selectedIndex); // Restore the moved item as the selected item 
                listView1.Items[selectedIndex] = nextItem;
                listView1.Items[nextIndex].Selected = true; 
                listView1.Focus();
                listView1.Refresh(); 
                
                /*
                for (int k = 0; k < listView1.SelectedItems.Count; k++)
                {
                    int selectedIndex = listView1.SelectedItems[k].Index; // clone the item that is moving 
                    ListViewItem cloneItem = (ListViewItem)listView1.Items[listView1.SelectedItems[k].Index].Clone(); // Save the position below the current selected node 
                    int nextIndex = listView1.SelectedItems[k].Index + 1;
                    ListViewItem nextItem = listView1.Items[nextIndex]; // Insert at new position 
                    listView1.Items.Insert(nextIndex + 1, cloneItem); // Remove the item that was at old position 
                    listView1.Items.RemoveAt(selectedIndex); // Restore the moved item as the selected item 
                    listView1.Items[selectedIndex] = nextItem;
                    listView1.Items[nextIndex].Selected = true;
                    listView1.Focus();
                    listView1.Refresh();
                }
                */

            }
            catch
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            /*
            string strBuffer = "";
            try
            {
                lbTbContent.SelectedIndex = lbOriginal.SelectedIndex;
  
                strBuffer = lbOriginal.Items[lbOriginal.SelectedIndex - 1].ToString();
                lbOriginal.Items[lbOriginal.SelectedIndex - 1] = lbOriginal.SelectedItem.ToString();
                lbOriginal.Items[lbOriginal.SelectedIndex] = strBuffer;
                lbOriginal.SelectedIndex--;

                strBuffer = lbTbContent.Items[lbTbContent.SelectedIndex - 1].ToString();
                lbTbContent.Items[lbTbContent.SelectedIndex - 1] = lbTbContent.SelectedItem.ToString();
                lbTbContent.Items[lbTbContent.SelectedIndex] = strBuffer;
                lbTbContent.SelectedIndex--;

                

            }
            catch
            {

            }*/
            try
            {
                int selectedIndex = listView1.SelectedItems[0].Index;           // clone the item that is moving
                ListViewItem cloneItem = (ListViewItem)listView1.Items[listView1.SelectedItems[0].Index].Clone(); // Save the position above the current selected node
                int previousIndex = listView1.SelectedItems[0].Index - 1;
                ListViewItem previousItem = listView1.Items[previousIndex]; // Insert at new position
                listView1.Items.Insert(previousIndex, cloneItem); // Remove the item that was at old position
                listView1.Items.RemoveAt(selectedIndex); // Restore the moved item as the selected item
                listView1.Items[selectedIndex] = previousItem;
                listView1.Items[previousIndex].Selected = true;
                listView1.Focus();
                listView1.Refresh();
            }
            catch
            {

            }


        }
        /*
        private void btnTbContent_Click(object sender, EventArgs e)
        {
             
            bool blErr = false;
            for (int i = 0; i < lbTbContent.Items.Count - 1; i++)
            {
                if (i == lbTbContent.SelectedIndex)
                {

                }
                else
                {
                    if (lbTbContent.Items[i].ToString() == txtTbContent.Text)
                    {
                        blErr = true;
                    }

                }

            }

            if (blErr == false)
            {
                lbLevel.Items[lbLevel.SelectedIndex] = txtLevel.Text; 
                lbTbContent.Items[lbTbContent.SelectedIndex] = txtTbContent.Text;
                   
            }
            else
            {
                MessageBox.Show("Caption Already Exist!", "Dupicate Caption", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }*/
        /*
        private void lbTbContent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbTbContent.SelectedIndex >= 0)
            {
                txtTbContent.Text = lbTbContent.SelectedItem.ToString();
                lbLevel.SelectedIndex = lbTbContent.SelectedIndex;
                txtLevel.Text = lbLevel.SelectedItem.ToString();
    
            }
        }
         */ 

        private void btnTOC_Click(object sender, EventArgs e)
        {
            CreateTOC();
        }

        private void CreateTOC()
        {
            bool blhr = false;
            if (MessageBox.Show("Create Table of Contents according to hierarchy level?\nSelect 'Yes' for creating '<div class=\"tocX\"'\nSelect 'No' for creating '<div class=\"tbcontent\"' ", "Hierarchy", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                blhr = true;
            }

            StreamWriter sw = new StreamWriter(strFldName + "\\tbcontent.xhtml");
            
            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                "<head>\n" +
                                "<title>" + clsStaticVrs.getBookTitle() + "</title>\n" +
                                "</head>\n" +
                                "<body class=\"book\">\n" +
                                "<div class=\"tocct\">Table of Contents</div>");


            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {

                if (listView1.Items[i].Text == "tbcontent.xhtml")
                {
                    listView1.Items.RemoveAt(i);
                    break;

                }
            }



            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {
                if (listView1.Items[i].Checked == true)
                {
                    if (blhr == true)
                    {
                        sw.WriteLine("<div class=\"toc" + listView1.Items[i].SubItems[2].Text + "\"><a href=\"" + listView1.Items[i].Text + "\">" + listView1.Items[i].SubItems[3].Text + "</a></div>");
                    }
                    else
                    {
                        sw.WriteLine("<div class=\"tbcontent\"><a href=\"" + listView1.Items[i].Text + "\">" + listView1.Items[i].SubItems[3].Text + "</a></div>");
                    }
                }
            }
            sw.WriteLine("</body>\n</html>"); 
            sw.Close();

           


            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {
                
                if (listView1.Items[i].Text == "cover.xhtml")
                {
                    ListViewItem lvi = new ListViewItem("tbcontent.xhtml");
                    lvi.SubItems.Add("Table of Contents");
                    lvi.SubItems.Add("0");
                    listView1.Items.Insert(i+1,lvi);
                    listView1.Items[i + 1].Checked = true; 
                    //MessageBox.Show(listView1.Items[i].Text);  
                    break;

                }
            }


            MessageBox.Show("Table of contents \"tbcontent.xhtml\" created successfully", "TOC", MessageBoxButtons.OK);    

        }
        /*
        private void CreateTOCOld()
        {
            StreamWriter sw = new StreamWriter(strFldName + "\\tbcontent.xhtml");

            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\n" +
                                "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n" +
                                "<head>\n" +
                                "<title>" + clsStaticVrs.getBookTitle() + "</title>\n" +
                                "<link href=\"style.css\" type=\"text/css\" rel=\"stylesheet\" />\n" +
                                "<link rel=\"stylesheet\" type=\"application/vnd.adobe-page-template+xml\" href=\"page-template.xpgt\"/>\n" +
                                "</head>\n" +
                                "<body class=\"book\">\n" +
                                "<div class=\"tocct\">Table of Contents</div>");


            for (int i = 0; i < lbTbContent.Items.Count - 1; i++)
            {
                sw.WriteLine("<div class=\"toc\"><a href=\"" + lbOriginal.Items[i].ToString() + "\">" + lbTbContent.Items[i].ToString() + "</a></div>");

            }
            sw.WriteLine("</body>\n</html>");
            sw.Close();

        }
        */
        
        private void btnNCX_Click(object sender, EventArgs e)
        {
            CreateTOCNCX();
        }


        private void ReassignNavPoint()
        {
            string strOEBPS = clsStaticVrs.getID();
            DirectoryInfo di = new DirectoryInfo(strFldName);
            string strParentFolder = di.Parent.FullName.ToString();

            
            StreamReader sr = new StreamReader(strParentFolder + "\\toc.ncx");
             
            string strMultiLines= sr.ReadToEnd();
            sr.Close();
            sr.Dispose(); 
            string[] strNCXLines = strMultiLines.Split('\n');
            int intNavPnt = 1;

            for (int h = 0; h < strNCXLines.Length; h++)
            {
                
                if (strNCXLines[h].IndexOf("<navPoint ") >= 0)
                {
                    //MessageBox.Show(strNCXLines[h]);
                    strNCXLines[h] = Regex.Replace(strNCXLines[h], "^(.*)navPoint id=(.*)$", "$1navPoint ") + "id=\"navPoint-" + intNavPnt.ToString() + "\" playOrder=\"" + intNavPnt.ToString() + "\">";
                    //MessageBox.Show(strNCXLines[h]);
                    intNavPnt++;
                }

            }

            StreamWriter sw = new StreamWriter(strParentFolder + "\\toc.ncx");
            sw.Write(string.Join("\n", strNCXLines));
            sw.Close();
            sw.Dispose();
            MessageBox.Show("navPoint ids and playOrders are re-created", "navPoint");  

        }

        private void CreateTOCNCX()
        {
            string strOEBPS = clsStaticVrs.getID();
            DirectoryInfo di = new DirectoryInfo(strFldName);
            string strParentFolder = di.Parent.FullName.ToString();

            StreamWriter sw = new StreamWriter(strParentFolder + "\\toc.ncx");

            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<!DOCTYPE ncx PUBLIC \"-//NISO//DTD ncx 2005-1//EN\" \"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\">\n" +
                            "<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\" xml:lang=\"en\">\n" +
                            "\t<head>\n" +
                            "\t\t<meta name=\"dtb:uid\" content=\"" + clsStaticVrs.getBookTitle() + "\"/>\n" +
                            "\t\t<meta name=\"dtb:depth\" content=\"1\"/>\n" +
                            "\t\t<meta name=\"dtb:totalPageCount\" content=\"0\"/>\n" +
                            "\t\t<meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n" +
                            "\t</head>\n" +
                            "\t<docTitle>\n" +
                            "\t\t<text>" + clsStaticVrs.getBookTitle() + "</text>\n" +
                            "\t</docTitle>\n" +
                            "\t<navMap>");
            
            listView2.Items.Clear();   

            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {

                if (listView1.Items[i].Checked == true)
                {
                    ListViewItem lvi = new ListViewItem(listView1.Items[i].Text);
                    lvi.SubItems.Add(listView1.Items[i].SubItems[1].Text);
                    lvi.SubItems.Add(listView1.Items[i].SubItems[2].Text);
                    listView2.Items.Add(lvi);   
                        
                }

            }



            int intSq = 0;

            for (int i = 0; i <= listView2.Items.Count - 1; i++)
            {

                    intSq++;
                    switch (listView2.Items[i].SubItems[2].Text)
                    {
                        case "0":
                            {


                                sw.WriteLine("\t\t<navPoint id=\"navPoint-" + intSq.ToString() + "\" playOrder=\"" + intSq.ToString() + "\">\n" +
                                "\t\t\t<navLabel>\n" +
                                "\t\t\t\t<text>" + listView2.Items[i].SubItems[1].Text + "</text>\n" +
                                "\t\t\t</navLabel>\n" +
                                "\t\t\t\t<content src=\"xhtml/" + listView2.Items[i].Text + "\"/>");

                                try
                                {
                                    if (listView2.Items[i + 1].SubItems[2].Text == "0")
                                    {
                                        sw.WriteLine("\t\t</navPoint>");
                                    }
                                    else
                                    {
                                        if (listView2.Items[i + 1].SubItems[2].Text == "1")
                                        {

                                        }
                                        else
                                        {

                                        }

                                    }
                                }
                                catch
                                {
                                    sw.WriteLine("\t\t</navPoint>");
                                }

                                break;
                            }
                        case "1":
                            {
                                sw.WriteLine("\t\t\t<navPoint id=\"navPoint-" + intSq.ToString() + "\" playOrder=\"" + intSq.ToString() + "\">\n" +
                                "\t\t\t\t<navLabel>\n" +
                                "\t\t\t\t\t<text>" + listView2.Items[i].SubItems[1].Text + "</text>\n" +
                                "\t\t\t\t</navLabel>\n" +
                                "\t\t\t\t\t<content src=\"xhtml/" + listView2.Items[i].Text + "\"/>");

                                try
                                {
                                    if (listView2.Items[i + 1].SubItems[2].Text == "1")
                                    {
                                        sw.WriteLine("\t\t\t</navPoint>");
                                    }
                                    else
                                    {
                                        if (listView2.Items[i + 1].SubItems[2].Text == "0")
                                        {
                                            sw.WriteLine("\t\t\t</navPoint>");
                                            sw.WriteLine("\t\t</navPoint>");
                                        }

                                    }
                                }
                                catch
                                {
                                    sw.WriteLine("\t\t\t</navPoint>");
                                }

                                break;
                            }
                        case "2":
                            {
                                sw.WriteLine("\t\t\t\t<navPoint id=\"navPoint-" + intSq.ToString() + "\" playOrder=\"" + intSq.ToString() + "\">\n" +
                                "\t\t\t\t\t<navLabel>\n" +
                                "\t\t\t\t\t\t<text>" + listView2.Items[i].SubItems[1].Text + "</text>\n" +
                                "\t\t\t\t\t</navLabel>\n" +
                                "\t\t\t\t\t\t<content src=\"xhtml/" + listView2.Items[i].Text + "\"/>");

                                try
                                {

                                    if (listView2.Items[i + 1].SubItems[2].Text == "2")
                                    {
                                        sw.WriteLine("\t\t\t\t</navPoint>");
                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (listView2.Items[i + 1].SubItems[2].Text == "1")
                                            {
                                                sw.WriteLine("\t\t\t\t</navPoint>");
                                                sw.WriteLine("\t\t\t</navPoint>");

                                            }
                                            else
                                            {
                                                try
                                                {
                                                    if (listView2.Items[i + 1].SubItems[2].Text == "0")
                                                    {
                                                        sw.WriteLine("\t\t\t\t</navPoint>");
                                                        sw.WriteLine("\t\t\t</navPoint>");
                                                        sw.WriteLine("\t\t</navPoint>");
                                                    }
                                                }
                                                catch
                                                {
                                                    sw.WriteLine("\t\t\t\t</navPoint>");
                                                    sw.WriteLine("\t\t\t</navPoint>");
                                                    sw.WriteLine("\t\t</navPoint>");
                                                 
                                                }
                                            }
                                        }
                                        catch
                                        {

                                            sw.WriteLine("\t\t\t\t</navPoint>");
                                            sw.WriteLine("\t\t\t</navPoint>");

                                        }

                                    }
                                }
                                catch
                                {
                                    sw.WriteLine("\t\t\t\t</navPoint>");
                                }


                                break;
                            }
                        case "3":
                            {
                                sw.WriteLine("\t\t\t\t\t<navPoint id=\"navPoint-" + intSq.ToString() + "\" playOrder=\"" + intSq.ToString() + "\">\n" +
                                "\t\t\t\t\t\t<navLabel>\n" +
                                "\t\t\t\t\t\t\t<text>" + listView2.Items[i].SubItems[1].Text + "</text>\n" +
                                "\t\t\t\t\t\t</navLabel>\n" +
                                "\t\t\t\t\t\t\t<content src=\"xhtml/" + listView2.Items[i].Text + "\"/>");


                                try
                                {
                                    if (listView2.Items[i + 1].SubItems[2].Text == "3")
                                    {
                                        sw.WriteLine("\t\t\t\t\t</navPoint>");

                                    }
                                    else
                                    {
                                        try
                                        {
                                            if (listView2.Items[i + 1].SubItems[2].Text == "2")
                                            {
                                                sw.WriteLine("\t\t\t\t\t</navPoint>");
                                                sw.WriteLine("\t\t\t\t</navPoint>");
                                            }
                                            else
                                            {
                                                try
                                                {
                                                    if (listView2.Items[i + 1].SubItems[2].Text == "1")
                                                    {
                                                        sw.WriteLine("\t\t\t\t\t</navPoint>");
                                                        sw.WriteLine("\t\t\t\t</navPoint>");
                                                        sw.WriteLine("\t\t\t</navPoint>");

                                                    }
                                                    else
                                                    {
                                                        try
                                                        {
                                                            if (listView2.Items[i + 1].SubItems[2].Text == "0")
                                                            {
                                                                sw.WriteLine("\t\t\t\t\t</navPoint>");
                                                                sw.WriteLine("\t\t\t\t</navPoint>");
                                                                sw.WriteLine("\t\t\t</navPoint>");
                                                                sw.WriteLine("\t\t</navPoint>");
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            sw.WriteLine("\t\t\t\t\t</navPoint>");
                                                            sw.WriteLine("\t\t\t\t</navPoint>");
                                                            sw.WriteLine("\t\t\t</navPoint>");
                                                            sw.WriteLine("\t\t</navPoint>");
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                    sw.WriteLine("\t\t\t\t\t</navPoint>");
                                                    sw.WriteLine("\t\t\t\t</navPoint>");
                                                    sw.WriteLine("\t\t\t</navPoint>");

                                                }
                                            }
                                        }
                                        catch
                                        {
                                            sw.WriteLine("\t\t\t\t\t</navPoint>");
                                            sw.WriteLine("\t\t\t\t</navPoint>");
                                        }
                                    }
                                }
                                catch
                                {
                                    sw.WriteLine("\t\t\t\t\t</navPoint>");
                                }



                                break;
                            }
                        default:
                            break;
                    }


                    //sw.WriteLine("<div class=\"toc\"><a href=\"" + lbOriginal.Items[i].ToString() + "\">" + lbTbContent.Items[i].ToString() + "</a></div>");

               

            }
            sw.WriteLine("\t</navMap>\n</ncx>");
            sw.Close();
            MessageBox.Show("NCX \"toc.ncx\" created successfully", "NCX", MessageBoxButtons.OK);

        }

        /*
        private void CreateTOCNCXOLD()
        {
            StreamWriter sw = new StreamWriter(strFldName + "\\toc.ncx");

            sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<!DOCTYPE ncx PUBLIC \"-//NISO//DTD ncx 2005-1//EN\" \"http://www.daisy.org/z3986/2005/ncx-2005-1.dtd\">\n" +
                            "<ncx xmlns=\"http://www.daisy.org/z3986/2005/ncx/\" version=\"2005-1\" xml:lang=\"en\">\n" +
                            "\t<head>\n" +
                            "\t\t<meta name=\"dtb:uid\" content=\"" + clsStaticVrs.getBookTitle() + "\"/>\n" +
                            "\t\t<meta name=\"dtb:depth\" content=\"1\"/>\n" +
                            "\t\t<meta name=\"dtb:totalPageCount\" content=\"0\"/>\n" +
                            "\t\t<meta name=\"dtb:maxPageNumber\" content=\"0\"/>\n" +
                            "\t</head>\n" +
                            "\t<docTitle>\n" +
                            "\t\t<text>" + clsStaticVrs.getBookTitle() + "</text>\n" +
                            "\t</docTitle>\n" +
                            "\t<navMap>");

            for (int i = 0; i < lbTbContent.Items.Count - 1; i++)
            {
                switch (lbLevel.Items[i].ToString())
                {
                    case "0":
                        {


                            sw.WriteLine("\t\t<navPoint id=\"navPoint-1\" playOrder=\"1\">\n" +
                            "\t\t\t<navLabel>\n" +
                            "\t\t\t\t<text>" + lbTbContent.Items[i].ToString() + "</text>\n" +
                            "\t\t\t</navLabel>\n" +
                            "\t\t\t\t<content src=\"OEBPS/" + lbOriginal.Items[i].ToString() + "\"/>");

                            try
                            {
                                if (lbLevel.Items[i + 1].ToString() == "0")
                                {
                                    sw.WriteLine("\t\t</navPoint>");
                                }
                                else
                                {
                                    if (lbLevel.Items[i + 1].ToString() == "1")
                                    {

                                    }
                                    else
                                    {

                                    }

                                }
                            }
                            catch
                            {

                            }

                            break;
                        }
                    case "1":
                        {
                            sw.WriteLine("\t\t\t<navPoint id=\"navPoint-1\" playOrder=\"1\">\n" +
                            "\t\t\t\t<navLabel>\n" +
                            "\t\t\t\t\t<text>" + lbTbContent.Items[i].ToString() + "</text>\n" +
                            "\t\t\t\t</navLabel>\n" +
                            "\t\t\t\t\t<content src=\"OEBPS/" + lbOriginal.Items[i].ToString() + "\"/>");

                            try
                            {
                                if (lbLevel.Items[i + 1].ToString() == "1")
                                {
                                    sw.WriteLine("\t\t\t</navPoint>");
                                }
                                else
                                {
                                    if (lbLevel.Items[i + 1].ToString() == "0")
                                    {
                                        sw.WriteLine("\t\t\t</navPoint>");
                                        sw.WriteLine("\t\t</navPoint>");
                                    }

                                }
                            }
                            catch
                            {

                            }

                            break;
                        }
                    case "2":
                        {
                            sw.WriteLine("\t\t\t\t<navPoint id=\"navPoint-1\" playOrder=\"1\">\n" +
                            "\t\t\t\t\t<navLabel>\n" +
                            "\t\t\t\t\t\t<text>" + lbTbContent.Items[i].ToString() + "</text>\n" +
                            "\t\t\t\t\t</navLabel>\n" +
                            "\t\t\t\t\t\t<content src=\"OEBPS/" + lbOriginal.Items[i].ToString() + "\"/>");

                            try
                            {

                                if (lbLevel.Items[i + 1].ToString() == "2")
                                {
                                    sw.WriteLine("\t\t\t\t</navPoint>");
                                }
                                else
                                {
                                    if (lbLevel.Items[i + 1].ToString() == "1")
                                    {
                                        sw.WriteLine("\t\t\t\t</navPoint>");
                                        sw.WriteLine("\t\t\t</navPoint>");

                                    }
                                    else
                                    {
                                        if (lbLevel.Items[i + 1].ToString() == "0")
                                        {
                                            sw.WriteLine("\t\t\t\t</navPoint>");
                                            sw.WriteLine("\t\t\t</navPoint>");
                                            sw.WriteLine("\t\t</navPoint>");
                                        }

                                    }

                                }
                            }
                            catch
                            {

                            }


                            break;
                        }
                    case "3":
                        {
                            sw.WriteLine("\t\t\t\t\t<navPoint id=\"navPoint-1\" playOrder=\"1\">\n" +
                            "\t\t\t\t\t\t<navLabel>\n" +
                            "\t\t\t\t\t\t\t<text>" + lbTbContent.Items[i].ToString() + "</text>\n" +
                            "\t\t\t\t\t\t</navLabel>\n" +
                            "\t\t\t\t\t\t\t<content src=\"OEBPS/" + lbOriginal.Items[i].ToString() + "\"/>");


                            try
                            {
                                if (lbLevel.Items[i + 1].ToString() == "3")
                                {
                                    sw.WriteLine("\t\t\t\t\t</navPoint>");

                                }
                                else
                                {
                                    if (lbLevel.Items[i + 1].ToString() == "2")
                                    {
                                        sw.WriteLine("\t\t\t\t\t</navPoint>");
                                        sw.WriteLine("\t\t\t\t</navPoint>");
                                    }
                                    else
                                    {
                                        if (lbLevel.Items[i + 1].ToString() == "1")
                                        {
                                            sw.WriteLine("\t\t\t\t\t</navPoint>");
                                            sw.WriteLine("\t\t\t\t</navPoint>");
                                            sw.WriteLine("\t\t\t</navPoint>");

                                        }
                                        else
                                        {
                                            if (lbLevel.Items[i + 1].ToString() == "0")
                                            {
                                                sw.WriteLine("\t\t\t\t\t</navPoint>");
                                                sw.WriteLine("\t\t\t\t</navPoint>");
                                                sw.WriteLine("\t\t\t</navPoint>");
                                                sw.WriteLine("\t\t</navPoint>");
                                            }

                                        }

                                    }
                                }
                            }
                            catch
                            {

                            }



                            break;
                        }
                    default:
                        break;
                }


                //sw.WriteLine("<div class=\"toc\"><a href=\"" + lbOriginal.Items[i].ToString() + "\">" + lbTbContent.Items[i].ToString() + "</a></div>");

            }
            sw.WriteLine("\t</navMap>\n</ncx>");
            sw.Close();
        }
        */
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtFilenameRe.Text = listView1.SelectedItems[0].Text;
                txtHeadRe.Text = listView1.SelectedItems[0].SubItems[1].Text;
                txtLevelRe.Text = listView1.SelectedItems[0].SubItems[2].Text;
                txtTbContentF.Text = listView1.SelectedItems[0].SubItems[3].Text;
            }
            catch
            {

            }
        }

        private void btnRename_Click(object sender, EventArgs e)
        {
            try
            {
                bool blErr = false;
                for (int i = 0; i < listView1.Items.Count - 1; i++)
                {
                    if (i == listView1.SelectedIndices[0])
                    {

                    }
                    else
                    {
                        if (listView1.Items[i].Text.ToString() == txtFilenameRe.Text)
                        {
                            blErr = true;
                        }

                    }

                }

                if (blErr == false)
                {
                    try
                    {
                        FileInfo fi = new FileInfo(strFldName + "\\" + listView1.SelectedItems[0].Text.ToString());
                        fi.MoveTo(strFldName + "\\" + txtFilenameRe.Text);

                        //lbOriginal.Items[lbOriginal.SelectedIndex] = txtFileName.Text;

                        ListViewItem lvix = listView1.SelectedItems[0];
                        lvix.Text = txtFilenameRe.Text;
                        lvix.SubItems[1].Text = txtHeadRe.Text;
                        lvix.SubItems[2].Text = txtLevelRe.Text;
                        lvix.SubItems[3].Text = txtTbContentF.Text;

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString(), "Error");
                    }
                }
                else
                {
                    MessageBox.Show("Filename already exist!", "Dupicate Filename", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }



                
            }
            catch
            {

            }
        }

        private void btnEpub_Click(object sender, EventArgs e)
        {

            createZip();

            /*
            try
            {
                // Depending on the directory this could be very large and would require more attention
                // in a commercial package.


                DirectoryInfo di = new DirectoryInfo(strFldName);
                string strParentFolder = di.Parent.FullName.ToString();
                string[] dirs = Directory.GetDirectories(strParentFolder);
                string strPatentParentFolder = Directory.GetParent(strParentFolder).FullName.ToString();  
            */
                /*
                foreach (string strSD in dirs)
                {
                    string[] filenames = Directory.GetFiles(strSD);

                }*/
              /*  

                // 'using' statements gaurantee the stream is closed properly which is a big source
                // of problems otherwise.  Its exception safe as well which is great.
                using (ZipOutputStream s = new ZipOutputStream(File.Create(strPatentParentFolder+"\\out.epub")))
                {

                    s.SetLevel(9); // 0 - store only to 9 - means best compression

                    byte[] buffer = new byte[4096];
                    foreach (string strSD in dirs)
                    {
                        string[] filenames = Directory.GetFiles(strSD);

                        foreach (string file in filenames)
                        {

                            // Using GetFileName makes the result compatible with XP
                            // as the resulting path is not absolute.
                            ZipEntry entry = new ZipEntry(Path.GetFileName(file));

                            // Setup the entry data as required.

                            // Crc and size are handled by the library for seakable streams
                            // so no need to do them here.

                            // Could also use the last write time or similar for the file.
                            entry.DateTime = DateTime.Now;
                             
                            s.PutNextEntry(entry);
                             
                            using (FileStream fs = File.OpenRead(file))
                            {

                                // Using a fixed size buffer here makes no noticeable difference for output
                                // but keeps a lid on memory usage.
                                int sourceBytes;
                                do
                                {
                                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                    s.Write(buffer, 0, sourceBytes);
                                } while (sourceBytes > 0);
                            }
                        }



                    }
                    // Finish/Close arent needed strictly as the using statement does this automatically

                    // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                    // the created file would be invalid.
                    s.Finish();

                    // Close is important to wrap things up and unlock the file.
                    s.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception during processing");

                // No need to rethrow the exception as for our purposes its handled.
            }

            */
        }

        private void createZip()
        {
            string strOEBPS = clsStaticVrs.getID();
            if (txtID.Text.Length > 2)
            {
                try
                {

                    DirectoryInfo di = new DirectoryInfo(strFldName);
                    string strParentFolder = di.Parent.FullName.ToString();
                    string strFile2Save = Directory.GetParent(strParentFolder).ToString() + "\\" + txtID.Text + ".epub";
                    string strParentParentFolder = Directory.GetParent(di.Parent.FullName.ToString()).FullName.ToString();
                    //MessageBox.Show(strFile2Save);
                    using (ZipFile zip = new ZipFile())
                    {
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                        zip.AddFile(strParentParentFolder + "\\" + "mimetype","");
                        zip.AddDirectory(strParentParentFolder + "\\" + "META-INF","META-INF");
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                        zip.AddDirectory(strParentParentFolder + "\\" + strOEBPS, strOEBPS); 
                        zip.Save(strFile2Save);
                    }
                    MessageBox.Show("'epub' created successfully\nFile Name\t: " + strFile2Save, "epub");
                }
                catch
                {
                    MessageBox.Show("Unable to create epub", "Error");  
                }
            }
            else
            {
                MessageBox.Show("Enter Identifier", "epub");  
            }

        }
        
        private void btnOpf_Click(object sender, EventArgs e)
        {
            if (txtID.Text.Length > 2)
            {
                createPUKOPF();
            }
            else
            {
                MessageBox.Show("Enter Identifier", "Invalid");
                txtID.Focus(); 
            }
        }


        private void createMIME_Type()
        {
            try
            {
                string strOEBPS = clsStaticVrs.getID();
                Application.UseWaitCursor = true;
                DirectoryInfo di = new DirectoryInfo(strFldName);
                string strParentFolder = Directory.GetParent(di.Parent.FullName.ToString()).FullName.ToString();

                StreamWriter sw = new StreamWriter(strParentFolder + "\\mimetype");
                sw.Write("application/epub+zip");
                sw.Close();

                DirectoryInfo diMeta = new DirectoryInfo(strParentFolder + "\\META-INF");
                if (diMeta.Exists)
                {
                    diMeta.Delete(true);
                }
                DirectoryInfo diMeta1 = new DirectoryInfo(strParentFolder);
                diMeta1.CreateSubdirectory("META-INF"); 

                StreamWriter swContainer = new StreamWriter(strParentFolder + "\\META-INF\\container.xml");

                swContainer.Write("<?xml version=\"1.0\"?>\n" +
                            "<container version=\"1.0\" xmlns=\"urn:oasis:names:tc:opendocument:xmlns:container\">\n" +
                            "  <rootfiles>\n" +
                            "    <rootfile full-path=\"" + strOEBPS + "/content.opf\" media-type=\"application/oebps-package+xml\"/>\n" +
                            "  </rootfiles>\n" +
                            "</container>");
                swContainer.Close();
                Application.UseWaitCursor = false;
                MessageBox.Show("mimetype and meta-inf created successfully", "mimetype");  
            }
            catch
            {
                MessageBox.Show("Unexpected Error", "Error");
                Application.UseWaitCursor = false;
            }

        }


        private void GetAllImages(string strParentFolder)
        {

            DirectoryInfo di = new DirectoryInfo(strParentFolder);
            DirectoryInfo di1 = new DirectoryInfo(strParentFolder + "\\images");
            if (di1.Exists)
            {
                if (MessageBox.Show("Folder 'images' already exist in " + strParentFolder + "\nDo you want to delete existing files?", "Delete Files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //di1.Attributes = FileAttributes.Archive;    
                    try
                    {
                        di1.Delete(true);
                        di.CreateSubdirectory("images");
                    }
                    catch
                    {
                        MessageBox.Show("Unable to delete or create Folder!\nSome files may be 'Read-Only'", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    
                }

            }
            else
            {
                
                di.CreateSubdirectory("images");
            }

            
            
            oFD1.Title = "Select images...";
            oFD1.Filter = "All Files (*.*)|*.*";
            oFD1.FilterIndex = 0;
            oFD1.FileName = ""; 
            oFD1.RestoreDirectory = true;

            try 
            {
                if (oFD1.ShowDialog() == DialogResult.OK)
                {
                    foreach (string  flSingle in oFD1.FileNames)
                    {
                        FileInfo fi = new FileInfo(flSingle);
                        fi.CopyTo(strParentFolder + "\\images\\" + fi.Name.ToString()); 

                    }

                }
            }
            catch
            {
                MessageBox.Show("Unable to copy file(s)!\nCopy Files Manually and Run Again", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
 



        }

        string strStylePath = "";

        private void GetAllStyles(string strParentFolder)
        {
            DirectoryInfo di = new DirectoryInfo(strParentFolder);

            DirectoryInfo di1 = new DirectoryInfo(strParentFolder + "\\styles");
            if (di1.Exists)
            {
                if (MessageBox.Show("Folder 'styles' already exist in " + strParentFolder + "\nDo you want to delete existing files?", "Delete Files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        di1.Delete(true);
                        di.CreateSubdirectory("styles");
                    }
                    catch
                    {
                        MessageBox.Show("Unable to delete or create Folder!\nSome files may be 'Read-Only'", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

            }
            else
            {
                di.CreateSubdirectory("styles");
            }


            oFD1.Title = "Select styles...";
            oFD1.Filter = "All Files (*.*)|*.*";
            oFD1.FilterIndex = 0;
            oFD1.FileName = "";
            oFD1.RestoreDirectory = true;
            strStylePath = "";
            try
            {
                if (oFD1.ShowDialog() == DialogResult.OK)
                {
                    foreach (string flSingle in oFD1.FileNames)
                    {
                        FileInfo fi = new FileInfo(flSingle);
                        fi.CopyTo(strParentFolder + "\\styles\\" + fi.Name.ToString());

                        if (fi.Extension == ".xpgt")
                        {
                            strStylePath = strStylePath + "<link rel=\"stylesheet\" type=\"application/vnd.adobe-page-template+xml\" href=\"../styles/" + fi.Name.ToString() + "\"/>\n";
                        }
                        else
                        {
                            if (fi.Extension == ".css")
                            {
                                strStylePath = strStylePath + "<link rel=\"stylesheet\" type=\"text/css\" href=\"../styles/" + fi.Name.ToString() + "\"/>\n";
                            }
                            else
                            {
                                strStylePath = strStylePath + "<link rel=\"stylesheet\" type=\"text\" href=\"../styles/" + fi.Name.ToString() + "\"/>\n";
                            }
                        }
                    }

                }
            }
            catch
            {
                MessageBox.Show("Unable to copy file(s)!\nCopy Files Manually and Run Again", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }


            //MessageBox.Show(strStylePath);  

        }



        private void GetAllFonts(string strParentFolder)
        {

            DirectoryInfo di = new DirectoryInfo(strParentFolder);
            DirectoryInfo di1 = new DirectoryInfo(strParentFolder + "\\fonts");
            if (di1.Exists)
            {
                if (MessageBox.Show("Folder 'fonts' already exist in " + strParentFolder + "\nDo you want to delete existing files?", "Delete Files", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    //di1.Attributes = FileAttributes.Archive;    
                    try
                    {
                        di1.Delete(true);
                        di.CreateSubdirectory("fonts");
                    }
                    catch
                    {
                        MessageBox.Show("Unable to delete or create Folder!\nSome files may be 'Read-Only'", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                }

            }
            else
            {

                di.CreateSubdirectory("fonts");
            }



            oFD1.Title = "Select fonts...";
            oFD1.Filter = "All Files (*.*)|*.*";
            oFD1.FilterIndex = 0;
            oFD1.FileName = "";
            oFD1.RestoreDirectory = true;

            try
            {
                if (oFD1.ShowDialog() == DialogResult.OK)
                {
                    foreach (string flSingle in oFD1.FileNames)
                    {
                        FileInfo fi = new FileInfo(flSingle);
                        fi.CopyTo(strParentFolder + "\\fonts\\" + fi.Name.ToString());

                    }

                }
            }
            catch
            {
                MessageBox.Show("Unable to copy file(s)!\nCopy Files Manually and Run Again", "Copy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }




        }




        private void createPUKOPF()
        {
            string strOEBPS = clsStaticVrs.getID();
            Application.UseWaitCursor = true;
            DirectoryInfo di = new DirectoryInfo(strFldName);
            string strParentFolder = di.Parent.FullName.ToString();

            StreamWriter sw = new StreamWriter(strParentFolder + "\\content.opf");
            sw.WriteLine("<?xml version=\"1.0\"?>\n" +
                    "<package xmlns=\"http://www.idpf.org/2007/opf\" unique-identifier=\"ISBN" + txtID.Text + "\" version=\"2.0\">\n" +
                    "\t<metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:opf=\"http://www.idpf.org/2007/opf\">\n" +
                    "\t\t<dc:title>" + txtTitle.Text + "</dc:title>\n" +
                    "\t\t<dc:creator opf:role=\"Author\" opf:file-as=\"" + txtCreator.Text + "\">" + txtCreator.Text + "</dc:creator>\n" +
                    "\t\t<dc:identifier id=\"ISBN" + txtID.Text + "\" opf:scheme=\"URN:ISBN/" + txtID.Text + "\"></dc:identifier>\n" +
                    "\t\t<dc:publisher>" + cmbPublisher.Text + "</dc:publisher>\n" +
                    "\t\t<dc:date>" + txtDate.Text + "</dc:date>\n" +
                    "\t\t<dc:language>" + txtLanguage.Text + "</dc:language>\n" +
                    "\t\t<dc:type>" + txttype.Text + "</dc:type>\n" +
                    "\t\t<dc:description>" + txtDescription.Text + "</dc:description>\n" +
                    "\t</metadata>"); 


            GetAllImages(strParentFolder);
            GetAllStyles(strParentFolder);
            GetAllFonts(strParentFolder);

            sw.WriteLine("\t<manifest>");

            DirectoryInfo diSt = new DirectoryInfo(strParentFolder + "\\styles");

            FileInfo[] fis = diSt.GetFiles("*.*");

            foreach (FileInfo  fiSt in fis)
            {
                if (fiSt.Extension.ToLower() == ".css")
                {
                    sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(".css", "").Replace(" ", "").Replace("-", "") + "\" href=\"styles/" + fiSt.Name.ToString() + "\" media-type=\"text/css\"/>");
                }
                else
                {
                    if (fiSt.Extension.ToLower() == ".xpgt")
                    {
                        sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(".xpgt", "").Replace(" ", "").Replace("-", "") + "\" href=\"styles/" + fiSt.Name.ToString() + "\" media-type=\"application/vnd.adobe-page-template+xml\"/>");
                    }
                    else
                    {
                        sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(fiSt.Extension.ToString(), "").Replace(" ", "").Replace("-", "") + "\" href=\"styles/" + fiSt.Name.ToString() + "\" media-type=\"application/unknown\"/>");
                    }
                }
            }



            diSt = new DirectoryInfo(strParentFolder + "\\images");

            FileInfo[] fig = diSt.GetFiles("*.*");

            foreach (FileInfo fiSt in fig)
            {
                //MessageBox.Show(fiSt.Extension.ToLower());
                if (fiSt.Extension.ToLower() == ".jpg")
                {
                    sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(".jpg", "").Replace(" ", "").Replace("-", "") + "\" href=\"images/" + fiSt.Name.ToString() + "\" media-type=\"image/jpeg\"/>");
                }
                else
                {
                    if (fiSt.Extension.ToLower() == ".png")
                    {
                        sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(".png", "").Replace(" ", "").Replace("-", "") + "\" href=\"images/" + fiSt.Name.ToString() + "\" media-type=\"image/png\"/>");
                    }
                    else
                    {
                        if (fiSt.Extension.ToLower() == ".gif")
                        {
                            sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(".gif", "").Replace(" ", "").Replace("-", "") + "\" href=\"images/" + fiSt.Name.ToString() + "\" media-type=\"image/gif\"/>");
                        }
                        else
                        {
                            sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(fiSt.Extension.ToString(), "").Replace(" ", "").Replace("-", "") + "\" href=\"images/" + fiSt.Name.ToString() + "\" media-type=\"application/unknown\"/>");
                        }
                    }
                }
            }



            diSt = new DirectoryInfo(strParentFolder + "\\xhtml");

            FileInfo[] fih = diSt.GetFiles("*.*");

            foreach (FileInfo fiSt in fih)
            {
                //MessageBox.Show(fiSt.Extension.ToLower());
                if (fiSt.Extension.ToLower() == ".xhtml")
                {
                    sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(".xhtml", "").Replace(" ", "").Replace("-", "") + "\" href=\"xhtml/" + fiSt.Name.ToString() + "\" media-type=\"application/xhtml+xml\"/>");
                }
                else
                {
                    sw.WriteLine("\t\t<item id=\"" + fiSt.Name.ToString().Replace(fiSt.Extension.ToString(), "").Replace(" ", "").Replace("-", "") + "\" href=\"xhtml/" + fiSt.Name.ToString() + "\" media-type=\"application/unknown\"/>");
                }
            }
            sw.WriteLine("\t\t<item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>"); 
            sw.WriteLine("\t</manifest>");
            sw.WriteLine("\t<spine toc=\"ncx\">\t");

            


            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {

                if (listView1.Items[i].Checked == true)
                {
                    sw.WriteLine("\t\t<itemref idref=\"" + listView1.Items[i].Text.ToString().Replace(".xhtml", "").Replace(" ", "").Replace("-", "") + "\"/>");
                }
            }

            diSt = new DirectoryInfo(strParentFolder + "\\xhtml");

            FileInfo[] fisp = diSt.GetFiles("*.xhtml");


            
            foreach (FileInfo fiSt in fisp)
            {

                StreamReader sr = new StreamReader(fiSt.FullName.ToString());

                string strMultiLines = sr.ReadToEnd();
                sr.Close();
                sr.Dispose(); 
                strMultiLines=strMultiLines.Replace("</title>\n</head>","</title>\n" + strStylePath + "</head>");

                StreamWriter swx = new StreamWriter(fiSt.FullName.ToString());
                swx.Write(strMultiLines);
                swx.Close();
                swx.Dispose(); 


            }
            

            sw.WriteLine("\t</spine>\n</package>"); 
            //sw.WriteLine("tst");
            sw.Close();
            MessageBox.Show("opf File created successfully","opf");  
            Application.UseWaitCursor = false;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            createMIME_Type();
            /*
            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {
                //sw.WriteLine("<div class=\"toc\"><a href=\"" + listView1.Items[i].Text + "\">" + listView1.Items[i].SubItems[1].Text + "</a></div>");
                if (listView1.Items[i].Checked == true)
                {
                    MessageBox.Show(listView1.Items[i].Text);
                }

            }*/
        }

        private void chkSelect_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSelect.Checked == true)
            {
                chkSelect.Text = "Deselect All";
                for (int i = 0; i <= listView1.Items.Count - 1; i++)
                {
                    listView1.Items[i].Checked = true;
                    
                }
            }
            else
            {
                chkSelect.Text = "Select All";
                for (int i = 0; i <= listView1.Items.Count - 1; i++)
                {
                    listView1.Items[i].Checked = false;

                }

            }
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            
        }

        private void btnMultiLevels_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= listView1.Items.Count - 1; i++)
            {
                if (listView1.Items[i].Checked == true)
                {
                    ListViewItem lvix = listView1.Items[i];
                    lvix.SubItems[2].Text = txtLevelRe.Text;
                }


            }

        }

        private void btnReassign_Click(object sender, EventArgs e)
        {
            ReassignNavPoint();
        }
       

       

       

    }
}