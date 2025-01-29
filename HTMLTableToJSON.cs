To convert an HTML table into a JSON object using C# Selenium while handling redundancy and ensuring robustness, follow this approach:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenQA.Selenium;

public static class TableToJsonConverter
{
    public static string ConvertTableToJson(IWebElement table)
    {
        var (headers, skipFirstRow) = GetHeaders(table);
        var rows = GetRows(table, skipFirstRow);
        var jsonData = ProcessRows(rows, headers);
        return JsonConvert.SerializeObject(jsonData, Formatting.Indented);
    }

    private static (List<string> Headers, bool SkipFirstRow) GetHeaders(IWebElement table)
    {
        bool skipFirstRow = false;
        var headers = new List<string>();

        // Extract headers from thead or first row
        var thead = table.FindElements(By.TagName("thead")).FirstOrDefault();
        if (thead != null)
        {
            headers.AddRange(thead.FindElements(By.TagName("th")).Select(th => th.Text.Trim()));
        }
        else
        {
            var firstRow = table.FindElements(By.TagName("tr")).FirstOrDefault();
            if (firstRow != null)
            {
                var ths = firstRow.FindElements(By.TagName("th"));
                if (ths.Any())
                {
                    headers.AddRange(ths.Select(th => th.Text.Trim()));
                    skipFirstRow = true;
                }
                else
                {
                    var tds = firstRow.FindElements(By.TagName("td"));
                    if (tds.Any())
                    {
                        headers.AddRange(tds.Select(td => td.Text.Trim()));
                        skipFirstRow = true;
                    }
                }
            }
        }

        // Generate headers if none found
        if (headers.Count == 0)
        {
            var firstDataRow = GetRows(table, false).FirstOrDefault();
            var cells = firstDataRow?.FindElements(By.TagName("td"));
            if (cells != null)
            {
                headers.AddRange(Enumerable.Range(1, cells.Count).Select(i => $"Column_{i}"));
            }
        }

        // Process duplicates and empty headers
        headers = ProcessHeaders(headers);

        return (headers, skipFirstRow);
    }

    private static List<string> ProcessHeaders(List<string> headers)
    {
        var headerCounts = new Dictionary<string, int>();
        var processedHeaders = new List<string>();

        foreach (var header in headers)
        {
            string baseHeader = string.IsNullOrWhiteSpace(header) ? "Column" : header;
            if (headerCounts.ContainsKey(baseHeader))
            {
                headerCounts[baseHeader]++;
                processedHeaders.Add($"{baseHeader}_{headerCounts[baseHeader]}");
            }
            else
            {
                headerCounts[baseHeader] = 1;
                processedHeaders.Add(baseHeader);
            }
        }

        return processedHeaders;
    }

    private static List<IWebElement> GetRows(IWebElement table, bool skipFirstRow)
    {
        var rows = table.FindElements(By.XPath(".//tr[not(ancestor::thead)]")).ToList();
        if (skipFirstRow && rows.Count > 0) rows.RemoveAt(0);
        return rows;
    }

    private static List<Dictionary<string, object>> ProcessRows(
        List<IWebElement> rows, List<string> headers)
    {
        var jsonData = new List<Dictionary<string, object>>();

        foreach (var row in rows)
        {
            var cells = row.FindElements(By.TagName("td"));
            if (cells.Count != headers.Count) continue;

            var rowData = new Dictionary<string, object>();
            for (int i = 0; i < headers.Count; i++)
            {
                string cellText = cells[i].Text.Trim();
                rowData[headers[i]] = string.IsNullOrEmpty(cellText) ? null : cellText;
            }
            jsonData.Add(rowData);
        }

        return jsonData;
    }
}
```

**Key Features:**
1. **Header Extraction:** Handles headers in `<thead>` or first row (`<th>` or `<td>`)
2. **Header Processing:** 
   - Generates unique names for duplicate headers
   - Replaces empty headers with "Column_N"
   - Auto-generates headers if none found
3. **Row Processing:**
   - Skips rows with incorrect cell counts
   - Trims whitespace and handles empty cells
4. **Robust Structure:** Handles various table structures and edge cases

**Usage:**
```csharp
IWebElement table = driver.FindElement(By.TagName("table"));
string json = TableToJsonConverter.ConvertTableToJson(table);
Console.WriteLine(json);
```

This implementation provides a comprehensive solution for converting HTML tables to JSON while addressing common edge cases and maintaining data integrity.
