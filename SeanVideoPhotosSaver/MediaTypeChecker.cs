using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SeanVideoPhotosSaver
{
    public class MediaTypeChecker
    {
        private static MediaTypeChecker _theInstance = new MediaTypeChecker();

        public static MediaTypeChecker Instance { get { return _theInstance; } }

        public string  GetFileExtension(string fileName)
        {
            string fileExt = null;

            if (!string.IsNullOrEmpty(fileName))
            {
                fileExt = Path.GetExtension(fileName).ToLower();
            }

            //if (fileName.Length > 4)
            //{
            //    fileExt = fileName.Substring(fileName.Length - 4, 4).ToLower();
            //}

            return fileExt;
        }

        public bool IsExtensionVideo(string extension)
        {
            return ((extension == ".avi") ||
                    (extension == ".mov") ||
                    (extension == ".mpg") ||
                    (extension == ".ts") ||
                    (extension == ".mkv") ||
                    (extension == ".mpeg") ||
                    (extension == ".mp4") ||
                    (extension == ".m4v") ||
                    (extension == ".m2ts") ||
                    (extension == ".wmv")
                    );
        }

        public bool IsExtentionsImage(string extension)
        {
            return ((extension == ".jpg") ||
                    (extension == ".bmp") ||
                    (extension == ".png") ||
                    (extension == ".gif") ||
                    (extension == ".jpeg"));
        }
    }
}
