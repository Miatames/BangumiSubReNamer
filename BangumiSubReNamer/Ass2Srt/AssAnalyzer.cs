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
using Microsoft.VisualBasic;

namespace Ass2Srt
{
    class AssAnalyzer
    {
        protected string ConvertToTrad(string strDialogue)
        {
            return (Microsoft.VisualBasic.Strings.StrConv(strDialogue as String, Microsoft.VisualBasic.VbStrConv.TraditionalChinese, 0));
			//return "";
		}

        protected string[] SplitString(string originalString, string seperator)
        {
            List<string> lSS = new List<string>();
            while (true)
            {
                int index = originalString.IndexOf(seperator);
                if (index >= 0)
                {
                    lSS.Add(originalString.Substring(0, index));
                    originalString = originalString.Substring(index + 2);
                }

                else
                {
                    lSS.Add(originalString);
                    break;
                }
            }

            return lSS.ToArray();
        }

        protected string[] DialogueSplitter(string strDialogues)
        {
            string[] strSplittedDialogues = SplitString(strDialogues, "\r\n");

            return strSplittedDialogues;
        }

        protected int CountDigitsInString(string strDialogues)
        {
            int numDigits = 0;
            for (int i = 0; i < strDialogues.Length; ++i)
            {
                if (strDialogues[i] >= '0' && strDialogues[i] <= '9')
                {
                    ++numDigits;
                }
            }

            return numDigits;
        }

        protected int CountSpacesInString(string strDialogues)
        {
            int numDigits = 0;
            for (int i = 0; i < strDialogues.Length; ++i)
            {
                if (strDialogues[i] == ' ')
                {
                    ++numDigits;
                }
            }

            return numDigits;
        }

        protected int CountLettersInString(string strDialogues, char l)
        {
            int numDigits = 0;
            for (int i = 0; i < strDialogues.Length; ++i)
            {
                if (strDialogues[i] == l)
                {
                    ++numDigits;
                }
            }

            return numDigits;
        }
    }
}
