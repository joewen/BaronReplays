using BaronReplays.RiotAPI.Services;
using BaronReplays.RiotAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace BaronReplays.LoLStaticData
{
    public class Champion : Base
    {
        private static object objLock = new object();
        protected static Champion instance;
        public static ChampionListDto ChampionList;
        protected Dictionary<Int32, String> IdToKey = new Dictionary<int, string>();
        protected Dictionary<String, String> KeyToName = new Dictionary<string, string>();

        Champion()
            : base(@"Champions\")
        {

        }

        protected override bool NeedToUpdate()
        {
            try
            {
                var oldVersion = ChampionList.version;
                ChampionList = Request.GetStaticData("na/v1.2/champion?champData=image&locale=" + Request.ApiLanguage, typeof(ChampionListDto), DirectoryPath + InfoFile);
                return ChampionList.version.CompareTo(oldVersion) != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override bool UpdateInfo()
        {
            ChampionList = Request.GetStaticData("na/v1.2/champion?champData=image&locale=" + Request.ApiLanguage, typeof(ChampionListDto), DirectoryPath + InfoFile);
            return ChampionList != null;
        }

        protected override void ReadInfo()
        {
            ChampionList = Request.ReadFromFile(DirectoryPath + InfoFile, typeof(ChampionListDto));
            DecodeData();
        }

        protected override void DecodeData()
        {
            Dictionary<String, BitmapSource> ImagesBoard = ReadImages();
            IdToKey.Clear();
            KeyToName.Clear();
            foreach (ChampionDto champion in ChampionList.data.Values)
            {
                try
                {
                    ImageDto image = champion.image;
                    IdToKey.Add(champion.id, champion.key);
                    KeyToName.Add(champion.key, champion.name);
                    string imagePath = DirectoryPath + champion.image.sprite;
                    if (!ImagesBoard.ContainsKey(imagePath))
                    {
                        String url = String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/sprite/{1}",ChampionList.version, champion.image.sprite);
                        File.Delete(imagePath);
                        Utilities.DownloadFile(url, imagePath);
                        ImagesBoard.Add(imagePath, Utilities.GetBitmapImage(imagePath));
                    }
                    ImageSource cropped = new CroppedBitmap(ImagesBoard[imagePath], new Int32Rect(image.x + 3, image.y + 3, image.w - 6, image.h - 6));
                    cropped.Freeze();   //一定要Freeze，因為跨Thread
                    if (Images.ContainsKey(champion.key))
                        Images.Remove(champion.key);
                    Images.Add(champion.key, cropped);
                }
                catch (Exception)
                {

                }
            }
        }

        public ImageSource GetImageByKey(String name)
        {
            if (Images.ContainsKey(name))
                return Images[name];
            return null;
        }

        public String GetKeyById(int id)
        {
            if (IdToKey.ContainsKey(id))
                return IdToKey[id];
            return "Unknow";
        }

        public String GetNameByKey(String key)
        {
            if (KeyToName.ContainsKey(key))
                return KeyToName[key];
            return "Unknow";
        }

        public String GetNameById(int id)
        {
            return GetNameByKey(GetKeyById(id));
        }


        public static Champion Instance
        {                               //https://msdn.microsoft.com/en-us/library/ff650316.aspx
            get
            {
                if (instance == null)
                {
                    lock (objLock)
                    {
                        if (instance == null)
                        {
                            instance = new Champion();
                        }

                    }
                }
                return instance;
            }
        }

    }

}
