using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XWave.Core.Services.Interfaces
{
    public interface IAuthorizationService
    {
        Task<string[]> GetRolesByUserName(string userName);

        Task<string[]> GetRolesByUserId(string userId);

        Task<bool> IsUserInRole(string userId, string role);
    }
}