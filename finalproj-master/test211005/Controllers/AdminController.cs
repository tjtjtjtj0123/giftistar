using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using test211005.Content;
using test211005.Models;

namespace test211005.Controllers
{
    public class AdminController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        // GET: Admin/Index - 전체 결제 내역 출력
        public ActionResult Index()
        {
            ViewBag.Title = "AP";
            if (Session["IsAdmin"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "Index";
                ru.ReturnCon = "Admin";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "전체", Value = "-1" });

            items.Add(new SelectListItem { Text = "신용카드", Value = "0" });

            items.Add(new SelectListItem { Text = "Paypal", Value = "1" });

            items.Add(new SelectListItem { Text = "휴대폰", Value = "2" });

            items.Add(new SelectListItem { Text = "판매금 캐시", Value = "3" });

            items.Add(new SelectListItem { Text = "보너스 캐시", Value = "4" });

            ViewBag.PaymentType = items;

            ShowAllChargeViewModel m = new ShowAllChargeViewModel();

            int totalRowCount = DASManager.CheckLength(0, "charge", null, null, -1, -1, null);
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            // (UserNo, StartDate, EndDate, UserId, Payment, PageStart, totalPageCount, innerRow, isExcel)
            m.ChargingList = DASManager.ShowChargeList(0, null, null, null, -1, pageNum, totalPageCount, 10, 0);
            return View(m);
        }
        /////* Ajax-GET: Admin/Index - 전체 충전 리스트 출력*/
        public JsonResult ShowAllChargeList(DateTime StartDate, DateTime EndDate, int Payment, string UserId)
        {
            DateTime end = EndDate.AddDays(1);
            UserModel um = DASManager.ShowUserDetail(UserId);
            int UserNo = um != null ? um.UserNo : 0;
            int totalRowCount = DASManager.CheckLength(UserNo, "charge", StartDate, end, Payment, -1, null);
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<ChargeModel> m = DASManager.ShowChargeList(0, StartDate, end, UserId, Payment, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/Index - 전체 충전 리스트 Excel로 출력*/
        public JsonResult ShowAllChargeListByExcel(DateTime StartDate, DateTime EndDate, int Payment, string UserId)
        {
            DateTime end = EndDate.AddDays(1);
            UserModel um = DASManager.ShowUserDetail(UserId);
            int UserNo = um != null ? um.UserNo : 0;
            int totalRowCount = DASManager.CheckLength(UserNo, "charge", StartDate, end, Payment, -1, null);
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<ChargeModel> m = DASManager.ShowChargeList(0, StartDate, end, UserId, Payment, pageNum, totalPageCount, 10, 1);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }


        // GET: Admin/OrderDetial
        public ActionResult OrderDetail(int CashNo)
        {
            if (Session["IsAdmin"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "Index";
                ru.ReturnCon = "Admin";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "popup";
            ViewBag.Poplike = 0;
            ChargeModel m = DASManager.ShowChargeDetail(CashNo);
            m.ReTrieveStatus = 0;
            m.RefundStatus = 0;

            /*캐시 회수, 취소 조건 걸어주기*/
            if (m.CashRemain > 0 && m.CashStatus == "정상")
            {
                m.RefundStatus = 1; // 회수
                if (m.CashPropertyNo == 3)
                {
                    if (m.ChargeCashAmount == m.CashRemain)
                        m.ReTrieveStatus = 1; // 취소
                }
            }
            
            return View(m);
        }

        /////* Ajax-GET: Admin/OrderDetail - 캐시 회수*/
        public JsonResult DoRefund(string RefundMsg, int CashNo)
        {
            DASManager.InsertRefundInfo(RefundMsg, CashNo);
            string json = "";
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/OrderDetail - 캐시 취소*/
        public JsonResult DoRetrieve(string RefundMsg, int CashNo, string UserId, string Payment)
        {
            if (Payment != "PayPal")
                Load_POQRefund(CashNo, UserId);
            DASManager.InsertRetrieveInfo(RefundMsg, CashNo);
            string json = "";
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /*시작 - POQ********************************************************************************************************************************************/
        public void Load_POQRefund(int CashNo, string UserId)
        {
            SetTls();
            logger.Error("들어옴");
            //-----------------------------------------------------------------------------------------
            // Description    : 결제 요청 API URL 및 파라미터 설정
            //                - 테스트 URL : https://testpgapi.payletter.com/v1.0/payments/request
            //                - 라이브 URL : https://pgapi.payletter.com/v1.0/payments/request
            //                - 휴대폰 이외의 PG로 결제 요청시 설정할 파라미터는 가이드 문서 참고를 참고하시기 바랍니다.
            //                - 가이드 문서 URL: https://pg.payletter.com/APIDocument/index.html
            //-----------------------------------------------------------------------------------------
            string strURL = "https://testpgapi.payletter.com/v1.0/payments/cancel"; // 결제 취소 요청 API 호출
            //string pgCode = "";

            //if (m.Payment == 0) // 신용카드
            //    pgCode = "creditcard";
            //else if (m.Payment == 2) // 휴대폰
            //    pgCode = "mobile";
            // 결제 요청 파라미터 설정 (JSON)
            // callback_url : 결제 완료 후 callback을 받을 가맹점 페이지 주소
            PGLogList m = DASManager.ShowPGLogDetail(CashNo, 0);
            string pgCode = "";

            if (m.Payment == 0) // 신용카드
                pgCode = "creditcard";
            else if (m.Payment == 2) // 휴대폰
                pgCode = "mobile";

            string strPostData = "{\"pgcode\" : \""+ pgCode +"\"," +
                                    "\"client_id\":\"pay_test\"," +
                                    "\"user_id\":\""+ UserId + "\"," +
                                    "\"tid\":\""+ m.Tid +"\"," +
                                    "\"amount\" : " + m.ChargePaidAmount + "," +
                                    "\"ip_addr\":\"127.0.0.1\"}";

            logger.Error("pgCode=" + pgCode + " tid=" + m.Tid + " amount=" + m.ChargePaidAmount);
            //-----------------------------------------------------------------------------------------
            // Description    : 결제 요청 (POST)
            //                - HttpRequestHeader Authorization : PLKEY + {가맹점_apikey}
            //                - 가맹점 계약이 완료되면 API Key가 발급되며 가맹점 관리자 페이지에서 확인하실 수 있습니다.
            //                - 가입 전에 테스트 환경에서 미리 구성된 API Key로 연동 테스트가 가능합니다. 
            //                - 가맹점 아이디 : pay_test
            //                - API Key (PAYMENT) : MTFBNTAzNTEwNDAxQUIyMjlCQzgwNTg1MkU4MkZENDA=
            //                - API Key (SEARCH)  : MUI3MjM0RUExQTgyRDA1ODZGRDUyOEM4OTY2QTVCN0Y=
            //-----------------------------------------------------------------------------------------
            try
            {
                /*webClient request 보내는 부분*/
                HttpWebRequest objWRequest = (HttpWebRequest)WebRequest.Create(strURL);
                objWRequest.Method = "POST";
                objWRequest.ContentType = "application/json";
                objWRequest.Host = "testpgapi.payletter.com";
                objWRequest.Headers.Add("Authorization", "PLKEY MTFBNTAzNTEwNDAxQUIyMjlCQzgwNTg1MkU4MkZENDA="); // Authorization 설정

                byte[] objResultByte = Encoding.UTF8.GetBytes(strPostData);
                objWRequest.ContentLength = objResultByte.Length;
                Stream objStream = objWRequest.GetRequestStream();
                objStream.Write(objResultByte, 0, objResultByte.Length);
                objStream.Close();

                objWRequest.Timeout = 20000;

                //-----------------------------------------------------------------------------------------
                // Description : API 요청에 대한 성공/실패 여부 (오류코드) 
                //               HTTP StatusCode 200 OK 인 경우에만 요청 처리 성공이며, 성공이 아닌 경우에는 아래 StatusCode를 참고하시기 바랍니다.          
                //              - 401 : [998] Authentication token is missing or incorrect. (인증 오류)
                //              - 403 : [993] Yon do not have authorization. (인증 오류)
                //              - 405 : [995] 요청된 메소드는 권한이 없습니다. (POST / GET 등 메소드 오류)
                //              - 406 : [2000]~[5000] 오류 상세 메시지 (비즈니스 로직 처리중 오류 발생)
                //              - 500 : [999] Internal server error (System 오류)
                //-----------------------------------------------------------------------------------------

                // 요청 처리 성공인 경우                                       
                // Response Parameters (성공시) : token, online_url, mobile_url
                HttpWebResponse objWResponse = (HttpWebResponse)objWRequest.GetResponse();

                StreamReader objStreamReader = new StreamReader(objWResponse.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                string strResponse = objStreamReader.ReadToEnd();
                JObject json = JObject.Parse(strResponse);
                logger.Error("끝!!");
                objStreamReader.Close();
                objWResponse.Close();
            }
            // 성공이 아닌 경우
            // Response Parameters (실패시) : code, message
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Response.Write(reader.ReadToEnd());
                }
            }
        }

        public void SetTls() // 페이지 시작할 때 얘 불러주기
        {
            bool platformSupportsTls12 = false;
            foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType)))
            {
                if (protocol.GetHashCode() == 3072)
                {
                    platformSupportsTls12 = true;
                }
            }

            // enable Tls12, if possible
            if (!ServicePointManager.SecurityProtocol.HasFlag((SecurityProtocolType)3072))
            {
                if (platformSupportsTls12)
                {
                    ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
                }
            }
        }

        /*끝********************************************************************************************************************************************************/

        public int paging(string page, int totalRowCount) /*페이징 처리 함수*/
        {
            int totalPageCount = 0;

            totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            if (page != null)
                return (int.Parse(page) - 1) * 10;
            else
                return 0;
        }

        // GET: Admin/ShowAllPurchase - 전체 구매 내역 출력
        public ActionResult ShowAllPurchase()
        {
            if (Session["IsAdmin"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "ShowAllPurchase";
                ru.ReturnCon = "Admin";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "AP";

            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "전체", Value = "-1" });

            items.Add(new SelectListItem { Text = "취소", Value = "0" });

            items.Add(new SelectListItem { Text = "정상", Value = "1" });

            items.Add(new SelectListItem { Text = "구매확정 전", Value = "2" });

            ViewBag.PurchaseStatus = items;

            int totalRowCount = DASManager.CheckLength(0, "purchase", null, null, -1, -1, null);
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<PurchaseDtlModel> m = DASManager.ShowPurchaseList(0, null, null, -1, null, null, pageNum, totalPageCount, 10, 0);
            return View(m);
        }

        /////* Ajax-GET: Admin/ShowAllPurchase - 구매 리스트 출력 - 엑셀 다운로드*/
        public JsonResult ShowAllPurchaseListByExcel(DateTime StartDate, DateTime EndDate, int Status, string UserId, string KeyWord)
        {
            DateTime end = EndDate.AddDays(1);

            UserModel um = DASManager.ShowUserDetail(UserId);
            int UserNo = um != null ? um.UserNo : 0;
            int totalRowCount = DASManager.CheckLength(UserNo, "purchase", StartDate, end, -1, Status, KeyWord);
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<PurchaseDtlModel> m = DASManager.ShowPurchaseList(0, StartDate, end, Status, UserId, KeyWord, pageNum, totalPageCount, 10, 1);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/PurchaseDetail - 구매 내역 상세 보기
        public ActionResult PurchaseDetail(int PurchaseNo)
        {
            if (Session["IsAdmin"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "ShowAllPurchase";
                ru.ReturnCon = "Admin";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "popup";
            PurchaseDtlModel m = DASManager.ShowPurchaseDetail(PurchaseNo);
            m.Gifticons = DASManager.ShowPurchasedGifticonList(PurchaseNo, -1, 0);
            return View(m);
        }

        /////* Ajax-GET: Admin/ShowAllPurchase - 구매 리스트 출력*/
        public JsonResult ShowAllPurchaseList(DateTime StartDate, DateTime EndDate, int Status, string UserId, string KeyWord)
        {
            DateTime end = EndDate.AddDays(1);

            UserModel um = DASManager.ShowUserDetail(UserId);
            int UserNo = um != null ? um.UserNo : 0;
            int totalRowCount = DASManager.CheckLength(UserNo, "purchase", StartDate, end, -1, Status, KeyWord);
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<PurchaseDtlModel> m = DASManager.ShowPurchaseList(0, StartDate, end, Status, UserId, KeyWord, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/PurchaseDetail - 구매 취소 실행*/
        public JsonResult DoPurchaseRefund(string RefundMsg, int PurchaseNo)
        {
            DASManager.InsertPurchaseRefundInfo(RefundMsg, PurchaseNo);
            string json = "";
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/ShowAllSelling - 전체 판매 내역 출력
        public ActionResult ShowAllSelling()
        {
            DASManager.UpdateGifticonStatus();
            if (Session["IsAdmin"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "ShowAllSelling";
                ru.ReturnCon = "Admin";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "AP";

            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "전체", Value = "-1" });

            items.Add(new SelectListItem { Text = "판매 완료", Value = "0" });

            items.Add(new SelectListItem { Text = "판매중", Value = "1" });

            items.Add(new SelectListItem { Text = "판매불가", Value = "2" });

            ViewBag.SellingStatus = items;

            int totalRowCount = DASManager.CheckLength(0, "selling", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<GifticonModel> m = DASManager.ShowRegistGifticonList(0, null, null, pageNum, totalPageCount, 10);
            return View(m);
        }

        /////* Ajax-GET: Admin/ShowAllPurchase - 구매 리스트 출력*/
        public JsonResult ShowAllSellingList(DateTime StartDate, DateTime EndDate, int Status, string UserId, string KeyWord)
        {
            DateTime end = EndDate.AddDays(1);
            UserModel um = DASManager.ShowUserDetail(UserId); // 사용자 존재 여부 확인
            int UserNo = um != null ? um.UserNo : 0;

            int totalRowCount = DASManager.CheckLength(UserNo, "selling", StartDate, end, -1, Status, KeyWord); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<GifticonModel> m = DASManager.ShowAllGifticonList(UserNo, StartDate, end, Status, KeyWord, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllPurchase - 구매 리스트 출력 - 엑셀 다운로드*/
        public JsonResult ShowAllSellingListByExcel(DateTime StartDate, DateTime EndDate, int Status, string UserId, string KeyWord)
        {
            DateTime end = EndDate.AddDays(1);
            UserModel um = DASManager.ShowUserDetail(UserId); // 사용자 존재 여부 확인
            int UserNo = um != null ? um.UserNo : 0;

            int totalRowCount = DASManager.CheckLength(UserNo, "selling", StartDate, end, -1, Status, KeyWord); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<GifticonModel> m = DASManager.ShowAllGifticonList(UserNo, StartDate, end, Status, KeyWord, pageNum, totalPageCount, 10, 1);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/ShowAllUser - 이용자 계좌출력
        public ActionResult ShowAllUser()
        {
            if (Session["IsAdmin"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "ShowAllUser";
                ru.ReturnCon = "Admin";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "AP";
            dynamic m = new ExpandoObject();
            m.AccountStatus = new AccountStatus();
            m.ChargeList = new ChargeList();
            m.ShowAllPurchaseViewModel = new ShowAllPurchaseViewModel();
            m.ShowAllSellingViewModel = new ShowAllSellingViewModel();
            m.ShowAllList = new ShowAllList();
            m.PGLogList = new PGLogList();
            m.FilterModel = new FilterModel();
            return View(m);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 계좌 상태 리스트 출력*/
        public JsonResult ShowUserTotalCash(int UserNo)
        {
            UserStatusModel usm = new UserStatusModel();
            usm.TotalCash = DASManager.ShowTotalCash(UserNo);
            usm.BonusCash = DASManager.ShowTotalCashByType(1, UserNo);
            usm.SellingCash = DASManager.ShowTotalCashByType(2, UserNo);
            usm.GiftiCash = DASManager.ShowTotalCashByType(3, UserNo);

            List<UserStatusModel> m = new List<UserStatusModel>();
            m.Add(usm);

            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 충전 리스트 출력*/
        public JsonResult ShowUserChargeList(int UserNo)
        {
            int totalRowCount = DASManager.CheckLength(UserNo, "charge", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<ChargeModel> m = DASManager.ShowChargeList(UserNo, null, null, null, -1, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 충전 리스트 출력 -  단, 필터링 됨*/
        public JsonResult ShowUserChargeListByDate(DateTime StartDate, DateTime EndDate, int UserNo)
        {
            DateTime end = EndDate.AddDays(1);

            int totalRowCount = DASManager.CheckLength(UserNo, "charge", StartDate, end, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<ChargeModel> m = DASManager.ShowChargeList(UserNo, StartDate, end, null, -1, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 판매 리스트 출력 -  단, 필터링 됨*/
        public JsonResult ShowUserSellingListByDate(DateTime StartDate, DateTime EndDate, int UserNo)
        {
            DateTime end = EndDate.AddDays(1);

            int totalRowCount = DASManager.CheckLength(UserNo, "selling", StartDate, end, -1, -1, null);
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<GifticonModel> m = DASManager.ShowAllGifticonList(UserNo, StartDate, end, -1, null, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 판매 리스트 출력*/
        public JsonResult ShowUserSellingList(int UserNo)
        {
            int totalRowCount = DASManager.CheckLength(UserNo, "selling", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<GifticonModel> m = DASManager.ShowAllGifticonList(UserNo, null, null, -1, null, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 구매 리스트 출력*/
        public JsonResult ShowUserPurchaseList(int UserNo)
        {
            int totalRowCount = DASManager.CheckLength(UserNo, "purchase", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<PurchaseDtlModel> m = DASManager.ShowPurchaseList(UserNo, null, null, -1, null, null, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 구매 리스트 출력 -  단, 필터링 됨*/
        public JsonResult ShowUserPurchaseListByDate(DateTime StartDate, DateTime EndDate, int UserNo)
        {
            DateTime end = EndDate.AddDays(1);

            int totalRowCount = DASManager.CheckLength(UserNo, "purchase", StartDate, end, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<PurchaseDtlModel> m = DASManager.ShowPurchaseList(UserNo, StartDate, end, -1, null, null, pageNum, totalPageCount, 10, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 리스트 출력 -  단, 필터링 됨*/
        public JsonResult ShowUserAllListByDate(DateTime StartDate, DateTime EndDate, int UserNo)
        {
            DateTime end = EndDate.AddDays(1);
            int pageNum = Request.QueryString["page"] == null ? 0 : (int.Parse(Request.QueryString["page"].ToString()) - 1) * 10;
            List<AllUserListModel> m = DASManager.ShowUserAllList(StartDate, end, UserNo, pageNum, 10);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 리스트 출력*/
        public JsonResult ShowUserAllList(int UserNo)
        {
            int pageNum = Request.QueryString["page"] == null ? 0 : (int.Parse(Request.QueryString["page"].ToString()) - 1) * 10;
            List<AllUserListModel> m = DASManager.ShowUserAllList(null, null, UserNo, pageNum, 10);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 PG 로그 리스트 출력 -  단, 필터링 됨*/
        public JsonResult ShowUserPGLogByDate(DateTime StartDate, DateTime EndDate, int UserNo)
        {
            DateTime end = EndDate.AddDays(1);
            int pageNum = Request.QueryString["page"] == null ? 0 : (int.Parse(Request.QueryString["page"].ToString()) - 1) * 10;
            List<PGLogModel> m = DASManager.ShowUserPGLogList(StartDate, end, UserNo, pageNum, 10);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Admin/ShowAllUser - 전체 PG 로그 리스트 출력*/
        public JsonResult ShowUserPGLog(int UserNo)
        {
            int pageNum = Request.QueryString["page"] == null ? 0 : (int.Parse(Request.QueryString["page"].ToString()) - 1) * 10;
            List<PGLogModel> m = DASManager.ShowUserPGLogList(null, null, UserNo, pageNum, 10);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }
    }
}