using System;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;

namespace TwilioAMD
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<Program>()
                .Build();

            var accountSid = configuration["accountSid"];
            var authToken = configuration["authToken"];

            TwilioClient.Init(accountSid, authToken);

            var call = CallResource.Create(
                machineDetection: "DetectMessageEnd",
                
                //asyncAMD settings
                asyncAmd: "true",
                asyncAmdStatusCallback: new Uri("http://bdb0-2601-40a-8002-2730-d900-c49a-9f5d-5499.ngrok.io/callback/index"),
                asyncAmdStatusCallbackMethod: HttpMethod.Post,
                
                //these two are for TwimL callback... we're not using a callback to get Twiml, we're sending it
                //url: new Uri("http://bdb0-2601-40a-8002-2730-d900-c49a-9f5d-5499.ngrok.io/callback/index"),
                //method: HttpMethod.Post,
                
                twiml: new Twilio.Types.Twiml("<Response><Say>Ahoy there!</Say></Response>"),
                from: new Twilio.Types.PhoneNumber(configuration["fromPhoneNumber"]),
                to: new Twilio.Types.PhoneNumber(configuration["toPhoneNumber"])
            );

            Console.WriteLine(call.Sid);
        }
    }
}
