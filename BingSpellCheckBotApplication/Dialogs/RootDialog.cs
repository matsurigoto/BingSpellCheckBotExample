using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using RestSharp;
using System.Collections.Generic;

namespace BingSpellCheckBotApplication.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private string key = "your_key";

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var client = new RestClient("https://api.cognitive.microsoft.com/bing/v7.0/");
            var request = new RestRequest("spellcheck?mode=proof&mkt=en-us", Method.POST);
            request.AddHeader("Ocp-Apim-Subscription-Key", key);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("text", activity.Text);

            var response = await client.ExecuteTaskAsync<RootObject>(request);
            var suggestion = string.Empty;
            foreach(var item in response.Data.flaggedTokens[0].suggestions)
            {
                suggestion += item.suggestion + " ";
            }
            await context.PostAsync($"{suggestion}");

            context.Wait(MessageReceivedAsync);
        }
    }

    public class Suggestion
    {
        public string suggestion { get; set; }
        public double score { get; set; }
    }

    public class FlaggedToken
    {
        public int offset { get; set; }
        public string token { get; set; }
        public string type { get; set; }
        public List<Suggestion> suggestions { get; set; }
    }

    public class RootObject
    {
        public string _type { get; set; }
        public List<FlaggedToken> flaggedTokens { get; set; }
    }
}