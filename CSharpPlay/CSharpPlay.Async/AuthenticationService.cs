using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpPlay.Async
{
    public class AuthenticationService
    {
        private readonly SimpleDataContext data;

        public AuthenticationService()
        {
            data = SimpleDataContext.DemoData;
        }

        public async Task<Guid?> AuthenticateUserAsync(string username, string password)
        {
            await Task.Delay(5000);

            var id = data.Users.Where(u => u.UserName == username && u.Password == password)
                               .Select(u => (Guid?)u.Id)
                               .SingleOrDefault();

            return id;
        }
    }
}
