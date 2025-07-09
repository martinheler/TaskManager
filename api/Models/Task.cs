namespace TaskManager.api.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string CreatedBy { get; set; } = "";
    public string AssignedTo { get; set; } = "";
}
