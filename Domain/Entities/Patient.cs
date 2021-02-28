namespace Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Domain.Enums;
    using Sieve.Attributes;

    [Table("Patient")]
    public class Patient
    {
        private Sex _sex = Enums.Sex.Unknown;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        [Sieve(CanFilter = true, CanSort = true)]
        public Guid PatientId { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string ExternalId { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string InternalId { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string FirstName { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string LastName { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public DateTimeOffset? Dob { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string Sex 
        {
            get => Enum.GetName(typeof(Sex), _sex);
            set
            {
                if(value.Trim().Equals("Male", StringComparison.InvariantCultureIgnoreCase) || value.Trim().Equals("M", StringComparison.InvariantCultureIgnoreCase))
                    _sex = Enums.Sex.Male;
                else if (value.Trim().Equals("Female", StringComparison.InvariantCultureIgnoreCase) || value.Trim().Equals("F", StringComparison.InvariantCultureIgnoreCase))
                    _sex = Enums.Sex.Female;
                else
                    _sex = Enums.Sex.Unknown;
            }
        }

        [Sieve(CanFilter = true, CanSort = true)]
        public string Gender { get; set; }

        [Sieve(CanFilter = true, CanSort = true)]
        public string Race { get; set; } // make a separate class

        [Sieve(CanFilter = true, CanSort = true)]
        public string Ethnicity { get; set; } // make a separate class

        // add-on property marker - Do Not Delete This Comment
    }
}