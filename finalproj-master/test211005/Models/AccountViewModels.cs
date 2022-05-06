using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace test211005.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="아이디를 입력해주세요.")]
        [Key]
        //[Remote(action: "VerifyUserId", controller: "AccountController")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "비밀번호를 입력해주세요.")]
        [MinLength(8, ErrorMessage = "비밀번호는 최소 8자 이상 입력해주세요.")]
        [DataType(DataType.Password)]
        public string UserPwd { get; set; }

        [Required(ErrorMessage ="비밀번호를 입력해주세요.")]
        [DataType(DataType.Password)]
        [Compare("UserPwd", ErrorMessage = "비밀번호가 같은 지 확인해주세요.")]
        public string UserPwdCheck { get; set; }

        //[RegularExpression("/^[ㄱ-ㅎ가-힣a-zA-Z]+$/", ErrorMessage = "이름에는 숫자, 특수문자(\\/:-*?\"<>` 등)를 포함할 수 없습니다.")]
        [RegularExpression("^([a-zA-Z가-힣]+)$", ErrorMessage = "이름에는 숫자, 모음, 자음, 특수문자(\\/:-*?\"<>` 등)를 포함할 수 없습니다.")]
        [Required(ErrorMessage ="이름을 입력해주세요.")]
        public string UserName { get; set; }

        public DateTime UserBirth { get; set; }
    }

    public class UserModel
    {
        public int UserNo { get; set; }
        public string UserId { get; set; }
        public string UserPwd { get; set; }
        public string UserName { get; set; }
        public string UserBirth { get; set; }

        public int UserType { get; set; }
        public int UserStatus { get; set; }
        public DateTime BanReleaseDate { get; set; }
        public string RegDate { get; set; }
    }

    public class ReturnUrl
    {
        public string ReturnCon { get; set; }
        public string ReturnAct { get; set; }
        public int ItemNo { get; set; }
    }

    public class UserStatusModel
    {
        public int TotalCash { get; set; }
        public TotalCashModel GiftiCash { get; set; } // 충전 캐시
        public TotalCashModel BonusCash { get; set; } // 보너스 캐시
        public TotalCashModel SellingCash { get; set; } // 판매금 캐시
    }
}