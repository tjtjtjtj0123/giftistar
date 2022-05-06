using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using test211005.Models;
using BOQv7Das_Net;

namespace test211005.Content
{
    public class LibraryDb : DbContext
    {
        public static SqlConnection con =
            new SqlConnection(ConfigurationManager.ConnectionStrings["DBCS"].ToString());

        public LibraryDb() : base("name=DBCS"){}

        public DbSet<ItemModel> Items { get; set; }

        //public static void InsertGifticon(string ItemName, int ItemRealPrice, int ItemSellingPrice, int CashRemain, DateTime DueDate, int ItemNo, int UserNo)
        public static void InsertGifticon()
        {
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            //cmd.CommandText = string.Format("Insert TB_USER values(nuy, '{1}', {2}, '{3}')", id, name, age, sex);

            cmd.CommandText = string.Format("Insert TGifticon(ItemName, ItemRealPrice, ItemSellingPrice) values('빙그레바나나맛우유', 1200, 1000)");

            cmd.CommandType = CommandType.Text;

            cmd.ExecuteNonQuery();

            con.Close();
        }

        public static DataSet ShowAllItem()
        {
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.CommandText = string.Format("Select * From TItem");

            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;

            DataSet ds = new DataSet();
            da.Fill(ds, "TItem");
            con.Close();

            return ds;
        }

        public static DataSet ShowTotalCash()
        {
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.CommandText = string.Format("Select SUM(TotalCash) AS total From TTotalCashMst");

            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;

            DataSet ds = new DataSet();
            da.Fill(ds, "TotalCash");
            con.Close();

            return ds;
        }

        internal static DataSet ShowItemDetailWithKey(string keyword)
        {
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.CommandText = string.Format("Select * From TItem WHERE ItemName LIKE '%{0}%'", keyword);

            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;

            DataSet ds = new DataSet();
            da.Fill(ds, "TItem");
            con.Close();

            return ds;
        }

        public static DataSet ShowGifticonList(int itemNo)
        {
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.CommandText = string.Format("Select * From TGifticon WHERE ItemNo = {0}", itemNo);

            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;

            DataSet ds = new DataSet();
            da.Fill(ds, "TGifticon");
            con.Close();

            return ds;
        }

        public static DataSet ShowItemDetail(int itemNo)
        {
            con.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            cmd.CommandText = string.Format("Select * From TItem WHERE ItemNo = {0}", itemNo);

            cmd.CommandType = CommandType.Text;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = cmd;

            DataSet ds = new DataSet();
            da.Fill(ds, "TItem");
            con.Close();

            return ds;
        }
    }
    
}