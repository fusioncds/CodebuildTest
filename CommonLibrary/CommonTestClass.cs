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
            return "CommonLibrary.CommonTestMethod()-v1.1";
        }

    }
}
