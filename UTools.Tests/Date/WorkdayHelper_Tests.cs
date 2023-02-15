using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UTools.Date;

namespace UTools.Tests.Date
{

    /// <summary>
    /// 工作日帮助类测试
    /// </summary>
    public class WorkdayHelper_Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void TestNextWorkDay()
        {
            // 测试第二天就是下一个工作日的情况
            var dateTime1 = new DateTime(2023, 2, 15);
            var nextWorkDay1 = dateTime1.NextWorkday();
            Assert.AreEqual(dateTime1.Date.AddDays(1).ToString(), nextWorkDay1.ToString());

            // 测试在周五时下一个工作日是之后第三天的情况
            var dateTime2 = new DateTime(2023, 2, 17);
            var nextWorkDay2 = dateTime2.NextWorkday();
            Assert.AreEqual(dateTime2.Date.AddDays(3).ToString(), nextWorkDay2.ToString());

            // 测试五一放三天的情况
            var dateTime3 = new DateTime(2023, 5, 1);
            var nextWorkDay3 = dateTime3.NextWorkday();
            Assert.AreEqual(dateTime3.Date.AddDays(3).ToString(), nextWorkDay3.ToString());

            // 测试五一放三天后周六补班的情况
            var dateTime4 = new DateTime(2023, 5, 5);
            var nextWorkDay4 = dateTime4.NextWorkday();
            Assert.AreEqual(dateTime4.Date.AddDays(1).ToString(), nextWorkDay4.ToString());

            Assert.Pass();
        }
    }
}
