using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using static System.Net.Mime.MediaTypeNames;

namespace test211005.Controllers
{
    public class HomeController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ActionResult Index()
        {
            ViewBag.ReturnCon = "";
            ViewBag.ReturnAct = "";
            if (Session["RegisterCheck"] != null)
            {
                Response.Write("<script language='javascript'>alert('회원가입 기념 보너스 캐시 3,000원이 지급되었습니다!');</script>");
                Session.Remove("RegisterCheck");
            }

            return View();
        }

        public ActionResult MyPage()
        {
            return View();
        }
    }
}