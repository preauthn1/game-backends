import './pagination.dart';

class ReferralRewardRecord {
  final List<ReferralRewardRecordItem>? list;
  final Pagination? pagination;

  ReferralRewardRecord({
    this.list,
    this.pagination,
  });

  factory ReferralRewardRecord.fromJson(Map<String, dynamic> json) {
    return ReferralRewardRecord(
      list: json['list'] != null
          ? (json['list'] as List)
              .map((i) => ReferralRewardRecordItem.fromJson(i))
              .toList()
          : null,
      pagination: json['pagination'] != null
          ? Pagination.fromJson(json['pagination'])
          : null,
    );
  }
}

class ReferralRewardRecordItem {
  final String? summaryTime;
  final String? amount;
  final String? members;
  final String? minerId;
  final String? minerName;

  ReferralRewardRecordItem({
    this.summaryTime,
    this.amount,
    this.members,
    this.minerId,
    this.minerName,
  });

  factory ReferralRewardRecordItem.fromJson(Map<String, dynamic> json) {
    return ReferralRewardRecordItem(
      summaryTime: json['summary_time']?.toString(),
      amount: json['amount']?.toString(),
      members: json['members']?.toString(),
      minerId: json['miner_id']?.toString(),
      minerName: json['miner_name']?.toString(),
    );
  }
}
