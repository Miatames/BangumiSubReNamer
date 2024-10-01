/************************************************************************
 *Copyright (C) 2013  David Cheng
 *Author:	David Cheng <david@dxcheng.me>
 ************************************************************************
 *
 *This program is free software; you can redistribute it and/or modify
 *it under the terms of the GNU General Public License as published by
 *the Free Software Foundation; either version 2 of the License, or
 *(at your option) any later version.
 *
 *This program is distributed in the hope that it will be useful,
 *but WITHOUT ANY WARRANTY; without even the implied warranty of
 *MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *GNU General Public License for more details.
 *
 *You should have received a copy of the GNU General Public License along
 *with this program; if not, write to the Free Software Foundation, Inc.,
 *51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 ************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Ass2Srt
{
    class SrtWriter
    {
        string strFileName;
        string strFolderPath;
        string strNewFolderPath;
        int type;
        FileStream fs;
        StreamWriter sw;

        public SrtWriter(string strFolderPath, string strFileNameWithoutExtension, int type)
        {
            this.strFolderPath = strFolderPath;
            this.strFileName = strFileNameWithoutExtension;
            this.type = type;
        }

        public bool WriteSrtFile(string[] strSrtLines)
        {
            CreateFolder();
            switch (type)
            {
                case 0:
                    strFileName += ".简体&英文";
                    break;
                case 1:
                    strFileName += ".繁体&英文";
                    break;
                case 2:
                    strFileName += ".简体";
                    break;
                case 3:
                    strFileName += ".繁体";
                    break;
                case 4:
                    strFileName += ".英文";
                    break;
            }

            strFileName += ".srt";

            try
            {
                fs = new FileStream(Path.Combine(strFolderPath, strFileName), FileMode.OpenOrCreate);
                //if (type == 0 || type == 2)
                //{
                //    sw = new StreamWriter(fs, Encoding.GetEncoding(936));
                //}
                //else if (type == 1 || type == 3)
                //{
                //    sw = new StreamWriter(fs, Encoding.GetEncoding(950));
                //}
                //else
                //{
                //sw = new StreamWriter(fs, Encoding.Unicode);
                //Now use UTF8 to make it work on UNIX-like systems.
                sw = new StreamWriter(fs, Encoding.UTF8);
                //}

                foreach (string s in strSrtLines)
                {
                    sw.WriteLine(s);
                    sw.Flush();
                }

                fs.Close();
                return true;
            }

            catch (Exception)
            {
                return false;
            }
        }

        private void CreateFolder()
        {
            string strDirectoryName = Path.Combine(strFolderPath, strFileName);
            if (Directory.Exists(strDirectoryName))
            {
            }

            else
            {
                Directory.CreateDirectory(strDirectoryName);
            }

            strNewFolderPath = strDirectoryName;
        }
    }
}