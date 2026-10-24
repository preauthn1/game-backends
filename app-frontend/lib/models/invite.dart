class Invite {
  final String? inviteCode;
  final List<InviteRebate>? rebate;
  final List<InviteReward>? reward;

  Invite({
    this.inviteCode,
    this.rebate,
    this.reward,
  });

  factory Invite.fromJson(Map<String, dynamic> json) {
    return Invite(
      inviteCode: json['invite_code']?.toString(),
      rebate: List<InviteRebate>.from(
        json['rebate'].map((item) => InviteRebate.fromJson(item)),
      ),
      reward: List<InviteReward>.from(
        json['reward'].map((item) => InviteReward.fromJson(item)),
      ),
    );
  }
}

class InviteRebate {
  final String? minerId;
  final String? minerName;
  final String? minerQuantity;
  final String? minerRebate;
  final String? salesAmount;
  final String? level;
  final String? percentage;

  InviteRebate({
    this.minerId,
    this.minerName,
    this.minerQuantity,
    this.minerRebate,
    this.salesAmount,
    this.level,
    this.percentage,
  });

  factory InviteRebate.fromJson(Map<String, dynamic> json) {
    return InviteRebate(
      minerId: json['miner_id']?.toString(),
      minerName: json['miner_name']?.toString(),
      minerQuantity: json['miner_quantity']?.toString(),
      minerRebate: json['miner_rebate']?.toString(),
      salesAmount: json['sales_amount']?.toString(),
      level: json['level']?.toString(),
      percentage: json['percentage']?.toString(),
    );
  }
}

class InviteReward {
  final String? minerId;
  final String? minerName;
  final String? minerQuantity;
  final String? minerReward;

  InviteReward({
    this.minerId,
    this.minerName,
    this.minerQuantity,
    this.minerReward,
  });

  factory InviteReward.fromJson(Map<String, dynamic> json) {
    return InviteReward(
      minerId: json['miner_id']?.toString(),
      minerName: json['miner_name']?.toString(),
      minerQuantity: json['miner_quantity']?.toString(),
      minerReward: json['miner_reward']?.toString(),
    );
  }
}
