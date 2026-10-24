class Avatar {
  final String? id;
  final String? image;
  final String? status;

  Avatar({
    required this.id,
    required this.image,
    required this.status,
  });

  factory Avatar.fromJson(Map<String, dynamic> json) {
    return Avatar(
      id: json['id']?.toString(),
      image: json['image']?.toString(),
      status: json['status']?.toString(),
    );
  }
}
