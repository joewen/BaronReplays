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
    public class Item:Base
    {
        private static object objLock = new object();
        protected static Item instance;
        public ItemListDto ItemList;

        Item()
            : base(@"Items\")
        {

        }

        protected override bool NeedToUpdate()
        {
            try
            {
                var oldVersion = ItemList.version;
                ItemList = BaronReplays.RiotAPI.Services.Request.GetStaticData("na/v1.2/item?itemListData=image&locale=" + Request.ApiLanguage, typeof(ItemListDto), DirectoryPath + InfoFile);
                return ItemList.version.CompareTo(oldVersion) != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public override bool UpdateInfo()
        {
            ItemList = BaronReplays.RiotAPI.Services.Request.GetStaticData("na/v1.2/item?itemListData=image&locale=" + Request.ApiLanguage, typeof(ItemListDto), DirectoryPath + InfoFile);
            return ItemList != null;
        }

        protected override void ReadInfo()
        {
            ItemList = BaronReplays.RiotAPI.Services.Request.ReadFromFile(DirectoryPath + InfoFile, typeof(ItemListDto));
            DecodeData();
        }


        protected override void DecodeData()
        {
            Dictionary<String, BitmapSource> ImagesBoard = ReadImages();
            foreach (ItemDto item in ItemList.data.Values)
            {
                try
                {
                    ImageDto image = item.image;
                    string imagePath = DirectoryPath + item.image.sprite;
                    if (!ImagesBoard.ContainsKey(imagePath))
                    {
                        String url = String.Format("http://ddragon.leagueoflegends.com/cdn/{0}/img/sprite/{1}", ItemList.version, item.image.sprite);
                        File.Delete(imagePath);
                        Utilities.DownloadFile(url, imagePath);
                        ImagesBoard.Add(imagePath, Utilities.GetBitmapImage(imagePath));
                    }
                    ImageSource cropped = new CroppedBitmap(ImagesBoard[imagePath], new Int32Rect(image.x, image.y, image.w, image.h));
                    cropped.Freeze();   //一定要Freeze，因為跨Thread
                    if (Images.ContainsKey(item.id))
                        Images.Remove(item.id);
                    Images.Add(item.id, cropped);
                }
                catch (Exception)
                {

                }
            }
        }

        public ImageSource GetImageById(Int32 id)
        {
            if (Images.ContainsKey(id))
                return Images[id];
            return null;
        }


        public static Item Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (objLock)
                    {
                        if (instance == null)
                        {
                            instance = new Item();
                        }

                    }
                }
                return instance;
            }
        }
    }

}
