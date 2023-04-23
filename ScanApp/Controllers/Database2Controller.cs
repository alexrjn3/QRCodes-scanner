using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using ScanApp.Models;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.Emit;
using System.Text;


namespace ScanApp.Controllers
{
    public class Database2Controller : Controller
    {
        //DB Connection
        SqlConnection con = new SqlConnection("Server = localhost\\SQLEXPRESS; Database= test_WEB2; Integrated Security = True;");


        //Add data in db
        public string AddCode(CodeModel codeModel)
        {
            try
            {
                //DateTime localDate = DateTime.Now;
                String str = codeModel.Content;
                char[] separator = { ';' };
                String[] strlist = str.Split(separator);

                SqlCommand cmd = new SqlCommand("InsertMaterial", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Material", strlist[0]);
                cmd.Parameters.AddWithValue("@Descriere", strlist[1]);
                cmd.Parameters.AddWithValue("@Cantitate", strlist[2]);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                return ("Data save Successfully");
            }
            catch (Exception ex)
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                return (ex.Message.ToString());
            }
        }


        //Take data from db
        public static List<T> LoadData<T>(string sql)
        {

            using (SqlConnection cnn = new SqlConnection("Server = localhost\\SQLEXPRESS; Database= test_WEB2; Integrated Security = True;"))
            {
                return cnn.Query<T>(sql).ToList();
            }
        }

    }
}
