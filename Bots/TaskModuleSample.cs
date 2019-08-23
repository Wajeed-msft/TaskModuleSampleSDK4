// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TaskModuleDemo.Models;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class TaskModuleHandler : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            AdaptiveCard card = new AdaptiveCard("1.0");
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Hello, click on Show Task Module.",
                Size = AdaptiveTextSize.Large
            });

            card.Actions.Add(new TaskModuleAction("Show Task Module 1", new ActionDetails() { ActionType = "action 1" }).ToAdaptiveCardAction());
            card.Actions.Add(new TaskModuleAction("Show Task Module 2", new ActionDetails() { ActionType = "action 2" }).ToAdaptiveCardAction());
            var message = MessageFactory.Attachment(card.ToAttachment());
            await turnContext.SendActivityAsync(message, cancellationToken);
        }

        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            switch (turnContext.Activity.Type)
            {
                // handle invokes
                case ActivityTypes.Invoke:
                    return OnInvokeActivityAsync(new DelegatingTurnContext<IInvokeActivity>(turnContext), cancellationToken);

                default:
                    return base.OnTurnAsync(turnContext, cancellationToken);
            }
        }

        protected virtual async Task OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (turnContext.Activity.Name)
            {
                case "task/fetch":
                    var activityValue = turnContext.Activity.Value.ToString();
                    var taskModuleRequestDetails = JsonConvert.DeserializeObject<TaskModuleRequest>(activityValue);
                    var actionDetails = JsonConvert.DeserializeObject<ActionDetails>(taskModuleRequestDetails.Data.ToString());
                    TaskModuleResponse response = new TaskModuleResponse
                    {
                        Task = new TaskModuleContinueResponse()
                        {
                            Type = "continue",
                            Value = new TaskModuleTaskInfo()
                            {
                                Title = "Test task module",
                                Height = 470,
                                Width = 400,
                                Card = GetSimpleAdaptiveCard(actionDetails.ActionType).ToAttachment()
                            }
                        }
                    };

                    await SendResponse(turnContext, response);

                    break;
                case "task/submit":
                    // Close task module by sending reponse as Accepted.
                    await SendResponse(turnContext);
                    await turnContext.SendActivityAsync(MessageFactory.Text("Thanks for submitting your request."));
                    break;
            }
        }

        private static async Task SendResponse(ITurnContext<IInvokeActivity> turnContext, object body = null)
        {
            await turnContext.SendActivityAsync(new Activity
            {
                Value = new InvokeResponse { Status = 200, Body = body },
                Type = ActivityTypesEx.InvokeResponse,
            });
        }

        private static AdaptiveCard GetSimpleAdaptiveCard(string type)
        {
            AdaptiveCard card = new AdaptiveCard("1.0");

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = "This is opened in Task Module for " + type,
                Size = AdaptiveTextSize.Large,
                Wrap = true
            });

            card.Body.Add(new AdaptiveImage()
            {
                Url = new Uri("http://adaptivecards.io/content/cats/1.png")
            });

            card.Actions.Add(new AdaptiveSubmitAction());

            return card;
        }
    }
}
