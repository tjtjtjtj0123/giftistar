using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using test211005.Models;
using System.Threading.Tasks;
using test211005.Content;
using Newtonsoft.Json;
using PayPal.Api;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using PagedList.Mvc;
using PagedList;
using log4net.Config;
using log4net;

namespace test211005.Controllers
{
    public class MyPageController : Controller
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /*페이징 처리 함수*/
        public int paging(string page, int totalRowCount, int innerRow)
        {
            int totalPageCount = 0;

            totalPageCount = totalRowCount / innerRow;
            if (totalRowCount % innerRow != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            if (page != null)
                return (int.Parse(page) - 1) * innerRow;
            else
                return 0;
        }

        //-----------------------------------------------------------------------------------------
        /*--MyPage/Index - 일반 사용자 마이페이지(My 캐시)*/
        //-----------------------------------------------------------------------------------------

        // GET: MyPage/Index - 일반 사용자 마이페이지(My 캐시)
        public ActionResult Index()
        {
            /*일반 사용자 세션 유무 확인*/
            if (Session["UserSession"] == null)
            { // 세션 無 : return url(현재 페이지 url) 담아서 Login 페이지로 Redirect
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "Index";
                ru.ReturnCon = "MyPage";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            /*캐시 충전 시도 확인해주는 세션 유무 확인*/
            if (Session["TryCharge"] != null) // 세션 有 : 세션 remove
                Session.Remove("TryCharge");

            ViewBag.Title = "MP"; // 좌측 퀵바 구분을 위한 ViewBag(일반 사용자 퀵바 - MP)

            /*캐시 현황 정보 받아오기*/
            MyPageViewModel m = new MyPageViewModel();
            m.User = (UserModel)Session["UserSession"];
            m.TotalCash = DASManager.ShowTotalCash(m.User.UserNo);              // 사용자의 캐시 현황 中 총 보유 잔액

            m.BonusCash = DASManager.ShowTotalCashByType(1, m.User.UserNo);     // 사용자의 캐시 현황 中 보너스 캐시 잔액
            m.SellingCash = DASManager.ShowTotalCashByType(2, m.User.UserNo);   // 사용자의 캐시 현황 中 판매금 캐시 잔액
            m.GiftiCash = DASManager.ShowTotalCashByType(3, m.User.UserNo);     // 사용자의 캐시 현황 中 충전 캐시 잔액

            /*페이징 처리(원래는 output 파라미터로 처리해주는데 수정 덜 됨)*/
            int totalRowCount = DASManager.CheckLength(m.User.UserNo, "charge", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 5;
            if (totalRowCount % 5 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 5);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 5 + 1;

            /*하단에 출력될 충전 내역 리스트 받아오기*/
            m.ChargingList = DASManager.ShowChargeList(m.User.UserNo, null, null, null, -1, pageNum, totalPageCount, 5, 0);
            return View(m);
        }

        /////* Ajax-GET: MyPage/Index - 기간 필터링해서 충전 리스트 출력*/
        public JsonResult ShowChargeListByDate(DateTime StartDate, DateTime EndDate)
        {
            UserModel um = (UserModel)Session["UserSession"];
            DateTime end = EndDate.AddDays(1);

            int totalRowCount = DASManager.CheckLength(um.UserNo, "charge", StartDate, end, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 5;
            if (totalRowCount % 5 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 5);
            ViewBag.Page = pageNum / 5;

            List<Models.ChargeModel> m = DASManager.ShowChargeList(um.UserNo, StartDate, end, null, -1, pageNum, totalPageCount, 5, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: MyPage/Index - 충전 리스트 출력*/
        public JsonResult ShowChargeList()
        {
            UserModel um = (UserModel)Session["UserSession"];

            int totalRowCount = DASManager.CheckLength(um.UserNo, "charge", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 5;
            if (totalRowCount % 5 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 5);
            ViewBag.Page = pageNum / 5;

            List<Models.ChargeModel> m = DASManager.ShowChargeList(um.UserNo, null, null, null, -1, pageNum, totalPageCount, 5, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: MyPage/Index - 기간 필터링해서 구매 리스트 출력*/
        public JsonResult ShowPurchaseListByDate(DateTime StartDate, DateTime EndDate)
        {
            UserModel um = (UserModel)Session["UserSession"];
            DateTime end = EndDate.AddDays(1);

            int totalRowCount = DASManager.CheckLength(um.UserNo, "purchase", StartDate, end, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 5;
            if (totalRowCount % 5 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 5);
            ViewBag.Page = pageNum / 5;

            List<PurchaseDtlModel> m = DASManager.ShowPurchaseList(um.UserNo, StartDate, end, -1, null, null, pageNum, totalPageCount, 5, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: MyPage/Index - 판매 리스트 출력*/
        public JsonResult ShowPurchaseList()
        {
            DASManager.UpdateGifticonStatus();
            UserModel um = (UserModel)Session["UserSession"];

            int totalRowCount = DASManager.CheckLength(um.UserNo, "purchase", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 5;
            if (totalRowCount % 5 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 5);
            ViewBag.Page = pageNum / 5;

            List<PurchaseDtlModel> m = DASManager.ShowPurchaseList(um.UserNo, null, null, -1, null, null, pageNum, totalPageCount, 5, 0);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: MyPage/Index - 기간 필터링해서 판매 리스트 출력*/
        public JsonResult ShowSellingListByDate(DateTime StartDate, DateTime EndDate)
        {
            UserModel um = (UserModel)Session["UserSession"];
            DateTime end = EndDate.AddDays(1);

            int totalRowCount = DASManager.CheckLength(um.UserNo, "selling", StartDate, end, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 5;
            if (totalRowCount % 5 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 5);
            ViewBag.Page = pageNum / 5;

            List<GifticonModel> m = DASManager.ShowRegistGifticonList(um.UserNo, StartDate, end, pageNum, totalPageCount, 5);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: MyPage/Index - 판매 리스트 출력*/
        public JsonResult ShowSellingList()
        {
            UserModel um = (UserModel)Session["UserSession"];

            int totalRowCount = DASManager.CheckLength(um.UserNo, "selling", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 5;
            if (totalRowCount % 5 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 5);
            ViewBag.Page = pageNum / 5;

            List<GifticonModel> m = DASManager.ShowRegistGifticonList(um.UserNo, null, null, pageNum, totalPageCount, 5);
            string json = JsonConvert.SerializeObject(m.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        //-----------------------------------------------------------------------------------------
        /*--MyPage/Charge - 캐시 충전*/
        //-----------------------------------------------------------------------------------------

        // GET: MyPage/Charge - 캐시 충전
        public ActionResult Charge(int? id)
        {
            ViewBag.Title = "MP"; // 좌측 퀵바 구분을 위한 ViewBag(일반 사용자 퀵바 - MP)

            Session["BackItemNo"] = id != null ? id : 0;

            /*일반 사용자 세션 유무 확인*/
            if (Session["UserSession"] == null)
            {// 세션 無 : return url(현재 페이지 url) 담아서 Login 페이지로 Redirect
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "Chrage";
                ru.ReturnCon = "MyPage";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            UserModel um = (UserModel)Session["UserSession"];

            /*블랙 유저인지 확인*/
            BlackUserModel bm = new BlackUserModel();
            bm.User = um;

            /*충전 제한 고객인 경우*/
            if (um.UserStatus == 2)
            {
                if (um.BanReleaseDate <= DateTime.Now) // 충전 제한 날짜가 지난 경우
                {
                    // 충전 제한 해제
                    DASManager.UpdateUserStatus(um.UserNo);
                }
                else /*결제 실패 - 충전 제한 고객*/
                {
                    bm.DeniedMsg = "캐시충전이 제한된 상태입니다.";
                    return View("ChargeFail", bm);
                }
            }
            else if (um.UserStatus == 0) /*결제 실패 - 휴면 고객*/
            {
                bm.DeniedMsg = "휴면 상태로 캐시충전이 제한된 상태입니다.";
                return View("ChargeFail", bm);
            }

            ChargeViewModel m = new ChargeViewModel();
            if (Session["TryCharge"] != null) // 충전 시도했던 정보 有
                m = (ChargeViewModel)Session["TryCharge"]; // 출력할 모델에 시도했던 정보 넣어주기
            else
            { // 시도했던 정보 無 : 초기 정보 넣어주기
                m.Payment = 0;
                m.ChargeAmount = 1;
                m.TotalCash = DASManager.ShowTotalCash(um.UserNo);
            }
            return View(m);
        }

        // POST: /MyPage/Charge - 캐시 충전
        [HttpPost]
        public ActionResult Charge(ChargeViewModel m)
        {
            ViewBag.Title = "MP"; // 좌측 퀵바 구분을 위한 ViewBag(일반 사용자 퀵바 - MP)

            /*충전 시도했던 정보가 있는 경우, remove*/
            if (Session["TryCharge"] != null)
                Session.Remove("TryCharge"); // 세션 有 : 세션 refresh
            Session["TryCharge"] = m;

            UserModel um = (UserModel)Session["UserSession"];
            m.TotalCash = DASManager.ShowTotalCash(um.UserNo); // 현재 사용자의 총잔액
            m.RegDate = DateTime.Now;

            /*필수 동의항목에 동의하지 않은 경우 현재 페이지로 Return*/
            if (m.AgreementCheck1 == false || m.AgreementCheck2 == false)
            {
                Response.Write(@"<script language='javascript'>alert('필수동의항목에 동의해주세요.');</script>");
                return View(m);
            }

            //-----------------------------------------------------------------------------------------
            /*결제 시도 가능*/
            //-----------------------------------------------------------------------------------------
            m.ChargeCashAmount = m.ChargeAmount * 1000; // 충전될 금액(예. ChargeAmount : 1 / ChargeCashAmount : 1000원)

            if (m.Payment == 0)
            { // 신용카드
                m.ChargePaidAmount = m.ChargeCashAmount;
                m.PaymentStr = "신용카드";
                m.ChargePaidAmountStr = String.Format("{0:#,0}", m.ChargePaidAmount) + "원";
                m.ChargedTotalCash = m.TotalCash + m.ChargeCashAmount; // 총 잔액 = 현 총잔액 + 충전 캐시 금액

                /*PG 로그 등록*/
                PGLogList pgm = new PGLogList();
                pgm.Payment = m.Payment;
                pgm.ChargePaidAmount = m.ChargePaidAmount;
                pgm.ChargeCashAmount = m.ChargeCashAmount;
                pgm.UserNo = um.UserNo;
                pgm.PGName = "creditcard";

                m.OrderNo = DASManager.InsertPGLofInfo(pgm);
                m.User = (UserModel)Session["UserSession"];
                Load_POQ(m.Payment, m.OrderNo, m.ChargePaidAmount);  // 결제 시작
                return View("Charge", m);
            }
            else if (m.Payment == 1)
            { // PayPal
                m.ChargePaidAmount = m.ChargeAmount;
                m.PaymentStr = "PayPal";
                m.ChargePaidAmountStr = String.Format("{0:#,0}", m.ChargePaidAmount) + "$";
                m.ChargedTotalCash = m.TotalCash + m.ChargeCashAmount;

                /*PG 로그 등록*/
                PGLogList pgm = new PGLogList();
                pgm.Payment = m.Payment;
                pgm.ChargePaidAmount = m.ChargePaidAmount;
                pgm.ChargeCashAmount = m.ChargeCashAmount;
                pgm.UserNo = um.UserNo;
                pgm.PGName = "PayPal";
                m.OrderNo = DASManager.InsertPGLofInfo(pgm);

                SetTls();
                System.Net.ServicePointManager.ServerCertificateValidationCallback +=
                delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                                        System.Net.Security.SslPolicyErrors sslPolicyErrors)
                {
                    return true; // **** Always accept
                };
                // Gettings context from the paypal bases on clientId and clientSecret for payment
                APIContext apiContext = PaypalConfiguration.GetAPIContext(); // clientId, clientSecret 정보

                /*페이팔 결제 시도 존재 유무 확인 후, 시도 setting*/
                string payerId = Request.Params["PayerID"]; // 결제 완료시 PayerId를 값으로 줌

                if (string.IsNullOrEmpty(payerId)) // 결제하지 않은 상태인지 확인
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/MyPage/PaymentWithPaypal/"+ m.OrderNo +"?"; // 결제를 하기 위해 이 url로 보내줌
                    var guid = Convert.ToString((new Random()).Next(100000)); // 페이팔 결제 사용할 새로운 랜덤값의 guid 생성

                    /* SET - 결제할 정보 넣은 Paypal 결제 정보 생성 */
                    var createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid, m.ChargePaidAmount);

                    // Get links returned from paypal response to create call function
                    var links = createdPayment.links.GetEnumerator(); // 페이팔 시도할 수 있는 link 생성
                    string paypalRedirectUrl = string.Empty;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url")) // 페이팔 결제 수단 결정하는 페이지 호출하는 부분이 나올때까지 넘겨서 결제창 띄우는 url 추출하기
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }
                    Session.Add(guid, createdPayment.id); // 세션에 guid 등록
                    m.ChargedTotalCash = m.TotalCash + m.ChargeCashAmount;
                    Response.Write(@"<script language='javascript'>window.open('" + paypalRedirectUrl + "', '네이버팝업', 'width=500, height=700, scrollbars=yes, resizable=no');</script>");
                    //return Redirect(paypalRedirectUrl); // PayPal 결제 시도
                    return View("Charge", m);
                }
            }
            else if (m.Payment == 2)
            { // 휴대폰
                m.ChargePaidAmount = m.ChargeCashAmount;
                m.PaymentStr = "휴대폰";
                m.ChargePaidAmountStr = String.Format("{0:#,0}", m.ChargePaidAmount) + "원";
                m.ChargedTotalCash = m.TotalCash + m.ChargeCashAmount;

                /*PG 로그 등록*/
                PGLogList pgm = new PGLogList();
                pgm.Payment = m.Payment;
                pgm.ChargePaidAmount = m.ChargePaidAmount;
                pgm.ChargeCashAmount = m.ChargeCashAmount;
                pgm.UserNo = um.UserNo;
                pgm.PGName = "mobile";

                m.OrderNo = DASManager.InsertPGLofInfo(pgm);
                m.User = (UserModel)Session["UserSession"];
                Load_POQ(m.Payment, m.OrderNo, m.ChargePaidAmount); // 결제 시작

                return View("Charge", m);
            }
            return View("Charge", m);
        }

        /*시작 - Paypal *******************************************************************************************************************************************/

        public ActionResult PaymentWithPaypal(int id)
        { /*PayPal 결제 시도 setting */
            ViewBag.Title = "popup";

            APIContext apiContext = PaypalConfiguration.GetAPIContext(); // PayPal API 사용 setting
            try
            {
                string payerId = Request.Params["PayerID"];

                /* 결제 시도 setting 유무 확인*/
                if (string.IsNullOrEmpty(payerId))
                { /*PayPal 결제 시도 setting 無*/
                    logger.Error(Request.Url.Scheme + "://" + Request.Url.Authority);
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/MyPage/PaymentWithPaypal/" + id + "?";
                    var guid = Convert.ToString((new Random()).Next(100000));
                    var createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid, 0);

                    // Get links returned from paypal response to create call function
                    /*GET - 생성한 결제 시도 정보로 결제창 띄울 링크 생성*/
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = string.Empty;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else /*PayPal 결제 시도 setting 有*/
                {
                    // This one will be executed when we have received all the payment params from previous call
                    var guid = Request.Params["guid"]; // PayPal 결제 실행시 사용할 guid

                    /*DO - 결제 정보를 넣은 새 결제 생성 실행*/
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        //return View("Failure"); // 실패했을 때 페이지
                        return RedirectToAction("ChargePaymentSuc/" + id, "MyPage");
                    }
                }
            }
            catch (Exception ex)
            {
                PaypalLogger.Log("Error: " + ex.Message);
                return RedirectToAction("ChargePaymentSuc/" + id, "MyPage");
                //return View("Failure");
            }
            //return View("ChargePaymentSuc"); // 결제 성공 시
            return RedirectToAction("ChargePaymentSuc/" + id, "MyPage");
        }

        /* Work with Paypal Payment*/
        private Payment payment;

        // create a payment using an APIContext (API 사용해서 페이팔 결제 정보 생성)
        private Payment CreatePayment(APIContext apiContext, string redirectUrl, int total) // 구매할 상품의 이름, 가격, 결제 화폐, 수량, 세금, 수수료 등을 설정함
        {
            var listItems = new ItemList() { items = new List<Item>() };

            /*상품명, 가격, 화폐단위, 수량 설정*/
            listItems.items.Add(new Item()
            {
                name = "기프티캐시",
                currency = "USD",
                price = total.ToString(),
                quantity = "1"
            });

            var payer = new Payer() { payment_method = "paypal" };

            // Do the configuration RefirectURLs here with redirectURLs object - 결제 후 사용할 return url 설정
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl
            };

            // Create details objdect - 세금, 수수료 설정
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = total.ToString() // 최종 정산 가격 = TotalPrice
            };

            // Create amount object - 총 결제해야할 금액에 대한 정보
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(), //tax + shipping + subtotal(총 사용자가 결제해야하는 금액)
                details = details
            };

            // Create transaction - 결제 트랜잭션 생성
            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "syj Testing transaction description",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = listItems
            });

            payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            return payment.Create(apiContext);
        }

        // Create ExecutePayment method - 결제 트랜잭션 실행
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

        /*끝********************************************************************************************************************************************/

        /*시작 - POQ********************************************************************************************************************************************/
        public void Load_POQ(int payment, int orderNo, int chargePaidAmount)
        {
            /*결제 시도*/
            SetTls(); // 버전 세팅
            //-----------------------------------------------------------------------------------------
            // Description    : 결제 요청 API URL 및 파라미터 설정
            //                - 테스트 URL : https://testpgapi.payletter.com/v1.0/payments/request
            //                - 라이브 URL : https://pgapi.payletter.com/v1.0/payments/request
            //                - 휴대폰 이외의 PG로 결제 요청시 설정할 파라미터는 가이드 문서 참고를 참고하시기 바랍니다.
            //                - 가이드 문서 URL: https://pg.payletter.com/APIDocument/index.html
            //-----------------------------------------------------------------------------------------
            string strURL = "https://testpgapi.payletter.com/v1.0/payments/request"; // 최초 자동 결제 요청 API 호출
            string domainURL = Request.Url.Scheme + "://" + Request.Url.Authority; // 최초 자동 결제 요청 API 호출

            string pgCode = "";
            if (payment == 0) // 신용카드
                pgCode = "creditcard";
            else if (payment == 2) // 휴대폰
                pgCode = "mobile";
            logger.Error(Request.Url.Scheme + "://" + Request.Url.Authority);

            UserModel um = (UserModel)Session["UserSession"];

            // 결제 요청 파라미터 설정 (JSON)
            // callback_url : 결제 완료 후 callback을 받을 가맹점 페이지 주소
            string strPostData = "{\"pgcode\" : \"" + pgCode + "\"," +
                                  "\"user_id\":\"" + um.UserId + "\"," +
                                  "\"user_name\":\"" + um.UserName + "\"," +
                                  "\"service_name\":\"페이레터\"," +
                                  "\"client_id\":\"pay_test\"," +
                                  "\"order_no\":\"" + orderNo + "\"," +
                                  "\"amount\":" + chargePaidAmount + "," +
                                  "\"product_name\":\"기프티캐시\"," +
                                  "\"email_flag\":\"N\"," +
                                  "\"autopay_flag\":\"N\"," +
                                  "\"receipt_flag\":\"Y\"," +
                                  "\"custom_parameter\":\"this is custom parameter\"," +
                                  "\"return_url\":\"" + domainURL + "/MyPage/ChargePaymentSuc/" + orderNo + "\"," +
                                  "\"callback_url\":\"" + domainURL + "/MyPage/ChargeNoti\"}";


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
                objWRequest.ContentType = "application/json; charset=utf-8";
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

                // 요청 처리 성공인 경우 - 결제창 url 넘겨줌                                 
                // Response Parameters (성공시) : token, online_url, mobile_url
                HttpWebResponse objWResponse = (HttpWebResponse)objWRequest.GetResponse();

                StreamReader objStreamReader = new StreamReader(objWResponse.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                string strResponse = objStreamReader.ReadToEnd();
                JObject json = JObject.Parse(strResponse);
                string url = (string)json["online_url"].ToString();
                Response.Write(@"<script language='javascript'>window.open('" + url + "', '네이버팝업', 'width=500, height=700, scrollbars=yes, resizable=no');</script>");

                objStreamReader.Close();
                objWResponse.Close();
                //Load_Callback();
            }
            // 성공이 아닌 경우
            // Response Parameters (실패시) : code, message
            catch (WebException ex)
            {
                return;
                //using (var stream = ex.Response.GetResponseStream())
                //using (var reader = new StreamReader(stream))
                //{
                //    Response.Write(reader.ReadToEnd());
                //}
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

        /*noti 받는 부분*/
        protected void Load_Callback()
        {
            //-----------------------------------------------------------------------------------------
            // Description    : Callback URL로 반환된 결제 결과 받아오기 (json)
            //                - 결제가 성공한 경우 callback_url로 결제 결과가 반환됩니다. (json)
            //                - 전달받은 Callback URL 을 통해서 결과값을 받아서 가맹점에 맞는 충전, 구매 등의 로직을 수행하도록 합니다.
            //-----------------------------------------------------------------------------------------
            StreamReader objStreamReader = new StreamReader(Request.GetBufferlessInputStream(), Encoding.GetEncoding("utf-8"));
            string strResponse = objStreamReader.ReadToEnd();

            objStreamReader.Close();

            //-- Callback URL로 반환된 결제 결과 받기
            if (string.IsNullOrEmpty(strResponse))
            {
                Response.Write("{\"code\":9999, \"message\":\"Response data is empty\"}");
                return;
            }

            //-- JSON 파싱
            PayResult objJsonData = JsonConvert.DeserializeObject<PayResult>(strResponse);

            //-----------------------------------------------------------------------------------------
            // Description    : 결제 결과 정보 세팅
            //                - 고객사에서 아래 결제 정보를 DB에 저장해 놓으시면 됩니다. 
            //                - 아래 정보들을 디버깅성으로 파일로그를 남겨놓으시기를 권고 드립니다.
            //                - Return Url / CallBack Url로 전달된 파라메터는 위/변조 방지를 위하여 SHA256 hash 값을 생성한 후 전달된 payhash 와 비교 검증을 수행하시기 바랍니다.
            //                - CallBack URL에서 처리 완료 후 성공시 {"code":0, "message":"실패시 실패 사유"} 형태의 json 문자열을 출력해 주시기 바랍니다.
            //                - code 가 0이 아닌 경우에는 통보가 실패한 것으로 간주되어 5분마다 최대 20번까지 재 전송됩니다.
            //-----------------------------------------------------------------------------------------
            string strUserID = objJsonData.user_id;                      //--가맹점 결제자(회원) 아이디(email, 영문 및 숫자 가능)
            string strUserName = objJsonData.user_name;                    //--가맹점 결제자(회원) 이름
            string strOrderNo = objJsonData.order_no;                     //--가맹점의 주문 번호
            string strServiceName = objJsonData.service_name;                 //--결제 서비스명
            string strProductName = objJsonData.product_name;                 //--결제상품명

            logger.Error("orderNo : " + strOrderNo);

            string strCustomParameter = objJsonData.custom_parameter;             //--주문요청시 가맹점이 전송한 값    
            string strTID = objJsonData.tid;                          //--결제고유번호
            string strCID = objJsonData.cid;                          //--승인번호
            double dblAmt = objJsonData.amount;                       //--결제요청 금액
            string strPayInfo = objJsonData.pay_info;                     //--결제 부가정보

            string strPGCode = objJsonData.pgcode;                       //--결제요청한 pg명
            string strBillKey = objJsonData.billkey;                      //--자동결제 재결제용 빌키
            string strDomesticFlag = objJsonData.domestic_flag;                //--국내 / 해외 신용카드 구분 (Y : 국내, N : 해외)
            string strTransactionDate = objJsonData.transaction_date;             //--거래일시(YYYY-MM-DD HH:MM:SS)
            string strCardInfo = objJsonData.card_info;                    //--카드 번호 (중간 4자리 masking 처리)

            string strPayHash = objJsonData.payhash;                      //--파라메터 검증을 위한 SHA256 hash 값 SHA256(user_id +amount + tid +결제용 API Key) * 일부 결제 수단은 전달되지 않습니다.(가상계좌 등)
            string strInstallMonth = objJsonData.installmonth;                 //--할부개월수

            DASManager.UpdatePGLogInfo(int.Parse(strOrderNo), int.Parse(dblAmt.ToString()), strTID); // DB 정보와 비교후 update (orderNo/ ChargPaidAmount / Tid)

            /*로그 찍는다*/
            //FileInfo log4netFile = new FileInfo(HttpContext.Current.Request.ServerVariables["APPL_PHYSICAL_PATH"] + "log4net.config");
            //XmlConfigurator.Configure(log4netFile);
            //ILog log = LogManager.GetLogger("FileLog");
            //log.Info("==============POQ_CAll BACK START================");

            //-- 현금영수증은 일부 결제 수단에 한해서 전달됩니다. callback_url에서 전달받은 결제 결과 확인 후 세팅 바랍니다.
            //string strReceiptCID        = objJsonData.cash_receipt.cid;             //--승인번호
            //string strReceiptCode       = objJsonData.cash_receipt.code;            //--결과코드
            //string strReceiptDealNo     = objJsonData.cash_receipt.deal_no;         //--발급시 전달받은 주문번호
            //string strReceiptIssueType  = objJsonData.cash_receipt.issue_type;      //--현금영수증 발행 구분(1:구매자발급, 2:자체발급)
            //string strReceiptMsg        = objJsonData.cash_receipt.message;         //--결과메시지

            //string strReceiptPayerSID   = objJsonData.cash_receipt.payer_sid;       //--신분확인 번호(휴대폰번호, 사업자번호)
            //string strReceiptType       = objJsonData.cash_receipt.type;            //--거래자구분(01:소득공제용, 02:사업자지출증빙용)


            //-- 처리 완료 후 성공시 {"code":0, "message":"success"} 외의 html 및 다른 코드가 해당페이지에 노출 되지 않도록 합니다.
            //-- code 값은 성공시 0, 실패시 0이 아닌 값
            Response.Write("{\"code\":0, \"message\":\"success\"}");

            objJsonData = null;
        }
        /*끝********************************************************************************************************************************************/

        // GET: MyPage/ChargeSuc -- 결제 성공
        public ActionResult ChargeSuc(int id)
        {
            ViewBag.Title = "MP";
            ChargeViewModel m = new ChargeViewModel();
            PGLogList pgm = DASManager.ShowPGLogDetail(0, id); // orderno에 해당하는 결제 정보 받아옴
            UserModel um = (UserModel)Session["UserSession"];

            m.OrderNo = id;
            m.Payment = pgm.Payment;
            m.ChargePaidAmount = pgm.ChargePaidAmount;
            m.ChargeCashAmount = pgm.ChargeCashAmount;
            m.ChargedTotalCash = DASManager.ShowTotalCash(um.UserNo) + m.ChargeCashAmount;
            m.RegDateStr = pgm.RegDateStr;

            if (m.Payment == 0)
            {
                m.PaymentStr = "신용카드";
                m.ChargePaidAmountStr = String.Format("{0:#,0}", m.ChargePaidAmount) + "원";
            }
            else if (m.Payment == 1)
            {
                m.PaymentStr = "PayPal";
                m.ChargePaidAmountStr = String.Format("{0:#,0}", m.ChargePaidAmount) + "$";
            }
            else if (m.Payment == 2)
            {
                m.PaymentStr = "핸드폰";
                m.ChargePaidAmountStr = String.Format("{0:#,0}", m.ChargePaidAmount) + "원";
            }

            if (m.Payment == 0 || m.Payment == 2)
                logger.Error("orderNo: " + m.OrderNo); // Callback 들어왔는지 파일로그로 확인
            else
            {
                DASManager.UpdatePGLogInfo(m.OrderNo, m.ChargePaidAmount, null);
                //DASManager.InsertChargeInfo(m, um); // 충전 데이터 insert -- paypal은 노티 없으니까 여기서
            }

            DASManager.InsertChargeInfo(m, um); // 충전 데이터 insert

            m.BackItemNo = (int)Session["BackItemNo"];

            Session.Remove("TryCharge");
            Session.Remove("backItemNo");
            return View(m);
        }

        // POQ - Callback 시, noti 확인해주는 부분
        public ActionResult ChargeNoti()
        {
            ViewBag.Title = "MP";
            Load_Callback();
            //DASManager.InsertChargeInfo(m, um); // 충전 데이터 insert -- 원래는 여기에 적어야하는데 지금은 ChargeSuc에 있음
            return View();
        }

        // GET: MyPage/ChargeFail
        public ActionResult ChargeFail()
        {
            ViewBag.Title = "MP";
            return View();
        }

        // GET: MyPage/MyPurchaseList
        public ActionResult MyPurchaseList()
        {
            if (Session["UserSession"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "MyPurchaseList";
                ru.ReturnCon = "MyPage";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            if (Session["TryCharge"] != null)
                Session.Remove("TryCharge");

            ViewBag.Title = "MP";
            UserModel um = (UserModel)Session["UserSession"];

            int totalRowCount = DASManager.CheckLength(um.UserNo, "purchase", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 10);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<PurchaseDtlModel> m = DASManager.ShowPurchaseList(um.UserNo, null, null, -1, null, null, pageNum, totalPageCount, 10, 0);
            return View(m);
        }

        // GET: MyPage/MyPurchaseDetail
        public ActionResult MyPurchaseDetail(int id)
        {
            if (Session["UserSession"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "MyPurchaseList";
                ru.ReturnCon = "MyPage";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "MP";
            PurchaseDtlModel m = DASManager.ShowPurchaseDetail(id);
            m.Gifticons = DASManager.ShowPurchasedGifticonList(id, -1, 0);
            return View(m);
        }

        ///* Ajax-GET: MyPage/MyPurchaseList - 구매확정 버튼 클릭*/
        public JsonResult AcceptToPurchase(int PurchaseNo)
        {
            UserModel um = (UserModel)Session["UserSession"];
            DASManager.UpdateGifticonSellingStatus(PurchaseNo);
            string json = "";
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: MyPage/MyItemList
        public ActionResult MyItemList()
        {
            if (Session["UserSession"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "MyItemList";
                ru.ReturnCon = "MyPage";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            if (Session["TryCharge"] != null)
                Session.Remove("TryCharge");

            ViewBag.Title = "MP";
            UserModel um = (UserModel)Session["UserSession"];

            int pageNum = Request.QueryString["page"] == null ? 0 : (int.Parse(Request.QueryString["page"].ToString()) - 1) * 10;
            List<HoldingItemModel> m = DASManager.ShowHoldingItemInfo(um.UserNo, pageNum, 10);
            int totalRowCount = 0;
            if (m.Count == 0)
                totalRowCount = 0;
            else
                totalRowCount = m[0].TotalRowCount;
            for (int i = 0; i < m.Count; i++)
                m[i].AvailableItemAmount = DASManager.checkAvailCount(m[i].HoldingItemNo);
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            return View(m);
        }

        // GET: MyPage/MyItemDetail
        [HttpGet]
        public ActionResult MyItemDetail(int id, string imgUrl)
        {
            DASManager.UpdateGifticonStatus();
            if (Session["UserSession"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "MyItemList";
                ru.ReturnCon = "MyPage";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            if (imgUrl != null)
                DownLoad(imgUrl);

            ViewBag.Title = "MP";
            UserModel um = (UserModel)Session["UserSession"];
            HoldingItemModel m = DASManager.ShowHoldingItemDetail(id, um.UserNo);
            m.Gifticons = DASManager.ShowHoldingGifticonInfo(id, um.UserNo, -1, 0);
            foreach (GifticonModel gm in m.Gifticons)
                if (um.UserNo == gm.BuyerNo)
                    gm.IsPresent = 0;
                else
                    gm.IsPresent = 1;
            return View(m);
        }

        // GET: MyPage/MySalesList
        public ActionResult MySalesList()
        {
            DASManager.UpdateGifticonStatus();
            if (Session["UserSession"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "MysalesList";
                ru.ReturnCon = "MyPage";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            if (Session["TryCharge"] != null)
                Session.Remove("TryCharge");
            ViewBag.Title = "MP";
            UserModel um = (UserModel)Session["UserSession"];

            int totalRowCount = DASManager.CheckLength(um.UserNo, "selling", null, null, -1, -1, null); // 출력할 총 컬럼 개수
            int totalPageCount = totalRowCount / 10;
            if (totalRowCount % 10 != 0) // 마지막 페이지set에 페이지수가 5개 아닌 경우
                totalPageCount++;
            Session["totalPageCount"] = totalPageCount;
            string page = Request.QueryString["page"] == null ? null : Request.QueryString["page"].ToString();
            int pageNum = paging(page, totalRowCount, 10);
            ViewBag.Page = pageNum == 0 ? 1 : pageNum / 10 + 1;

            List<GifticonModel> m = DASManager.ShowRegistGifticonList(um.UserNo, null, null, pageNum, totalPageCount, 10);
            return View(m);
        }

        // GET: MyPage/MySalesDetail
        public ActionResult MySalesDetail(int id)
        {
            DASManager.UpdateGifticonStatus();
            if (Session["UserSession"] == null)
            {
                ReturnUrl ru = new ReturnUrl();
                ru.ReturnAct = "MySalesList";
                ru.ReturnCon = "MyPage";
                Session["ReturnUrl"] = ru;
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Title = "MP";
            GifticonModel m = DASManager.ShowGifticonDetail(id);
            m.imgfile = m.imgfile.Substring(1);
            return View(m);
        }

        public ActionResult Success()
        {
            return View();
        }

        public ActionResult Loading()
        {
            ViewBag.Title = "MP";
            return View();
        }

        public ActionResult ChargePaymentSuc(int? id)
        {
            ViewBag.Title = "popup";
            ViewBag.OrderNo = id;
            return View();
        }

        public void DownLoad(string imgurl)
        {
            logger.Error("im in download func");
            string fullFileName = "기프티콘이미지";
            logger.Error(System.IO.Directory.GetCurrentDirectory());
            string rootUrl = "D:/Webhosting/test211005/test211005";
            rootUrl += "/Content/upload/" + imgurl;
            System.Drawing.Image img = System.Drawing.Image.FromFile(rootUrl);
            MemoryStream ms = new MemoryStream();
            if (imgurl.Substring(imgurl.Length - 3).Equals("png"))
            {
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                fullFileName += ".png";
            }
            else
            { // (imgurl.Substring(imgurl.Length - 3).Equals("jpg"))
                img.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                fullFileName += ".jpg";
            }

            byte[] fileByteArray = ms.ToArray();
            Response.Clear();
            Response.ContentType = "application/unknown";
            Response.AddHeader("content-disposition", "attachment; filename=" + fullFileName);
            Response.OutputStream.Write(fileByteArray, 0, fileByteArray.Length); Response.End();
        }

        public ActionResult Withdraw()
        {
            if (Session["UserSession"] == null)
                Response.Write(@"<script language='javascript'>window.close();</script>");
            ViewBag.Title = "popup";
            WithdrawModel m = new WithdrawModel();
            UserModel um = (UserModel)Session["UserSession"];
            m.TotalCash = DASManager.ShowTotalCash(um.UserNo);
            return View(m);
        }

        public ActionResult WithdrawSuc(int withdrawAmount, string accountNo)
        {
            ViewBag.Title = "popup";
            WithdrawModel m = new WithdrawModel();
            UserModel um = (UserModel)Session["UserSession"];
            m.TotalCash = DASManager.ShowTotalCash(um.UserNo);
            m.WithdrawAmount = withdrawAmount;
            m.AccountNo = accountNo;

            Load_WithdrawApi(m);
            DASManager.InsertWithdrawInfo(um.UserNo, withdrawAmount);
            return View();
        }

        /*캐시 출금 신청*/
        public void Load_WithdrawApi(WithdrawModel m)
        {
            /*결제 시도*/
            SetTls(); // 버전 세팅
            //-----------------------------------------------------------------------------------------
            // 출금 URL 설정
            //-----------------------------------------------------------------------------------------

            string strURL = "https://developers.nonghyup.com/ReceivedTransferAccountNumber.nh"; // 최초 자동 결제 요청 API 호출
            string domainURL = Request.Url.Scheme + "://" + Request.Url.Authority; // 최초 자동 결제 요청 API 호출

            Random r = new Random();
            string v_strIsTuno = r.Next(100000000).ToString();
            // 결제 요청 파라미터 설정 (JSON)
            // callback_url : 결제 완료 후 callback을 받을 가맹점 페이지 주소
            string strPostData = "{" +
                                 "\"Header\":{" +
                                 "\"ApiNm\":\"ReceivedTransferAccountNumber\"," +
                                 "\"Tsymd\":\"20211126\"," +
                                 "\"Trtm\":\"003544\"," +
                                 "\"Iscd\":\"001364\"," +
                                 "\"FintechApsno\":\"001\"," +
                                 "\"ApiSvcCd\":\"ReceivedTransferA\", " +
                                 "\"IsTuno\":\"" + v_strIsTuno + "\"," +
                                 "\"AccessToken\": " +
                                 "\"373066d671d95d3e5b6b3b880a5d26694efe6412248b3bd2423e8516229f455b\"}," +
                                 "\"Bncd\":\"011\"," +
                                 "\"Acno\":\"" + m.AccountNo + "\"," +
                                 "\"Tram\":\"" + m.WithdrawAmount + "\"," +
                                 "\"DractOtlt\":\"테스트\"," +
                                 "\"MractOtlt\":\"테스트\"" +
                                 "}";
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
                //HttpWebRequest objWRequest = (HttpWebRequest)WebRequest.Create(strURL);
                //objWRequest.Method = "POST";
                //objWRequest.ContentType = "application/json; charset=utf-8";

                //byte[] objResultByte = Encoding.UTF8.GetBytes(strPostData);
                //objWRequest.ContentLength = objResultByte.Length;

                //Stream objStream = objWRequest.GetRequestStream();
                //objStream.Write(objResultByte, 0, objResultByte.Length);
                //objStream.Close();

                //objWRequest.Timeout = 20000;

                string uri = strURL;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json; utf-8";

                //Stream objStream = objWRequest.GetRequestStream();
                //objStream.Write(objResultByte, 0, objResultByte.Length);
                //objStream.Close();

                //objWRequest.Timeout = 20000;

                using (var streamWriter = new StreamWriter(request.GetRequestStream())) //전송
                {
                    string json1 = strPostData;
                    streamWriter.Write(json1);
                }

                var response = (HttpWebResponse)request.GetResponse(); //응답
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var apiResult = streamReader.ReadToEnd();
                    JObject jobj = JObject.Parse(apiResult);
                    //if (jobj["SCHEDULE"]["code"].ToString() == "00")
                    //{
                    //    //roomNumber = jobj["SCHEDULE"]["conferenceid"].ToString();
                    //}
                }


                //-----------------------------------------------------------------------------------------
                // Description : API 요청에 대한 성공/실패 여부 (오류코드) 
                //               HTTP StatusCode 200 OK 인 경우에만 요청 처리 성공이며, 성공이 아닌 경우에는 아래 StatusCode를 참고하시기 바랍니다.          
                //              - 401 : [998] Authentication token is missing or incorrect. (인증 오류)
                //              - 403 : [993] Yon do not have authorization. (인증 오류)
                //              - 405 : [995] 요청된 메소드는 권한이 없습니다. (POST / GET 등 메소드 오류)
                //              - 406 : [2000]~[5000] 오류 상세 메시지 (비즈니스 로직 처리중 오류 발생)
                //              - 500 : [999] Internal server error (System 오류)
                //-----------------------------------------------------------------------------------------

                // 요청 처리 성공인 경우 - 결제창 url 넘겨줌                                 
                // Response Parameters (성공시) : token, online_url, mobile_url
                //HttpWebResponse objWResponse = (HttpWebResponse)objWRequest.GetResponse();
                //logger.Error("check");
                //StreamReader objStreamReader = new StreamReader(objWResponse.GetResponseStream(), Encoding.GetEncoding("utf-8"));
                //string strResponse = objStreamReader.ReadToEnd();
                //JObject json = JObject.Parse(strResponse);

                //objStreamReader.Close();
                //objWResponse.Close();
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
    }
}