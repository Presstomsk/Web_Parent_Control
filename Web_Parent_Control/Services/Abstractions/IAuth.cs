using Microsoft.AspNetCore.Mvc;

namespace Web_Parent_Control.Services.Abstractions
{
    public interface IAuth
    {
        void Authorization(string login, ControllerBase controller);
        void Logout(ControllerBase controller);
    }
}
