using System;
namespace PassComplexObjects
{
    public class EmployeeJson
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


		public EmployeeJson(JobPosition position, string name, string email)
		{
			Name = name;
			Position = position;
			Email = email;
		}

		
    }
}
