
USE TaskManagerDb;
GO

INSERT INTO Tasks (Title, Description, DueDate, Status, CreatedBy, AssignedTo)
VALUES 
('Implement login', 'OAuth2 authentication with Microsoft', '2025-07-10', 'Pending', 'admin', 'martin'),
('Create dashboard UI', 'React Bootstrap layout', '2025-07-12', 'In Progress', 'admin', 'alice'),
('Deploy backend', 'Publish Azure Functions', '2025-07-15', 'Done', 'martin', 'bob');
