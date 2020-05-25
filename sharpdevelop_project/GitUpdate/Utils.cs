using System;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Atechnology.ecad;

namespace GitUpdate
{
	public static class Utils
	{

		public static string ComputeCheckSum(byte[] rawData)
		{
			using (SHA1 sha = SHA1.Create())
			{
				StringBuilder builder = new StringBuilder();
				byte[] hash = sha.ComputeHash(rawData);
				for (int i = 0; i < hash.Length; i++)
				{
					builder.Append(hash[i].ToString("x2"));
				}
				return builder.ToString();
			}
		}
		
		public static bool CheckInternetConnection()
		{
			Ping myPing = new Ping();
			String host = "8.8.8.8";
			byte[] buffer = new byte[32];
			int timeout = 1000;
			PingOptions pingOptions = new PingOptions();
			PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
			if (reply.Status == IPStatus.Success) {
				return true;
			}
			return false;
		}
		
		public static bool SendEmail(string _login, string _password, string _from, string _to, string _subject, string _body)
		{
			try
			{
				MailAddress from = new MailAddress(_from);
				MailAddress to = new MailAddress(_to);
				MailMessage m = new MailMessage(from, to);

				m.Subject = _subject;
				m.Body = _body;

				SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587);

				smtp.Credentials = new NetworkCredential(_login, _password);
				smtp.EnableSsl = true;
				smtp.Send(m);
				
			} catch (Exception e) {
				return false;
			}
			
			return true;
		}
		
		public static void SendErrorEmailToHelpDesk(string error)
		{
			string login = "updatemodule@sarokna.ru";
			string pass = "123Qwerty";
			string subject = "AutoUpdate Failure Information";
			string to = "helpdesk@sarokna.ru";
			string userLogin = (Settings.People == null) ? "Unknown login" : Settings.People.Login;
			string body = String.Format(@"Login: {0} "
			                            + "OS Version: {1} "
			                            + "Exception: {2}", userLogin, getOSInfo(), error);
			
			SendEmail(login, pass, login, to, subject, body);
		}
		
		public static void SendErrorEmailToHelpDesk(Exception e)
		{
			string exceptionText = e.Message 
				+ Environment.NewLine 
				+ e.InnerException 
				+ Environment.NewLine 
				+ e.StackTrace 
				+ Environment.NewLine
				+ e.Source;
			SendErrorEmailToHelpDesk(exceptionText);
		}
		
		static string getOSInfo()
		{
			//Get Operating system information.
			OperatingSystem os = Environment.OSVersion;
			//Get version information about the os.
			Version vs = os.Version;

			//Variable to hold our return value
			string operatingSystem = "";

			if (os.Platform == PlatformID.Win32Windows)
			{
				//This is a pre-NT version of Windows
				switch (vs.Minor)
				{
					case 0:
						operatingSystem = "95";
						break;
					case 10:
						if (vs.Revision.ToString() == "2222A")
							operatingSystem = "98SE";
						else
							operatingSystem = "98";
						break;
					case 90:
						operatingSystem = "Me";
						break;
					default:
						break;
				}
			}
			else if (os.Platform == PlatformID.Win32NT)
			{
				switch (vs.Major)
				{
					case 3:
						operatingSystem = "NT 3.51";
						break;
					case 4:
						operatingSystem = "NT 4.0";
						break;
					case 5:
						if (vs.Minor == 0)
							operatingSystem = "2000";
						else
							operatingSystem = "XP";
						break;
					case 6:
						if (vs.Minor == 0)
							operatingSystem = "Vista";
						else if (vs.Minor == 1)
							operatingSystem = "7";
						else if (vs.Minor == 2)
							operatingSystem = "8";
						else
							operatingSystem = "8.1";
						break;
					case 10:
						operatingSystem = "10";
						break;
					default:
						break;
				}
			}
			//Make sure we actually got something in our OS check
			//We don't want to just return " Service Pack 2" or " 32-bit"
			//That information is useless without the OS version.
			if (operatingSystem != "")
			{
				//Got something.  Let's prepend "Windows" and get more info.
				operatingSystem = "Windows " + operatingSystem;
				//See if there's a service pack installed.
				if (os.ServicePack != "")
				{
					//Append it to the OS name.  i.e. "Windows XP Service Pack 3"
					operatingSystem += " " + os.ServicePack;
				}
				//Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
				//operatingSystem += " " + getOSArchitecture().ToString() + "-bit";
			}
			//Return the information we've gathered.
			return operatingSystem;
		}
		
	}
}
