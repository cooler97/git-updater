using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace GitUpdate
{
	public class Updater
	{
		static string DIR_SETTINGS = "update/setting";
		static string DIR_SQL = "update/sql";
		
		static string PATH_SETTINGS = "Setting";
		static string PATH_SQL = "Update";
		
		private string rootDirectory;
		
		private string lastSaveFilePath;
		
		private GitApi gitApi;
		private MainForm mainForm;
		
		private List<Content> settingList = new List<Content>();
		
		public Exception error = null;
		
		public Updater(string rootDirectory, string accessToken)
		{
			this.rootDirectory = rootDirectory;
			gitApi = new GitApi(accessToken, this);
		}
		
		public Exception GetError()
		{
			return error;
		}
		
		public List<Content> CheckSettingsUpdate()
		{
			return CheckUpdate(DIR_SETTINGS);
		}
		
		public List<Content> GetSettingsSql()
		{
			return CheckUpdate(DIR_SQL);
		}
		
		public Form GetMainForm(bool readme)
		{
			mainForm = new MainForm();
			
			if(readme)
				mainForm.SetDescription(GetReadme());
			
			return mainForm;
		}
		
		public string GetReadme()
		{
			Content readme = gitApi.GetReadme();
			
			if(readme == null || String.IsNullOrEmpty(readme.ContentString))
				return "Ошибка обращения к репозиторию, проблема с ключами?";
			
			byte[] data = System.Convert.FromBase64String(readme.ContentString);
			
			return System.Text.UTF8Encoding.UTF8.GetString(data);
		}
		
		protected List<Content> CheckUpdate(string dir)
		{
			List<Content> contentList = new List<Content>();
			
			try
			{
				contentList = gitApi.GetUpdate(dir);
				
				if(contentList == null)
				{
					return new List<Content>();
				}
			}
			catch(Exception e){
				error = e;
				Utils.SendErrorEmailToHelpDesk(e);
			}
			
			return contentList;
		}
		
		public bool DownloadFile(Content content, string basePath)
		{
			bool result = false;
			bool isFileExist = false;
			string path = basePath + "\\" + content.Name;
			string hashCode = String.Empty;
			byte[] data;
			
			lastSaveFilePath = path;
			
			try
			{
				if(File.Exists(path))
				{
					data = File.ReadAllBytes(path);
					isFileExist = true;
				}
				else
				{
					data = gitApi.Download(content.Path);
				}
				
				hashCode = ComputeHashCode(data, content.Size);
				
				if(isFileExist && !hashCode.Equals(content.Sha))
				{
					File.WriteAllBytes(path, data);
					result = true;
				}
				else if(!isFileExist && hashCode.Equals(content.Sha))
				{
					File.WriteAllBytes(path, data);
					result = true;
				}
				
			}
			catch (UnauthorizedAccessException e)
			{
				MessageBox.Show("Автоматическое обновление не смогло получить доступ к папке с программой, "
				                + "это произошло в следствии отсутсвия необходимых прав.\r\nДля корректной "
				                + "работы необходимо запустить программу с правами администратора ",
				                "Ошибка доступа",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Error);
				
				error = e;
				Utils.SendErrorEmailToHelpDesk(e);
			}
			catch (Exception e) {
				error = e;
				Utils.SendErrorEmailToHelpDesk(e);
			}
			
			return result;
		}
		
		public string GetLastSaveFilePath()
		{
			return lastSaveFilePath;
		}
		
		protected bool CheckAndSave(List<Content> list, string path) {
			bool result = false;
			foreach(Content c in list){
				
				if(c.Type != "file")
					continue;
				
				result = this.DownloadFile(c, path);
			}
			return result;
		}
		
		public bool CheckAndSaveSql(){
			if(!Directory.Exists(this.CombinePath(PATH_SQL)))
				Directory.CreateDirectory(this.CombinePath(PATH_SQL));
			return this.CheckAndSave(this.GetSettingsSql(), this.CombinePath(PATH_SQL));
		}
		
		public string CheckSetting(){
			List<Content> tempContent = this.CheckSettingsUpdate();
			settingList = new List<Content>();
			
			for(int count = tempContent.Count - 1; count >= 0; count--)
			{
				if(tempContent[count].Name == "readme.txt"){
					tempContent.Remove(tempContent[count]);
				}
			}
			
			if(tempContent.Count != 1)
				return String.Empty;
			
			settingList.Add(tempContent[0]);
			
			string fileSha = tempContent[0].Sha;
			
			return fileSha;
		}
		
		public bool SaveSetting(){
			if(!Directory.Exists(this.CombinePath(PATH_SETTINGS))){
				Directory.CreateDirectory(this.CombinePath(PATH_SETTINGS));
			}
			return this.CheckAndSave(settingList, this.CombinePath(PATH_SETTINGS));
		}
		
		public bool EnabledTlsSupport()
		{
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolTypeExtensions.Tls12;
			}
			catch(Exception e)
			{
				error = e;
				Utils.SendErrorEmailToHelpDesk(e);
				return false;
			}
			
			return true;
		}
		
		private string CombinePath(string path)
		{
			return this.rootDirectory + "\\" + path;
		}
		
		private string ComputeHashCode(byte[] data, int size)
		{
			if(data == null || data.Length == 0)
				return String.Empty;
			
			byte[] forCheckSum = Encoding.UTF8.GetBytes(String.Format("blob {0}\0", size));
			byte[] newArray = new byte[forCheckSum.Length + data.Length];
			Array.Copy(forCheckSum, newArray, forCheckSum.Length);
			Array.Copy(data, 0, newArray, forCheckSum.Length, data.Length);
			return Utils.ComputeCheckSum(newArray);
		}
	}
}
