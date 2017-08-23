
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
using Java.Lang;

namespace PassComplexObjects
{

    public class EmployeeParcelableCreator : Java.Lang.Object, IParcelableCreator
	{
		public Java.Lang.Object CreateFromParcel(Parcel source)
		{
            return new EmployeeParcelable((JobPosition)source.ReadInt(),source.ReadString(), source.ReadString());
		}

		public Java.Lang.Object[] NewArray(int size)
		{
			return new Java.Lang.Object[size];
		}


    }
}
