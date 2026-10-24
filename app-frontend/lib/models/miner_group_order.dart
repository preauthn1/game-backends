class MinerGroupOrder {
  String? mmId;
  String? username;
  String? type;

  MinerGroupOrder({
    this.mmId,
    this.username,
    this.type,
  });

  factory MinerGroupOrder.fromJson(Map<String, dynamic> json) =>
      MinerGroupOrder(
        mmId: json['mm_id']?.toString(),
        username: json['username']?.toString(),
        type: json['type']?.toString(),
      );
}
