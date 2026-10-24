class AgentCenter {
  final String? teamCount;
  final String? salesRebate;
  final String? referralReward;
  final List<AgentCenterTeam>? team;
  final List<AgentCenterSales>? sales;

  AgentCenter({
    required this.teamCount,
    required this.salesRebate,
    required this.referralReward,
    required this.team,
    required this.sales,
  });

  factory AgentCenter.fromJson(Map<String, dynamic> json) {
    return AgentCenter(
      teamCount: json['team_count']?.toString(),
      salesRebate: json['sales_rebate']?.toString(),
      referralReward: json['referral_reward']?.toString(),
      team: json['team'] != null
          ? List<AgentCenterTeam>.from(
              json['team'].map(
                (x) => AgentCenterTeam.fromJson(x),
              ),
            )
          : null,
      sales: json['sales'] != null
          ? List<AgentCenterSales>.from(
              json['sales'].map(
                (x) => AgentCenterSales.fromJson(x),
              ),
            )
          : null,
    );
  }
}

class AgentCenterTeam {
  final String? account;
  final String? level;
  final String? createdAt;

  AgentCenterTeam({
    required this.account,
    required this.level,
    required this.createdAt,
  });

  factory AgentCenterTeam.fromJson(Map<String, dynamic> json) {
    return AgentCenterTeam(
      account: json['account']?.toString(),
      level: json['level']?.toString(),
      createdAt: json['created_at']?.toString(),
    );
  }
}

class AgentCenterSales {
  final String? username;
  final String? minerId;
  final String? minerName;
  final String? qty;
  final String? createdAt;

  AgentCenterSales({
    required this.username,
    required this.minerId,
    required this.minerName,
    required this.qty,
    required this.createdAt,
  });

  factory AgentCenterSales.fromJson(Map<String, dynamic> json) {
    return AgentCenterSales(
      username: json['username']?.toString(),
      minerId: json['miner_id']?.toString(),
      minerName: json['miner_name']?.toString(),
      qty: json['qty']?.toString(),
      createdAt: json['created_at']?.toString(),
    );
  }
}
