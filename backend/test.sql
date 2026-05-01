SELECT * FROM Users;

CREATE VIEW vw_AppointmentsWithUsers AS
SELECT 
    a.Id,
    a.Username,
    u.FullName,
    a.DogName,
    a.DogSize,
    a.Date,
    a.CreatedAt,
    a.Price,
    a.DurationMinutes
FROM Appointments a
INNER JOIN Users u ON a.Username = u.Username;
