using Microsoft.VisualStudio.TestTools.UnitTesting;
using EstatusCFDI.Sat.Helpers; // Assuming SatQueryBuilder is in this namespace

namespace EstatusCFDI.Sat.Tests
{
    [TestClass]
    public class SatQueryBuilderTests
    {
        private const string DefaultRfcEmisor = "XEXX010101000";
        private const string DefaultRfcReceptor = "XEXX010101001";
        private const string DefaultUuid = "TEST-UUID";

        private void RunTest(decimal montoTotal, string expectedTotalFormateado)
        {
            // Arrange
            string rfcEmisor = DefaultRfcEmisor;
            string rfcReceptor = DefaultRfcReceptor;
            string uuid = DefaultUuid;
            string expectedQuery = $"?re={rfcEmisor}&rr={rfcReceptor}&tt={expectedTotalFormateado}&id={uuid}";

            // Act
            string actualQuery = SatQueryBuilder.ConstruirCadenaParams(rfcEmisor, rfcReceptor, montoTotal, uuid);

            // Assert
            Assert.AreEqual(expectedQuery, actualQuery, $"Test failed for montoTotal: {montoTotal}");
        }

        [TestMethod]
        public void TestMethod_ValorTipicoConDecimales()
        {
            // e.g., total=123.45m -> "?re=XEXX010101000&rr=XEXX010101001&tt=000123,450000&id=VALID-UUID"
            // (using DefaultUuid for consistency in tests)
            RunTest(123.45m, "000123,450000");
        }

        [TestMethod]
        public void TestMethod_ValorEntero()
        {
            // e.g., total=789m -> "...&tt=000789,000000&..."
            RunTest(789m, "000789,000000");
        }

        [TestMethod]
        public void TestMethod_ValorPequeño()
        {
            // e.g., total=0.123m -> "...&tt=000000,123000&..."
            RunTest(0.123m, "000000,123000");
        }

        [TestMethod]
        public void TestMethod_MenosDeSeisDecimales()
        {
            // e.g., total=1.2345m -> "...&tt=000001,234500&..."
            RunTest(1.2345m, "000001,234500");
        }

        [TestMethod]
        public void TestMethod_ValorCero()
        {
            // e.g., total=0m -> "...&tt=000000,000000&..."
            RunTest(0m, "000000,000000");
        }

        [TestMethod]
        public void TestMethod_ValorGrande()
        {
            // e.g., total=1234567.89m -> "...&tt=1234567,890000&..."
            // The integer part "000000" in format string is a minimum, actual integer part will be used if larger.
            RunTest(1234567.89m, "1234567,890000");
        }

        [TestMethod]
        public void TestMethod_ValorConSeisDecimalesExactos()
        {
            // e.g., total=12.123456m -> "...&tt=000012,123456&..."
            RunTest(12.123456m, "000012,123456");
        }
    }
}
