using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Lucene.Net;
using Lucene.Net.Index;
using Lucene.Net.Analysis;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;

using System.Data;
using System.Data.Odbc;


namespace LucIndexing
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("Lucene Indexing 작업 ");
            Console.WriteLine(" ");
            Console.WriteLine("## [Directory]에 인덱싱");
            Console.WriteLine("LucIndexing.exe INDEX [Directory] [ConfigFile]");
            Console.WriteLine(" ");
            Console.WriteLine("## [Directory]에 인덱싱 추가 (RAMDirectory 이용)");
            Console.WriteLine("LucIndexing.exe RAMCREATE [Directory] [ConfigFile]");
            Console.WriteLine("LucIndexing.exe RAMMERGE [Directory] [ConfigFile]");
            Console.WriteLine("LucIndexing.exe RAMINSERT [Directory] [ConfigFile]");
            Console.WriteLine(" ");
            Console.WriteLine("-- [SrcDirectory]에서 [Directory]으로 인덱싱 추가");
            Console.WriteLine("LucIndexing.exe CREATE [Directory] [SrcDirectory]");
            Console.WriteLine("LucIndexing.exe MERGE [Directory] [SrcDirectory]");
            Console.WriteLine("LucIndexing.exe INSERT [Directory] [SrcDirectory]");
            Console.WriteLine(" ");

            Console.WriteLine("## ConfigFile ");
            Console.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            Console.WriteLine("<LuceneIndex>");
            Console.WriteLine("  <Source ConnectionString=\"\">");
            Console.WriteLine("    <Query>SQL</Query>");
            Console.WriteLine("  </Source>");
            Console.WriteLine("  ");
            Console.WriteLine("  <Document>");
            Console.WriteLine("    <Add Name=\"Field\" Store=\"Yes\" Index=\"NO\" />  ");
            Console.WriteLine("    <Add Name=\"Field\" Store=\"Yes\" Index=\"ANALYZED\" />  ");
            Console.WriteLine("    <Add Name=\"Field\" Store=\"Yes\" Index=\"NOT_ANALYZED\" />  ");
            Console.WriteLine("    <!-- ... -->");
            Console.WriteLine("  </Document>");
            Console.WriteLine("</LuceneIndex>");

        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 2)
                {
                    Usage();
                    return;
                }

                string sCommand = args[0].ToUpper();
                string sDirectory = args[1];
                string sCommandQuery = "";
                DateTime dtStart;

                if (sCommand == "INDEX" || sCommand == "RAMCREATE" || sCommand == "RAMMERGE" || sCommand == "RAMINSERT")
                {
                    string sConfig = args[2];

                    XmlDocument doc = GetConfig(sConfig);
                    string sConnectionString = doc.SelectSingleNode("LuceneIndex/Source").Attributes["ConnectionString"].Value;
                    sCommandQuery = doc.SelectSingleNode("LuceneIndex/Source/Query").InnerText;

                    Console.WriteLine("데이터 수집중..");
                    dtStart = DateTime.Now;
                    DataSet ds = GetSourceData(sConnectionString, sCommandQuery);
                    Console.WriteLine("데이터 수집 : {0}초 ", DateTime.Now.Subtract(dtStart).TotalSeconds);

                    if (ds == null)
                    {
                        throw new Exception("데이터가 없습니다.");
                    }

                    Analyzer analyzer = new Lucene.Net.Analysis.CJK.CJKAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

                    if (sCommand == "INDEX")
                    {
                        using (Lucene.Net.Store.Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(sDirectory)))
                        {
                            dtStart = DateTime.Now;
                            Indexing(sCommand, analyzer, directory, ds, doc);
                            Console.WriteLine("데이터 인덱싱 : {0}초 ", DateTime.Now.Subtract(dtStart).TotalSeconds);
                        }
                    }
                    else if (sCommand == "RAMCREATE" || sCommand == "RAMMERGE" || sCommand == "RAMINSERT")
                    {
                        using (Lucene.Net.Store.Directory directory = FSDirectory.Open(new System.IO.DirectoryInfo(sDirectory)))
                        using (Lucene.Net.Store.Directory RAMDir = new RAMDirectory())
                        {
                            dtStart = DateTime.Now;
                            Indexing(sCommand, analyzer, RAMDir, ds, doc);
                            Console.WriteLine("데이터 인덱싱 : {0}초 ", DateTime.Now.Subtract(dtStart).TotalSeconds);

                            Merging(sCommand, analyzer, directory, RAMDir);
                        }
                    }
                }
                else if (sCommand == "MERGE" || sCommand == "INSERT" || sCommand == "CREATE") 
                {
                    string sSrcDir = args[2];

                    using (Analyzer analyzer = new Lucene.Net.Analysis.CJK.CJKAnalyzer(Lucene.Net.Util.Version.LUCENE_30))
                    using (Lucene.Net.Store.Directory SrcDir = FSDirectory.Open(new System.IO.DirectoryInfo(sSrcDir)))
                    using (Lucene.Net.Store.Directory DescDir = FSDirectory.Open(new System.IO.DirectoryInfo(sDirectory)))
                    {
                        dtStart = DateTime.Now;
                        Merging(sCommand, analyzer, DescDir, SrcDir);
                        Console.WriteLine("데이터 인덱스 추가 : {0}초 ", DateTime.Now.Subtract(dtStart).TotalSeconds);
                    }
                }
                else
                {
                    throw new Exception("Command 가 올바르지 않습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        private static void Merging(string sCommand, Analyzer analyzer, Lucene.Net.Store.Directory DescDir, Lucene.Net.Store.Directory SrcDir)
        {
            bool bCreate = false;

            if (sCommand == "CREATE" || sCommand == "RAMCREATE")
            {
                bCreate = true;
            }
            
            using (IndexReader idxReader = IndexReader.Open(SrcDir, true))
            using (IndexWriter writer = new IndexWriter(DescDir, analyzer, bCreate, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                int docs = idxReader.NumDocs();
                Console.WriteLine("INDEXING {0} {1}", sCommand, docs);

                if (sCommand == "MERGE" || sCommand == "RAMMERGE")
                {
                    int i = 0;
                    for (i = 0; i < docs; ++i)
                    {
                        string sValue = idxReader.Document(i).Get("id");
                        writer.DeleteDocuments(new Term("id", sValue));

                        if ((i % 1000) == 0)
                        {
                            Console.Write("DeleteDocuments {0}                \r", i);
                        }
                    }
                    //Commit 추가해야 삭제 완료 (2015.11.05 홍영선 추가)
                    writer.Commit();
                    Console.WriteLine("");
                    Console.WriteLine("DeleteDocuments {0} END", i);
                }

                // 성능향상 튜닝
                Console.WriteLine("INDEXING AddIndexesNoOptimize()");
                DateTime dtStart = DateTime.Now;
                writer.AddIndexesNoOptimize(SrcDir);
                Console.WriteLine("AddIndexesNoOptimize : {0}초 ", DateTime.Now.Subtract(dtStart).TotalSeconds);

                Console.WriteLine("INDEXING Optimize()");
                dtStart = DateTime.Now;
                writer.Optimize();
                Console.WriteLine("Optimize : {0}초 ", DateTime.Now.Subtract(dtStart).TotalSeconds);
            }
        }

        static XmlDocument GetConfig(string sConfig)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(sConfig);

            return doc;
        }

        static DataSet GetSourceData(string sConnectionSting, string sQuery)
        {
            DataSet ds = null;

            using (GLASS.AdoNetOdbc ado = new GLASS.AdoNetOdbc())
            {
                ado.Open(sConnectionSting);

                OdbcCommand cmd = new OdbcCommand(sQuery);
                cmd.CommandTimeout = 60 * 10; // 10분

                ds = ado.ExecuteDataSet(cmd);
            }

            return ds;
        }

        static void Indexing(string sCommand, Analyzer analyzer, Lucene.Net.Store.Directory directory, DataSet ds, XmlDocument docAdd)
        {
            // 인덱싱 종류 사전화
            Dictionary<string, Field.Index> DicIndex = new Dictionary<string, Field.Index>();
            DicIndex.Add("NO", Field.Index.NO);
            DicIndex.Add("ANALYZED", Field.Index.ANALYZED);
            DicIndex.Add("NOT_ANALYZED", Field.Index.NOT_ANALYZED);
    
            Console.WriteLine("Lucene.Net.Analysis.CJK - INDEXING START");

            bool bCreate = true;
            using (IndexWriter writer = new IndexWriter(directory, analyzer, bCreate, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                int nRow = 0;
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    XmlNodeList nlist = docAdd.SelectNodes("LuceneIndex/Document/Add");
                    Document doc = new Document();

                    foreach (XmlNode node in nlist)
                    {
                        string sName = node.Attributes["Name"].Value;
                        string sIndex = node.Attributes["Index"].Value;
                        string sStore = node.Attributes["Store"].Value;
                        string sIsNumber = node.Attributes["IsNumber"] != null ? node.Attributes["IsNumber"].Value : "NO";

                        Field.Store store = Field.Store.YES;

                        if (sStore.ToUpper() != "YES")
                        {
                            store = Field.Store.NO;
                        }

                        if (sIsNumber =="YES")
                        {
                            NumericField numField = new NumericField(sName, Field.Store.YES, true);
                            numField.SetFloatValue(Int32.Parse(r[sName].ToString()));
                            doc.Add(numField);
                        }
                        else
                        {
                            doc.Add(new Field(sName, r[sName].ToString(), store, DicIndex[sIndex]));
                        }
                    }
                    
                    writer.AddDocument(doc);

                    if ((nRow % 10000) == 0)
                    {
                        Console.Write("INDEXING {0}                \r", nRow);
                    }

                    nRow++;
                }
                Console.WriteLine("");
                Console.WriteLine("INDEXING {0} END", nRow);

                Console.WriteLine("INDEXING Optimize");
                DateTime dtStart = DateTime.Now;
                writer.Optimize();
                Console.WriteLine("Optimize : {0}초 ", DateTime.Now.Subtract(dtStart).TotalSeconds);
                
            }
        }
    }
}
