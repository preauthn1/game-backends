import './pagination.dart';

class SalesRebateRecord {
  final List<SalesRebateRecordItem>? list;
  final Pagination? pagination;

  SalesRebateRecord({
    this.list,
    this.pagination,
  });

  factory SalesRebateRecord.fromJson(Map<String, dynamic> json) {
    return SalesRebateRecord(
      list: json['list'] != null
          ? (json['list'] as List)
              .map((i) => SalesRebateRecordItem.fromJson(i))
              .toList()
          : null,
      pagination: json['pagination'] != null
          ? Pagination.fromJson(json['pagination'])
          : null,
    );
  }
}

class SalesRebateRecordItem {
  final String? summaryTime;
  final String? amount;
  final String? minerId;
  final String? minerName;
  final String? account;
  final String? info;

  SalesRebateRecordItem({
    this.summaryTime,
    this.amount,
    this.minerId,
    this.minerName,
    this.account,
    example.invalid,
  });

  factory SalesRebateRecordItem.fromJson(Map<String, dynamic> json) {
    return SalesRebateRecordItem(
      summaryTime: json['summary_time']?.toString(),
      amount: json['amount']?.toString(),
      minerId: json['miner_id']?.toString(),
      minerName: json['miner_name']?.toString(),
      account: json['account']?.toString(),
      info: json['info']?.toString(),
    );
  }
}
