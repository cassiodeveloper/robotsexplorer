using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotsExplorer.Model;
using System.Collections.Generic;
using System.Xml;
using System.Net;

namespace RobotsExplorer.Test
{
    [TestClass]
    public class UtilTest
    {
        [TestMethod]
        public void ReadFileTest()
        {
            Domain domainNames = new Domain();
            domainNames.DomainNames = new List<string>();

            Util.Util.ReadTextFileAndFillDomainsList(@"C:\Users\mtzcpd1053\Documents\Domains.txt", domainNames);

            Assert.IsTrue(domainNames.DomainNames.Count > 0);
        }

        [TestMethod]
        public void DownloadSitemapXML()
        {
            HttpManager.HttpManager manager = new HttpManager.HttpManager();
            WebRequest request = manager.WebRequestFactory("http://www.ikea.com/sitemap.xml", "http:mtzcpd1053:Cvc1723@:cvcproxy01.cvc.com.br:8080", string.Empty, 0);

            Assert.IsNotNull(Util.Util.LoadSiteMapXml(Util.Util.ParseResponseStreamToText(request.GetResponse())));
        }
    }
}