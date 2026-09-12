-- Leo Education sample data for Supabase SQL Editor.
-- Safe to run more than once: rows are upserted by primary key.

BEGIN;

INSERT INTO "Users" ("userId", "fullName", email, phone, "passwordHash", "avatarURL", status, "createdAt", "updatedAt")
VALUES
  (1, 'Local Admin', 'admin@leo.local', '0900000000', '$2a$11$0j5JGEtyAplzLX41uaUJ/u6EbqCXUXYM19/5ZMXA6j0I/3qBEeY12', NULL, 'Active', '2026-08-01 08:00:00', '2026-08-01 08:00:00')
ON CONFLICT ("userId") DO UPDATE SET
  "fullName" = EXCLUDED."fullName",
  email = EXCLUDED.email,
  phone = EXCLUDED.phone,
  "passwordHash" = EXCLUDED."passwordHash",
  status = EXCLUDED.status,
  "updatedAt" = EXCLUDED."updatedAt";

INSERT INTO "Subjects" ("subjectId", "hashCode", "subjectName", description, "imageUrl", "isActive", "createdAt", "updatedAt")
VALUES
  (1, 'subject-1', 'Tiếng Anh', 'Giao tiếp, phát âm, IELTS và tiếng Anh học thuật.', NULL, TRUE, '2026-08-01 08:00:00', '2026-08-01 08:00:00'),
  (2, 'subject-2', 'Toán', 'Toán tư duy, toán nền tảng và luyện thi chuyển cấp.', NULL, TRUE, '2026-08-01 08:00:00', '2026-08-01 08:00:00'),
  (3, 'subject-3', 'Vật lý', 'Củng cố kiến thức vật lý THCS/THPT qua bài tập thực hành.', NULL, TRUE, '2026-08-01 08:00:00', '2026-08-01 08:00:00'),
  (4, 'subject-4', 'Hóa học', 'Nắm chắc lý thuyết, phản ứng hóa học và kỹ năng giải đề.', NULL, TRUE, '2026-08-01 08:00:00', '2026-08-01 08:00:00'),
  (5, 'subject-5', 'Ngữ văn', 'Rèn đọc hiểu, nghị luận xã hội và nghị luận văn học.', NULL, TRUE, '2026-08-01 08:00:00', '2026-08-01 08:00:00'),
  (6, 'subject-6', 'Sinh học', 'Hệ thống kiến thức sinh học và luyện bài tập theo chuyên đề.', NULL, TRUE, '2026-08-01 08:00:00', '2026-08-01 08:00:00')
ON CONFLICT ("subjectId") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "subjectName" = EXCLUDED."subjectName",
  description = EXCLUDED.description,
  "imageUrl" = EXCLUDED."imageUrl",
  "isActive" = EXCLUDED."isActive",
  "updatedAt" = EXCLUDED."updatedAt";

INSERT INTO "Instructors" ("Id", "hashCode", "FullName", "Role", "Bio", "AvatarUrl", "Rating", "Experience", "IsActive")
VALUES
  (1, 'instructor-1', 'Nguyễn Minh Anh', 'Giảng viên IELTS', 'Chuyên luyện nền tảng phát âm, giao tiếp và IELTS Foundation cho học sinh mới bắt đầu.', NULL, 5.00, '6 năm', TRUE),
  (2, 'instructor-2', 'Trần Quốc Bảo', 'Giảng viên Toán', 'Tập trung phương pháp giải nhanh, tư duy logic và hệ thống lỗ hổng kiến thức.', NULL, 4.80, '8 năm', TRUE),
  (3, 'instructor-3', 'Lê Hoàng Phúc', 'Giảng viên Vật lý', 'Dạy học qua thí nghiệm mô phỏng và bài tập ứng dụng thực tế.', NULL, 4.70, '5 năm', TRUE),
  (4, 'instructor-4', 'Phạm Thu Hà', 'Giảng viên Ngữ văn', 'Hướng dẫn học sinh viết bài mạch lạc, có luận điểm và cảm nhận cá nhân.', NULL, 4.90, '7 năm', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "FullName" = EXCLUDED."FullName",
  "Role" = EXCLUDED."Role",
  "Bio" = EXCLUDED."Bio",
  "AvatarUrl" = EXCLUDED."AvatarUrl",
  "Rating" = EXCLUDED."Rating",
  "Experience" = EXCLUDED."Experience",
  "IsActive" = EXCLUDED."IsActive";

INSERT INTO "Courses" ("courseId", "hashCode", "courseName", description, "imageUrl", "subjectId", "instructorId", price, "billingType", "startDate", "endDate", "createdAt", "updatedAt")
VALUES
  (1, 'course-1', 'Tiếng Anh giao tiếp căn bản', 'Khóa học giúp học viên tự tin giao tiếp trong các tình huống hằng ngày.', NULL, 1, 1, 1500000, 'Monthly', '2026-09-01 00:00:00', '2026-11-30 00:00:00', '2026-08-01 09:00:00', '2026-08-01 09:00:00'),
  (2, 'course-2', 'IELTS Foundation', 'Xây nền từ vựng, ngữ pháp và kỹ năng nghe nói đọc viết cho mục tiêu IELTS 4.5-5.5.', NULL, 1, 1, 3200000, 'FullCourse', '2026-09-15 00:00:00', '2026-12-15 00:00:00', '2026-08-01 09:10:00', '2026-08-01 09:10:00'),
  (3, 'course-3', 'Toán tư duy lớp 6-7', 'Rèn tư duy phân tích, kỹ năng trình bày và giải toán nâng cao.', NULL, 2, 2, 2200000, 'Monthly', '2026-09-05 00:00:00', '2026-11-05 00:00:00', '2026-08-01 09:20:00', '2026-08-01 09:20:00'),
  (4, 'course-4', 'Vật lý mất gốc THCS', 'Ôn lại công thức trọng tâm, phương pháp đọc đề và giải bài tập cơ bản.', NULL, 3, 3, 1800000, 'Monthly', '2026-09-10 00:00:00', '2026-11-10 00:00:00', '2026-08-01 09:30:00', '2026-08-01 09:30:00'),
  (5, 'course-5', 'Ngữ văn luyện thi vào 10', 'Ôn đọc hiểu, nghị luận xã hội và nghị luận văn học theo cấu trúc đề thi.', NULL, 5, 4, 2800000, 'FullCourse', '2026-09-12 00:00:00', '2026-12-12 00:00:00', '2026-08-01 09:40:00', '2026-08-01 09:40:00')
ON CONFLICT ("courseId") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "courseName" = EXCLUDED."courseName",
  description = EXCLUDED.description,
  "imageUrl" = EXCLUDED."imageUrl",
  "subjectId" = EXCLUDED."subjectId",
  "instructorId" = EXCLUDED."instructorId",
  price = EXCLUDED.price,
  "billingType" = EXCLUDED."billingType",
  "startDate" = EXCLUDED."startDate",
  "endDate" = EXCLUDED."endDate",
  "updatedAt" = EXCLUDED."updatedAt";

INSERT INTO "Students" ("studentId", "hashCode", "fullName", email, phone, note, status, "createdAt", "updatedAt")
VALUES
  (1, 'student-1', 'Nguyễn Văn A', 'student1@example.com', '0900000001', 'Đã xếp lớp tiếng Anh giao tiếp.', 'Active', '2026-08-02 09:00:00', '2026-08-02 09:00:00'),
  (2, 'student-2', 'Trần Thị B', 'student2@example.com', '0900000002', 'Chưa xếp lớp, đang chờ tư vấn lịch học.', 'Active', '2026-08-03 10:00:00', '2026-08-03 10:00:00'),
  (3, 'student-3', 'Võ Ngọc E', 'student5@example.com', '0900000005', 'Đã xếp lớp Toán tư duy.', 'Active', '2026-08-04 16:00:00', '2026-08-04 16:00:00'),
  (4, 'student-4', 'Hoàng Kim H', 'student8@example.com', '0900000008', 'Đã đóng trọn khóa IELTS Foundation.', 'Active', '2026-08-05 09:00:00', '2026-08-05 09:00:00'),
  (5, 'student-5', 'Ngô Bảo L', 'student11@example.com', '0900000011', 'Học thử đạt yêu cầu, đã chuyển thành học viên.', 'Active', '2026-08-06 13:00:00', '2026-08-06 13:00:00')
ON CONFLICT ("studentId") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "fullName" = EXCLUDED."fullName",
  email = EXCLUDED.email,
  phone = EXCLUDED.phone,
  note = EXCLUDED.note,
  status = EXCLUDED.status,
  "updatedAt" = EXCLUDED."updatedAt";

INSERT INTO "CourseRegistrations" ("registrationId", "hashCode", "fullName", email, phone, "courseId", "studentId", status, source, note, "paymentMode", "paidAmount", "lastPaymentAt", "tuitionNote", "createdAt")
VALUES
  (1, 'registration-1', 'Nguyễn Văn A', 'student1@example.com', '0900000001', 1, 1, 'Đã nhập học', 'Website', 'Đã xếp lớp mẫu.', 'Monthly', 1500000, '2026-08-02 09:00:00', 'Đã đóng tháng đầu tiên.', '2026-08-02 09:00:00'),
  (2, 'registration-2', 'Trần Thị B', 'student2@example.com', '0900000002', 1, 2, 'Đã gọi', 'Facebook', 'Phụ huynh cần lịch tối.', 'Monthly', 0, NULL, 'Chưa thanh toán.', '2026-08-03 10:00:00'),
  (3, 'registration-3', 'Lê Hoàng C', 'student3@example.com', '0900000003', 2, NULL, 'Mới', 'Zalo', 'Cần tư vấn lộ trình IELTS.', NULL, 0, NULL, NULL, '2026-08-04 08:00:00'),
  (4, 'registration-4', 'Phạm Minh D', 'student4@example.com', '0900000004', 2, NULL, 'Đã gọi', 'Facebook', 'Hẹn gọi lại cuối tuần.', NULL, 0, NULL, NULL, '2026-08-05 14:00:00'),
  (5, 'registration-5', 'Võ Ngọc E', 'student5@example.com', '0900000005', 3, 3, 'Đã nhập học', 'Giới thiệu', 'Đã xếp lớp Toán tư duy.', 'Monthly', 4400000, '2026-08-04 16:00:00', 'Đã đóng 2 tháng.', '2026-08-04 16:00:00'),
  (6, 'registration-6', 'Đặng Gia F', 'student6@example.com', '0900000006', 4, NULL, 'Mới', 'Website', 'Lead mới từ website.', NULL, 0, NULL, NULL, '2026-08-07 11:00:00'),
  (7, 'registration-7', 'Bùi Anh G', 'student7@example.com', '0900000007', 5, NULL, 'Đã hủy', 'Zalo', 'Chưa sắp xếp được thời gian học.', NULL, 0, NULL, NULL, '2026-08-08 15:00:00'),
  (8, 'registration-8', 'Hoàng Kim H', 'student8@example.com', '0900000008', 2, 4, 'Đã nhập học', 'Website', 'Đã xếp lớp IELTS Foundation.', 'FullCourse', 3200000, '2026-08-05 09:00:00', 'Đã đóng trọn khóa.', '2026-08-05 09:00:00'),
  (9, 'registration-9', 'Mai Tuấn I', 'student9@example.com', '0900000009', 1, NULL, 'Mới', 'Facebook', 'Cần kiểm tra đầu vào.', NULL, 0, NULL, NULL, '2026-08-09 17:00:00'),
  (10, 'registration-10', 'Đỗ Thanh K', 'student10@example.com', '0900000010', 3, NULL, 'Đã gọi', 'Giới thiệu', 'Đang cân nhắc học phí.', NULL, 0, NULL, NULL, '2026-08-10 10:00:00')
ON CONFLICT ("registrationId") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "fullName" = EXCLUDED."fullName",
  email = EXCLUDED.email,
  phone = EXCLUDED.phone,
  "courseId" = EXCLUDED."courseId",
  "studentId" = EXCLUDED."studentId",
  status = EXCLUDED.status,
  source = EXCLUDED.source,
  note = EXCLUDED.note,
  "paymentMode" = EXCLUDED."paymentMode",
  "paidAmount" = EXCLUDED."paidAmount",
  "lastPaymentAt" = EXCLUDED."lastPaymentAt",
  "tuitionNote" = EXCLUDED."tuitionNote";

INSERT INTO "TeachingClasses" ("classId", "hashCode", "className", "courseId", "subjectId", "instructorId", "startDate", "endDate", status, note, "createdAt", "updatedAt")
VALUES
  (1, 'class-1', 'TA-GT-01', 1, 1, 1, '2026-09-01 18:00:00', '2026-11-30 20:00:00', 'Active', 'Lớp giao tiếp tối thứ 2-4-6.', '2026-08-11 08:00:00', '2026-08-11 08:00:00'),
  (2, 'class-2', 'IELTS-FD-01', 2, 1, 1, '2026-09-15 18:30:00', '2026-12-15 20:30:00', 'Active', 'Lớp IELTS Foundation tối thứ 3-5.', '2026-08-11 08:10:00', '2026-08-11 08:10:00'),
  (3, 'class-3', 'TOAN-TD-01', 3, 2, 2, '2026-09-05 17:30:00', '2026-11-05 19:00:00', 'Active', 'Lớp Toán tư duy cuối tuần.', '2026-08-11 08:20:00', '2026-08-11 08:20:00'),
  (4, 'class-4', 'VLY-MG-01', 4, 3, 3, '2026-09-10 19:00:00', '2026-11-10 20:30:00', 'Planning', 'Lớp đang gom học viên.', '2026-08-11 08:30:00', '2026-08-11 08:30:00')
ON CONFLICT ("classId") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "className" = EXCLUDED."className",
  "courseId" = EXCLUDED."courseId",
  "subjectId" = EXCLUDED."subjectId",
  "instructorId" = EXCLUDED."instructorId",
  "startDate" = EXCLUDED."startDate",
  "endDate" = EXCLUDED."endDate",
  status = EXCLUDED.status,
  note = EXCLUDED.note,
  "updatedAt" = EXCLUDED."updatedAt";

INSERT INTO "TeachingClassStudents" ("classId", "registrationId", "createdAt")
VALUES
  (1, 1, '2026-08-12 09:00:00'),
  (2, 8, '2026-08-12 09:10:00'),
  (3, 5, '2026-08-12 09:20:00')
ON CONFLICT ("classId", "registrationId") DO UPDATE SET
  "createdAt" = EXCLUDED."createdAt";

INSERT INTO "ConsultationLogs" ("consultationLogId", "registrationId", "contactedAt", channel, "staffName", result, note, "createdAt")
VALUES
  (1, 3, '2026-08-04 09:00:00', 'Zalo', 'Tư vấn viên A', 'Cần tư vấn thêm', 'Phụ huynh hỏi lịch buổi tối và học phí.', '2026-08-04 09:05:00'),
  (2, 4, '2026-08-05 15:00:00', 'Điện thoại', 'Tư vấn viên B', 'Hẹn gọi lại', 'Cuối tuần phụ huynh rảnh hơn.', '2026-08-05 15:05:00'),
  (3, 10, '2026-08-10 11:00:00', 'Điện thoại', 'Tư vấn viên A', 'Đang cân nhắc', 'Gửi thêm thông tin học phí và lịch lớp.', '2026-08-10 11:05:00')
ON CONFLICT ("consultationLogId") DO UPDATE SET
  "registrationId" = EXCLUDED."registrationId",
  "contactedAt" = EXCLUDED."contactedAt",
  channel = EXCLUDED.channel,
  "staffName" = EXCLUDED."staffName",
  result = EXCLUDED.result,
  note = EXCLUDED.note,
  "createdAt" = EXCLUDED."createdAt";

INSERT INTO "ContactRequests" ("Id", "hashCode", "FullName", "Email", "Phone", "Message", "Status", "CreatedAt", "UpdatedAt")
VALUES
  (1, 'contact-1', 'Nguyễn Phụ Huynh', 'parent1@example.com', '0911000001', 'Tôi muốn được tư vấn khóa tiếng Anh giao tiếp cho con lớp 7.', 'New', '2026-08-13 08:00:00', '2026-08-13 08:00:00'),
  (2, 'contact-2', 'Trần Minh Tú', 'tuminh@example.com', '0911000002', 'Cho tôi xin thông tin học phí lớp IELTS Foundation.', 'Processing', '2026-08-13 09:00:00', '2026-08-13 10:00:00'),
  (3, 'contact-3', 'Lê Hoài Nam', 'namlh@example.com', '0911000003', 'Trung tâm còn lớp toán cuối tuần không?', 'Resolved', '2026-08-14 14:00:00', '2026-08-14 16:00:00')
ON CONFLICT ("Id") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "FullName" = EXCLUDED."FullName",
  "Email" = EXCLUDED."Email",
  "Phone" = EXCLUDED."Phone",
  "Message" = EXCLUDED."Message",
  "Status" = EXCLUDED."Status",
  "UpdatedAt" = EXCLUDED."UpdatedAt";

INSERT INTO "Testimonials" ("testimonialId", "hashCode", "studentName", "jobTitle", content, rating, "avatarURL", "isActive", "createdAt")
VALUES
  (1, 'testimonial-1', 'Nguyễn Văn A', 'Học viên tiếng Anh', 'Sau 2 tháng học, em tự tin nói tiếng Anh hơn và không còn sợ phát âm sai.', 5, NULL, TRUE, '2026-08-15 09:00:00'),
  (2, 'testimonial-2', 'Hoàng Kim H', 'Học viên IELTS Foundation', 'Giáo viên theo sát từng buổi, bài tập rõ ràng và dễ tự ôn ở nhà.', 5, NULL, TRUE, '2026-08-15 10:00:00'),
  (3, 'testimonial-3', 'Võ Ngọc E', 'Học viên Toán tư duy', 'Em hiểu cách phân tích đề hơn, không còn làm bài theo kiểu đoán công thức.', 4, NULL, TRUE, '2026-08-15 11:00:00')
ON CONFLICT ("testimonialId") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "studentName" = EXCLUDED."studentName",
  "jobTitle" = EXCLUDED."jobTitle",
  content = EXCLUDED.content,
  rating = EXCLUDED.rating,
  "avatarURL" = EXCLUDED."avatarURL",
  "isActive" = EXCLUDED."isActive",
  "createdAt" = EXCLUDED."createdAt";

INSERT INTO "Blogs" ("Id", "hashCode", "Title", "Summary", "Content", "ImageUrl", "Author", "CreatedAt")
VALUES
  (1, 'blog-1', 'Cách xây dựng thói quen học tiếng Anh mỗi ngày', 'Một số gợi ý đơn giản giúp học viên duy trì việc học tiếng Anh đều đặn.', 'Học ngoại ngữ cần sự đều đặn hơn là học dồn. Học viên có thể bắt đầu bằng 15 phút nghe, 10 phút ghi từ mới và 5 phút nói lại nội dung đã nghe. Khi thói quen ổn định, việc tăng thời lượng học sẽ nhẹ nhàng hơn.', NULL, 'Leo Education', '2026-08-16 08:00:00'),
  (2, 'blog-2', 'Làm sao để học sinh bớt sợ môn Toán?', 'Tập trung vào nền tảng, cách đọc đề và tư duy từng bước.', 'Nhiều học sinh sợ Toán vì mất gốc ở những kiến thức nhỏ. Việc học nên bắt đầu từ việc nhận diện dạng bài, viết lại dữ kiện và trình bày từng bước giải. Khi học sinh hiểu quy trình, sự tự tin sẽ tăng dần.', NULL, 'Leo Education', '2026-08-16 09:00:00'),
  (3, 'blog-3', 'Chuẩn bị gì trước khi học IELTS Foundation?', 'Checklist ngắn cho học viên bắt đầu lộ trình IELTS.', 'Trước khi học IELTS, học viên nên kiểm tra lại ngữ pháp nền tảng, vốn từ theo chủ đề quen thuộc và khả năng nghe các đoạn hội thoại ngắn. Một lộ trình phù hợp sẽ giúp tránh học quá sức ngay từ đầu.', NULL, 'Leo Education', '2026-08-16 10:00:00')
ON CONFLICT ("Id") DO UPDATE SET
  "hashCode" = EXCLUDED."hashCode",
  "Title" = EXCLUDED."Title",
  "Summary" = EXCLUDED."Summary",
  "Content" = EXCLUDED."Content",
  "ImageUrl" = EXCLUDED."ImageUrl",
  "Author" = EXCLUDED."Author",
  "CreatedAt" = EXCLUDED."CreatedAt";

SELECT setval(pg_get_serial_sequence('"Users"', 'userId'), COALESCE((SELECT MAX("userId") FROM "Users"), 1), true);
SELECT setval(pg_get_serial_sequence('"Subjects"', 'subjectId'), COALESCE((SELECT MAX("subjectId") FROM "Subjects"), 1), true);
SELECT setval(pg_get_serial_sequence('"Instructors"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Instructors"), 1), true);
SELECT setval(pg_get_serial_sequence('"Courses"', 'courseId'), COALESCE((SELECT MAX("courseId") FROM "Courses"), 1), true);
SELECT setval(pg_get_serial_sequence('"Students"', 'studentId'), COALESCE((SELECT MAX("studentId") FROM "Students"), 1), true);
SELECT setval(pg_get_serial_sequence('"CourseRegistrations"', 'registrationId'), COALESCE((SELECT MAX("registrationId") FROM "CourseRegistrations"), 1), true);
SELECT setval(pg_get_serial_sequence('"TeachingClasses"', 'classId'), COALESCE((SELECT MAX("classId") FROM "TeachingClasses"), 1), true);
SELECT setval(pg_get_serial_sequence('"ConsultationLogs"', 'consultationLogId'), COALESCE((SELECT MAX("consultationLogId") FROM "ConsultationLogs"), 1), true);
SELECT setval(pg_get_serial_sequence('"ContactRequests"', 'Id'), COALESCE((SELECT MAX("Id") FROM "ContactRequests"), 1), true);
SELECT setval(pg_get_serial_sequence('"Testimonials"', 'testimonialId'), COALESCE((SELECT MAX("testimonialId") FROM "Testimonials"), 1), true);
SELECT setval(pg_get_serial_sequence('"Blogs"', 'Id'), COALESCE((SELECT MAX("Id") FROM "Blogs"), 1), true);

COMMIT;
