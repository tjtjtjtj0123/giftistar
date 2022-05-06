using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using test211005.Content;
using test211005.Models;
using System.Web.Mvc.Routing;
using PayPal.Api;
using System.IO;

namespace test211005.Controllers
{
    public class ProductController : Controller
    {
        public int paging(string page, int totalRowCount, int innerRow) /*페이징 처리 함수*/
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

        public List<int> GifticonLst = new List<int>();

        // GET: Product/Index - 판매중인 상품 리스트 보기
        public ActionResult Index()
        {
            DASManager.UpdateGifticonStatus();
            int pageNum = Request.QueryString["page"] == null ? 0 : (int.Parse(Request.QueryString["page"].ToString()) - 1) * 10;
            int itemSellingFilter = Session["UserSession"] == null ? 0 : ((UserModel)Session["UserSession"]).UserNo;
            List<ItemModel> ds = DASManager.ShowAvailableItem(pageNum, 10, itemSellingFilter);
            double totalRowCount = ds[0].TotalRowCount;
            int totalPageCount = int.Parse(Math.Ceiling(totalRowCount / 10).ToString());
            Session["totalPageCount"] = totalPageCount;
            int page = Request.QueryString["page"] == null ? 1 : int.Parse(Request.QueryString["page"].ToString());
            ViewBag.Page = page;
            if (totalRowCount == 0)
                ds = new List<ItemModel>();
            return View(ds);
        }

        // GET: Product/ProductRegist - 판매 등록
        [HttpGet]
        public ActionResult ProductRegist()
        {
            if (Session["UserSession"] == null)
            {
                ReturnUrl m = new ReturnUrl();
                m.ReturnAct = "ProductRegist";
                m.ReturnCon = "Product";
                m.ItemNo = 0;
                Session["ReturnUrl"] = m;
                return RedirectToAction("Login", "Account");
            }
            GifticonModel g = new GifticonModel();
            g.DueDate = DateTime.Now;
            return View(g);
        }

        [HttpPost]
        public ActionResult ProductRegist(GifticonModel m, HttpPostedFileBase imgfile)
        {
            /*판매 등록한 기프티콘 정보*/
            ItemModel i = (ItemModel) Session["ItemSession"];

            m.ItemNo = i.ItemNo;
            m.ItemName = i.ItemName;
            m.ItemRealPrice = i.ItemRealPrice;
            m.ItemProviderName = i.ItemProviderName;
            UserModel u = (UserModel)Session["UserSession"];
            m.UserNo = u.UserNo;
            m.ItemSellingPrice = int.Parse(m.ItemSellingPriceStr);

            /*업로드한 상품 DB에 저장*/
            string path = uploadimage(imgfile);
            if (path != "-1") // 선택된 파일 有
                m.imgfile = path;
            else // 선택된 파일 無
                return View("ProductRegist", m);

            Response.Write(@"<script language='javascript'>alert('상품 등록에 성공하였습니다.');</script>");
            DASManager.InsertGifticonInfo(m);
            Session.Remove("ItemSesion");
            return View("ProductRegistSuc", m);
        }

        /*이미지 DB에 저장해주는 부분*/
        public string uploadimage(HttpPostedFileBase file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0){
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png")) {
                    try {
                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);
                        //    ViewBag.Message = "File uploaded successfully";
                    }catch (Exception ex) {
                        path = "-1";
                    }
                }else { // 파일이 확장자가 맞지 않는 경우
                    Response.Write("<script>alert('jpg, jpeg, png 확장자 파일만 선택이 가능합니다.'); </script>");
                }
            }else {
                Response.Write("<script>alert('파일을 선택해주세요.'); </script>");
                path = "-1";
            }
            return path;
        }

        // GET: Product/ProductRegistSuc - 상품 판매 등록 성공
        public ActionResult ProductRegistSuc(GifticonModel m)
        {            
            return View(m);
        }

        /////* Ajax-GET: Product/ProductRegist - 아이템 리스트 출력*/
        public JsonResult SearchKeyword(string keyword) // 키워드로 아이템 검색
        {
            DASManager.UpdateGifticonStatus();
            int pageNum = Request.QueryString["page"] == null ? 0 : (int.Parse(Request.QueryString["page"].ToString()) - 1) * 5;
            List<ItemModel> ds = DASManager.ShowItemByKeyWord(keyword, pageNum, 5);
            string json = JsonConvert.SerializeObject(ds.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Product/Index - 필터링된 아이템 리스트 출력*/
        public JsonResult SearchProductByType(int categoryType, string brandType, int costLineType) // 키워드로 아이템 검색
        {
            DASManager.UpdateGifticonStatus();
            int pageNum = Request.QueryString["page"] == null ? 0 : (int.Parse(Request.QueryString["page"].ToString()) - 1) * 10;
            int UserNo = Session["UserSession"] == null ? 0 : ((UserModel)Session["UserSession"]).UserNo;
            List<ItemModel> ds = DASManager.ShowItemByType(categoryType, brandType, costLineType, pageNum, 10, UserNo);

            string json = JsonConvert.SerializeObject(ds.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        /////* Ajax-GET: Product/Index - 아이템 리스트 출력*/
        public JsonResult ChooseProduct(int itemNo)
        {
            DASManager.UpdateGifticonStatus();
            ItemModel i = DASManager.ShowItemDetail(itemNo);
            Session["ItemSession"] = i;
            List<ItemModel> ds = new List<ItemModel>();
            ds.Add(i);
            //List<ItemModel> ds = DASManager.ShowAllItem();
            string json = JsonConvert.SerializeObject(ds, Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: Product/ProductDetail - 상품 상세 보기
        public ActionResult ProductDetail(int id)
        {
            DASManager.UpdateGifticonStatus();
            ItemModel ds = DASManager.ShowItemDetail(id);
            ds.UserNo = Session["UserSession"] == null ? 0 : ((UserModel)Session["UserSession"]).UserNo;
            ds.Gifticons = DASManager.ShowAvailableGifticonList(id, -1, 0, 0);

            return View(ds);
        }

        //GET: Product/ProductPurchase - 상품 구매
        public ActionResult ProductPurchase(int id)
        {
            DASManager.UpdateGifticonStatus();
            if (Session["UserSession"] == null)
            {
                ReturnUrl m = new ReturnUrl();
                m.ReturnAct = "ProductPurchase";
                m.ReturnCon = "Product";
                m.ItemNo = id;
                Session["ReturnUrl"] = m;
                return RedirectToAction("Login", "Account");
            }
            ItemModel im = DASManager.ShowItemDetail(id); // Item 정보
            UserModel um = (UserModel) Session["UserSession"];
            List<GifticonModel> gm = DASManager.ShowAvailableGifticonList(id, -1, 0, um.UserNo); // 해당 Item에 대한 기프티콘 정보
            if (gm.Count == 0)
            {
                return RedirectToAction("Index", "Product", new { ac = "success" });
            }
            int totalAsset = DASManager.ShowTotalCash(um.UserNo); // 전 캐시 잔액 정보
            PurchaseModel p = new PurchaseModel();

            p.Item = im;

            p.StockAmount = gm.Count;
            p.Gifticons = gm;
            p.TotalAsset = totalAsset;

            return View(p);
        }

        /////* Ajax-GET: Product/ProductPresent - 수신인 존재 여부 확인*/
        //public JsonResult CheckIsExist(string KeyWord) // 키워드로 사용자 검색
        //{
        //    List<UserModel> ds = DASManager.ShowUserByKeyWord(KeyWord);
        //    string json = JsonConvert.SerializeObject(ds.ToArray(), Formatting.Indented);
        //    return Json(json, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult CheckIsExist(string UserId) // 키워드로 아이템 검색
        {
            UserModel um = DASManager.ShowUserDetail(UserId);
            List<UserModel> ds = new List<UserModel>();
            ds.Add(um);
            string json = JsonConvert.SerializeObject(ds.ToArray(), Formatting.Indented);
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        // GET: Product/ProductPurchaseSuc - 상품 구매 성공
        public ActionResult ProductPurchaseSuc(int itemNo, int total, string gifticonNos)
        {
            PurchaseModel p = new PurchaseModel();
            p.Item = DASManager.ShowItemDetail(itemNo); // Item 정보
            p.TotalPrice = total;

            string[] c = gifticonNos.Split(',');
            int count = 0;
            foreach (string ch in c)
                if (ch != "") // 클릭시, 공백이 들어올 수도 있어서 실 gifticonNo만 걸러서 저장
                {
                    GifticonLst.Add(int.Parse(ch));
                    count++;
                }

            p.PurchaseAmount = count;
            p.User = (UserModel) Session["UserSession"];

            /*리스트 형식의 orderList로 넘겨줘야하므로 배열 string으로 변경 예)40,50,*/
            string str = "";
            for (int i = 0; i < c.Length; i++)
                str += c[i] + ",";

            if (str.Substring(0, 1) == ",")
                str = str.Substring(1);

            DASManager.InsertPurchaseInfo(p, str); // 구매 정보 등록
            return View(p);
        }

        // GET: Product/ProductPresent - 상품 선물
        public ActionResult ProductPresent(int id)
        {
            if (Session["UserSession"] == null)
            {
                ReturnUrl m = new ReturnUrl();
                m.ReturnAct = "ProductPresent";
                m.ReturnCon = "Product";
                m.ItemNo = id;
                Session["ReturnUrl"] = m;
                return RedirectToAction("Login", "Account");
            }
            ItemModel im = DASManager.ShowItemDetail(id); // Item 정보
            UserModel um = (UserModel)Session["UserSession"]; // User Session 정보
            List<GifticonModel> gm = DASManager.ShowAvailableGifticonList(id, -1, 0, um.UserNo); // 해당 Item에 대한 기프티콘 정보
            if (gm.Count == 0)
            {
                return RedirectToAction("Index", "Product", new { ac = "success" });
            }

            int totalAsset = DASManager.ShowTotalCash(um.UserNo); // 전 캐시 잔액 정보
            PurchaseModel p = new PurchaseModel();

            p.Item = im;
            p.User = um;
            p.StockAmount = gm.Count;
            p.Gifticons = gm;
            p.TotalAsset = totalAsset;

            return View(p);
        }

        // GET: Product/ProductPresentSuc - 상품 선물 성공
        public ActionResult ProductPresentSuc(int itemNo, int total, string gifticonNos, string recepientId)
        {
            PurchaseModel p = new PurchaseModel();
            p.Item = DASManager.ShowItemDetail(itemNo); // Item 정보
            p.TotalPrice = total;

            string[] c = gifticonNos.Split(',');
            int count = 0;
            foreach(string ch in c)
                if (ch != "") // 클릭시, 공백이 들어올 수도 있어서 실 gifticonNo만 걸러서 저장
                    count++;

            p.PurchaseAmount = count;
            p.User = (UserModel)Session["UserSession"];

            /*리스트 형식의 orderList로 넘겨줘야하므로 배열 string으로 변경 예)40,50,*/
            string str = "";
            for (int i = 0; i < c.Length; i++)
                str += c[i] + ",";

            if (str.Substring(0, 1) == ",")
                str = str.Substring(1);

            p.RecepientId = recepientId;
            UserModel um = DASManager.ShowUserDetail(recepientId);
            p.RecepientNo = um.UserNo;

            DASManager.InsertPurchaseInfo(p, str); // 구매 정보 등록
            return View(p);
        }
    }
}