using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Lucene.Net;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

using DicSqlType = System.Collections.Generic.Dictionary<string, string>;


namespace LucSearchLib
{
    class GroupType
    {
        public Dictionary<string, string> Group;
        public int Count;
    }


    public class CJKSearch
    {
        public static string QueryHelp()
        {
            System.Text.StringBuilder sb= new System.Text.StringBuilder();
            sb.AppendLine("select(sel) [ * | field, ... ] ");
            sb.AppendLine("from    directory ");
            sb.AppendLine("where   field1='val' and field2='val' or ... ");
            sb.AppendLine("range field1 = [minRangeVal TO MaxRangeVal]");
            sb.AppendLine("order by [_score | _doc | field1 [order_expression], ...] ");
            sb.AppendLine("group by field1, ...  ");
            sb.AppendLine(" ");
            sb.AppendLine("( *order_expression : [ asc | desc | doc_asc | doc_desc | score_asc | score_desc | int_asc | int_desc ] ) ");
            sb.AppendLine("( *group result fields : group (group by value), count (group by count)");

            return sb.ToString();
        }

        public static XmlDocument SearchQuery(string sRoot, string Sql, int Pos, int Count)
        {
            XmlDocument xml = GetDocment();

            try
            {
                DicSqlType SqlDic = SqlParse(Sql);

                string sPath = SqlDic["from"];

                if (!string.IsNullOrEmpty(sRoot))
                {
                    sPath = string.Format("{0}\\{1}", sRoot, SqlDic["from"]);
                }

                Lucene.Net.Store.Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(sPath));
                Analyzer analyzer = new Lucene.Net.Analysis.CJK.CJKAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
                
                QueryParser parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, "", analyzer);
                //parser.DefaultOperator = QueryParser.Operator.AND;
                parser.AllowLeadingWildcard = true;

                string Query = SqlDic["where"];
                Query query = parser.Parse(Query);

                using (IndexSearcher searcher = new IndexSearcher(directory, true))
                {
                    string Group = SqlDic.Keys.Contains("group") ? SqlDic["group"] : "";
                    string Sort = SqlDic.Keys.Contains("order") ? SqlDic["order"] : "";
                    string Range = SqlDic.Keys.Contains("range") ? SqlDic["range"] : "";

                    Lucene.Net.Search.Sort sr = SortParse(Sort);
                   Lucene.Net.Search.Filter ftr = RangeParse(Range);
                    //Lucene.Net.Search.Filter ftr = NumericRangeFilter.NewIntRange("salesprice", 10000, 20000, true, true);
                    
                    DateTime dtStart = DateTime.Now;

                    TopDocs tdocs = searcher.Search(query, ftr, Pos + Count, sr);
                   
                    ScoreDoc[] hits = tdocs.ScoreDocs;
  
                    string sDur = string.Format("{0:0.0000}", DateTime.Now.Subtract(dtStart).TotalSeconds);

                    int results = hits.Length;
                    {
                        XmlElement node = xml.CreateElement("ret");
                        node.AppendChild(CreateElement(xml, "totalhits", tdocs.TotalHits.ToString()));
                        node.AppendChild(CreateElement(xml, "hits", (hits.Length - Pos) <= 0 ? "0" : (hits.Length - Pos).ToString()));
                        node.AppendChild(CreateElement(xml, "seconds", sDur));
                        node.AppendChild(CreateElement(xml, "query", Query));
                        node.AppendChild(CreateElement(xml, "Range", RangeParseTEXT(Range)));
                        xml.SelectSingleNode("/docs").AppendChild(node);
                    }

                    if (!string.IsNullOrEmpty(Group))
                    {
                        SortedDictionary<string, GroupType> dicGroup = new SortedDictionary<string, GroupType>();

                        string sName = Group.Replace(" ", "");
                        string[] arName = sName.Split(new char[] { ',' });

                        for (int i = Pos; i < Pos + Count; ++i)
                        {
                            if (i >= results) break;
                            Document doc = searcher.Doc(hits[i].Doc);

                            string gValue = "";
                            for (int ix = 0; ix < arName.Length; ++ix)
                            {
                                if (ix != 0) gValue += "/";
                                gValue += doc.Get(arName[ix]);
                            }

                            if (dicGroup.Keys.Contains(gValue))
                            {
                                dicGroup[gValue].Count++;
                            }
                            else
                            {
                                GroupType gp = new GroupType();
                                gp.Count = 1;
                                gp.Group = new Dictionary<string, string>();

                                for (int ix = 0; ix < arName.Length; ++ix)
                                {
                                    gp.Group.Add(arName[ix], doc.Get(arName[ix]));
                                }

                                dicGroup.Add(gValue, gp); 
                            }
                        }

                        foreach (GroupType gr in dicGroup.Values)
                        {
                            XmlElement node = CreateElement(xml, "doc", "");

                            foreach (string fname in gr.Group.Keys)
                            {
                                string value = gr.Group[fname];
                                node.AppendChild(CreateElement(xml, fname, value));
                            }

                            node.AppendChild(CreateElement(xml, "count", gr.Count.ToString()));
                            xml.SelectSingleNode("/docs").AppendChild(node);
                        }
                    }
                    else
                    {
                        for (int i = Pos; i < Pos + Count; ++i)
                        {
                            if (i >= results) break;

                            XmlElement node = CreateElement(xml, "doc", "");
                            Document doc = searcher.Doc(hits[i].Doc);

                            string Select = SqlDic["select"].IndexOf("*") >= 0 ? "" : SqlDic["select"];

                            if (string.IsNullOrEmpty(Select))
                            {
                                foreach (Field f in doc.GetFields())
                                {
                                    node.AppendChild(CreateElement(xml, f.Name, doc.Get(f.Name)));
                                }
                            }
                            else
                            {
                                string[] Selects = Select.Split(new char[] { ',' });

                                foreach (string f in Selects)
                                {
                                    node.AppendChild(CreateElement(xml, f.Trim(), doc.Get(f.Trim())));
                                }
                            }

                            xml.SelectSingleNode("/docs").AppendChild(node);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                xml.SelectSingleNode("/docs").AppendChild(CreateElement(xml, "except", ex.Message));
            }


            return xml;
        }

        private static Filter RangeParse(string Range)
        {
           // Range = "salesprice = [0 TO 100]";

            if (!string.IsNullOrEmpty(Range))
            {
                string[] Ranges = Range.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if(Ranges.Length == 2)
                {
                    string RangeField = Ranges[0].Trim().ToLower();
                    string[] RangeData = Ranges[1].Replace("[", "").Replace("]", "").Split(new string[] { "TO" },  StringSplitOptions.RemoveEmptyEntries);

                    if (RangeData.Length != 2)
                    {
                        return null;
                    }

                   float RangeStart = float.Parse(RangeData[0]);
                   float RangeEnd = float.Parse(RangeData[1]);

                  //  int RangeStart = int.Parse(RangeData[0]);
                  //  int RangeEnd = int.Parse(RangeData[1]);


                    return NumericRangeFilter.NewFloatRange(RangeField, RangeStart, RangeEnd, true, true);
                   
                }
                return null;
            }

            return null;
        }

        private static string RangeParseTEXT(string Range) 
        {
            // Range = "salesprice = [0 TO 100]";

            if (!string.IsNullOrEmpty(Range))
            {
                string[] Ranges = Range.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (Ranges.Length == 2)
                {
                    string RangeField = Ranges[0];
                    string[] RangeData = Ranges[1].Replace("[", "").Replace("]", "").Split(new string[] { "TO" }, StringSplitOptions.RemoveEmptyEntries);

       
                    if (RangeData.Length != 2)
                    {
                        return "RangeDate길이 : " + RangeData.Length.ToString() ;
                    }

                    float RangeStart = float.Parse(RangeData[0]);
                    float RangeEnd = float.Parse(RangeData[1]);

                    return "[ RangeField : " + RangeField + " RangeStart : " + RangeStart + " RangeEnd : " + RangeEnd + "]";

                }
                return "RangeDate길이 : " + Ranges.Length.ToString();
            }

            return "Range 널";
        }

        public static Lucene.Net.Search.Sort SortParse(string Sort)
        {
            Lucene.Net.Search.Sort sr = new Lucene.Net.Search.Sort();

            if (!string.IsNullOrEmpty(Sort))
            {
                string[] Sorts = Sort.Split(new char[] { ',' });
                SortField[] sf = new SortField[Sorts.Length];

                for (int i = 0; i < Sorts.Length; ++i)
                {
                    string[] ss = Sorts[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (ss.Length == 1)
                    {
                        if (ss[0] == "_score")
                        {
                            sf[i] = SortField.FIELD_SCORE;
                        }
                        else if (ss[0] == "_doc")
                        {
                            sf[i] = SortField.FIELD_DOC;
                        }
                        else
                        {
                            sf[i] = new SortField(ss[0].Trim(), SortField.STRING, false);
                        }
                    }
                    else if (ss.Length == 2)
                    {
                        string sortd = ss[1].Trim();

                        if (sortd == "asc" || sortd == "desc")
                        {
                            bool bReverse = sortd == "desc" ? true : false;
                            sf[i] = new SortField(ss[0].Trim(), SortField.STRING, bReverse);
                        }
                        else if (sortd == "doc_asc" || sortd == "doc_desc")
                        {
                            bool bReverse = sortd == "doc_desc" ? true : false;
                            sf[i] = new SortField(ss[0].Trim(), SortField.DOC, bReverse);
                        }
                        else if (sortd == "score_asc" || sortd == "score_desc")
                        {
                            bool bReverse = sortd == "score_desc" ? true : false;
                            sf[i] = new SortField(ss[0].Trim(), SortField.SCORE, bReverse);
                        }
                        else if (sortd == "int_asc" || sortd == "int_desc")
                        {
                            bool bReverse = sortd == "int_desc" ? true : false;
                            sf[i] = new SortField(ss[0].Trim(), SortField.INT, bReverse);
                        }
                    }
                }

                sr.SetSort(sf);
            }

            return sr;
        }

        public static DicSqlType SqlParse(string sQuery)
        {
            DicSqlType Sql = new DicSqlType();

            Regex re = new Regex(
                @"(?:'(?:[^']|'')*'|-?\d+(?:\.\d+)?(?:[eE]-?\d+)?|\w+|[<>=]{2}|\S)",
                RegexOptions.IgnoreCase | RegexOptions.Multiline );

            MatchCollection mc = re.Matches(sQuery);
            string sKey = "";

            foreach (Match m in mc)
            {
                string value = m.Value.ToLower();

                if (value == "select" || value == "sel" || value == "from" || value == "where" || 
                    value == "group" || value == "order" || value =="range")
                {
                    sKey = (value == "sel") ? "select" : value;
                    Sql[sKey] = "";
                }
                else
                {
                    if (!string.IsNullOrEmpty(sKey))
                    {
                        if (sKey == "from") Sql[sKey] += m.Value;
                        else
                        {
                            string v = m.Value;
                            if (sKey == "where" && (v.ToLower() == "and" || v.ToLower() == "or" || v.ToLower() == "not"))
                            {
                                v = v.ToUpper();
                            }
                            else if (v.ToLower() == "by" && (sKey == "group" || sKey == "order"))
                            {
                                v = "";
                            }

                            Sql[sKey] += v + " ";
                        }
                    }
                }
            }

            if (Sql.Keys.Contains("where"))
            {
                Sql["where"] = Sql["where"].Replace("=", ":").Replace("'", "\"").Replace("+ ", "+");
            }

            return Sql;
        }

        public static XmlDocument GetDocment()
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml("<docs />");

            return xdoc;
        }

        public static XmlElement CreateElement(XmlDocument doc, string sName, string sValue)
        {
            XmlElement node = doc.CreateElement(sName);
            node.InnerText = sValue;

            return node;
        }

    }
}
