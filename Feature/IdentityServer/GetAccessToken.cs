using IdentityModel.Client;
using MediatR;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.IdentityServer
{
    public class GetAccessToken
    {
        public class GetAccessTokenCommand : IRequest<string>
        {
            public string ClientId { get; private set; }
            public string ClientSecret { get; private set; }
            public string Scopes { get; private set; }

            public GetAccessTokenCommand(string clientId, string clientSecret, string scopes)
            {
                ClientId = clientId;
                ClientSecret = clientSecret;
                Scopes = scopes;
            }

            public class GetAccessTokenCommandHandler : IRequestHandler<GetAccessTokenCommand, string>
            {
                private readonly HttpClient _client;

                public GetAccessTokenCommandHandler(IHttpClientFactory httpClientFactory)
                {
                    _client = httpClientFactory.CreateClient(HASClientFactories.IDENTITY);
                }

                public async Task<string> Handle(GetAccessTokenCommand cmd, CancellationToken cancellationToken)
                {
                    var disco = await _client.GetDiscoveryDocumentAsync();
                    if (disco.IsError)
                    {
                        throw new Exception(disco.Error, disco.Exception);
                    }

                    var tokenResponse = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        ClientId = cmd.ClientId,
                        ClientSecret = cmd.ClientSecret,
                        Scope = cmd.Scopes
                    });

                    if (tokenResponse.IsError)
                    {
                        throw new Exception(tokenResponse.Error, tokenResponse.Exception);
                    }

                    return tokenResponse.AccessToken;
                }
            }
        }
    }
}
