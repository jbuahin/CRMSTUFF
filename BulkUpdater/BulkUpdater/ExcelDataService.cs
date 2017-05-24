using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkUpdater
{
    public class InpuDataObject
    {
        public string LookupFieldId { get; set; }
    }
    public class ExcelDataService
    {
        OleDbConnection Conn;
        OleDbCommand Cmd;

        public ExcelDataService()
        {
            string ExcelFilePath = @"C:\Users\joshu\Documents\Visual Studio 2015\Projects\New folder\BulkUpdater\BulkUpdater\Files\ProgLeasing.xlsx";

            string excelConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + ExcelFilePath + ";Extended Properties=Excel 12.0;Persist Security Info=True";
            Conn = new OleDbConnection(excelConnectionString);
        }

        /// <summary>  
        /// Method to Get All the Records from Excel  
        /// </summary>  
        /// <returns></returns>  
        public async Task<ObservableCollection<InpuDataObject>> ReadRecordFromEXCELAsync()
        {
            ObservableCollection<InpuDataObject> InpuDataObjects = new ObservableCollection<InpuDataObject>();
            await Conn.OpenAsync();
            Cmd = new OleDbCommand();
            Cmd.Connection = Conn;
            Cmd.CommandText = "Select * from [Sheet1$]";
            var Reader = await Cmd.ExecuteReaderAsync();
            while (Reader.Read())
            {
                InpuDataObjects.Add(new InpuDataObject()
                {
                    LookupFieldId = Reader["LookupFieldId"].ToString()
                });
            }
            Reader.Close();
            Conn.Close();
            return InpuDataObjects;
        }

        /// <summary>  
        /// Method to Insert Record in the Excel  
        /// S1. If the EmpNo =0, then the Operation is Skipped.  
        /// S2. If the LookupField is already exist, then it is taken for Update  
        /// </summary>  
        /// <param name="Emp"></param>  
        public async Task<bool> ManageExcelRecordsAsync(InpuDataObject lookup)
        {
            bool IsSave = false;
            if (lookup.LookupFieldId != null)
            {
                await Conn.OpenAsync();
                Cmd = new OleDbCommand();
                Cmd.Connection = Conn;
                Cmd.Parameters.AddWithValue("@LookupFieldId", lookup.LookupFieldId);

                if (!IsLookupRecordExistAsync(lookup).Result)
                {
                    Cmd.CommandText = "Insert into [Sheet1$] values (@LookupFieldId)";
                }
                else
                {
                    Cmd.CommandText = "Update [Sheet1$] set LookupFieldId=@LookupFieldId where LookupFieldId=@LookupFieldId";

                }
                int result = await Cmd.ExecuteNonQueryAsync();
                if (result > 0)
                {
                    IsSave = true;
                }
                Conn.Close();
            }
            return IsSave;

        }
        /// <summary>  
        /// The method to check if the record is already available   
        /// in the workgroup  
        /// </summary>  
        /// <param name="emp"></param>  
        /// <returns></returns>  
        private async Task<bool> IsLookupRecordExistAsync(InpuDataObject lookup)
        {
            bool IsRecordExist = false;
            Cmd.CommandText = "Select * from [Sheet1$] where LookupFieldId=@LookupFieldId";
            var Reader = await Cmd.ExecuteReaderAsync();
            if (Reader.HasRows)
            {
                IsRecordExist = true;
            }

            Reader.Close();
            return IsRecordExist;
        }
    }
}
