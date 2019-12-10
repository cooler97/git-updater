using System;

using System.Windows.Forms;

namespace GitUpdate
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        void Label2Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        void Label1Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            //            Utils.CheckInternetConnection();
//
            //                        Updater upd = new Updater("C:\\");
            //                        this.descriptionBox.Text = upd.GetReadme();
            //                        upd.CheckSetting();
            //                        upd.SaveSetting();
        }
        
        public void SetDescription(string text)
        {
            this.descriptionBox.Text = text;
        }
    }
}
