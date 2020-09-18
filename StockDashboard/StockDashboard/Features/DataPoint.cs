using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace StockDashboard.Features
{
    [DataContract]
    public class DataPoint
    {
		public DataPoint(dynamic x, dynamic y)
		{
			this.X = x;
			this.Y = y;
		}
		//Hello
		//Explicitly setting the name to be used while serializing to JSON.
		[DataMember(Name = "label")]
		public dynamic X = null;

		//Explicitly setting the name to be used while serializing to JSON.
		[DataMember(Name = "y")]
		public dynamic Y = null;
	}
}
