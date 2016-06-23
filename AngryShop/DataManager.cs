using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Xml.Serialization;
using AngryShop.Entities;
using AngryShop.Helpers;
using AngryShop.Items;

namespace AngryShop
{
    /// <summary>
    /// This class manages all configuration data 
    /// </summary>
    public static class DataManager
    {
        /// <summary> Data from "Config" file </summary>
        public static Configuration Configuration { get; set; }

        /// <summary> "Stop" or ignored words that are not to be shown in list of words </summary>
        public static List<string> IgnoredWords { get; set; }

        /// <summary> Process ID for our program </summary>
        public static int ThisProcessId { get; set; }

        /// <summary> Last focused control (we need this to send new text to this control) </summary>
        public static AutomationElement LastAutomationElement { get; set; }

        /// <summary> Last focused control text (we need this because LastAutomationElement doesn't always contain the whole text string but only 4096 symbols) </summary>
        public static string LastAutomationElementText { get; set; }


        /// <summary> Opens ignored words file </summary>
        public static void OpenIgnoredWordsFile()
        {
            var path = getPathToIgnoredWordsFile();
            if (File.Exists(path))
            {
                try
                {
                    using (var sr = new StreamReader(path))
                    {
                        string text = sr.ReadToEnd();
                        IgnoredWords = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(@"Could not read the ""Ignored words"" file.\r\n{0}", ex.Message));
                    IgnoredWords = new List<string>();
                }
            }
            else
            {
                IgnoredWords = new List<string>();
            }
        }

        public static void SaveIgnoredWords()
        {
            IgnoredWords = IgnoredWords.OrderBy(p => p).ToList();
            var strb = new StringBuilder();
            foreach (var word in IgnoredWords)
            {
                strb.AppendFormat("{0}\n", word);
            }
            var path = getPathToIgnoredWordsFile();
            File.WriteAllText(path, strb.ToString());
        }

        /// <summary> Opens instance of Configuration </summary>
        public static void OpenConfiguration()
        {
            string path = getPathToConfigFileName();
            if (!File.Exists(path))
            {
                Configuration = new Configuration();
                Configuration.SetToDefaultValues();
                SaveConfiguration();
            }
            else
            {
                try
                {
                    var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);

                    byte[] key = { 0x01, 0x05, 0xA7, 0xC6, 0x09, 0x39, 0xF4, 0x10, 0xD1, 0xA9, 0xC4, 0xA8, 0xAA, 0xCE, 0x9A, 0xBF };
                    byte[] iv = { 0xF9, 0xC5, 0x37, 0xF1, 0x91, 0x13, 0x28, 0x10, 0x08, 0x23, 0x07, 0xAC, 0xE2, 0xB1, 0x11, 0x80 };

                    var rmCrypto = new RijndaelManaged();
                    var crypto = new CryptoStream(fileStream, rmCrypto.CreateDecryptor(key, iv), CryptoStreamMode.Read);
                    var zipStream = new GZipStream(crypto, CompressionMode.Decompress);
                    var serializer = new XmlSerializer(typeof(Configuration));
                    Configuration = (Configuration)serializer.Deserialize(zipStream);
                    zipStream.Close();
                    crypto.Close();
                    fileStream.Close();
                }
                catch (Exception e)
                {
                    LogHelper.SaveError(e);
                    Configuration = new Configuration();
                    Configuration.SetToDefaultValues();
                    SaveConfiguration();
                }
            }
            Configuration.NeedsSaving = false;
        }


        /// <summary> Saves instance of Configuration </summary>
        public static void SaveConfiguration()
        {
            try
            {
                string path = getPathToConfigFileName();

                var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

                byte[] key = { 0x01, 0x05, 0xA7, 0xC6, 0x09, 0x39, 0xF4, 0x10, 0xD1, 0xA9, 0xC4, 0xA8, 0xAA, 0xCE, 0x9A, 0xBF };
                byte[] iv = { 0xF9, 0xC5, 0x37, 0xF1, 0x91, 0x13, 0x28, 0x10, 0x08, 0x23, 0x07, 0xAC, 0xE2, 0xB1, 0x11, 0x80 };

                var rmCrypto = new RijndaelManaged();
                var crypto = new CryptoStream(fileStream, rmCrypto.CreateEncryptor(key, iv), CryptoStreamMode.Write);
                var zipStream = new GZipStream(crypto, CompressionMode.Compress);
                var serializer = new XmlSerializer(typeof(Configuration));
                serializer.Serialize(zipStream, Configuration);
                zipStream.Close();
                crypto.Close();
                fileStream.Close();
                Configuration.NeedsSaving = false;
            }
            catch (Exception e)
            {
                LogHelper.SaveError(e);
            }
        }



        private static string getPathToConfigFileName()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ConfigFileName);
        }

        private static string getPathToIgnoredWordsFile()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.IgnoredWordsFileName);
        }
    }
}
