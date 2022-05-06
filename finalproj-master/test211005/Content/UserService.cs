using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace test211005.Content
{
    public class UserService
    {
        public bool VerifyUserId(string userId)
        {
            if (DASManager.ShowUserDetail(userId).UserId == null)
                return true; // 회원가입 가능
            else
                return false; // 회원가입 불가능
        }
    }
}