using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace test211005.Models
{
    public class MyPageViewModel
    {
        public UserModel User { get; set; }
        public int TotalCash { get; set; }
        public TotalCashModel GiftiCash { get; set; } // 충전 캐시
        public TotalCashModel BonusCash { get; set; } // 보너스 캐시
        public TotalCashModel SellingCash { get; set; } // 판매금 캐시

        public List<ChargeModel> ChargingList { get; set; } // 충전 내역
        public int PageCount { get; set; } // 페이지 수
    }

    public class ChargeModel
    {
        // 충전 번호
        public int ChargeNo { get; set; }
        // 사용자 번호
        public int UserNo { get; set; }
        // 사용자 ID
        public string UserId { get; set; }
        // 충전수단
        public string Payment { get; set; }
        // 결제 금액
        public string ChargePaidAmount { get; set; }
        
        // 캐시 충전 금액
        public int ChargeCashAmount { get; set; }
        // 잔액
        public int CashRemain { get; set; }
        // 결제상태
        public string CashStatus { get; set; }
        // 충전일시
        public string RegDate { get; set; }
        // 취소일시
        public string RefundDate { get; set; }

        // 캐시 만료일
        public string CashDueDate { get; set; }
        // 사용자 이름
        public string UserName { get; set; }
        // 회수 가능 여부 확인
        public int RefundStatus { get; set; }
        // 취소 가능 여부 확인
        public int ReTrieveStatus { get; set; }
        // 캐시 속성 번호
        public int CashPropertyNo { get; set; }

        public int TotalPageCount { get; set; }

    }

    public class TotalCashModel
    {
        public int CashType { get; set; }
        public int TotalCash { get; set; }
        public int TotalIncomeCash { get; set; }
        public int TotalWithdrawCash { get; set; }
    }

    public class ChargeViewModel
    {
        /*
         * 충전 수단
         * 충전 금액
         * 필수 동의항목1 체크
         * 필수 동의항목2 체크
         */
        public int Payment { get; set; }
        public string PaymentStr { get; set; }
        public int TotalCash { get; set; }
        public int ChargedTotalCash { get; set; }
        public int ChargeAmount { get; set; }

        public int ChargePaidAmount { get; set; }
        public string ChargePaidAmountStr { get; set; }
        public int ChargeCashAmount { get; set; }
        [Required(ErrorMessage = "필수동의항목을 체크해주세요.")]
        public bool AgreementCheck1 { get; set; }
        [Required(ErrorMessage = "필수동의항목을 체크해주세요.")]
        public bool AgreementCheck2 { get; set; }

        public DateTime RegDate { get; set; }
        public int OrderNo { get; set; }
        public UserModel User { get; set; }
        public string RegDateStr { get; set; }
        public int BackItemNo { get; set; }
    }

    public class PurchaseDtlModel
    {
        public int PurchaseNo { get; set; }
        public string UserId { get; set; }
        public string ItemName { get; set; }
        public string ItemProviderName { get; set; }
        public int PurchaseAmount { get; set; }

        public int TotalPrice { get; set; }
        public int PresentStatus { get; set; }
        public string PurchaseStatusStr { get; set; }
        public string RecepientId { get; set; }
        public int RecepientNo { get; set; }

        public int PurchaseStatus { get; set; }
        public string RefundDate { get; set; }
        public string RegDate { get; set; }
        public int UserNo { get; set; }
        public int ItemNo { get; set; }

        public int ItemRealPrice { get; set; }
        public List<GifticonModel> Gifticons { get; set; }
        public int TotalPageCount { get; set; }
    }

    public class BlackUserModel
    {
        public UserModel User { get; set; }
        public string DeniedMsg { get; set; }
    }

    public class HoldingItemModel
    {
        public string ItemName { get; set; }
        public string ItemProviderName { get; set; }
        public int HoldingItemAmount { get; set; }
        public int AvailableItemAmount { get; set; }
        public int ItemNo { get; set; }

        public int HoldingItemNo { get; set; }
        public int ItemRealPrice { get; set; }
        public List<GifticonModel> Gifticons { get; set; }
        public int TotalRowCount { get; set; }
    }

    public class PayResult
    {
        public string user_id;
        public string user_name;
        public string order_no;
        public string service_name;
        public string product_name;

        public string custom_parameter;
        public string tid;
        public string cid;
        public double amount;
        public string pay_info;

        public string pgcode;
        public string billkey;
        public string domestic_flag;
        public string transaction_date;
        public string card_info;

        public string payhash;
        public string installmonth;
        public string nonsettleamt;
        public CashReceipt cash_receipt;
    }

    public class CashReceipt
    {
        public string cid;
        public string code;
        public string deal_no;
        public string issue_type;
        public string message;

        public string payer_sid;
        public string type;
    }

    /*출금 신청 모델*/
    public class WithdrawModel
    {
        // 현 보유 캐시 총액
        public int TotalCash { get; set; }
        // 출금할 캐시 금액
        public int WithdrawAmount { get; set; }
        // 계좌번호
        public string AccountNo { get; set; }
    }

}