using System;
using Android.OS;
using Android.Runtime;
using Java.Interop;

namespace PassComplexObjects
{
	public enum JobPosition
	{
		Excecutive,
		Operator,
		Supervisor
	}


    public class EmployeeParcelable: Java.Lang.Object, IParcelable
    {
		public string Name
		{
			get;
			set;
		}

	
		public JobPosition Position
		{
			get;
			set;
		}

		
	
		public string Email
		{
			get;
			set;
		}

       
        public EmployeeParcelable(JobPosition position, string name,  string email)
		{
			Name = name;
			Position = position;
			Email = email;
		}

        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
        {
            dest.WriteInt((int)Position);
            dest.WriteString(Name);
            dest.WriteString(Email);
        }

		[ExportField("CREATOR")]
        public static EmployeeParcelableCreator InititalizeCreator()
		{
			return new EmployeeParcelableCreator();
		}
    }
}
