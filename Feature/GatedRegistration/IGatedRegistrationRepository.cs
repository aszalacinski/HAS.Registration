using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HAS.Registration.Feature.GatedRegistration
{
    public interface IGatedRegistrationRepository
    {
        Task<InvitedUser> Find(Expression<Func<InvitedUserDAO, bool>> expression);
        Task<IEnumerable<InvitedUser>> FindAll(Expression<Func<InvitedUserDAO, bool>> expression);
        Task<InvitedUser> Update(InvitedUser invitedUser);
        Task<InvitedUser> Add(InvitedUser invitedUser);
    }
}
