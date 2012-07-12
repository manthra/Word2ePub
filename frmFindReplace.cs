using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ORC
{
    public partial class frmFindReplace : Form
    {
        public frmFindReplace(RichTextBox rtbContent)
        {
            InitializeComponent();
            _rtbContent = rtbContent;
        }

        // Declare the regex and match as class level variables
        // to make happen find next
        private Regex regex;
        private Match match;
        //frmORC frmorc;

        private RichTextBox _rtbContent;

        // variable to indicate finding first time 
        // or is it a find next
        private bool isFirstFind = true;
        private void replaceAllButton_Click(object sender, EventArgs e)
        {
            Regex replaceRegex = GetRegExpression();
            String replacedString;

            

            //TextBox parentTextBox = ((Form)this.Owner)

            // get the current SelectionStart
            int selectedPos = _rtbContent.SelectionStart;

            // get the replaced string
            replacedString = replaceRegex.Replace(_rtbContent.Text, replaceTextBox.Text);

            // Is the text changed?
            if (_rtbContent.Text != replacedString)
            {
                // then replace it
                _rtbContent.Text = replacedString;
                MessageBox.Show("Replacements are made.   ", Application.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // restore the SelectionStart
                _rtbContent.SelectionStart = selectedPos;
            }
            else // inform user if no replacements are made
            {
                MessageBox.Show(String.Format("Cannot find '{0}'.   ", searchTextBox.Text),
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            _rtbContent.Focus();
        }



        // This function makes and returns a RegEx object
        // depending on user input
        private Regex GetRegExpression()
        {
            Regex result;
            String regExString;

            // Get what the user entered
            regExString = searchTextBox.Text;

            if (useRegulatExpressionCheckBox.Checked)
            {
                // If regular expressions checkbox is selected,
                // our job is easy. Just do nothing
            }
            // wild cards checkbox checked
            else if (useWildcardsCheckBox.Checked)
            {
                regExString = regExString.Replace("*", @"\w*");     // multiple characters wildcard (*)
                regExString = regExString.Replace("?", @"\w");      // single character wildcard (?)

                // if wild cards selected, find whole words only
                regExString = String.Format("{0}{1}{0}", @"\b", regExString);
            }
            else
            {
                // replace escape characters
                regExString = Regex.Escape(regExString);
            }

            // Is whole word check box checked?
            if (matchWholeWordCheckBox.Checked)
            {
                regExString = String.Format("{0}{1}{0}", @"\b", regExString);
            }

            // Is match case checkbox checked or not?
            if (matchCaseCheckBox.Checked)
            {
                result = new Regex(regExString);
            }
            else
            {
                result = new Regex(regExString, RegexOptions.IgnoreCase);
            }

            return result;
        }


        // Click event handler of find button
        private void findButton_Click(object sender, EventArgs e)
        {
            FindText();
        }

        // finds the text in searchTextBox in rtbContent
        private void FindText()
        {
            // Is this the first time find is called?
            // Then make instances of RegEx and Match
            if (isFirstFind)
            {
                regex = GetRegExpression();
                match = regex.Match(_rtbContent.Text);
                isFirstFind = false;
            }
            else
            {
                // match.NextMatch() is also ok, except in Replace
                // In replace as text is changing, it is necessary to
                // find again
                //match = match.NextMatch();
                match = regex.Match(_rtbContent.Text, match.Index + 1);
            }

            // found a match?
            if (match.Success)
            {
                // then select it
                _rtbContent.SelectionStart = match.Index;
                _rtbContent.SelectionLength = match.Length;
                _rtbContent.Focus();
                _rtbContent.ScrollToCaret();
                
            }
            else // didn't find? bad luck.
            {
                MessageBox.Show(String.Format("Cannot find '{0}'.   ", searchTextBox.Text),
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                isFirstFind = true;
            }
        }


        // Click event handler of replaceButton
        private void replaceButton_Click(object sender, EventArgs e)
        {
            // Make a local RegEx and Match instances
            Regex regexTemp = GetRegExpression();
            Match matchTemp = regexTemp.Match(_rtbContent.SelectedText);

            if (matchTemp.Success)
            {
                // check if it is an exact match
                if (matchTemp.Value == _rtbContent.SelectedText)
                {
                    _rtbContent.SelectedText = replaceTextBox.Text;
                }
            }

            FindText();
        }

        // TextChanged event handler of searchTextBox
        // Set isFirstFind to true, if text changes
        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            isFirstFind = true;
        }

        // CheckedChanged event handler of matchWholeWordCheckBox
        // Set isFirstFind to true, if check box is checked or unchecked
        private void matchWholeWordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            isFirstFind = true;
        }

        // CheckedChanged event handler of matchCaseCheckBox
        // Set isFirstFind to true, if check box is checked or unchecked
        private void matchCaseCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            isFirstFind = true;
        }

        // CheckedChanged event handler of useWildcardsCheckBox
        // Set isFirstFind to true, if check box is checked or unchecked
        private void useWildcardsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            isFirstFind = true;
        }


        private void useRegulatExpressionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            isFirstFind = true;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        

    }
}
