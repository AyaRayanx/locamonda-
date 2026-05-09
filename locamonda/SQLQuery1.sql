-- الحالة الأولى: طلب حجز جديد (Pending)
INSERT INTO Bookings (StartDate, EndDate, Status, TotalPrice, CreatedAt, UserId, PropertyId, ConfirmedAt, IsDone)
VALUES (
    '2026-06-01 12:00:00', -- StartDate (في المستقبل)
    '2026-06-05 12:00:00', -- EndDate
    'Pending',             -- الحالة منتظر
    14000.00,              -- السعر
    '2026-05-09 10:00:00', -- تاريخ النهاردة
    '1',                   -- UserId (غيريه لليوزر الكاستومر عندك)
    2,                     -- PropertyId (غيريه لرقم العقار المملوك للأونر)
    NULL,                  -- لسه مفيش موافقة
    0                      -- IsDone = False
);

-- الحالة الثانية: حجز مؤكد (Confirmed) ومستني الـ Done
INSERT INTO Bookings (StartDate, EndDate, Status, TotalPrice, CreatedAt, UserId, PropertyId, ConfirmedAt, IsDone)
VALUES (
    '2026-05-15 12:00:00', -- StartDate
    '2026-05-20 12:00:00', -- EndDate
    'Confirmed',           -- الحالة مؤكد
    42500.00, 
    '2026-05-08 09:00:00', 
    '1', 
    4,                     -- PropertyId (اتأكدي إن الـ Id ده موجود)
    '2026-05-09 11:00:00', -- اتوافق عليه النهاردة
    0                      -- IsDone = False (لسه مخلصش)
);