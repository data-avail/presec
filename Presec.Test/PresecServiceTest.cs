using Presec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Presec.Service.Models;
using System.Collections.Generic;
using Presec.Service.Repositories;
using Presec.Service.Cahching;
using System.Configuration;

namespace Presec.Test
{
    
    
    /// <summary>
    ///Это класс теста для StationRepositoryTest, в котором должны
    ///находиться все модульные тесты StationRepositoryTest
    ///</summary>
    [TestClass()]
    public class PresecServiceTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Check all stations for addresses like винокур
        ///</summary>
        [TestMethod()]
        public void GetAllByAddr_винокур()
        {
            StationRepository target = new StationRepository(); 
            ODataQueryOperation operation = new ODataQueryOperation(); 
            operation.ContextParameters = new Dictionary<string, string>();
            operation.ContextParameters.Add("addr", "винокур");
            IEnumerable<Station> actual;
            actual = target.GetAll(operation);
            Assert.AreEqual(2, actual.Count());
            var first = actual.First();
            Assert.AreEqual(2173, first.id);
            Assert.AreEqual(5, first.near.Count());

            
        }

        /// <summary>
        ///Check all stations for addresses like ник
        ///</summary>
        [TestMethod()]
        public void GetAllByAddr_ник()
        {
            StationRepository target = new StationRepository();
            ODataQueryOperation operation = new ODataQueryOperation();
            operation.ContextParameters = new Dictionary<string, string>();
            operation.ContextParameters.Add("addr", "ник");
            IEnumerable<Station> actual;
            actual = target.GetAll(operation);
            Assert.AreEqual(5, actual.Count());
        }


        /// <summary>
        ///Check for case when none station found
        ///</summary>
        [TestMethod()]
        public void GetAllByAddrNotFound()
        {
            StationRepository target = new StationRepository();
            ODataQueryOperation operation = new ODataQueryOperation();
            operation.ContextParameters = new Dictionary<string, string>();
            operation.ContextParameters.Add("addr", "гебельс");
            IEnumerable<Station> actual;
            actual = target.GetAll(operation);
            Assert.AreEqual(0, actual.Count());
        }

        [TestMethod()]
        public void GetSuggestions_вин()
        {
            GeoSuggestionRepository target = new GeoSuggestionRepository();
            ODataQueryOperation operation = new ODataQueryOperation();
            operation.ContextParameters = new Dictionary<string, string>();
            operation.ContextParameters.Add("term", "россия, москва, виноку");
            IEnumerable<GeoSuggestion> actual;
            actual = target.GetAll(operation);
            Assert.AreNotEqual(0, actual.Count());
            Assert.AreNotEqual(null, actual.First().id);
            Assert.AreNotEqual(null, actual.First().refer);
            Assert.AreNotEqual(null, actual.First().term);
            Assert.AreNotEqual(null, actual.First().descr);
        }

        [TestMethod()]
        public void GetSuggestions_орехов()
        {
            GeoSuggestionRepository target = new GeoSuggestionRepository();
            ODataQueryOperation operation = new ODataQueryOperation();
            operation.ContextParameters = new Dictionary<string, string>();
            operation.ContextParameters.Add("term", "россия, москва, орехов");
            IEnumerable<GeoSuggestion> actual;
            actual = target.GetAll(operation);
            Assert.AreNotEqual(0, actual.Count());
            Assert.AreNotEqual(null, actual.First().id);
            Assert.AreNotEqual(null, actual.First().refer);
            Assert.AreNotEqual(null, actual.First().term);
            Assert.AreNotEqual(null, actual.First().descr);

        }


        [TestMethod()]
        public void CheckDataServiceProviders()
        {
            CustomDataServiceProvider provider = new CustomDataServiceProvider();
            var actual = provider.RepositoryFor("Presec.Service.Models.Station");
            Assert.AreNotEqual(null, actual);
            actual = provider.RepositoryFor("Presec.Service.Models.GeoSuggestion");
            Assert.AreNotEqual(null, actual);
        }

        /*
        [TestMethod()]
        public void CheckSuggestionCache()
        {
            //Configuration variables from add-ons (including database connectionstrings) are NOT available in the application configuration file while tests are executed
            if (ConfigurationManager.AppSettings["Environment"] != "Debug")
                return;

            using (var cache = new Cache<IEnumerable<GeoSuggestion>>())
            {
                var term = Guid.NewGuid().ToString();

                //try
                {

                    var actual = cache.Get(term);
                    Assert.AreEqual(null, actual);
                    var geoSuggestion1 = new GeoSuggestion { id = "id1", descr = "descr1", refer = "refer1", term = term };
                    var geoSuggestion2 = new GeoSuggestion { id = "id2", descr = "descr2", refer = "refer2", term = term };
                    cache.Set(term, new [] {geoSuggestion1, geoSuggestion2});
                    actual = cache.Get(term);
                    Assert.AreNotEqual(null, actual);
                    var act = actual.ToArray();
                    Assert.AreEqual(2, act.Length);
                    Assert.AreEqual(geoSuggestion1.id, act[0].id);
                    Assert.AreEqual(geoSuggestion1.descr, act[0].descr);
                    Assert.AreEqual(geoSuggestion1.refer, act[0].refer);
                    Assert.AreEqual(geoSuggestion1.term, act[0].term);
                    Assert.AreEqual(geoSuggestion2.id, act[1].id);
                    Assert.AreEqual(geoSuggestion2.descr, act[1].descr);
                    Assert.AreEqual(geoSuggestion2.refer, act[1].refer);
                    Assert.AreEqual(geoSuggestion2.term, act[1].term);
                    cache.Remove(term);
                    actual = cache.Get(term);
                    Assert.AreEqual(null, actual);
                    cache.Remove(term);
                }
            }
        }
         */

        [TestMethod()]
        public void CheckGetAddrRegex()
        {
            string[] inputs = new[] { 
                "винок", "улица винокурова", //
                "хошемин", "площадь хошемина", //
                "дагестанский", "проезд дагестанский", //
                "ул. сепаратистская", "ул сепарат", //
                "улица сепаратистская", "сепарат улица", //
                "винок 22", "улица винокурова д. 22", //
                "винок дом 22", "винок д. 22к2",
                "Энтузиастов ш дом 22", "энтузиастов шоссе 22",
                "Ленинградское шоссе, 21"
            }; //

            string[] expecteds = new[] {
                "|винок", "улица|винокурова", //
                "|хошемин", "площадь|хошемина", //
                "|дагестанский", "проезд|дагестанский", //
                "улица|сепаратистская", "улица|сепарат", //
                "улица|сепаратистская", "улица|сепарат", //
                "|винок,|22", "улица|винокурова,дом|22", //
                "|винок,дом|22", "|винок,дом|22к2",
                "шоссе|энтузиастов,дом|22", "шоссе|энтузиастов,|22",
                "шоссе|ленинградское,|21"
            }; //


            
            StationRepository rep = new  StationRepository();
            
            for (int i = 0; i < inputs.Length; i++)
            {
                var expected = expecteds[i].Split(',');
                if (expected.Length == 1) expected = new[] { expected[0], null };
                var actual = rep.GetAddr(inputs[i]);
                Assert.AreEqual(expected[0], actual[0]);
                Assert.AreEqual(expected[1], actual[1]);
            }

            
        }

        [TestMethod()]
        public void GetByGoogleRef_лисичанская()
        {
            //ClRBAAAA8fiTTf0qlAtK1NnUblpdGtY5T_se4KtvLvxTJNjs2FnIIWK2TDgNdRcgEps1NCdPNTrHv3k1GOXlrnCvjVdGLBs_H_ToMeNfZ3MxZsqmIpgSEHm3jvB1DyRNfk0iVWaRgVkaFCmpKkegcbJuY17m_yrg9a5xzLuA
            StationRepository target = new StationRepository();
            ODataQueryOperation operation = new ODataQueryOperation();
            operation.ContextParameters = new Dictionary<string, string>();
            operation.ContextParameters.Add("addr", "россия, москва, лисичанская улица");
            operation.ContextParameters.Add("gref", "ClRBAAAA8fiTTf0qlAtK1NnUblpdGtY5T_se4KtvLvxTJNjs2FnIIWK2TDgNdRcgEps1NCdPNTrHv3k1GOXlrnCvjVdGLBs_H_ToMeNfZ3MxZsqmIpgSEHm3jvB1DyRNfk0iVWaRgVkaFCmpKkegcbJuY17m_yrg9a5xzLuA");
            IEnumerable<Station> actual;
            actual = target.GetAll(operation);
            Assert.AreEqual(5, actual.Count());

        }

        [TestMethod()]
        public void GetAllMoscowMapRegions()
        {
            MapRegionRepository target = new MapRegionRepository();
            MapRegion actual = null;
            
            actual = target.GetOne("37.398300804197795;55.51375574905723;38.08494631201029;55.90144684501253;street");
            Assert.AreEqual(1999, actual.coords.Count());

            actual = target.GetOne("37.398300804197795;55.51375574905723;38.08494631201029;55.90144684501253;district");
            Assert.AreEqual(113, actual.coords.Count());
            

            actual = target.GetOne("37.398300804197795;55.51375574905723;38.08494631201029;55.90144684501253;city");
            Assert.AreEqual(8, actual.coords.Count());

        }

    }
}

