#define Serializable
//#define Parcelable
//#define Json

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System;

namespace PassComplexObjects
{
    [Activity(Label = "PassComplexObjects", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        int count = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += delegate
            {

                var watch = System.Diagnostics.Stopwatch.StartNew();

#if Parcelable
				EmployeeParcelable myEmployeeParcelable = new EmployeeParcelable(JobPosition.Operator, "Alan Lopez", "alan@mail.com");
				Intent secondActivityIntentParcelable = new Intent(this, typeof(SecondActivity));
                secondActivityIntentParcelable.PutExtra("employee", myEmployeeParcelable);
				StartActivity(secondActivityIntentParcelable);
#endif

#if Serializable
                EmployeeSerializable myEmployeeSerializable = new EmployeeSerializable(JobPosition.Operator, "Alan Lopez", "alan@mail.com");
				Intent secondActivityIntentSerializable = new Intent(this, typeof(SecondActivity));
				secondActivityIntentSerializable.PutExtra("employee", myEmployeeSerializable);
				StartActivity(secondActivityIntentSerializable);
#endif

#if Json
                EmployeeJson myEmployeeJson = new EmployeeJson(JobPosition.Operator, "Alan Lopez", "alan@mail.com");
				Intent secondActivityIntentJson = new Intent(this, typeof(SecondActivity));
				secondActivityIntentJson.PutExtra("employee", Newtonsoft.Json.JsonConvert.SerializeObject(myEmployeeJson));
				StartActivity(secondActivityIntentJson);
#endif
				


                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("Time in PutExtra");
                Console.WriteLine($"{elapsedMs} ms");
            };
        }
    }
}

