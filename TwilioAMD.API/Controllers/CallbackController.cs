using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Twilio.AspNet.Core;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;

namespace TwilioAMD.API.Controllers
{
    [ApiController]
    [Route("callback")]
    public class CallbackController : TwilioController
    {
        //private readonly ITwilioRestClient client;

        //public CallbackController(ITwilioRestClient client)
        //{
        //    this.client = client;
        //}

        [HttpGet]
        [Route("start")]
        public IActionResult Start()
        {
            var configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<Program>()
                .Build();

            var call = CallResource.Create(
                machineDetection: "DetectMessageEnd",
                
                asyncAmd: "true",
                asyncAmdStatusCallback: new Uri("http://fbc7-2601-40a-8002-2730-dc35-5e16-b63c-941.ngrok.io/callback/index"),
                asyncAmdStatusCallbackMethod: HttpMethod.Post,
                
                //with all these values maxed, when a human does answer, asymcAMC callback is still kicking in before the Twiml prompt finishes reading
                //machineDetectionTimeout: 59,
                //machineDetectionSpeechThreshold: 6000,
                //machineDetectionSpeechEndThreshold: 5000,
                //machineDetectionSilenceTimeout: 10000,

                //twiml: new Twilio.Types.Twiml("<Response><Say>Praesent in mauris eu tortor porttitor accumsan. Mauris suscipit, ligula sit amet pharetra semper, nibh ante cursus purus, vel sagittis velit mauris vel metus. Aenean fermentum risus id tortor. Integer imperdiet lectus quis justo. Integer tempor. Vivamus ac urna vel leo pretium faucibus. Mauris elementum mauris vitae tortor. In dapibus augue non sapien. Aliquam ante. Curabitur bibendum justo non orci. Morbi leo mi, nonummy eget, tristique non, rhoncus non, leo. Nullam faucibus mi quis velit. Integer in sapien. Fusce tellus odio, dapibus id, fermentum quis, suscipit id, erat. Fusce aliquam vestibulum ipsum. Aliquam erat volutpat. Pellentesque sapien. Cras elementum. Nulla pulvinar eleifend sem. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Quisque porta. Vivamus porttitor turpis ac leo.Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Nullam feugiat, turpis at pulvinar vulputate, erat libero tristique tellus, nec bibendum odio risus sit amet ante. Aliquam erat volutpat. Nunc auctor. Mauris pretium quam et urna. Fusce nibh. Duis risus. Curabitur sagittis hendrerit ante. Aliquam erat volutpat. Vestibulum erat nulla, ullamcorper nec, rutrum non, nonummy ac, erat. Duis condimentum augue id magna semper rutrum. Nullam justo enim, consectetuer nec, ullamcorper ac, vestibulum in, elit. Proin pede metus, vulputate nec</Say></Response>"),
                
                //with this Twiml, I'm getting call not in progress in asyncAMD callback
                twiml: new Twilio.Types.Twiml("<Response><Say>This is McCarthy Primary Care calling to remind you of your upcoming appointment on November 13th, 2021 at ten o'clock A.M. with Dr. McCarthy. If you need to cancel your appointment, please contact us during normal business hours at 867-5309. Thanks!</Say></Response>"),
                
                from: new Twilio.Types.PhoneNumber(configuration["fromPhoneNumber"]),
                to: new Twilio.Types.PhoneNumber(configuration["toPhoneNumber"])
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

                //answeredBy is "machine_end_beep", when I send the call to voicemail on my mobile phone or I let it ring out and it falls to voicemail
                //but when I answer the phone and don't say antyhing, this callback is invoked about half way through the initial prompt, and answeredBy is "unknown", which then interuppts
                //the current initial prompt and reads this one... which I don't want to do.
                //HOWEVER, when I answer the phone and say "hello", answeredBy is "human"
                if (answeredBy != "human")
                {
                    var call = CallResource.Update(
                        twiml: new Twilio.Types.Twiml("<Response><Say>This is the voicemail I'm going to leave for you instead of asking you a question because you're talking to a machine.</Say></Response>"),
                        pathSid: callSid,
                        pathAccountSid: accountSid
                    );
                }
            }

            return Content(response.ToString(), "text/xml");
        }
    }
}
