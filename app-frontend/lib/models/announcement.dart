class Announcement {
  final String? announcementId;
  final String? announcementType;
  final String? announcementTypeText;
  final String? announcementTitle;
  final String? announcementContent;
  final String? startTime;
  final String? endTime;
  final String? isRead;

  Announcement({
    this.announcementId,
    this.announcementType,
    this.announcementTypeText,
    this.announcementTitle,
    this.announcementContent,
    this.startTime,
    this.endTime,
    this.isRead,
  });

  factory Announcement.fromJson(Map<String, dynamic> json) {
    return Announcement(
      announcementId: json['announcement_id']?.toString(),
      announcementType: json['announcement_type']?.toString(),
      announcementTypeText: json['announcement_type_text']?.toString(),
      announcementTitle: json['announcement_title']?.toString(),
      announcementContent: json['announcement_content']?.toString(),
      startTime: json['start_time']?.toString(),
      endTime: json['end_time']?.toString(),
      isRead: json['is_read']?.toString(),
    );
  }
}
