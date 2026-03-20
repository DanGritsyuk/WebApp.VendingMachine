using HireWise.Common.Entities.LoginModels;
using Microsoft.AspNetCore.Http;

namespace VendingMachine.BLL.Logic.Contracts
{
    public interface IAuthenticationLogic
    {
        IResult LoginAsync(LoginModel loginModel, CancellationToken cancellationToken = default);
    }
}
