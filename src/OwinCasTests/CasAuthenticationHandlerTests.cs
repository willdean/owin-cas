using System;
using System.Net.Http;
using NUnit.Framework;
using Owin.Cas;

namespace Owin.Cas.Tests
{
    [TestFixture]
    public sealed class CasAuthenticationHandlerTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestConstruction()
        {
            HttpClient client = new HttpClient();
            var handler = new CasAuthenticationHandler(client, null);

        }
         
    }
}