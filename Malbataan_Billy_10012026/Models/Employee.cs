namespace Malbataan_Billy_10012026.Models
{
    /// <summary>
    /// Represents an employee with identification, contact, compensation, and hire date information.
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Unique identifier for the employee.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Full name of the employee.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Department or team to which the employee belongs.
        /// </summary>
        public string Department { get; set; } 

        /// <summary>
        /// Employee's email address used for contact.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Employee's salary expressed in the application's currency units.
        /// </summary>
        public double Salary { get; set; }

        /// <summary>
        /// Date the employee was hired. Stored as a string; prefer ISO-8601 format (e.g., "2026-10-02").
        /// </summary>
        public string HiredDate { get; set; }
    }
}
