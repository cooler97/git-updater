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
		
		public void SetDescription(string text)
		{
			descriptionBox.Text = text;
		}
		
		void Label2Click(object sender, EventArgs e)
		{
			this.Close();
		}
		
		void Label1Click(object sender, EventArgs e)
		{
			
			/*Updater upd = new Updater("C:\\", "ACCESS_TOKEN_NEED_HERE");
			
			upd.EnabledTlsSupport();
			
			upd.CheckSetting();
			upd.SaveSetting();*/
			
			DialogResult = DialogResult.OK;
		}
	}

}
