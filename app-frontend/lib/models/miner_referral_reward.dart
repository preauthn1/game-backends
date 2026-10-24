import 'package:collection/collection.dart';
//  [{miner_id: 1, miner_name: S19k, reward_list: [{quantity: 3, amount: 20.0000000000}, {quantity: 5, amount: 502.0000000000}, {quantity: 10, amount: 1001.0000000000}]}, {miner_id: 7, miner_name: PS5 Pro, reward_list: [{quantity: 3, amount: 20.0000000000}, {quantity: 5, amount: 50.0000000000}, {quantity: 10, amount: 100.0000000000}]}, {miner_id: 48, miner_name: dsgdfsgsdg, reward_list: [{quantity: 3, amount: 22.0000000000}, {quantity: 5, amount: 66.0000000000}, {quantity: 10, amount: 99.0000000000}]}, {miner_id: 4, miner_name: L7, reward_list: [{quantity: 3, amount: 20.0000000000}, {quantity: 5, amount: 50.0000000000}, {quantity: 10, amount: 100.0000000000}]}, {miner_id: 3, miner_name: MarsXXX, reward_list: [{quantity: 3, amount: 20.0000000000}, {quantity: 5, amount: 50.0000000000}, {quantity: 10, amount: 90.0000000000}]}]

class ReferralRewardDetail {
  final String? quantity;
  final String? amount;

  ReferralRewardDetail({
    this.quantity,
    this.amount,
  });

  factory ReferralRewardDetail.fromJson(Map<String, dynamic> json) {
    return ReferralRewardDetail(
      quantity: json['quantity']?.toString(),
      amount: json['amount']?.toString(),
    );
  }
}

class MinerReferralReward {
  final String? minerId;
  final String? minerName;
  final List<ReferralRewardDetail>? rewardList;

  MinerReferralReward({
    this.minerId,
    this.minerName,
    this.rewardList,
  });

  String getReward(String quantity) {
    final reward = rewardList?.firstWhereOrNull(
      (element) => element.quantity == quantity,
    );
    return reward?.amount ?? '-';
  }

  factory MinerReferralReward.fromJson(Map<String, dynamic> json) {
    return MinerReferralReward(
      minerId: json['miner_id']?.toString(),
      minerName: json['miner_name']?.toString(),
      rewardList: json['reward_list'] != null
          ? (json['reward_list'] as List)
              .map((i) => ReferralRewardDetail.fromJson(i))
              .toList()
          : null,
    );
  }
}
