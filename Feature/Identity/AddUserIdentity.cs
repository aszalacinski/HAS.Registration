using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityUser = Microsoft.AspNetCore.Identity.MongoDb.IdentityUser;

namespace HAS.Registration.Feature.Identity
{
    public class AddUserIdentity
    {
        public class AddUserIdentityCommand : IRequest<IdentityUser>
        {
            public string Username { get; private set; }
            public string Email { get; private set; }
            public string Password { get; private set; }

            public AddUserIdentityCommand(string username, string email, string password)
            {
                Username = username;
                Email = email;
                Password = password;
            }
        }

        public class AddUserIdentityCommandHandler : IRequestHandler<AddUserIdentityCommand, IdentityUser>
        {
            private readonly UserManager<IdentityUser> _userManager;

            public AddUserIdentityCommandHandler(UserManager<IdentityUser> userManager)
            {
                _userManager = userManager;
            }

            public async Task<IdentityUser> Handle(AddUserIdentityCommand cmd, CancellationToken cancellationToken)
            {
                var user = new IdentityUser(cmd.Username, cmd.Email);
                var result = await _userManager.CreateAsync(user, cmd.Password);

                if(result.Succeeded)
                {
                    return user;
                }
                else
                {
                    throw new IdentityUserCreateException($"Error(s) occurred on identity creation", result.Errors);
                }
            }
        }
    }

    public class IdentityUserCreateException : Exception
    {
        public IEnumerable<IdentityError> Errors { get; private set; }

        public IdentityUserCreateException() : base() { }

        public IdentityUserCreateException(string message) : base(message) { }

        public IdentityUserCreateException(string message, Exception inner) : base(message, inner) { }

        public IdentityUserCreateException(string message, IEnumerable<IdentityError> error) : base(message) 
        {
            Errors = error;
        }
    }
}
