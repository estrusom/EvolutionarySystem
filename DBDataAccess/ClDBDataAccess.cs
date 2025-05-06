using ClEvSy;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DBDataAccess
{
    public static class clDBDataAccess
    {
        const string SELECT = "SELECT";
        const string FROM = "FROM";
        private static EvSyDb _evSyDb = new EvSyDb();
        private static List<EvSyDb> evSySb = new List<EvSyDb>();
        private static List<string> realFieldList = new List<string>();

        private static SQLiteConnection Connection;
        private static List<string> Dataset = new List<string>();
        public static void ConnectionDb(string ConnetcionString)
        {
            Connection = new SQLiteConnection(string.Format("Data Source={0}", ConnetcionString));
            Connection.Open();
            columnsNameRealDB();
        }
        public static void SqlQuerySelect(string SqlString)
        {
            int init = SqlString.ToUpper().IndexOf(SELECT) + SELECT.Length;
            int fine = SqlString.ToUpper().IndexOf(FROM);
            string subString = SqlString.Substring(init, fine - init).Trim();
            string[] fieldSql = subString.Split(',');
            SQLiteCommand cmdSelectRecord = new SQLiteCommand()
            {
                Connection = Connection,
                CommandType = CommandType.Text,
                CommandText = SqlString
            };
            object[] oRecord = new object[fieldSql.Count()];
            SQLiteDataReader record = cmdSelectRecord.ExecuteReader();
            if (record.HasRows)
            {
                while (record.Read())
                {
                    EvSyDb lEvSyDb = new EvSyDb();
                    Type EvSyDbTy = lEvSyDb.GetType();
                    string s = "({0});";
                    int recRead = record.GetValues(oRecord);
                    for (int i = 0; (i < fieldSql.Count()); i++)
                    {
                        var pInfo = EvSyDbTy.GetProperties().Where(P => P.Name.ToUpper() == realFieldList[i].ToUpper());
                        if (pInfo.Any())
                        {
                            Type pTy = pInfo.First().PropertyType;
                            switch (pTy.Name)
                            {
                                case "Int32":
                                    {
                                        if (int.TryParse(oRecord[i].ToString().Trim(), out int retInt))
                                        {
                                            pInfo.First().SetValue(lEvSyDb, retInt);
                                        }
                                    }
                                    break;
                                case "Int64":
                                    {
                                        if (Int64.TryParse(oRecord[i].ToString().Trim(), out Int64 retInt))
                                        {
                                            pInfo.First().SetValue(lEvSyDb, retInt);
                                        }
                                    }
                                    break;
                                case "String":
                                    {
                                        pInfo.First().SetValue(lEvSyDb, oRecord[i].ToString().Trim());
                                    }
                                    break;
                                case "Object":
                                    {
                                        pInfo.First().SetValue(lEvSyDb, oRecord[i]);
                                    }
                                    break;
                                case "Boolean":
                                    {
                                        pInfo.First().SetValue(lEvSyDb, Convert.ToBoolean(oRecord[i]));
                                    }
                                    break;
                                default:
                                    {
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    evSySb.Add(lEvSyDb);
                    EvSyDbFnc.AddXml(EvSyDbFnc.SerializationEvSyDb(lEvSyDb));
                }
            }
        }
        public static List<EvSyDb> GetDbRecord()
        {
            return evSySb;
        }
        #region private methods
        /// <summary>
        /// Nomi dei campi freali della tabella reale
        /// </summary>
        private static void columnsNameRealDB()
        {
            SQLiteCommand command = new SQLiteCommand()
            {
                Connection = Connection,
                CommandType = CommandType.Text,
                CommandText = "select * from (select * from sqlite_master where type='table')where tbl_name = 'FieldsRegistry'"
            };
            SQLiteDataReader reader = command.ExecuteReader();
            object[] ob = new object[5];
            if (reader.HasRows)
            {
                bool primaryFound = false;
                while (reader.Read())
                {
                    //studiare come si aggiunge un campo
                    int i = reader.GetValues(ob);
                    int ini = ob[4].ToString().IndexOf('(');
                    int end = ob[4].ToString().LastIndexOf(')');
                    string[] fields = ob[4].ToString().Substring(ini + 1, end - ini - 1).Trim().Split(',');
                    for (int j = 0; j < fields.Count(); j++)
                    {
                        if (!fields[j].Contains("PRIMARY") && !primaryFound)
                        {
                            string s1 = fields[j].Trim();
                            fields[j] = s1.Substring(0, s1.IndexOf(' '));
                        }
                        else
                        {
                            primaryFound = true;
                            fields[j] = "";
                        }
                    }
                    realFieldList = fields.ToList();
                    var v = realFieldList.Where(F => F.Equals(""));
                    if (v.Any())
                    {
                        for (int j = 0; j <= v.Count(); j++)
                        {
                            realFieldList.Remove("");
                        }
                    }
                    realFieldList.Remove("");
                }
            }
        }
        #endregion
    }
}
