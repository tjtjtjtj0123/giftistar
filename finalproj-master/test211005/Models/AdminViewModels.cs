using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.DynamicData;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace test211005.Models
{
    public class FilterModel
    {
        public string FUserId { get; set; }
        public int FPayment { get; set; }
        public IEnumerable<SelectListItem> FStatus
        {
            get;
            set;
        }
        public string FItemName { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime FStartDate { get; set; }
        public DateTime FEndDate { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int RefundPrice { get; set; }
        public string RefundMsg { get; set; }
    }

    /*전체 결제 내역*/
    public class ShowAllChargeViewModel
    {
        public List<ChargeModel> ChargingList { get; set; } // 충전 내역
    }

    // 구매 내역
    public class ShowAllPurchaseViewModel:FilterModel
    {

        // 구매번호
        public int PurchaseNo { get; set; }
        // 고객번호
        public int UserNo { get; set; }
        // 아이디
        public string UserId { get; set; }
        // 상품명
        public string ItemName { get; set; } 
        // 수량
        public int Amount { get; set; }
        // 캐시 금액
        public int TotalPrice { get; set; }
        // 구매일시
        public DateTime RegDate { get; set; }
        // 취소일시
        public DateTime RefundDate { get; set; }
        // 상태
        public int Status { get; set; }
    }

    // 판매 내역
    public class ShowAllSellingViewModel : FilterModel
    {
        // 판매번호
        public int PurchaseNo { get; set; }
        // 고객번호
        public int UserNo { get; set; }
        // 아이디(판매자)
        public int UserId { get; set; }
        // 상품명
        public string ItemName { get; set; }
        // 수량
        public int Amount { get; set; }
        // 캐시 금액
        public int ItemSellinglPrice { get; set; }
        // 잔여 캐시 금액
        public int CashRemain { get; set; }
        // 등록일시
        public DateTime RegDate { get; set; }
        // 판매일시
        public DateTime RefundDate { get; set; }
        // 상태
        public int Status { get; set; }
    }

    //계좌상태
    public class AccountStatus
    {
        /*잔액*/
        // 총 기프티캐시
        public int CashLeft { get; set; }
        // 충전된 기프티캐시
        public int GiftiCashLeft { get; set; }
        // 보너스 기프티캐시
        public int BonusCashLeft { get; set; }
        // 판매금 기프티캐시
        public int SellingIncomeCashLeft { get; set; }

        /*누계*/
        // 충전된 기프티캐시
        public int TotalGiftiCash { get; set; }
        // 충전된 보너스 기프티캐시
        public int TotalBonusCash { get; set; }
        // 충전된 판매금 기프티캐시
        public int TotalSellingIcomeCash { get; set; }

        // 사용한 기프티캐시
        public int TotalUsedGiftiCash { get; set; }
        // 사용한 보너스 기프티캐시
        public int TotalUsedBonusCash { get; set; }
        // 사용한 판매금 기프티캐시
        public int TotalUsedSellingIncomeCash { get; set; }
    }
        
    /*충전내역*/
    public class ChargeList
    {
        // 번호
        public int OrderNo { get; set; }
        // 충전 수단
        public int Payment { get; set; }
        // 결제 금액
        public int ChargePaidAmount { get; set; }
        // 충전 캐시 금액
        public int ChargeCashAmount { get; set; }
        // 남은 캐시 금액
        public int ChargeRemain { get; set; }
        // 충전 일시
        public DateTime chargeDate { get; set; }
        // 취소일시
        public DateTime RefundDate { get; set; }
        // 상태
        public int Status { get; set; }
    }

    /*전체내역*/
    public class ShowAllList
    {
        // 일시
        public DateTime Date { get; set; }
        // 충전수단
        public int Payment { get; set; }
        // 결제 금액
        public int ChargePaidAmount { get; set; }
        // 충전 캐시 금액
        public int ChargeCashAmount { get; set; }
        // 상품명
        public string ItemName { get; set; }
        // 아이템 가격
        public int ItemSellingPrice { get; set; }
        // 상태
        public int Status { get; set; }
        // 환불 사유
        public string RefunsMsg { get; set; }
    }

    /*PG로그내역*/
    public class PGLogList
    {
        // 주문번호
        public int OrderNo { get; set; }
        // 결제수단
        public int Payment { get; set; }
        // 승인결과
        public int ApprovalStatus { get; set; }
        // 등록일시
        public DateTime RegDate { get; set; }
        // tid
        public string Tid { get; set; }

        // 결제 금액
        public int ChargePaidAmount { get; set; }
        public int UserNo { get; set; }
        public int ChargeCashAmount { get; set; }
        public string PGName { get; set; }
        public string RegDateStr { get; set; }
    }

    /*전체 내역 조회*/
    public class AllUserListModel
    {
        public string RegDate { get; set; }
        public string Payment { get; set; }
        public string ChargePaidAmount { get; set; }
        public string ChargeCashAmount { get; set; }
        public string ItemName { get; set; }

        public string TotalPrice { get; set; }
        public string Status { get; set; }
        public int TotalRowCount { get; set; }
    }

    /*PG 로그 내역 조회*/
    public class PGLogModel
    {
        public int OrderNo { get; set; }
        public int Payment { get; set; }
        public string PaymentStr { get; set; }
        public string ApprovalStatus { get; set; }
        public string RegDate { get; set; }

        public int TotalRowCount { get; set; }
        public int ChargeCashAmount { get; set; }
        public string ChargePaidAmountStr { get; set; }
        public int ChargePaidAmount { get; set; }
        public string PGName { get; set; }
        public int UserNo { get; set; }
    }
}