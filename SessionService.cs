using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using E_Raamatud.Model;

namespace E_Raamatud
{
    public static class SessionService
    {
        public static User? CurrentUser { get; private set; }

        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}
