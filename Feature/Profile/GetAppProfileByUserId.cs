using HAS.Registration.Models;
using IdentityModel.Client;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static HAS.Registration.Feature.IdentityServer.GetAccessToken;

namespace HAS.Registration
{
    public class GetAppProfileByUserId
    {
        public class GetAppProfileByUserIdQuery : IRequest<Profile>
        {
            public string UserId { get; private set; }
            public GetAppProfileByUserIdQuery(string userId) => UserId = userId;
        }

        public class GetAppProfileByUserIdQueryHandler : IRequestHandler<GetAppProfileByUserIdQuery, Profile>
        {
            private readonly IMediator _mediator;
            private readonly HttpClient _httpClient;
            private readonly IConfiguration _configuration;

            public GetAppProfileByUserIdQueryHandler(IMediator mediator, IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _mediator = mediator;
                _httpClient = httpClientFactory.CreateClient(HASClientFactories.PROFILE);
                _configuration = configuration;
            }

            public async Task<Profile> Handle(GetAppProfileByUserIdQuery query, CancellationToken cancellationToken)
            {
                var clientId = _configuration["MPY:IdentityServer:RegistrationApp:ClientId"];
                var clientSecret = _configuration["MPY:IdentityServer:RegistrationApp:ClientSecret"];
                var scopes = _configuration["MPY:IdentityServer:RegistrationApp:Scopes"];

                // get access token
                var token = await _mediator.Send(new GetAccessTokenCommand(clientId, clientSecret, scopes));

                string uri = $"by/{query.UserId}";

                _httpClient.SetBearerToken(token);

                var response = await _httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();

                var profile = JsonSerializer.Deserialize<Profile>(content, DefaultJsonSettings.Settings);

                return profile;
            }
        }
    }
}
