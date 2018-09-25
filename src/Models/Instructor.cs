using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace Workforce.Models
{
    public class Instructor
    {
        public int Id { get; set; }

		[Required]
		[Display(Name="First Name")]
        public string FirstName { get; set; }

		[Required]
		[Display(Name="Last Name")]
        public string LastName { get; set; }

		[Required]
		[Display(Name="Slack Handle")]
		[MaxLength(20)]
        public string SlackHandle { get; set; }

        public string Specialty { get; set; }

		[Required]
		public int CohortId { get; set; }

        public Cohort Cohort { get; set; }

		[Display(Name = "Instructor Name")]
		public string FullName
		{
			get
			{
				return $"{FirstName} {LastName}";
			}
		}

    }
}
