﻿using System;
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

            //this cost money, so keep it commented out!
            //var type = new List<string> {
            //    "caller-name"
            //};
            //var phoneNumber = PhoneNumberResource.Fetch(
            //    type: type,
            //    pathPhoneNumber: new Twilio.Types.PhoneNumber(configuration["toPhoneNumber"])
            //);
            //var phoneNumberJson = JsonConvert.SerializeObject(phoneNumber);

            var call = CallResource.Create(
                machineDetection: "DetectMessageEnd",
                asyncAmd: "true",
                asyncAmdStatusCallback: new Uri($"{configuration["ngrokUrl"]}/callback/index"),
                asyncAmdStatusCallbackMethod: HttpMethod.Post,
                twiml: new Twilio.Types.Twiml("<Response><Say>Hello there. This is a longer message that will be about as long as the real meessage askgin you to confirm or cancel your appointment. Hopefully it's long enough!</Say></Response>"),
                from: new Twilio.Types.PhoneNumber(configuration["fromPhoneNumber"]),
                to: new Twilio.Types.PhoneNumber(configuration["toPhoneNumber"])
            );

            Console.WriteLine(call.Sid);

            //these two are for TwimL callback... we're not using a callback to get Twiml, we're sending it
            //url: new Uri("http://bdb0-2601-40a-8002-2730-d900-c49a-9f5d-5499.ngrok.io/callback/index"),
            //method: HttpMethod.Post,

            Console.ReadLine();
        }
    }
}
