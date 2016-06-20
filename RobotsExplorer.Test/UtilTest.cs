using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotsExplorer.Model;
using System.Collections.Generic;

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
    }
}