using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using test211005.Content;
using test211005.Models;
using System.Text.RegularExpressions;

namespace test211005.Controllers
{

    public class AccountController : Controller
    {
        public UserService _userService = new UserService();

        // GET: Account/Register
        [HttpGet]
        public ActionResult Register()
        {
            RegisterViewModel m = new RegisterViewModel();
            ViewBag.UserIdExist = "";
            return View(m);
        }

        // POST : Account/Register
        [HttpPost]
        public ActionResult Register(RegisterViewModel m)
        {
            ViewBag.UserIdExist = "";
            if (m.UserId != null)
            {
                UserModel u = DASManager.ShowUserDetail(m.UserId);
                if (u.UserId != null)
                {
                    ViewBag.UserIdExist = "이미 존재하는 아이디입니다.";
                    return View(m);
                }
                else
                {
                    Regex regex = new Regex(@"[^a-zA-Z0-9]");
                    if (regex.IsMatch(m.UserId))
                    {
                        ViewBag.UserIdExist = "아이디에는 한글, 특수문자(\\/:-*?\"<>` 등)를 포함할 수 없습니다.";
                        return View(m);
                    }
                    if (m.UserId.Length < 4 || m.UserId.Length > 50)
                    {
                        ViewBag.UserIdExist = "4~50자 이내 영문/숫자만 사용 가능합니다.";
                        return View(m);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.UserIdExist = "";
                return View(m);
            }

            DASManager.InsertUserInfo(m);
            ModelState.Clear();
            
            UserModel um = DASManager.ShowUserDetail(m.UserId);
            Session["UserSession"] = um;
            Session["RegisterCheck"] = "yes";
            return RedirectToAction("Index", "Home");
        }

        // GET : Account/Login
        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.checkLogin = "";
            UserModel m = new UserModel();
            return View(m);
        }

        [HttpPost]
        public ActionResult Login(UserModel m)
        {
            UserModel u = DASManager.ShowUserDetail(m.UserId);
            if (u.UserId == null) { // id가 존재하지 않는 경우 fail
                ViewBag.CheckErrMsg = "error";
                return View(m);
            }

            if (u.UserPwd != m.UserPwd) { // pwd가 존재하지 않는 경우 fail
                ViewBag.CheckErrMsg = "error";
                return View(m);
            }

            /*로그인 성공*/
            Session["UserSession"] = u;
            if (u.UserType == 1)
                Session["IsAdmin"] = 1;

            /*돌아갈 url이 있는 경우*/
            if (Session["ReturnUrl"] != null)
            {
                ReturnUrl url = (ReturnUrl)Session["ReturnUrl"];

                /*Admin으로 가고자 하는데 admin이 로그인했는 지 확인*/
                if (url.ReturnAct == "Index" && url.ReturnCon == "Admin")
                {
                    if (Session["IsAdmin"] != null) // Admin이 로그인
                        Session.Remove("ReturnUrl");
                }

                string act = url.ReturnAct;
                if (url.ItemNo != 0)
                    act += "/" + url.ItemNo;
                return RedirectToAction(act, url.ReturnCon);
            }
            else
            {
                if (Session["IsAdmin"] != null)
                    return RedirectToAction("Index", "Admin");
                else
                    return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

    }
}

