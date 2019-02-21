/**
 * This is the controller to handle Service Contracts.
 * @type controller
 * @author Tommy Cornett <thcornett@deloitte.com>
 **/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AssetTrackPOC.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AssetTrackPOC.Controllers
{

    public sealed class ServiceClient
    {
        #region Declarations

        const int MOCK_SERVICE_DELAY = 1500; //delay in ms for mocking service response times

        static object _padLock = new object();
        static volatile ServiceClient _instance;

        List<ServiceEnvironment> _environments = new List<ServiceEnvironment>();
        int _selectedEnvironment;

        #endregion

        //*******************************************************

        #region Constructor

        private ServiceClient()
        {
            //No public consturctor for a singleton class

            //set up service environments
#if DEBUG
            _environments.Add(new ServiceEnvironment()
            {
                EnvironmentName = "MOCK",
                BaseURL = Constants.domain,
                IsMock = true
            });

            _environments.Add(new ServiceEnvironment()
            {
                EnvironmentName = "DEV",
                BaseURL = Constants.domain//"http://ec2-52-91-125-113.compute-1.amazonaws.com:3000/api/"
            });
#else
            _environments.Add(new ServiceEnvironment()
            {
                EnvironmentName = "PROD",
                BaseURL = "http://localhost/"
            });
#endif
        }

        #endregion

        //*******************************************************

        #region Singleton Instance

        public static ServiceClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_padLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ServiceClient();
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        //*******************************************************

        #region Public Properties

        public ServiceEnvironment CurrentEnvironment
        {
            get
            {
                return _environments[_selectedEnvironment];
            }
        }

        #endregion

        //*******************************************************

        #region Private Methods

        async Task<WebServiceResponse> CallWebService(string servicePath, string httpMethod, string postData)
        {
            WebServiceResponse response = new WebServiceResponse();

            try
            {
                var baseAddress = new Uri(CurrentEnvironment.BaseURL);

                using (var handler = new HttpClientHandler())
                {
                    using (var httpClient = new HttpClient(handler))
                    {
                        httpClient.BaseAddress = baseAddress;

                        //set request headers
                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage webResponse = null;

                        switch (httpMethod.ToUpper())
                        {
                            case "GET":
                                webResponse = await httpClient.GetAsync(servicePath);
                                break;

                            case "POST":
                                webResponse = await httpClient.PostAsync(servicePath, new StringContent(postData, System.Text.Encoding.UTF8, "application/json"));
                                break;

                            case "PUT":
                                webResponse = await httpClient.PutAsync(servicePath, new StringContent(postData, System.Text.Encoding.UTF8, "application/json"));
                                break;

                            case "DELETE":
                                webResponse = await httpClient.DeleteAsync(servicePath);
                                break;

                            default:
                                throw new Exception("Invalid HTTP Method Parementer");
                        }

                        response.ResponseCode = (int)webResponse.StatusCode;
                        Debug.WriteLine(string.Format("HttpResponse -> Status Code: {0}", response.ResponseCode));

                        //if (webResponse.IsSuccessStatusCode) // Commented to receive Error message from server
                        //{
                            response.ResponseString = await webResponse.Content.ReadAsStringAsync();

                            Debug.WriteLine(string.Format("HttpResponse -> {0}", response.ResponseString));

                       // }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("** EXCEPTION :: ServiceClient.CallWebService :: {0}",
                                              ex.Message));
            }

            return response;
        }

        #endregion

        //*******************************************************

        #region Public Methods

        /// <summary>
        /// Sets the environment.
        /// </summary>
        /// <param name="environmentID">Environment identifier.</param>
        public void SetEnvironment(int environmentID)
        {
            _selectedEnvironment = environmentID;
        }

        /// <summary>
        /// Login the specified username and password.
        /// </summary>
        /// <returns>The login.</returns>
        /// <param name="assetId">AssetId.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public async Task<WebServiceResponse> LoginDevice(string assetId, string username, string password)
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();

                switch (username.ToLower())
                {
                    case "baduser":
                        response.ResponseCode = 401;
                        response.ResponseString = "";
                        break;
                    case "George":
                        response.ResponseCode = 200;
                        response.ResponseString = "{roles:[\"2\"]}";
                        break;
                    case "Virginia Wolf":
                        response.ResponseCode = 200;
                        response.ResponseString = "{roles:[\"1\"]}";
                        break;
                    case "Fred":
                        response.ResponseCode = 200;
                        response.ResponseString = "{roles:[\"0\"]}";
                        break;
                    case "Harold":
                        response.ResponseCode = 200;
                        response.ResponseString = "{roles:[\"0\"]}";
                        break;
                    case "Scott Beo":
                        response.ResponseCode = 200;
                        response.ResponseString = "{roles:[\"0\"]}";
                        break;
                    default:
                        response.ResponseCode = 200;
                        response.ResponseString = "{roles:[\"2\"]}";
                        break;
                }

                await Task.Delay(MOCK_SERVICE_DELAY);
            } else {
            JObject jObject = new JObject();
                jObject.Add("assetId", assetId);
                jObject.Add("username", username);
                jObject.Add("password", password);
                string payload = JsonConvert.SerializeObject(jObject);

            response = await CallWebService(Constants.loginURL/*"loginDevice"*/, "POST", payload);
           }

            return response;
        }

        //*******************************************************

        /// <summary>
        /// Ingest a new device
        /// </summary>
        /// <returns>EUS info</returns>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="assetId">AssetId.</param>
        /// <param name="IMEI">IMEI.</param>
        /// <param name="typeOf">typeOf.</param>
        /// <param name="fromCostCode">FromCostCode.</param>
        /// <param name="receipientUsername">ReceipientUsername.</param>
        /// <param name="receipientCostCode">ReceipientCostCode.</param> 
        /// 
        /// 

        public async Task<WebServiceResponse> CreateDevice(string username, string password, string assetId, string IMEI, string typeOf, string fromCostCode,
                                                           string receipientUsername, string receipientCostCode)
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();

                response.ResponseCode = 200;//StatusCode
                response.ResponseString = "{eusAddress: \"0x038F817cCd8c59f710BdB5806213c3EC996ec2f3\",eusPassword: \"testPass\",deviceAddress: \"0x0076ed2DD9f7dc78e3f336141329F8784D8cd564\",assetId: \"0x12345678\",typeOf: \"0xA0123456\"}";
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                JObject jObject = new JObject();
                jObject.Add("username",  username); 
                jObject.Add("password",  password); 
                jObject.Add("assetId",  assetId); 
                jObject.Add("IMEI",  IMEI); 
                jObject.Add("typeOf",  typeOf); 
                jObject.Add("fromCostCode",  fromCostCode); 
                jObject.Add("receipientUsername",  receipientUsername); 
                jObject.Add("receipientCostCode",  receipientCostCode); 

                string payload = JsonConvert.SerializeObject(jObject);
                response = await CallWebService(Constants.ingestDeviceURL, "POST", payload);
        }

        return response;
    }

        //*******************************************************

        /// <summary>
        /// Dispose Device
        /// </summary>
        /// <returns>Success/Error</returns>
        /// <param name="assetId">AssetId.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
   
        public async Task<WebServiceResponse>DisposeDevice(string assetId, string username, string password){
            WebServiceResponse response;

            if(CurrentEnvironment.IsMock){
                response = new WebServiceResponse();

                response.ResponseCode = 200;
                response.ResponseString = "{deviceAddress:  \"0x461dE94b2fAdbb57c8DE4976fa40233F9e9D1744\", username: \"Mobile1\", password: \"Password#1\"}";
                await Task.Delay(MOCK_SERVICE_DELAY);
            }else{


            JObject jObject = new JObject();
                jObject.Add("assetId", assetId);
                jObject.Add("username", username);
                jObject.Add("password", password);
            string payload = JsonConvert.SerializeObject(jObject);

                response = await CallWebService(Constants.deactivateDeviceURL/*"disposeDevice"*/, "POST", payload);
            }
            return response;
        }

        //*******************************************************

        /// <summary>
        /// pastTransfers
        /// get all past transfers from within a certain timeframe (denoted by the filter object itself)
        /// </summary>
        /// <returns>List of transfers - currently null</returns>
        /// <param name="type">Type.</param>
        /// <param name="username">Username.</param>
        public async Task<WebServiceResponse> PastTransfers(string type, string username, string isActive)
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();

                response.ResponseCode = 200;
                response.ResponseString = "";//currently null mock uo data
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                string appendedURL = Constants.pastTransfesURL + Constants.PARAM_START+ Constants.USERNAME_PARAM + username + Constants.PARAM_CONNECTOR +Constants.ISACTIVE_PARAM + isActive+Constants.PARAM_CONNECTOR+ Constants.TYPE_PARAM+type;
                response = await CallWebService(appendedURL,/*"pastTransfers",*/ "GET", null/*payload*/);
            }
            return response;
        }

        //*******************************************************

        /// <summary>
        /// activateDevice
        /// </summary>
        /// <returns>Device signature and information</returns>
        /// <param name="assetId">AssetId.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="isDisposalAccept">IsDisposalAccept.</param>
        public async Task<WebServiceResponse> ActivateDevice(string assetId, string username, string password, bool isDisposalAccept)
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();

                response.ResponseCode = 200;
                response.ResponseString = "{deviceSignature: {v: 28, r: \"0x907c81a899c69b98fac4cbeb60dda208665aefd161a6a98fa39737c224b94587\", " +
                    "s: \"0x4258671a55ca38500b350465f6c88c1ba1251c3c7b364ff12fd4a40f7e044eb8\", " +
                    "messageHash: \"0x60505c83a54729c334d0192c9600d7e74a33b94cf213c70a73ca441ca21da555\", " +
                    "message: \"1517431274\"}, assetId: \"250624\", username: \"George\", password: \"poop\"}";
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                JObject jObject = new JObject();
                jObject.Add("assetId", assetId);
                jObject.Add("username", username);
                jObject.Add("password", password);
                string payload = JsonConvert.SerializeObject(jObject);

                if(isDisposalAccept){
                    response = await CallWebService(Constants.acceptDisposalURL/*"activateDevice"*/, "POST", payload);
                }else{
                response = await CallWebService(Constants.acceptDeviceURL/*"activateDevice"*/, "POST", payload);
                }

            }
            return response;
        }

        //*******************************************************
    
        /// <summary>
        /// DEPRICATED
        /// transferDeviceToEUS
        /// transfer a device from one address to another address
        /// </summary>
        /// <returns>Currently Null</returns>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="assetId">AssetId.</param>
        public async Task<WebServiceResponse> TransferDeviceToEUS(string username, string password, string assetId)
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();

                response.ResponseCode = 200;
                response.ResponseString = "";
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                JObject jObject = new JObject();
                jObject.Add("username", username);
                jObject.Add("password", password);
                jObject.Add("assetId", assetId);
                string payload = JsonConvert.SerializeObject(jObject);

                response = await CallWebService(Constants.collectDevice/*"transferDevice"*/, "POST", payload);
            }
            return response;
        }

        //*******************************************************

        /// <summary>
        /// Not Using on mobile
        /// pastLogins
        /// view and be able to filter through all previous logins made
        /// </summary>
        /// <returns>Currently Null</returns>
        /// <param name="fromBlock">FromBlock.</param>
        /// <param name="toBlock">ToBlock.</param>
        public async Task<WebServiceResponse> PastLogins(string fromBlock, string toBlock)
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();
                response.ResponseCode = 200;
                response.ResponseString = "";//currently null mock uo data
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                JObject jObject = new JObject();
                jObject.Add("fromBlock", fromBlock);
                jObject.Add("toBlock", toBlock);
                string payload = JsonConvert.SerializeObject(jObject);
                response = await CallWebService(Constants.pastLoginsURL,/*"pastLogins",*/ "POST", payload);
            }
            return response;
        }


        //*******************************************************

        /// <summary>
        /// getAccounts
        /// view and be able to filter through all previous logins made
        /// </summary>
        /// <returns>List of accounts</returns>
        public async Task<WebServiceResponse> GetAccounts()
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();
                response.ResponseCode = 200;
                response.ResponseString = "[]";//currently null mock uo data
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                string appendedURL = Constants.getAccountsURL + Constants.PARAM_START + Constants.PARAM_LOCATION;
                response = await CallWebService(appendedURL,/*"pastLogins",*/ "GET", null);
            }
            return response;
        }


        //*******************************************************

        /// <summary>
        /// getMyDevice
        /// </summary>
        /// <returns>List of device associate under username</returns>
        /// <param name="username">Username.</param>
        public async Task<WebServiceResponse> getDevices(string username)
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();

                response.ResponseCode = 200;
                response.ResponseString = "";//currently null mock uo data
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                string appendedURL = Constants.getDeviceURL + Constants.PARAM_START + Constants.USERNAME_PARAM + username;

                response = await CallWebService(appendedURL/*"pastIngestions"*/, "GET", null/*payload*/);
            }
            return response;
        }

        //*******************************************************

        /// <summary>
        /// deactivateDevice
        /// </summary>
        /// <returns>Success/Error</returns>
        /// <param name="assetId">AssetId.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public async Task<WebServiceResponse> DeactivateDevice(string assetId, string username, string password)
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();

                response.ResponseCode = 200;
                response.ResponseString = "";
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                JObject jObject = new JObject();
                jObject.Add("assetId", assetId);
                jObject.Add("username", username);
                jObject.Add("password", password);
                string payload = JsonConvert.SerializeObject(jObject);

                response = await CallWebService(Constants.disposeDeviceURL/*deactivateDeviceURL"deactivateDevice"*/, "POST", payload);
            }
            
            return response;
        }

     
        //*******************************************************

        /// <summary>
        /// DEPRICATED
        /// getDeviceAddress // tempo solution before integrating web3j
        /// </summary>
        /// <returns>deviceAddress and Keys</returns>
        public async Task<WebServiceResponse> getDeviceAddress()
        {
            WebServiceResponse response;

            if (CurrentEnvironment.IsMock)
            {
                response = new WebServiceResponse();

                response.ResponseCode = 200;
                response.ResponseString = "{privateKey: \"0x9b38200cd5b670b96e6989990e19561c7545cc529e229774bd89b007fbafd3af\", address: \"0xED4bBBa47a46dB224aD4Ccb8751E8f83d4f09E1d\"}";//currently null mock uo data
                await Task.Delay(MOCK_SERVICE_DELAY);
            }
            else
            {
                JObject jObject = new JObject();
                jObject.Add("message", "1517435735");
                jObject.Add("address", "0x7d7cd6673140F319c2a0180106f7DA45f2810087");
                jObject.Add("password", "password#1");
                string payload = JsonConvert.SerializeObject(jObject);

                response = await CallWebService(Constants.createDeviceAddress, "GET", null);
            }
            return response;
        }

        #endregion

        //*******************************************************

    } //class
} //namespace
