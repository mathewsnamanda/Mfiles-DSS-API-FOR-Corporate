using MFilesAPI;
using MfilesDSS.Models;
using MfilesDSS.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MfilesDSS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;
        private readonly IConfiguration _configuration;

        public ValuesController(ILogger<ValuesController> logger, IConfiguration Configuration)
        {
            _logger = logger;
            _configuration = Configuration ?? throw new ArgumentNullException(nameof(Configuration));
        }
        // GET: api/<ValuesController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string MfilesUsername = this._configuration.GetConnectionString("Mfiles_Username");
            string MfilesPassword = this._configuration.GetConnectionString("Mfiles_Password");
            string MfilesGuid = this._configuration.GetConnectionString("Mfiles_GuidB");
            string MfilesUrl = this._configuration.GetConnectionString("Mfiles_MfilesUrl");


            var options = new RestClientOptions("https://dsscallcorp.techedge.dev")
            {
                MaxTimeout = -1,
            };
            var client = new RestClient(options);
            var request = new RestRequest($"/api/values/{MfilesGuid}", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
           
            if(response.StatusCode==System.Net.HttpStatusCode.OK)
            {

                var myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(response.Content);

                if(myDeserializedClass!=null)
                {
                    var mfServerApplication = new MFilesServerApplication();

                    // Connect to a local server using the default parameters (TCP/IP, localhost, current Windows user).
                    // https://www.m-files.com/api/documentation/index.html#MFilesAPI~MFilesServerApplication~Connect.html
                    mfServerApplication.Connect(MFAuthType.MFAuthTypeSpecificMFilesUser,
                                                UserName: MfilesUsername,
                                                Password: MfilesPassword,
                                                Domain: "MyDomain",
                                                ProtocolSequence: "ncacn_ip_tcp", // Connect using TCP/IP.
                                                NetworkAddress: MfilesUrl, // Connect to m-files.mycompany.com
                                                Endpoint: "2266");
                    try
                    {
                        //connecting to vault
                        var vault = mfServerApplication.LogInToVault(MfilesGuid);

                        foreach (var item in myDeserializedClass)
                        {
                            IMfiles mfiles = new MfilesClass();
                            mfiles.mfiles(vault,item);
                        }


                    }
                    catch (Exception ex)
                    {

                    }
                }
               
            }
                return Ok();
        }

    }
}
