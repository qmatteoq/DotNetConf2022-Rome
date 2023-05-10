// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.0

using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TicketBot.Models;

namespace TicketBot.Bots
{
    public class TicketBot : TeamsActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id);

            var replyText = $"Welcome {member.Name}, your mail address is {member.Email}";

            if (!string.IsNullOrEmpty(turnContext.Activity.Text) && turnContext.Activity.Text.Contains("ticket"))
            {
                var card = CreateIntroCard();
                var reply = MessageFactory.Attachment(new[] { card });
                await turnContext.SendActivityAsync(reply, cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
            }
        }

        #region Task Module

        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var taskInfo = new TaskModuleTaskInfo();
            taskInfo.Card = CreateSubmitCard();
            taskInfo.Title = "My task";

            var response = new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = taskInfo
                }
            };

            return Task.FromResult(response);
        }

        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(ITurnContext<IInvokeActivity> turnContext, TaskModuleRequest taskModuleRequest, CancellationToken cancellationToken)
        {
            var teamsData = JsonConvert.DeserializeObject<Data>(taskModuleRequest.Data.ToString());
            var reply = MessageFactory.Text($"Your ticket with has been created successfully with the following information: Title: {teamsData.TicketTitle} - Description: {teamsData.TicketDescription}");
            await turnContext.SendActivityAsync(reply, cancellationToken);

            var response = new TaskModuleResponse
            {
                Task = new TaskModuleMessageResponse
                {
                    Value = "Thanks!"
                }
            };

            return response;
        }

        public Attachment CreateIntroCard()
        {
            AdaptiveCard card = new AdaptiveCard("1.3")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Do you want to search or create a new ticket?"
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = "Search",
                        Url = new Uri("http://www.microsoft.com")
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = "New ticket",
                        DataJson = @"{""value"": ""test"",""msteams"": { ""type"": ""task/fetch"" } }",

                    }
                }
            };

            var attachment = new Attachment
            {
                Name = "New ticket",
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return attachment;
        }

        public Attachment CreateSubmitCard()
        {
            AdaptiveCard card = new AdaptiveCard("1.3")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = "Create new ticket",
                        Size = AdaptiveTextSize.Large
                    },
                    new AdaptiveTextInput
                    {
                        Label = "Title",
                        Id = "ticketTitle"
                    },
                    new AdaptiveTextInput
                    {
                        Label = "Description",
                        Id = "ticketDescription"
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Submit",
                        DataJson = @"{""value"": ""test"",""msteams"": { ""type"": ""task/fetch"" } }",

                    }
                }
            };

            var attachment = new Attachment
            {
                Name = "New ticket",
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            return attachment;

        }

        #endregion


        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        #region Messaging Extensions - Search

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var text = query?.Parameters?[0]?.Value as string ?? string.Empty;

            HttpClient client = new HttpClient();
            var json = await client.GetStringAsync("https://reqres.in/api/users");
            var users = JsonConvert.DeserializeObject<Users>(json);

            IEnumerable<User> results = new List<User>();

            if (string.IsNullOrEmpty(text))
            {
                results = users.Data;
            }
            else
            {
                results = users.Data.Where(x => x.FirstName.Contains(text) || x.LastName.Contains(text));
            }

            var attachments = new List<MessagingExtensionAttachment>();
            foreach (var user in results)
            {
                var card = new ThumbnailCard
                {
                    Title = $"{user.FirstName} {user.LastName}:",
                    Subtitle = user.Email,
                    Tap = new CardAction { Type = "invoke", Value = user }
                };

                var attachment = new MessagingExtensionAttachment
                {
                    ContentType = ThumbnailCard.ContentType,
                    Content = card
                };

                attachments.Add(attachment);
            }

            var result = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = attachments
                }
            };

            return await Task.FromResult(result);
        }

        protected override Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            var user = query.ToObject<User>();

            var card = new ThumbnailCard
            {
                Title = $"{user.FirstName} {user.LastName}:",
                Subtitle = user.Email,
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec id mollis metus, sed aliquet velit. Donec felis nulla, hendrerit a commodo quis, gravida nec quam. Nullam nec rutrum mauris. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec rutrum elit ex, at ultrices quam hendrerit vitae. Vivamus diam velit, maximus non malesuada in, placerat quis metus. Sed luctus bibendum est, et ornare velit condimentum in. Nunc sed justo quam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Etiam accumsan, eros et pretium vestibulum, turpis erat tincidunt magna, et fringilla lectus massa at turpis. Suspendisse eget tristique lacus, ut cursus neque. Pellentesque consectetur sed quam eget porta. Donec elementum rutrum scelerisque. Nunc nec rutrum purus. Sed aliquam, nisi vel placerat dictum, orci elit lobortis nunc, molestie dictum massa turpis id nisl.",
                Buttons = new List<CardAction> { new CardAction { Type = "openUrl", Value = "https://www.microsoft.com", Text = "Open item" } }
            };

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = card,
            };

            return Task.FromResult(new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new List<MessagingExtensionAttachment> { attachment }
                }
            });
        }

        #endregion

        #region Messaging Extensions - Action

        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            var ticket = JsonConvert.DeserializeObject<NewTicket>(action.Data.ToString());

            var response = new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                },
            };
            var card = new HeroCard
            {
                Title = ticket.Title,
                Subtitle = ticket.Description,
                Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec id mollis metus, sed aliquet velit. Donec felis nulla, hendrerit a commodo quis, gravida nec quam. Nullam nec rutrum mauris. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec rutrum elit ex, at ultrices quam hendrerit vitae. Vivamus diam velit, maximus non malesuada in, placerat quis metus. Sed luctus bibendum est, et ornare velit condimentum in. Nunc sed justo quam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Etiam accumsan, eros et pretium vestibulum, turpis erat tincidunt magna, et fringilla lectus massa at turpis. Suspendisse eget tristique lacus, ut cursus neque. Pellentesque consectetur sed quam eget porta. Donec elementum rutrum scelerisque. Nunc nec rutrum purus. Sed aliquam, nisi vel placerat dictum, orci elit lobortis nunc, molestie dictum massa turpis id nisl.",
                Buttons = new List<CardAction> { new CardAction { Type = "openUrl", Value = "https://www.microsoft.com", Text = "See more details" } }
            };

            var attachments = new List<MessagingExtensionAttachment>();
            attachments.Add(new MessagingExtensionAttachment
            {
                Content = card,
                ContentType = HeroCard.ContentType,
                Preview = card.ToAttachment(),
            });

            response.ComposeExtension.Attachments = attachments;
            return Task.FromResult(response);
        }

        #endregion

        #region Mesagging Extensions - Link unfurling

        protected override async Task<MessagingExtensionResponse> OnTeamsAppBasedLinkQueryAsync(ITurnContext<IInvokeActivity> turnContext, AppBasedLinkQuery query, CancellationToken cancellationToken)
        {
            Uri.TryCreate(query.Url, UriKind.Absolute, out var url);
            var index = url.Segments[2];

            HttpClient client = new HttpClient();
            var json = await client.GetStringAsync($"https://reqres.in/api/users/{index}");
            var data = JsonConvert.DeserializeObject<SingleUser>(json);

            var card = new ThumbnailCard
            {
                Title = $"{data.User.FirstName} {data.User.LastName}:",
                Subtitle = data.User.Email,
            };

            var attachment = new MessagingExtensionAttachment
            {
                ContentType = ThumbnailCard.ContentType,
                Content = card
            };


            var result = new MessagingExtensionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    Type = "result",
                    AttachmentLayout = "list",
                    Attachments = new[] { attachment }
                }
            };

            return await Task.FromResult(result);
        }

        #endregion
    }
}
