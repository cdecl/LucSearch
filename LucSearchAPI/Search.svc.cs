using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.Xml;
using System.Xml.Linq;
using System.IO;

using LucSearchLib;


namespace LucSearchAPI
{
    // 참고: 여기서 클래스 이름 "Service1"을 변경하는 경우 Web.config 및 연결된 .svc 파일에서 "Service1"에 대한 참조도 업데이트해야 합니다.
    public class Search : ISearch
    {
        public string GetRootDir()
        {
            return System.Configuration.ConfigurationManager.AppSettings["ROOT"];
        }

        public string QueryHelp()
        {
            return LucSearchLib.CJKSearch.QueryHelp();
        }

        public string GetLogFile()
        {
            return System.Configuration.ConfigurationManager.AppSettings["LOG"];
        }

        public XElement SearchQuery(string Sql, string Pos, string Count)
        {
            XmlDocument doc = null;
           
            try
            {
                GLASS.FileLog log = new GLASS.FileLog(GetLogFile());
                log.WriteLine(string.Format("{0}/{1},{2}", Pos, Count, Sql));
                
                doc = CJKSearch.SearchQuery(GetRootDir(), Sql, int.Parse(Pos), int.Parse(Count));
            }
            catch (Exception)
            {
            }

            return XElement.Load(doc.CreateNavigator().ReadSubtree()); ;
        }

        public Stream SearchQueryJson(string Sql, string Pos, string Count)
        {
            string sJson = "";

            try
            {
                GLASS.FileLog log = new GLASS.FileLog(GetLogFile());
                log.WriteLine(string.Format("{0}/{1},{2}", Pos, Count, Sql));

                XmlDocument doc = CJKSearch.SearchQuery(GetRootDir(), Sql, int.Parse(Pos), int.Parse(Count));
                sJson = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);

            }
            catch (Exception)
            {
            }

            System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            return new MemoryStream(Encoding.UTF8.GetBytes(sJson));
        }

        public Stream SearchQueryJsonP(string Sql, string callback, string Pos, string Count)
        {
            string sJson = "";
            
            try
            {
                GLASS.FileLog log = new GLASS.FileLog(GetLogFile());
                log.WriteLine(string.Format("{0}/{1},{2}", Pos, Count, Sql));

                XmlDocument doc = CJKSearch.SearchQuery(GetRootDir(), Sql, int.Parse(Pos), int.Parse(Count));
                sJson = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc);
                sJson = string.Format("{0}({1});", callback, sJson);
            }
            catch (Exception)
            {
            }

            System.ServiceModel.Web.WebOperationContext.Current.OutgoingResponse.ContentType = "application/json; charset=utf-8";
            return new MemoryStream(Encoding.UTF8.GetBytes(sJson));
        }
    }
}
