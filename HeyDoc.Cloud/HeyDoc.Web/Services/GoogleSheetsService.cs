using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Data = Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace HeyDoc.Web.Services
{
    public class GoogleSheetsService
    {
        private string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private string _appName = "HOPE Server";
        private SheetsService _sheetsService;

        public async Task Connect(string credsFilename)
        {
            var credential = (await GoogleCredential.FromFileAsync(credsFilename, new System.Threading.CancellationToken())).CreateScoped(_scopes);

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName
            });
        }

        public async Task<string> CreateOrReplaceSheet(string googleSheetsId, string sheetName, IList<IList<object>> data)
        {
            if (_sheetsService == null)
            {
                throw new Exception("Service must be initialized with .Connect() before using.");
            }

            // adding a new sheet if does not exist
            try
            {
                await _sheetsService.Spreadsheets.BatchUpdate(new Data.BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Data.Request>()
                    {
                        new Data.Request
                        {
                            AddSheet = new Data.AddSheetRequest
                            {
                                Properties = new Data.SheetProperties
                                {
                                    Title = sheetName
                                }
                            }
                        }
                    }
                }, googleSheetsId).ExecuteAsync();
            }
            catch
            {
                // happens when the sheet already exists
            }

            // clear sheet
            await _sheetsService.Spreadsheets.Values.Clear(new Data.ClearValuesRequest(), googleSheetsId, sheetName).ExecuteAsync();

            // updating cells with values
            List<Data.ValueRange> updateData = new List<Data.ValueRange>();
            var dataValueRange = new Data.ValueRange
            {
                Range = sheetName,
                Values = data
            };
            updateData.Add(dataValueRange);

            Data.BatchUpdateValuesRequest requestBody = new Data.BatchUpdateValuesRequest
            {
                ValueInputOption = "USER_ENTERED",
                Data = updateData
            };

            var request = _sheetsService.Spreadsheets.Values.BatchUpdate(requestBody, googleSheetsId);

            Data.BatchUpdateValuesResponse response = await request.ExecuteAsync();

            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> InsertOrReplaceRows(string googleSheetsId, string sheetName, IList<IList<object>> data, int startRowIndex = 0, int? endRowIndex = null, int? startColIndex = 0, int? endColIndex = null, bool shouldReplace = false)
        {
            if (_sheetsService == null)
            {
                throw new Exception("Service must be initialized with .Connect() before using.");
            }

            if (!endRowIndex.HasValue)
            {
                endRowIndex = startRowIndex + data.Count;
            }
            if (!endColIndex.HasValue)
            {
                endColIndex = startColIndex + data.Select(e => e.Count).Max(); // maximum number of columns
            }

            // adding a new sheet if does not exist
            try
            {
                await _sheetsService.Spreadsheets.BatchUpdate(new Data.BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Data.Request>()
                    {
                        new Data.Request
                        {
                            AddSheet = new Data.AddSheetRequest
                            {
                                Properties = new Data.SheetProperties
                                {
                                    Title = sheetName
                                }
                            }
                        }
                    }
                }, googleSheetsId).ExecuteAsync();
            }
            catch
            {
                // happens when the sheet already exists
            }

            var sheetsInfo = await _sheetsService.Spreadsheets.Get(googleSheetsId).ExecuteAsync();
            var sheetId = sheetsInfo.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName).Properties.SheetId.Value;

            if (!shouldReplace)
            {
                await _sheetsService.Spreadsheets.BatchUpdate(new Data.BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Data.Request>()
                    {
                        new Data.Request
                        {
                            InsertDimension = new Data.InsertDimensionRequest
                            {
                                Range = new Data.DimensionRange
                                {
                                    SheetId = sheetId,
                                    Dimension = "rows",
                                    StartIndex = startRowIndex,
                                    EndIndex = endRowIndex
                                }
                            }
                        }
                    }
                }, googleSheetsId).ExecuteAsync();
            }

            IList<Data.RowData> rows = data.Select(dList => new Data.RowData
            {
                Values = dList.Select(d => new Data.CellData
                {
                    UserEnteredValue = new Data.ExtendedValue
                    {
                        StringValue = d.ToString()
                    }
                }).ToList()
            }).ToList();

            var range = new Data.GridRange
            {
                SheetId = sheetId,
                StartRowIndex = startRowIndex,
                EndRowIndex = endRowIndex,
                StartColumnIndex = startColIndex,
                EndColumnIndex = endColIndex
            };

            Data.BatchUpdateSpreadsheetResponse response = null;
            response = await _sheetsService.Spreadsheets.BatchUpdate(new Data.BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Data.Request>()
                {
                    new Data.Request
                    {
                        UpdateCells = new Data.UpdateCellsRequest
                        {
                            Rows = rows,
                            Range = range,
                            Fields = "*"
                        }
                    }
                }
            }, googleSheetsId).ExecuteAsync();

            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> AppendRows(string googleSheetsId, string sheetName, List<IList<object>> data, bool shouldReplace = false, string range = null)
        {
            if (_sheetsService == null)
            {
                throw new Exception("Service must be initialized with .Connect() before using.");
            }

            // adding a new sheet if does not exist
            try
            {
                await _sheetsService.Spreadsheets.BatchUpdate(new Data.BatchUpdateSpreadsheetRequest
                {
                    Requests = new List<Data.Request>()
                    {
                        new Data.Request
                        {
                            AddSheet = new Data.AddSheetRequest
                            {
                                Properties = new Data.SheetProperties
                                {
                                    Title = sheetName
                                }
                            }
                        }
                    }
                }, googleSheetsId).ExecuteAsync();
            }
            catch
            {
                // happens when the sheet already exists
            }
            
            // updating cells with values
            var dataValueRange = new Data.ValueRange
            {
                Range = $"{ sheetName }",
                Values = data
            };

            var request = _sheetsService.Spreadsheets.Values.Append(dataValueRange, googleSheetsId, $"{ sheetName }{ (range != null ? ("!" + range) : "") }");
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            request.InsertDataOption = shouldReplace ? SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE : SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            
            Data.AppendValuesResponse response = await request.ExecuteAsync();

            return JsonConvert.SerializeObject(response);
        }
    }
}