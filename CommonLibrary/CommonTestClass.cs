using CommonModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary
{
    public class CommonTestClass : ICommonTest
    {
        public CommonTestClass()
        {

        }

        public string CommonTestMethod()
        {
            return "CommonLibrary - this is common test method from common class version 1";
        }

    }
}
