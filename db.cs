/*
  Mesut Cemil Aslan ve Bahadır AKKÖY
  mca ve abahad
  2016 
 */
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace mca
{
    public class vt
    {
        public static string sondankirp(string text, int lenght = 1)
        {
            if (text.Length > 1)
            {
                return text.Substring(0, (text.Length - lenght));
            }
            else
            {
                return text;
            }
        }
        public static NpgsqlConnection Conn()
        {
            NpgsqlConnection Conn = new NpgsqlConnection(PROJE.Startup.StaticConfig.GetConnectionString("DefaultConnection"));
            Conn.Open();
            return Conn;
        }
        public static int cmd(string sqlcumle)
        {
            NpgsqlConnection Conn = vt.Conn();
            NpgsqlCommand Cmd = new NpgsqlCommand(sqlcumle, Conn);
            int sonuc = 0;
            try
            {
                sonuc = Cmd.ExecuteNonQuery();
                //sonuc = Convert.ToInt16(Cmd.LastInsertedId);
            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message + " (" + sqlcumle + ")");
            }
            Cmd.Dispose();
            Conn.Close();
            Conn.Dispose();
            return (sonuc);
        }
        public class parameter
        {
            public enum command
            {
                insert = 0,
                update = 1
            }
            private string _field;
            private object _value;
            public string field
            {
                get { return _field; }
                set { _field = value; }
            }
            public object value
            {
                get { return _value; }
                set { _value = value; }
            }
            public parameter(string field, object value)
            {
                _field = field;
                _value = value;
            }
        }
        public static int cmd(vt.parameter.command komut, string tablo, List<vt.parameter> parameters, vt.parameter where = null)
        {
            // kullanim örneği
            //List<vt.parameter> degerler = new List<vt.parameter>();
            //degerler.Add(new vt.parameter("label", "mesut"));
            //degerler.Add(new vt.parameter("value", "test"));
            //degerler.Add(new vt.parameter("metin", "buraya'da  yazalım."));
            //degerler.Add(new vt.parameter("tarih", DateTime.Now));
            //vt.cmd(vt.parameter.command.insert, "test", degerler);

            int sonuc = 0;
            switch (komut)
            {
                case vt.parameter.command.insert:
                    string sql = "INSERT INTO " + tablo + "({__MCA_KOLONLAR__}) VALUES ({__MCA_DEGERLER__});";
                    string kolonlar = "";
                    string degerler = "";
                    foreach (vt.parameter p in parameters)
                    {
                        kolonlar += p.field + ",";
                        degerler += "@" + p.field + ",";
                    }
                    kolonlar = sondankirp(kolonlar);
                    degerler = sondankirp(degerler);
                    sql = sql.Replace("{__MCA_KOLONLAR__}", kolonlar).Replace("{__MCA_DEGERLER__}", degerler);
                    NpgsqlConnection Conn = vt.Conn();
                    NpgsqlCommand Cmd = new NpgsqlCommand(sql, Conn);
                    Cmd.Parameters.Clear();
                    foreach (vt.parameter p in parameters)
                    {
                        Cmd.Parameters.AddWithValue("@" + p.field, p.value);
                    }
                    try
                    {
                        sonuc = Cmd.ExecuteNonQuery();
                        //sonuc = Convert.ToInt16(Cmd.LastInsertedId);
                    }
                    catch (NpgsqlException ex)
                    {
                        throw new Exception(ex.Message + " (" + sql + ")");
                    }
                    Cmd.Dispose();
                    Conn.Close();
                    Conn.Dispose();
                    break;
                case vt.parameter.command.update:
                    string usql = "UPDATE " + tablo + " SET {__MCA_UPDATEPARAM__}";
                    if (where != null)
                    {
                        usql += " WHERE " + where.field + "=@W__" + where.field;
                    }
                    string uparam = "";
                    foreach (vt.parameter p in parameters)
                    {
                        uparam += p.field + "=" + "@" + p.field + ",";
                    }
                    uparam = sondankirp(uparam);
                    usql = usql.Replace("{__MCA_UPDATEPARAM__}", uparam);
                    NpgsqlConnection uConn = vt.Conn();
                    NpgsqlCommand uCmd = new NpgsqlCommand(usql, uConn);
                    uCmd.Parameters.Clear();
                    foreach (vt.parameter p in parameters)
                    {
                        uCmd.Parameters.AddWithValue("@" + p.field, p.value);
                    }
                    if (where != null)
                    {
                        uCmd.Parameters.AddWithValue("@W__" + where.field, where.value);
                    }
                    try
                    {
                        sonuc = uCmd.ExecuteNonQuery();
                    }
                    catch (NpgsqlException ex)
                    {
                        throw new Exception(ex.Message + " (" + usql + ")");
                    }
                    uCmd.Dispose();
                    uConn.Close();
                    uConn.Dispose();
                    break;
            }
            return sonuc;
        }
        public static DataTable GetDataTable(string sql)
        {
            NpgsqlConnection Conn = vt.Conn();
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql, Conn);
            DataTable dt = new DataTable();
            try
            {
                adapter.Fill(dt);
            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message + " (" + sql + ")");
            }
            adapter.Dispose();
            Conn.Close();
            Conn.Dispose();
            return dt;
        }
        public static DataSet GetDataSet(string sql)
        {
            NpgsqlConnection Conn = vt.Conn();
            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql, Conn);
            DataSet ds = new DataSet();
            try
            {
                adapter.Fill(ds);
            }
            catch (NpgsqlException ex)
            {
                throw new Exception(ex.Message + " (" + sql + ")");
            }
            adapter.Dispose();
            Conn.Close();
            Conn.Dispose();
            return ds;
        }
        public static DataRow GetDataRow(string sql)
        {
            DataTable table = GetDataTable(sql);
            if (table.Rows.Count == 0) return null;
            return table.Rows[0];
        }
        public static string GetDataCell(string sql)
        {
            DataTable table = GetDataTable(sql);
            if (table.Rows.Count == 0) return null;
            return table.Rows[0][0].ToString();
        }
        public static int GetCount(string strSQL)
        {
            DataSet ds = GetDataSet(strSQL);
            return ds.Tables[0].Rows.Count;
        }
    }
}
