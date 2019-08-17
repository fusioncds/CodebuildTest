using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using TestAPI.Utils;

namespace TestAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/TestDLL")]
    public class TestDLLController : BaseController
    {
        public TestDLLController(IConfiguration configuration) : base(configuration)
        {
            lstMessages.Add("Constructor TestDLLController \n");
        }

        [HttpGet]
        public List<string> Get()
        {
            List<string> messages = new List<string>();
            string output = "";
            try
            {
                string methodOutput = "my-test";//icommonTest.CommonTestMethod();
                lstMessages.Add("method output :" + methodOutput);

            }
            catch (Exception ex)
            {

                lstMessages.Add(ex.Message);

            }

            foreach (var item in lstMessages)
            {
                messages.Add(item);
                //output += item + "<BR>";
            }
            lstMessages.Clear();
            return messages;

        }

    }
}