using System;
using System.IO;

namespace LegacyBanking.Business
{
    public static class LegacyLogger
    {
        private static readonly string LogFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "legacy.log");

        public static void Info(string message)
        {
            File.AppendAllText(
                LogFile,
                DateTime.Now + " INFO " + message + Environment.NewLine);
        }

        public static void Error(Exception ex)
        {
            File.AppendAllText(
                LogFile,
                DateTime.Now + " ERROR " + ex + Environment.NewLine);
        }
    }
}
