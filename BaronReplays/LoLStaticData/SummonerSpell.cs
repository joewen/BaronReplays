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
    public class SummonerSpell:Base
    {
        private static object objLock = new object();
        protected static SummonerSpell instance;
        public static SummonerSpellListDto SummonerSpellList;
        private static Dictionary<Int32, String> NumberToKey = new Dictionary<int,string>();
        SummonerSpell()
            : base(@"Spells\")
        {

        }

        protected override bool NeedToUpdate() 
        {
            return false;   //招喚師技能其實很少更新...有更新再從BR下載換就好
        }

        public override bool UpdateInfo()
        {
            SummonerSpellList = BaronReplays.RiotAPI.Services.Request.GetStaticData("na/v1.2/summoner-spell?spellData=image&locale=" + Request.ApiLanguage, typeof(SummonerSpellListDto), DirectoryPath + InfoFile);
            return SummonerSpellList != null;
        }

        protected override void ReadInfo()
        {
            SummonerSpellList = BaronReplays.RiotAPI.Services.Request.ReadFromFile(DirectoryPath + InfoFile, typeof(SummonerSpellListDto));
            DecodeData();
        }


        protected override void DecodeData()
        {
            Dictionary<String, BitmapSource> ImagesBoard = ReadImages();
            NumberToKey.Clear();
            foreach (SummonerSpellDto spell in SummonerSpellList.data.Values)
            {
                try
                {
                    ImageDto image = spell.image;
                    NumberToKey.Add(spell.id, spell.key);
                    string imagePath = DirectoryPath + spell.image.sprite;
                    if (!ImagesBoard.ContainsKey(imagePath))
                    {
                        String url = String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/sprite/{1}", SummonerSpellList.version, spell.image.sprite);
                        File.Delete(imagePath);
                        Utilities.DownloadFile(url, imagePath);
                        ImagesBoard.Add(imagePath, Utilities.GetBitmapImage(imagePath));
                    }
                    ImageSource cropped = new CroppedBitmap(ImagesBoard[imagePath], new Int32Rect(image.x, image.y, image.w, image.h));
                    cropped.Freeze();   //一定要Freeze，因為跨Thread
                    if (Images.ContainsKey(spell.key))
                        Images.Remove(spell.key);
                    Images.Add(spell.key, cropped);
                }
                catch (Exception)
                {

                }
            }
        }

        public ImageSource GetImageByName(String name)
        {
            if (Images.ContainsKey(name))
                return Images[name];
            return null;
        }

        public ImageSource GetImageById(int id)
        {
            if (NumberToKey.ContainsKey(id))
                return GetImageByName(NumberToKey[id]);
            return null;
        }

        public String GetKeyById(int id)
        {
            if (NumberToKey.ContainsKey(id))
                return NumberToKey[id];
            return "Unknow Champion";
        }

        public static SummonerSpell Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (objLock)
                    {
                        if (instance == null)
                        {
                            instance = new SummonerSpell();
                        }
                    }
                }
                return instance;
            }
        }
    }

}
