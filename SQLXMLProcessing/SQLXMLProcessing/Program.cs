using SQLXMLProcessing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SQLXMLProcessing
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.Write("Test");


            //update the Coonection string...........
            var sConnection = "";

            //Update the XML object.............
            var strXML = @"
                <Records>
                    <Record>
                        <Id>1</Id>
                        <Name>John Doe</Name>
                        <Age>30</Age>
                    </Record>
                    <Record>
                        <Id>2</Id>
                        <Name>Jane Smith</Name>
                        <Age>25</Age>
                    </Record>
                </Records>";
            //Call the XML to process into SQL.............
            SQLXMLCRUD.InsertXMLData(sConnection, strXML);

            Console.WriteLine("XML process Completed");

        }
    }
}
