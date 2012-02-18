using Presec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Presec.Service.Models;
using System.Collections.Generic;
using Presec.Service.Repositories;

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
            operation.ContextParameters.Add("term", "россия, москва, вин");
            IEnumerable<GeoSuggestion> actual;
            actual = target.GetAll(operation);
            Assert.AreNotEqual(0, actual.Count());
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

    }
}
