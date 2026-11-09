namespace UniversityCafeteria.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string FullName  { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public string Position { get; set; }

        public decimal Salary { get; set; }

        public DateTime HireDate { get; set; }
        public ICollection<Order> orders { get; set; }

    }
}
