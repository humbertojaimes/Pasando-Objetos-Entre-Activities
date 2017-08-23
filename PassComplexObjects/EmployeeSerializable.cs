using System;
using Android.Runtime;
using Java.Interop;
using Java.IO;

namespace PassComplexObjects
{
    public class EmployeeSerializable : Java.Lang.Object, ISerializable
    {

		public EmployeeSerializable(IntPtr handle, JniHandleOwnership transfer)
            : base (handle, transfer)
        {
		}

        [Export("readObject", Throws = new[] {
        typeof (Java.IO.IOException),
        typeof (Java.Lang.ClassNotFoundException)})]
        private void ReadObject(Java.IO.ObjectInputStream source)
        {
            Position = (JobPosition)ReadInt(source);
            Name = ReadNullableString(source);
            Email = ReadNullableString(source);
        }

        [Export("writeObject", Throws = new[] {
        typeof (Java.IO.IOException),
        typeof (Java.Lang.ClassNotFoundException)})]
        private void WriteObject(Java.IO.ObjectOutputStream destination)
        {
            WriteInt(destination, (int)Position);
            WriteNullableString(destination, Name);
            WriteNullableString(destination, Email);
        }

		static void WriteNullableString(Java.IO.ObjectOutputStream dest, string value)
		{
			dest.WriteBoolean(value != null);
			if (value != null)
				dest.WriteUTF(value);
		}

		static void WriteInt(Java.IO.ObjectOutputStream dest, int value)
		{	
            dest.WriteInt(value);
		}

		static string ReadNullableString(Java.IO.ObjectInputStream source)
		{
			if (source.ReadBoolean())
				return source.ReadUTF();
			return null;
		}

		static int ReadInt(Java.IO.ObjectInputStream source)
		{
            return source.ReadInt();
		}

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


        public EmployeeSerializable(JobPosition position, string name, string email)
        {
            Name = name;
            Position = position;
            Email = email;
        }


    }
}
