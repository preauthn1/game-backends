class MyMiner {
  final String? mmId;
  final String? name;
  final String? image;
  final String? type;
  final String? createdAt;
  final String? endTime;
  final String? sellbackStart;

  MyMiner({
    required this.mmId,
    required this.name,
    required this.image,
    required this.type,
    required this.createdAt,
    required this.endTime,
    required this.sellbackStart,
  });

  factory MyMiner.fromJson(Map<String, dynamic> json) {
    return MyMiner(
      mmId: json['mm_id']?.toString(),
      name: json['name']?.toString(),
      image: json['image']?.toString(),
      type: json['type']?.toString(),
      createdAt: json['created_at']?.toString(),
      endTime: json['end_time']?.toString(),
      sellbackStart: json['sellback_start']?.toString(),
    );
  }
}
