using System;
using System.Globalization;
using System.IO;

namespace AngryShop.Helpers
{
    class LogHelper
    {
        public static void SaveError(Exception e)
        {
            var text = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "log.txt", true);
            text.WriteLine("----------" + DateTime.Now.ToString(CultureInfo.InvariantCulture) + "-----------");
            text.WriteLine(e.Message);
            text.WriteLine(e.StackTrace);
            text.WriteLine("----------------------------------------------------------------------------------------------------");
            text.Close();
        }
    }
}
