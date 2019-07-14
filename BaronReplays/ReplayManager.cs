using BaronReplays.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace BaronReplays
{
    public class ReplayManager : DispatcherObject
    {
        private Dictionary<LoLRecorder, SimpleLoLRecord> recorderToRecords;

        private ExtendedObservableCollection<SimpleLoLRecord> records;
        public ExtendedObservableCollection<SimpleLoLRecord> Records
        {
            get
            {
                return records;
            }
            set
            {
                records = value;
            }
        }

        private ExtendedObservableCollection<SimpleLoLRecord> searchResult;
        public ExtendedObservableCollection<SimpleLoLRecord> SearchResult
        {
            get
            {
                return searchResult;
            }
            set
            {
                searchResult = value;
            }
        }

        private String directoryPath;
        private String DirectoryPath
        {
            get
            {
                return directoryPath;
            }
            set
            {
                directoryPath = value;
                InitDirectoryWatcher();
            }
        }

        public ReplayManager()
        {
            searchKeyWords = String.Empty;
            records = new ExtendedObservableCollection<SimpleLoLRecord>();
            recorderToRecords = new Dictionary<LoLRecorder, SimpleLoLRecord>();
            searchResult = new ExtendedObservableCollection<SimpleLoLRecord>();
            InitSearchWorker();
        }

        public delegate void SearchCompletedDelegate(ReplayManager rpmanager);
        public event SearchCompletedDelegate SearchCompleted;

        public event MainWindow.RecordDelegate PlayRecordEvent;
        public event MainWindow.RecordDelegate ExportRecordEvent;
        public event MainWindow.RemoveRecoderDelegate StopRecordingEvent;

        private FileSystemWatcher directoryWatcher;

        private String searchKeyWords;
        private Boolean IsSearching
        {
            get
            {
                return searchKeyWords.Length != 0;
            }
        }

        private BackgroundWorker searchWorker;
        private void InitSearchWorker()
        {
            if (searchWorker == null)
            {
                searchWorker = new BackgroundWorker();
                searchWorker.DoWork += Search;
                searchWorker.RunWorkerCompleted += Search_RunWorkerCompleted;
            }
        }

        private void Search_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            searchResult = new ExtendedObservableCollection<SimpleLoLRecord>(e.Result as List<SimpleLoLRecord>);
            if (SearchCompleted != null)
                SearchCompleted(this);
        }

        public void SearchAsync(String keyWords)
        {
            searchKeyWords = keyWords;
            if (IsSearching)
                searchWorker.RunWorkerAsync(keyWords);
        }

        public void Search(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            List<SimpleLoLRecord> resultList = new List<SimpleLoLRecord>();
            e.Result = resultList;

            for (int i = 0; i < records.Count; i++)
            {
                SimpleLoLRecord slr = records[i];
                if (slr == null)
                    continue;
                if (!slr.IsRecording)
                {
                    if (BaronReplays.Helper.FileHelper.GetFileName( slr.FileName).IndexOf(searchKeyWords, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        resultList.Add(slr);
                        continue;
                    }
                }
                if (slr.Search(searchKeyWords))
                    resultList.Add(slr);
            }
        }

        public void SearchFavorite()
        {
            List<SimpleLoLRecord> resultList = new List<SimpleLoLRecord>();

            foreach (SimpleLoLRecord slr in records) 
            {
                if (slr == null)
                    continue;
                if (slr.Favorite)
                {
                    resultList.Add(slr);
                }
            }
            searchResult = new ExtendedObservableCollection<SimpleLoLRecord>(resultList as List<SimpleLoLRecord>);
            if (SearchCompleted != null)
                SearchCompleted(this);
        }


        private void InitDirectoryWatcher()
        {
            if (directoryWatcher != null)
            {
                directoryWatcher.EnableRaisingEvents = false;
                directoryWatcher.Dispose();
            }
            directoryWatcher = new FileSystemWatcher();
            directoryWatcher.Filter = "*.lpr";
            directoryWatcher.Renamed += ReplayDirectoryWatcher_Renamed;
            directoryWatcher.Deleted += ReplayDirectoryWatcher_Deleted;
            directoryWatcher.Created += ReplayDirectoryWatcher_Created;
            directoryWatcher.Path = directoryPath;
            directoryWatcher.EnableRaisingEvents = true;
        }

        public void StopFileSystemWatcher()
        {
            if (directoryWatcher == null)
                throw new Exception("呼叫此方法之前要先設定監看的資料夾");
            else
            {
                directoryWatcher.EnableRaisingEvents = false;
            }
        }

        public void StartFileSystemWatcher()
        {
            if (directoryWatcher == null)
                throw new Exception("呼叫此方法之前要先設定監看的資料夾");
            else
            {
                directoryWatcher.EnableRaisingEvents = true;
            }
        }

        private void ReplayDirectoryWatcher_Created(object sender, FileSystemEventArgs e)
        {
            FileInfo info = new FileInfo(e.FullPath);
            DateTime modifyTime = DateTime.MinValue;
            do
            {
                SpinWait.SpinUntil(() => false, 1000);
                if (modifyTime == info.LastWriteTime)
                    break;
            } 
            while (ReadRecord(e.FullPath, true) == null) ;
            //wait for write complete
        }

        private void ReplayDirectoryWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            SimpleLoLRecord slr = QueryByFileName(e.FullPath);
            if (slr != null)
                RemoveRecord(slr);
        }

        private void ReplayDirectoryWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            SimpleLoLRecord slr = QueryByFileName(e.OldFullPath);
            if (slr != null)
                slr.FileName = e.FullPath;
            else
            {
                slr = QueryByFileName(e.FullPath);//如果是BR改的，simple record的名字就已經是改過的了
                if (slr == null)
                    ReadRecord(e.FullPath, true);
            }
        }

        private SimpleLoLRecord QueryByFileName(String filename)
        {
            IEnumerable<SimpleLoLRecord> queryResult = Records.Where(r => r.FileName == filename);
            if (queryResult.Count() > 0)
                return queryResult.First();
            else
                return null;
        }

        public delegate void LoadRecordsAsyncDelegate(String path);
        public void LoadRecordsAsync(String path)
        {
            records.Clear();
            DirectoryPath = path;
            InitBackGroundWorker();
            loadRecordWorker.RunWorkerAsync();
        }

        private delegate void RecordUIThreadDelegte(SimpleLoLRecord slr);
        private void InsertRecordUIThread(SimpleLoLRecord slr)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(new RecordUIThreadDelegte(InsertRecordUIThread), slr);
            }
            else
            {
                records.Insert(0, slr);
                if (IsSearching)
                {
                    if (slr.Search(searchKeyWords))
                        searchResult.Insert(0, slr);
                }
            }
        }

        private delegate void MultiRecordUIThreadDelegte(IEnumerable<SimpleLoLRecord> slrs);
        private void InsertMultiRecordUIThread(IEnumerable<SimpleLoLRecord> slrs)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(new MultiRecordUIThreadDelegte(InsertMultiRecordUIThread), slrs);
            }
            else
            {
                records.AddRange(slrs);
            }
        }


        public void RemoveRecord(SimpleLoLRecord slr)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(new RecordUIThreadDelegte(RemoveRecord), slr);
            }
            else
            {
                if (records.Contains(slr))
                    records.Remove(slr);
                if (searchResult.Contains(slr))
                    searchResult.Remove(slr);
            }
        }

        public SimpleLoLRecord ReadRecord(LoLRecord record, Boolean addToList)
        {
            SimpleLoLRecord slr = LoLRecord.GetSimpleLoLRecord(record);
            if (slr != null)
            {
                InitSimpleRecord(slr);
                if (addToList)
                {
                    InsertRecordUIThread(slr);
                }
            }
            return slr;
        }


        public SimpleLoLRecord ReadRecord(String path, Boolean addToList)
        {
            LoLRecord lr = new LoLRecord();
            lr.readFromFile(path, true);
            if (lr.IsBroken)
                return null;
            if (!loadRecordWorker.IsBusy)
            {
                if (!GameDatabase.Instance.IsExistsGame(lr.GameId, lr.GamePlatform))
                    GameDatabase.Instance.AddGame(lr);
            }
            return ReadRecord(lr, addToList);
        }


        private void InitSimpleRecord(SimpleLoLRecord slr)
        {
            slr.DeleteClickEvent += DeleteRecord;
            if (PlayRecordEvent != null)
                slr.WatchClickEvent += PlayRecordEvent;
            if (ExportRecordEvent != null)
                slr.ExportClickEvent += ExportRecordEvent;
            slr.GetGameFromDB();
        }

        public void DeleteRecord(SimpleLoLRecord slr)
        {
            if (slr.RecoringRecorder == null)
            {
                if (File.Exists(slr.FileName))
                    File.Delete(slr.FileName);
            }
            else
            {
                StopRecordingEvent(slr.RecoringRecorder, true);
                RemoveRecord(slr);
                recorderToRecords.Remove(slr.RecoringRecorder);
            }
        }

        public void UpdateRecordingRecord(LoLRecorder recorder)
        {
            if (recorderToRecords.ContainsKey(recorder))
            {
                recorderToRecords[recorder].UpdateSummonerData();
            }
            else
            {
                SimpleLoLRecord slr = ReadRecord(recorder.record, true);
                slr.RecoringRecorder = recorder;
                recorderToRecords.Add(recorder, slr);
            }
        }

        public SimpleLoLRecord GetRecordFromRecorder(LoLRecorder recorder)
        {
            if (recorderToRecords.ContainsKey(recorder))
                return recorderToRecords[recorder];
            return null;
        }

        private BackgroundWorker loadRecordWorker;
        private void InitBackGroundWorker()
        {

            if (loadRecordWorker == null)
            {
                loadRecordWorker = new BackgroundWorker();
                loadRecordWorker.DoWork += LoadRecords;
                loadRecordWorker.RunWorkerCompleted += LoadRecordsWorkerDone;
            }
        }

        private void LoadRecordsWorkerDone(object sender, RunWorkerCompletedEventArgs e)
        {

        }


        private void LoadRecords(object sender, DoWorkEventArgs e)
        {
            System.Diagnostics.Stopwatch loadWatch = new System.Diagnostics.Stopwatch();
            DirectoryInfo loldir = new DirectoryInfo(DirectoryPath);
            List<SimpleLoLRecord> tempList = new List<SimpleLoLRecord>();
            try
            {
                FileInfo[] files = loldir.GetFiles("*.lpr").OrderByDescending(x => x.LastWriteTime).ToArray();
                loadWatch.Start();
                foreach (FileInfo f in files)
                {
                    SimpleLoLRecord slr = ReadRecord(f.FullName,false);
                    if (slr != null)
                    {
                        tempList.Add(slr);
                    }
                    if (loadWatch.ElapsedMilliseconds > 500)
                    {
                        //add into view
                        InsertMultiRecordUIThread(tempList);
                        tempList.Clear();
                        loadWatch.Restart();
                    }
                }
                InsertMultiRecordUIThread(tempList);
                loadWatch.Stop();
            }
            catch (Exception)
            {

            }
            CheckDatabase();
        }

        private void CheckDatabase()
        {
            List<LoLRecord> needToAddToDb = new List<LoLRecord>();
            List<SimpleLoLRecord> needToUpdate = new List<SimpleLoLRecord>();
            foreach (SimpleLoLRecord slr in records)
            {
                if (!GameDatabase.Instance.IsExistsGame(slr.GameId, slr.GamePlatform))
                {
                    needToUpdate.Add(slr);
                    LoLRecord record = new LoLRecord();
                    record.readFromFile(slr.FileName,true);
                    needToAddToDb.Add(record);
                }
            }
            GameDatabase.Instance.AddGame(needToAddToDb.ToArray());
            foreach (SimpleLoLRecord slr in needToUpdate)
            {
                slr.GetGameFromDB();
            }
        }


        public void ChangeSummonerName(String newName)
        {
            foreach (SimpleLoLRecord slr in records.ToList())
            {
                slr.SelectPlayer(newName);
            }
        }
    }
}
