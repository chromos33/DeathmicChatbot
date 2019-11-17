using BobDeathmic.Helper.EventCalendar;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BobDeathmic.Tests.Helper.EventCalendar
{
    [TestFixture]
    public class MutableTupleTests
    {
        [Test]
        public void MutableTuple_ChangeValue_ExpectedChange()
        {
            MutableTuple<string, string> test = new MutableTuple<string, string>("V1", "V2");
            test.First = "Test";
            Assert.That(test.First,Is.EqualTo("Test"));
            Assert.That(test.Second,Is.EqualTo("V2"));
        }
        
    }
}
