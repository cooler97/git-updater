using System;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

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
        
    }
}
