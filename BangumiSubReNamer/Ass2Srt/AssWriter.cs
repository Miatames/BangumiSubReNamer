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
    class AssWriter
    {
        string strFileName;
        string strFolderPath;
        string strNewFolderPath;
        int type;
        FileStream fs;
        StreamWriter sw;

        public AssWriter(string strFolderPath, string strFileNameWithoutExtension, int type)
        {
            this.strFolderPath = strFolderPath;
            this.strFileName = strFileNameWithoutExtension;
            this.type = type;       // 5-简英  6-繁英  7-简体  8-繁体  9-英文
        }

        public bool WriteAssFile(string[] strAssDescription, string[] strAssDialogues)
        {
            CreateFolder();
            switch (type)
            {
                case 5:
                    strFileName += ".简体&英文";
                    break;
                case 6:
                    strFileName += ".繁体&英文";
                    break;
                case 7:
                    strFileName += ".简体";
                    break;
                case 8:
                    strFileName += ".繁体";
                    break;
                case 9:
                    strFileName += ".英文";
                    break;
            }

            strFileName += ".ass";

            try
            {
                fs = new FileStream(strNewFolderPath + "\\" + strFileName, FileMode.OpenOrCreate);
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
                    //NOW USE UTF8 to make it work on UNIX-like systems.
                sw = new StreamWriter(fs, Encoding.UTF8);
                //}

                for(int i = 0; i < strAssDescription.Length - 1; i++)
                {
                    sw.WriteLine(strAssDescription[i]);
                    sw.Flush();
                }

                foreach(string s in strAssDialogues)
                {
                    sw.WriteLine(s);
                    sw.Flush();
                }

                fs.Close();
                return true;
            }

            catch(Exception)
            {
                return false;
            }
        }

        private void CreateFolder()
        {
            string strDirectoryName = strFolderPath + "\\" + strFileName;
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
