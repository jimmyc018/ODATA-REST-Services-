//Name: Jimmy Chun Yew Chan
//UPI: JCHA752
//ID: 8302265

namespace A4FS

open System
open System.Collections.Generic
open System.Data.Services
open System.Data.Services.Common
open System.Data.Services.Providers
open System.Linq
open System.ServiceModel
open System.ServiceModel.Web
open System.Web
open System.Xml.Linq

//Customer Class
type [<AllowNullLiteral(); DataServiceKey("CustomerID")>] 
  Customer () = class  // public
    member val CustomerID: string = null with get, set
    member val CompanyName: string = null with get, set
    member val ContactName: string = null with get, set
    member val Orders: seq<Order> = null with get, set
    end

//Order Class
and [<AllowNullLiteral(); DataServiceKey("OrderID")>] 
  Order () = class  // public
    member val OrderID: int = 0 with get, set
    member val CustomerID: string = null with get, set
    member val OrderDate: Nullable<DateTime> = Nullable<DateTime>() with get, set
    member val ShippedDate: Nullable<DateTime> = Nullable<DateTime>() with get, set
    member val Freight: Nullable<decimal> = Nullable<decimal>() with get, set
    member val ShipName: string = null with get, set
    member val ShipCity: string = null with get, set
    member val ShipCountry: string = null with get, set
    member val Customer: Customer = null with get, set
    end

type public MyDataSource () = class
    static let FOLDER = @".\data\"
    // @"C:\usertmp\";

    static let mutable _MyCustomers: seq<Customer> = null
    static let mutable _MyOrders: seq<Order> = null
    static let xn = XName.Get
    
    static do  // MyDataSource ()
        Console.WriteLine("... loading {0}\\XCustomers.xml", FOLDER)
        _MyCustomers <- XElement
            .Load(FOLDER + @"\XCustomers.xml")
            .Elements(xn "Customer")
            .Select(fun x -> 
                new Customer (
                    CustomerID = x.Element(xn "CustomerID").Value,
                    CompanyName = x.Element(xn "CompanyName").Value,
                    ContactName = x.Element(xn "ContactName").Value))
            .ToArray()

        Console.WriteLine("... loading {0}\\XOrders.xml", FOLDER)
        _MyOrders <- XElement
            .Load(FOLDER + @"\XOrders.xml")
            .Elements(xn "Order")
            .Select(fun x -> 
                new Order (
                    OrderID = Int32.Parse (x.Element(xn "OrderID").Value),
                    CustomerID = x.Element(xn "CustomerID").Value,
                    OrderDate = (if isNull(x.Element(xn "OrderDate")) then Nullable<DateTime>() else (if String.IsNullOrEmpty(x.Element(xn "OrderDate").Value) then Nullable<DateTime>() else Nullable (DateTime.Parse (x.Element(xn "OrderDate").Value)))),
                    ShippedDate = (if isNull(x.Element(xn "ShippedDate")) then Nullable<DateTime>() else (if String.IsNullOrEmpty(x.Element(xn "ShippedDate").Value) then Nullable<DateTime>() else Nullable (DateTime.Parse (x.Element(xn "ShippedDate").Value)))),
                    Freight = (if isNull(x.Element(xn "Freight")) then Nullable<decimal>() else (if String.IsNullOrEmpty(x.Element(xn "Freight").Value) then Nullable<decimal>() else Nullable (Decimal.Parse (x.Element(xn "Freight").Value)))),
                    ShipName = x.Element(xn "ShipName").Value,
                    ShipCity = x.Element(xn "ShipCity").Value,
                    ShipCountry = x.Element(xn "ShipCountry").Value))
            .ToArray();

        Console.WriteLine("... relating _MyCustomers with _MyOrders")

        let MyOrderLookUp = _MyOrders.ToLookup (fun order -> order.CustomerID)
        let MyCustomerDictionary = _MyCustomers.ToDictionary (fun customer -> customer.CustomerID)
        for order in _MyOrders do order.Customer <- MyCustomerDictionary.[order.CustomerID]
        for customer in _MyCustomers do customer.Orders <- MyOrderLookUp.[customer.CustomerID]
        Console.WriteLine("... starting");

    //Queryables Customers	
    member this.Customers
        with get (): IQueryable<Customer> = 
            Console.WriteLine("... returning Customers")
            _MyCustomers.AsQueryable()
    //Queryables Orders		
    member this.Orders
        with get (): IQueryable<Order> = 
            Console.WriteLine("... returning Orders")
            _MyOrders.AsQueryable()
    end

[<ServiceBehavior(IncludeExceptionDetailInFaults = true)>]
type public A4FS () = class 
    inherit DataService<MyDataSource> ()
    static member InitializeService (config: DataServiceConfiguration): unit =
        config.SetEntitySetAccessRule ("*", EntitySetRights.AllRead)
        // config.SetServiceOperationAccessRule ("MyServiceOperation", ServiceOperationRights.All)
        config.DataServiceBehavior.MaxProtocolVersion <- DataServiceProtocolVersion.V3;
        config.UseVerboseErrors <- true;
    end 