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

        /// <summary> "Common" or "stop" words that are not to be shown in list of words </summary>
        public static List<string> CommonWords { get; set; }

        public static int ThisProcessId { get; set; }
        //public static int LastProcessId { get; set; }

        public static AutomationElement LastAutomationElement { get; set; }
        //public static string LastAutomationName { get; set; }


        /// <summary> Opens common words </summary>
        public static void OpenCommonWords()
        {
            var path = getPathToCommonWordsFile();
            if (File.Exists(path))
            {
                try
                {
                    using (var sr = new StreamReader(path))
                    {
                        string text = sr.ReadToEnd();
                        CommonWords = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format(@"Could not read the ""Common words"" file.\r\n{0}", ex.Message));
                    CommonWords = new List<string>();
                }
            }
            else
            {
                CommonWords = new List<string>();
            }
        }

        public static void SaveCommonWords()
        {
            var strb = new StringBuilder();
            foreach (var commonWord in DataManager.CommonWords)
            {
                strb.AppendFormat("{0}\n", commonWord);
            }
            var path = getPathToCommonWordsFile();
            File.WriteAllText(path, strb.ToString());
        }

        /// <summary> Opens instance of Configuration </summary>
        public static void OpenConfiguration()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ConfigFileName);
            if (!File.Exists(path))
            {
                Configuration = new Configuration();
                Configuration.SetToDefaultCommonValues();
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
                }
            }
            Configuration.NeedsSaving = false;
        }


        /// <summary> Saves instance of Configuration </summary>
        public static void SaveConfiguration()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.ConfigFileName);

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

        private static string getPathToCommonWordsFile()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Constants.CommonWordsFileName);
        }
    }
}
