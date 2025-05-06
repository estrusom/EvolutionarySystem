using ClEvSy;
using DBDataAccess;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace EvolutiveSystem
{
    internal class Program
    {
        const string CONNECTION_STRING = @"C:\Progetti\Scrigno\DNAC\DNAC-01.db";
        [STAThread]
        static void Main(string[] args)
        {
            string function = "DB_FORM";
            switch (function)
            {
                case "READ_FROM_DB":
                    {
                        EvSyDb localEvSyDb = new EvSyDb();
                        clDBDataAccess.ConnectionDb(CONNECTION_STRING);
                        string sqlString = "SELECT ID, TableName, KEY, FieldName, DataType, Value, EncryptedField, Registry FROM FieldsRegistry ORDER BY KEY, ID";
                        clDBDataAccess.SqlQuerySelect(sqlString);
                        List<EvSyDb> db = clDBDataAccess.GetDbRecord();
                        List<string> xmlList = EvSyDbFnc.GetXmlList(); 
                    }
                    break;
                case "CREA_RECORD":
                    {
                        EvSyDb myDb = new EvSyDb()
                        {
                            TableName = "DNAC_REGISTRY",
                            Key = "12345",
                            FieldName = "ID_FIlE",
                            DataType = "System.String",
                            Value = "12345",
                            Registry = 0,
                            Id = 0,
                            EncryptedField = false
                        };
                        string xml = EvSyDbFnc.SerializationEvSyDb(myDb);
                        Console.WriteLine(xml);
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xml);
                        EvSyDb evsd = EvSyDbFnc.DeserializeEvSyDb(xmlDoc);
                    }
                    break;
                case "LIST_DB":
                    {
                        EvSyDb localEvSyDb = new EvSyDb();
                        clDBDataAccess.ConnectionDb(CONNECTION_STRING);
                        string sqlString = "SELECT ID, TableName, KEY, FieldName, DataType, Value, EncryptedField, Registry FROM FieldsRegistry ORDER BY KEY, ID";
                        clDBDataAccess.SqlQuerySelect(sqlString);
                        List<EvSyDb> db = clDBDataAccess.GetDbRecord();

                        EvDbRec recList = new EvDbRec();
                        
                        foreach (EvSyDb dbRec in db)
                        {
                            recList.Add(dbRec);
                        }
                        string xml = EvSyDbFnc.SerializzationEvDbRec(recList);
                        Console.WriteLine(xml);
                        Console.ReadLine();
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xml);
                        EvDbRec evSyDbXml = EvSyDbFnc.DeserializeEvDbRec(xmlDoc);
                        var key = evSyDbXml.Where(K => K.EncryptedField == true);
                        if (key.Any())
                        {
                            Console.WriteLine(key.First().Value);
                        }
                    }
                    break;
                case "DB_FORM":
                    {
                        Application.Run(new FrmTestEvolutiveDB());
                    }
                    break;
            }
            //Console.ReadLine();
        }
    }
}
