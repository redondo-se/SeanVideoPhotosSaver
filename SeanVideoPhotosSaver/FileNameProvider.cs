using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;

namespace SeanVideoPhotosSaver
{
    public class FileNameProvider
    {
        public delegate void FileListLoadCompleteDelegate();
        public event FileListLoadCompleteDelegate FileListLoadedCompleted;

        private static FileNameProvider _theInstance = new FileNameProvider();
        public static FileNameProvider Instance { get { return _theInstance; } }

        private const string APP_FOLDER_NAME = "SeanVideoPhotosSaver";

        private List<string> _fileList;
        private BackgroundWorker _fileNameLoader;
        private bool _loadedFromCache;

        public bool LoadedFromCache
        {
            get { return _loadedFromCache; }
        }

        private string _imageAndVideoFolder;

        private FileNameProvider()
        {
            //_syncContext = syncContext;

            RegistryKey settingsKey = Registry.CurrentUser.OpenSubKey(Settings.KEY_PATH, true);
            if (settingsKey != null)
            {
                _imageAndVideoFolder = (string)(settingsKey.GetValue(Settings.IMAGE_PATH));
            }

            _fileNameLoader = new BackgroundWorker();
            _fileNameLoader.DoWork += new DoWorkEventHandler(_fileNameLoader_DoWork);
            _fileNameLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_fileNameLoader_RunWorkerCompleted);
            LoadFileListAsync();
        }

        public bool LoadFileListAsync()
        {
            if (_fileNameLoader.IsBusy)
            {
                return false;
            }

            _fileNameLoader.RunWorkerAsync();
            return true;
        }

        void _fileNameLoader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (FileListLoadedCompleted != null)
            {
                FileListLoadedCompleted();
            }
        }

        void _fileNameLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!string.IsNullOrEmpty(_imageAndVideoFolder))
            {
                // Look for list in user directory
                try
                {
                    string cachedFile = GetCacheFileName();
                    Stream stream = File.Open(cachedFile, FileMode.Open);
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    _fileList = (List<string>)bFormatter.Deserialize(stream);
                    stream.Close();
                    _loadedFromCache = true;
                }
                catch (Exception)
                {
                    _fileList = new List<string>(Directory.GetFiles(_imageAndVideoFolder, "*.*", SearchOption.AllDirectories));
                    _loadedFromCache = false;
                }
            }
        }

        private string GetCacheFileName()
        {
            string userDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + APP_FOLDER_NAME;
            if (!Directory.Exists(userDir))
            {
                Directory.CreateDirectory(userDir);
            }
            return (userDir + "\\" + _imageAndVideoFolder.Replace('\\', '_').Replace(':', '_'));
        }

        public void SaveFileList()
        {
            if (!string.IsNullOrEmpty(_imageAndVideoFolder) && (_fileList != null) && (_fileList.Count > 0))
            {
                string cachedFile = GetCacheFileName();
                using (Stream stream = File.Open(cachedFile, FileMode.Create))
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(stream, _fileList);
                }
            }
        }

        private int _numSinceVideo = 0;
        private int _numFailedFindVideo = 0;
        private const int MAX_TRIES_FIND_VIDEO = 30;
        private const int MAX_CONTINUOUS_IMAGE = 10;
        private const int MAX_FAILED_FIND_VIDEO = 10;  // if were not finding videos, quit trying to force it
        public string GetNextFileName()
        {
            string retVal = null;

            if (_fileList == null)
            {
                return null;
            }

            while ((retVal == null) && (_fileList.Count > 0))
            {
                int index = RandomGenerator.Instance.Next(0, _fileList.Count);
                string tempVal = _fileList[index];
                string fileExt = MediaTypeChecker.Instance.GetFileExtension(tempVal);

                // try to find videos if we're seeing too many images.
                if ((_numFailedFindVideo < MAX_FAILED_FIND_VIDEO) && (_numSinceVideo >= MAX_CONTINUOUS_IMAGE))
                {
                    int triesToFindVideo = 0;
                    while ((triesToFindVideo++ < MAX_TRIES_FIND_VIDEO) && MediaTypeChecker.Instance.IsExtentionsImage(fileExt))
                    {
                        index = RandomGenerator.Instance.Next(0, _fileList.Count);
                        tempVal = _fileList[index];
                        fileExt = MediaTypeChecker.Instance.GetFileExtension(tempVal);
                    }

                    if (MediaTypeChecker.Instance.IsExtentionsImage(fileExt))
                    {
                        ++_numFailedFindVideo;
                    }
                }

                _fileList.RemoveAt(index);

                if ((MediaTypeChecker.Instance.IsExtensionVideo(fileExt) || MediaTypeChecker.Instance.IsExtentionsImage(fileExt)) && File.Exists(tempVal))
                {
                    retVal = tempVal;
                }

                if (MediaTypeChecker.Instance.IsExtentionsImage(fileExt))
                {
                    ++_numSinceVideo;
                }
                else
                {
                    _numSinceVideo = 0;
                }

                // Delete the cace file
                if (_fileList.Count <= 0)
                {
                    try
                    {
                        File.Delete(GetCacheFileName());
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }
            }

            return retVal;
        }
    }
}
