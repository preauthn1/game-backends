class MinerRebate {
  final String? minerId;
  final String? minerName;
  final String? percentage1;
  final String? percentage2;
  final String? percentage3;

  MinerRebate({
    required this.minerId,
    required this.minerName,
    required this.percentage1,
    required this.percentage2,
    required this.percentage3,
  });

  factory MinerRebate.fromJson(Map<String, dynamic> json) {
    return MinerRebate(
      minerId: json['miner_id']?.toString(),
      minerName: json['miner_name']?.toString(),
      percentage1: json['percentage1']?.toString(),
      percentage2: json['percentage2']?.toString(),
      percentage3: json['percentage3']?.toString(),
    );
  }
}
