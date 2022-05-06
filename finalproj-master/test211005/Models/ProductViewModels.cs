using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace test211005.Models
{
    public class ItemModel
    {
        [Key]
        public int ItemNo { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public int ItemRealPrice { get; set; }
        public int ItemLowestPrice { get; set; }

        public string ItemProviderName { get; set; }
        public int BiggestDiscountPercent { get; set; }
        public int Status { get; set; }
        public DateTime RegDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public int CategoryNo { get; set; }
        public List<GifticonModel> Gifticons { get; set; }
        public int TotalRowCount { get; set; }
        public int UserNo { get; set; }
    }

    public class PurchaseModel
    {
        public ItemModel Item { get; set; }
        public UserModel User { get; set; }
        public int TotalPrice { get; set; }
        public int TotalAsset { get; set; }
        public int PurchaseAmount { get; set; }

        public int StockAmount { get; set; }
        public string RecepientId { get; set; }
        public int RecepientNo { get; set; }
        public List<GifticonModel> Gifticons { get; set; }
        // public List<int> ChkBox { get; set; }
    }

    public class GifticonModel
    {
        public int ItemNo { get; set; }
        public int UserNo { get; set; }
        public string ItemName { get; set; }
        public int ItemRealPrice { get; set; }
        public bool IsChecked { get; set; }

        public int GifticonNo { get; set; }
        public int DiscountPercent { get; set; }
        public DateTime DueDate { get; set; }
        public string DueDateStr { get; set; }
        public int ItemSellingPrice { get; set; }

        public string SellingStatus { get; set; }
        public string RegDate { get; set; }
        public string SoldDate { get; set; }
        public int CashRemain { get; set; }
        public string GifticonStatus { get; set; }

        public string ItemProviderName { get; set; }
        public int BuyerNo { get; set; }
        public string BuyerId { get; set; }
        public int IsPresent { get; set; }
        public string UserId { get; set; }

        public string imgfile { get; set; }
        public int TotalPageCount { get; set; }
        public string ItemSellingPriceStr { get; set; }
        public int PurchaseStatus { get; set; } // 기프티콘 구매 상태(구매완료 : 0 / 판매중 : 1 / 구매확정 전 : 2)
    }

    public class ImageModel
    {
        [Key]
        public int ImageId { get; set; }
        public string Title { get; set; }
        public string ImageName { get; set; }
    }
}