using System;
using System.IO;

namespace Claysys_Online_Course_Learning_portal.Utilities
{
    public static class Logger
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog.txt");

        public static void LogError(Exception ex)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine("Date: " + DateTime.Now.ToString());
                    writer.WriteLine("Message: " + ex.Message);
                    writer.WriteLine("StackTrace: " + ex.StackTrace);
                    writer.WriteLine("-------------------------------------------------------");
                }
            }
            catch (Exception loggingEx)
            {
                Console.WriteLine("Error logging exception: " + loggingEx.Message);
            }
        }
    }
}
