
.NET Core Currency Converter API
--------------------------------
This repository holds the source code for a .NET Core web API that offers currency conversion functionalities.

### Features
*   Retrieve the latest exchange rates for a specified currency.
*   Retrieve the latest exchange rate between two currencies.
*   Explore historical exchange rates for a specified base currency within a date range.
    

### Getting Started

**Prerequisites:**

*   .NET SDK 8.0 or later ([https://dotnet.microsoft.com/en-us/download](https://dotnet.microsoft.com/en-us/download))
    

**Running the API Locally:**

1.  Clone this repository.
    
2.  Navigate to the project directory in your terminal.
    
3.  Restore dependencies: dotnet restore
    
4.  Build the project: dotnet build
    
5.  Run the API:
    
    *   dotnet run (starts the API)
        
    *   dotnet watch (starts the API and automatically rebuilds on code changes)
        

**Configuration:**

The API utilizes configuration settings stored in `appsettings.json` (or a different environment-specific file). You can modify these settings, such as the `ExcludedCurrencies` , `FrankfurtAPIBaseUrl` (an external currency data provider used by the API).

### Endpoints

**1. /CurrencyConverterApi/latest**

-   **Description:** Retrieves the latest exchange rate for a specified base currency.
-   **API Controller:** ["CurrencyConverterApi"]
-   **Parameters:**
    -   `baseCurrency` (string,  **required**): The base currency for which to retrieve the latest exchange rate.
-   **Responses:**
    -   **200 (Success):** Returns an object containing the exchange rates for various target currencies relative to the requested base currency. The specific format of the response object depends on your implementation.

**2. /CurrencyConverterApi/convert**

-   **Description:** Converts a given amount from one currency to another using the latest exchange rates.
-   **API Controller:** ["CurrencyConverterApi"]
-   **Parameters:**
    -   `Amount` (number, format: double,  **required**): The amount to be converted.
    -   `From` (string,  **required**): The currency from which the amount is being converted.
    -   `To` (string,  **required**): The target currency to which the amount is being converted.
-   **Responses:**
    -   **200 (Success):** Returns an object containing the converted amount in the target currency. The specific format of the response object depends on your implementation.

**3. /CurrencyConverterApi/history**

-   **Description:** Retrieves historical exchange rates for a specified base currency within a date range. Supports pagination for retrieving data in chunks.
-   **Tags:** ["CurrencyConverterApi"]
-   **Parameters:**
    -   `BaseCurrency` (string,  **required**): The base currency for which to retrieve historical exchange rates.
    -   `StartDate` (string, format: date-time,  **required**): The start date of the desired historical data range (inclusive).
    -   `EndDate` (string, format: date-time,  **required**): The end date of the desired historical data range (inclusive).
    -   `Take` (integer, format: int32, optional): The number of records to retrieve per page for pagination (defaults to a reasonable value).
    -   `Page` (integer, format: int32, optional): The current page number for pagination (defaults to 1).
-   **Responses:**
    -   **200 (Success):** Returns an object containing historical exchange rates for the requested base currency within the specified date range. The structure of this object depends on your specific implementation, but it typically includes:
        -   `TotalRecords` (integer): The total number of historical rates available for the given criteria.
        -   `PageSize` (integer): The number of records returned per page (matches the `Take` parameter).
        -   `PageNo` (integer): The current page number (matches the `Page` parameter).
        -   `Data` (object): An object containing historical exchange rates. The format of this data may vary depending on your implementation.

### Development

This project utilizes `xUnit` for unit testing. Run tests using the `dotnet test` command.

### Future Enhancements

 - Improve caching for `/CurrencyConverterApi/history` api to retrieve the data from cache if the specific range is within the already cached data.