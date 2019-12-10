using System;
using System.Collections.Generic;
using System.IO;
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
        
        public Updater(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
            gitApi = new GitApi();
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
            byte[] data = System.Convert.FromBase64String(readme.ContentString);
            return System.Text.UTF8Encoding.UTF8.GetString(data);
        }
        
        protected List<Content> CheckUpdate(string dir)
        {
            List<Content> contentList = new List<Content>();
            
            try
            {
                contentList = gitApi.GetUpdate(dir);
            }
            catch(Exception e ){
                MessageBox.Show(e.Message + "\\n" + e.InnerException);
            }
            
            return contentList;
        }
        
        public bool SaveDownloadedFile(Content content, string basePath)
        {
            bool result = false;
            string path = basePath + "\\" + content.Name;
            string checkSum = String.Empty;
            byte[] data;
            
            try
            {
                if(File.Exists(path)){
                    data = File.ReadAllBytes(path);
                    checkSum = ComputeHashCode(data, content.Size);
                    if(!checkSum.Equals(content.Sha))
                    {
                        data = gitApi.Download(content.Path);
                        File.WriteAllBytes(path, data);
                        result = true;
                    }
                }
                else
                {
                    data = gitApi.Download(content.Path);
                    checkSum = ComputeHashCode(data, content.Size);
                    if(checkSum.Equals(content.Sha))
                    {
                        File.WriteAllBytes(path, data);
                        result = true;
                    }
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.Message + "\\n" + e.InnerException);
            }
            
            lastSaveFilePath = path;
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
                
                result = this.SaveDownloadedFile(c, path);
            }
            return result;
        }
        
        public bool CheckAndSaveSql(){
            if(!Directory.Exists(this.CombinePath(PATH_SQL)))
                return false;
            return this.CheckAndSave(this.GetSettingsSql(), this.CombinePath(PATH_SQL));
        }
        
        public string CheckSetting(){
            settingList = this.CheckSettingsUpdate();
            
            for(int count = settingList.Count - 1; count >= 0; count--)
            {
                if(settingList[count].Name == "readme.txt"){
                    settingList.Remove(settingList[count]);
                }
            }
            
            if(settingList.Count != 1)
                return null;
            
            return settingList.ToArray()[0].Sha;
        }
        
        public bool SaveSetting(){
            if(!Directory.Exists(this.CombinePath(PATH_SETTINGS))){
                Directory.CreateDirectory(this.CombinePath(PATH_SETTINGS));
            }
            return this.CheckAndSave(settingList, this.CombinePath(PATH_SETTINGS));
        }
        
        private string CombinePath(string path)
        {
            return this.rootDirectory + "\\" + path;
        }
        
        private string ComputeHashCode(byte[] data, int size)
        {
            byte[] forCheckSum = Encoding.UTF8.GetBytes(String.Format("blob {0}\0", size));
            byte[] newArray = new byte[forCheckSum.Length + data.Length];
            Array.Copy(forCheckSum, newArray, forCheckSum.Length);
            Array.Copy(data, 0, newArray, forCheckSum.Length, data.Length);
            return Utils.ComputeCheckSum(newArray);
        }
    }
}
