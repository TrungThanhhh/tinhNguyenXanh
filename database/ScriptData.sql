INSERT INTO EventCategories (Name)
VALUES
(N'Môi trường'),
(N'Giáo dục'),
(N'Y tế'),
(N'Cộng đồng'),
(N'Trẻ em'),
(N'Người già'),
(N'Động vật'),
(N'Khác');



INSERT INTO Organizations (
    UserId, Name, OrganizationType, Description,
    AvatarUrl, FocusAreas, ContactEmail, PhoneNumber,
    Website, Address, City, District, Ward,
    TaxCode, FoundedDate, LegalRepresentative,
    VerificationDocsUrl, DocumentType, Verified, VerifiedDate,
    VerificationNotes, FacebookUrl, InstagramUrl, ZaloNumber,
    JoinedDate, IsActive
)
VALUES
-- 1. Vietnam Volunteer
(
    'e9ea7ece-f56c-4325-822c-1c5d27b45abb',
    N'Vietnam Volunteer',
    N'NGO',
    N'Tổ chức hoạt động vì môi trường: trồng cây, dọn rác, giáo dục môi trường và phát triển cộng đồng.',
    NULL,
    N'Môi trường, Giáo dục, Cộng đồng',
    'info@vietnamvolunteer.org.vn',
    '0866881370',
    'https://vietnamvolunteer.org.vn',
    N'Hồ Chí Minh, Việt Nam',
    N'Hồ Chí Minh',
    N'Quận 1',
    NULL,
    NULL,
    '2015-01-01',
    N'Nguyễn Văn A',
    NULL,
    NULL,
    1,
    GETDATE(),
    N'Tổ chức uy tín trong lĩnh vực môi trường',
    'https://facebook.com/vietnamvolunteer',
    NULL,
    NULL,
    GETDATE(),
    1
),

-- 2. SJ Vietnam
(
    '2',
    N'SJ Vietnam (Solidarités Jeunesses Vietnam)',
    N'NGO',
    N'Tổ chức thanh niên quốc tế hoạt động trong các dự án môi trường, giáo dục, phát triển cộng đồng.',
    NULL,
    N'Giáo dục, Môi trường, Thanh niên',
    'info@sjvietnam.org',
    '02437170544',
    'https://www.sjvietnam.org',
    N'Hà Nội, Việt Nam',
    N'Hà Nội',
    N'Quận Ba Đình',
    NULL,
    NULL,
    '2004-03-12',
    N'Trần B',
    NULL,
    NULL,
    1,
    GETDATE(),
    N'Đối tác quốc tế hoạt động lâu năm',
    'https://facebook.com/sjvietnam',
    NULL,
    NULL,
    GETDATE(),
    1
),

-- 3. Blue Dragon Children''s Foundation
(
    '3',
    N'Blue Dragon Children''s Foundation',
    N'NGO',
    N'Tổ chức bảo vệ trẻ em, chống buôn bán người, hỗ trợ trẻ em có hoàn cảnh khó khăn và tiếp cận giáo dục.',
    NULL,
    N'Trẻ em, Bảo vệ, Cộng đồng',
    'info@bluedragon.org',
    '02437170544',
    'https://bluedragon.org',
    N'96 Tô Ngọc Vân, Tây Hồ, Hà Nội',
    N'Hà Nội',
    N'Quận Tây Hồ',
    NULL,
    NULL,
    '2003-09-01',
    N'Michael Brosowski',
    NULL,
    NULL,
    1,
    GETDATE(),
    N'Tổ chức quốc tế uy tín hỗ trợ trẻ em Việt Nam',
    'https://facebook.com/bluedragonchildren',
    NULL,
    NULL,
	GETDATE(),
	1
),

-- 4. Saigon Children''s Charity (Saigonchildren)
(
    '4',
    N'Saigon Children''s Charity',
    N'NGO',
    N'Hỗ trợ trẻ em có hoàn cảnh khó khăn thông qua giáo dục, học bổng, xây trường và đào tạo nghề.',
    NULL,
    N'Giáo dục, Trẻ em, Cộng đồng',
    'info@saigonchildren.com',
    '02839303502',
    'https://saigonchildren.com',
    N'59 Trần Quốc Thảo, Phường 7, Quận 3, TP. Hồ Chí Minh',
    N'Hồ Chí Minh',
    N'Quận 3',
    N'Phường 7',
    NULL,
    '1992-01-01',
    N'Huỳnh C',
    NULL,
    NULL,
    1,
    GETDATE(),
    N'Tổ chức giáo dục lâu đời tại Việt Nam',
    'https://facebook.com/saigonchildren',
    NULL,
    NULL,
	GETDATE(),
	1
),

-- 5. Hanoikids Club
(
    '5',
    N'Hanoikids Voluntary English Club',
    N'CLB Tình nguyện',
    N'Câu lạc bộ tình nguyện hướng dẫn viên tiếng Anh miễn phí cho khách quốc tế, quảng bá văn hóa Việt Nam.',
    NULL,
    N'Văn hóa, Giáo dục, Du lịch',
    'contact@hanoikids.org',
    '0901234567',
    'https://hanoikids.org',
    N'Hà Nội, Việt Nam',
    N'Hà Nội',
    N'Quận Hoàn Kiếm',
    NULL,
    NULL,
    '2006-04-01',
    N'Lê D',
    NULL,
    NULL,
    1,
    GETDATE(),
    N'Câu lạc bộ hướng dẫn viên trẻ uy tín',
    'https://facebook.com/hanoikids',
    NULL,
    NULL,
	GETDATE(),
	1
);


INSERT INTO Events
(Title, Description, StartTime, EndTime, Location, MaxVolunteers, OrganizationId, CategoryId, Status, Images, IsHidden)
VALUES
-- Workcamp môi trường
(N'Workcamp Bảo vệ môi trường Cát Bà',
 N'Dọn rác, bảo vệ thiên nhiên tại đảo Cát Bà.',
 '2025-06-01', '2025-06-07',
 N'Cát Bà, Hải Phòng', 50, 9, 1, 'planned', NULL, 0),

-- Dự án giáo dục cho trẻ em
(N'Lớp học kỹ năng sống cho trẻ em',
 N'Dạy kỹ năng sống, tiếng Anh cho trẻ em khó khăn.',
 '2025-03-01', '2025-05-31',
 N'Hà Nội', 30, 9, 2, 'completed', NULL, 0),

-- Chương trình y tế cộng đồng
(N'Khám bệnh & tư vấn sức khỏe miễn phí',
 N'Tổ chức khám bệnh, tư vấn sức khỏe cho người dân vùng sâu vùng xa.',
 '2025-04-10', '2025-04-12',
 N'Hà Giang', 20, 9, 3, 'completed', NULL, 0),

-- Hỗ trợ cộng đồng
(N'Tặng quà người già neo đơn dịp Tết',
 N'Tặng quà, chăm sóc người già neo đơn dịp Tết Nguyên Đán.',
 '2025-01-18', '2025-01-25',
 N'Hà Nội', 40, 9, 4, 'completed', NULL, 0),

-- Chương trình chăm sóc trẻ em
(N'Trung Thu vui vẻ – SJ Vietnam',
 N'Vui chơi, tặng quà cho trẻ em nghèo dịp Trung Thu.',
 '2025-09-28', '2025-09-29',
 N'Hà Nội', 50, 9, 5, 'planned', NULL, 0);
