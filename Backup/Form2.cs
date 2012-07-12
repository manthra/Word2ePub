using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ORC
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            clsStaticVrs.setID(textBox1.Text);    
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            textBox1.Text = clsStaticVrs.getID();
        }
    }
}