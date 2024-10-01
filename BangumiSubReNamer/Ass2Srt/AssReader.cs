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
    class AssReader
    {
        string filePath;
        string fileContent;
        FileStream fs;
        StreamReader sr;

        public AssReader(string filePath)
        {
            this.filePath = filePath;
        }

        public bool IsValid()
        {
            try
            {
                fs = new FileStream(filePath, FileMode.Open);
                sr = new StreamReader(fs, true);

                //2024.10.01 Miatames修改 移除文件大小限制
                /*if (fs.Length > 1024 * 1024 || fs.Length <= 0)    //文件不得大于1M
                {
                    return false;
                }

                else
                {*/
                    fileContent = sr.ReadToEnd();

                    if (fileContent.Contains("Dialogue:"))
                    {
                        return true;
                    }

                    else
                    {
                        return false;
                    }
                // }
            }

            catch (Exception)
            {
                return false;
            }

            finally
            {
                fs.Close();
            }
        }

        public string ReadDialogues()
        {
            string strDialogues;
            int firstDialoguePos = fileContent.IndexOf("Dialogue");
            strDialogues = fileContent.Substring(firstDialoguePos);

            return strDialogues;
        }

        public string ReadDescription()
        {
            string strDescription;
            int firstDialoguePos = fileContent.IndexOf("Dialogue");
            strDescription = fileContent.Substring(0, firstDialoguePos);

            return strDescription;
        }
    }
}
