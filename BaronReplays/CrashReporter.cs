using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Windows;
using System.Security;

namespace BaronReplays
{
    class CrashReporter
    {
        private List<string> _filename;     //檔案完整名稱 例如 ErrorBug.txt  
        private List<string> _filepath;     /*完整資料夾路徑 例如 ftp: //ahri.tw/年-月-日_時-分-秒_毫秒 */
        private FtpWebRequest _request;
        private FtpWebResponse _response;

        public CrashReporter()
        {
            _filename = new List<string>();
            _filepath = new List<string>();
        }

        public bool SendToServer()
        {
            string folderpath = "ftp://ahri.tw/"
                              + String.Format("[{0}] {1}",Utilities.Version, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff"));

            if (FtpDirectoryExists(folderpath) != 0)
            {
                try
                {
                    _request = (FtpWebRequest)WebRequest.Create(folderpath);
                    _request.Credentials = new NetworkCredential("brreport", "brreport");
                    _request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    _response = (FtpWebResponse)_request.GetResponse();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    Random rand = new Random();
                    int randNumber;
                    do
                    {
                        randNumber = rand.Next();
                    } while (FtpDirectoryExists(folderpath + randNumber) != 1);
                    folderpath = folderpath + randNumber;
                    _request = (FtpWebRequest)WebRequest.Create(folderpath);
                    _request.Credentials = new NetworkCredential("brreport", "brreport");
                    _request.Method = WebRequestMethods.Ftp.MakeDirectory;
                    _response = (FtpWebResponse)_request.GetResponse();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            for (int i = 0; i < _filename.Count; i++)
            {
                try
                {
                    _request = (FtpWebRequest)WebRequest.Create(folderpath + "/" + _filename[i]);
                    _request.Method = WebRequestMethods.Ftp.UploadFile;
                    _request.Credentials = new NetworkCredential("brreport", "brreport");

                    StreamReader sourceStream = new StreamReader(_filepath[i]);
                    byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                    sourceStream.Close();
                    _request.ContentLength = fileContents.Length;
                    Stream requestStream = _request.GetRequestStream();
                    requestStream.Write(fileContents, 0, fileContents.Length);
                    requestStream.Close();

                    _response = (FtpWebResponse)_request.GetResponse();
                    Console.WriteLine("Upload File Complete, status {0}", _response.StatusDescription);
                    _response.Close();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        public void AddFile(string filepath)    //上傳檔案
        {
            _filename.Add(Path.GetFileName(filepath));
            _filepath.Add(filepath);
        }

        //0 = 有資料夾 1=無資料夾 2=無法連接伺服器
        private int FtpDirectoryExists(string directory)       //判斷server有沒有相同資料夾
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create(directory);
                request.Credentials = new NetworkCredential("brreport", "brreport");
                request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return 1;
                else if (response.StatusCode == FtpStatusCode.Undefined)
                    return 2;
            }
            return 0;
        }
    }
}
