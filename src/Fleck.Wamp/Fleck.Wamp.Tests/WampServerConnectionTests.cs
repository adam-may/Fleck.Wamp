using Fleck.Wamp.Interfaces;
using Moq;
using NUnit.Framework;

namespace Fleck.Wamp.Tests
{
    [TestFixture]
    public class WampServerConnectionTests
    {
        private IWampServerConnection _connMock;
        private Mock<IWampConnection> _innerConnMock;

        [TestFixtureSetUp]
        public void Setup()
        {
            _innerConnMock = new Mock<IWampConnection>();

            _connMock = new WampServerConnection(_innerConnMock.Object, c => { });
        }

        [Test]
        public void TestEqualityImplementations()
        {
            var s = new Mock<IWampConnection>();
            var c2 = new WampServerConnection(s.Object, c => { });

            // ReSharper disable PossibleUnintendedReferenceComparison
            // ReSharper disable EqualExpressionComparison
#pragma warning disable 252,253
            Assert.IsFalse(_connMock.Equals(c2));
            Assert.IsTrue(_connMock.Equals(_connMock));
            Assert.IsFalse(_connMock == c2);
            Assert.IsTrue(_connMock != c2);
            Assert.IsTrue(_connMock != _connMock);
            Assert.IsTrue(_connMock != _connMock);
#pragma warning restore 252,253
            // ReSharper restore EqualExpressionComparison
            // ReSharper restore PossibleUnintendedReferenceComparison
        }
    }
}
