#define Serializable
//#define Parcelable
//#define Json

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PassComplexObjects
{
    [Activity(Label = "SecondActivity")]
    public class SecondActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var watch = System.Diagnostics.Stopwatch.StartNew();
			

#if Parcelable
			EmployeeParcelable myEmployeeParcelable = (EmployeeParcelable)Intent.GetParcelableExtra("employee");                               
#endif

#if Serializable

			EmployeeSerializable myEmployeeSerializable = (EmployeeSerializable)Intent.GetSerializableExtra("employee");
#endif

#if Json
			EmployeeJson myEmployeeJson = Newtonsoft.Json.JsonConvert.DeserializeObject<EmployeeJson>(Intent.GetStringExtra("employee"));
#endif

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("Time in GetExtra");
            Console.WriteLine($"{elapsedMs} ms");
        }
    }
}
