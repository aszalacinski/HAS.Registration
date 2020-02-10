using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.Alert
{
    public class ThrowAlert
    {
        public class ThrowAlertCommand : IRequest<bool>
        {
            public string Type { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }

            public ThrowAlertCommand(string type, string title, string body)
            {
                Type = type;
                Title = title;
                Body = body;
            }

            public ThrowAlertCommand(string type, string title, IEnumerable<string> body)
            {
                Type = type;
                Title = title;
                
                foreach(string item in body)
                {
                    Body += $"{item}<br />";
                }

            }
        }

        public class ThrowAlertCommandHandler : IRequestHandler<ThrowAlertCommand, bool>
        {
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

            public ThrowAlertCommandHandler(IHttpContextAccessor httpContextAccessor, ITempDataDictionaryFactory tempDataDictionaryFactory)
            {
                _httpContextAccessor = httpContextAccessor;
                _tempDataDictionaryFactory = tempDataDictionaryFactory;
            }

            public Task<bool> Handle(ThrowAlertCommand cmd, CancellationToken cancellationToken)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);

                tempData["_alert.type"] = cmd.Type;
                tempData["_alert.title"] = cmd.Title;
                tempData["_alert.body"] = cmd.Body;

                return Task.FromResult(false);
            }
        }

        public static class AlertType
        {
            public const string PRIMARY = "primary";
            public const string SECONDARY = "secondary";
            public const string SUCCESS = "success";
            public const string DANGER = "danger";
            public const string WARNING = "warning";
            public const string INFO = "info";
            public const string LIGHT = "light";
            public const string DARK = "dark";
        }
    }
}
