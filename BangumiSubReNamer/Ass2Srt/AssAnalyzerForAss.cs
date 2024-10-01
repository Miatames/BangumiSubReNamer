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
using System.Text.RegularExpressions;

namespace Ass2Srt
{
    class AssAnalyzerForAss : AssAnalyzer
    {
        string strDialogues;
        string strDescriptions;
        int type;
        bool DoubleOrSingle;

        public AssAnalyzerForAss(string strDialogues, string strDescriptions, int type, bool DoubleOrSingle)
        {
            this.strDialogues = strDialogues;
            this.strDescriptions = strDescriptions;
            this.type = type;       // 5-简英  6-繁英  7-简体  8-繁体  9-英文
            this.DoubleOrSingle = DoubleOrSingle;
        }

        public void Analyze(out string[] oDescriptions, out string[] oDialogues)
        {
            string[] ssDialogues = DialogueSplitter(strDialogues);
            string[] ssDescription = DialogueSplitter(strDescriptions);
            string[] ssDialoguesProcessed = DialogueProcessor(ssDialogues);

            oDialogues = ssDialoguesProcessed;
            oDescriptions = ssDescription;
        }

        public string[] DialogueProcessor(string[] ssDialogues)
        {
            if (type == 5)
            {
                return ssDialogues;
            }

            List<string> lDialogues = new List<string>();
            List<string> lSimpChiSubs = new List<string>();
            List<string> lEngSubs = new List<string>();
            Regex rx = new Regex(@"[^\{^\}]+");
            Regex exEngOrChi = new Regex(@"[\u4e00-\u9fa5]");

            foreach (string s in ssDialogues)
            {
                string strDiag = "";
                lSimpChiSubs.Clear();
                lEngSubs.Clear();
                MatchCollection MC = rx.Matches(s);
                foreach (Match M in MC)
                {
                    if (M.ToString() == "" || M.ToString()[0] == '\\')
                    {
                        continue;
                    }

                    else
                    {
                        //if (M.ToString().Contains("Dialogue:"))
                        //{
                        //    string[] tss = SplitString(M.ToString(), ",,");
                        //    if (tss[tss.Length - 1] != "")
                        //    {
                        //        strDiag += tss[tss.Length - 1];
                        //    }
                        //}

                        //else
                        //{
                        //    strDiag += M.ToString();
                        //}

                        if (DoubleOrSingle)
                        {
                            //M.ToString()ing[] ss = SplitM.ToString()ing(M.ToString()Diag, "\\N");
                            //foreach (M.ToString()ing M.ToString() in ss)
                            //{
                            if (M.ToString() != "")
                            {
                                if (exEngOrChi.IsMatch(M.ToString()))
                                {
                                    lSimpChiSubs.Add(M.ToString());
                                }

                                else
                                {
                                    if (!M.ToString().Contains("Dialogue:") && !M.ToString().Contains("YYeTs"))
                                    {
                                        lEngSubs.Add(M.ToString());
                                    }
                                }
                                //}
                            }
                        }

                        else
                        {
                            if (M.ToString() != "")
                            {
                                lSimpChiSubs.Add(M.ToString());
                            }
                        }
                    }
                }

                switch (type)
                {
                    case 6:
                        {
                            strDiag = CvrtSubToTradChi(s, lSimpChiSubs);
                            if (strDiag != "")
                            {
                                lDialogues.Add(strDiag);
                            }
                            break;
                        }

                    case 7:
                        {
                            strDiag = DeleteEngSub(s, lEngSubs);
                            if (strDiag != "")
                            {
                                lDialogues.Add(strDiag);
                            }
                            break;
                        }

                    case 8:
                        {
                            strDiag = CvrtSubToTradChi(s, lSimpChiSubs);
                            strDiag = DeleteEngSub(strDiag, lEngSubs);
                            if (strDiag != "")
                            {
                                lDialogues.Add(strDiag);
                            }
                            break;
                        }
                }
            }

            return lDialogues.ToArray();
        }

        //public string[] DescriptionProcessor(string[] ssDialogues)
        //{

        //}

        private string DeleteEngSub(string strDialogue, List<string> lEngSubs)
        {
            string strProcessed = "";
             Regex exEngOrChi = new Regex(@"[\u4e00-\u9fa5]");

            foreach (string strEngSub in lEngSubs)
            {
                if (strEngSub != " " && !strEngSub.Contains("■") && strDialogue.Contains("\\N"))
                {
                    strDialogue = strDialogue.Replace(strEngSub, "\\*tObErEmOvEd*/");
                }
            }

            string[] ss = SplitString(strDialogue, "\\N");

            for(int i = 0; i < ss.Length; i++)
            {
                if (ss[i].Contains("\\*tObErEmOvEd*/"))
                {
                    continue;
                }

                else
                {
                    if (i == 0)
                    {
                        strProcessed += ss[i];
                    }

                    else
                    {
                        strProcessed += ("\\N" + ss[i]);
                    }
                }
            }

            return strProcessed;
        }

        private string CvrtSubToTradChi(string strDialogue, List<string> lSimpChiSubs)
        {
            List<string> lTradChiSubs = new List<string>();
            string strProcessed = strDialogue;

            foreach (string sSimpChiSub in lSimpChiSubs)
            {
                lTradChiSubs.Add(ConvertToTrad(sSimpChiSub));
            }

            for (int i = 0; i < lSimpChiSubs.Count; i++)
            {
                strProcessed = strProcessed.Replace(lSimpChiSubs[i], lTradChiSubs[i]);
            }

            return strProcessed;
        }
    }
}
