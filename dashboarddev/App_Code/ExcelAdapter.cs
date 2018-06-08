using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Xml;
using System.IO;

public static class ExcelAdapter
{
    private static List<String> ColumnNames = new List<String>() 
    { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
      "AA", "AB", "AC", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "AL", "AM", "AN", "AO", "AP", "AQ", "AR", "AS", "AT", "AU", "AV", "AW", "AX", "AY", "AZ", " "};

    // Open/Close
    public static SpreadsheetDocument OpenSpreadSheet(String fileName, int connectionAttempts)
    {
        SpreadsheetDocument ss = null;
        // attempt to connect (file may be locked by another user)
        for (int i = 0; i < connectionAttempts; i++)
        {
            try
            {
                ss = SpreadsheetDocument.Open(fileName, true);
                ss.WorkbookPart.Workbook.CalculationProperties.ForceFullCalculation = true;
                ss.WorkbookPart.Workbook.CalculationProperties.FullCalculationOnLoad = true;
                ss.WorkbookPart.Workbook.CalculationProperties.CalculationOnSave = true;
                return ss;
            }
            catch(Exception r) 
            {
                // break loop if connection fails for unknown reason
                if (!r.Message.Contains("used by another process")) 
                {
                    break;
                }
                System.Threading.Thread.Sleep(250);
            }
        }
        return ss;
    }
    public static void CloseSpreadSheet(SpreadsheetDocument ss)
    {
        ss.Close();
    }
    public static void CloseSpreadSheet(SpreadsheetDocument ss, int closeAttempts)
    {
        for (int i = 0; i < closeAttempts; i++)
        {
            try
            {
                ss.Close();
            }
            catch (Exception r)
            {
                // break loop if connection fails for unknown reason
                if (!r.Message.Contains("used by another process"))
                {
                    break;
                }
                System.Threading.Thread.Sleep(250);
            }
        }
    }

    // Inserting
    public static WorksheetPart InsertBlankWorkSheet(SpreadsheetDocument ss, String newSheetName)
    {
        // Add a new worksheet part to the workbook.
        WorkbookPart wbp = ss.WorkbookPart;
        WorksheetPart new_wsp = wbp.AddNewPart<WorksheetPart>();
        SheetData sheet_data = new SheetData();
        new_wsp.Worksheet = new Worksheet(sheet_data);
        new_wsp.Worksheet.Save();

        Sheets sheets = wbp.Workbook.GetFirstChild<Sheets>();
        String relationshipId = wbp.GetIdOfPart(new_wsp);
        // Get a unique ID for the new sheet.
        uint sheetId = 1;
        if (sheets.Elements<Sheet>().Count() > 0)
        {
            sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
        }
        String sheetName = newSheetName;
        // Append the new worksheet and associate it with the workbook.
        Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
        sheets.Append(sheet);
        wbp.Workbook.Save();

        return new_wsp;
    }
    public static WorksheetPart InsertWorkSheetWithData(SpreadsheetDocument ss, String newSheetName, DataTable dt_data, bool includeHeaderRow, bool addTotalPriceColumn)
    {
        // Add a new worksheet part to the workbook.
        WorksheetPart new_wsp = InsertBlankWorkSheet(ss, newSheetName);
        AddDataToWorkSheet(new_wsp, dt_data, includeHeaderRow, true, addTotalPriceColumn);

        return new_wsp;
    }
    public static void AddDataToWorkSheet(SpreadsheetDocument ss, String sheetName, DataTable dt_data, bool includeHeaderRow, bool clearSheet, bool addTotalPriceColumn)
    {
        WorksheetPart wsp_group = GetWorkSheetPartByName(ss, sheetName);
        AddDataToWorkSheet(wsp_group, dt_data, includeHeaderRow, clearSheet, addTotalPriceColumn);
    }
    public static void AddDataToWorkSheet(WorksheetPart wsp, DataTable dt_data, bool includeHeaderRow, bool clearSheet, bool addTotalPriceColumn)
    {
        if (dt_data.Rows.Count > 0)
        {
            Worksheet worksheet = wsp.Worksheet;
            SheetData sheet_data = worksheet.GetFirstChild<SheetData>();
            if(clearSheet)
                sheet_data.RemoveAllChildren();

            // Add header
            Row headerRow = new Row();
            List<String> columns = new List<String>();
            foreach (DataColumn column in dt_data.Columns)
            {
                columns.Add(column.ColumnName);
                Cell cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(column.ColumnName);
                headerRow.AppendChild(cell);
            }
            if(includeHeaderRow)
                sheet_data.AppendChild(headerRow);

            // Add row data
            double test_number = -1;
            foreach (DataRow dr in dt_data.Rows)
            {
                Row row = new Row();
                foreach (String col in columns)
                {
                    Cell cell = new Cell();
                    if (Double.TryParse(dr[col].ToString(), out test_number))
                        cell.DataType = CellValues.Number;
                    else
                        cell.DataType = CellValues.String;

                    // Check if this is a formula column
                    if (col.Contains("(F)"))
                        cell.CellFormula = new CellFormula(dr[col].ToString());
                    else
                        cell.CellValue = new CellValue(dr[col].ToString());

                    row.AppendChild(cell);
                }
                sheet_data.AppendChild(row);
            }

            if (addTotalPriceColumn)
            {
                // THIS APPLIES ONLY TO FINANCE DUE LISTINGS -- WILL NEED REMOVING // 
                // Add total row
                int price_idx = dt_data.Columns.IndexOf("Price");
                if (price_idx != -1)
                {
                    Row t_row = new Row();
                    foreach (String col in columns)
                    {
                        t_row.AppendChild(new Cell() { DataType = CellValues.String, CellValue = new CellValue(String.Empty) });
                    }
                    sheet_data.AppendChild(t_row);
                    ((Cell)t_row.ChildElements[0]).CellValue = new CellValue("Totals");
                    ((Cell)t_row.ChildElements[price_idx]).DataType = CellValues.Number;
                    ((Cell)t_row.ChildElements[price_idx]).CellFormula = new CellFormula("SUM(G2:G" + (dt_data.Rows.Count + 1) + ")");
                    ((Cell)t_row.ChildElements[(price_idx + 1)]).DataType = CellValues.Number;
                    ((Cell)t_row.ChildElements[(price_idx + 1)]).CellFormula = new CellFormula("SUM(H2:H" + (dt_data.Rows.Count + 1) + ")");
                }
            }
        }
    }
    public static void CloneWorkSheet(SpreadsheetDocument ss, String sheetNameToClone, String newSheetName)
    {
        WorkbookPart wbp = ss.WorkbookPart; // get reference to Workbook in SpreadSheet doc

        // Get the source sheet to be copied
        WorksheetPart source_wsp = GetWorkSheetPartByName(ss, sheetNameToClone);

        //Take advantage of AddPart for deep cloning
        SpreadsheetDocument temp_ss = SpreadsheetDocument.Create(new MemoryStream(), ss.DocumentType);
        WorkbookPart temp_wbp = temp_ss.AddWorkbookPart();
        WorksheetPart temp_wsp = temp_wbp.AddPart<WorksheetPart>(source_wsp);
        //Add cloned sheet and all associated parts to workbook
        WorksheetPart cloned_wsp = wbp.AddPart<WorksheetPart>(temp_wsp);
        cloned_wsp.Worksheet.Save();

        //Table definition parts are somewhat special and need unique ids...so let's make an id based on count
        int numTableDefParts = source_wsp.GetPartsCountOfType<TableDefinitionPart>();
        //Clean up table definition parts (tables need unique ids)
        if (numTableDefParts != 0)
            FixupTableParts(cloned_wsp, numTableDefParts);
        //There should only be one sheet that has focus
        CleanView(cloned_wsp);

        Sheets sheets = wbp.Workbook.GetFirstChild<Sheets>();
        // Get a unique ID for the new sheet (important)
        uint sheetId = 1;
        if (sheets.Elements<Sheet>().Count() > 0)
        {
            sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
        }
        // Add new sheet to wbp's sheet collection
        Sheet cloned_sheet = new Sheet();
        cloned_sheet.Name = newSheetName;
        cloned_sheet.Id = wbp.GetIdOfPart(cloned_wsp);
        cloned_sheet.SheetId = sheetId;
        sheets.Append(cloned_sheet);

        // Save Changes
        wbp.Workbook.Save();
    }

    // Updating
    public static void UpdateCell(SpreadsheetDocument ss, String sheetName, int rowIndex, String columnName, String cellValue)
    {
        WorksheetPart wsp = GetWorkSheetPartByName(ss, sheetName);
        if (wsp != null)
        {
            Cell cell = GetCell(wsp.Worksheet, columnName, rowIndex);
            cell.CellValue = new CellValue(cellValue);
            cell.DataType = new EnumValue<CellValues>(CellValues.Number);

            // Save the worksheet.
            wsp.Worksheet.Save();
        }
    }
    private static Cell GetCell(Worksheet worksheet, String columnName, int rowIndex)
    {
        Row row = GetRow(worksheet, rowIndex);
        if (row == null)
            return null;

        return row.Elements<Cell>().Where(c => String.Compare
               (c.CellReference.Value, columnName +
               rowIndex, true) == 0).First();
    }
    private static Row GetRow(Worksheet worksheet, int rowIndex)
    {
        return worksheet.GetFirstChild<SheetData>(). Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
    } 

    // Retrieving
    public static DataTable GetWorkSheetData(SpreadsheetDocument ss, String sheetName, bool includeHeaderRow, int headerRowOffset = 0)
    {
        DataTable dt = new DataTable();

        WorksheetPart wsp;
        if (sheetName != null)
            wsp = GetWorkSheetPartByName(ss, sheetName);
        else
            wsp = GetFirstWorkSheetPart(ss);

        if (wsp != null)
        {
            Worksheet ws = wsp.Worksheet;
            SheetData sheet_data = ws.GetFirstChild<SheetData>();
            IEnumerable<Row> rows = sheet_data.Descendants<Row>();

            foreach (Cell cell in rows.ElementAt(0))
            {
                String ColumnName = GetCellValue(ss, cell);
                if (!dt.Columns.Contains(ColumnName))
                    dt.Columns.Add(ColumnName);
            }

            // Fill data table with Excel data.
            // This implementation includes blank cells from Excel datasets, meaning the datatable will not
            // skew when encoutering skipped elements in the XML file (blank cells are null, and are not included in the XML).
            foreach (Row row in rows)
            {
                DataRow tempRow = dt.NewRow();

                int columnIndex = 0;
                foreach (Cell cell in row.Descendants<Cell>())
                {
                    // Gets the column index of the cell with data
                    int cellColumnIndex = ColumnNames.IndexOf(GetColumnName(cell.CellReference));
                    // (int)GetColumnIndexFromName(GetColumnName(cell.CellReference));

                    if (columnIndex < cellColumnIndex)
                    {
                        do
                        {
                            if (tempRow.ItemArray.Length > columnIndex)
                                tempRow[columnIndex] = String.Empty;
                            else
                                break;
                            columnIndex++;
                        }
                        while (columnIndex < cellColumnIndex);
                    }

                    String cell_value = GetCellValue(ss, cell);
                    if (columnIndex < tempRow.ItemArray.Length)
                        tempRow[columnIndex] = cell_value;

                    columnIndex++;
                }
                dt.Rows.Add(tempRow);
            }

            if (headerRowOffset != 0)
            {
                for (int row = 1; row < (headerRowOffset + 1); row++)
                    dt.Rows.RemoveAt(0);

                // Attempt to set new DataTable header names (as the headers would have been set earlier, without regard for the header offset)
                if (dt.Rows.Count > 0)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        String ColumnName = dt.Rows[0][j].ToString();
                        if (String.IsNullOrEmpty(ColumnName)) // give dummny name if necessary
                            ColumnName = j.ToString();
                        dt.Columns[j].ColumnName = ColumnName;
                    }
                }
            }
            
            if (!includeHeaderRow)
                dt.Rows.RemoveAt(0);
        }

        return dt;
    }
    public static DataTable GetFirstWorkSheetData(SpreadsheetDocument ss, bool includeHeaderRow, int headerRowOffset = 0)
    {
        return GetWorkSheetData(ss, null, false, headerRowOffset);
    }
    private static string GetCellValue(SpreadsheetDocument ss, Cell cell)
    {
        SharedStringTablePart stringTablePart = ss.WorkbookPart.SharedStringTablePart;
        String value = String.Empty;
        if (cell.CellValue != null)
            value = cell.CellValue.InnerXml;

        if (value != String.Empty && cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
        else
            return value;
    }
    
    // Misc
    public static WorksheetPart GetWorkSheetPartByName(SpreadsheetDocument ss, String sheetName)
    {
        IEnumerable<Sheet> sheets = ss.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>().Where(s => s.Name == sheetName);
        if (sheets.Count() == 0)
        {
            // The specified worksheet does not exist
            ss.Close();
            return null;
            //throw new Exception("Worksheet by name '" + sheetName + "' doesn't exist.");
        }

        String relationshipId = sheets.First().Id.Value;
        WorksheetPart worksheetPart = (WorksheetPart)ss.WorkbookPart.GetPartById(relationshipId);
        return worksheetPart;
    }
    public static WorksheetPart GetFirstWorkSheetPart(SpreadsheetDocument ss)
    {
        IEnumerable<Sheet> sheets = ss.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();//.Where(s => s.Name == sheetName);
        if (sheets.Count() == 0)
        {
            // The specified worksheet does not exist
            //return null;
            throw new Exception("Can't find first worksheet in document!");
        }

        String relationshipId = sheets.First().Id.Value;
        WorksheetPart worksheetPart = (WorksheetPart)ss.WorkbookPart.GetPartById(relationshipId);
        return worksheetPart;
    }
    public static void FixupTableParts(WorksheetPart worksheetPart, int numTableDefParts)
    {
        //Every table needs a unique id and name
        foreach (TableDefinitionPart tableDefPart in worksheetPart.TableDefinitionParts)
        {
            numTableDefParts++;
            tableDefPart.Table.Id = (uint)numTableDefParts;
            tableDefPart.Table.DisplayName = "CopiedTable" + numTableDefParts;
            tableDefPart.Table.Name = "CopiedTable" + numTableDefParts;
            tableDefPart.Table.Save();
        }
    }
    public static void CleanView(WorksheetPart worksheetPart)
    {
        //There can only be one sheet that has focus
        SheetViews views = worksheetPart.Worksheet.GetFirstChild<SheetViews>();
        if (views != null)
        {
            views.Remove();
            worksheetPart.Worksheet.Save();
        }
    }
    public static String GetColumnName(String cellReference)
    {
        // Create a regular expression to match the column name portion of the cell name.
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[A-Za-z]+");
        System.Text.RegularExpressions.Match match = regex.Match(cellReference);

        return match.Value;
    }
    public static int? GetColumnIndexFromName(string columnName) // no longer used
    {
        int? columnIndex = null;

        //string[] colLetters = System.Text.RegularExpressions.Regex.Split(columnName, "([A-Z]+)");
        //colLetters = colLetters.Where(s => !string.IsNullOrEmpty(s)).ToArray();

        //if (colLetters.Count() <= 2)
        //{
        //    int index = 0;
        //    foreach (string col in colLetters)
        //    {
        //        List<char> col1 = colLetters.ElementAt(index).ToCharArray().ToList();
        //        int? indexValue = ColumnNames.IndexOf(col1.ElementAt(index));

        //        if (indexValue != -1)
        //        {
        //            // The first letter of a two digit column needs some extra calculations
        //            if (index == 0 && colLetters.Count() == 2)
        //                columnIndex = columnIndex == null ? (indexValue + 1) * 26 : columnIndex + ((indexValue + 1) * 26);
        //            else
        //                columnIndex = columnIndex == null ? indexValue : columnIndex + indexValue;
        //        }
        //        index++;
        //    }
        //}
        return columnIndex;
    }
}