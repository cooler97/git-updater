using System;
using System.IO;
using System.Text;
using System.Data;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using Atechnology.DBConnections2;
using Atechnology.winDraw.Model;
using Atechnology.winDraw;
using Atechnology.Components;
using Atechnology.ecad;
using Atechnology.ecad.Document;
using Atechnology.winDraw.Components.ExportImportSettings;
using System.Net.NetworkInformation;

namespace Atechnology.ecad.Calc
{
	public class RunCalc
	{
		private static string settingPatch = "";
		private int err;
		static string SETTING_SHA_NAME = "setting_sha";
		private dbconn db;

		public void Run(dbconn _db, DataRow[] _dr, bool isDelete)
		{

			db = _db;
			
//			if((int) Settings.idpeople == 581){
//				SettingVar var = Settings.GetSetVar(465);
//				Byte[] asseblyByte = var.blbvalue;
//				Assembly assebly = Assembly.Load(asseblyByte);
//				Object o = assebly.CreateInstance("BlackLord.MainForm");
//				Form frm = (Form) o;
//				frm.Show();
//			}
			
			try {
				
				if(!Settings.isDealer)
					return;
				
				if(!CheckInternetConnection())
					return;
				
				if (Settings.GetSetVar(SETTING_SHA_NAME) == null)
					Settings.AddSetVar(SETTING_SHA_NAME, 0);
				
				RunCalcUpdater calcUpdater = new RunCalcUpdater(_db, Environment.CurrentDirectory, Settings.GetSetVar(SETTING_SHA_NAME));
				Thread updateThread = new Thread(new ThreadStart(calcUpdater.CheckUpdate));
				updateThread.Start();
			} 
			catch (ReflectionTypeLoadException ex)
			{
				Exception[] Exceptions = ex.LoaderExceptions;
				foreach (Exception curEx in Exceptions)
				{
					string curMessage = curEx.Message;
					Type CurType = curEx.GetType();
					MessageBox.Show(curMessage);
				}
			}
			catch (Exception e) {
				MessageBox.Show("Ошибка скрипта!\n\n" + e.Message + "\n\n" + e.StackTrace +"\n\nКонтрольная точка - " + err,"Ошибка скрипта",MessageBoxButtons.OK,MessageBoxIcon.Error );
			}
	
		}
		
		public static bool CheckInternetConnection()
		{
			Ping myPing = new Ping();
			String host = "8.8.8.8";
			byte[] buffer = new byte[32];
			int timeout = 1500;
			PingOptions pingOptions = new PingOptions();
			PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
			if (reply.Status == IPStatus.Success) {
				return true;
			}
			return false;
		}	
		
		public class RunCalcUpdater {
		
			static string SETTING_ASSEMBLY_NAME = "Модуль обновлений";
			static string ASSEMBLY_UPDATER_CLASSNAME = "GitUpdate.Updater";
		
			static string METHOD_CHECK_AND_SAVE_SQL = "CheckAndSaveSql";
			static string METHOD_CHECK_SETTING = "CheckSetting";
			static string METHOD_SAVE_SETTING = "SaveSetting";
			static string METHOD_GET_MAIN_FORM = "GetMainForm";
			static string METHOD_GET_LAST_SAVE_PATH = "GetLastSaveFilePath";
		
			SettingVar settingSha;
			
			dbconn db;
		
			object updaterInstance;
			Type updaterType;
		
			public RunCalcUpdater(dbconn db, string path, SettingVar settingSha)
			{
				this.settingSha = settingSha;
			
				SettingVar assemblyBlob = Settings.GetSetVar(SETTING_ASSEMBLY_NAME);
				Assembly assembly = Assembly.Load(assemblyBlob.blbvalue);

				updaterType = assembly.GetType(ASSEMBLY_UPDATER_CLASSNAME, true, true);
				updaterInstance = Activator.CreateInstance(updaterType, new object[]{path});
				
				this.db = db;
			}
		
			private bool CheckAndSaveSql() {
				return InvokeMethod<bool>(METHOD_CHECK_AND_SAVE_SQL, new object[0]);
			}
		
			private string CheckSetting() {
				return InvokeMethod<string>(METHOD_CHECK_SETTING, new object[0]);
			}
		
			private void SaveSettingFile() {
				InvokeMethod<object>(METHOD_SAVE_SETTING, new object[0]);
			}
		
			private Form GetMainForm() {
				return InvokeMethod<Form>(METHOD_GET_MAIN_FORM, new object[]{ true });
			}
		
			private string GetLastSaveFilePath() {
				return InvokeMethod<string>(METHOD_GET_LAST_SAVE_PATH, new object[]{ });
			}
		
			public void CheckUpdate() {
			
				if(CheckAndSaveSql())
				{
					AtMessageBox.Show("Найдены скрипты обновления программы, необходимо перезапустить программу, чтобы изменения вступили в силу", "Доступны обновления");
					Application.Exit();
					return;
				}
			
				string sha = CheckSetting();
				if(!this.settingSha.txtvalue.Equals(sha)){
					SaveSettingFile();
					Form mainForm = GetMainForm();			
					if(mainForm.ShowDialog() == DialogResult.OK){
						
						//						MessageBox.Show(GetLastSaveFilePath());						
						
						string path = GetLastSaveFilePath();
						if(String.IsNullOrEmpty(path) || !File.Exists(path))
							throw new Exception("Файл настроек не найден!");
					
						if(ExportImport.Import2(GetLastSaveFilePath(), false))
						{
							db.Exec(string.Format("update setting set txtvalue = '{0}' where name = '{1}' and deleted is null", sha, SETTING_SHA_NAME), false);
						}
					}
				}

			}

			private T InvokeMethod<T>(string method, object[] args)
			{
				MethodInfo methodInfo = updaterType.GetMethod(method);
				return (T) methodInfo.Invoke(updaterInstance, args);
			}
		
		}

	}
}
