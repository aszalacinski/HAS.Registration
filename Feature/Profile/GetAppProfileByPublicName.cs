using HAS.Registration.Models;
using IdentityModel.Client;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Registration.Feature.IdentityServer.GetAccessToken;

namespace HAS.Registration
{
    public class GetAppProfileByPublicName
    {
        public class GetAppProfileByPublicNameQuery : IRequest<Profile>
        {
            public string PublicName { get; private set; }

            public GetAppProfileByPublicNameQuery(string publicName) => PublicName = publicName;
        }

        public class GetAppProfileByPublicNameQueryHandler : IRequestHandler<GetAppProfileByPublicNameQuery, Profile>
        {
            private readonly IMediator _mediator;
            private readonly HttpClient _httpClient;
            private readonly IConfiguration _configuration;

            public GetAppProfileByPublicNameQueryHandler(IMediator mediator, IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _mediator = mediator;
                _httpClient = httpClientFactory.CreateClient(HASClientFactories.PROFILE);
                _configuration = configuration;
            }

            public async Task<Profile> Handle(GetAppProfileByPublicNameQuery query, CancellationToken cancellationToken)
            {
                var clientId = _configuration["MPY:IdentityServer:RegistrationApp:ClientId"];
                var clientSecret = _configuration["MPY:IdentityServer:RegistrationApp:ClientSecret"];
                var scopes = _configuration["MPY:IdentityServer:RegistrationApp:Scopes"];

                // get access token
                var token = await _mediator.Send(new GetAccessTokenCommand(clientId, clientSecret, scopes));

                string nameCheckUri = $"by/publicName/{query.PublicName}";

                _httpClient.SetBearerToken(token);

                var iProfile = await _httpClient.GetAsync(nameCheckUri);

                if (!iProfile.IsSuccessStatusCode)
                {
                    return null;
                }

                var ncContent = await iProfile.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<Profile>(ncContent, DefaultJsonSettings.Settings);
            }
        }

    }
}
