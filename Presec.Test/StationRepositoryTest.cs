using Presec;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using Microsoft.Data.Services.Toolkit.QueryModel;
using Presec.Models.ServiceModels;
using System.Collections.Generic;

namespace Presec.Test
{
    
    
    /// <summary>
    ///Это класс теста для StationRepositoryTest, в котором должны
    ///находиться все модульные тесты StationRepositoryTest
    ///</summary>
    [TestClass()]
    public class StationRepositoryTest
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
        public void GetAllByAddr()
        {
            StationRepository target = new StationRepository(); // TODO: инициализация подходящего значения
            ODataQueryOperation operation = new ODataQueryOperation(); // TODO: инициализация подходящего значения
            operation.ContextParameters = new Dictionary<string, string>();
            operation.ContextParameters.Add("addr", "винокур");
            IEnumerable<Station> actual;
            actual = target.GetAll(operation);
            Assert.AreEqual(2, actual.Count());
            var first = actual.First();
            Assert.AreEqual(2173, first.id);
            Assert.AreEqual(5, first.near.Count());

            //Assert.Inconclusive("Проверьте правильность этого метода теста.");
        }
    }
}
