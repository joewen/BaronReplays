using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BaronReplays.LoLStaticData
{
    public abstract class Base
    {
        public delegate void DataUpdatedDelegate(Base sender);
        public event DataUpdatedDelegate DataUpdated;

        protected String DirectoryPath = "";
        protected String InfoFile = "info.txt";
        protected dynamic Images;


        public Base(String directoryPath)
        {
            DirectoryPath = directoryPath;
            Utilities.IfDirectoryNotExitThenCreate(DirectoryPath);
            Images = new Dictionary<object, ImageSource>();
            if (File.Exists(DirectoryPath + InfoFile))
            {
                ReadInfo();
                CheckUpdateAsync();
            }
            else
            {
                ForceRenewAsync();
            }
        }

        public async void ForceRenewAsync()
        {
            await Task.Factory.StartNew(ForceRenew);
        }

        private object renewLock = new object();
        public void ForceRenew()
        {
            if (UpdateInfo())
            {
                DeleteImages();
                DecodeData();
                DataUpdated?.Invoke(this);
            }
        }

        public async void CheckUpdateAsync()
        {
            await Task.Factory.StartNew(CheckUpdate);
        }

        public void CheckUpdate()
        {
            if (NeedToUpdate())
            {
                ForceRenew();
            }
        }

        private void DeleteImages()
        {
            String[] pngFiles = Directory.GetFiles(DirectoryPath, "*.png");
            foreach (String s in pngFiles)
            {
                try
                {
                    File.Delete(s);
                }
                catch (Exception e)
                {
                    Logger.Instance.WriteLog(e.Message);
                }
            }
        }

        protected abstract bool NeedToUpdate();
        public abstract bool UpdateInfo();

        protected abstract void ReadInfo();
        protected abstract void DecodeData();

        protected Dictionary<String, BitmapSource> ReadImages()
        {
            Dictionary<String, BitmapSource> result = new Dictionary<String, BitmapSource>();
            String[] pngFiles = Directory.GetFiles(DirectoryPath, "*.png");
            foreach (String s in pngFiles)
            {
                BitmapImage image = Utilities.GetBitmapImage(s);
                if (image == null)
                {
                    File.Delete(s);
                }
                else
                {
                    result.Add(s, image);
                }
            }
            return result;
        }
    }
}
