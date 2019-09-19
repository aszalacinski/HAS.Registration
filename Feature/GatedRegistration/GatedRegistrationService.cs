using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public class GatedRegistrationService
    {
        private readonly IGatedRegistrationRepository _repository;

        public GatedRegistrationService(IGatedRegistrationRepository repository)
        {
            _repository = repository;
        }

        public async Task<GatedRegistrationServiceResponse<ResultResponse<bool>>> AttemptToRegister(string emailAddress, string entryCode)
        {
            InvitedUser user = await _repository.Find(x => x.EmailAddress.ToUpper() == emailAddress.ToUpper());

            if(user != null)
            {
                if(user.Verify(entryCode))
                {
                    if (user.IsInvited())
                    {
                        if (!user.IsRegistered())
                        {
                            user.Register();
                            user.Log(true);
                            var updatedUser = await _repository.Update(user);
                            if (updatedUser != null)
                            {
                                return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(true, 204), string.Empty);
                            }

                            return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(false, 400), "An error occurred during update of Invited User record");
                        }
                        else
                        {
                            // user is in database and was invited and has already registered
                            // log entry attempt, return false
                            user.Log(false);
                            var updatedUser = await _repository.Update(user);
                            if (updatedUser != null)
                            {
                                return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(false, 204), "User has already registered");
                            }

                            return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(false, 400), "An error occurred during update of Invited User record");
                        }
                    }
                    else
                    {
                        // add user is in database but is uninvited, capture email, log entry attempt, return false
                        user.Log(false);
                        var updatedUser = await _repository.Update(user);
                        if (updatedUser != null)
                        {
                            return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(false, 302), "User is in database but is uninvited");
                        }

                        return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(false, 400), "An error occurred during update of Invited User record");
                    }
                }
                else
                {
                    //user attempted to log in with invalid entry code
                    user.Log(false, entryCode);
                    var updatedUser = await _repository.Update(user);
                    if (updatedUser != null)
                    {
                        return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(false, 401), "User attempted to register with invalid entry code");
                    }

                    return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(false, 400), "An error occurred during update of Invited User record");
                }
            }
            else
            {
                // add user to database as uninvited, capture email, log entry attempt return false
                InvitedUser newUser = InvitedUser.Create(string.Empty, emailAddress, "0F0F0F", false, false, DateTime.MinValue);
                newUser.Log(false);
                var aUser = _repository.Add(newUser);
                return new GatedRegistrationServiceResponse<ResultResponse<bool>>(ResultResponse<bool>.Create(false, 200), "User was added to database as uninvited");
            }

        }
    }
}
