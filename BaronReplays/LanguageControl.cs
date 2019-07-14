using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace BaronReplays
{
    public class LanguageControl
    {
        public static ResourceDictionary LanguageResource;

        public static List<String> LanguageList = new List<string>() { "ar", "de", "en-US", "es", "es-AR", "fi", "fr", "hr", "id", "it", "nl", "pl", "pt", "ro", "ru", "sv", "tr", "vi", "zh-TW" };

        public static void ChangeLanguage(String lang)
        {
            ResourceDictionary rd = null;
            try
            {
                rd = Application.LoadComponent(new Uri("Lang/" + lang + ".xaml", UriKind.Relative)) as ResourceDictionary;
            }
            catch (Exception)
            {
                rd = Application.LoadComponent(new Uri("Lang/en-US.xaml", UriKind.Relative)) as ResourceDictionary;
                lang = "en-US";
            }

            if (rd != null)
            {
                int dictCount = Application.Current.Resources.MergedDictionaries.Count;
                //if (Application.Current.Resources.MergedDictionaries.Count > 0)
                //{
                //    Application.Current.Resources.MergedDictionaries.Clear();
                //}
                Application.Current.Resources.MergedDictionaries.Add(rd);
                if (Application.Current.Resources.MergedDictionaries.Contains(LanguageResource))
                {
                    Application.Current.Resources.MergedDictionaries.Remove(LanguageResource);
                }
                LanguageResource = rd;
                if (String.Compare(Properties.Settings.Default.Language, lang) != 0)
                {
                    Properties.Settings.Default.Language = lang;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public static void LoadLanguage()
        {
            //WriteLanguagesToFile();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                ChangeLanguage("zh-TW");
            }
            else if (Properties.Settings.Default.Language.Length != 0)
            {
                ChangeLanguage(Properties.Settings.Default.Language);
            }
            else
            {
                if (LanguageList.Contains(CultureInfo.CurrentCulture.Name))
                    ChangeLanguage(CultureInfo.CurrentCulture.Name);
                else
                    ChangeLanguage("en-US");
            }
        }

        public static void WriteLanguagesToFile()
        {
            using (StreamWriter sw = new StreamWriter("Lang.txt"))
            {
                foreach (String s in LanguageList)
                {
                    CultureInfo ci = new CultureInfo(s);
                    sw.WriteLine(ci.NativeName);

                }
                sw.Close();
            }
        }
    }

}
