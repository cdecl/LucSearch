using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

using System.ServiceModel.Web;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace LucSearchAPI
{
    // 참고: 여기서 인터페이스 이름 "IService1"을 변경하는 경우 Web.config에서 "IService1"에 대한 참조도 업데이트해야 합니다.
    [ServiceContract]
    public interface ISearch
    {

        [OperationContract]
        [WebInvoke(
            BodyStyle = WebMessageBodyStyle.Bare,
            Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            UriTemplate = "/root"
        )]
        string GetRootDir();

        [OperationContract]
        [WebInvoke(
            BodyStyle = WebMessageBodyStyle.Bare,
            Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            UriTemplate = "/help"
        )]
        string QueryHelp();


        [OperationContract]
        [WebInvoke(
            BodyStyle = WebMessageBodyStyle.Bare,
            Method = "GET",
            ResponseFormat = WebMessageFormat.Xml,
            UriTemplate = "/xml/{pos}/{count}/?query={query}"
        )]
        XElement SearchQuery(string query, string pos, string count);


        [OperationContract]
        [WebInvoke(
            BodyStyle = WebMessageBodyStyle.Bare,
            Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/json/{pos}/{count}/?query={query}"
        )]
        Stream SearchQueryJson(string query, string pos, string count);


        [OperationContract]
        [WebInvoke(
            BodyStyle = WebMessageBodyStyle.Bare,
            Method = "GET",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/jsonp/{pos}/{count}/?query={query}&callback={callback}"
        )]
        Stream SearchQueryJsonP(string query, string callback, string Pos, string Count);
    }

}
