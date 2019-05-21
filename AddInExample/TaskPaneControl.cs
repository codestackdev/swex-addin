using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using CodeStack.SwEx.Common.Attributes;
using CodeStack.SwEx.AddIn.Example.Properties;

namespace CodeStack.SwEx.AddIn.Example
{
    [ComVisible(true)]
    [Icon(typeof(Resources), nameof(Resources.command_group_icon))]
    public partial class TaskPaneControl : UserControl
    {
        public TaskPaneControl()
        {
            InitializeComponent();
        }

        private void OnSendMessage(object sender, EventArgs e)
        {
            MessageBox.Show(txtText.Text);
        }
    }
}
