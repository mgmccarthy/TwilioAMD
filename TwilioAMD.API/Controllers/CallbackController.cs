using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Twilio;
using Twilio.AspNet.Core;
using Twilio.Clients;
using Twilio.Http;
using Twilio.Rest;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;

namespace TwilioAMD.API.Controllers
{
    [ApiController]
    [Route("callback")]
    public class CallbackController : TwilioController
    {
        private readonly ITwilioRestClient client;

        public CallbackController(ITwilioRestClient client)
        {
            this.client = client;
        }

        [HttpGet]
        [Route("start")]
        public IActionResult Start()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<Program>()
                .Build();

            //var accountSid = configuration["accountSid"];
            //var authToken = configuration["authToken"];
            //TwilioClient.Init(accountSid, authToken);

            var call = CallResource.Create(
                machineDetection: "DetectMessageEnd",
                asyncAmd: "true",
                asyncAmdStatusCallback: new Uri("http://23c7-2601-40a-8002-2730-ccdd-6490-15ab-a18d.ngrok.io/callback/index"),
                asyncAmdStatusCallbackMethod: HttpMethod.Post,
                twiml: new Twilio.Types.Twiml("<Response><Say>Hello there. This is a longer message that will be about as long as the real meessage askgin you to confirm or cancel your appointment. Hopefully it's long enough!</Say></Response>"),
                from: new Twilio.Types.PhoneNumber(configuration["fromPhoneNumber"]),
                to: new Twilio.Types.PhoneNumber(configuration["toPhoneNumber"]),
                client: this.client
            );

            return Ok();
        }

        [HttpPost]
        [Route("index")]
        public IActionResult Index()
        {
            var response = new VoiceResponse();

            Request.Form.TryGetValue("CallSid", out var callSid);
            Request.Form.TryGetValue("AccountSid", out var accountSid);

            if (Request.Form.TryGetValue("AnsweredBy", out var answeredBy))
            {
                Debug.WriteLine($"answeredBy: {answeredBy}");
                Debug.WriteLine($"callSid: {callSid}");
                Debug.WriteLine($"accountSid: {accountSid}");

                if (answeredBy != "human")
                {

                    //var configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    //    .AddJsonFile("appsettings.json", true)
                    //    .AddUserSecrets<Program>()
                    //    .Build();
                    //var authToken = configuration["authToken"];
                    //TwilioClient.Init(accountSid, authToken);

                    var call = CallResource.Update(
                        twiml: new Twilio.Types.Twiml("<Response><Say>This is the voicemal I'm goign to leave for you instead of asking you a question b/c you're talking to a machine.</Say></Response>"),
                        pathSid: callSid,
                        pathAccountSid: accountSid,
                        client: this.client
                    );
                }
            }

            return Content(response.ToString(), "text/xml");
        }

        //[HttpPost]
        //[Route("index")]
        //public IActionResult Index()
        //{
        //    var response = new VoiceResponse();

        //    if (Request.Form.TryGetValue("AnsweredBy", out var answeredBy))
        //    {
        //        //async amd tutorial
        //        //https://www.twilio.com/blog/async-answering-machine-detection-tutorial

        //        Debug.WriteLine($"answeredBy: {answeredBy}");

        //        if (answeredBy == "human")
        //        {
        //            response.Say("this is the message I'm going to ask the callee and gather input");
        //        }
        //        else
        //        {
        //            response.Say("this is the voice message");
        //        }

        //        //SyncAmd(answeredBy, response);
        //        //AsyncAmd(answeredBy, response);
        //    }

        //    return Content(response.ToString(), "text/xml");
        //    //return new TwiMLResult(response.ToString());
        //}

        private static void AsyncAmd(StringValues answeredBy, VoiceResponse response)
        {
            //when a human answers, this is never invoked...
            if (answeredBy == "human")
            {
                response.Say("this is the message I'm going to ask the callee and gather input");
            }
            else //for AMD async callback URL, AnsweredBy is always "unknown", and no message is left on my voicemail
            {
                //response.Say("this is the voice message");
            }
        }

        private static void SyncAmd(StringValues answeredBy, VoiceResponse response)
        {
            //when using non-async option with regular callback URL
            if (answeredBy == "machine_end_beep" || answeredBy == "machine_end_silence" || answeredBy == "machine_end_other")
            {
                response.Say("this is the voice message");
            }
            else
            {
                response.Say("this is the message I'm going to ask the callee and gather input");
            }
        }
    }
}
