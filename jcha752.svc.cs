//Name: Jimmy Chun Yew Chan
//ID: 8302265
//UPI: JCHA752

using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Data.Services.Providers;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;
using System.Xml.Linq;

namespace A4CS {
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class A4CS : DataService<MyDataSource> {
        public static void InitializeService(DataServiceConfiguration config) {
            config.SetEntitySetAccessRule("*", EntitySetRights.AllRead);
            // config.SetServiceOperationAccessRule("MyServiceOperation", ServiceOperationRights.All);
            config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
            config.UseVerboseErrors = true;
        }
    }
    //Customer Class
    [DataServiceKey("CustomerID")]
    public class Customer {
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public IEnumerable<Order> Orders { get; set; }
    }

    //Order Class
    [DataServiceKey("OrderID")]
    public class Order {
        public int OrderID { get; set; }
        public string CustomerID { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public decimal? Freight { get; set; }
        public string ShipName { get; set; }
        public string ShipCity { get; set; }
        public string ShipCountry { get; set; }
        public Customer Customer { get; set; }
    }

    public class MyDataSource {
        static string FOLDER = @".\data\";

        static MyDataSource() {
            Console.WriteLine(" ... loading {FOLDER}\\XCustomers.xml");
            _MyCustomers = 
                XElement.Load(FOLDER + @"\XCustomers.xml")
                .Elements("Customer")
                .Select(x => new Customer {
                    CustomerID = (string)x.Element("CustomerID"),
                    CompanyName = (string)x.Element("CompanyName"),
                    ContactName = (string)x.Element("ContactName"),
                }).ToArray();

            Console.WriteLine("... loading {FOLDER}\\XOrders.xml");
            _MyOrders =
                XElement.Load(FOLDER + @"\XOrders.xml")
                .Elements("Order")
                .Select(x => new Order {
                    OrderID = (int)x.Element("OrderID"),
                    CustomerID = (string)x.Element("CustomerID"),
                    OrderDate = string.IsNullOrEmpty((string)x.Element("OrderDate")) ? (DateTime?)null : (DateTime?)x.Element("OrderDate"),
                    ShippedDate = string.IsNullOrEmpty((string)x.Element("ShippedDate")) ? (DateTime?)null : (DateTime?)x.Element("ShippedDate"),
                    Freight = string.IsNullOrEmpty((string)x.Element("Freight")) ? (decimal?)null : (decimal?)x.Element("Freight"),
                    ShipName = (string)x.Element("ShipName"),
                    ShipCity = (string)x.Element("ShipCity"),
                    ShipCountry = (string)x.Element("ShipCountry"),
                }).ToArray();

            Console.WriteLine(" ... relating _MyCustomers with _MyOrders");

            var ordersLookup = _MyOrders.ToLookup( order_cust => order_cust.CustomerID);
            var customerDictionary = _MyCustomers.ToDictionary(customer => customer.CustomerID);
            foreach (var order_cust in _MyOrders) order_cust.Customer = customerDictionary[order_cust.CustomerID];
            foreach (var customer in _MyCustomers) customer.Orders = ordersLookup[customer.CustomerID];  
            Console.WriteLine(" ... starting");
        }
        
        //Enumerables Customer
        static IEnumerable<Customer> _MyCustomers;
        //Enumerables Order
        static IEnumerable<Order> _MyOrders;
        //Queryables Customer
        public IQueryable<Customer> Customers {
            get {
                Console.WriteLine("... returning Customers");
                return _MyCustomers.AsQueryable();
            }
        }
        //Queryables Order
        public IQueryable<Order> Orders {
            get {
                Console.WriteLine("... returning Orders");
                return _MyOrders.AsQueryable();
            }
        }
    }	
}